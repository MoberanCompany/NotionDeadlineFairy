using NotionDeadlineFairy.Abstractions;
using NotionDeadlineFairy.Services;
using NotionDeadlineFairy.ViewModels;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace NotionDeadlineFairy.Views
{
    public partial class MainWindow : Window
    {
        private HwndSource? _hwndSource;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();

            ServiceLocator.Instance.Register<IWidget>(this);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            _hwndSource = (HwndSource)PresentationSource.FromVisual(this);
        }

        private void DragHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();


                if (this.DataContext is MainViewModel vm)
                {
                    vm.Left = this.Left;
                    vm.Top = this.Top;
                }
            }
        }
    }
}
