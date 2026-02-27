using NotionDeadlineFairy.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NotionDeadlineFairy.Services.Filtering
{
    public class NotionPageDataFieldExtractor : ITaskFieldExtractor<NotionPageData>
    {
        public string GetFieldText(NotionPageData task, string field)
        {
            if (task == null) return "";
            if (field.Equals("Title", StringComparison.OrdinalIgnoreCase))
                return task.Title ?? "";
            if (field.Equals("DatabaseTitle", StringComparison.OrdinalIgnoreCase))
                return task.DatabaseTitle ?? "";
            if (task.Values != null)
            {
                if (task.Values.TryGetValue(field, out var nf) && nf != null)
                    return nf.Text ?? "";
                var byName = task.Values.Values
                    .FirstOrDefault(v => v?.Name != null &&
                                         v.Name.Equals(field, StringComparison.OrdinalIgnoreCase));
                if (byName != null)
                    return byName.Text ?? "";
            }
            return "";
        }

        public string GetAllFieldsText(NotionPageData task)
        {
            if (task == null) return "";
            var parts = new List<string> { task.Title ?? "", task.DatabaseTitle ?? "" };
            if (task.Values != null)
                parts.AddRange(task.Values.Values.Select(v => v?.Text ?? ""));
            return string.Join(" ", parts);
        }
    }
}