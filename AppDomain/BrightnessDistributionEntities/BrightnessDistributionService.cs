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

        public Result RunTauTuning(int number, int tau)
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

                if (!cameraProvider.ConnectedCamera.IsGrabbing)
                {
                    cameraProvider.ConnectedCamera.StartGrabbing();
                }

                cameraProvider.ConnectedCamera.ExposureTime = tau;
                portProvider.WriteCommand($"#LED{number}ON");
                Thread.Sleep(1000);
                portProvider.WriteCommand($"#LED{number}OFF");
                return Result.Success();
            }
            catch (Exception e)
            {
                return Result.Failure(e.Message);
            }
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

                if (!cameraProvider.ConnectedCamera.IsGrabbing)
                {
                    cameraProvider.ConnectedCamera.StartGrabbing();
                }

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
            var snapshotsCount = 4;

            if (diodeBehavior.Step != 0)
            {
                portProvider.WriteCommand("#ENBLON");
                portProvider.WriteCommand($"{diodeBehavior.Step}");
                manualResetEvent.WaitOne();
                manualResetEvent.Reset();
                portProvider.WriteCommand("#ENBLOFF");
            }

            portProvider.WriteCommand("#LEDAOFF");

            portProvider.WriteCommand($"#LED{diodeBehavior.Diode.Number}ON");
            Thread.Sleep(1);
            TakeSnapshots(snapshotsCount, (int)diodeBehavior.Tau, diodeBehavior.Diode.MaxEnergy);

            portProvider.WriteCommand($"#LED{diodeBehavior.Diode.Number}OFF");
            Thread.Sleep(1);
            TakeSnapshots(snapshotsCount, (int)diodeBehavior.Tau, diodeBehavior.Diode.MaxEnergy);

            portProvider.WriteCommand($"#LED{diodeBehavior.Diode.Number}ON");
            Thread.Sleep(1);
            var km1Tau = (int)diodeBehavior.CalculateKm1Tau();
            TakeSnapshots(snapshotsCount, km1Tau, diodeBehavior.Diode.MaxEnergy);

            portProvider.WriteCommand($"#LED{diodeBehavior.Diode.Number}OFF");
            Thread.Sleep(1);
            TakeSnapshots(snapshotsCount, (int)diodeBehavior.Tau, diodeBehavior.Diode.MaxEnergy);

            portProvider.WriteCommand($"#LED{diodeBehavior.Diode.Number}ON");
            var km2Tau = (int)diodeBehavior.CalculateKm2Tau();
            TakeSnapshots(snapshotsCount, km2Tau, diodeBehavior.Diode.Number);

            Thread.Sleep(1);
            TakeSnapshots(snapshotsCount, (int)diodeBehavior.Tau, diodeBehavior.Diode.MaxEnergy);
            portProvider.WriteCommand($"#LED{diodeBehavior.Diode.Number}OFF");
        }

        private void TakeSnapshots(int count, int tau, int energy)
        {
            for (var i = 0; i < count; i++)
            {
                cameraProvider.ConnectedCamera.ExposureTime = tau;
                snapshotStorage.Add(TakeSnapshot(energy));
            }
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