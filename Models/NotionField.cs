using System;
using System.Collections.Generic;
using System.Text;

namespace NotionDeadlineFairy.Models
{
    public class NotionField
    {
        public required string Name { get; set; }
        public string? Value { get; set; }
        public string? Text { get; set; }
        public NotionFieldType Type { get; set; } = NotionFieldType.None;


        public enum NotionFieldType {
            None,
            Text,
            User,
            Number,
            Date,
        }
    }
}
