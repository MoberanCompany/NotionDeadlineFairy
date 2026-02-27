using NotionDeadlineFairy.Services;
using NotionDeadlineFairy.ViewModels;
using NotionDeadlineFairy.Views;
using System.Windows;

namespace NotionDeadlineFairy
{
    public partial class App : System.Windows.Application
    {

        private TrayService? _trayService;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SettingService.Instance.Load();

            _trayService = new TrayService();
            _trayService.Initialize(() => Shutdown());

            var originalMainWindow = new MainWindow();

            var windowControlWindowViewModel = new WindowControlWindowViewModel();
            var windowControlWindow = new WindowControlWindow();

            windowControlWindow.MainContentFrame.Content = originalMainWindow.Content;
            windowControlWindow.DataContext = windowControlWindowViewModel;


            // 편집모드 토글 시 windowControlWindowViewModel.IsEditMode = boolean; 필요합니다.

            windowControlWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            SettingService.Instance.Save();
            _trayService?.Dispose();
            _trayService = null;
            base.OnExit(e);
        }
    }
}
