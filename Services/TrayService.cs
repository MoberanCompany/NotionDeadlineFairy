using NotionDeadlineFairy.Abstractions;
using NotionDeadlineFairy.Models;
using Drawing = System.Drawing;
using Forms = System.Windows.Forms;

namespace NotionDeadlineFairy.Services
{
    public class TrayMenuCallbacks
    {
        public required Action<WindowMode> OnWindowModeChanged { get; init; }
        public required Action<bool> OnAutoStartChanged { get; init; }
        public required Action<int> OnPollingIntervalChanged { get; init; }
        public required Action OnDatabaseEditRequested { get; init; }
        public required Action OnExitRequested { get; init; }
    }

    public class TrayService
    {
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

        public void Initialize(TrayMenuCallbacks callbacks)
        {
            var setting = SettingService.Instance.Current;
            var trayMenu = new Forms.ContextMenuStrip();

            // 1. Window Mode
            var windowModeMenu = new Forms.ToolStripMenuItem("윈도우 모드");
            _normalItem = new Forms.ToolStripMenuItem("Normal", null, (_, _) =>
            {
                SetWindowModeChecked(WindowMode.Normal);
                callbacks.OnWindowModeChanged(WindowMode.Normal);
            });
            _topmostItem = new Forms.ToolStripMenuItem("Top-most", null, (_, _) =>
            {
                SetWindowModeChecked(WindowMode.Topmost);
                callbacks.OnWindowModeChanged(WindowMode.Topmost);
            });
            _bottommostItem = new Forms.ToolStripMenuItem("Bottom-most", null, (_, _) =>
            {
                SetWindowModeChecked(WindowMode.Bottommost);
                callbacks.OnWindowModeChanged(WindowMode.Bottommost);
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
            _autoStartItem.CheckedChanged += (_, _) =>
                callbacks.OnAutoStartChanged(_autoStartItem.Checked);
            trayMenu.Items.Add(_autoStartItem);

            // 3. Polling interval
            var pollingMenu = new Forms.ToolStripMenuItem("폴링 주기");
            _poll1MinItem = new Forms.ToolStripMenuItem("1분", null, (_, _) =>
            {
                SetPollingIntervalChecked(60);
                callbacks.OnPollingIntervalChanged(60);
            });
            _poll5MinItem = new Forms.ToolStripMenuItem("5분", null, (_, _) =>
            {
                SetPollingIntervalChecked(300);
                callbacks.OnPollingIntervalChanged(300);
            });
            _poll10MinItem = new Forms.ToolStripMenuItem("10분", null, (_, _) =>
            {
                SetPollingIntervalChecked(600);
                callbacks.OnPollingIntervalChanged(600);
            });
            _poll30MinItem = new Forms.ToolStripMenuItem("30분", null, (_, _) =>
            {
                SetPollingIntervalChecked(1800);
                callbacks.OnPollingIntervalChanged(1800);
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

                List<IWidget> views = ServiceLocator.Instance.GetService<IWidget>();
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
            _clickThroughItem.CheckedChanged += (_, _) =>
            {
                var enabled = _clickThroughItem.Checked;
                SettingService.Instance.Current.IsClickThrough = enabled;
                SettingService.Instance.Save();
                var views = ServiceLocator.Instance.GetService<IWidget>();
                views?.ForEach(v => v.SetClickThrough(enabled));
            };
            trayMenu.Items.Add(_clickThroughItem);

            trayMenu.Items.Add(new Forms.ToolStripSeparator());

            // 7. Database edit
            trayMenu.Items.Add("데이터베이스 편집", null, (_, _) => callbacks.OnDatabaseEditRequested());

            trayMenu.Items.Add(new Forms.ToolStripSeparator());

            // Exit
            trayMenu.Items.Add("종료", null, (_, _) => callbacks.OnExitRequested());

            _trayIcon = new Forms.NotifyIcon
            {
                Icon = Drawing.SystemIcons.Application,
                Visible = true,
                Text = "NotionOverlayWidget",
                ContextMenuStrip = trayMenu
            };
        }

        private void SetWindowModeChecked(WindowMode mode)
        {
            if (_normalItem is not null) _normalItem.Checked = mode == WindowMode.Normal;
            if (_topmostItem is not null) _topmostItem.Checked = mode == WindowMode.Topmost;
            if (_bottommostItem is not null) _bottommostItem.Checked = mode == WindowMode.Bottommost;
        }

        private void SetPollingIntervalChecked(int seconds)
        {
            if (_poll1MinItem is not null) _poll1MinItem.Checked = seconds == 60;
            if (_poll5MinItem is not null) _poll5MinItem.Checked = seconds == 300;
            if (_poll10MinItem is not null) _poll10MinItem.Checked = seconds == 600;
            if (_poll30MinItem is not null) _poll30MinItem.Checked = seconds == 1800;
        }

        public void Dispose()
        {
            if (_trayIcon is null)
            {
                return;
            }

            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            _trayIcon = null;
        }
    }
}
