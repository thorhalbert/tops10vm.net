using System;

namespace Monitor.Subsystems.Console
{
    public class ConsoleOutputEvent : EventArgs
    {
        public string Output { get; private set; }

        public ConsoleOutputEvent(string output)
        {
            Output = output;
        }
    }
}