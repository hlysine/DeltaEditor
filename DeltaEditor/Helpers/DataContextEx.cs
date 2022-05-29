using System;
using System.Windows;

namespace DeltaEditor.Helpers
{
    public static class DataContextEx
    {
        public static readonly DependencyProperty DataContextExProperty =
            DependencyProperty.RegisterAttached("DataContextEx",
                                       typeof(Object),
                                       typeof(DataContextEx));

        public static Object GetDataContextEx(DependencyObject element)
        {
            return element.GetValue(DataContextExProperty);
        }

        public static void SetDataContextEx(DependencyObject element, Object value)
        {
            element.SetValue(DataContextExProperty, value);
        }
    }
}
