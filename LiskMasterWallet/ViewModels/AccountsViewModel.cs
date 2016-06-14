using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace LiskMasterWallet.ViewModels
{
    public class AccountsViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Account> _accounts;

        public AccountsViewModel()
        {
            _accounts = new ObservableCollection<Account>(Globals.DbContext.Accounts);
        }

        public ObservableCollection<Account> Accounts
        {
            get { return _accounts; }
            set
            {
                _accounts = value;
                RaisePropertyChanged("Accounts");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public async Task UpdateAccounts()
        {
            foreach (var a in Accounts)
            {
                try
                {
                    var actinf = await Globals.API.Accounts_GetAccount(a.Address);
                    if (actinf?.account == null || !actinf.success)
                        continue;
                    a.Balance = Lisk.API.LiskAPI.LSKLongToDecimal(actinf.account.balance);
                    if (!string.IsNullOrEmpty(actinf.account.username))
                        a.FriendlyName = actinf.account.username;
                    //a.FriendlyName = DateTime.Now.ToString();
                    await Globals.DbContext.SaveChangesAsync();

                    RaisePropertyChanged("Accounts");
                    RaisePropertyChanged("TotalBalance");
                }
                catch (Exception crap)
                {
                    Console.WriteLine("AccountsViewModel.UpdateAccounts threw an error: " + crap.Message);
                }
            }
        }

        public async Task AddAccountAsync(Account account)
        {
            Globals.DbContext.Accounts.Add(account);
            await Globals.DbContext.SaveChangesAsync();
            Accounts = new ObservableCollection<Account>(Globals.DbContext.Accounts);
            RaisePropertyChanged("Accounts");
            RaisePropertyChanged("TotalBalance");
            RaisePropertyChanged("TotalAccounts");
        }

        public async Task RemoveAccountAsync(Account account)
        {
            Globals.DbContext.Accounts.Remove(account);
            await Globals.DbContext.SaveChangesAsync();
            Accounts = new ObservableCollection<Account>(Globals.DbContext.Accounts);
            RaisePropertyChanged("Accounts");
            RaisePropertyChanged("TotalBalance");
            RaisePropertyChanged("TotalAccounts");
        }

        public async Task RemoveAccountAsync(string address)
        {
            var account = (from a in Accounts
                where a.Address == address
                select a).FirstOrDefault();
            if (account == null)
                return;
            Globals.DbContext.Accounts.Remove(account);
            await Globals.DbContext.SaveChangesAsync();
            Accounts = new ObservableCollection<Account>(Globals.DbContext.Accounts);
            RaisePropertyChanged("Accounts");
            RaisePropertyChanged("TotalBalance");
            RaisePropertyChanged("TotalAccounts");
        }
    }
}