using System;
using System.Globalization;
using System.Windows.Data;

namespace DeltaQuestionEditor_WPF.Helpers
{
    class BoolToStringNotConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as bool?).GetValueOrDefault() ? "" : "Not ";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
