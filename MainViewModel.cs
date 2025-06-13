using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Linq;
using System.Windows.Threading;

namespace JiraWorkTracker
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string? _jiraId;
        private string _originalEstimate = "8"; // Dummy value
        private string _remainingEstimate = "5"; // Dummy value
        private DispatcherTimer? _timer;
        private WorkLogEntry? _activeEntry;
        private RelayCommand? _startWorkingCommand;
        private RelayCommand? _stopWorkingCommand;

        public string? JiraId
        {
            get => _jiraId;
            set { _jiraId = value; OnPropertyChanged(); _startWorkingCommand?.RaiseCanExecuteChanged(); OnPropertyChanged(nameof(ShouldHighlightStartWorking)); }
        }
        public string OriginalEstimate
        {
            get => _originalEstimate;
            set { _originalEstimate = value; OnPropertyChanged(); }
        }
        public string RemainingEstimate
        {
            get => _remainingEstimate;
            set { _remainingEstimate = value; OnPropertyChanged(); }
        }

        public ObservableCollection<WorkLogEntry> WorkLogs { get; } = new();

        public ICommand StartWorkingCommand => _startWorkingCommand!;
        public ICommand StopWorkingCommand => _stopWorkingCommand!;

        public bool ShouldHighlightStartWorking => CanStartWorking();

        public MainViewModel()
        {
            // Load from JSON
            var loaded = WorkLogStorageService.Load();
            foreach (var entry in loaded)
            {
                WorkLogs.Add(entry);
                SubscribeToWorkLogEntry(entry);
            }
            WorkLogs.CollectionChanged += (s, e) => {
                if (e.NewItems != null)
                {
                    foreach (WorkLogEntry entry in e.NewItems)
                        SubscribeToWorkLogEntry(entry);
                }
                if (e.OldItems != null)
                {
                    foreach (WorkLogEntry entry in e.OldItems)
                        UnsubscribeFromWorkLogEntry(entry);
                }
                _startWorkingCommand?.RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(ShouldHighlightStartWorking));
            };
            RefreshEntryCommands();
            RefreshLogCommands();
            _startWorkingCommand = new RelayCommand(_ => StartWorking(), _ => CanStartWorking());
            _stopWorkingCommand = new RelayCommand(_ => StopWorking(), _ => CanStopWorking());
        }

        private void SubscribeToWorkLogEntry(WorkLogEntry entry)
        {
            entry.PropertyChanged += WorkLogEntry_PropertyChanged;
        }
        private void UnsubscribeFromWorkLogEntry(WorkLogEntry entry)
        {
            entry.PropertyChanged -= WorkLogEntry_PropertyChanged;
        }
        private void WorkLogEntry_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WorkLogEntry.IsActive) || e.PropertyName == nameof(WorkLogEntry.StoppedAt))
            {
                _startWorkingCommand?.RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(ShouldHighlightStartWorking));
            }
        }

        private void EnsureEntryCommand(WorkLogEntry entry)
        {
            entry.StartWorkOnThisCommand = new RelayCommand(_ => StartWorkOnEntry(entry));
        }

        private void EnsureLogCommand(WorkLogEntry entry)
        {
            entry.LogCommand = new RelayCommand(_ => LogEntry(entry));
        }

        private void RefreshEntryCommands()
        {
            foreach (var entry in WorkLogs)
            {
                EnsureEntryCommand(entry);
            }
        }

        private void RefreshLogCommands()
        {
            foreach (var entry in WorkLogs)
            {
                EnsureLogCommand(entry);
            }
        }

        private void StartWorking()
        {
            if (_activeEntry != null && _activeEntry.StoppedAt == null)
                StopWorking();

            // Check if an entry with the same JiraId already exists
            var existingEntry = WorkLogs.FirstOrDefault(e => e.JiraId == JiraId);
            if (existingEntry != null)
            {
                // Move to top if not already there
                int oldIndex = WorkLogs.IndexOf(existingEntry);
                if (oldIndex > 0)
                    WorkLogs.Move(oldIndex, 0);
                // Reset timing and mark as active
                existingEntry.StartedAt = DateTime.Now;
                existingEntry.StoppedAt = null;
                existingEntry.IsActive = true;
                existingEntry.RunningTimer = "00:00";
                _activeEntry = existingEntry;
                SetActiveEntry(existingEntry);
                StartTimer();
                SaveLogs();
                OnPropertyChanged(nameof(WorkLogs));
                _stopWorkingCommand?.RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(ShouldHighlightStartWorking));
                return;
            }

            // Otherwise, create a new entry
            _activeEntry = new WorkLogEntry
            {
                JiraId = JiraId,
                StartedAt = DateTime.Now,
                RunningTimer = "00:00",
                StoppedAt = null,
                LogTime = "",
                CumulativeMinutes = 0,
                IsActive = true
            };
            EnsureEntryCommand(_activeEntry);
            EnsureLogCommand(_activeEntry);
            WorkLogs.Insert(0, _activeEntry);
            SetActiveEntry(_activeEntry);
            StartTimer();
            SaveLogs();
            OnPropertyChanged(nameof(WorkLogs));
            _stopWorkingCommand?.RaiseCanExecuteChanged();
            OnPropertyChanged(nameof(ShouldHighlightStartWorking));
        }

        private bool CanStartWorking()
        {
            if (string.IsNullOrWhiteSpace(JiraId))
                return false;
            // Disable if any entry is active and not stopped
            if (WorkLogs.Any(e => e.IsActive && e.StoppedAt == null))
                return false;
            return true;
        }

        private bool CanStopWorking()
        {
            // Enable if any entry is active and not stopped
            return WorkLogs.Any(e => e.IsActive && e.StoppedAt == null);
        }

        private void SetActiveEntry(WorkLogEntry entry)
        {
            foreach (var e in WorkLogs)
                e.IsActive = false;
            if (entry != null)
                entry.IsActive = true;
            _startWorkingCommand?.RaiseCanExecuteChanged();
            _stopWorkingCommand?.RaiseCanExecuteChanged();
            OnPropertyChanged(nameof(ShouldHighlightStartWorking));
        }

        private void StartWorkOnEntry(WorkLogEntry entry)
        {
            // If already working on this entry and it's not stopped, stop it
            if (_activeEntry == entry && _activeEntry.StoppedAt == null)
            {
                StopWorking();
                return;
            }
            if (_activeEntry != null && _activeEntry.StoppedAt == null)
                StopWorking();
            // Set the top box JiraId to the selected entry
            JiraId = entry.JiraId;
            // Move the entry to the top of the collection if not already there
            int oldIndex = WorkLogs.IndexOf(entry);
            if (oldIndex > 0)
            {
                WorkLogs.Move(oldIndex, 0);
            }
            // Do not reset CumulativeMinutes or RunningTimer, just set StartedAt and mark as active
            entry.StartedAt = DateTime.Now;
            entry.StoppedAt = null;
            entry.IsActive = true;
            _activeEntry = entry;
            SetActiveEntry(entry);
            StartTimer();
            SaveLogs();
            OnPropertyChanged(nameof(WorkLogs));
            _stopWorkingCommand?.RaiseCanExecuteChanged();
        }

        private void StartTimer()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Tick -= Timer_Tick;
            }
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
            _timer.Tick += Timer_Tick;
            _timer.Start();
            Timer_Tick(null, null);
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_activeEntry != null && _activeEntry.StoppedAt == null)
            {
                var elapsed = DateTime.Now - _activeEntry.StartedAt;
                int totalMinutes = _activeEntry.CumulativeMinutes + (int)elapsed.TotalMinutes;
                _activeEntry.RunningTimer = string.Format("{0:00}:{1:00}", totalMinutes / 60, totalMinutes % 60);
            }
        }

        private void StopWorking()
        {
            if (_activeEntry != null && _activeEntry.StoppedAt == null)
            {
                _activeEntry.StoppedAt = DateTime.Now;
                var elapsed = _activeEntry.StoppedAt.Value - _activeEntry.StartedAt;
                var minutes = Math.Max(1, (int)Math.Ceiling(elapsed.TotalMinutes));
                _activeEntry.CumulativeMinutes += minutes;
                var logSpan = TimeSpan.FromMinutes(_activeEntry.CumulativeMinutes);
                _activeEntry.RunningTimer = string.Format("{0:00}:{1:00}", (int)logSpan.TotalHours, logSpan.Minutes);
                _activeEntry.LogTime = $"{_activeEntry.CumulativeMinutes} min";
                _timer?.Stop();
                _activeEntry.IsActive = false;
                SaveLogs();
                OnPropertyChanged(nameof(WorkLogs));
                _stopWorkingCommand?.RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(ShouldHighlightStartWorking));
            }
        }

        private void LogEntry(WorkLogEntry entry)
        {
            entry.CumulativeMinutes = 0;
            entry.RunningTimer = "00:00";
            entry.LogTime = "";
            entry.StoppedAt = DateTime.Now;
            entry.IsActive = false;
            SaveLogs();
            OnPropertyChanged(nameof(WorkLogs));
        }

        private void SaveLogs()
        {
            WorkLogStorageService.Save(WorkLogs);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // Simple RelayCommand implementation
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object>? _canExecute;
        public RelayCommand(Action<object> execute, Predicate<object>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }
        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute(parameter!);
        public void Execute(object? parameter) => _execute(parameter!);
        public event EventHandler? CanExecuteChanged;
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
