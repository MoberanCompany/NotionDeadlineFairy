using System.Collections.Generic;

namespace NotionDeadlineFairy.Models.Filtering
{
    public enum LogicOp { AND, OR }
    public enum CondOp { Contains, NotContains }

    public abstract class FilterNode { }

    public sealed class FilterGroup : FilterNode
    {
        public LogicOp Op { get; set; } = LogicOp.AND;
        public List<FilterNode> Children { get; } = new();
    }

    public sealed class FilterCondition : FilterNode
    {
        public string Field { get; set; } = "";  
        public CondOp Op { get; set; } = CondOp.Contains;
        public string Value { get; set; } = "";  
    }
}