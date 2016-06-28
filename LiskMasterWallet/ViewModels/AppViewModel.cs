using System.ComponentModel;
using System.Linq;

namespace LiskMasterWallet.ViewModels
{
    public class AppViewModel : INotifyPropertyChanged
    {
        private AccountsViewModel _accountsViewModel;
        private string _statustext;
        private TransactionsViewModel _transactionsViewModel;

        public AppViewModel()
        {
            _accountsViewModel = new AccountsViewModel();
            _transactionsViewModel = new TransactionsViewModel();
            Globals.OnNewBlockReceived += Globals_OnNewBlockReceived;
            _statustext = "Loading...";
            _accountsViewModel.PropertyChanged += ViewModel_PropertyChanged;
            _transactionsViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void Globals_OnNewBlockReceived(Lisk.API.Responses.Block_Object block)
        {
            StatusText = "Blocks " + Globals.CurrentBlockHeight + "    Server " +
                         Globals.API.Server_Url.ToLower().Replace("https://", "") + "    " + TotalBalance + " LSK in " +
                         TotalAccounts + " accounts";
            RaisePropertyChanged("CurrentBlockHeight");
        }

        public AccountsViewModel AccountsViewModel
        {
            get { return _accountsViewModel; }
            set
            {
                _accountsViewModel = value;
                RaisePropertyChanged("AccountsViewModel.Accounts");
                RaisePropertyChanged("Accounts");
                RaisePropertyChanged("AccountsViewModel");
            }
        }

        public TransactionsViewModel TransactionsViewModel
        {
            get { return _transactionsViewModel; }
            set
            {
                _transactionsViewModel = value;
                RaisePropertyChanged("TransactionsViewModel.RecentTransactions");
                RaisePropertyChanged("TransactionsViewModel.Transactions");
                RaisePropertyChanged("Transactions");
                RaisePropertyChanged("TransactionsViewModel");
            }
        }

        public string StatusText
        {
            get { return _statustext; }
            set
            {
                _statustext = value;
                RaisePropertyChanged("StatusText");
            }
        }

        public decimal TotalBalance
        {
            get
            {
                try
                {
                    return (from b in Globals.DbContext.Accounts select b.Balance).Sum();
                }
                catch
                {
                    return 0m;
                }
            }
        }

        public int TotalAccounts
        {
            get
            {
                try
                {
                    return (from a in Globals.DbContext.Accounts select a).Count();
                }
                catch
                {
                    return 0;
                }
            }
        }

        public long CurrentBlockHeight
        {
            get { return Globals.CurrentBlockHeight; }
        }

        public string CurrentServer
        {
            get { return Globals.API.Server_Url; }
        }

        public static string SelectedAccountFriendlyName { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(e.PropertyName);
        }

        public void RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}