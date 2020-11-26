using Basler.Pylon;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace AppDomain.Utils
{
    public static class ImageUtils
    {
        public static byte[] ConvertToBytes(this PixelDataConverter converter, IGrabResult grabResult)
        {
            if (grabResult.Width == 0 || grabResult.Height == 0)
            {
                return new byte[0];
            }

            var bufferSize = grabResult.Width * grabResult.Height;

            switch (grabResult.PixelTypeValue)
            {
                case PixelType.Mono8:
                    converter.OutputPixelFormat = PixelType.Mono8;
                    break;
                case PixelType.Mono12:
                case PixelType.Mono12p:
                case PixelType.Mono12packed:
                    converter.OutputPixelFormat = PixelType.Mono16;
                    bufferSize *= 2;
                    break;
                default:
                    throw new NotSupportedException($"Pixel type {grabResult.PixelTypeValue} not supported");
            }

            var bytes = new byte[bufferSize];
            converter.Convert(bytes, grabResult);

            return bytes;
        }

        public static Bitmap Convert(this PixelDataConverter converter, IGrabResult grabResult)
        {
            if (grabResult.Width == 0 || grabResult.Height == 0)
            {
                return null;
            }

            var bitmap = new Bitmap(grabResult.Width, grabResult.Height, PixelFormat.Format32bppRgb);
            var rectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var bitmapData = bitmap.LockBits(rectangle, ImageLockMode.ReadWrite, bitmap.PixelFormat);
            converter.OutputPixelFormat = PixelType.BGRA8packed;

            var ptrBmp = bitmapData.Scan0;
            converter.Convert(ptrBmp, bitmapData.Stride * bitmap.Height, grabResult);
            bitmap.UnlockBits(bitmapData);
            return bitmap;
        }
    }
}