using System.IO;

namespace DeltaQuestionEditor_WPF.Logging
{
    using static DeltaQuestionEditor_WPF.Helpers.Helper;
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
