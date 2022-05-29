using DeltaEditor.ViewModels;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace DeltaEditor.Views
{
    using static Helpers.Helper;
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel viewModel;

        public MainWindow()
        {
            DataContext = viewModel = new MainViewModel();
            InitializeComponent();
            HideBoundingBox(root);
            HideBoundingBox(topicSelectorDialogHost.DialogContent);
            HideBoundingBox(welcomeDialogHost.DialogContent);
            HideBoundingBox(validatorDialogHost.DialogContent);
            HideBoundingBox(mainDialogHost.DialogContent);
        }

        private void mainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (viewModel.CloseWindowCommand.CanExecute((e, this)))
                viewModel.CloseWindowCommand.Execute((e, this));
        }

        private void btnExitApp_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void mediaPanel_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (viewModel.AddMediaCommand.CanExecute(files))
                    viewModel.AddMediaCommand.Execute(files);
            }
        }

        private void gridWelcomePanel_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files.Length == 0)
                    return;

                var qdb = files.Where(x => Path.GetExtension(x) == ".qdb").ToArray();

                if (qdb.Length > 0)
                    if (viewModel.OpenFileCommand.CanExecute(qdb))
                        viewModel.OpenFileCommand.Execute(qdb);

                var xls = files.Where(x => Regex.IsMatch(Path.GetExtension(x), @"\.xls[xb]?")).ToArray();

                if (xls.Length > 0)
                    if (viewModel.ImportFromExcelCommand.CanExecute(xls))
                        viewModel.ImportFromExcelCommand.Execute(xls);
            }
        }

        private void welcomeDialogHost_DialogOpened(object sender, MaterialDesignThemes.Wpf.DialogOpenedEventArgs eventArgs)
        {
            scrollWelcomeDialog.ScrollToTop();
        }
    }
}
