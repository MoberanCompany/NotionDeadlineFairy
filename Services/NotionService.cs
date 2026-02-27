using NotionDeadlineFairy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotionDeadlineFairy.Services
{
    public class NotionService
    {
        private static readonly Lazy<NotionService> _instance =
            new(() => new NotionService());

        public static NotionService Instance => _instance.Value;

        private readonly NotionApi _notionApi = NotionApi.Instance;

        public NotionService() { }

        public async Task<List<NotionPageData>> GetAllDatabaseItemsAsync()
        {
            var configs = SettingService.Instance.Current.DatabaseConfigs;
            var merged = new List<NotionPageData>();

            foreach (var config in configs)
            {
                if (string.IsNullOrWhiteSpace(config.ApiToken) ||
                    string.IsNullOrWhiteSpace(config.DatabaseUrl))
                {
                    continue;
                }

                try {
                    var items = await _notionApi.GetDatabaseItemsAsync(config);
                    merged.AddRange(items);
                }
                catch(Exception ex)
                {
                    MessageBox.Show($"Error) {config.Name} DB: {ex.Message}");
                }
            }

            return merged
                .OrderBy(x => x.EndAt ?? DateTime.MaxValue)
                .ToList();
        }
    }
}
