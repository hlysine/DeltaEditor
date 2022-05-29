using System.Windows;

namespace DeltaEditor.Views
{
    using static DeltaEditor.Helpers.Helper;
    /// <summary>
    /// Interaction logic for ExceptionWindow.xaml
    /// </summary>
    public partial class ExceptionWindow : Window
    {
        public ExceptionWindow()
        {
            InitializeComponent();
            HideBoundingBox(root);
        }
    }
}
