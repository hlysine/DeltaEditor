using DeltaEditor.Helpers;
using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace DeltaEditor.ViewModels
{
    class ExceptionViewModel : NotifyPropertyChanged
    {
        private Exception exception;
        public Exception Exception
        {
            get => exception;
            set => SetAndNotify(ref exception, value, new[] { nameof(ExceptionBody) });
        }
        private string missingDependencyLink = null;
        public string MissingDependencyLink
        {
            get => missingDependencyLink;
            set => SetAndNotify(ref missingDependencyLink, value);
        }

        public string ExceptionBody => Helper.ExceptionToString(exception);
        ICommand githubIssueCommand;
        public ICommand GithubIssueCommand => githubIssueCommand ??= new RelayCommand(
                    // execute
                    (param) =>
                    {
                        StringBuilder message = new StringBuilder();
                        System.Reflection.AssemblyName assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
                        message.AppendLine($"Exception in {assemblyName.Name} v{assemblyName.Version}");
                        message.AppendLine($"Source: {exception.Source}");
                        message.AppendLine(exception.Message);
                        message.AppendLine();
                        message.AppendLine(exception.StackTrace);
                        Process.Start($"https://github.com/Profound-Education-Centre/DeltaEditor/issues/new?assignees=&labels=bug&template=bug_report.md&title=&body={WebUtility.UrlEncode(string.Format(Properties.Resources.issue_template, message.ToString()))}");
                        Window window = param as Window;
                        if (window == null)
                            return;
                        window.Close();
                    },
                    // can execute
                    (param) =>
                    {
                        return Exception != null;
                    }
                );
        ICommand downloadDependencyCommand;
        public ICommand DownloadDependencyCommand => downloadDependencyCommand ??= new RelayCommand(
                    // execute
                    (param) =>
                    {
                        Process.Start(MissingDependencyLink);
                    },
                    // can execute
                    (param) =>
                    {
                        return !MissingDependencyLink.IsNullOrWhiteSpace();
                    }
                );
        ICommand closeWindowCommand;
        public ICommand CloseWindowCommand => closeWindowCommand ??= new RelayCommand(
                    // execute
                    (param) =>
                    {
                        Window window = param as Window;
                        if (window == null)
                            return;
                        window.Close();
                    },
                    // can execute
                    (param) =>
                    {
                        return true;
                    }
                );
    }
}
