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

namespace DeltaQuestionEditor_WPF
{
    using static Helpers.Helper;
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        int updateProgress = 0;
        public int UpdateProgress
        {
            get
            {
                return updateProgress;
            }
            set
            {
                updateProgress = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UpdateProgress)));
            }
        }

        string title;

        SemaphoreSlim updateFinished = new SemaphoreSlim(0, 1);

        public MainWindow()
        {
            InitializeComponent();
            HideBoundingBox(root);

            Title = "Delta Question Editor v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            title = Title;
        }

        private async void mainWindow_SourceInitialized(object sender, EventArgs e)
        {
            try
            {
                // TODO: github link
                using (var mgr = await UpdateManager.GitHubUpdateManager("https://github.com/Henry-YSLin/DeltaQuestionEditor-WPF"))
                {
                    var updateInfo = await mgr.CheckForUpdate(false, (progress) =>
                    {
                        UpdateProgress = progress;
                        Dispatcher.Invoke(() => Title = title + $" - Checking {progress}%");
                    });
                    if (updateInfo.ReleasesToApply.Any())
                    {
                        var result = await mgr.UpdateApp((progress) =>
                        {
                            UpdateProgress = progress;
                            Dispatcher.Invoke(() => Title = title + $" - Updating {progress}%");
                        });
                        await Task.Delay(500);
                        Dispatcher.Invoke(() => Title = title + " - Restart app to update");
                    }
                    else
                    {
                        Dispatcher.Invoke(() => Title = title);
                    }
                }
            }
            catch (Exception ex)
            {
                App.LogException(ex, ex.Source);
            }
            updateFinished.Release();
        }

        bool canExit = false;

        private async void mainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (canExit)
            {
                return;
            }
            if (updateFinished.CurrentCount <= 0)
            {
                // Still updating
                e.Cancel = true;
                //Show progress panel
                await updateFinished.WaitAsync();
                canExit = true;
                Close();
            }
        }
    }
}
