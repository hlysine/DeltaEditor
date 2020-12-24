using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeltaQuestionEditor_WPF.Models.Validation.Rules
{
    using static DeltaQuestionEditor_WPF.Helpers.Helper;
    public class QuestionSetRule : QuestionSetValidationRule
    {
        public override List<ValidationProblem> Validate(QuestionSet questionSet)
        {
            List<ValidationProblem> problems = new List<ValidationProblem>();
            if (questionSet.Questions == null || questionSet.Questions.Count == 0)
            {
                problems.Add(new ValidationProblem(ProblemSeverity.Error, "There are no questions in this question set.", null));
            }
            if (questionSet.TopicName == "Invalid topic")
            {
                problems.Add(new ValidationProblem(ProblemSeverity.Error, "This question set has an invalid topic code.", null));
            }
            return problems;
        }
    }
}
