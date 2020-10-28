using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace DeltaQuestionEditor_WPF.Helpers
{
    class WindowTitleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 3) return "Delta Question Editor";
            string fileName = values[0] as string;
            string appVersion = values[1] as string;
            string updateString = values[2] as string;
            string ret = "Delta Question Editor";
            if (fileName != null) ret = fileName + " - " + ret;
            if (!appVersion.IsNullOrWhiteSpace()) ret += " v" + appVersion;
            if (!updateString.IsNullOrWhiteSpace()) ret += " - " + updateString;
            return ret;

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
