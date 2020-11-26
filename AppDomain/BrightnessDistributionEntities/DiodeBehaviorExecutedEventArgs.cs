using System;

namespace AppDomain.BrightnessDistributionEntities
{
    public class DiodeBehaviorExecutedEventArgs : EventArgs
    {
        public byte Number { get; }

        public DiodeBehaviorExecutedEventArgs(byte number)
        {
            Number = number;
        }
    }
}