using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BigMath.Utils;
using LiskMasterWallet.Helpers;
using LiskMasterWallet.ViewModels;
using Microsoft.Win32;

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
                var nd = new NoticeDialog("Invalid Account Secret", "Invalid account secret.\r\nPlease correct and try again.");
                nd.ShowDialog();
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
                var nd = new NoticeDialog("Account Record Exists", "A record for this account secret or friendly name already exists.\r\nPlease use a different secret or friendly name and try again");
                nd.ShowDialog();
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
                    Balance = Lisk.API.LiskAPI.LSKLongToDecimal(act.account.balance)
                };
                await AccountsViewModel.AddAccountAsync(ni);
                NavigationCommands.BrowseBack.Execute(null, null);
            }
        }

        private async void BulkImportButton_OnClick(object sender, RoutedEventArgs e)
        {
            var fd = new OpenFileDialog();
            fd.Multiselect = false;
            fd.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
            var res = (bool)fd.ShowDialog();
            if (!res || string.IsNullOrEmpty(fd.FileName) || !File.Exists(fd.FileName))
                return;

            string[] secrets;
            try
            {
                secrets = File.ReadAllText(fd.FileName).Split(',');
            }
            catch (Exception)
            {
                var nd = new NoticeDialog("Bulk Account Import", "Error: CSV file could not be parsed.\r\nPlease correct the file and try again.");
                nd.ShowDialog();
                return;
            }

            using (var avm = new AuthViewModel { ActionDescription = "Bulk Import Accounts " })
            {
                var rmpw = new AuthRequestDialog(avm);
                rmpw.ShowDialog();
                if (!avm.Accepted)
                    return;
                if (rmpw.DialogResult == null || rmpw.DialogResult == false || string.IsNullOrEmpty(avm.Password))
                    return;
                int i = 0;
                foreach (var sec in secrets)
                {
                    var act = await Globals.API.Accounts_Open(sec);
                    if(!act.success)
                        continue;

                    var hasrecord = (from a in Globals.AppViewModel.AccountsViewModel.Accounts
                                     where a.Address == act.account.address
                                     select a).Any();
                    if (hasrecord)
                        continue;

                    var ssece = AppHelpers.EncryptString(sec, avm.Password);
                    var ni = new Account
                    {
                        Address = act.account.address,
                        PublicKey = act.account.publicKey,
                        FriendlyName = act.account.address,
                        SecretHash = ssece,
                        Balance = Lisk.API.LiskAPI.LSKLongToDecimal(act.account.balance),
                        LastUpdate = DateTime.UtcNow
                    };
                    try
                    {
                        await AccountsViewModel.AddAccountAsync(ni);
                        i++;
                    }
                    catch (Exception crap)
                    {
                        Console.WriteLine("Error saving imported account " + act.account.address + " | " + crap.Message);
                    }
                }
                var nd = new NoticeDialog("Bulk Account Import", "Bulk account import complete.\r\n" + i + " of " + secrets.Length + " accounts imported.");
                nd.ShowDialog();
                NavigationCommands.BrowseBack.Execute(null, null);
            }
        }
    }
}