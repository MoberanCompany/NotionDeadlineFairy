using Microsoft.Win32;
using NotionDeadlineFairy.Models;
using NotionDeadlineFairy.Services;
using NotionDeadlineFairy.Views;
using System.Runtime.InteropServices;
using NotionDeadlineFairy.ViewModels;
using NotionDeadlineFairy.Views;
using System.Windows;
using System.Windows.Interop;

namespace NotionDeadlineFairy
{
    public partial class App : System.Windows.Application
    {

        private TrayService? _trayService;
        private SettingsWindow? _settingsWindow;
        private MainWindow? _mainWindow;
        private WindowControlWindow? _windowControlWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SettingService.Instance.Load();

            _windowControlWindow = new WindowControlWindow();
            _mainWindow = new MainWindow();
            _windowControlWindow.MainContentFrame.Content = _mainWindow.Content;
            _windowControlWindow.Show();

            var setting = SettingService.Instance.Current;
            ApplyWindowMode(setting.WindowMode);
            ApplyClickThrough(setting.IsClickThrough);
            ApplyEditMode(setting.IsEditMode);

            _trayService = new TrayService();
            _trayService.Initialize(new TrayMenuCallbacks
            {
                OnWindowModeChanged = OnWindowModeChanged,
                OnAutoStartChanged = OnAutoStartChanged,
                OnPollingIntervalChanged = OnPollingIntervalChanged,
                OnEditModeChanged = OnEditModeChanged,
                OnRefreshRequested = OnRefreshRequested,
                OnClickThroughChanged = OnClickThroughChanged,
                OnDatabaseEditRequested = OpenDatabaseEdit,
                OnExitRequested = () => Shutdown(),
            });


            ApplyTheme(setting.BackgroundColor, setting.ForegroundColor);
            ThemeService.Instance.ThemeChanged += OnThemeChanged;
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
            if (_windowControlWindow != null && _windowControlWindow.DataContext is WindowControlWindowViewModel vm)
            {
                vm.BackgroundColor = backgroundColorCode;
                vm.ForegroundColor = foregroundColorCode;
            }
        }

        private void OnWindowModeChanged(WindowMode mode)
        {
            SettingService.Instance.Current.WindowMode = mode;
            SettingService.Instance.Save();
            ApplyWindowMode(mode);
        }

        private void ApplyWindowMode(WindowMode mode)
        {
            if (_mainWindow is null) return;

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

        private void OnAutoStartChanged(bool enabled)
        {
            SettingService.Instance.Current.AutoStart = enabled;
            SettingService.Instance.Save();

            using var key = Registry.CurrentUser.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            if (key is null) return;

            const string appName = "NotionDeadlineFairy";
            if (enabled)
            {
                var exePath = Environment.ProcessPath;
                if (exePath is not null)
                    key.SetValue(appName, exePath);
            }
            else
            {
                key.DeleteValue(appName, false);
            }
        }

        private void OnPollingIntervalChanged(int seconds)
        {
            SettingService.Instance.Current.PollingIntervalSeconds = seconds;
            SettingService.Instance.Save();
        }

        private void OnEditModeChanged(bool enabled)
        {
            SettingService.Instance.Current.IsEditMode = enabled;
            SettingService.Instance.Save();
            ApplyEditMode(enabled);
        }

        private void ApplyEditMode(bool enabled)
        {
            if (_windowControlWindow is null) return;
            if(_windowControlWindow.DataContext is WindowControlWindowViewModel vm)
            {
                vm.IsEditMode = enabled;
            }
        }

        private void OnRefreshRequested()
        {
            // TODO: 데이터 새로고침 로직 구현
        }

        private void OnClickThroughChanged(bool enabled)
        {
            SettingService.Instance.Current.IsClickThrough = enabled;
            SettingService.Instance.Save();
            ApplyClickThrough(enabled);
        }

        private void ApplyClickThrough(bool enabled)
        {
            _mainWindow?.SetClickThrough(enabled);
        }

        private void OpenDatabaseEdit()
        {
            if (_settingsWindow is { IsLoaded: true })
            {
                _settingsWindow.Activate();
                return;
            }

            _settingsWindow = new SettingsWindow();
            _settingsWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            SettingService.Instance.Save();
            _trayService?.Dispose();
            _trayService = null;
            base.OnExit(e);
        }

        #region P/Invoke

        private static readonly IntPtr HWND_BOTTOM = new(1);
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOACTIVATE = 0x0010;

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter,
            int x, int y, int cx, int cy, uint flags);

        #endregion
    }
}
