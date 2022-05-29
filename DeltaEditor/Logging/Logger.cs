using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaEditor.Logging
{
    using static DeltaEditor.Helpers.Helper;
    static class Logger
    {
        public static List<ILogger> Loggers { get; private set; } = new List<ILogger>();

        public static void LogException(Exception exception, string source, Severity severity = Severity.Error)
        {
            StringBuilder message = new StringBuilder();
            try
            {
                message.AppendLine("Exception from " + source);
                message.AppendLine(exception.ExceptionToString());
                message.AppendLine();
                message.AppendLine("===================");
                message.AppendLine();
                Log(message.ToString(), severity);
            }
            catch (Exception)
            {
            }
        }

        public static void Log(string text, Severity severity = Severity.Info)
        {
            string message = $"[{severity}][{DateTime.Now:R}] {text}";
            Loggers.ForEach(x => x.Log(message));
        }
    }

    enum Severity
    {
        Error,
        Warning,
        Info
    }
}
