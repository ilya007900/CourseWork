using System;

namespace AppDomain.PupilReactionEntities
{
    public class BrightChangedEventArgs : EventArgs
    {
        public ushort Bright { get; }

        public BrightChangedEventArgs(ushort bright)
        {
            Bright = bright;
        }
    }
}