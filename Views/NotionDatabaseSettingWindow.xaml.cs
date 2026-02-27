using NotionDeadlineFairy.ViewModels;
using System.Windows;
using UserControl = System.Windows.Controls.UserControl;

namespace NotionDeadlineFairy.Views
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class NotionDatabaseSettingWindow : Window
    {
        public NotionDatabaseSettingWindow()
        {
            InitializeComponent();
            this.DataContext = new NotionDatabaseSettingViewModel();
        }
    }
}
