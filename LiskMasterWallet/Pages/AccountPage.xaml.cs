using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LiskMasterWallet.ViewModels;

namespace LiskMasterWallet.Pages
{
    /// <summary>
    ///     Interaction logic for AccountPage.xaml
    /// </summary>
    public partial class AccountPage : UserControl
    {
        public AccountPage()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var bpuri = new Uri("\\Pages\\Accounts\\Overview.xaml", UriKind.RelativeOrAbsolute);
            if (ActModernTab.SelectedSource != bpuri)
                ActModernTab.SelectedSource = bpuri;
            try
            {
                var act = (from a in Globals.AppViewModel.AccountsViewModel.Accounts
                    where a.FriendlyName == AppViewModel.SelectedAccountFriendlyName
                    select a).First();
                if (act.Address == AppViewModel.SelectedAccountFriendlyName)
                    HeaderTextBlock.Text = AppViewModel.SelectedAccountFriendlyName;
                else
                    HeaderTextBlock.Text = AppViewModel.SelectedAccountFriendlyName + "    " + act.Address;
            }
            catch (Exception crap)
            {
                Console.WriteLine("AccountPage.Loaded threw an error: " + crap.Message);
            }
        }
    }
}