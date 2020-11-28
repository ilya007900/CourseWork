using System;
using System.Threading;
using AppDomain.FunctionalExtensions;
using AppDomain.Services;

namespace AppDomain.PupilReactionEntities
{
    public class PupilReactionService
    {
        private readonly CameraProvider cameraProvider;
        private readonly PortProvider portProvider;
        private readonly PupilReactionSnapshotStorage snapshotStorage = new PupilReactionSnapshotStorage();

        private Timer timer;
        private PupilReaction model;

        public event EventHandler<BrightChangedEventArgs> BrightChanged;
        public event EventHandler<EventArgs> Stopped; 

        public PupilReactionService(CameraProvider cameraProvider, PortProvider portProvider)
        {
            this.cameraProvider = cameraProvider;
            this.portProvider = portProvider;
        }

        public Result Run(PupilReaction model, bool isAutoMode)
        {
            if (cameraProvider.ConnectedCamera == null)
            {
                return Result.Failure("Camera not found. Please connect camera.");
            }

            if (portProvider.ConnectedPort == null)
            {
                return Result.Failure("Port not found. Please connect port");
            }

            if (this.model != null)
            {
                return Result.Failure("Previous operation was not finished.");
            }

            try
            {
                this.model = model;
                this.model.Init();
                OnBrightChanged(this.model.CurrentBright);

                portProvider.WriteCommand("#LEDAON");
                portProvider.WriteCommand($"#PWMB{this.model.StartingBrightLevel}");

                if (isAutoMode)
                {
                    timer = new Timer(state =>
                    {
                        if (this.model.CurrentBright == byte.MaxValue)
                        {
                            Stop();
                        }

                        IncreaseBright();
                        Snapshot();
                    }, null, 2000, 2000);
                }
            }
            catch (Exception e)
            {
                return Result.Failure(e.Message);
            }

            return Result.Success();
        }

        public void Stop()
        {
            timer?.Dispose();
            snapshotStorage.Save(model.StartingBrightLevel, model.CurrentBright);
            portProvider.WriteCommand("#LEDAOFF");
            portProvider.WriteCommand("#LEDBOFF");
            model = null;
            OnStopped();
        }

        public void IncreaseBright()
        {
            var currentBright = model.IncreaseBright();
            portProvider.WriteCommand("#PWMB" + currentBright);
            OnBrightChanged(currentBright);
        }

        public void Snapshot()
        {
            var bytes = cameraProvider.ConnectedCamera.TakeSnapshot();
            var snapshot = new PupilReactionSnapshot
            {
                Image = bytes,
                DateTime = DateTime.Now,
                ExposureTime = cameraProvider.ConnectedCamera.ExposureTime,
                Gain = cameraProvider.ConnectedCamera.Gain,
                PixelFormat = cameraProvider.ConnectedCamera.PixelFormat,
                PWM = model.CurrentBright
            };

            snapshotStorage.Add(snapshot);
        }

        private void OnBrightChanged(ushort bright)
        {
            BrightChanged?.Invoke(this, new BrightChangedEventArgs(bright));
        }

        private void OnStopped()
        {
            Stopped?.Invoke(this, EventArgs.Empty);
        }
    }
}