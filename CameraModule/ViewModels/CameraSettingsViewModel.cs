using AppDomain.Cameras;
using AppDomain.Events;
using AppDomain.Services;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;

namespace CameraModule.ViewModels
{
    public class CameraSettingsViewModel : BindableBase, IDialogAware
    {
        private readonly CameraService cameraService;

        private CameraBasler camera;

        public event Action<IDialogResult> RequestClose;

        public string Title { get; } = "Camera Settings";

        public CameraBasler Camera
        {
            get => camera;
            private set => SetProperty(ref camera, value);
        }

        public CameraSettingsViewModel(CameraService cameraService)
        {
            this.cameraService = cameraService;
            this.cameraService.CameraConnected += OnCameraConnected;
        }

        private void OnCameraConnected(object sender, CameraConnectedEvent args)
        {
            Camera = cameraService.ConnectedCamera;
            RaisePropertyChanged(nameof(Camera));
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            Camera = cameraService.ConnectedCamera;
        }
    }
}