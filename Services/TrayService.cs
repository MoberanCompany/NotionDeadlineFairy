using Drawing = System.Drawing;
using Forms = System.Windows.Forms;

namespace NotionDeadlineFairy.Services
{
    public class TrayService
    {
        private Forms.NotifyIcon? _trayIcon;

        public TrayService() { }

        public void Initialize(Action onExitRequested)
        {
            var trayMenu = new Forms.ContextMenuStrip();
            trayMenu.Items.Add("종료", null, (_, _) => onExitRequested());

            _trayIcon = new Forms.NotifyIcon
            {
                Icon = Drawing.SystemIcons.Application,
                Visible = true,
                Text = "NotionOverlayWidget",
                ContextMenuStrip = trayMenu
            };
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
