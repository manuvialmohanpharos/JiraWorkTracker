using System.Windows;
using System.Diagnostics;
using System.Windows.Navigation;

namespace JiraWorkTracker
{
    public partial class CredentialsDialog : Window
    {
        public string Email => EmailBox.Text.Trim();
        public string ApiToken => TokenBox.Password.Trim();
        public CredentialsDialog()
        {
            InitializeComponent();
        }
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(ApiToken))
            {
                ErrorText.Text = "Email and API Token are required.";
                return;
            }
            DialogResult = true;
        }
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
