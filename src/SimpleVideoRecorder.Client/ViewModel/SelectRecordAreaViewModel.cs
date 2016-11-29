using System;
using System.Diagnostics;
using System.Windows.Input;
using SimpleVideoRecorder.Client.Common;
using SimpleVideoRecorder.Client.View;
using SimpleVideoRecorder.Core.ScreenCapture;

namespace SimpleVideoRecorder.Client.ViewModel
{
    public class SelectRecordAreaViewModel : ViewModelBase, ISelectRecordAreaViewModel, IShowSelectionViewModel
    {
        private readonly IRecordingServiceProvider recordingServiceProvider;
        private readonly INavigationService navigationService;

        private IRecordingService recordingService;
        private bool isVisible;
        private bool isRecording;

        public bool IsVisible
        {
            get { return isVisible; }
            set { Set(ref isVisible, value); }
        }

        public bool IsRecording
        {
            get { return isRecording; }
            set { Set(ref isRecording, value); }
        }

        public string StatusText
        {
            get
            {
                // TODO : show something more informational
                return IsRecording ? "Recording" : "Idle";
            }
        }

        public ICommand HideSelectionViewCommand { get; private set; }

        public ICommand ShowSelectionFormCommand { get; private set; }

        public ICommand StopActiveRecordCommand { get; private set; }

        public ICommand ExitApplicationCommand { get; private set; }

        public ICommand ShowSettingsCommand { get; private set; }

        private IRecordingService ActiveRecordingService
        {
            get
            {
                return recordingService;
            }

            set
            {
                if (value != null)
                {
                    Debug.Assert(recordingService == null);
                }

                recordingService = value;
            }
        }

        public SelectRecordAreaViewModel(IRecordingServiceProvider recordingServiceProvider, INavigationService navigationService)
        {
            Debug.Assert(recordingServiceProvider != null);
            Debug.Assert(navigationService != null);

            this.recordingServiceProvider = recordingServiceProvider;
            this.navigationService = navigationService;

            InitialzeCommands();
        }

        public void OnSelectionSet(RegionBlock selectedRegion)
        {
            Debug.WriteLine($"New selection: X={selectedRegion.X} Y={selectedRegion.Y} Width={selectedRegion.Width} Height={selectedRegion.Height}");

            Debug.Assert(ActiveRecordingService == null);

            ActiveRecordingService = recordingServiceProvider.Create(selectedRegion, KnownFourCC.Codecs.MotionJpeg, 70);
            HideSelectionViewCommand.Execute(null);

            BeginRecord();
        }

        private void BeginRecord()
        {
            Debug.Assert(ActiveRecordingService != null);

            IsRecording = true;
            ActiveRecordingService.Start();
        }

        private void InitialzeCommands()
        {
            HideSelectionViewCommand = new RelayCommand(x => IsVisible = false);
            ShowSelectionFormCommand = new RelayCommand(x => ShowSelectionForm());
            ExitApplicationCommand = new RelayCommand(x => ExitApplication());
            StopActiveRecordCommand = new RelayCommand(x => EndRecording());
            ShowSettingsCommand = new RelayCommand(x => ShowSettings());
        }

        private void ShowSettings()
        {
            navigationService.ShowDialog<ISettingsView>(ShowWindowBehavior.Single, () =>
            {
                ISettingsView view = new SettingsView();
                ISettingsViewModel viewModel = new SettingsViewModel();
                view.DataContext = viewModel;
                return view;
            });
        }

        private void EndRecording()
        {
            ActiveRecordingService.Stop();
            ActiveRecordingService = null;
            IsRecording = false;
        }

        private void ShowSelectionForm()
        {
            IsVisible = true;
        }

        private void ExitApplication()
        {
            ActiveRecordingService?.Stop();

            Environment.Exit(0);
        }
    }
}
