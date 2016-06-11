using System.Windows;
using System.Windows.Controls;
using Lisk.API;
using LiskMasterWallet.Helpers;

namespace LiskMasterWallet.Pages.Settings
{
    /// <summary>
    ///     Interaction logic for Appearance.xaml
    /// </summary>
    public partial class General : UserControl
    {
        public General()
        {
            InitializeComponent();

            // create and assign the appearance view model
            DataContext = Properties.Settings.Default;
            UpdateServersListView();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private async void AddServerButton_Click(object sender, RoutedEventArgs e)
        {
            var serverurl = NewServerTextBox.Text.ToLower().Trim();
            if (string.IsNullOrEmpty(serverurl))
                return;
            if (!serverurl.StartsWith("https://"))
            {
                MessageBox.Show(
                    "Error: Server must start with \"https://\".\r\nPlease correct the address and try again.");
                return;
            }
            var pingtime = AppHelpers.GetServerResponseTime(serverurl);
            if (pingtime <= 0 || pingtime > 120)
            {
                MessageBox.Show("Error: Server connection failed or did not respond fast enough (" + pingtime +
                                " ms).\r\nPlease try a different server.");
                return;
            }

            try
            {
                var _api = new LiskAPI(serverurl);
                var ss = await _api.Loader_Status();
                if (!ss.loaded)
                {
                    MessageBox.Show("Error: Server failed sync status test.\r\nPlease try a different server.");
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Error: Server failed api test.\r\nPlease try a different server.");
                return;
            }

            Properties.Settings.Default.Servers.Add(serverurl);
            Properties.Settings.Default.Save();
            NewServerTextBox.Text = "";
            UpdateServersListView();
        }

        private void UpdateServersListView()
        {
            ServersListView.Items.Clear();
            foreach (var s in Properties.Settings.Default.Servers)
            {
                ServersListView.Items.Add(s);
            }
        }

        private void SaveRestartButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
            App.ForceRestart();
        }
    }
}