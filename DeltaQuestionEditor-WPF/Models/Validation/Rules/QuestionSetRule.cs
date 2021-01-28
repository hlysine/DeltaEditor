using DeltaQuestionEditor_WPF.Consts;
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
                problems.Add(new ValidationProblem(ProblemSeverity.Error, ValidationProblems.QUESTION_SET_EMPTY, null));
            }
            if (questionSet.TopicName == "Invalid topic")
            {
                problems.Add(new ValidationProblem(ProblemSeverity.Error, ValidationProblems.QUESTION_SET_INVALID_TOPIC, null));
            }
            return problems;
        }
    }
}
