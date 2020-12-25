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
    public class QuestionMediaMarkdownRule : QuestionSetValidationRule
    {
        private List<ValidationProblem> validateText(QuestionSet questionSet, Question question, int i, string text, string textName)
        {
            List<ValidationProblem> problems = new List<ValidationProblem>();
            if (!text.IsNullOrWhiteSpace())
            {
                MatchCollection matches = Regex.Matches(text, @"!\[(.*?)(?<!(?<!\\)\\(?:\\\\)*)\]\((.*?)(?<!(?<!\\)\\(?:\\\\)*)\)");
                bool customAltText = false;
                bool nonexistentPath = false;
                foreach (Match match in matches)
                {
                    if (match.Groups[1].Value != "media")
                    {
                        customAltText = true;
                    }
                    if (!questionSet.Media.Select(x => x.FileName.Replace('\\', '/')).Contains(match.Groups[2].Value.Replace('\\', '/')))
                    {
                        nonexistentPath = true;
                    }
                }
                if (customAltText)
                {
                    problems.Add(new ValidationProblem(ProblemSeverity.Warning, $"The media code in the {textName} of question {i + 1} contains custom alternate text. Please avoid doing so if possible.", question));
                }
                if (nonexistentPath)
                {
                    problems.Add(new ValidationProblem(ProblemSeverity.Error, $"The media code in the {textName} of question {i + 1} contains invalid path. Please make sure that the related media files are imported and the path is correct.", question));
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
                problems.AddRange(validateText(questionSet, question, i, question.Text, "question text"));
                if (question.Answers != null && question.Answers.Count == 4)
                {
                    problems.AddRange(validateText(questionSet, question, i, question.Answers[0], "correct answer"));
                    problems.AddRange(validateText(questionSet, question, i, question.Answers[1], "first wrong answer"));
                    problems.AddRange(validateText(questionSet, question, i, question.Answers[2], "second wrong answer"));
                    problems.AddRange(validateText(questionSet, question, i, question.Answers[3], "third wrong answer"));
                }
            }
            return problems;
        }
    }
}
