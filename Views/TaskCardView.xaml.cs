using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using NotionDeadlineFairy.Models;
using NotionDeadlineFairy.ViewModels;

namespace NotionDeadlineFairy.Views
{
    public partial class TaskCardView : System.Windows.Controls.UserControl
    {
        public TaskCardView()
        {
            InitializeComponent();
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            // 쉬프트 클릭 상세정보
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                DetailPanel.Visibility = DetailPanel.Visibility == Visibility.Visible
                    ? Visibility.Collapsed
                    : Visibility.Visible;
                e.Handled = true;
            }

            // 컨트롤 클릭 주소 이동
            else if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (this.DataContext is TaskItemViewModel vm && !string.IsNullOrEmpty(vm.Url))
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo(vm.Url) { UseShellExecute = true });
                        e.Handled = true;
                    }
                    catch { }
                }
            }
            base.OnPreviewMouseLeftButtonDown(e);
        }
    }
}