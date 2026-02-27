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
            // 컨트롤 클릭 주소 이동
            if (Keyboard.Modifiers == ModifierKeys.Control)
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