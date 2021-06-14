using DeltaQuestionEditor_WPF.Helpers;
using DeltaQuestionEditor_WPF.Logging;
using DeltaQuestionEditor_WPF.ViewModels;
using DeltaQuestionEditor_WPF.Views;
using Squirrel;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace DeltaQuestionEditor_WPF
{
    using static Helpers.Helper;
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            RenderOptions.ProcessRenderMode = RenderMode.Default;
            Logger.Loggers.Add(new ConsoleLogger());
            Logger.Loggers.Add(new TextFileLogger());
            try
            {
                using (var mgr = UpdateManager.GitHubUpdateManager("https://github.com/Profound-Education-Centre/DeltaQuestionEditor-WPF").Result)
                {
                    // Note, in most of these scenarios, the app exits after this method
                    // completes!
                    SquirrelAwareApp.HandleEvents(
                      onInitialInstall: v => mgr.CreateShortcutForThisExe(),
                      onAppUpdate: v => mgr.CreateShortcutForThisExe(),
                      onAppUninstall: v =>
                      {
                          mgr.RemoveShortcutForThisExe();
                          mgr.RemoveUninstallerRegistryEntry();
                      },
                      onFirstRun: () => { FileAssociations.EnsureAssociationsSet(); });
                }
            }
            catch (Exception) { }
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            SetupExceptionHandling();
        }

        private void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                ShowExceptionDialog((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");
            };

            DispatcherUnhandledException += (s, e) =>
            {
                ShowExceptionDialog(e.Exception, "Application.Current.DispatcherUnhandledException");
#if DEBUG
                if (e.Exception is NullReferenceException && e.Exception.Source == "MaterialDesignExtensions")
                    e.Handled = true;
#else
                e.Handled = true;
#endif
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                ShowExceptionDialog(e.Exception, "TaskScheduler.UnobservedTaskException");
#if DEBUG
#else
                e.SetObserved();
#endif
            };
        }

        private bool matchAssemblyException(Exception ex)
        {
            return ex != null && ex.Source == "DeltaQuestionEditor-WPF" && ex is FileNotFoundException && ex.Message.Contains(".dll");
        }

        private void ShowExceptionDialog(Exception ex, string source)
        {
            Logger.LogException(ex, source);
            // TODO: what is this exception?
            if (ex is NullReferenceException && ex.Source == "MaterialDesignExtensions")
                return;
            ExceptionWindow window = new ExceptionWindow();
            ExceptionViewModel viewModel = new ExceptionViewModel();
            viewModel.Exception = ex;
            if (matchAssemblyException(ex.GetInnermostException()))
                viewModel.MissingDependencyLink =
                    "https://www.microsoft.com/en-us/download/details.aspx?id=48145";
            window.DataContext = viewModel;
            window.ShowDialog();
        }
    }
}
