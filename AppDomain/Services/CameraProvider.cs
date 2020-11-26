using AppDomain.Cameras;
using AppDomain.Events;
using Basler.Pylon;
using System;
using System.Linq;
using System.Threading;

namespace AppDomain.Services
{
    public class CameraProvider
    {
        private Timer timer;

        public event EventHandler<CameraExceptionEventArgs> CameraFailed;
        public event EventHandler<EventArgs> CameraConnected;
        public event EventHandler<EventArgs> CameraDisconnected;

        public CameraBasler ConnectedCamera { get; private set; }

        public CameraProvider()
        {
            StartPortsMonitoring();
        }

        private void StartPortsMonitoring()
        {
            timer = new Timer(Monitor, null, 0, 3000);
        }

        private void Monitor(object state)
        {
            var availableCameras = CameraFinder.Enumerate();
            if (availableCameras.Any() && ConnectedCamera == null)
            {
                try
                {
                    var camera = new Camera();
                    camera.Open();
                    ConnectedCamera = new CameraBasler(camera);
                    ConnectedCamera.Camera.ConnectionLost += Camera_ConnectionLost;
                    OnCameraConnected();
                    timer.Dispose();
                }
                catch (Exception)
                {
                    OnCameraFailed("The camera is in use by another application. Close the other application to start recording");
                }
            }
            else
            {
                OnCameraFailed("Camera not found");
            }
        }

        private void OnCameraFailed(string message)
        {
            CameraFailed?.Invoke(this, new CameraExceptionEventArgs(message));
        }

        private void OnCameraConnected()
        {
            CameraConnected?.Invoke(this, EventArgs.Empty);
        }

        private void Camera_ConnectionLost(object sender, EventArgs e)
        {
            CameraDisconnected?.Invoke(this, EventArgs.Empty);

            ConnectedCamera = null;
            StartPortsMonitoring();
        }
    }
}