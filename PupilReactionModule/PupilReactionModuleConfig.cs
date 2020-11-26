using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using PupilReactionModule.Views;

namespace PupilReactionModule
{
    public class PupilReactionModuleConfig : IModule
    {
        private readonly IRegionManager regionManager;

        public PupilReactionModuleConfig(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<object, PupilReactionView>(nameof(PupilReactionView));

            regionManager.RequestNavigate("TabControlRegion", nameof(PupilReactionView));
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}