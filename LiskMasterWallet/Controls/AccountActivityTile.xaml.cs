using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LiskMasterWallet.Controls
{
    /// <summary>
    ///     Interaction logic for AccountActivityTile.xaml
    /// </summary>
    public partial class AccountActivityTile : UserControl
    {
        public AccountActivityTile()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ItemSeparator.Background = new SolidColorBrush((Color) FindResource("AccentColor"));
        }
    }
}