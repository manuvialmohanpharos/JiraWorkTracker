using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JiraWorkTracker
{
    public class JiraService
    {
        private readonly string _baseUrl = "https://pharossystems.atlassian.net/rest/api/3/";
        private readonly string _email;
        private readonly string _apiToken;
        private readonly HttpClient _httpClient;

        public JiraService(string email, string apiToken)
        {
            _email = email ?? throw new ArgumentNullException(nameof(email));
            _apiToken = apiToken ?? throw new ArgumentNullException(nameof(apiToken));
            _httpClient = new HttpClient();
            var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_email}:{_apiToken}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string?> GetIssueSummaryAsync(string issueKey)
        {
            var url = _baseUrl + $"issue/{issueKey}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return null;
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("fields", out var fields) && fields.TryGetProperty("summary", out var summary))
                return summary.GetString();
            return null;
        }

        public async Task<bool> LogWorkAsync(string issueKey, int minutes, string comment = "")
        {
            var url = _baseUrl + $"issue/{issueKey}/worklog";
            object? commentField = null;
            if (!string.IsNullOrWhiteSpace(comment))
            {
                // Use Atlassian Document Format (ADF) for Jira Cloud
                commentField = new {
                    type = "doc",
                    version = 1,
                    content = new[] {
                        new {
                            type = "paragraph",
                            content = new[] {
                                new { type = "text", text = comment }
                            }
                        }
                    }
                };
            }
            var body = new
            {
                comment = commentField,
                timeSpentSeconds = minutes * 60
            };
            var jsonBody = JsonSerializer.Serialize(body, new JsonSerializerOptions { IgnoreNullValues = true });
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            return response.IsSuccessStatusCode;
        }

        public async Task<(string? summary, int? originalEstimateMinutes, int? remainingEstimateMinutes)> GetIssueEstimatesAsync(string issueKey)
        {
            var url = _baseUrl + $"issue/{issueKey}?fields=summary,timetracking";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return (null, null, null);
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            string? summary = null;
            int? originalEstimate = null;
            int? remainingEstimate = null;
            if (doc.RootElement.TryGetProperty("fields", out var fields))
            {
                if (fields.TryGetProperty("summary", out var summaryProp))
                    summary = summaryProp.GetString();
                if (fields.TryGetProperty("timetracking", out var tt))
                {
                    if (tt.TryGetProperty("originalEstimateSeconds", out var origSec))
                        originalEstimate = origSec.GetInt32() / 60;
                    else if (tt.TryGetProperty("originalEstimate", out var origStr) && int.TryParse(origStr.GetString(), out var origMin))
                        originalEstimate = origMin;
                    if (tt.TryGetProperty("remainingEstimateSeconds", out var remSec))
                        remainingEstimate = remSec.GetInt32() / 60;
                    else if (tt.TryGetProperty("remainingEstimate", out var remStr) && int.TryParse(remStr.GetString(), out var remMin))
                        remainingEstimate = remMin;
                }
            }
            return (summary, originalEstimate, remainingEstimate);
        }
    }
}
