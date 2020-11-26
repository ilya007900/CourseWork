using AppDomain.BrightnessDistributionEntities;
using AppDomain.FunctionalExtensions;
using Prism.Mvvm;

namespace WpfApp.ViewModels
{
    public class DiodeBehaviorViewModel : BindableBase
    {
        private bool use;
        private byte number;
        private int maxEnergy;
        private double tau;
        private int km1;
        private int km2;
        private short step;
        private bool isInUse;

        public bool Use
        {
            get => use;
            set => SetProperty(ref use, value);
        }

        public byte Number
        {
            get => number;
            set => SetProperty(ref number, value);
        }

        public int MaxEnergy
        {
            get => maxEnergy;
            set => SetProperty(ref maxEnergy, value);
        }

        public double Tau
        {
            get => tau;
            set => SetProperty(ref tau, value);
        }

        public int Km1
        {
            get => km1;
            set => SetProperty(ref km1, value);
        }

        public int Km2
        {
            get => km2;
            set => SetProperty(ref km2, value);
        }

        public short Step
        {
            get => step;
            set => SetProperty(ref step, value);
        }

        public bool IsInUse
        {
            get => isInUse;
            set => SetProperty(ref isInUse, value);
        }

        public static DiodeBehaviorViewModel From(Diode diode)
        {
            return new DiodeBehaviorViewModel
            {
                Number = diode.Number,
                MaxEnergy = diode.MaxEnergy
            };
        }

        public static Result<DiodeBehavior> To(DiodeBehaviorViewModel vm)
        {
            var diodeCreateResult = Diode.Create(vm.Number, vm.MaxEnergy);
            if (diodeCreateResult.HasErrors)
            {
                return Result<DiodeBehavior>.Failure(diodeCreateResult.ErrorMessage);
            }

            return DiodeBehavior.Create(diodeCreateResult.Value, vm.Tau, vm.Km1, vm.Km2, vm.Step);
        }
    }
}