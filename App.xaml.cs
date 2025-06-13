using System;
using System.Windows;
using Microsoft.Win32;

namespace JiraWorkTracker
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
        }

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionLock || e.Reason == SessionSwitchReason.SessionLogoff)
            {
                // Try to stop working if MainWindow is available
                if (Current.MainWindow?.DataContext is MainViewModel vm)
                {
                    var stopWorking = vm.GetType().GetMethod("StopWorking", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    stopWorking?.Invoke(vm, null);
                }
            }
            else if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                // Show the main window when user unlocks the workstation
                Current.Dispatcher.Invoke(() => {
                    if (Current.MainWindow != null)
                    {
                        Current.MainWindow.Show();
                        Current.MainWindow.WindowState = WindowState.Normal;
                        Current.MainWindow.Activate();
                    }
                });
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
            base.OnExit(e);
        }
    }
}
