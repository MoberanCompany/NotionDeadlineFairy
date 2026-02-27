using NotionDeadlineFairy.Commands;
using NotionDeadlineFairy.Models;
using NotionDeadlineFairy.Services;
using System.Collections.ObjectModel;

namespace NotionDeadlineFairy.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public ObservableCollection<NotionConfig> DatabaseConfigs { get; }

        public RelayCommand AddConfigCommand { get; }
        public RelayCommand RemoveConfigCommand { get; }
        public RelayCommand SaveCommand { get; }

        public SettingsViewModel()
        {
            var setting = SettingService.Instance.Current;
            DatabaseConfigs = new ObservableCollection<NotionConfig>(setting.DatabaseConfigs);

            AddConfigCommand = new RelayCommand(_ =>
            {
                DatabaseConfigs.Add(new NotionConfig
                {
                    ApiToken = "",
                    DatabaseUrl = ""
                });
            });

            RemoveConfigCommand = new RelayCommand(param =>
            {
                if (param is NotionConfig config)
                {
                    DatabaseConfigs.Remove(config);
                }
            });

            SaveCommand = new RelayCommand(_ =>
            {
                var current = SettingService.Instance.Current;
                current.DatabaseConfigs = [.. DatabaseConfigs];
                SettingService.Instance.Save();
            });
        }
    }
}
