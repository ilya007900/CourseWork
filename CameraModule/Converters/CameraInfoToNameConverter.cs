using System;
using System.Globalization;
using System.Windows.Data;
using AppDomain.ExtensionMethods;
using Basler.Pylon;

namespace CameraModule.Converters
{
    public class CameraInfoToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ICameraInfo cameraInfo)
            {
                return cameraInfo.GetName();
            }

            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Can not convert to camera info");
        }
    }
}