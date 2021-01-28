using DeltaQuestionEditor_WPF.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
