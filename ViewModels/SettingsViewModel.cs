using NotionDeadlineFairy.Commands;
using NotionDeadlineFairy.Models;
using NotionDeadlineFairy.Services;
using System.Collections.ObjectModel;

namespace NotionDeadlineFairy.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public ObservableCollection<NotionDatabaseSettingItemViewModel> DatabaseConfigs { get; set; }

        private readonly NotionApi notionApi = NotionApi.Instance;
        private readonly SettingService settingService = SettingService.Instance;

        public RelayCommand SaveCommand { get; }
        public RelayCommand TestFetchCommand { get; }
        public RelayCommand AddConfigCommand { get; }
        public RelayCommand RemoveConfigCommand { get; }

        public SettingsViewModel()
        {
            var setting = SettingService.Instance.Current;

            if (settingService.Current.DatabaseConfigs.Count > 0)
            {
                var list = settingService.Current.DatabaseConfigs.Select(x => x.ToVm());
                DatabaseConfigs = new ObservableCollection<NotionDatabaseSettingItemViewModel>(list);
            }
            else
            {
                DatabaseConfigs = new();

            }

            AddConfigCommand = new RelayCommand(_ =>
            {
                DatabaseConfigs.Add(new NotionDatabaseSettingItemViewModel
                {
                    Name = "",
                    ApiToken = "",
                    DatabaseUrl = ""
                });
            });

            TestFetchCommand = new RelayCommand((arg) =>
            {
                if (arg is NotionDatabaseSettingItemViewModel item)
                {
                    if (string.IsNullOrEmpty(item.ApiToken))
                    {
                        item.ResultMessage = "ApiKey Required";
                        return;
                    }

                    if (string.IsNullOrEmpty(item.DatabaseUrl))
                    {
                        item.ResultMessage = "DatabaseUrl Required";
                        return;
                    }

                    var config = item.GetConfig();
                    try
                    {
                        item.ResultMessage = "Fetching..";

                        var result = notionApi.GetDatabaseItems(config);
                        item.IsConnected = true;

                        var message = "OK";

                        if (result.Count == 0)
                        {
                            message = "No Content";
                        }
                        var one = result.FirstOrDefault(x => x.EndAt != null);
                        if (one == null)
                        {
                            message = "EndDate Property Not Found";
                        }

                        item.ResultMessage = message;
                    }
                    catch (Exception ex)
                    {
                        item.IsConnected = false;
                        item.ResultMessage = ex.Message;
                    }

                }
            });


            RemoveConfigCommand = new RelayCommand(param =>
            {
                if (param is NotionDatabaseSettingItemViewModel config)
                {
                    DatabaseConfigs.Remove(config);
                }
            });

            SaveCommand = new RelayCommand(_ =>
            {
                var current = SettingService.Instance.Current;
                current.DatabaseConfigs = DatabaseConfigs.Select(x => x.GetConfig()).ToList();
                SettingService.Instance.Save();
            });
        }
    }
}
