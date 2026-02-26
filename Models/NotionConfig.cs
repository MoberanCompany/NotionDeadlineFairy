using System;
using System.Collections.Generic;
using System.Text;

namespace NotionDeadlineFairy.Models
{
    public class NotionConfig
    {
        public required string ApiToken { get; set; }
        public required string DatabaseUrl { get; set; }
        public string DatePropertyName { get; set; } = "";
        public string TextFilter { get; set; } = "";
    }
}
