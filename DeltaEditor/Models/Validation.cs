using DeltaEditor.Helpers;
using System;

namespace DeltaEditor.Models
{
    public class ValidationToken : NotifyPropertyChanged
    {
        private string hash;
        public string Hash
        {
            get => hash;
            set => SetAndNotify(ref hash, value);
        }

        private DateTime timestamp;
        public DateTime Timestamp
        {
            get => timestamp;
            set => SetAndNotify(ref timestamp, value);
        }
    }
}
