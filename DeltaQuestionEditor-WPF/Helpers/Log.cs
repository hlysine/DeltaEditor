using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeltaQuestionEditor_WPF.Helpers
{
    using static Helper;
    static class Logger
    {
        public static List<ILogger> Loggers { get; private set; } = new List<ILogger>();

        public static void LogException(Exception exception, string source, Severity severity = Severity.Error)
        {
            StringBuilder message = new StringBuilder();
            message.AppendLine($"Exception from ({source})");
            try
            {
                System.Reflection.AssemblyName assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
                message.AppendLine($" in {assemblyName.Name} v{assemblyName.Version}");
                message.AppendLine(exception.Message);
                message.AppendLine(exception.StackTrace);
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

    interface ILogger
    {
        void Log(string text);
    }

    class ConsoleLogger : ILogger
    {
        public void Log(string text)
        {
            Console.WriteLine(text);
        }
    }

    class TextFileLogger : ILogger
    {
        public TextFileLogger()
        {
            EnsurePathExist(AppDataPath());
        }

        public void Log(string text)
        {
            File.AppendAllText(AppDataPath("log.txt"), text);
        }
    }
}
