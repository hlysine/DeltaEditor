using DeltaQuestionEditor_WPF.Consts;
using DeltaQuestionEditor_WPF.Models;
using ExcelDataReader;
using MaterialDesignColors.Recommended;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace DeltaQuestionEditor_WPF.DataSources
{
    using static DeltaQuestionEditor_WPF.Helpers.Helper;
    public class ExcelFileDataSource : IDataSource
    {
        public string FilePath { get; set; }
        public string LastFailMessage { get; set; }

        private List<string> problems = new List<string>();
        private int questions = 0;
        private int media = 0;
        private DataSet dataSet = null;
        private List<(DataTable, int, int)> tableScores = new List<(DataTable, int, int)>();
        List<string[]> rows;
        private DataTable dataTable = null;
        private bool hasHeader;
        private List<(Field, int)> columnMapping = new List<(Field, int)>();

        private void failed(string reason) => LastFailMessage = $"{Path.GetFileName(FilePath)} import failed: {reason}";

        private int headerScore(object[] itemsArray)
        {
            string[] headerItems = itemsArray.Select(x => x.ToString()).ToArray();
            int fitScore = 0;
            if (headerItems[0] is string text && text != null && text.Trim().ToLower().Contains(new[] { "question", "prompt" }))
            {
                fitScore++;
            }
            for (int i = 1; i < 5; i++)
                if (headerItems.Length > i
                && headerItems[i] != null
                && headerItems[i].Trim().ToLower().Contains(new[] { "answer", "choice" }))
                {
                    fitScore++;
                }
            if (headerItems.Length > 5)
            {
                if (headerItems.Skip(5).Where(x => x != null).Any(x => x.Trim().ToLower().Contains("skill")))
                    fitScore++;
                if (headerItems.Skip(5).Where(x => x != null).Any(x => x.Trim().ToLower().Contains(new[] { "diff", "level" })))
                    fitScore++;
            }
            return fitScore;
        }

        public async Task<bool> ReadFile(string path)
        {
            FilePath = path;
            if (!IsFileLocked(path))
            {
                await Task.Run(() =>
                {
                    using (var stream = File.Open(FilePath, FileMode.Open, FileAccess.Read))
                    {
                        using (var reader = ExcelReaderFactory.CreateReader(stream))
                        {
                            dataSet = reader.AsDataSet();
                        }
                    }
                });
                return true;
            }
            return false;
        }

        public async Task<bool> AnalyzeFile()
        {
            if (dataSet == null) throw new InvalidOperationException("Read the file before analyzing");
            return await Task.Run(() =>
            {
                if (dataSet.Tables.Count < 1)
                {
                    failed(ExcelImportFailures.NO_WORKSHEETS_FOUND);
                    return false;
                }
                DataTable table = null;
                var candidates = dataSet.Tables.Cast<DataTable>().Where(x => x.Columns.Count >= 5 && x.Rows.Count >= 1);
                if (candidates.Count() == 0)
                {
                    failed(ExcelImportFailures.TOO_FEW_COLUMNS);
                    return false;
                }
                tableScores = candidates.Select(x =>
                {

                    int fitScore = 0;
                    fitScore += headerScore(x.Rows[0].ItemArray) * x.Rows.Count;

                    foreach (DataRow row in x.Rows.Cast<DataRow>().Skip(1))
                    {
                        string[] contentItems = row.ItemArray.Select(x => x.ToString()).ToArray();
                        for (int i = 0; i < 5; i++)
                            if (contentItems.Length > i
                            && !contentItems[i].IsNullOrWhiteSpace())
                            {
                                fitScore++;
                            }
                        if (contentItems.Length > 5)
                        {
                            if (contentItems.Skip(5).Where(x => x != null).Any(x => Regex.IsMatch(x, @"^[\d., ]+$")))
                                fitScore++;
                            if (contentItems.Skip(5).Where(x => x != null).Any(x => Regex.IsMatch(x, @"^[123]$")))
                                fitScore++;
                        }
                    }
                    return (x, fitScore, 7 * (2 * x.Rows.Count - 1));
                }).ToList();
                var tableScoreSet = tableScores.MaxBy(x => x.Item2 / (double)x.Item3).FirstOrDefault();
                if (tableScoreSet != default) table = tableScoreSet.Item1;
                if (table == null)
                {
                    failed(ExcelImportFailures.NO_VALID_WORKSHEET);
                    return false;
                }

                dataTable = table;

                if (headerScore(table.Rows[0].ItemArray) >= 5)
                {
                    hasHeader = true;
                    columnMapping.Add((Field.Question, 0));
                    columnMapping.Add((Field.CorrectAnswer, 1));
                    columnMapping.Add((Field.WrongAnswer1, 2));
                    columnMapping.Add((Field.WrongAnswer2, 3));
                    columnMapping.Add((Field.WrongAnswer3, 4));
                    string[] headers = table.Rows[0].ItemArray.Select(x => x.ToString()).ToArray();
                    var skillHeader = headers.Skip(5).Where(x => x != null).FirstOrDefault(x => x.Trim().ToLower().Contains("skill"));
                    if (skillHeader != null)
                        columnMapping.Add((Field.Skills, Array.IndexOf(headers, skillHeader)));
                    var diffHeader = headers.Skip(5).Where(x => x != null).FirstOrDefault(x => x.Trim().ToLower().Contains(new[] { "diff", "level" }));
                    if (diffHeader != null)
                        columnMapping.Add((Field.Difficulty, Array.IndexOf(headers, diffHeader)));
                    var mediaHeader = headers.Skip(5).Where(x => x != null).FirstOrDefault(x => x.Trim().ToLower().Contains(new[] { "graph", "link", "file", "media", "image", "picture", "diagram", "photo" }));
                    if (mediaHeader != null)
                        columnMapping.Add((Field.Media, Array.IndexOf(headers, mediaHeader)));
                }
                else
                {
                    hasHeader = false;
                    problems.Add(ExcelImportProblems.NO_HEADERS);
                    columnMapping.Add((Field.Question, 0));
                    columnMapping.Add((Field.CorrectAnswer, 1));
                    columnMapping.Add((Field.WrongAnswer1, 2));
                    columnMapping.Add((Field.WrongAnswer2, 3));
                    columnMapping.Add((Field.WrongAnswer3, 4));
                    int[] diffColumnScore = new int[table.Rows.Count - 5];
                    foreach (DataRow row in table.Rows)
                    {
                        string[] contents = row.ItemArray.Select(x => x.ToString()).ToArray();
                        for (int i = 5; i < contents.Length; i++)
                        {
                            if (Regex.IsMatch(contents[i], @"^[123]$"))
                                diffColumnScore[i - 5]++;
                        }
                    }
                    int diffScoreMax = diffColumnScore.Max();
                    int diffScoreIndex = -1;
                    if (diffScoreMax >= table.Rows.Count / 2)
                    {
                        diffScoreIndex = Array.IndexOf(diffColumnScore, diffScoreMax) + 5;
                        columnMapping.Add((Field.Difficulty, diffScoreIndex));
                    }
                    List<(int, int)> skillColumnScore = new List<(int, int)>();
                    for (int i = 5; i < table.Columns.Count; i++)
                    {
                        skillColumnScore.Add((i, 0));
                    }
                    foreach (DataRow row in table.Rows)
                    {
                        string[] contents = row.ItemArray.Select(x => x.ToString()).ToArray();
                        for (int i = 5; i < contents.Length; i++)
                        {
                            if (Regex.IsMatch(contents[i], @"^[\d., ]+$"))
                            {
                                int tmp = skillColumnScore.First(x => x.Item1 == i).Item2;
                                skillColumnScore.RemoveAll(x => x.Item1 == i);
                                skillColumnScore.Add((i, tmp + 1));
                            }
                        }
                    }
                    int skillScoreMax = skillColumnScore.Where(x => x.Item1 != diffScoreIndex).Max(x => x.Item2);
                    if (skillScoreMax >= table.Rows.Count / 2)
                    {
                        columnMapping.Add((Field.Skills, skillColumnScore.First(x => x.Item2 == skillScoreMax).Item1));
                    }
                }
                if (!columnMapping.Any(x => x.Item1 == Field.Skills))
                {
                    problems.Add(ExcelImportProblems.MISSING_COLUMN_REQUIRED_SKILLS);
                }
                if (!columnMapping.Any(x => x.Item1 == Field.Difficulty))
                {
                    problems.Add(ExcelImportProblems.MISSING_COLUMN_DIFFICULTY);
                }
                if (columnMapping.Any(x => x.Item1 == Field.Media))
                {
                    problems.Add(ExcelImportProblems.EXTRA_COLUMN_MEDIA);
                }
                List<int> nonemptyColumns = new List<int>();
                rows = dataTable.Rows.Cast<DataRow>().Select(x => x.ItemArray.Select(y => y.ToString()).ToArray()).ToList();
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    if (rows.Any(x => !x[i].IsNullOrWhiteSpace()))
                        nonemptyColumns.Add(i);
                }
                nonemptyColumns.Where(x => !columnMapping.Any(y => y.Item2 == x)).ForEach(x => problems.Add(string.Format(ExcelImportProblems.UNUSED_COLUMN, x + 1, hasHeader ? $"({rows[0][x]})" : "")));
                return true;
            });
        }

        public async Task<bool> ImportQuestions(LocalFileDataSource dataSource)
        {
            return await Task.Run(() =>
             {
                 if (hasHeader) rows.RemoveAt(0);
                 foreach (string[] row in rows)
                 {
                     Question question = new Question();
                     question.Id = NewGuid();
                     for (int i = 0; i < 4; i++)
                         question.Answers.Add(null);
                     question.Difficulty = 2;
                     (Field, int) mapping;
                     if ((mapping = columnMapping.FirstOrDefault(x => x.Item1 == Field.Question)) != default)
                     {
                         question.Text = row[mapping.Item2];
                         if (row[mapping.Item2].IsNullOrWhiteSpace())
                         {
                             problems.Add(string.Format(ExcelImportProblems.EMPTY_QUESTION_TEXT, dataSource.QuestionSet.Questions.Count + 1));
                         }
                     }
                     if ((mapping = columnMapping.FirstOrDefault(x => x.Item1 == Field.CorrectAnswer)) != default)
                     {
                         question.Answers[0] = row[mapping.Item2];
                         if (row[mapping.Item2].IsNullOrWhiteSpace())
                         {
                             problems.Add(string.Format(ExcelImportProblems.EMPTY_CORRECT_ANSWER, dataSource.QuestionSet.Questions.Count + 1));
                         }
                     }
                     if ((mapping = columnMapping.FirstOrDefault(x => x.Item1 == Field.WrongAnswer1)) != default)
                     {
                         question.Answers[1] = row[mapping.Item2];
                         if (row[mapping.Item2].IsNullOrWhiteSpace())
                         {
                             problems.Add(string.Format(ExcelImportProblems.EMPTY_WRONG_ANSWER, 1, dataSource.QuestionSet.Questions.Count + 1));
                         }
                     }
                     if ((mapping = columnMapping.FirstOrDefault(x => x.Item1 == Field.WrongAnswer2)) != default)
                     {
                         question.Answers[2] = row[mapping.Item2];
                         if (row[mapping.Item2].IsNullOrWhiteSpace())
                         {
                             problems.Add(string.Format(ExcelImportProblems.EMPTY_WRONG_ANSWER, 2, dataSource.QuestionSet.Questions.Count + 1));
                         }
                     }
                     if ((mapping = columnMapping.FirstOrDefault(x => x.Item1 == Field.WrongAnswer3)) != default)
                     {
                         question.Answers[3] = row[mapping.Item2];
                         if (row[mapping.Item2].IsNullOrWhiteSpace())
                         {
                             problems.Add(string.Format(ExcelImportProblems.EMPTY_WRONG_ANSWER, 3, dataSource.QuestionSet.Questions.Count + 1));
                         }
                     }
                     if ((mapping = columnMapping.FirstOrDefault(x => x.Item1 == Field.Difficulty)) != default)
                     {
                         if (!row[mapping.Item2].IsNullOrWhiteSpace())
                         {
                             if (ushort.TryParse(row[mapping.Item2], out ushort result))
                             {
                                 question.Difficulty = result;
                             }
                             else
                             {
                                 problems.Add(string.Format(ExcelImportProblems.INVALID_DIFFICULTY, dataSource.QuestionSet.Questions.Count + 1));
                             }
                         }
                         else
                         {
                             problems.Add(string.Format(ExcelImportProblems.EMPTY_DIFFICULTY, dataSource.QuestionSet.Questions.Count + 1));
                         }
                     }
                     if ((mapping = columnMapping.FirstOrDefault(x => x.Item1 == Field.Skills)) != default)
                     {
                         if (!row[mapping.Item2].IsNullOrWhiteSpace())
                         {
                             bool fix = false;
                             string tmp = row[mapping.Item2];
                             string[] arr;
                             if (tmp.Trim().TrimEnd('\r', '\n').Contains(new[] { "|", ",", ";", "&", "+", "\n" }))
                                 arr = tmp.Split('|', ',', ';', '&', '+', '\n');
                             else
                             {
                                 arr = tmp.Split('|', ',', ';', '&', '+', '\n', ' ');
                                 if (arr.Any(x => x.StartsWith(".") || x.EndsWith(".") || x == "."))
                                 {
                                     arr = new[] { tmp };
                                 }
                             }
                             arr = arr.Select(x => Regex.Replace(x, @"\s", "")).ToArray();
                             arr.ForEach(x =>
                             {
                                 //if ((x.Count(y => y == '.') < 2 && Regex.IsMatch(x, @"^\d+(?:\.\d+)*$")) || (x.Count(y => y == '.') == 2 && !x.StartsWith($"{dataSource.QuestionSet.Form}.{dataSource.QuestionSet.Chapter}")))
                                 //{
                                 //    question.Skills.Add($"{dataSource.QuestionSet.Form}.{dataSource.QuestionSet.Chapter}.{x}");
                                 //    fix = true;
                                 //}
                                 if (Regex.IsMatch(x, @"^\d+(?:\.\d+)*$") && (!x.StartsWith($"{dataSource.QuestionSet.Form}.{dataSource.QuestionSet.Chapter}") || x.Count(y => y == '.') < 3))
                                 {
                                     question.Skills.Add($"{dataSource.QuestionSet.Form}.{dataSource.QuestionSet.Chapter}.{x}");
                                     fix = true;
                                 }
                                 else
                                 {
                                     if (!Regex.IsMatch(x, @"^\d+(?:\.\d+)*$"))
                                     {
                                         problems.Add(string.Format(ExcelImportProblems.INVALID_SKILLS, dataSource.QuestionSet.Questions.Count + 1));
                                     }
                                     question.Skills.Add(x);
                                 }
                             });
                             if (fix)
                                 problems.Add(string.Format(ExcelImportProblems.SKILLS_TOO_SHORT, dataSource.QuestionSet.Questions.Count + 1, dataSource.QuestionSet.Form, dataSource.QuestionSet.Chapter));
                         }
                         else
                         {
                             problems.Add(string.Format(ExcelImportProblems.EMPTY_SKILLS, dataSource.QuestionSet.Questions.Count + 1));
                         }
                     }
                     if ((mapping = columnMapping.FirstOrDefault(x => x.Item1 == Field.Media)) != default)
                     {
                         if (!row[mapping.Item2].IsNullOrWhiteSpace())
                         {
                             string[] paths = Regex.Split(row[mapping.Item2], @"\r\n|\r|\n");
                             if (paths.Length == 1)
                             {
                                 paths = paths[0].Split('|', ',', ';', '&', '+');
                             }
                             foreach (string path in paths)
                             {
                                 Media media = locateAndImportMedia(path, dataSource).Result;
                                 question.Text += $"\r\n\r\n![media]({media.FileName.Replace('\\', '/')})";
                             }
                         }
                     }
                     Application.Current.Dispatcher.Invoke(() =>
                     {
                         dataSource.QuestionSet.Questions.Add(question);
                     });
                     questions++;
                 }
                 return true;
             });
        }

        private async Task<Media> locateAndImportMedia(string mediaName, LocalFileDataSource dataSource)
        {
            string path = Path.Combine(Path.GetDirectoryName(FilePath), mediaName);
            if (!File.Exists(path))
            {
                string newPath = Path.Combine(Path.GetDirectoryName(FilePath), Path.GetFileName(path));
                if (File.Exists(newPath))
                {
                    path = newPath;
                }
                else
                {
                    string filename = Path.GetFileName(path);
                    if (filename.IndexOf('.') == -1) filename += ".*";
                    var allFiles = Directory.GetFiles(Path.GetDirectoryName(FilePath), filename, SearchOption.AllDirectories);
                    if (allFiles.Length > 0)
                    {
                        path = allFiles.OrderBy(x => x.Length).First();
                    }
                    else
                    {
                        problems.Add(string.Format(ExcelImportProblems.MEDIA_NOT_FOUND, mediaName));
                        return null;
                    }
                }
            }
            if (new FileInfo(path).Length > 500 * 1024)
            {
                problems.Add(string.Format(ExcelImportProblems.MEDIA_TOO_LARGE, mediaName));
            }
            string id = await dataSource.AddMedia(path);
            Media media = dataSource.QuestionSet.Media.First(x => x.Id == id);
            this.media++;
            return media;
        }

        private async Task<string> importMedia(string text, LocalFileDataSource dataSource)
        {
            MatchCollection matches = Regex.Matches(text, @"!\[(.*?)(?<!(?<!\\)\\(?:\\\\)*)\]\((.*?)(?<!(?<!\\)\\(?:\\\\)*)\)");
            List<(string, string)> replacements = new List<(string, string)>();
            foreach (Match match in matches)
            {
                if (dataSource.QuestionSet.Media.Any(x => x.FileName.Replace('\\', '/') == match.Groups[2].Value.Replace('\\', '/'))) continue;
                Media media = await locateAndImportMedia(match.Groups[2].Value, dataSource);
                if (media != null)
                {
                    replacements.Add((match.Value, $"![media]({media.FileName.Replace('\\', '/')})"));
                }
            }
            foreach ((string, string) entry in replacements)
            {
                text = text.Replace(entry.Item1, entry.Item2);
            }
            return text;
        }

        public async Task<bool> ImportMedia(LocalFileDataSource dataSource)
        {
            foreach (Question question in dataSource.QuestionSet.Questions)
            {
                question.Text = await importMedia(question.Text, dataSource);
                question.Answers[0] = await importMedia(question.Answers[0], dataSource);
                question.Answers[1] = await importMedia(question.Answers[1], dataSource);
                question.Answers[2] = await importMedia(question.Answers[2], dataSource);
                question.Answers[3] = await importMedia(question.Answers[3], dataSource);
            }

            return true;
        }

        public string GetImportReport()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"This is a report of the import process of \"{Path.GetFileName(FilePath)}\" at {DateTime.Now:F}");
            sb.AppendLine();
            sb.AppendLine($"Please read this report carefully to ensure that the program is reading the Excel file correctly.");
            sb.AppendLine($"If there are problems listed in the report, please resolve them manually.");
            sb.AppendLine();
            sb.AppendLine($"If the import result is very different from your expected result, you may create an issue through this link to notify the developers:");
            sb.AppendLine("    https://github.com/Profound-Education-Centre/DeltaQuestionEditor-WPF/issues/new/choose");
            sb.AppendLine($"Make sure to include a link to the Excel file that you are trying to import when creating the issue.");
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine($"====== IMPORT SUMMARY ======");
            sb.AppendLine($"File: {FilePath}");
            sb.AppendLine($"Number of worksheets in the file: {dataSet.Tables.Count}");
            sb.AppendLine();
            foreach (DataTable table in dataSet.Tables)
            {
                sb.AppendLine($" - Worksheet \"{table.TableName}\"");
                sb.AppendLine($"   Size: {table.Columns.Count} columns by {table.Rows.Count} rows");
                var conf = tableScores.FirstOrDefault(x => x.Item1 == table);
                if (conf != default)
                    sb.AppendLine($"   Confidence: {conf.Item2 / (double)conf.Item3 * 100:F2}% ({conf.Item2}/{conf.Item3})");
                else
                    sb.AppendLine($"   Confidence: 0%");
                sb.AppendLine();
            }
            sb.AppendLine();
            sb.AppendLine($"Worksheet \"{dataTable.TableName}\" is selected for import");
            sb.AppendLine($"This table has {(hasHeader ? "" : "no ")}headers");
            sb.AppendLine();
            foreach ((Field, int) mapping in columnMapping)
            {
                sb.AppendLine($" - {mapping.Item1} is at column {mapping.Item2 + 1}{(hasHeader ? $": \"{dataTable.Rows[0].ItemArray[mapping.Item2]}\"" : "")}");
            }
            sb.AppendLine();
            sb.AppendLine($"Number of questions: {questions}");
            sb.AppendLine($"Number of media files: {media}");
            sb.AppendLine($"    (actual number of imported media files may be lower if there are duplicate files)");
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine($"====== PROBLEMS ======");
            sb.AppendLine($"Number of problems: {problems.Count}");
            sb.AppendLine();
            foreach (string problem in problems)
            {
                sb.AppendLine(problem);
                sb.AppendLine();
            }
            sb.AppendLine();
            sb.AppendLine($"====== END OF REPORT ======");

            return sb.ToString();
        }

        public enum Field
        {
            None,
            Question,
            CorrectAnswer,
            WrongAnswer1,
            WrongAnswer2,
            WrongAnswer3,
            Skills,
            Difficulty,
            Media
        }
    }
}
