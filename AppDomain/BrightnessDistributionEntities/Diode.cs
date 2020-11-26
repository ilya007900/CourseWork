using System.Collections.Generic;
using AppDomain.FunctionalExtensions;

namespace AppDomain.BrightnessDistributionEntities
{
    public class Diode
    {
        public static readonly Diode Diode1 = new Diode(1, 630);
        public static readonly Diode Diode2 = new Diode(2, 710);
        public static readonly Diode Diode3 = new Diode(3, 730);
        public static readonly Diode Diode4 = new Diode(4, 830);
        public static readonly Diode Diode5 = new Diode(5, 880);
        public static readonly Diode Diode6 = new Diode(6, 930);
        public static readonly Diode Diode7 = new Diode(7, 980);

        public static IReadOnlyList<Diode> DefaultDiodes = new List<Diode>
        {
            Diode1, Diode2, Diode3, Diode4, Diode5, Diode6, Diode7
        };

        public byte Number { get; set; }
        public int MaxEnergy { get; set; }

        private Diode(byte number, int maxEnergy)
        {
            Number = number;
            MaxEnergy = maxEnergy;
        }

        public static Result<Diode> Create(byte number, int maxEnergy)
        {
            if (number == 0)
            {
                return Result<Diode>.Failure("Number can't be zero");
            }

            if (maxEnergy <= 0)
            {
                return Result<Diode>.Failure($"Error in diode number {number}. Max energy can't be zero or negative");
            }

            return Result<Diode>.Success(new Diode(number, maxEnergy));
        }
    }
}