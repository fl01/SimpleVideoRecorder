using System.Windows;
using SimpleVideoRecorder.Client.Common;
using SimpleVideoRecorder.Client.View;
using SimpleVideoRecorder.Client.ViewModel;
using SimpleVideoRecorder.Core.ScreenCapture;
using SimpleVideoRecorder.Core.ScreenDetails;

namespace SimpleVideoRecorder.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            IRecordingServiceProvider provider = new RecordingServiceProvider(new WinFormsScreenMetadataService());

            ISelectRecordAreaView view = new SelectRecordAreaView();
            ISelectRecordAreaViewModel viewModel = new SelectRecordAreaViewModel(provider, new NavigationService());

            view.DataContext = viewModel;
            view.ShowDialog();
        }
    }
}
