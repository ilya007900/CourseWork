using AppDomain.Entities;
using AppDomain.Events;
using AppDomain.ExtensionMethods;
using AppDomain.Utils;
using Basler.Pylon;
using System;
using System.Collections.Generic;
using System.Threading;

namespace AppDomain.Cameras
{
    public class CameraBasler : NotifyPropertyChanged
    {
        private readonly ManualResetEvent imageWaitEvent = new ManualResetEvent(false);

        private PixelDataConverter converter = new PixelDataConverter();

        private bool needSnapshot = false;
        private IGrabResult lastGrabResult;

        public ICamera Camera { get; }

        public event EventHandler<ImageGrabbedEvent> ImageGrabbed;

        public bool IsOpen => Camera.IsOpen;

        public string Name => Camera.CameraInfo.GetName();

        public double ExposureTimeMin => Camera.GetExposureTimeMin();

        public double ExposureTimeMax => Camera.GetExposureTimeMax();

        public double ExposureTime
        {
            get => Camera.GetExposureTime();
            set
            {
                var result = Camera.SetExposureTime(value);
                if (!result.HasErrors)
                {
                    OnPropertyChanged();
                }
            }
        }

        public bool ExposureAuto
        {
            get => Camera.GetExposureAuto();
            set
            {
                Camera.SetExposureAuto(value);
                OnPropertyChanged();
            }
        }

        public double GainMin => Camera.GetGainMin();

        public double GainMax => Camera.GetGainMax();

        public double Gain
        {
            get => Camera.GetGain();
            set
            {
                var result = Camera.SetGain(value);
                if (!result.HasErrors)
                {
                    OnPropertyChanged();
                }
            }
        }

        public bool GainAuto
        {
            get => Camera.GetGainAuto();
            set
            {
                Camera.SetGainAuto(value);
                OnPropertyChanged();
            }
        }

        public IEnumerable<string> PixelFormats => Camera.GetPixelFormats();

        public string PixelFormat
        {
            get => Camera.GetPixelFormat();
            set
            {
                Camera.SetPixelFormat(value);
                converter = new PixelDataConverter();
            }
        }

        public double FrameRate => Camera.GetFrameRate();

        public bool IsGrabbing => Camera.StreamGrabber.IsGrabbing;

        public CameraBasler(ICamera camera)
        {
            Camera = camera;
        }

        public void StartGrabbing()
        {
            if (IsGrabbing)
            {
                return;
            }

            Camera.StreamGrabber.ImageGrabbed += OnImageGrabbed;
            Camera.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
            OnPropertyChanged(nameof(IsGrabbing));
        }

        public void StopGrabbing()
        {
            if (!IsGrabbing)
            {
                return;
            }

            Camera.StreamGrabber.ImageGrabbed -= OnImageGrabbed;
            Camera.StreamGrabber.Stop();
            OnPropertyChanged(nameof(IsGrabbing));
        }

        public byte[] TakeSnapshot()
        {
            if (!IsGrabbing)
            {
                throw new Exception("Can't take snapshot. Camera isn't grabbing");
            }

            needSnapshot = true;
            imageWaitEvent.WaitOne();
            imageWaitEvent.Reset();

            return converter.ConvertToBytes(lastGrabResult);
        }

        private void OnImageGrabbed(object sender, ImageGrabbedEventArgs e)
        {
            OnPropertyChanged(nameof(FrameRate));
            if (ExposureAuto)
            {
                OnPropertyChanged(nameof(ExposureTime));
            }

            if (GainAuto)
            {
                OnPropertyChanged(nameof(Gain));
            }

            if (needSnapshot)
            {
                lastGrabResult = e.GrabResult.Clone();
                needSnapshot = false;
                imageWaitEvent.Set();
            }
            else
            {
                var bitmap = converter.Convert(e.GrabResult);
                ImageGrabbed?.Invoke(sender, new ImageGrabbedEvent(bitmap));
            }
        }
    }
}