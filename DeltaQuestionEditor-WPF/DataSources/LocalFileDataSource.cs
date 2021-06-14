﻿using DeltaQuestionEditor_WPF.Helpers;
using DeltaQuestionEditor_WPF.Models;
using MoreLinq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace DeltaQuestionEditor_WPF.DataSources
{
    using static Helper;
    public class LocalFileDataSource : NotifyPropertyChanged, IDataSource, IDisposable
    {
        private QuestionSet questionSet = null;
        public QuestionSet QuestionSet
        {
            get => questionSet;
            private set => SetAndNotify(ref questionSet, value);
        }
        private string filePath = null;
        public string FilePath
        {
            get => filePath;
            private set => SetAndNotify(ref filePath, value, new[] { SafeFileName });
        }
        public string SafeFileName => FilePath.IsNullOrEmpty() ? string.Empty : Path.GetFileName(FilePath);
        private DateTime lastSaved;
        public DateTime LastSaved
        {
            get => lastSaved;
            set => SetAndNotify(ref lastSaved, value);
        }
        private string tempPath;
        public string TempPath
        {
            get => tempPath;
            set => SetAndNotify(ref tempPath, value);
        }
        private FileStream fileStream;

        public LocalFileDataSource()
        {
            TempPath = Path.Combine(Path.GetTempPath(), "DeltaQuestionEditor", Guid.NewGuid().ToString());
            EnsurePathExist(TempPath);
            EnsurePathExist(Path.Combine(TempPath, "Media"));
        }

        public void Dispose()
        {
            Directory.Delete(TempPath, true);
            if (fileStream != null)
                fileStream.Dispose();
        }

        private static readonly Dictionary<int, Dictionary<int, string>> topicNames = new Dictionary<int, Dictionary<int, string>>
        {
            [1] = new Dictionary<int, string>
            {
                [0] = "Basic Mathematics",
                [1] = "Directed Numbers and the Number Line",
                [2] = "Introduction to Algebra",
                [3] = "Algebraic Equations in One Unknown",
                [4] = "Percentages (I)",
                [5] = "Estimation in Numbers and Measurement",
                [6] = "Introduction to Geometry",
                [7] = "Symmetry and Transformation",
                [8] = "Areas and Volumes (I)",
                [9] = "Congruence and Similarity",
                [10] = "Introduction to Coordinates",
                [11] = "Angles related to Lines",
                [12] = "Manipulation of Simple Polynomials",
                [13] = "Introduction to Various Stages of Statistics",
                [14] = "Simple Statistical Diagrams and Graphs (I)",
            },
            [2] = new Dictionary<int, string>
            {
                [1] = "Rate and Ratio",
                [2] = "Identities and Factorization",
                [3] = "Algebraic Fractions and Formulas",
                [4] = "More about Factorization of Polynomials",
                [5] = "Approximation and Errors",
                [6] = "Angles related to Rectilinear Figures",
                [7] = "Simple Statistical Diagrams and Graphs (II)",
                [8] = "Linear Equations in Two Unknowns",
                [9] = "Laws of Integral Indices",
                [10] = "Introduction to Deductive Geometry",
                [11] = "Rational and Irrational Numbers",
                [12] = "Pythagoras' Theorem",
                [13] = "Areas and Volumes (II)",
                [14] = "Trigonometric Ratios",
            },
            [3] = new Dictionary<int, string>
            {
                [1] = "Linear Inequalities in One Unknown",
                [2] = "Percentages (II)",
                [3] = "Special Lines and Centres in a Triangle",
                [4] = "Quadrilaterals",
                [5] = "More about 3D Figures",
                [6] = "Measures of Central Tendency",
                [7] = "Areas and Volumes (III)",
                [8] = "Coordinate Geometry of Straight Lines",
                [9] = "Trigonometric Relations",
                [10] = "Applications of Trigonometry",
                [11] = "Introduction to Probability",
            },
            [4] = new Dictionary<int, string>
            {
                [1] = "Number System",
                [2] = "Equation of Straight Lines",
                [3] = "Quadratic Equations in One unknown ",
                [4] = "Basic Knowledge of Functions",
                [5] = "Quadratic Functions",
                [6] = "More about Polynomials",
                [7] = "Exponential Functions",
                [8] = "Logarithmic Functions",
                [9] = "Rational Functions",
                [10] = "Basic Properties of Circles",
                [11] = "More about Basic Properties of Circles",
                [12] = "Basic Trigonometry",
            },
            [5] = new Dictionary<int, string>
            {
                [1] = "More about Equations",
                [2] = "Inequalities in One Unknown",
                [3] = "More about Graphs of Functions",
                [4] = "Permutation and Combination",
                [5] = "More about Probability",
                [6] = "Variations  ",
                [7] = "Equations of Circles",
                [8] = "Locus",
                [9] = "Solving Triangles",
                [10] = "Applications in Trigonometry",
                [11] = "Measures of Dispersion",
                [12] = "More about Dispersion",
            },
            [6] = new Dictionary<int, string>
            {
                [1] = "Arithmetic Sequences",
                [2] = "Geometric Sequences",
                [3] = "Linear Inequalities in Two Unknowns and Linear Programming",
                [4] = "Uses and Abuses of Statistics",
            }
        };

        /// <summary>
        /// Get the topic name by form and chapter number
        /// </summary>
        public string GetTopicName(int form, int chapter)
        {
            if (topicNames.TryGetValue(form, out var dict))
            {
                if (dict.TryGetValue(chapter, out string name))
                {
                    return name;
                }
            }
            return null;
        }

        /// <summary>
        /// Convert a path that is relative to the temp folder to an absolute path.
        /// </summary>
        public string TempToAbsolutePath(string relativePath)
        {
            return Path.Combine(TempPath, relativePath);
        }

        /// <summary>
        /// Create a new, empty question set.
        /// </summary>
        public void CreateQuestionSet()
        {
            QuestionSet = new QuestionSet();
            QuestionSet.DataSource = this;
            QuestionSet.Media.ForEach(x => x.DataSource = this);
            FilePath = null;
        }

        public enum LoadQuestionStatus
        {
            Success,
            FolderStructureAutoFixed,
            FileLocked,
            QuestionSetJsonNotFound
        }

        /// <summary>
        /// Load a question set (.qdb) file.
        /// </summary>
        /// <param name="path">Path to qdb file.</param>
        public async Task<LoadQuestionStatus> LoadQuestionSet(string path)
        {
            if (IsFileLocked(path))
                return LoadQuestionStatus.FileLocked;
            if (fileStream != null && FilePath != path)
            {
                fileStream.Dispose();
                fileStream = null;
            }
            FilePath = path;
            return await Task.Run(() =>
            {
                bool autoFixed = false;
                ClearDirectory(TempPath);
                if (fileStream == null)
                    fileStream = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Read, true))
                {
                    archive.ExtractToDirectory(TempPath);
                }
                if (!File.Exists(Path.Combine(TempPath, "questionset.json")))
                {
                    var allFiles = Directory.GetFiles(TempPath, "questionset.json", SearchOption.AllDirectories);
                    string jsonPath = null;
                    if (allFiles.Length > 0)
                    {
                        jsonPath = allFiles.OrderBy(x => x.Length).First();
                    }
                    if (jsonPath != null)
                    {
                        // Attempt to fix folder structure
                        string path = GetRelativePath(TempPath + "\\", jsonPath);
                        if (path.StartsWith("."))
                        {
                            path = Regex.Match(path, @"\.\\(.*?)(\\|$)").Groups[1].Value;
                        }
                        else if (path.StartsWith("\\"))
                        {
                            path = Regex.Match(path, @"\\(.*?)(\\|$)").Groups[1].Value;
                        }
                        else
                        {
                            path = Regex.Match(path, @"(.*?)(\\|$)").Groups[1].Value;
                        }
                        string guid = NewGuid();
                        Directory.Move(Path.Combine(TempPath, path), Path.Combine(TempPath, guid));
                        jsonPath = jsonPath.Replace(Path.Combine(TempPath, path), Path.Combine(TempPath, guid));

                        bool mediaExists = Directory.Exists(Path.Combine(Path.GetDirectoryName(jsonPath), "Media"));

                        DirectoryInfo di = new DirectoryInfo(TempPath);

                        foreach (FileInfo file in di.GetFiles())
                        {
                            file.Delete();
                        }
                        foreach (DirectoryInfo dir in di.GetDirectories())
                        {
                            if (!(dir.Name == guid || (!mediaExists && dir.Name == "Media")))
                                dir.Delete(true);
                        }

                        di = new DirectoryInfo(Path.GetDirectoryName(jsonPath));

                        foreach (FileInfo file in di.GetFiles())
                        {
                            file.MoveTo(Path.Combine(TempPath, file.Name));
                        }
                        foreach (DirectoryInfo dir in di.GetDirectories())
                        {
                            dir.MoveTo(Path.Combine(TempPath, dir.Name));
                        }

                        Directory.Delete(Path.Combine(TempPath, guid), true);

                        autoFixed = true;
                    }
                    else
                    {
                        return LoadQuestionStatus.QuestionSetJsonNotFound;
                    }
                }
                QuestionSet = JsonConvert.DeserializeObject<QuestionSet>(File.ReadAllText(Path.Combine(TempPath, "questionset.json")));
                QuestionSet.DataSource = this;
                QuestionSet.Media.ForEach(x => x.DataSource = this);
                LastSaved = DateTime.Now;
                if (!autoFixed)
                    return LoadQuestionStatus.Success;
                else
                    return LoadQuestionStatus.FolderStructureAutoFixed;
            });
        }

        /// <summary>
        /// Save the question set to a qdb file. Will clean the media folder in the process.
        /// </summary>
        /// <param name="path">Save location of the qdb file. Can be omitted if the question set is loaded through a path.</param>
        /// <exception cref="ArgumentNullException">Thrown if path is null and the question set is never saved.</exception>
        /// <exception cref="InvalidOperationException">Thrown if <code>QuestionSet</code> is null.</exception>
        public async Task SaveQuestionSet(string path = null)
        {
            if (QuestionSet == null)
                throw new InvalidOperationException("QuestionSet is null");
            if (fileStream != null && FilePath != path)
            {
                fileStream.Dispose();
                fileStream = null;
            }
            if (path != null)
                FilePath = path;
            else if (FilePath == null)
                throw new ArgumentNullException("path is null and there is no saved file path");
            if (fileStream == null)
                fileStream = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            await Task.Run(() =>
            {
                File.WriteAllText(Path.Combine(TempPath, "questionset.json"), JsonConvert.SerializeObject(QuestionSet));
                EnsurePathExist(Path.Combine(TempPath, "Media"));

                // Clean the media folder
                foreach (string mediaPath in Directory.EnumerateFiles(Path.Combine(TempPath, "Media")))
                {
                    if (!QuestionSet.Media.Select(x => Path.GetFileName(x.FileName)).Contains(Path.GetFileName(mediaPath)))
                    {
                        File.Delete(mediaPath);
                    }
                }

                using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Update, true))
                {
                    for (int i = archive.Entries.Count - 1; i >= 0; i--)
                    {
                        archive.Entries[i].Delete();
                    }
                    archive.CreateEntryFromDirectory(TempPath);
                }
                fileStream.Flush();
            });
            LastSaved = DateTime.Now;
        }

        /// <summary>
        /// Add a media file to the question set
        /// </summary>
        /// <param name="path">Path to the media file</param>
        /// <exception cref="InvalidOperationException">Thrown if <code>QuestionSet</code> is null.</exception>
        public async Task<string> AddMedia(string path)
        {
            if (QuestionSet == null)
                throw new InvalidOperationException("QuestionSet is null");

            Media media = new Media();
            await Task.Run(() =>
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                using (BufferedStream bs = new BufferedStream(fs))
                {
                    using (SHA1Managed sha1 = new SHA1Managed())
                    {
                        media.Name = Path.GetFileName(path);
                        media.Id = BitConverter.ToString(sha1.ComputeHash(bs)).Replace("-", "");
                        media.FileName = Path.Combine("Media", $"{media.Id}{Path.GetExtension(path)}");
                        media.DataSource = this;
                    }
                }
                string newPath = Path.Combine(TempPath, media.FileName);
                if (!File.Exists(newPath))
                    File.Copy(path, newPath);
            });
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (!QuestionSet.Media.Any(x => x.Id == media.Id))
                {
                    QuestionSet.Media.Add(media);
                }
            });
            return media.Id;
        }

        /// <summary>
        /// Replace a media file with another one, updating all related references
        /// </summary>
        /// <param name="oldMedia">Old media object</param>
        /// <param name="newMediaPath">Path to new media file</param>
        /// <returns>The new media object, null if the new media object is the same as the old one.</returns>
        public async Task<Media> ReplaceMedia(Media oldMedia, string newMediaPath)
        {
            string replaceReferences(string text, string oldPath, string newPath)
            {
                return Regex.Replace(text, $@"!\[(.*?)\]\({Regex.Escape(oldPath.Replace('\\', '/'))}\)", $"![$1]({newPath.Replace('\\', '/')})");
            }

            string id = await AddMedia(newMediaPath);
            if (id == oldMedia.Id)
                return null;
            Media newMedia = QuestionSet.Media.First(x => x.Id == id);

            Application.Current.Dispatcher.Invoke(() =>
            {
                QuestionSet.Media.Remove(newMedia);
                QuestionSet.Media.Insert(QuestionSet.Media.IndexOf(oldMedia), newMedia);
                foreach (Question question in QuestionSet.Questions)
                {
                    question.Text = replaceReferences(question.Text, oldMedia.FileName, newMedia.FileName);
                    if (question.Answers != null)
                    {
                        for (int i = 0; i < question.Answers.Count; i++)
                        {
                            question.Answers[i] = replaceReferences(question.Answers[i], oldMedia.FileName, newMedia.FileName);
                        }
                    }
                }

                QuestionSet.Media.Remove(oldMedia);
            });

            return newMedia;
        }

        /// <summary>
        /// Delete a media file from the question set. Does NOT remove the media entry from the question set.
        /// </summary>
        public async Task DeleteMedia(Media media)
        {
            await Task.Run(() =>
            {
                string path = Path.Combine(TempPath, media.FileName);
                if (File.Exists(path))
                    File.Delete(path);
            });
        }
    }
}
