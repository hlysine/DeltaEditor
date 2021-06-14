using DeltaQuestionEditor_WPF.Helpers;

namespace DeltaQuestionEditor_WPF.Config
{
    public class ConfigObject : NotifyPropertyChanged
    {
        private bool hideWelcomeDialog = false;
        public bool HideWelcomeDialog
        {
            get => hideWelcomeDialog;
            set => SetAndNotify(ref hideWelcomeDialog, value);
        }
    }
}
