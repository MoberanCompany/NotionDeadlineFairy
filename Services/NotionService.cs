using NotionDeadlineFairy.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NotionDeadlineFairy.Services
{
    public class NotionService
    {
        private static readonly Lazy<NotionService> _instance =
            new(() => new NotionService());

        public static NotionService Instance => _instance.Value;

        private readonly NotionApi _notionApi = NotionApi.Instance;

        public NotionService() { }

        public List<NotionPageData> GetAllDatabaseItems()
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

                var items = _notionApi.GetDatabaseItems(config);
                merged.AddRange(items);
            }

            return merged
                .OrderBy(x => x.EndAt ?? DateTime.MaxValue)
                .ToList();
        }
    }
}
