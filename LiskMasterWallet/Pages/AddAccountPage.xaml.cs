using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LiskMasterWallet.Helpers;
using LiskMasterWallet.ViewModels;

namespace LiskMasterWallet.Pages
{
    /// <summary>
    ///     Interaction logic for AddAccountPage.xaml
    /// </summary>
    public partial class AddAccountPage : UserControl
    {
        public AddAccountPage()
        {
            InitializeComponent();
        }

        private async void ModernButton_Click(object sender, RoutedEventArgs e)
        {
            AccountSecretTextBox.Text = (await Globals.API.GenerateNewSecret()).MnemonicSentence;
        }

        private async void ModernButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(AccountSecretTextBox.Text.Trim()))
                return;
            var sec = AccountSecretTextBox.Text;
            var issecvalid = await AppHelpers.IsSecretValid(sec);
            if (!issecvalid)
            {
                MessageBox.Show("Invalid account secret.\r\nplease correct and try again.", "Invalid Account Secret",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            AccountSecretTextBox.Text = "";
            var act = await Globals.API.Accounts_Open(sec);
            var fn = AccountFriendlyNameTextBox.Text.Trim();
            AccountFriendlyNameTextBox.Text = "";
            if (string.IsNullOrEmpty(fn))
                fn = !string.IsNullOrEmpty(act.account.username) ? act.account.username : act.account.address;

            var hasrecord = (from a in Globals.AppViewModel.AccountsViewModel.Accounts
                where a.Address == act.account.address || a.FriendlyName == fn
                select a).Any();
            if (hasrecord)
            {
                MessageBox.Show(
                    "A record for this account secret or friendly name already exists.\r\nPlease use a different secret or friendly name and try again",
                    "Account Record Exists",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var avm = new AuthViewModel {ActionDescription = "Add / Import new account " + fn})
            {
                var rmpw = new AuthRequestDialog(avm);
                rmpw.ShowDialog();
                if (!avm.Accepted)
                    return;
                if (rmpw.DialogResult == null || rmpw.DialogResult == false || string.IsNullOrEmpty(avm.Password))
                    return;

                var ssece = AppHelpers.EncryptString(sec, avm.Password);
                var ni = new Account
                {
                    Address = act.account.address,
                    PublicKey = act.account.publicKey,
                    FriendlyName = fn,
                    SecretHash = ssece,
                    Balance = Globals.API.LSKLongToDecimal(act.account.balance)
                };
                Globals.AppViewModel.AccountsViewModel.AddAccountAsync(ni);
                NavigationCommands.BrowseBack.Execute(null, null);
            }
        }
    }
}