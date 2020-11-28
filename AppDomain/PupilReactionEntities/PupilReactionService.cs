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
        private PupilReaction behavior;

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

            if (behavior != null)
            {
                return Result.Failure("Previous operation was not finished.");
            }

            try
            {
                if (!cameraProvider.ConnectedCamera.IsGrabbing)
                {
                    cameraProvider.ConnectedCamera.StartGrabbing();
                }

                behavior = model;
                behavior.Init();
                OnBrightChanged(behavior.CurrentBright);

                portProvider.WriteCommand("#LEDAON");
                portProvider.WriteCommand($"#PWMB{behavior.StartingBrightLevel}");

                if (isAutoMode)
                {
                    timer = new Timer(state =>
                    {
                        if (behavior.CurrentBright == byte.MaxValue)
                        {
                            Stop();
                            return;
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
            snapshotStorage.Save(behavior.StartingBrightLevel, behavior.CurrentBright);
            portProvider.WriteCommand("#LEDBOFF");
            behavior = null;
            OnStopped();
        }

        public void IncreaseBright()
        {
            var currentBright = behavior.IncreaseBright();
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
                PWM = behavior.CurrentBright
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