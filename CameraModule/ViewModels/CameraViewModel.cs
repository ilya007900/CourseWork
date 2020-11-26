using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AppDomain.Cameras;
using AppDomain.Services;
using AppDomain.Utils;
using CameraModule.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace CameraModule.ViewModels
{
    public class CameraViewModel : BindableBase
    {
        private readonly CameraService cameraService;
        private readonly IDialogService dialogService;

        private BitmapImage image;

        public CameraBasler Camera => cameraService.ConnectedCamera;

        public BitmapImage Image
        {
            get => image;
            set => SetProperty(ref image, value);
        }

        public DelegateCommand OpenCameraSelectionCommand => new DelegateCommand(OpenCameraSelection);

        public DelegateCommand StartVideoCommand => new DelegateCommand(StartVideo);

        public DelegateCommand StopVideoCommand => new DelegateCommand(StopVideo);

        public CameraViewModel(CameraService cameraService, IDialogService dialogService)
        {
            this.cameraService = cameraService;
            this.dialogService = dialogService;
            this.cameraService.CameraConnected += CameraService_CameraConnected;
        }

        private void OpenCameraSelection()
        {
            dialogService.ShowDialog(nameof(CameraSelectionView));
        }

        private void StartVideo()
        {
            Camera.StartGrabbing();
        }

        private void StopVideo()
        {
            Camera.StopGrabbing();
        }

        private void CameraService_CameraConnected(object sender, AppDomain.Events.CameraConnectedEvent e)
        {
            Camera.ImageGrabbed += Camera_ImageGrabbed;
            RaisePropertyChanged(nameof(Camera));
        }

        private void Camera_ImageGrabbed(object sender, AppDomain.Events.ImageGrabbedEvent e)
        {
            var bitmap = ImageUtils.Convert(e.GrabResult);
            var bitmapImage = ImageUtils.Convert(bitmap);
            bitmapImage.Freeze();
            Dispatcher.CurrentDispatcher.Invoke(() => Image = bitmapImage);
        }
    }
}