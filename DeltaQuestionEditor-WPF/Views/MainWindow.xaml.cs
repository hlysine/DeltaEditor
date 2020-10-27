using DeltaQuestionEditor_WPF.ViewModels;
using Squirrel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DeltaQuestionEditor_WPF.Views
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
        }

        bool canExit = false;

        private async void mainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (canExit)
            {
                return;
            }
            if (!viewModel.UpdateFinished)
            {
                // Still updating
                e.Cancel = true;
                //Show progress panel

                await viewModel.AwaitUpdateFinish();
                canExit = true;
                Close();
            }
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
    }
}
