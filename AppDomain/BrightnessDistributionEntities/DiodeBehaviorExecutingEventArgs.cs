using System;

namespace AppDomain.BrightnessDistributionEntities
{
    public class DiodeBehaviorExecutingEventArgs : EventArgs
    {
        public byte Number { get; }

        public DiodeBehaviorExecutingEventArgs(byte number)
        {
            Number = number;
        }
    }
}