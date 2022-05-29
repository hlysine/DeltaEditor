using System;

namespace DeltaEditor.Logging
{
    class ConsoleLogger : ILogger
    {
        public void Log(string text)
        {
            Console.WriteLine(text);
        }
    }
}
