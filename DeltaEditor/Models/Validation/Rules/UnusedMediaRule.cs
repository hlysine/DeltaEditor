using DeltaEditor.Consts;
using MoreLinq;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DeltaEditor.Models.Validation.Rules
{
    using static DeltaEditor.Helpers.Helper;
    public class UnusedMediaRule : QuestionSetValidationRule
    {
        private List<string> findMedia(string text)
        {
            if (!text.IsNullOrWhiteSpace())
            {
                MatchCollection matches = Regex.Matches(text, @"!\[(.*?)(?<!(?<!\\)\\(?:\\\\)*)\]\((.*?)(?<!(?<!\\)\\(?:\\\\)*)\)");
                return matches.Cast<Match>().Select(x => x.Groups[2].Value.Replace('\\', '/')).ToList();
            }
            return new List<string>();
        }

        public override List<ValidationProblem> Validate(QuestionSet questionSet)
        {
            List<ValidationProblem> problems = new List<ValidationProblem>();
            List<string> usedMedia = new List<string>();
            for (int i = 0; i < questionSet.Questions.Count; i++)
            {
                Question question = questionSet.Questions[i];
                usedMedia.AddRange(findMedia(question.Text));
                if (question.Answers != null)
                {
                    foreach (string ans in question.Answers)
                    {
                        usedMedia.AddRange(findMedia(ans));
                    }
                }
            }
            questionSet.Media.Where(x => !usedMedia.Contains(x.FileName.Replace('\\', '/')))
                .ForEach(x => problems.Add(new ValidationProblem(ProblemSeverity.Warning, string.Format(ValidationProblems.MEDIA_UNUSED, x.Name), x)));
            return problems;
        }
    }
}
