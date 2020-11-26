using AppDomain.Cameras;
using AppDomain.Services;
using Prism.Commands;
using Prism.Mvvm;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfApp.Utils;

namespace WpfApp.ViewModels
{
    public class CameraViewModel : BindableBase
    {
        private readonly CameraProvider cameraProvider;
        private readonly SnapshotsStorage snapshotsStorage;

        private string cameraState;
        private BitmapImage image;

        private DelegateCommand startVideoCommand;
        private DelegateCommand stopVideoCommand;
        private DelegateCommand takeSnapshotCommand;

        public string CameraState
        {
            get => cameraState;
            set => SetProperty(ref cameraState, value);
        }

        public BitmapImage Image
        {
            get => image;
            set => SetProperty(ref image, value);
        }

        public CameraBasler Camera => cameraProvider.ConnectedCamera;

        public DelegateCommand StartVideoCommand
        {
            get
            {
                if (startVideoCommand == null)
                {
                    startVideoCommand = new DelegateCommand(StartVideo, () => Camera != null && !Camera.IsGrabbing);
                    startVideoCommand.ObservesProperty(() => Camera);
                    startVideoCommand.ObservesProperty(() => Camera.IsGrabbing);
                }

                return startVideoCommand;
            }
        }

        public DelegateCommand StopVideoCommand
        {
            get
            {
                if (stopVideoCommand == null)
                {
                    stopVideoCommand = new DelegateCommand(StopVideo, () => Camera != null && Camera.IsGrabbing);
                    stopVideoCommand.ObservesProperty(() => Camera);
                    stopVideoCommand.ObservesProperty(() => Camera.IsGrabbing);
                }

                return stopVideoCommand;
            }
        }

        public DelegateCommand TakeSnapshotCommand
        {
            get
            {
                if (takeSnapshotCommand == null)
                {
                    takeSnapshotCommand = new DelegateCommand(TakeSnapshot, () => Camera != null && Camera.IsGrabbing);
                    takeSnapshotCommand.ObservesProperty(() => Camera);
                    takeSnapshotCommand.ObservesProperty(() => Camera.IsGrabbing);
                }

                return takeSnapshotCommand;
            }
        }

        public CameraViewModel(CameraProvider cameraProvider)
        {
            snapshotsStorage = new SnapshotsStorage();
            this.cameraProvider = cameraProvider;
            this.cameraProvider.CameraFailed += CameraProvider_CameraFailed;
            this.cameraProvider.CameraConnected += CameraProvider_CameraConnected;
            this.cameraProvider.CameraDisconnected += CameraProvider_CameraDisconnected;
        }

        private void StartVideo()
        {
            Camera.StartGrabbing();
        }

        private void StopVideo()
        {
            Camera.StopGrabbing();
        }

        private void TakeSnapshot()
        {
            var pixels = Camera.TakeSnapshot();
            snapshotsStorage.Save(pixels);
        }

        private void CameraProvider_CameraConnected(object sender, System.EventArgs e)
        {
            CameraState = string.Empty;
            RaisePropertyChanged(nameof(Camera));
            Camera.ImageGrabbed += Camera_ImageGrabbed;
        }

        private void CameraProvider_CameraFailed(object sender, AppDomain.Events.CameraExceptionEventArgs e)
        {
            CameraState = e.Message;
        }

        private void CameraProvider_CameraDisconnected(object sender, System.EventArgs e)
        {
            RaisePropertyChanged(nameof(Camera));
        }

        private void Camera_ImageGrabbed(object sender, AppDomain.Events.ImageGrabbedEvent e)
        {
            var bitmapImage = ImageUtils.Convert(e.Image);
            bitmapImage.Freeze();
            Dispatcher.CurrentDispatcher.Invoke(() => Image = bitmapImage);
        }
    }
}