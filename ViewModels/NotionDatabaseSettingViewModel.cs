using NotionDeadlineFairy.Commands;
using NotionDeadlineFairy.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace NotionDeadlineFairy.ViewModels
{
    public class NotionDatabaseSettingViewModel: BaseViewModel
    {
        private readonly NotionApi notionApi = NotionApi.Instance;
        private readonly SettingService settingService = SettingService.Instance;
        public ObservableCollection<NotionDatabaseSettingItemViewModel> SettingList { get; set; }

        public RelayCommand SaveCommand { get; }
        public RelayCommand TestFetchCommand { get; }

        public NotionDatabaseSettingViewModel()
        {
            if(settingService.Current.DatabaseConfigs.Count > 0)
            {
                var list = settingService.Current.DatabaseConfigs.Select(x => x.ToVm());
                SettingList = new ObservableCollection<NotionDatabaseSettingItemViewModel>(list);
            }
            else
            {
                SettingList = new();
            }

            SaveCommand = new RelayCommand((_) =>
            {
                var list = SettingList.Select(x => x.GetConfig()).ToList();
                settingService.Current.DatabaseConfigs.Clear();
                settingService.Current.DatabaseConfigs.AddRange(list);
                settingService.Save();
            });

            TestFetchCommand = new RelayCommand((arg) =>
            {
                if(arg is NotionDatabaseSettingItemViewModel item)
                {
                    if (string.IsNullOrEmpty(item.ApiKey))
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

                        if(result.Count ==  0)
                        {
                            message = "No Content";
                        }
                        var one = result.FirstOrDefault(x => x.EndAt != null);
                        if(one == null)
                        {
                            message = "EndDate Property Not Found";
                        }

                        item.ResultMessage = message;
                    }
                    catch(Exception ex)
                    {
                        item.IsConnected = false;
                        item.ResultMessage = ex.Message;
                    }

                }
            });


        }

    }
}
