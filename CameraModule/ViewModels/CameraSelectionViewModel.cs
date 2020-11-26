using System;
using AppDomain.Services;
using Basler.Pylon;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Linq;
using AppDomain.ExtensionMethods;
using Prism.Services.Dialogs;

namespace CameraModule.ViewModels
{
    public class CameraSelectionViewModel : BindableBase, IDialogAware
    {
        private readonly CameraService cameraService;

        private IReadOnlyList<ICameraInfo> infos;
        private ICameraInfo selectedInfo;

        public event Action<IDialogResult> RequestClose;

        public string Title { get; } = "Camera Selection";

        public IReadOnlyList<ICameraInfo> Infos
        {
            get => infos;
            set => SetProperty(ref infos, value);
        }

        public ICameraInfo SelectedInfo
        {
            get => selectedInfo;
            set
            {
                SetProperty(ref selectedInfo, value);
                cameraService.UpdateCamera(value);
            }
        }

        public DelegateCommand RefreshCommand => new DelegateCommand(Refresh);

        public DelegateCommand ConnectCommand=>new DelegateCommand(Connect);

        public CameraSelectionViewModel(CameraService cameraService)
        {
            this.cameraService = cameraService;
        }

        private void Refresh()
        {
            Infos = cameraService.GetAvailableCameras();
        }

        private void Connect()
        {
            cameraService.Connect();
            RequestClose?.Invoke(new DialogResult());
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
            Refresh();

            var selectedCameraName = cameraService.ConnectedCamera?.Camera.CameraInfo.GetName();
            if (selectedCameraName != null)
            {
                SelectedInfo = Infos.FirstOrDefault(x => x.GetName() == selectedCameraName);
            }
        }
    }
}