using AppDomain.Services;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Unity;
using System.Windows;
using WpfApp.ViewModels;
using WpfApp.Views;

namespace WpfApp
{
    public partial class App : PrismApplication
    {
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton(typeof(CameraProvider));
            containerRegistry.RegisterSingleton(typeof(PortProvider));

            ViewModelLocationProvider.Register<ShellWindow, MainViewModel>();
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<ShellWindow>();
        }
    }
}
