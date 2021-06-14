using DeltaQuestionEditor_WPF.Helpers;

namespace DeltaQuestionEditor_WPF.Models.Validation
{
    public class ValidationProblem : NotifyPropertyChanged
    {
        private string description;
        public string Description
        {
            get => description;
            set => SetAndNotify(ref description, value);
        }
        private ProblemSeverity severity;
        public ProblemSeverity Severity
        {
            get => severity;
            set => SetAndNotify(ref severity, value);
        }
        private object focusObject;
        public object FocusObject
        {
            get => focusObject;
            set => SetAndNotify(ref focusObject, value);
        }

        public ValidationProblem(ProblemSeverity severity, string description, object focusObject)
        {
            (Severity, Description, FocusObject) = (severity, description, focusObject);
        }
    }

    public enum ProblemSeverity
    {
        Error,
        Warning
    }
}
