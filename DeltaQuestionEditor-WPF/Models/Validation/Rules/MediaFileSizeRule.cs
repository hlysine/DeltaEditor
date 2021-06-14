using DeltaQuestionEditor_WPF.Consts;
using System.Collections.Generic;
using System.IO;

namespace DeltaQuestionEditor_WPF.Models.Validation.Rules
{
    public class MediaFileSizeRule : QuestionSetValidationRule
    {
        private const int MAX_FILE_SIZE = 500 * 1024;

        public override List<ValidationProblem> Validate(QuestionSet questionSet)
        {
            List<ValidationProblem> problems = new List<ValidationProblem>();
            foreach (Media media in questionSet.Media)
            {
                if (new FileInfo(media.FullPath).Length > MAX_FILE_SIZE)
                {
                    problems.Add(new ValidationProblem(ProblemSeverity.Error, string.Format(ValidationProblems.MEDIA_TOO_LARGE, media.Name, media.FileName), media));
                }
            }
            return problems;
        }
    }
}
