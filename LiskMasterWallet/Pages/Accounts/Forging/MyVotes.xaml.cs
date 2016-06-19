using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LiskMasterWallet.Helpers;
using LiskMasterWallet.ViewModels;

namespace LiskMasterWallet.Pages.Accounts.Forging
{
    /// <summary>
    /// Interaction logic for Votes.xaml
    /// </summary>
    public partial class MyVotes : UserControl
    {
        public MyVotes()
        {
            InitializeComponent();
        }

        private async void MyVotes_OnLoaded(object sender, RoutedEventArgs e)
        {
            var addy =
                (from a in Globals.DbContext.Accounts
                 where a.FriendlyName == AppViewModel.SelectedAccountFriendlyName
                 select a.Address).First();
            var vts = await Globals.API.Delegates_GetVotes(addy);
            var lvts = vts.delegates.Select(v => new Delegate_Class(v)).ToList();
            VotesFromMeItemsControl.ItemsSource = lvts;
        }
    }
}
