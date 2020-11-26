using System;

namespace AppDomain.Events
{
    public class PortCommandSentEventArgs : EventArgs
    {
        public string Command { get; }

        public PortCommandSentEventArgs(string command)
        {
            Command = command;
        }
    }
}