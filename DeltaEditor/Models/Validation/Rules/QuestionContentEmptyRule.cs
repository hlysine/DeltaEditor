using DeltaEditor.Consts;
using System.Collections.Generic;

namespace DeltaEditor.Models.Validation.Rules
{
    using static DeltaEditor.Helpers.Helper;
    public class QuestionContentEmptyRule : QuestionSetValidationRule
    {
        public override List<ValidationProblem> Validate(QuestionSet questionSet)
        {
            List<ValidationProblem> problems = new List<ValidationProblem>();
            for (int i = 0; i < questionSet.Questions.Count; i++)
            {
                Question question = questionSet.Questions[i];
                if (question.Text.IsNullOrWhiteSpace())
                {
                    problems.Add(new ValidationProblem(ProblemSeverity.Error, string.Format(ValidationProblems.CONTENT_EMPTY, "question text", i + 1), question));
                }
                if (question.Answers == null || question.Answers.Count != 4)
                {
                    problems.Add(new ValidationProblem(ProblemSeverity.Error, string.Format(ValidationProblems.WRONG_NUMBER_OF_ANSWERS, i + 1), question));
                }
                if (question.Answers[0].IsNullOrWhiteSpace())
                {
                    problems.Add(new ValidationProblem(ProblemSeverity.Error, string.Format(ValidationProblems.CONTENT_EMPTY, "correct answer", i + 1), question));
                }
                if (question.Answers[1].IsNullOrWhiteSpace())
                {
                    problems.Add(new ValidationProblem(ProblemSeverity.Error, string.Format(ValidationProblems.CONTENT_EMPTY, "first wrong answer", i + 1), question));
                }
                if (question.Answers[2].IsNullOrWhiteSpace())
                {
                    problems.Add(new ValidationProblem(ProblemSeverity.Error, string.Format(ValidationProblems.CONTENT_EMPTY, "second wrong answer", i + 1), question));
                }
                if (question.Answers[3].IsNullOrWhiteSpace())
                {
                    problems.Add(new ValidationProblem(ProblemSeverity.Error, string.Format(ValidationProblems.CONTENT_EMPTY, "third wrong answer", i + 1), question));
                }
                if (question.Difficulty < 1 || question.Difficulty > 3)
                {
                    problems.Add(new ValidationProblem(ProblemSeverity.Error, string.Format(ValidationProblems.WRONG_DIFFICULTY, i + 1), question));
                }
                if (question.Skills == null || question.Skills.Count == 0)
                {
                    problems.Add(new ValidationProblem(ProblemSeverity.Error, string.Format(ValidationProblems.CONTENT_EMPTY, "skills", i + 1), question));
                }
            }
            return problems;
        }
    }
}
