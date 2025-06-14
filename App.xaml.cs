using System;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using System.Text.Json;

namespace JiraWorkTracker
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);
                ShutdownMode = ShutdownMode.OnExplicitShutdown;
                SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;

                // Prompt for credentials if config.json does not exist or is invalid
                AppConfig? config = null;
                string configPath = "config.json";
                bool credentialsSet = false;

                if (!File.Exists(configPath))
                {
                    credentialsSet = PromptForCredentials(configPath, out config);
                }
                else
                {
                    try
                    {
                        config = ConfigLoader.Load(configPath);
                        credentialsSet = true;
                    }
                    catch (Exception)
                    {
                        credentialsSet = PromptForCredentials(configPath, out config);
                    }
                }

                if (!credentialsSet)
                {
                    Shutdown();
                    return;
                }

                // Always show the main window after credentials are set/loaded
                var mainWindow = new MainWindow();
                MainWindow = mainWindow;
                mainWindow.Show();
                ShutdownMode = ShutdownMode.OnMainWindowClose;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Startup exception: {ex}", "Fatal Error");
                Shutdown();
            }
        }

        private bool PromptForCredentials(string configPath, out AppConfig? config)
        {
            config = null;
            AppConfig? localConfig = null;
            var dlg = new CredentialsDialog();
            var result = dlg.ShowDialog();
            if (result == true)
            {
                localConfig = new AppConfig
                {
                    JiraCredentials = new JiraCredentials
                    {
                        Email = dlg.Email,
                        ApiToken = dlg.ApiToken
                    }
                };
                var json = JsonSerializer.Serialize(localConfig, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configPath, json);
            }
            config = localConfig;
            return config != null;
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
