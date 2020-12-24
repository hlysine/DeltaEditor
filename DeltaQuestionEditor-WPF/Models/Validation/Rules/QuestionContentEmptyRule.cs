using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeltaQuestionEditor_WPF.Models.Validation.Rules
{
    using static DeltaQuestionEditor_WPF.Helpers.Helper;
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
                    problems.Add(new ValidationProblem(ProblemSeverity.Error, $"The question text of question {i + 1} is empty.", question));
                }
                if (question.Answers == null || question.Answers.Count != 4)
                {
                    problems.Add(new ValidationProblem(ProblemSeverity.Error, $"The number of answers in question {i + 1} is incorrect.", question));
                }
                if (question.Answers[0].IsNullOrWhiteSpace())
                {
                    problems.Add(new ValidationProblem(ProblemSeverity.Error, $"The correct answer of question {i + 1} is empty.", question));
                }
                if (question.Answers[1].IsNullOrWhiteSpace())
                {
                    problems.Add(new ValidationProblem(ProblemSeverity.Error, $"The first wrong answer of question {i + 1} is empty.", question));
                }
                if (question.Answers[2].IsNullOrWhiteSpace())
                {
                    problems.Add(new ValidationProblem(ProblemSeverity.Error, $"The second wrong answer of question {i + 1} is empty.", question));
                }
                if (question.Answers[3].IsNullOrWhiteSpace())
                {
                    problems.Add(new ValidationProblem(ProblemSeverity.Error, $"The third wrong answer of question {i + 1} is empty.", question));
                }
                if (question.Difficulty < 1 || question.Difficulty > 3)
                {
                    problems.Add(new ValidationProblem(ProblemSeverity.Error, $"The difficulty of question {i + 1} is invalid.", question));
                }
                if (question.Skills == null || question.Skills.Count == 0)
                {
                    problems.Add(new ValidationProblem(ProblemSeverity.Error, $"The skills of question {i + 1} is empty.", question));
                }
            }
            return problems;
        }
    }
}
