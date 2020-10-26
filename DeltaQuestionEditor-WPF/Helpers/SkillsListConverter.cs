using MoreLinq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DeltaQuestionEditor_WPF.Helpers
{
    class SkillsListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ObservableCollection<string> val = value as ObservableCollection<string>;
            if (val == null) return null;
            return string.Join(",", val);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string val = value as string;
            if (val.IsNullOrWhiteSpace()) return new ObservableCollection<string>();
            val = val.Replace(" ", "");
            ObservableCollection<string> collection = new ObservableCollection<string>();
            val.Split(',').ForEach(x => collection.Add(x));
            return collection;
        }
    }
}
