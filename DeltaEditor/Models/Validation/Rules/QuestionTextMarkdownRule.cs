using DeltaEditor.Consts;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DeltaEditor.Models.Validation.Rules
{
    using static DeltaEditor.Helpers.Helper;
    public class QuestionTextMarkdownRule : QuestionSetValidationRule
    {
        private bool checkAsciiMath(string text)
        {
            var matches = Regex.Matches(text, @"(?<!\\)\\*`");
            foreach (Match match in matches)
            {
                if ((match.Value.Count(x => x == '\\') / 2) % 2 == 1)
                {
                    text = Regex.Replace(text, @"(?<!\\)" + match.Value.Replace("\\", "\\\\"), "");
                }
            }
            matches = Regex.Matches(text, @"`.*?`");
            return matches.Cast<Match>().Any(x => Regex.IsMatch(x.Value, @"[a-zA-Z]{2}\s[a-zA-Z]{2}"));
        }

        private bool checkLaTeX(string text)
        {
            var matches = Regex.Matches(text, @"(?<!\\)\\*\$");
            foreach (Match match in matches)
            {
                if ((match.Value.Count(x => x == '\\') / 2) % 2 == 1)
                {
                    text = Regex.Replace(text, @"(?<!\\)" + match.Value.Replace("\\", "\\\\").Replace("$", @"\$"), "");
                }
            }
            matches = Regex.Matches(text, @"\$.*?\$");
            return matches.Cast<Match>().Any(x => Regex.IsMatch(x.Value, @"[a-zA-Z]{2}\s[a-zA-Z]{2}"));
        }

        private bool checkUnformattedMath(string text)
        {
            var matches = Regex.Matches(text, @"(?<!\\)\\*`");
            foreach (Match match in matches)
            {
                if ((match.Value.Count(x => x == '\\') / 2) % 2 == 1)
                {
                    text = Regex.Replace(text, @"(?<!\\)" + match.Value.Replace("\\", "\\\\"), "");
                }
            }
            matches = Regex.Matches(text, @"`.*?`");
            foreach (Match match in matches)
            {
                text = text.Replace(match.Value, "");
            }
            matches = Regex.Matches(text, @"(?<!\\)\\*\$");
            foreach (Match match in matches)
            {
                if ((match.Value.Count(x => x == '\\') / 2) % 2 == 1)
                {
                    text = Regex.Replace(text, @"(?<!\\)" + match.Value.Replace("\\", "\\\\").Replace("$", @"\$"), "");
                }
            }
            matches = Regex.Matches(text, @"\$.*?\$");
            foreach (Match match in matches)
            {
                text = text.Replace(match.Value, "");
            }
            matches = Regex.Matches(text, @"(?<=\w{2,}\s*)[-/](?=\s*\w{2,})");
            for (int j = matches.Count - 1; j >= 0; j--)
            {
                Match match = matches[j];
                text = text.Substring(0, match.Index) + text.Substring(match.Index + match.Length);
            }
            return Regex.IsMatch(text, @"[+\-*/0-9]");
        }

        private List<ValidationProblem> validateText(Question question, int i, string text, string textName)
        {
            List<ValidationProblem> problems = new List<ValidationProblem>();
            if (!text.IsNullOrWhiteSpace())
            {
                // Check for misinterpreted AsciiMath
                if (checkAsciiMath(text))
                    problems.Add(new ValidationProblem(ProblemSeverity.Warning, string.Format(ValidationProblems.TEXT_ASCIIMATH_MISINTERPRETATION, textName, i + 1), question));

                // Check for misinterpreted LaTeX
                if (checkLaTeX(text))
                    problems.Add(new ValidationProblem(ProblemSeverity.Warning, string.Format(ValidationProblems.TEXT_LATEX_MISINTERPRETATION, textName, i + 1), question));

                // Check for unformatted math
                if (checkUnformattedMath(text))
                    problems.Add(new ValidationProblem(ProblemSeverity.Warning, string.Format(ValidationProblems.TEXT_UNFORMATTED_MATH, textName, i + 1), question));
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
