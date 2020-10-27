using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DeltaQuestionEditor_WPF.Helpers
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
