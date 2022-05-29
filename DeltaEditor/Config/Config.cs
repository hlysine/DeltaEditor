using DeltaEditor.Helpers;

namespace DeltaEditor.Config
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
