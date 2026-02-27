using NotionDeadlineFairy.Services.Filtering;
using System.Windows;

namespace NotionDeadlineFairy.Views
{
    public partial class FilterEditWindow : Window
    {
        public string ResultText { get; private set; } = "";

        public FilterEditWindow(string existing = "")
        {
            InitializeComponent();
            FilterTextBox.Text = existing;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            ResultText = FilterTextBox.Text.Trim();
            DialogResult = true;
            
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}