using AppDomain.Services;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Windows;

namespace WpfApp.ViewModels
{
    public class PortViewModel : BindableBase
    {
        private readonly PortProvider portProvider;

        private string selectedPort;

        private DelegateCommand<string> connectCommand;
        private DelegateCommand disconnectCommand;
        private DelegateCommand refreshCommand;
        private DelegateCommand<string> executeCommand;

        public IReadOnlyList<string> AvailablePorts => portProvider.GetAvailablePorts();

        public IReadOnlyList<string> PredefinedCommands => new List<string>
        {
            "#STAR",
            "#LEDAON",
            "#LEDBON"
        };

        public ObservableCollection<string> SentCommands { get; } = new ObservableCollection<string>();

        public ObservableCollection<string> ReceivedData { get; } = new ObservableCollection<string>();

        public string SelectedPort
        {
            get => selectedPort;
            set => SetProperty(ref selectedPort, value);
        }

        public SerialPort Port => portProvider.ConnectedPort;

        public DelegateCommand<string> ConnectCommand
        {
            get
            {
                if (connectCommand == null)
                {
                    connectCommand = new DelegateCommand<string>(portProvider.Connect, port => !string.IsNullOrEmpty(port) && Port == null);
                    connectCommand.ObservesProperty(() => SelectedPort);
                    connectCommand.ObservesProperty(() => Port);
                }

                return connectCommand;
            }
        }

        public DelegateCommand DisconnectCommand
        {
            get
            {
                if (disconnectCommand == null)
                {
                    disconnectCommand = new DelegateCommand(portProvider.Disconnect, () => Port != null);
                    disconnectCommand.ObservesProperty(() => Port);
                }

                return disconnectCommand;
            }
        }

        public DelegateCommand RefreshCommand
        {
            get
            {
                if (refreshCommand == null)
                {
                    refreshCommand = new DelegateCommand(() => RaisePropertyChanged(nameof(AvailablePorts)));
                }

                return refreshCommand;
            }
        }

        public DelegateCommand<string> ExecuteCommand
        {
            get
            {
                if (executeCommand == null)
                {
                    executeCommand = new DelegateCommand<string>(portProvider.WriteCommand, c => !string.IsNullOrEmpty(c));
                }

                return executeCommand;
            }
        }

        public PortViewModel(PortProvider portProvider)
        {
            this.portProvider = portProvider;

            this.portProvider.PortConnected += PortProvider_PortConnected;
            this.portProvider.PortDisconnected += PortProvider_PortDisconnected;
            this.portProvider.CommandSent += PortProvider_CommandSent;
        }

        private void PortProvider_PortConnected(object sender, System.EventArgs e)
        {
            RaisePropertyChanged(nameof(Port));
            Port.DataReceived += Port_DataReceived;
        }

        private void PortProvider_PortDisconnected(object sender, System.EventArgs e)
        {
            RaisePropertyChanged(nameof(Port));
            Port.DataReceived -= Port_DataReceived;
        }

        private void PortProvider_CommandSent(object sender, AppDomain.Events.PortCommandSentEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => SentCommands.Add(e.Command));
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => ReceivedData.Add(e.ToString()));
        }
    }
}