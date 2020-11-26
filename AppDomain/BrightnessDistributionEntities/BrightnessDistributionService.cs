using AppDomain.FunctionalExtensions;
using AppDomain.Services;
using System;
using System.Collections.Generic;
using System.Threading;

namespace AppDomain.BrightnessDistributionEntities
{
    public class BrightnessDistributionService
    {
        private readonly CameraProvider cameraProvider;
        private readonly PortProvider portProvider;
        private readonly BrightnessDistributionSnapshotStorage snapshotStorage = new BrightnessDistributionSnapshotStorage();
        private readonly ManualResetEvent manualResetEvent = new ManualResetEvent(false);

        public event EventHandler<DiodeBehaviorExecutingEventArgs> DiodeBehaviorExecuting;
        public event EventHandler<DiodeBehaviorExecutedEventArgs> DiodeBehaviorExecuted;

        public BrightnessDistributionService(CameraProvider cameraProvider, PortProvider portProvider)
        {
            this.cameraProvider = cameraProvider;
            this.portProvider = portProvider;
        }

        public Result Run(IReadOnlyList<DiodeBehavior> diodeBehaviors)
        {
            if (cameraProvider.ConnectedCamera == null)
            {
                return Result.Failure("Camera not found. Please connect camera.");
            }

            if (portProvider.ConnectedPort == null)
            {
                return Result.Failure("Port not found. Please connect port.");
            }

            try
            {
                cameraProvider.ConnectedCamera.ExposureAuto = false;
                portProvider.ConnectedPort.DataReceived += ConnectedPort_DataReceived;

                foreach (var diodeBehavior in diodeBehaviors)
                {
                    if (cameraProvider.ConnectedCamera.ExposureAuto)
                    {
                        return Result.Failure("Exposure in auto mode");
                    }

                    try
                    {
                        OnDiodeBehaviorExecuting(diodeBehavior.Diode.Number);
                        ExecuteDiodeBehavior(diodeBehavior);
                    }
                    finally
                    {
                        OnDiodeBehaviorExecuted(diodeBehavior.Diode.Number);
                    }
                }

                snapshotStorage.Save();
            }
            catch (Exception e)
            {
                return Result.Failure(e.Message);
            }
            finally
            {
                portProvider.ConnectedPort.DataReceived -= ConnectedPort_DataReceived;
            }

            return Result.Success();
        }

        public BrightnessDistributionSnapshot TakeSnapshot(int energy)
        {
            return new BrightnessDistributionSnapshot
            {
                Image = cameraProvider.ConnectedCamera.TakeSnapshot(),
                ExposureTime = cameraProvider.ConnectedCamera.ExposureTime,
                PixelFormat = cameraProvider.ConnectedCamera.PixelFormat,
                Energy = energy,
                DateTime = DateTime.Now
            };
        }

        private void ExecuteDiodeBehavior(DiodeBehavior diodeBehavior)
        {
            cameraProvider.ConnectedCamera.ExposureTime = diodeBehavior.Tau;

            portProvider.WriteCommand("#ENBLON");
            portProvider.WriteCommand($"{diodeBehavior.Step}");

            manualResetEvent.WaitOne();
            manualResetEvent.Reset();

            portProvider.WriteCommand("#ENBLOFF");
            portProvider.WriteCommand("#LEDAOFF");

            snapshotStorage.Add(TakeSnapshot(diodeBehavior.Diode.MaxEnergy));

            portProvider.WriteCommand($"#LED{diodeBehavior.Diode.Number}ON");
            Thread.Sleep((int)diodeBehavior.Tau);
            snapshotStorage.Add(TakeSnapshot(diodeBehavior.Diode.MaxEnergy));

            var km1Tau = diodeBehavior.CalculateKm1Tau();
            cameraProvider.ConnectedCamera.ExposureTime = km1Tau;
            Thread.Sleep((int)km1Tau);
            snapshotStorage.Add(TakeSnapshot(diodeBehavior.Diode.MaxEnergy));

            var km2Tau = diodeBehavior.CalculateKm2Tau();
            cameraProvider.ConnectedCamera.ExposureTime = km2Tau;
            Thread.Sleep((int)km2Tau);
            snapshotStorage.Add(TakeSnapshot(diodeBehavior.Diode.MaxEnergy));

            portProvider.WriteCommand($"#LED{diodeBehavior.Diode.Number}OFF");
        }

        private void OnDiodeBehaviorExecuting(byte number)
        {
            DiodeBehaviorExecuting?.Invoke(this, new DiodeBehaviorExecutingEventArgs(number));
        }

        private void OnDiodeBehaviorExecuted(byte number)
        {
            DiodeBehaviorExecuted?.Invoke(this, new DiodeBehaviorExecutedEventArgs(number));
        }

        private void ConnectedPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            manualResetEvent.Set();
        }
    }
}