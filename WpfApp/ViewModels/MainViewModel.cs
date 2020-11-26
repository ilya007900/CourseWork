using Prism.Mvvm;

namespace WpfApp.ViewModels
{
    public class MainViewModel : BindableBase
    {
        public CameraViewModel CameraViewModel { get; }

        public PortViewModel PortViewModel { get; }

        public PupilReactionViewModel PupilReactionViewModel { get; }

        public BrightnessDistributionViewModel BrightnessDistributionViewModel { get; }

        public MainViewModel(
            CameraViewModel cameraViewModel, 
            PortViewModel portViewModel,
            PupilReactionViewModel pupilReactionViewModel,
            BrightnessDistributionViewModel brightnessDistributionViewModel)
        {
            CameraViewModel = cameraViewModel;
            PortViewModel = portViewModel;
            PupilReactionViewModel = pupilReactionViewModel;
            BrightnessDistributionViewModel = brightnessDistributionViewModel;
        }
    }
}