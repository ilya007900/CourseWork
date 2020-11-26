using AppDomain.Services;
using ArduinoModule.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace ArduinoModule
{
    public class ArduinoModuleConfig : IModule
    {
        private readonly IRegionManager regionManager;

        public ArduinoModuleConfig(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<ArduinoService>();

            containerRegistry.Register<object, ArduinoView>(nameof(ArduinoView));

            regionManager.RequestNavigate("TabControlRegion", nameof(ArduinoView));
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}