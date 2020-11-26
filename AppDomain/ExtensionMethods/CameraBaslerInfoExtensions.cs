using Basler.Pylon;

namespace AppDomain.ExtensionMethods
{
    public static class CameraBaslerInfoExtensions
    {
        public static string GetName(this ICameraInfo cameraInfo)
        {
            return cameraInfo["FriendlyName"];
        }
    }
}