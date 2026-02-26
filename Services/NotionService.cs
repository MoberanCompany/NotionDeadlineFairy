using NotionDeadlineFairy.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NotionDeadlineFairy.Services
{
    public class NotionService
    {
        private static readonly Lazy<NotionService> _instance =
            new Lazy<NotionService>(() => new NotionService());

        public static NotionService Instance => _instance.Value;

        private readonly NotionApi _notionAPI = NotionApi.Instance;

        public NotionService() { }

        public List<NotionPage> GetAllDatabaseItems()
        {
            var db1 = _notionAPI.GetDatabaseItems("", "");
            var db2 = _notionAPI.GetDatabaseItems("", "");
            return db1.Concat(db2).ToList();
        }
    }
}
