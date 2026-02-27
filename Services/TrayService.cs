using Microsoft.Win32;
using NotionDeadlineFairy.Abstractions;
using NotionDeadlineFairy.Models;
using System.IO;
using Drawing = System.Drawing;
using Forms = System.Windows.Forms;

namespace NotionDeadlineFairy.Services
{
    public class TrayService 
    {
        private DateTime _lastRefreshTime = DateTime.Now;
        private Forms.NotifyIcon? _trayIcon;

        private Forms.ToolStripMenuItem? _normalItem;
        private Forms.ToolStripMenuItem? _topmostItem;
        private Forms.ToolStripMenuItem? _bottommostItem;

        private Forms.ToolStripMenuItem? _autoStartItem;
        private Forms.ToolStripMenuItem? _editModeItem;
        private Forms.ToolStripMenuItem? _clickThroughItem;

        private Forms.ToolStripMenuItem? _poll1MinItem;
        private Forms.ToolStripMenuItem? _poll5MinItem;
        private Forms.ToolStripMenuItem? _poll10MinItem;
        private Forms.ToolStripMenuItem? _poll30MinItem;

        public TrayService() { }

        public void Initialize()
        {
            AppSetting setting = SettingService.Instance.Current;
            ContextMenuStrip trayMenu = new Forms.ContextMenuStrip();

            // 1. Window Mode
            ToolStripMenuItem windowModeMenu = new Forms.ToolStripMenuItem("윈도우 모드");
            _normalItem = new Forms.ToolStripMenuItem("Normal", null, (_, _) =>
            {
                SetWindowModeChecked(WindowMode.Normal);
            });
            _topmostItem = new Forms.ToolStripMenuItem("Top-most", null, (_, _) =>
            {
                SetWindowModeChecked(WindowMode.Topmost);
            });
            _bottommostItem = new Forms.ToolStripMenuItem("Bottom-most", null, (_, _) =>
            {
                SetWindowModeChecked(WindowMode.Bottommost);
            });
            windowModeMenu.DropDownItems.AddRange([_normalItem, _topmostItem, _bottommostItem]);
            SetWindowModeChecked(setting.WindowMode);
            trayMenu.Items.Add(windowModeMenu);

            // 2. Auto-start
            _autoStartItem = new Forms.ToolStripMenuItem("윈도우 시작 시 자동 시작")
            {
                CheckOnClick = true,
                Checked = setting.AutoStart
            };
            _autoStartItem.CheckedChanged += OnAutoStartChanged;
            trayMenu.Items.Add(_autoStartItem);

            // 3. Polling interval
            ToolStripMenuItem pollingMenu = new Forms.ToolStripMenuItem("폴링 주기");
            _poll1MinItem = new Forms.ToolStripMenuItem("1분", null, (_, _) =>
            {
                SetPollingIntervalChecked(60);
            });
            _poll5MinItem = new Forms.ToolStripMenuItem("5분", null, (_, _) =>
            {
                SetPollingIntervalChecked(300);
            });
            _poll10MinItem = new Forms.ToolStripMenuItem("10분", null, (_, _) =>
            {
                SetPollingIntervalChecked(600);
            });
            _poll30MinItem = new Forms.ToolStripMenuItem("30분", null, (_, _) =>
            {
                SetPollingIntervalChecked(1800);
            });
            pollingMenu.DropDownItems.AddRange([_poll1MinItem, _poll5MinItem, _poll10MinItem, _poll30MinItem]);
            SetPollingIntervalChecked(setting.PollingIntervalSeconds);
            trayMenu.Items.Add(pollingMenu);

            trayMenu.Items.Add(new Forms.ToolStripSeparator());

            // 4. Edit mode
            _editModeItem = new Forms.ToolStripMenuItem("위젯 편집 모드")
            {
                CheckOnClick = true,
                Checked = setting.IsEditMode
            };
            _editModeItem.CheckedChanged += (_, _) =>
            {
                bool enabled = _editModeItem.Checked;

                SettingService.Instance.Current.IsEditMode = enabled;
                SettingService.Instance.Save();

                List<IWidget>? views = ServiceLocator.Instance.GetService<IWidget>();
                views?.ForEach(v => v.SetEditMode(enabled));
            };
            trayMenu.Items.Add(_editModeItem);

            // 5. Refresh now
            trayMenu.Items.Add("지금 새로고침", null, (_, _) =>
            {
                var datas = ServiceLocator.Instance.GetService<IWidget>();
                datas?.ForEach(d => d.Refresh());
            });

            // 6. Click Through
            _clickThroughItem = new Forms.ToolStripMenuItem("Click Through")
            {
                CheckOnClick = true,
                Checked = setting.IsClickThrough
            };
            _clickThroughItem.CheckedChanged += OnClickThroughChanged;
            trayMenu.Items.Add(_clickThroughItem);

            trayMenu.Items.Add(new Forms.ToolStripSeparator());

            // 7. Database edit
            trayMenu.Items.Add("데이터베이스 편집", null, (_, _) =>
            {
                var navigation = ServiceLocator.Instance.GetService<INavigation>()?.FirstOrDefault();
                navigation?.OpenDatabaseEdit();
            });

            trayMenu.Items.Add(new Forms.ToolStripSeparator());

            // Exit
            trayMenu.Items.Add("종료", null, (_, _) =>
            {
                var app = ServiceLocator.Instance.GetService<INavigation>()?.FirstOrDefault();
                app?.Quit();
            });

            _trayIcon = new Forms.NotifyIcon
            {
                Icon = GetApplicationIcon(),
                Visible = true,
                Text = "NotionOverlayWidget",
                ContextMenuStrip = trayMenu
            };
        }

