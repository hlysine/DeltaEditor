using System;

namespace DeltaQuestionEditor_WPF.Logging
{
    class ConsoleLogger : ILogger
    {
        public void Log(string text)
        {
            Console.WriteLine(text);
        }
    }
}
