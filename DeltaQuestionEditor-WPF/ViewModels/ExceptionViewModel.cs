using DeltaQuestionEditor_WPF.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DeltaQuestionEditor_WPF.ViewModels
{
    class ExceptionViewModel : NotifyPropertyChanged
    {
        private Exception exception;
        public Exception Exception
        {
            get => exception;
            set => SetAndNotify(ref exception, value, new[] { nameof(ExceptionBody) });
        }

        private string getExceptionBody(Exception ex)
        {
            string ret = string.Format("{0} in {1}\r\n\r\nStack Trace:\r\n{2}", ex.Message, ex.Source, ex.StackTrace);
            if (ex.InnerException != null)
            {
                ret += "\r\n\r\nInner exception:\r\n" + getExceptionBody(ex.InnerException);
            }
            return ret;
        }

        public string ExceptionBody
        {
            get
            {
                return getExceptionBody(exception);
            }
        }


        ICommand githubIssueCommand;
        public ICommand GithubIssueCommand
        {
            get
            {
                return githubIssueCommand ??= new RelayCommand(
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
                        Process.Start($"https://github.com/Profound-Education-Centre/DeltaQuestionEditor-WPF/issues/new?assignees=&labels=bug&template=bug_report.md&title=&body={WebUtility.UrlEncode(string.Format(Properties.Resources.issue_template, message.ToString()))}");
                        Window window = param as Window;
                        if (window == null) return;
                        window.Close();
                    },
                    // can execute
                    (param) =>
                    {
                        return Exception != null;
                    }
                );
            }
        }


        ICommand closeWindowCommand;
        public ICommand CloseWindowCommand
        {
            get
            {
                return closeWindowCommand ??= new RelayCommand(
                    // execute
                    (param) =>
                    {
                        Window window = param as Window;
                        if (window == null) return;
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
    }
}
