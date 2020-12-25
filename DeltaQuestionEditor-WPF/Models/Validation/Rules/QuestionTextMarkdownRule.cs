using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
                    problems.Add(new ValidationProblem(ProblemSeverity.Warning, $"The backtick signs (`) in the {textName} of question {i + 1} seem to be misinterpreted as AsciiMath. Please double-check for errors.", question));
                }
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
                    problems.Add(new ValidationProblem(ProblemSeverity.Warning, $"The dollar signs ($) in the {textName} of question {i + 1} seem to be misinterpreted as LaTeX. Please double-check for errors.", question));
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
