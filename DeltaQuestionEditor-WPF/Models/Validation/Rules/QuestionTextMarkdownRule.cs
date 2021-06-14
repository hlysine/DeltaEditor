using DeltaQuestionEditor_WPF.Consts;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DeltaQuestionEditor_WPF.Models.Validation.Rules
{
    using static DeltaQuestionEditor_WPF.Helpers.Helper;
    public class QuestionTextMarkdownRule : QuestionSetValidationRule
    {
        private List<ValidationProblem> validateText(Question question, int i, string text, string textName)
        {
            List<ValidationProblem> problems = new List<ValidationProblem>();
            if (!text.IsNullOrWhiteSpace())
            {
                // Check for misinterpreted AsciiMath
                string tmp = text;
                MatchCollection matches = Regex.Matches(tmp, @"(?<!\\)\\*`");
                foreach (Match match in matches)
                {
                    if ((match.Value.Count(x => x == '\\') / 2) % 2 == 1)
                    {
                        tmp = Regex.Replace(tmp, @"(?<!\\)" + match.Value.Replace("\\", "\\\\"), "");
                    }
                }
                matches = Regex.Matches(tmp, @"`.*?`");
                if (matches.Cast<Match>().Any(x => Regex.IsMatch(x.Value, @"[a-zA-Z]{2}\s[a-zA-Z]{2}")))
                {
                    problems.Add(new ValidationProblem(ProblemSeverity.Warning, string.Format(ValidationProblems.TEXT_ASCIIMATH_MISINTERPRETATION, textName, i + 1), question));
                }

                // Check for misinterpreted LaTeX
                tmp = text;
                matches = Regex.Matches(tmp, @"(?<!\\)\\*\$");
                foreach (Match match in matches)
                {
                    if ((match.Value.Count(x => x == '\\') / 2) % 2 == 1)
                    {
                        tmp = Regex.Replace(tmp, @"(?<!\\)" + match.Value.Replace("\\", "\\\\").Replace("$", @"\$"), "");
                    }
                }
                matches = Regex.Matches(tmp, @"\$.*?\$");
                if (matches.Cast<Match>().Any(x => Regex.IsMatch(x.Value, @"[a-zA-Z]{2}\s[a-zA-Z]{2}")))
                {
                    problems.Add(new ValidationProblem(ProblemSeverity.Warning, string.Format(ValidationProblems.TEXT_LATEX_MISINTERPRETATION, textName, i + 1), question));
                }

                // Check for unformatted math
                tmp = text;
                matches = Regex.Matches(tmp, @"(?<!\\)\\*`");
                foreach (Match match in matches)
                {
                    if ((match.Value.Count(x => x == '\\') / 2) % 2 == 1)
                    {
                        tmp = Regex.Replace(tmp, @"(?<!\\)" + match.Value.Replace("\\", "\\\\"), "");
                    }
                }
                matches = Regex.Matches(tmp, @"`.*?`");
                foreach (Match match in matches)
                {
                    tmp = tmp.Replace(match.Value, "");
                }
                matches = Regex.Matches(tmp, @"(?<!\\)\\*\$");
                foreach (Match match in matches)
                {
                    if ((match.Value.Count(x => x == '\\') / 2) % 2 == 1)
                    {
                        tmp = Regex.Replace(tmp, @"(?<!\\)" + match.Value.Replace("\\", "\\\\").Replace("$", @"\$"), "");
                    }
                }
                matches = Regex.Matches(tmp, @"\$.*?\$");
                foreach (Match match in matches)
                {
                    tmp = tmp.Replace(match.Value, "");
                }
                matches = Regex.Matches(tmp, @"(?<=\w{2,}\s*)[-/](?=\s*\w{2,})");
                for (int j = matches.Count - 1; j >= 0; j--)
                {
                    Match match = matches[j];
                    tmp = tmp.Substring(0, match.Index) + tmp.Substring(match.Index + match.Length);
                }
                if (Regex.IsMatch(tmp, @"[+\-*/0-9]"))
                {
                    problems.Add(new ValidationProblem(ProblemSeverity.Warning, string.Format(ValidationProblems.TEXT_UNFORMATTED_MATH, textName, i + 1), question));
                }
            }
            return problems;
        }

        public override List<ValidationProblem> Validate(QuestionSet questionSet)
        {
            List<ValidationProblem> problems = new List<ValidationProblem>();
            for (int i = 0; i < questionSet.Questions.Count; i++)
            {
                Question question = questionSet.Questions[i];
                problems.AddRange(validateText(question, i, question.Text, "question text"));
                if (question.Answers != null && question.Answers.Count == 4)
                {
                    problems.AddRange(validateText(question, i, question.Answers[0], "correct answer"));
                    problems.AddRange(validateText(question, i, question.Answers[1], "first wrong answer"));
                    problems.AddRange(validateText(question, i, question.Answers[2], "second wrong answer"));
                    problems.AddRange(validateText(question, i, question.Answers[3], "third wrong answer"));
                }
            }
            return problems;
        }
    }
}
