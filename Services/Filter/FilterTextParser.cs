using NotionDeadlineFairy.Models.Filtering;

namespace NotionDeadlineFairy.Services.Filtering
{
    
    public static class FilterTextParser
    {
        public static FilterNode Parse(string input)
        {
            var tokens = Tokenize(input.Trim());
            var pos = 0;
            return ParseOr(tokens, ref pos);
        }

        // OR 파싱 (우선순위 낮음)
        private static FilterNode ParseOr(List<string> tokens, ref int pos)
        {
            var left = ParseAnd(tokens, ref pos);
            while (pos < tokens.Count && tokens[pos].ToUpper() == "OR")
            {
                pos++;
                var right = ParseAnd(tokens, ref pos);
                var group = new FilterGroup { Op = LogicOp.OR };
                group.Children.Add(left);
                group.Children.Add(right);
                left = group;
            }
            return left;
        }

        // AND 파싱 (우선순위 높음)
        private static FilterNode ParseAnd(List<string> tokens, ref int pos)
        {
            var left = ParsePrimary(tokens, ref pos);
            while (pos < tokens.Count && tokens[pos].ToUpper() == "AND")
            {
                pos++;
                var right = ParsePrimary(tokens, ref pos);
                var group = new FilterGroup { Op = LogicOp.AND };
                group.Children.Add(left);
                group.Children.Add(right);
                left = group;
            }
            return left;
        }

        // 괄호 or 단일 키워드
        private static FilterNode ParsePrimary(List<string> tokens, ref int pos)
        {
            if (pos >= tokens.Count)
                return new FilterCondition { Op = CondOp.Contains, Value = "" };

            if (tokens[pos] == "(")
            {
                pos++; // ( 소비
                var node = ParseOr(tokens, ref pos);
                if (pos < tokens.Count && tokens[pos] == ")") pos++; // ) 소비
                return node;
            }

            // NOT 처리
            if (tokens[pos].ToUpper() == "NOT")
            {
                pos++;
                var keyword = pos < tokens.Count ? tokens[pos++] : "";
                return new FilterCondition { Op = CondOp.NotContains, Value = keyword };
            }

            return new FilterCondition { Op = CondOp.Contains, Value = tokens[pos++] };
        }

        private static List<string> Tokenize(string input)
        {
            var tokens = new List<string>();
            var i = 0;
            while (i < input.Length)
            {
                if (char.IsWhiteSpace(input[i])) { i++; continue; }

                if (input[i] == '(') { tokens.Add("("); i++; continue; }
                if (input[i] == ')') { tokens.Add(")"); i++; continue; }

                // 단어 읽기
                var start = i;
                while (i < input.Length && !char.IsWhiteSpace(input[i])
                       && input[i] != '(' && input[i] != ')')
                    i++;

                tokens.Add(input[start..i]);
            }
            return tokens;
        }
    }
}