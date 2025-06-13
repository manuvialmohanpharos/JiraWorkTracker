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
        private string _jiraId;
        private string _originalEstimate = "8"; // Dummy value
        private string _remainingEstimate = "5"; // Dummy value
        private DispatcherTimer _timer;
        private WorkLogEntry _activeEntry;
        private RelayCommand _startWorkingCommand;

        public string JiraId
        {
            get => _jiraId;
            set { _jiraId = value; OnPropertyChanged(); _startWorkingCommand?.RaiseCanExecuteChanged(); }
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

        public ICommand StartWorkingCommand => _startWorkingCommand;
        public ICommand StopWorkingCommand { get; }

        public MainViewModel()
        {
            // Load from JSON
            var loaded = WorkLogStorageService.Load();
            foreach (var entry in loaded)
            {
                WorkLogs.Add(entry);
            }
            RefreshEntryCommands();
            RefreshLogCommands();
            _startWorkingCommand = new RelayCommand(_ => StartWorking(), _ => CanStartWorking());
            StopWorkingCommand = new RelayCommand(_ => StopWorking(), _ => WorkLogs.Any() && WorkLogs.First().StoppedAt == null);
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
        }

        private bool CanStartWorking()
        {
            // Disable if JiraId is empty or if the top row is the active entry with the same JiraId
            if (string.IsNullOrWhiteSpace(JiraId))
                return false;
            if (WorkLogs.Count > 0 && WorkLogs[0].JiraId == JiraId && WorkLogs[0].IsActive && WorkLogs[0].StoppedAt == null)
                return false;
            return true;
        }

        private void SetActiveEntry(WorkLogEntry entry)
        {
            foreach (var e in WorkLogs)
                e.IsActive = false;
            if (entry != null)
                entry.IsActive = true;
            _startWorkingCommand?.RaiseCanExecuteChanged();
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

        private void Timer_Tick(object sender, EventArgs e)
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // Simple RelayCommand implementation
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;
        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }
        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object parameter) => _execute(parameter);
        public event EventHandler CanExecuteChanged;
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
