using System.IO;
using System.Text.Json;

namespace JiraWorkTracker
{
    public class AppConfig
    {
        public JiraCredentials JiraCredentials { get; set; } = new JiraCredentials();
    }

    public class JiraCredentials
    {
        public string Email { get; set; } = string.Empty;
        public string ApiToken { get; set; } = string.Empty;
    }

    public static class ConfigLoader
    {
        public static AppConfig Load(string path = "config.json")
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Configuration file not found: {path}");
            var json = File.ReadAllText(path);
            var config = JsonSerializer.Deserialize<AppConfig>(json);
            if (config == null || string.IsNullOrWhiteSpace(config.JiraCredentials.Email) || string.IsNullOrWhiteSpace(config.JiraCredentials.ApiToken))
                throw new InvalidOperationException("Invalid or missing Jira credentials in config.json");
            return config;
        }
    }
}
