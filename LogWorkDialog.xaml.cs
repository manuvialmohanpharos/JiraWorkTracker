using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace JiraWorkTracker
{
    public partial class LogWorkDialog : Window, INotifyPropertyChanged
    {
        public string JiraId { get; set; } = string.Empty;
        private int _minutesToLog;
        private int _initialRemainingEstimate;
        private bool _remainingEstimateManuallyEdited = false;
        private string _comment = string.Empty;
        public int MinutesToLog
        {
            get => _minutesToLog;
            set
            {
                if (_minutesToLog != value)
                {
                    _minutesToLog = value;
                    OnPropertyChanged();
                    if (!_remainingEstimateManuallyEdited)
                    {
                        RemainingEstimate = _initialRemainingEstimate - _minutesToLog;
                    }
                }
            }
        }
        private int _remainingEstimate;
        public int RemainingEstimate
        {
            get => _remainingEstimate;
            set
            {
                if (_remainingEstimate != value)
                {
                    _remainingEstimate = value;
                    _remainingEstimateManuallyEdited = true;
                    OnPropertyChanged();
                }
            }
        }
        private int _remainingAfterLog;
        public int RemainingAfterLog
        {
            get => _remainingAfterLog;
            set { _remainingAfterLog = value; OnPropertyChanged(); }
        }
        public string Comment
        {
            get => _comment;
            set { _comment = value; OnPropertyChanged(); }
        }
        public bool Confirmed { get; private set; }

        public LogWorkDialog(string jiraId, int minutesToLog, int remainingEstimate)
        {
            InitializeComponent();
            JiraId = jiraId;
            _initialRemainingEstimate = remainingEstimate;
            _minutesToLog = minutesToLog;
            _remainingEstimate = remainingEstimate - minutesToLog;
            UpdateRemainingAfterLog();
            DataContext = this;
        }

        private void UpdateRemainingAfterLog()
        {
            RemainingAfterLog = RemainingEstimate - MinutesToLog;
        }

        private void Log_Click(object sender, RoutedEventArgs e)
        {
            Confirmed = true;
            DialogResult = true;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
