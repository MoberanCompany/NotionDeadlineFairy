using System;
using System.Collections.Generic;
using System.Text;

namespace NotionDeadlineFairy.Models
{
    public class NotionPage
    {
        public required string Url { get; set; }
        public required string Title { get; set; }
        public DateTime? End { get; set; }
        public string? User { get; set; }
        public string? State { get; set; }
    }
}
