using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LiskMasterWallet.ViewModels;

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

        private async void AccountHistoryItem_OnLoaded(object sender, RoutedEventArgs e)
        {
            ItemSeparator.Background = new SolidColorBrush((Color)FindResource("AccentColor"));
            if (loaded)
                return;
            var dc = (Transaction)DataContext;
            var ttype = dc.TType;
            var hassenderfn = (from u in Globals.DbContext.Accounts where u.Address == dc.Sender select u.FriendlyName).Any();
            var hasreceiverfn = (from u in Globals.DbContext.Accounts where u.Address == dc.Receiver select u.FriendlyName).Any();
            var senderfn = dc.Sender;
            var receiverfn = dc.Receiver;
            if (hassenderfn)
                senderfn = (from u in Globals.DbContext.Accounts where u.Address == dc.Sender select u.FriendlyName).First();
            if (hasreceiverfn)
                receiverfn = (from u in Globals.DbContext.Accounts where u.Address == dc.Receiver select u.FriendlyName).First();
            var issender = false || AppViewModel.SelectedAccountFriendlyName == senderfn;
            switch (ttype)
            {
                case 0:
                    if (issender)
                    {
                        DescriptionTextBox.Text = "Sent " + dc.Amount.ToString("F8") + " LSK to " + receiverfn;
                        ItemImage.Source = (ImageSource)FindResource("AppbarMinus");
                    }
                    else
                    {
                        DescriptionTextBox.Text = "Received " + dc.Amount.ToString("F8") + " LSK from " + senderfn;
                        ItemImage.Source = (ImageSource)FindResource("AppbarAdd");
                    }
                    break;
                case 1:
                    DescriptionTextBox.Text = "Created new signature";
                    ItemImage.Source = (ImageSource)FindResource("AppbarPenAdd");
                    break;
                case 2:
                    RETRY:
                    try
                    {
                        var pk = await Globals.API.Accounts_GetAccount(dc.Sender);
                        if (pk != null && pk.success && pk.account != null &&
                            !string.IsNullOrEmpty(pk.account.publicKey))
                        {
                            var un = await Globals.API.Delegates_Get(pk.account.publicKey);
                            if (un != null && un.success && un.@delegate != null &&
                                !string.IsNullOrEmpty(un.@delegate.username))
                                DescriptionTextBox.Text = "Registered as a delegate " + un.@delegate.username;
                            else
                            {
                                DescriptionTextBox.Text = "Registered as a delegate";
                            }
                        }
                        else
                        {
                            DescriptionTextBox.Text = "Registered as a delegate";
                        }
                    }
                    catch
                    {
                        goto RETRY;
                    }
                    ItemImage.Source = (ImageSource)FindResource("AppbarLink");
                    break;
                case 3:
                    DescriptionTextBox.Text = "Voted for delegates";
                    ItemImage.Source = (ImageSource)FindResource("AppbarCheckmarkPencilTop");
                    break;
                case 4:
                    DescriptionTextBox.Text = "Multi signature";
                    ItemImage.Source = (ImageSource)FindResource("AppbarPen");
                    break;
                case 5:
                    DescriptionTextBox.Text = "DAPP";
                    ItemImage.Source = (ImageSource)FindResource("AppbarCogs");
                    break;
                case 6:
                    DescriptionTextBox.Text = "Transfer in from"  + senderfn + " amount " + dc.Amount;
                    ItemImage.Source = (ImageSource)FindResource("AppbarAdd");
                    break;
                case 7:
                    DescriptionTextBox.Text = "Transfer out to " + receiverfn + " amount " + dc.Amount;
                    ItemImage.Source = (ImageSource)FindResource("AppbarMinus");
                    break;
                default:
                    break;
            }
            loaded = true;
        }
    }
}
