using System;
using System.Globalization;
using System.Windows.Data;

namespace DeltaQuestionEditor_WPF.Helpers
{
    class WindowTitleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 1)
                return "Delta Question Editor";
            string fileName = values[0] as string;
            string appVersion = null;
            if (values.Length > 1)
                appVersion = values[1] as string;
#if DEBUG
            string ret = "Delta Question Editor (Debug)";
#else
            string ret = "Delta Question Editor";
#endif
            if (!fileName.IsNullOrEmpty())
                ret = fileName + " - " + ret;
            if (!appVersion.IsNullOrWhiteSpace())
                ret += " v" + appVersion;
            return ret;

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
