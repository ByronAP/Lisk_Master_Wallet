using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Lisk.API;
using Lisk.API.Responses;
using LiskMasterWallet.Helpers;
using LiskMasterWallet.ViewModels;

namespace LiskMasterWallet.Pages.Accounts
{
    // 0.000000005m or half a satoshi is removed from the accounts calculated available balance to ensure rounding doesnt cause a problem
    public partial class Send : UserControl
    {
        public Send()
        {
            InitializeComponent();
        }

        private void SendAmountTextBox_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private static bool IsTextAllowed(string text)
        {
            var regex = new Regex("[^0-9.]+");
            return !regex.IsMatch(text);
        }

        private void SendAmountTextBox_OnPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (!e.DataObject.GetDataPresent(typeof(string))) return;
            if (!IsTextAllowed((string) e.DataObject.GetData(typeof(string))))
                e.CancelCommand();
            else e.CancelCommand();
        }

        private void AddressBookComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.AddedItems.Count <= 0)
                    return;
                var text = e.AddedItems[0].ToString().Trim();
                ToAddressTextBox.Text = string.IsNullOrEmpty(text) ? "" : text.Split('|')[1];
            }
            catch
            {
            }
        }

        private async void SendLSKButton_OnClick(object sender, RoutedEventArgs e)
        {
            var act = (from a in Globals.AppViewModel.AccountsViewModel.Accounts
                where a.FriendlyName == AppViewModel.SelectedAccountFriendlyName
                select a).First();
            var avail = act.Balance;
            decimal iamount;
            if (!decimal.TryParse(SendAmountTextBox.Text.Trim(), out iamount))
            {
                ShowNotice("Send LSK", "Please enter a valid send amount and try again.");
                return;
            }
            if (iamount + Properties.Settings.Default.SendFee > avail || iamount < 0.1m)
            {
                ShowNotice("Send LSK", "Sorry the amount specified exceeds your available balance.\r\nPlease enter a valid send amount and try again.");
                return;
            }
            //verify the toaddress is valid by checking the length and format
            if (ToAddressTextBox.Text.Trim().Length < 20 || ToAddressTextBox.Text.Trim().Length > 21 || !ToAddressTextBox.Text.Trim().EndsWith("l"))
            {
                ShowNotice("Send LSK", "Sorry the address specified does not apear to be valid.\r\nPlease check the address for errors and try again.");
                return;
            }
            transactions_send_response res;
            using (
                var avm = new AuthViewModel
                {
                    ActionDescription =
                        "Send " + iamount + " LSK from " + act.FriendlyName + " to " + ToAddressTextBox.Text.Trim()
                })
            {
                var rmpw = new AuthRequestDialog(avm);
                rmpw.ShowDialog();
                if (!avm.Accepted)
                    return;
                if (rmpw.DialogResult == null || rmpw.DialogResult == false || string.IsNullOrEmpty(avm.Password))
                    return;
                var actsec = AppHelpers.DecryptString(act.SecretHash, avm.Password);
                res = await Globals.API.Transactions_Send(actsec, (long) LiskAPI.LSKDecimalToLong(iamount),
                    ToAddressTextBox.Text.Trim(), act.PublicKey, "");
            }
            if (res == null || !res.success || string.IsNullOrEmpty(res.transactionId))
            {
                if (res != null && !string.IsNullOrEmpty(res.error))
                {
                    Console.WriteLine("Send transaction failed or did not return a transaction id, " + res.error);
                    var nd = new NoticeDialog("Send LSK Failed",
                        "Sending of " + iamount + " LSK from " + act.FriendlyName + " to " +
                        ToAddressTextBox.Text.Trim() + " failed.\r\nError: " + res.error);
                    nd.ShowDialog();
                }
                else
                {
                    Console.WriteLine("Send transaction failed or did not return a transaction id, no additional data");
                    var nd = new NoticeDialog("Send LSK Failed",
                        "Sending of " + iamount + " LSK from " + act.FriendlyName + " to " +
                        ToAddressTextBox.Text.Trim() + " failed.\r\nError: no error data available.");
                    nd.ShowDialog();
                }
            }
            else
            {
                Console.WriteLine("Send transaction id " + res.transactionId + " sent " + iamount + " LSK from " +
                                  act.FriendlyName + " to " + ToAddressTextBox.Text.Trim());
                await TransactionsViewModel.UpdateTransactions();
                await AccountsViewModel.UpdateAccount(act.Address);
                var nd = new NoticeDialog("Send LSK",
                    "Sent " + iamount + " LSK from " + act.FriendlyName + " to " + ToAddressTextBox.Text.Trim());
                nd.ShowDialog();
            }
            try
            {
                Send_OnLoaded(null, null);
            }
            catch
            {
            }
        }

        private void Send_OnLoaded(object sender, RoutedEventArgs e)
        {
            AddressBookComboBox.Items.Clear();
            var act = (from a in Globals.AppViewModel.AccountsViewModel.Accounts
                where a.FriendlyName == AppViewModel.SelectedAccountFriendlyName
                select a).First();
            var avail = act.Balance - 0.000000005m;
            if (avail < 0)
                avail = 0;
            AvailableBalanceTextBox.Text = avail.ToString("F8");
            FeeAmountTextBox.Text = Properties.Settings.Default.SendFee.ToString("F8");
            // don't bother binding this control since we may want finer control
            AddressBookComboBox.Items.Add("");
            foreach (var a in Properties.Settings.Default.AddressBookEntries)
            {
                try
                {
                    var i = a.Split('|');
                    // if the current account is in the addres book don't add it to the list
                    if (i[1] == act.Address || i[0] == act.FriendlyName)
                        continue;
                    AddressBookComboBox.Items.Add(i[0] + " | " + i[1]);
                }
                catch
                {
                    Console.WriteLine("Error parsing address book item " + a);
                }
            }
        }

        private void SendAmountTextBox_OnTextInput(object sender, TextCompositionEventArgs e)
        {
            CalcAmounts();
        }

        private void SendAmountTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            CalcAmounts();
        }

        private void CalcAmounts()
        {
            try
            {
                decimal iamount;
                if (!decimal.TryParse(SendAmountTextBox.Text.Trim(), out iamount))
                    return;
                if (iamount < 0)
                    SendAmountTextBox.Text = "0.1";
                var act = (from a in Globals.AppViewModel.AccountsViewModel.Accounts
                    where a.FriendlyName == AppViewModel.SelectedAccountFriendlyName
                    select a).First();
                AvailableBalanceTextBox.Text = (act.Balance - 0.000000005m).ToString("F8");
                var ttl = iamount + Properties.Settings.Default.SendFee;
                TotalAmountTextBox.Text = ttl.ToString("F8");
                if (ttl > act.Balance)
                {
                    var nd = new NoticeDialog("Invalid Amount", "Total amount exceeds available balance.");
                    nd.ShowDialog();
                }
            }
            catch
            {
            }
        }

        private void AvailableBalanceTextBox_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var availbal = decimal.Parse(AvailableBalanceTextBox.Text);
            var sttl = availbal - Properties.Settings.Default.SendFee;
            SendAmountTextBox.Text = sttl.ToString("F8");
        }

        private bool? ShowNotice(string title, string message)
        {
            var nd = new NoticeDialog(title, message);
            return nd.ShowDialog();
        }
    }
}