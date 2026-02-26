using NotionDeadlineFairy.Services;
using System.Windows;

namespace NotionDeadlineFairy
{
    public partial class App : System.Windows.Application
    {

        private TrayService? _trayService;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _trayService = new TrayService();
            _trayService.Initialize(() => Shutdown());
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _trayService?.Dispose();
            _trayService = null;
            base.OnExit(e);
        }
    }
}
