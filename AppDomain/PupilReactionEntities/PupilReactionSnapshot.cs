using System;

namespace AppDomain.PupilReactionEntities
{
    public class PupilReactionSnapshot
    {
        public byte[] Image { get; set; }

        public double ExposureTime { get; set; }

        public double Gain { get; set; }

        public string PixelFormat { get; set; }

        public ushort PWM { get; set; }

        public DateTime DateTime { get; set; }
    }
}