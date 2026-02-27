using NotionDeadlineFairy.Abstractions;
using NotionDeadlineFairy.Services;
using NotionDeadlineFairy.ViewModels;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace NotionDeadlineFairy.Views
{
    public partial class MainWindow : IWidget
    {
        #region P/Invoke

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WM_NCHITTEST = 0x0084;
        private static readonly IntPtr HTTRANSPARENT = new(-1);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong", SetLastError = true)]
        private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        private static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex) =>
            IntPtr.Size == 8 ? GetWindowLongPtr64(hWnd, nIndex) : new IntPtr(GetWindowLong32(hWnd, nIndex));

        private static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong) =>
            IntPtr.Size == 8 ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong) : new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));

        #endregion

        private HwndSourceHook? _clickThroughHook;
        private bool _isClickThrough;

        public void SetClickThrough(bool enable)
        {
            _isClickThrough = enable;
            IsHitTestVisible = !enable;
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

        public void Refresh()
        {
            if (this.DataContext is MainViewModel vm)
            {
                vm.Refresh();
            }
        }

        public void SetEditMode(bool enabled)
        {
        }

        public void ReDraw()    
        {
            // TODO
            throw new NotImplementedException();
        }
    }
}
