using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace JiraWorkTracker
{
    public static class WorkLogStorageService
    {
        private static readonly string FilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JiraWorkTracker", "worklogs.json");

        public static void Save(ObservableCollection<WorkLogEntry> workLogs)
        {
            try
            {
                var dir = Path.GetDirectoryName(FilePath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                var json = JsonSerializer.Serialize(workLogs);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save work logs: {ex}");
            }
        }

        public static ObservableCollection<WorkLogEntry> Load()
        {
            try
            {
                if (!File.Exists(FilePath))
                    return new ObservableCollection<WorkLogEntry>();
                var json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<ObservableCollection<WorkLogEntry>>(json) ?? new ObservableCollection<WorkLogEntry>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load work logs: {ex}");
                return new ObservableCollection<WorkLogEntry>();
            }
        }
    }
}
