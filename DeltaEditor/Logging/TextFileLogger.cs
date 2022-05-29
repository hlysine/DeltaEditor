using System.IO;

namespace DeltaEditor.Logging
{
    using static DeltaEditor.Helpers.Helper;
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
