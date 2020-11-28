using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using AppDomain.BrightnessDistributionEntities;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using WpfApp.Services;

namespace WpfApp.ViewModels
{
    public class BrightnessDistributionViewModel : BindableBase
    {
        private readonly BrightnessDistributionService service;
        private readonly FileDialogService fileDialogService;

        private IReadOnlyList<DiodeBehaviorViewModel> diodes =
            Diode.DefaultDiodes.Select(DiodeBehaviorViewModel.From).ToList();

        private bool tauTuning;
        private bool isInProgress;
        private string state;

        private bool stopRequired;

        private DelegateCommand loadDiodesCommand;
        private DelegateCommand saveDiodesCommand;
        private DelegateCommand startCommand;
        private DelegateCommand stopCommand;

        public IReadOnlyList<DiodeBehaviorViewModel> Diodes
        {
            get => diodes;
            set => SetProperty(ref diodes, value);
        }

        public bool TauTuning
        {
            get => tauTuning;
            set => SetProperty(ref tauTuning, value);
        }

        public bool IsInProgress
        {
            get => isInProgress;
            set => SetProperty(ref isInProgress, value);
        }

        public string State
        {
            get => state;
            set => SetProperty(ref state, value);
        }

        public DelegateCommand StartCommand
        {
            get
            {
                if (startCommand == null)
                {
                    startCommand = new DelegateCommand(Start, () => !IsInProgress);
                    startCommand.ObservesProperty(() => IsInProgress);
                }

                return startCommand;
            }
        }

        public DelegateCommand StopCommand
        {
            get
            {
                if (stopCommand == null)
                {
                    stopCommand = new DelegateCommand(Stop, () => IsInProgress);
                    stopCommand.ObservesProperty(() => IsInProgress);
                }

                return stopCommand;
            }
        }

        public DelegateCommand LoadDiodesCommand =>
            loadDiodesCommand ?? (loadDiodesCommand = new DelegateCommand(LoadDiodes));

        public DelegateCommand SaveDiodesCommand =>
            saveDiodesCommand ?? (saveDiodesCommand = new DelegateCommand(SaveDiodes));

        public BrightnessDistributionViewModel(
            BrightnessDistributionService service,
            FileDialogService fileDialogService)
        {
            this.service = service;
            this.fileDialogService = fileDialogService;
        }

        public void LoadDiodes()
        {
            try
            {
                if (fileDialogService.OpenFileDialog("Text file (*.json)|*.json"))
                {
                    var json = File.ReadAllText(fileDialogService.FilePath);
                    Diodes = JsonConvert.DeserializeObject<IReadOnlyList<DiodeBehaviorViewModel>>(json);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public void SaveDiodes()
        {
            try
            {
                if (fileDialogService.SaveFileDialog("Text file (*.json)|*.json"))
                {
                    var json = JsonConvert.SerializeObject(Diodes);
                    File.WriteAllText(fileDialogService.FilePath, json);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public void Start()
        {
            var useDiodes = Diodes.Where(x => x.Use).ToArray();

            if (TauTuning)
            {
                var thread = new Thread(() => RunTauTuning(useDiodes));
                thread.Start();
            }
            else
            {
                var diodesBehavior = new List<DiodeBehavior>();
                foreach (var diodeViewModel in useDiodes)
                {
                    var createResult = DiodeBehaviorViewModel.To(diodeViewModel);
                    if (createResult.HasErrors)
                    {
                        MessageBox.Show(createResult.ErrorMessage);
                        return;
                    }

                    diodesBehavior.Add(createResult.Value);
                }

                var thread = new Thread(() => Run(diodesBehavior));
                thread.Start();
            }
        }

        public void Run(IReadOnlyList<DiodeBehavior> diodesBehavior)
        {
            IsInProgress = true;
            State = "In Progress";

            service.DiodeBehaviorExecuting += Service_DiodeBehaviorExecuting;
            service.DiodeBehaviorExecuted += Service_DiodeBehaviorExecuted;

            var result = service.Run(diodesBehavior);
            if (result.HasErrors)
            {
                MessageBox.Show(result.ErrorMessage);
            }

            service.DiodeBehaviorExecuting -= Service_DiodeBehaviorExecuting;
            service.DiodeBehaviorExecuted -= Service_DiodeBehaviorExecuted;

            IsInProgress = false;
            State = "Finished";
        }

        public void RunTauTuning(IReadOnlyList<DiodeBehaviorViewModel> diodesBehavior)
        {
            if (!diodesBehavior.Any())
            {
                return;
            }

            IsInProgress = true;
            State = "In Progress";

            var i = 0;
            while (!stopRequired)
            {
                diodesBehavior[i].IsInUse = true;
                service.RunTauTuning(diodesBehavior[i].Number, (int) diodesBehavior[i].Tau);
                diodesBehavior[i].IsInUse = false;

                if (i == diodesBehavior.Count - 1)
                {
                    i = 0;
                }
                else
                {
                    i++;
                }
            }

            IsInProgress = false;
            State = "Finished";
            stopRequired = false;
        }

        public void Stop()
        {
            if (TauTuning)
            {
                stopRequired = true;
            }
        }

        private void Service_DiodeBehaviorExecuting(object sender, DiodeBehaviorExecutingEventArgs e)
        {
            Diodes.First(x => x.Number == e.Number).IsInUse = true;
        }

        private void Service_DiodeBehaviorExecuted(object sender, DiodeBehaviorExecutedEventArgs e)
        {
            Diodes.First(x => x.Number == e.Number).IsInUse = false;
        }
    }
}