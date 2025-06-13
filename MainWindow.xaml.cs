using System.Windows;
using System;
using System.Windows.Forms; // Correct namespace for NotifyIcon
using System.Threading.Tasks;

namespace JiraWorkTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.NotifyIcon? _notifyIcon;
        private bool _isExit;

        public MainWindow()
        {
            InitializeComponent();
            // DataContext is set in XAML
            this.Closing += MainWindow_Closing;
            this.StateChanged += MainWindow_StateChanged;
            SetupTrayIcon();
        }

        private void SetupTrayIcon()
        {
            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location);
            _notifyIcon.Visible = true;
            _notifyIcon.Text = "Jira Work Tracker";
            _notifyIcon.DoubleClick += (s, e) => ShowMainWindow();
            var menu = new System.Windows.Forms.ContextMenuStrip();
            menu.Items.Add("Open", null, (s, e) => ShowMainWindow());
            menu.Items.Add("Exit", null, (s, e) => { _isExit = true; Close(); });
            _notifyIcon.ContextMenuStrip = menu;
        }

        private void MainWindow_StateChanged(object? sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }
        }

        private void ShowMainWindow()
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_isExit)
            {
                e.Cancel = true;
                Hide();
                // Do NOT stop working when hiding to tray
                return;
            }
            else
            {
                _notifyIcon?.Dispose();
                // Only stop working when actually exiting
                if (DataContext is MainViewModel vm)
                {
                    var stopWorking = vm.GetType().GetMethod("StopWorking", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    stopWorking?.Invoke(vm, null);
                }
            }
        }

        public async Task<(bool confirmed, int minutesToLog, int remainingEstimate, string comment)> ShowLogWorkDialogAsync(string jiraId, int minutesToLog, int remainingEstimate)
        {
            var dialog = new LogWorkDialog(jiraId, minutesToLog, remainingEstimate)
            {
                Owner = this
            };
            var result = dialog.ShowDialog();
            return (result == true && dialog.Confirmed, dialog.MinutesToLog, dialog.RemainingEstimate, dialog.Comment);
        }
    }
}