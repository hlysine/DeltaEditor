using MoreLinq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace DeltaEditor.Helpers
{
    public class NotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void NotifyChanged(IEnumerable<string> propertyName)
        {
            propertyName.ForEach(x => NotifyChanged(x));
        }

        protected void SetAndNotify<T>(ref T variable, T value, IEnumerable<string> calculatedProperties = null, [CallerMemberName] string memberName = null)
        {
            Contract.Requires(memberName != null);
            variable = value;
            NotifyChanged(memberName);
            if (calculatedProperties != null)
                NotifyChanged(calculatedProperties);
        }
    }
}
