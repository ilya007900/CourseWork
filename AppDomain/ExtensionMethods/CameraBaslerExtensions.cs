using System.Collections.Generic;
using AppDomain.FunctionalExtensions;
using Basler.Pylon;

namespace AppDomain.ExtensionMethods
{
    public static class CameraBaslerExtensions
    {
        private const string ExposureAutoModeOn = "Continuous";
        private const string ExposureAutoModeOff = "Off";
        private const string GainAutoModeOn = "Continuous";
        private const string GainAutoModeOff = "Off";

        public static double GetExposureTimeMin(this ICamera camera)
        {
            return camera.Parameters[PLCamera.ExposureTime].GetMinimum();
        }

        public static double GetExposureTimeMax(this ICamera camera)
        {
            return camera.Parameters[PLCamera.ExposureTime].GetMaximum();
        }

        public static double GetExposureTime(this ICamera camera)
        {
            return camera.Parameters[PLCamera.ExposureTime].GetValue();
        }

        public static Result SetExposureTime(this ICamera camera, double value)
        {
            var exposureTimeMax = camera.GetExposureTimeMax();
            if (value > exposureTimeMax)
            {
                return Result.Failure($"Exposure time can't be grater {exposureTimeMax}");
            }

            var exposureTimeMin = camera.GetExposureTimeMin();
            if (value < exposureTimeMin)
            {
                return Result.Failure($"Exposure time can't be less {exposureTimeMin}");
            }

            if (camera.GetExposureAuto())
            {
                return Result.Failure("Exposure time can't be set if exposure auto mode on");
            }

            camera.Parameters[PLCamera.ExposureTime].SetValue(value);
            return Result.Success();
        }

        public static bool GetExposureAuto(this ICamera camera)
        {
            var currentMode = camera.Parameters[PLCamera.ExposureAuto].GetValue();
            return string.CompareOrdinal(currentMode, ExposureAutoModeOn) == 0;
        }

        public static void SetExposureAuto(this ICamera camera, bool value)
        {
            camera.Parameters[PLCamera.ExposureAuto].SetValue(value ? ExposureAutoModeOn : ExposureAutoModeOff);
        }

        public static double GetGainMin(this ICamera camera)
        {
            return camera.Parameters[PLCamera.Gain].GetMinimum();
        }

        public static double GetGainMax(this ICamera camera)
        {
            return camera.Parameters[PLCamera.Gain].GetMaximum();
        }

        public static double GetGain(this ICamera camera)
        {
            return camera.Parameters[PLCamera.Gain].GetValue();
        }

        public static Result SetGain(this ICamera camera, double value)
        {
            var gainMax = camera.GetGainMax();
            if (value > gainMax)
            {
                return Result.Failure($"Gain can't be grater {gainMax}");
            }

            var gainMin = camera.GetGainMin();
            if (value < gainMin)
            {
                return Result.Failure($"Gain can't be less {gainMin}");
            }

            if (camera.GetGainAuto())
            {
                return Result.Failure("Gain can't be set if gain auto mode on");
            }

            camera.Parameters[PLCamera.Gain].SetValue(value);
            return Result.Success();
        }

        public static bool GetGainAuto(this ICamera camera)
        {
            var currentMode = camera.Parameters[PLCamera.GainAuto].GetValue();
            return string.CompareOrdinal(currentMode, GainAutoModeOn) == 0;
        }

        public static void SetGainAuto(this ICamera camera, bool value)
        {
            camera.Parameters[PLCamera.GainAuto].SetValue(value ? GainAutoModeOn : GainAutoModeOff);
        }

        public static IEnumerable<string> GetPixelFormats(this ICamera camera)
        {
            return camera.Parameters[PLCamera.PixelFormat].GetAllValues();
        }

        public static double GetFrameRate(this ICamera camera)
        {
            return camera.Parameters[PLCamera.ResultingFrameRate].GetValue();
        }

        public static string GetPixelFormat(this ICamera camera)
        {
            return camera.Parameters[PLCamera.PixelFormat].GetValue();
        }

        public static void SetPixelFormat(this ICamera camera, string value)
        {
            camera.Parameters[PLCamera.PixelFormat].SetValue(value);
        }
    }
}