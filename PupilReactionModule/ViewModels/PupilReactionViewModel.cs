using AppDomain.PupilReactionEntities;
using AppDomain.Services;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Threading;
using WpfCommon;

namespace PupilReactionModule.ViewModels
{
    public class PupilReactionViewModel : BaseViewModel
    {
        private readonly CameraProvider cameraProvider;
        private readonly PortProvider portProvider;
        private readonly PupilReactionSnapshotStorage snapshotStorage;

        private readonly List<PupilReactionSnapshot> snapshots = new List<PupilReactionSnapshot>();

        private byte startingBrightLevel;
        private byte brightIncreaseCoefficient;
        private bool inProgress;
        private bool isAutoMode;
        private string state;

        public PupilReaction Model { get; private set; }

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

        public DelegateCommand StartCommand => new DelegateCommand(Start);

        public DelegateCommand StopCommand => new DelegateCommand(Stop);

        public DelegateCommand IncreaseBrightCommand => new DelegateCommand(IncreaseBright);

        public PupilReactionViewModel(CameraProvider cameraProvider, PortProvider portProvider)
        {
            this.cameraProvider = cameraProvider;
            this.portProvider = portProvider;

            this.cameraProvider.CameraConnected += CameraService_CameraConnected;

            snapshotStorage = new PupilReactionSnapshotStorage();

            Title = "Pupil Reaction";
            Enabled = cameraProvider.ConnectedCamera != null;// && arduinoService.ConnectedPort != null;
        }

        private void Start()
        {
            InProgress = true;
            State = "InProgress";

            Model = new PupilReaction(StartingBrightLevel, BrightIncreaseCoefficient);
            Model.Start();
            OnModelUpdated();

            cameraProvider.ConnectedCamera.StartGrabbing();

            //var result = arduinoService.WriteCommand("#LEDAON");
            //if (result.HasErrors)
            //{
            //    MessageBox.Show(result.ErrorMessage);
            //    return;
            //}

            //result = arduinoService.WriteCommand($"#PWMB{Model.StartingBrightLevel}");
            //if (result.HasErrors)
            //{
            //    MessageBox.Show(result.ErrorMessage);
            //}

            if (!IsAutoMode)
            {
                return;
            }

            var thread = new Thread(() =>
            {
                while (InProgress)
                {
                    Thread.Sleep(2000);
                    if (InProgress)
                    {
                        IncreaseBright();
                    }
                    else
                    {
                        break;
                    }
                }
            });

            thread.Start();
        }

        private void Stop()
        {
            //var result = arduinoService.WriteCommand("#LEDAOFF");
            //if (result.HasErrors)
            //{
            //    MessageBox.Show(result.ErrorMessage);
            //    return;
            //}

            //result = arduinoService.WriteCommand("#LEDBOFF");
            //if (result.HasErrors)
            //{
            //    MessageBox.Show(result.ErrorMessage);
            //    return;
            //}

            InProgress = false;
            cameraProvider.ConnectedCamera.StopGrabbing();
            State = "Finished";

            //snapshotStorage.Save(snapshots, Model.StartingBrightLevel, Model.CurrentBright);
        }

        private void IncreaseBright()
        {
            Model.IncreaseBright();
            OnModelUpdated();

            Snapshot();

            //var result = arduinoService.WriteCommand("#PWMB" + Model.CurrentBright);
            //if (result.HasErrors)
            //{
            //    MessageBox.Show(result.ErrorMessage);
            //}
        }

        public void Snapshot()
        {
            var bytes = cameraProvider.ConnectedCamera.Snapshot();
            var snapshot = new PupilReactionSnapshot
            {
                Image = bytes,
                DateTime = DateTime.Now,
                ExposureTime = cameraProvider.ConnectedCamera.ExposureTime,
                Gain = cameraProvider.ConnectedCamera.Gain,
                PixelFormat = cameraProvider.ConnectedCamera.PixelFormat,
                PWM = Model.CurrentBright
            };

            snapshots.Add(snapshot);
        }

        private void OnModelUpdated()
        {
            RaisePropertyChanged(nameof(Model));
        }

        private void CameraService_CameraConnected(object sender, EventArgs e)
        {
            Enabled = cameraProvider.ConnectedCamera != null;
        }
    }
}