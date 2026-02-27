using NotionDeadlineFairy.Abstractions;
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
        public RelayCommand EditTextFilterCommand { get; }

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

            TestFetchCommand = new RelayCommand(async (arg) =>
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
                        item.ResultMessage = "Validating..";

                        var properties = await notionApi.GetDatabasePropertiesAsync(config);
                        if (!string.IsNullOrWhiteSpace(config.EndDatePropertyName))
                        {
                            var endDateProperty = properties.FirstOrDefault(x =>
                                string.Equals(x.Name, config.EndDatePropertyName, StringComparison.OrdinalIgnoreCase));

                            if (endDateProperty == null)
                            {
                                item.IsConnected = false;
                                item.ResultMessage = "EndDate Property Not Found";
                                return;
                            }

                            if (!string.Equals(endDateProperty.Type, "date", StringComparison.OrdinalIgnoreCase))
                            {
                                item.IsConnected = false;
                                item.ResultMessage = $"EndDate Property Type Invalid ({endDateProperty.Type})";
                                return;
                            }
                        }

                        var showingProperties = config.ShowingProperties
                            .Where(x => !string.IsNullOrWhiteSpace(x))
                            .Select(x => x.Trim())
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToList();

                        var invalidShowingProperties = showingProperties
                            .Where(showingProperty => !properties.Any(property =>
                                string.Equals(property.Name, showingProperty, StringComparison.OrdinalIgnoreCase)))
                            .ToList();

                        if (invalidShowingProperties.Count > 0)
                        {
                            item.IsConnected = false;
                            item.ResultMessage = $"ShowingProperties Not Found: {string.Join(", ", invalidShowingProperties)}";
                            return;
                        }

                        item.ResultMessage = "Fetching..";

                        var result = await notionApi.GetDatabaseItemsAsync(config);
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

                var views = ServiceLocator.Instance.GetService<IWidget>();
                if (views == null)
                    return;
                foreach (var view in views)
                {
                    try
                    {
                        view.Refresh();
                    }
                    catch (Exception ex)
                    {
                        // ignore
                    }
                }
            });

            EditTextFilterCommand = new RelayCommand(_ =>
            {
                // TODO ���� ���� �˾�
            });
        }
    }
}
