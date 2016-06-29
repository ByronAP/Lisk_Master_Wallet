using System.Linq;
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
        private bool loaded;

        public AccountActivityTile()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ItemSeparator.Background = new SolidColorBrush((Color) FindResource("AccentColor"));

            if (loaded)
                return;

            var dc = (Transaction) DataContext;
            var ttype = dc.TType;
            var hassenderfn =
                (from u in Globals.DbContext.Accounts where u.Address == dc.Sender select u.FriendlyName).Any();
            var hasreceiverfn =
                (from u in Globals.DbContext.Accounts where u.Address == dc.Receiver select u.FriendlyName).Any();
            var senderfn = dc.Sender;
            var receiverfn = dc.Receiver;
            if (hassenderfn)
                senderfn =
                    (from u in Globals.DbContext.Accounts where u.Address == dc.Sender select u.FriendlyName).First();
            if (hasreceiverfn)
                receiverfn =
                    (from u in Globals.DbContext.Accounts where u.Address == dc.Receiver select u.FriendlyName).First();
            switch (ttype)
            {
                case 0:
                    if (hassenderfn)
                        DescriptionTextBox.Text = senderfn + " sent " + dc.Amount.ToString("F8") + " LSK to " +
                                                  receiverfn;
                    else if (hasreceiverfn)
                        DescriptionTextBox.Text = receiverfn + " received " + dc.Amount.ToString("F8") + " from " +
                                                  senderfn;
                    else
                        DescriptionTextBox.Text = "transfered " + dc.Amount.ToString("F8") + " LSK from " + senderfn +
                                                  " to " + receiverfn;
                    break;
                case 1:
                    DescriptionTextBox.Text = senderfn + " created new signature";
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
                                DescriptionTextBox.Text = senderfn + " registered as a delegate " +
                                                          un.@delegate.username;
                            else
                            {
                                DescriptionTextBox.Text = senderfn + " registered as a delegate";
                            }
                        }
                        else
                        {
                            DescriptionTextBox.Text = senderfn + " registered as a delegate";
                        }
                    }
                    catch
                    {
                        goto RETRY;
                    }

                    break;
                case 3:
                    DescriptionTextBox.Text = senderfn + " voted for delegates";
                    break;
                case 4:
                    DescriptionTextBox.Text = senderfn + " multi signature";
                    break;
                case 5:
                    DescriptionTextBox.Text = senderfn + " DAPP";
                    break;
                case 6:
                    DescriptionTextBox.Text = senderfn + " to " + receiverfn + " in transfer " + dc.Amount;
                    break;
                case 7:
                    DescriptionTextBox.Text = senderfn + " to " + receiverfn + " out transfer " + dc.Amount;
                    break;
                default:
                    break;
            }
            loaded = true;
        }
    }
}