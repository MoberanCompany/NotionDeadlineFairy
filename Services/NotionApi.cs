using NotionDeadlineFairy.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace NotionDeadlineFairy.Services
{
    public class NotionApi
    {
        private const string NotionVersion = "2022-06-28";
        private static readonly HttpClient _httpClient = new()
        {
            BaseAddress = new Uri("https://api.notion.com/v1/")
        };

        private static readonly Lazy<NotionApi> _instance =
            new(() => new NotionApi());

        public static NotionApi Instance => _instance.Value;

        public NotionApi() { }

        public List<NotionPageData> GetDatabaseItems(NotionConfig config)
        {
            ArgumentNullException.ThrowIfNull(config);

            if (string.IsNullOrWhiteSpace(config.ApiToken))
            {
                throw new ArgumentException("Notion API token is required.", nameof(config));
            }

            var databaseId = ParseDatabaseId(config.DatabaseUrl);
            if (string.IsNullOrWhiteSpace(databaseId))
            {
                throw new ArgumentException("Invalid Notion database URL.", nameof(config));
            }

            var items = new List<NotionPageData>();
            string? nextCursor = null;

            do
            {
                var requestBody = nextCursor is null
                    ? "{}"
                    : JsonSerializer.Serialize(new Dictionary<string, string> { ["start_cursor"] = nextCursor });

                using var request = new HttpRequestMessage(HttpMethod.Post, $"databases/{databaseId}/query");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", config.ApiToken);
                request.Headers.Add("Notion-Version", NotionVersion);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                using var response = _httpClient.Send(request);
                var payload = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"Notion API request failed ({(int)response.StatusCode}): {payload}");
                }

                using var doc = JsonDocument.Parse(payload);
                var root = doc.RootElement;

                if (!root.TryGetProperty("results", out var resultsElement) ||
                    resultsElement.ValueKind != JsonValueKind.Array)
                {
                    break;
                }

                foreach (var pageElement in resultsElement.EnumerateArray())
                {
                    if (!pageElement.TryGetProperty("properties", out var propertiesElement) ||
                        propertiesElement.ValueKind != JsonValueKind.Object)
                    {
                        continue;
                    }

                    var values = new Dictionary<string, NotionField>(StringComparer.OrdinalIgnoreCase);
                    foreach (var property in propertiesElement.EnumerateObject())
                    {
                        values[property.Name] = ParseField(property.Name, property.Value);
                    }

                    var title = ExtractTitle(propertiesElement);
                    if (string.IsNullOrWhiteSpace(title))
                    {
                        title = "(Untitled)";
                    }

                    DateTime? endAt = null;
                    if (!string.IsNullOrWhiteSpace(config.EndDatePropertyName) &&
                        propertiesElement.TryGetProperty(config.EndDatePropertyName, out var endProperty))
                    {
                        endAt = ExtractDateTime(endProperty);
                    }

                    items.Add(new NotionPageData
                    {
                        DatabaseTitle = config.Name ?? string.Empty,
                        Url = pageElement.TryGetProperty("url", out var urlElement) ? (urlElement.GetString() ?? string.Empty) : string.Empty,
                        Title = title,
                        EndAt = endAt,
                        Values = values,
                    });
                }

                nextCursor = root.TryGetProperty("next_cursor", out var nextCursorElement)
                    ? nextCursorElement.GetString()
                    : null;
            }
            while (!string.IsNullOrWhiteSpace(nextCursor));

            return items;
        }

        private static string ParseDatabaseId(string databaseUrl)
        {
            if (string.IsNullOrWhiteSpace(databaseUrl))
            {
                return string.Empty;
            }

            var match = Regex.Match(
                databaseUrl,
                @"([0-9a-fA-F]{8}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{12}|[0-9a-fA-F]{32})");

            if (!match.Success)
            {
                return string.Empty;
            }

            var compact = match.Groups[1].Value.Replace("-", string.Empty, StringComparison.Ordinal);
            if (compact.Length != 32)
            {
                return string.Empty;
            }

            return $"{compact[..8]}-{compact.Substring(8, 4)}-{compact.Substring(12, 4)}-{compact.Substring(16, 4)}-{compact.Substring(20, 12)}";
        }

        private static string ExtractTitle(JsonElement propertiesElement)
        {
            foreach (var property in propertiesElement.EnumerateObject())
            {
                if (property.Value.TryGetProperty("type", out var typeElement) &&
                    typeElement.GetString() == "title")
                {
                    var text = ExtractRichText(property.Value, "title");
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        return text;
                    }
                }
            }

            return string.Empty;
        }

        private static NotionField ParseField(string name, JsonElement property)
        {
            var field = new NotionField { Name = name };
            var propertyType = property.TryGetProperty("type", out var typeElement)
                ? typeElement.GetString() ?? string.Empty
                : string.Empty;

            switch (propertyType)
            {
                case "title":
                    field.Type = NotionField.NotionFieldType.Text;
                    field.Text = ExtractRichText(property, "title");
                    field.Value = field.Text;
                    break;
                case "rich_text":
                    field.Type = NotionField.NotionFieldType.Text;
                    field.Text = ExtractRichText(property, "rich_text");
                    field.Value = field.Text;
                    break;
                case "number":
                    field.Type = NotionField.NotionFieldType.Number;
                    if (property.TryGetProperty("number", out var numberElement) &&
                        numberElement.ValueKind != JsonValueKind.Null)
                    {
                        field.Value = numberElement.GetDouble().ToString(CultureInfo.InvariantCulture);
                    }
                    break;
                case "date":
                    field.Type = NotionField.NotionFieldType.Date;
                    field.Value = ExtractDateString(property);
                    break;
                case "people":
                    field.Type = NotionField.NotionFieldType.User;
                    field.Text = ExtractPeople(property);
                    field.Value = field.Text;
                    break;
                default:
                    field.Type = NotionField.NotionFieldType.Text;
                    field.Text = ExtractGenericText(propertyType, property);
                    field.Value = field.Text;
                    break;
            }

            return field;
        }

        private static string ExtractRichText(JsonElement property, string elementName)
        {
            if (!property.TryGetProperty(elementName, out var textArray) ||
                textArray.ValueKind != JsonValueKind.Array)
            {
                return string.Empty;
            }

            var chunks = new List<string>();
            foreach (var textElement in textArray.EnumerateArray())
            {
                if (textElement.TryGetProperty("plain_text", out var plainTextElement))
                {
                    var plainText = plainTextElement.GetString();
                    if (!string.IsNullOrWhiteSpace(plainText))
                    {
                        chunks.Add(plainText);
                    }
                }
            }

            return string.Join(" ", chunks);
        }

        private static string ExtractDateString(JsonElement property)
        {
            if (!property.TryGetProperty("date", out var dateElement) ||
                dateElement.ValueKind != JsonValueKind.Object)
            {
                return string.Empty;
            }

            if (!dateElement.TryGetProperty("end", out var endElement))
            {
                return string.Empty;
            }
            else
            {
                if (!dateElement.TryGetProperty("start", out endElement))
                {
                    return string.Empty;
                }

            }

            return endElement.GetString() ?? string.Empty;
        }

        private static DateTime? ExtractDateTime(JsonElement property)
        {
            var dateText = ExtractDateString(property);
            if (string.IsNullOrWhiteSpace(dateText))
            {
                return null;
            }

            if (DateTimeOffset.TryParse(dateText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dto))
            {
                return dto.LocalDateTime;
            }

            if (DateTime.TryParse(dateText, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            {
                return dt;
            }

            return null;
        }

        private static string ExtractPeople(JsonElement property)
        {
            if (!property.TryGetProperty("people", out var peopleElement) ||
                peopleElement.ValueKind != JsonValueKind.Array)
            {
                return string.Empty;
            }

            var names = new List<string>();
            foreach (var person in peopleElement.EnumerateArray())
            {
                if (person.TryGetProperty("name", out var nameElement))
                {
                    var name = nameElement.GetString();
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        names.Add(name);
                    }
                }
            }

            return string.Join(", ", names);
        }

        private static string ExtractGenericText(string propertyType, JsonElement property)
        {
            switch (propertyType)
            {
                case "select":
                case "status":
                    if (property.TryGetProperty(propertyType, out var optionElement) &&
                        optionElement.ValueKind == JsonValueKind.Object &&
                        optionElement.TryGetProperty("name", out var optionName))
                    {
                        return optionName.GetString() ?? string.Empty;
                    }
                    break;
                case "multi_select":
                    if (property.TryGetProperty("multi_select", out var multiElement) &&
                        multiElement.ValueKind == JsonValueKind.Array)
                    {
                        var names = new List<string>();
                        foreach (var item in multiElement.EnumerateArray())
                        {
                            if (item.TryGetProperty("name", out var nameElement))
                            {
                                var name = nameElement.GetString();
                                if (!string.IsNullOrWhiteSpace(name))
                                {
                                    names.Add(name);
                                }
                            }
                        }

                        return string.Join(", ", names);
                    }
                    break;
                case "checkbox":
                    if (property.TryGetProperty("checkbox", out var checkboxElement) &&
                        checkboxElement.ValueKind is JsonValueKind.True or JsonValueKind.False)
                    {
                        return checkboxElement.GetBoolean().ToString();
                    }
                    break;
                case "url":
                case "email":
                case "phone_number":
                    if (property.TryGetProperty(propertyType, out var textValue))
                    {
                        return textValue.GetString() ?? string.Empty;
                    }
                    break;
            }

            return string.Empty;
        }
    }
}
