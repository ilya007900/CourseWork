using AppDomain.FunctionalExtensions;

namespace AppDomain.BrightnessDistributionEntities
{
    public class DiodeBehavior
    {
        public Diode Diode { get; }
        public double Tau { get; }
        public int Km1 { get; }
        public int Km2 { get; }
        public short Step { get; }

        private DiodeBehavior(Diode diode, double tau, int km1, int km2, short step)
        {
            Diode = diode;
            Tau = tau;
            Km1 = km1;
            Km2 = km2;
            Step = step;
        }

        public static Result<DiodeBehavior> Create(Diode diode, double tau, int km1, int km2, short step)
        {
            if (diode == null)
            {
                return Result<DiodeBehavior>.Failure("Diode can't be null");
            }

            if (km1 == 0)
            {
                return Result<DiodeBehavior>.Failure("Km1 can't be zero");
            }

            if (km2 == 0)
            {
                return Result<DiodeBehavior>.Failure("Km2 can't be zero");
            }

            return Result<DiodeBehavior>.Success(new DiodeBehavior(diode, tau, km1, km2, step));
        }

        public double CalculateKm1Tau() => Tau / Km1;

        public double CalculateKm2Tau() => Tau / Km2;
    }
}