using DeltaQuestionEditor_WPF.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeltaQuestionEditor_WPF.Models
{
    public class Media : NotifyPropertyChanged
    {
        private string id;
        public string Id
        {
            get => id;
            set => SetAndNotify(ref id, value);
        }


        private string fileName;
        public string FileName
        {
            get => fileName;
            set => SetAndNotify(ref fileName, value);
        }
    }
}
