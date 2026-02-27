using NotionDeadlineFairy.ViewModels;
using System.Windows;
using System.Windows.Input;


namespace NotionDeadlineFairy.Views
{
    /// <summary>
    /// Window1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WindowControlWindow : Window
    {
        public WindowControlWindow()
        {
            InitializeComponent();
        }

        private void DragHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();

                if(this.DataContext is WindowControlWindowViewModel vm)
                {
                    vm.Left = this.Left;
                    vm.Top = this.Top;
                }
            }
        }

    }
}
