using System.Text.Json;
using System.Text.Json.Nodes;

namespace NotionDeadlineFairy.Models.Filtering
{
    public static class FilterSerializer
    {
        public static string Serialize(FilterNode? node)
        {
            if (node == null) return "";
            return JsonSerializer.Serialize(ToJson(node));
        }

        public static FilterNode? Deserialize(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try
            {
                var obj = JsonNode.Parse(json);
                return FromJson(obj);
            }
            catch { return null; }
        }

        private static JsonObject ToJson(FilterNode node)
        {
            if (node is FilterGroup g)
            {
                return new JsonObject
                {
                    ["type"] = "group",
                    ["op"] = g.Op.ToString(),
                    ["children"] = new JsonArray(g.Children.Select(c => (JsonNode)ToJson(c)).ToArray())
                };
            }
            else if (node is FilterCondition c)
            {
                return new JsonObject
                {
                    ["type"] = "condition",
                    ["field"] = c.Field,
                    ["op"] = c.Op.ToString(),
                    ["value"] = c.Value
                };
            }
            return new JsonObject();
        }

        private static FilterNode? FromJson(JsonNode? node)
        {
            if (node == null) return null;
            var type = node["type"]?.GetValue<string>();

            if (type == "group")
            {
                var group = new FilterGroup
                {
                    Op = Enum.Parse<LogicOp>(node["op"]?.GetValue<string>() ?? "AND")
                };
                var children = node["children"]?.AsArray();
                if (children != null)
                    foreach (var child in children)
                    {
                        var childNode = FromJson(child);
                        if (childNode != null) group.Children.Add(childNode);
                    }
                return group;
            }
            else if (type == "condition")
            {
                return new FilterCondition
                {
                    Field = node["field"]?.GetValue<string>() ?? "",
                    Op = Enum.Parse<CondOp>(node["op"]?.GetValue<string>() ?? "Contains"),
                    Value = node["value"]?.GetValue<string>() ?? ""
                };
            }
            return null;
        }
    }
}