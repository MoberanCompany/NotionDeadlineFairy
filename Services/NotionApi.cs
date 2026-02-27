using NotionDeadlineFairy.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NotionDeadlineFairy.Services
{
    public class NotionApi
    {
        private static readonly Lazy<NotionApi> _instance =
            new Lazy<NotionApi>(() => new NotionApi());

        public static NotionApi Instance => _instance.Value;

        public NotionApi() { }
        public List<NotionPageData> GetDatabaseItems(string token, string databaseUrl)
        {
            return new List<NotionPageData>();
        }
    }
}
