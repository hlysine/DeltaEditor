using System.Windows;

namespace DeltaQuestionEditor_WPF.Views
{
    using static DeltaQuestionEditor_WPF.Helpers.Helper;
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
