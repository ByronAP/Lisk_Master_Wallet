﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Lisk.API;

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
            if (PropertyChanged != null) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public static async Task UpdateAccounts()
        {
            var cdt = DateTime.UtcNow.AddMinutes(-2);
            var actstoupd = (from a in Globals.DbContext.Accounts where a.LastUpdate < cdt select a).Take(40);
            foreach (var a in actstoupd)
            {
                try
                {
                    var actinf = await Globals.API.Accounts_GetAccount(a.Address);
                    if (actinf == null || actinf.account == null || !actinf.success)
                        continue;
                    a.Balance = LiskAPI.LSKLongToDecimal(actinf.account.balance);
                    a.LastUpdate = DateTime.UtcNow;
                    var dg = await Globals.API.Delegates_Get(a.PublicKey);
                    if (dg != null && dg.success && dg.@delegate != null && !string.IsNullOrEmpty(dg.@delegate.username))
                        a.FriendlyName = dg.@delegate.username;

                    await Globals.DbContext.SaveChangesAsync();

                    Globals.AppViewModel.AccountsViewModel.RaisePropertyChanged("Accounts");
                    Globals.AppViewModel.AccountsViewModel.RaisePropertyChanged("TotalBalance");
                }
                catch (Exception crap)
                {
                    Console.WriteLine("AccountsViewModel.UpdateAccounts threw an error: " + crap.Message);
                }
            }
        }

        public static async Task UpdateAccount(string address)
        {
            var acttoupd = (from a in Globals.DbContext.Accounts where a.Address == address select a).First();
            try
            {
                var actinf = await Globals.API.Accounts_GetAccount(acttoupd.Address);
                if (actinf == null || actinf.account == null || !actinf.success)
                    return;
                acttoupd.Balance = LiskAPI.LSKLongToDecimal(actinf.account.balance);
                acttoupd.LastUpdate = DateTime.UtcNow;
                var dg = await Globals.API.Delegates_Get(acttoupd.PublicKey);
                if (dg != null && dg.success && dg.@delegate != null && !string.IsNullOrEmpty(dg.@delegate.username))
                    acttoupd.FriendlyName = dg.@delegate.username;
                await Globals.DbContext.SaveChangesAsync();

                Globals.AppViewModel.AccountsViewModel.RaisePropertyChanged("Accounts");
                Globals.AppViewModel.AccountsViewModel.RaisePropertyChanged("TotalBalance");
            }
            catch (Exception crap)
            {
                Console.WriteLine("AccountsViewModel.UpdateAccount threw an error: " + crap.Message);
            }
        }

        public static async Task AddAccountAsync(Account account)
        {
            Globals.DbContext.Accounts.Add(account);
            await Globals.DbContext.SaveChangesAsync();
            Globals.AppViewModel.AccountsViewModel.Accounts =
                new ObservableCollection<Account>(Globals.DbContext.Accounts);
            Globals.AppViewModel.AccountsViewModel.RaisePropertyChanged("Accounts");
            Globals.AppViewModel.AccountsViewModel.RaisePropertyChanged("TotalBalance");
            Globals.AppViewModel.AccountsViewModel.RaisePropertyChanged("TotalAccounts");
        }

        public static async Task RemoveAccountAsync(Account account)
        {
            Globals.DbContext.Accounts.Remove(account);
            await Globals.DbContext.SaveChangesAsync();
            Globals.AppViewModel.AccountsViewModel.Accounts =
                new ObservableCollection<Account>(Globals.DbContext.Accounts);
            Globals.AppViewModel.AccountsViewModel.RaisePropertyChanged("Accounts");
            Globals.AppViewModel.AccountsViewModel.RaisePropertyChanged("TotalBalance");
            Globals.AppViewModel.AccountsViewModel.RaisePropertyChanged("TotalAccounts");
        }

        public static async Task RemoveAccountAsync(string address)
        {
            var account = (from a in Globals.DbContext.Accounts
                where a.Address == address
                select a).FirstOrDefault();
            if (account == null)
                return;
            Globals.DbContext.Accounts.Remove(account);
            await Globals.DbContext.SaveChangesAsync();
            Globals.AppViewModel.AccountsViewModel.Accounts =
                new ObservableCollection<Account>(Globals.DbContext.Accounts);
            Globals.AppViewModel.AccountsViewModel.RaisePropertyChanged("Accounts");
            Globals.AppViewModel.AccountsViewModel.RaisePropertyChanged("TotalBalance");
            Globals.AppViewModel.AccountsViewModel.RaisePropertyChanged("TotalAccounts");
        }
    }
}