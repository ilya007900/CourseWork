using AppDomain.Events;
using System;
using System.Collections.Generic;
using System.IO.Ports;

namespace AppDomain.Services
{
    public class PortProvider
    {
        public SerialPort ConnectedPort { get; private set; }

        public IReadOnlyList<string> GetAvailablePorts() => SerialPort.GetPortNames();

        public event EventHandler PortConnected;
        public event EventHandler PortDisconnected;
        public event EventHandler<PortCommandSentEventArgs> CommandSent;

        public void Connect(string portName)
        {
            ConnectedPort = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One);
            ConnectedPort.Open();
            ConnectedPort.DtrEnable = true;
            OnPortConnected();
        }

        public void Disconnect()
        {
            ConnectedPort.Close();
            ConnectedPort = null;
            OnPortDisconnected();
        }

        public void WriteCommand(string command)
        {
            OnCommandSent(command);
            ConnectedPort.WriteLine(command);
        }

        private void OnPortConnected()
        {
            PortConnected?.Invoke(this, EventArgs.Empty);
        }

        private void OnPortDisconnected()
        {
            PortDisconnected?.Invoke(this, EventArgs.Empty);
        }

        private void OnCommandSent(string command)
        {
            CommandSent?.Invoke(this, new PortCommandSentEventArgs(command));
        }
    }
}