        private Drawing.Icon GetApplicationIcon()
        {
            try
            {
                // 실행 중인 애플리케이션의 아이콘 추출
                var exePath = Environment.ProcessPath;
                if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
                {
                    return Drawing.Icon.ExtractAssociatedIcon(exePath) ?? Drawing.SystemIcons.Application;
                }
            }
            catch
            {
                // 아이콘 로드 실패 시 기본 아이콘 사용
            }

            return Drawing.SystemIcons.Application;
        }

        private async Task StartPollingAsync(dynamic setting, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(1000, cancellationToken);
                    var now = DateTime.Now;
                    if ((now - _lastRefreshTime).TotalSeconds >= setting.PollingIntervalSeconds)
                    {
                        try
                        {
                            var widgets = ServiceLocator.Instance.GetService<IWidget>();
                            widgets?.ForEach(w => w.Refresh());
                        }
                        catch
                        {
                            // 새로고침 중 예외가 발생해도 무시
                        }
                        _lastRefreshTime = now;
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private void SetWindowModeChecked(WindowMode mode)
        {
            if (_normalItem != null) _normalItem.Checked = mode == WindowMode.Normal;
            if (_topmostItem != null) _topmostItem.Checked = mode == WindowMode.Topmost;
            if (_bottommostItem != null) _bottommostItem.Checked = mode == WindowMode.Bottommost;

            SettingService.Instance.Current.WindowMode = mode;
            SettingService.Instance.Save();

            var widgets = ServiceLocator.Instance.GetService<IWidget>();
            widgets?.ForEach(w => w.SetWindowMode(mode));
        }

        private void SetPollingIntervalChecked(int seconds)
        {
            if (_poll1MinItem != null)
            {
                _poll1MinItem.Checked = (seconds == 60);
            }
            if (_poll5MinItem != null)
            {
                _poll5MinItem.Checked = (seconds == 300);
            }
            if (_poll10MinItem != null)
            {
                _poll10MinItem.Checked = (seconds == 600);
            }
            if (_poll30MinItem != null)
            {
                _poll30MinItem.Checked = (seconds == 1800);
            }

            SettingService.Instance.Current.PollingIntervalSeconds = seconds;
            SettingService.Instance.Save();

            PollingService.Instance.UpdateInterval(seconds);
        }

        private void OnAutoStartChanged(object? sender, EventArgs e)
        {
            if (_autoStartItem is null) return;

            var enabled = _autoStartItem.Checked;
            SettingService.Instance.Current.AutoStart = enabled;
            SettingService.Instance.Save();

            using var key = Registry.CurrentUser.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            if (key is null) return;

            const string appName = "NotionDeadlineFairy";
            if (enabled)
            {
                var exePath = Environment.ProcessPath;
                if (exePath != null)
                    key.SetValue(appName, exePath);
            }
            else
            {
                key.DeleteValue(appName, false);
            }
        }

        private void OnEditModeChanged(object? sender, EventArgs e)
        {
            if (_editModeItem is null) return;

            var enabled = _editModeItem.Checked;
            SettingService.Instance.Current.IsEditMode = enabled;
            SettingService.Instance.Save();

            var views = ServiceLocator.Instance.GetService<IWidget>();
            views?.ForEach(v => v.SetEditMode(enabled));
        }

        private void OnClickThroughChanged(object? sender, EventArgs e)
        {
            if (_clickThroughItem is null) return;

            var enabled = _clickThroughItem.Checked;
            SettingService.Instance.Current.IsClickThrough = enabled;
            SettingService.Instance.Save();

            var views = ServiceLocator.Instance.GetService<IWidget>();
            views?.ForEach(v => v.SetClickThrough(enabled));
        }

        public void Dispose()
        {
            // 트레이 아이콘 정리
            if (_trayIcon is null) return;
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            _trayIcon = null;
        }
    }
}
