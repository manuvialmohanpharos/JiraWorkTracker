using System.Windows;

namespace JiraWorkTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // DataContext is set in XAML
            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                var stopWorking = vm.GetType().GetMethod("StopWorking", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                stopWorking?.Invoke(vm, null);
            }
        }
    }
}