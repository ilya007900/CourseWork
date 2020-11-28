using AppDomain.PupilReactionEntities;
using Prism.Commands;
using Prism.Mvvm;
using System.Windows;

namespace WpfApp.ViewModels
{
    public class PupilReactionViewModel : BindableBase
    {
        private readonly PupilReactionService service;

        private byte startingBrightLevel;
        private byte brightIncreaseCoefficient;
        private ushort currentBright;
        private bool inProgress;
        private bool isAutoMode;
        private string state;

        private DelegateCommand startCommand;
        private DelegateCommand stopCommand;
        private DelegateCommand increaseBrightCommand;

        public byte StartingBrightLevel
        {
            get => startingBrightLevel;
            set => SetProperty(ref startingBrightLevel, value);
        }

        public byte BrightIncreaseCoefficient
        {
            get => brightIncreaseCoefficient;
            set => SetProperty(ref brightIncreaseCoefficient, value);
        }

        public ushort CurrentBright
        {
            get => currentBright;
            set => SetProperty(ref currentBright, value);
        }

        public bool IsAutoMode
        {
            get => isAutoMode;
            set => SetProperty(ref isAutoMode, value);
        }

        public bool InProgress
        {
            get => inProgress;
            set => SetProperty(ref inProgress, value);
        }

        public string State
        {
            get => state;
            set => SetProperty(ref state, value);
        }

        public DelegateCommand StartCommand
        {
            get
            {
                if (startCommand == null)
                {
                    startCommand = new DelegateCommand(Start, () => !InProgress);
                    startCommand.ObservesProperty(() => InProgress);
                }

                return startCommand;
            }
        }

        public DelegateCommand StopCommand
        {
            get
            {
                if (stopCommand == null)
                {
                    stopCommand = new DelegateCommand(Stop, () => InProgress);
                    stopCommand.ObservesProperty(() => InProgress);
                }

                return stopCommand;
            }
        }

        public DelegateCommand IncreaseBrightCommand
        {
            get
            {
                if (increaseBrightCommand == null)
                {
                    increaseBrightCommand = new DelegateCommand(IncreaseBright, () => InProgress && !IsAutoMode);
                    increaseBrightCommand.ObservesProperty(() => InProgress);
                    increaseBrightCommand.ObservesProperty(() => IsAutoMode);
                }

                return increaseBrightCommand;
            }
        }

        public PupilReactionViewModel(PupilReactionService service)
        {
            this.service = service;
        }

        private void Start()
        {
            InProgress = true;
            State = "In Progress";

            service.Stopped += Service_Stopped;
            service.BrightChanged += Service_BrightChanged;

            var model = new PupilReaction(StartingBrightLevel, BrightIncreaseCoefficient);
            var result = service.Run(model, IsAutoMode);
            if (result.HasErrors)
            {
                MessageBox.Show(result.ErrorMessage);
                InProgress = false;
                State = string.Empty;
            }
        }

        private void Stop()
        {
            service.Stop();
            OnServiceStopped();
        }

        private void IncreaseBright()
        {
            service.Snapshot();
            service.IncreaseBright();
        }

        private void OnServiceStopped()
        {
            service.BrightChanged -= Service_BrightChanged;
            InProgress = false;
            State = "Finished";
        }

        private void Service_Stopped(object sender, System.EventArgs e)
        {
            OnServiceStopped();
        }

        private void Service_BrightChanged(object sender, BrightChangedEventArgs e)
        {
            CurrentBright = e.Bright;
        }
    }
}