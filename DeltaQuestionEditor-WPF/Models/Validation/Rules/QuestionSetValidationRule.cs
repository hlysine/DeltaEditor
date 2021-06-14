using System.Collections.Generic;

namespace DeltaQuestionEditor_WPF.Models.Validation.Rules
{
    public abstract class QuestionSetValidationRule
    {
        public abstract List<ValidationProblem> Validate(QuestionSet questionSet);
    }
}
