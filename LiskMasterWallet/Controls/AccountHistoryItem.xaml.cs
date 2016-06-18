using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LiskMasterWallet.Controls
{
    /// <summary>
    /// Interaction logic for AccountHistoryItem.xaml
    /// </summary>
    public partial class AccountHistoryItem : UserControl
    {
        private bool loaded = false;
        public AccountHistoryItem()
        {
            InitializeComponent();
        }

        private void AccountHistoryItem_OnLoaded(object sender, RoutedEventArgs e)
        {
            ItemSeparator.Background = new SolidColorBrush((Color)FindResource("AccentColor"));
            if (loaded)
                return;
        }
    }
}
