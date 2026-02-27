using Microsoft.Win32;
using NotionDeadlineFairy.Abstractions;
using NotionDeadlineFairy.Models;
using NotionDeadlineFairy.Services;
using NotionDeadlineFairy.Utils;
using NotionDeadlineFairy.ViewModels;
using NotionDeadlineFairy.Views;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace NotionDeadlineFairy
{
    public partial class App : System.Windows.Application, INavigation, IWidget
    {
        #region P/Invoke

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WM_NCHITTEST = 0x0084;
        private static readonly IntPtr HTTRANSPARENT = new(-1);
        private static readonly IntPtr HWND_BOTTOM = new(1);
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_MINIMIZE = 0xF020;

        [DllImport("user32.dll", EntryPoint = "GetWindowLong", SetLastError = true)]
        private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter,
            int x, int y, int cx, int cy, uint flags);

        private static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex) =>
            IntPtr.Size == 8 ? GetWindowLongPtr64(hWnd, nIndex) : new IntPtr(GetWindowLong32(hWnd, nIndex));

        private static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong) =>
            IntPtr.Size == 8 ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong) : new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));

        #endregion

        private HwndSourceHook? _clickThroughHook;
        private bool _isClickThrough;

        private WindowState _lastNonMinimized = WindowState.Normal;
        private bool _restoring;
        private HwndSource? _hwndSource;

        private TrayService? _trayService;
        private SettingsWindow? _settingsWindow;
        private MainWindow? _mainWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            SettingService.Instance.Load();

            DispatcherHelper.Initialize(Dispatcher);

            ServiceLocator.Instance.Register<INavigation>(this);
            ServiceLocator.Instance.Register<IWidget>(this);
            
            _mainWindow = new MainWindow();
            _mainWindow.SourceInitialized += _mainWindow_SourceInitialized;
            _mainWindow.StateChanged += _mainWindow_StateChanged;
            _mainWindow.Show();

            _trayService = new TrayService();
            _trayService.Initialize();

            ApplyTheme(SettingService.Instance.Current.BackgroundColor, SettingService.Instance.Current.ForegroundColor);
            ApplyEditMode(SettingService.Instance.Current.IsEditMode);
            ThemeService.Instance.ThemeChanged += OnThemeChanged;
            PollingService.Instance.Start(setting.PollingIntervalSeconds);
        }


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

        private void _mainWindow_SourceInitialized(object? sender, EventArgs e)
        {
            _hwndSource = (HwndSource)PresentationSource.FromVisual(_mainWindow);
            _hwndSource.AddHook(WndProc);
        }

        private void OnThemeChanged(string backgroundColorCode, string foregroundColorCode)
        {
            SettingService.Instance.Current.BackgroundColor = backgroundColorCode;
            SettingService.Instance.Current.ForegroundColor = foregroundColorCode;
            SettingService.Instance.Save();
            ApplyTheme(backgroundColorCode, foregroundColorCode);
        }


        private void ApplyTheme(string backgroundColorCode, string foregroundColorCode)
        {

        }

        private void OnWindowModeChanged(WindowMode mode)
        {
            _hwndSource = (HwndSource)PresentationSource.FromVisual((Visual)_mainWindow);
            _hwndSource.AddHook(WndProc);
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
            PollingService.Instance.UpdateInterval(seconds);
        }


        private void OnEditModeChanged(bool enabled)
        {
            SettingService.Instance.Current.IsEditMode = enabled;
            SettingService.Instance.Save();
            //ApplyEditMode(enabled);
        }

        private void ApplyEditMode(bool enabled)
        {
            if (_mainWindow is null) return;
            if(_mainWindow.DataContext is MainViewModel vm)
            {
                vm.IsEditMode = enabled;
            }
        }

        private void OnRefreshRequested()
        {
            // TODO: 데이터 새로고침 로직 구현
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
            PollingService.Instance.Stop();
            SettingService.Instance.Save();
            _trayService?.Dispose();
            _trayService = null;
            base.OnExit(e);
        }
        #endregion

        public void SetWindowMode(WindowMode mode)
        {
            switch (mode)
            {
                case WindowMode.Normal:
                    _mainWindow.Topmost = false;
                    break;
                case WindowMode.Topmost:
                    _mainWindow.Topmost = true;
                    break;
                case WindowMode.Bottommost:
                    _mainWindow.Topmost = false;
                    var handle = new WindowInteropHelper(_mainWindow).Handle;
                    if (handle != IntPtr.Zero)
                    {
                        SetWindowPos(handle, HWND_BOTTOM, 0, 0, 0, 0,
                            SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
                    }
                    break;
            }
        }

        public void SetClickThrough(bool enable)
        {
            _isClickThrough = enable;
            _mainWindow.IsHitTestVisible = !enable;
            ApplyClickThroughStyle(enable);
        }

        private void ApplyClickThroughStyle(bool enable)
        {
            if (_hwndSource == null)
            {
                // 0.5초 후에 다시 시도 (HWND가 아직 생성되지 않았을 수 있음)
                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(500);
                    Dispatcher.Invoke(() => ApplyClickThroughStyle(enable));
                });

                return;
            }

            _clickThroughHook ??= ClickThroughWndProc;
            _hwndSource.RemoveHook(_clickThroughHook);

            var exStyle = (int)GetWindowLongPtr(_hwndSource.Handle, GWL_EXSTYLE);
            if (enable)
            {
                SetWindowLongPtr(_hwndSource.Handle, GWL_EXSTYLE, (IntPtr)(exStyle | WS_EX_TRANSPARENT));
                _hwndSource.AddHook(_clickThroughHook);
            }
            else
            {
                SetWindowLongPtr(_hwndSource.Handle, GWL_EXSTYLE, (IntPtr)(exStyle & ~WS_EX_TRANSPARENT));
            }
        }

        private IntPtr ClickThroughWndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_NCHITTEST)
            {
                handled = true;
                return HTTRANSPARENT;
            }
            return IntPtr.Zero;
        }
    }
}
