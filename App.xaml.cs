using Microsoft.Win32;
using NotionDeadlineFairy.Abstractions;
using NotionDeadlineFairy.Models;
using NotionDeadlineFairy.Services;
using NotionDeadlineFairy.Utils;
using NotionDeadlineFairy.Views;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace NotionDeadlineFairy
{
    public partial class App : System.Windows.Application, INavigation
    {
        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_MINIMIZE = 0xF020;

        private WindowState _lastNonMinimized = WindowState.Normal;
        private bool _restoring;
        private HwndSource? _source;

        private TrayService? _trayService;
        private SettingsWindow? _settingsWindow;
        private MainWindow? _mainWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DispatcherHelper.Initialize(Dispatcher);

            ServiceLocator.Instance.Register<INavigation>(this);

            SettingService.Instance.Load();

            _mainWindow = new MainWindow();
            _mainWindow.SourceInitialized += _mainWindow_SourceInitialized;
            _mainWindow.StateChanged += _mainWindow_StateChanged;
            _mainWindow.Show();

            _trayService = new TrayService();
            _trayService.Initialize();
        }

        private void _mainWindow_SourceInitialized(object? sender, EventArgs e)
        {
            _source = (HwndSource)PresentationSource.FromVisual((Visual)sender);
            _source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // 최소화 명령 자체를 차단 (버튼/Alt+Space/Win+D 등에서 오는 SC_MINIMIZE 대비)
            if (msg == WM_SYSCOMMAND && ((wParam.ToInt32() & 0xFFF0) == SC_MINIMIZE))
            {
                handled = true;
                RestoreToLastState();
            }
            return IntPtr.Zero;
        }

        private void RestoreToLastState()
        {
            if (_restoring) return;
            _restoring = true;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    var target = _lastNonMinimized == WindowState.Minimized
                        ? WindowState.Normal
                        : _lastNonMinimized;

                    _mainWindow.WindowState = target;

                    // 필요하면 앞으로 가져오기(원치 않으면 주석)
                    // Activate();
                    // Topmost = true;
                }
                finally
                {
                    _restoring = false;
                }
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }


        private void _mainWindow_StateChanged(object? sender, EventArgs e)
        {
            if (_mainWindow.WindowState == WindowState.Minimized)
                RestoreToLastState();
            else
                _lastNonMinimized = _mainWindow.WindowState;
        }

        #region INavigation
        public void OpenDatabaseEdit()
        {
            if (_settingsWindow is { IsLoaded: true })
            {
                _settingsWindow.Activate();
                return;
            }

            _settingsWindow = new SettingsWindow();
            _settingsWindow.Show();
        }

        public void Quit()
        {
            System.Windows.Application.Current.Shutdown();
        }
        #endregion


        protected override void OnExit(ExitEventArgs e)
        {
            // 설정 저장
            SettingService.Instance.Save();

            // 트레이 서비스 종료
            _trayService?.Dispose();
            _trayService = null;

            // 프로그램 종료
            base.OnExit(e);
        }
    }
}
