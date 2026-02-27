using System;
using System.Collections.Generic;
using System.Text;

namespace NotionDeadlineFairy.Models
{
    public class NotionPageData
    {
        public string DatabaseTitle { get; set; } = "";
        public required string Url { get; set; }
        public required string Title { get; set; }
        public DateTime? EndAt { get; set; }
        public Dictionary<string, NotionField> Values { get; set; } = new();
    }
}
