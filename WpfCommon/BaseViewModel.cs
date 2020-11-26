using Prism.Mvvm;

namespace WpfCommon
{
    public class BaseViewModel : BindableBase
    {
        private string title;
        private bool enabled;

        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        public bool Enabled
        {
            get => enabled;
            set => SetProperty(ref enabled, value);
        }
    }
}