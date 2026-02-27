using NotionDeadlineFairy.Models.Filtering;
using System.Linq;

namespace NotionDeadlineFairy.Services.Filtering
{
    public static class FilterEvaluator
    {
        public static bool Evaluate<TTask>(FilterNode node, TTask task, ITaskFieldExtractor<TTask> extractor)
        {
            return node switch
            {
                FilterGroup g => EvalGroup(g, task, extractor),
                FilterCondition c => EvalCond(c, task, extractor),
                _ => true
            };
        }

        private static bool EvalGroup<TTask>(FilterGroup g, TTask task, ITaskFieldExtractor<TTask> extractor)
        {
            var results = g.Children.Select(child => Evaluate(child, task, extractor));
            return g.Op == LogicOp.AND ? results.All(x => x) : results.Any(x => x);
        }

        private static bool EvalCond<TTask>(FilterCondition c, TTask task, ITaskFieldExtractor<TTask> extractor)
        {
            var keyword = (c.Value ?? "").Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(keyword)) return true;

            var allText = extractor.GetAllFieldsText(task).ToLowerInvariant();

            return c.Op switch
            {
                CondOp.Contains => allText.Contains(keyword),
                CondOp.NotContains => !allText.Contains(keyword),
                _ => true
            };
        }
    }
}