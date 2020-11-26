using AppDomain.Services;
using CameraModule.ViewModels;
using CameraModule.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace CameraModule
{
    public class CameraModuleConfig : IModule
    {
        private readonly IRegionManager regionManager;

        public CameraModuleConfig(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<CameraService>();

            regionManager.RegisterViewWithRegion("CameraRegion", typeof(CameraView));

            containerRegistry.RegisterDialog<CameraSelectionView, CameraSelectionViewModel>();
        }
    }
}