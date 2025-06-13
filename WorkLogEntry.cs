using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Text.Json.Serialization;

namespace JiraWorkTracker
{
    public class WorkLogEntry : INotifyPropertyChanged
    {
        private string? _jiraId;
        private DateTime _startedAt;
        private string? _runningTimer;
        private DateTime? _stoppedAt;
        private string? _logTime;
        private int _cumulativeMinutes;
        private bool _isActive;

        [JsonIgnore]
        public ICommand? StartWorkOnThisCommand { get; set; } // Set by ViewModel
        [JsonIgnore]
        public ICommand? LogCommand { get; set; } // Set by ViewModel

        [JsonIgnore]
        public bool IsActive
        {
            get => _isActive;
            set { _isActive = value; OnPropertyChanged(); OnPropertyChanged(nameof(JiraIdToolTip)); }
        }

        [JsonIgnore]
        public string JiraIdToolTip => IsActive && StoppedAt == null ? "Stop Working" : "Start working";

        public int CumulativeMinutes
        {
            get => _cumulativeMinutes;
            set { _cumulativeMinutes = value; OnPropertyChanged(); OnPropertyChanged(nameof(RunningTimer)); }
        }

        public string? JiraId
        {
            get => _jiraId;
            set { _jiraId = value; OnPropertyChanged(); }
        }
        public DateTime StartedAt
        {
            get => _startedAt;
            set { _startedAt = value; OnPropertyChanged(); }
        }
        public string? RunningTimer
        {
            get => _runningTimer;
            set { _runningTimer = value; OnPropertyChanged(); }
        }
        public DateTime? StoppedAt
        {
            get => _stoppedAt;
            set { _stoppedAt = value; OnPropertyChanged(); OnPropertyChanged(nameof(JiraIdToolTip)); }
        }
        public string? LogTime
        {
            get => _logTime;
            set { _logTime = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
