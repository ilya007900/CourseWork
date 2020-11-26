namespace AppDomain.PupilReactionEntities
{
    public class PupilReaction
    {
        public byte StartingBrightLevel { get; }

        public byte BrightIncreaseCoefficient { get; }

        public ushort CurrentBright { get; private set; }

        public PupilReaction(byte startingBrightLevel, byte brightIncreaseCoefficient)
        {
            StartingBrightLevel = startingBrightLevel;
            BrightIncreaseCoefficient = brightIncreaseCoefficient;
        }

        public void Init()
        {
            CurrentBright = StartingBrightLevel;
        }

        public ushort IncreaseBright()
        {
            var pwm = CurrentBright + BrightIncreaseCoefficient;
            if (pwm > byte.MaxValue)
            {
                pwm = byte.MaxValue;
            }

            var asUshort = (ushort)pwm;
            return CurrentBright = asUshort;
        }
    }
}