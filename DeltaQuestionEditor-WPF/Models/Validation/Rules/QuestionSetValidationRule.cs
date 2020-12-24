using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeltaQuestionEditor_WPF.Models.Validation.Rules
{
    public abstract class QuestionSetValidationRule
    {
        public abstract List<ValidationProblem> Validate(QuestionSet questionSet);
    }
}
