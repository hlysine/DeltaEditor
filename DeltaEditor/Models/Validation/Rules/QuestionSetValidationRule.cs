using System.Collections.Generic;

namespace DeltaEditor.Models.Validation.Rules
{
    public abstract class QuestionSetValidationRule
    {
        public abstract List<ValidationProblem> Validate(QuestionSet questionSet);
    }
}
