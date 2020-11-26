using Microsoft.Win32;

namespace WpfApp.Services
{
    public class FileDialogService
    {
        public string FilePath { get; private set; }

        public bool OpenFileDialog(string filter)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = filter
            };

            if (openFileDialog.ShowDialog() == true)
            {
                FilePath = openFileDialog.FileName;
                return true;
            }

            return false;
        }

        public bool SaveFileDialog(string filter)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = filter
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                FilePath = saveFileDialog.FileName;
                return true;
            }

            return false;
        }
    }
}