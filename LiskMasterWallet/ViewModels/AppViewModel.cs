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
            Globals.OnDelegate10SecondTimerTick += Globals_OnDelegate10SecondTimerTick;
            Globals.OnDelegate60SecondTimerTick += Globals_OnDelegate60SecondTimerTick;
            _statustext = "Loading...";
            _accountsViewModel.PropertyChanged += ViewModel_PropertyChanged;
            _transactionsViewModel.PropertyChanged += ViewModel_PropertyChanged;
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

        public long CurrentBlockHeight => Globals.CurrentBlockHeight;
        public string CurrentServer => Globals.API.Server_Url;
        public static string SelectedAccountFriendlyName { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(e.PropertyName);
        }

        public void RaisePropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private void Globals_OnDelegate10SecondTimerTick()
        {
            StatusText = "Blocks " + Globals.CurrentBlockHeight + "    Server " +
                         Globals.API.Server_Url.ToLower().Replace("https://", "") + "    " + TotalBalance + " LSK in " +
                         TotalAccounts + " accounts";
            RaisePropertyChanged("CurrentBlockHeight");
        }

        private async void Globals_OnDelegate60SecondTimerTick()
        {
            _accountsViewModel.UpdateAccounts();
            RaisePropertyChanged("AccountsViewModel");

            await _transactionsViewModel.UpdateTransactions();
            RaisePropertyChanged("TransactionsViewModel");
        }
    }
}