using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LiskMasterWallet.ViewModels;

namespace LiskMasterWallet.Pages.Accounts
{
    /// <summary>
    ///     Interaction logic for History.xaml
    /// </summary>
    public partial class Overview : UserControl
    {
        public Overview()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = (from a in Globals.AppViewModel.AccountsViewModel.Accounts
                where a.FriendlyName == AppViewModel.SelectedAccountFriendlyName
                select a).First();
            Console.WriteLine(DataContext);
        }
    }
}