using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lisk.API;
using Lisk.API.Responses;
using LiskMasterWallet.Helpers;

namespace LiskMasterWallet.ViewModels
{
    public class TransactionsViewModel : INotifyPropertyChanged
    {
        private static readonly BackgroundWorker UpdateWorker = new BackgroundWorker();

        public TransactionsViewModel()
        {
            Transactions = new ObservableCollection<Transaction>(Globals.DbContext.Transactions);
            RecentTransactions = new ObservableCollection<Transaction>((from t in Transactions
                                                                        orderby t.Created descending
                                                                        select t).Take(20));
            Transactions.CollectionChanged += CollectionChanged;
            UpdateWorker.DoWork += UpdateWorker_DoWork;
            Globals.OnNewTransactionsReceived += Globals_OnNewTransactionsReceived;
        }

        public ObservableCollection<Transaction> Transactions { get; set; }
        public ObservableCollection<Transaction> RecentTransactions { get; private set; }

        public ObservableCollection<Transaction> CurrentAccountTransactions
        {
            get
            {
                var addy =
                    (from a in Globals.DbContext.Accounts
                     where a.FriendlyName == AppViewModel.SelectedAccountFriendlyName
                     select a.Address).First();
                var trans = from t in Globals.DbContext.Transactions
                            where t.Sender == addy || t.Receiver == addy
                            orderby t.Created descending
                            select t;
                var result = new ObservableCollection<Transaction>();
                foreach (var t in trans)
                {
                    result.Add(t);
                }
                return result;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private async void UpdateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            await UpdateTransactionsAsync();
        }

        internal async Task UpdateTransactionsAsync()
        {
            try
            {
                var cdt = DateTime.UtcNow.AddMinutes(-2);
                var accounts =
                    (from a in Globals.DbContext.Accounts where a.LastTransactionsUpdate < cdt select a.Address).Take(20);
                if (!accounts.Any())
                    return;
                foreach (var a in accounts)
                {
                    //TODO: the code in this loop should be put into a private method since it is duplicated
                    do
                    {
                        // just wait
                    } while (Globals.API == null);
                    var stx = await Globals.API.Transactions_GetList("", a, "", 50, 0, "t_timestamp:desc");
                    Thread.Sleep(300);
                    var rtx = await Globals.API.Transactions_GetList("", "", a, 50, 0, "t_timestamp:desc");
                    var trans = new List<Transaction_Object>();
                    if (stx != null && stx.transactions.Any())
                        trans.AddRange(stx.transactions);
                    if (rtx != null && rtx.transactions.Any())
                        trans.AddRange(rtx.transactions);

                    var newtrans = from t in trans select t;
                    foreach (var t in newtrans)
                    {
                        Globals.AppViewModel.TransactionsViewModel.Transactions =
                            new ObservableCollection<Transaction>(Globals.DbContext.Transactions);
                        if ((from c in Globals.DbContext.Transactions where c.Id == t.id select c).Any())
                            continue;
                        var ni = new Transaction
                        {
                            Id = t.id,
                            Created = AppHelpers.TimestampToDateTime(t.timestamp),
                            Amount = LiskAPI.LSKLongToDecimal(t.amount),
                            Block = (await Globals.API.Blocks_GetHeight()).height - long.Parse(t.confirmations),
                            Receiver = t.recipientId,
                            Sender = t.senderId,
                            TType = int.Parse(t.type),
                            Fee = LiskAPI.LSKLongToDecimal(t.fee)
                        };
                        if (string.IsNullOrEmpty(ni.Receiver))
                            ni.Receiver = "";
                        try
                        {
                            Globals.AppViewModel.TransactionsViewModel.Transactions =
                                new ObservableCollection<Transaction>(Globals.DbContext.Transactions);
                            if ((from r in Globals.DbContext.Transactions where r.Id == ni.Id select r).Any())
                                continue;
                            await AddTransactionAsync(ni, false);
                        }
                        catch (Exception crap)
                        {
                            Console.WriteLine("TransactionsViewModel.UpdateTransactions Error saving transaction: " +
                                              crap.Message);
                        }
                    }
                    var act =
                        (from c in Globals.DbContext.Accounts where c.Address == a select c).First();
                    act.LastTransactionsUpdate = DateTime.UtcNow;
                    await Globals.DbContext.SaveChangesAsync();
                }
            }
            catch (Exception crap)
            {
                Console.WriteLine("TransactionsViewModel.UpdateTransactions threw an exception: " + crap.Message + " | " +
                                  crap.Source);
            }

            try
            {
                Globals.AppViewModel.TransactionsViewModel.Transactions =
                    new ObservableCollection<Transaction>(Globals.DbContext.Transactions);
                Globals.AppViewModel.TransactionsViewModel.RecentTransactions =
                    new ObservableCollection<Transaction>((from t in Globals.DbContext.Transactions
                                                           orderby t.Created descending
                                                           select t).Take(20));
                Globals.AppViewModel.TransactionsViewModel.RaisePropertyChanged("Transactions");
                Globals.AppViewModel.TransactionsViewModel.RaisePropertyChanged("RecentTransactions");
            }
            catch (Exception crap)
            {
                // the async thread is probably not caught up
                Console.WriteLine("TransactionsViewModel.UpdateTransactions threw an exception: " + crap.Message);
            }
        }

        internal async Task UpdateTransactionsForAccountAsync(string address)
        {
            try
            {
                var accounts =
                    (from a in Globals.DbContext.Accounts where a.Address == address select a.Address).Any();
                if (!accounts)
                    return;
                do
                {
                    // just wait
                } while (Globals.API == null);

                var stx = await Globals.API.Transactions_GetList("", address, "", 50, 0, "t_timestamp:desc");
                Thread.Sleep(300);
                var rtx = await Globals.API.Transactions_GetList("", "", address, 50, 0, "t_timestamp:desc");
                var trans = new List<Transaction_Object>();
                if (stx != null && stx.transactions.Any())
                    trans.AddRange(stx.transactions);
                if (rtx != null && rtx.transactions.Any())
                    trans.AddRange(rtx.transactions);

                var newtrans = from t in trans select t;
                foreach (var t in newtrans)
                {
                    Globals.AppViewModel.TransactionsViewModel.Transactions =
                        new ObservableCollection<Transaction>(Globals.DbContext.Transactions);
                    if ((from c in Globals.DbContext.Transactions where c.Id == t.id select c).Any())
                        continue;
                    var ni = new Transaction
                    {
                        Id = t.id,
                        Created = AppHelpers.TimestampToDateTime(t.timestamp),
                        Amount = LiskAPI.LSKLongToDecimal(t.amount),
                        Block = (await Globals.API.Blocks_GetHeight()).height - long.Parse(t.confirmations),
                        Receiver = t.recipientId,
                        Sender = t.senderId,
                        TType = int.Parse(t.type),
                        Fee = LiskAPI.LSKLongToDecimal(t.fee)
                    };
                    if (string.IsNullOrEmpty(ni.Receiver))
                        ni.Receiver = "";
                    try
                    {
                        Globals.AppViewModel.TransactionsViewModel.Transactions =
                            new ObservableCollection<Transaction>(Globals.DbContext.Transactions);
                        if ((from r in Globals.DbContext.Transactions where r.Id == ni.Id select r).Any())
                            continue;
                        await AddTransactionAsync(ni, false);
                    }
                    catch (Exception crap)
                    {
                        Console.WriteLine("TransactionsViewModel.UpdateTransactionsForAccountAsync Error saving transaction: " +
                                          crap.Message);
                    }
                }
                var act =
                    (from c in Globals.DbContext.Accounts where c.Address == address select c).First();
                act.LastTransactionsUpdate = DateTime.UtcNow;
                await Globals.DbContext.SaveChangesAsync();
            }
            catch (Exception crap)
            {
                Console.WriteLine("TransactionsViewModel.UpdateTransactionsForAccountAsync threw an exception: " + crap.Message + " | " +
                                  crap.Source);
            }

            try
            {
                Globals.AppViewModel.TransactionsViewModel.Transactions =
                    new ObservableCollection<Transaction>(Globals.DbContext.Transactions);
                Globals.AppViewModel.TransactionsViewModel.RecentTransactions =
                    new ObservableCollection<Transaction>((from t in Globals.DbContext.Transactions
                                                           orderby t.Created descending
                                                           select t).Take(20));
                Globals.AppViewModel.TransactionsViewModel.RaisePropertyChanged("Transactions");
                Globals.AppViewModel.TransactionsViewModel.RaisePropertyChanged("RecentTransactions");
            }
            catch (Exception crap)
            {
                // the async thread is probably not caught up
                Console.WriteLine("TransactionsViewModel.UpdateTransactionsForAccountAsync threw an exception: " + crap.Message);
            }
        }

        private void Globals_OnNewTransactionsReceived(Transaction_Object[] transactions)
        {
            if (UpdateWorker.IsBusy)
                return;
            var hasaddys = (from a in Globals.AppViewModel.AccountsViewModel.Accounts select a.Address).Any();
            if (!hasaddys)
                return;
            var addys = (from a in Globals.AppViewModel.AccountsViewModel.Accounts select a.Address).ToArray();
            if (
                transactions.Select(t => (from a in addys where a == t.recipientId || a == t.senderId select a).Any())
                    .Any(haswork => haswork))
            {
                UpdateWorker.RunWorkerAsync();
            }
        }

        public void RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Globals.DbContext.SaveChangesAsync();
            RaisePropertyChanged("Transactions");
            RaisePropertyChanged("RecentTransactions");
        }

        public static async Task AddTransactionAsync(Transaction transaction, bool raisechangedevent = true)
        {
            Globals.DbContext.Transactions.Add(transaction);
            await Globals.DbContext.SaveChangesAsync();
            Globals.AppViewModel.TransactionsViewModel.Transactions =
                new ObservableCollection<Transaction>(Globals.DbContext.Transactions);
            Globals.AppViewModel.TransactionsViewModel.RecentTransactions =
                new ObservableCollection<Transaction>((from t in Globals.DbContext.Transactions
                                                       orderby t.Created descending
                                                       select t).Take(20));
            if (raisechangedevent)
            {
                Globals.AppViewModel.TransactionsViewModel.RaisePropertyChanged("Transactions");
                Globals.AppViewModel.TransactionsViewModel.RaisePropertyChanged("RecentTransactions");
            }
            // we check both sides since we could potentially be sending to one of our own accounts
            var istx =
                (from a in Globals.AppViewModel.AccountsViewModel.Accounts
                 where a.Address == transaction.Sender
                 select a).Any();
            var isrx =
                (from a in Globals.AppViewModel.AccountsViewModel.Accounts
                 where a.Address == transaction.Receiver
                 select a).Any();
            if (istx)
                await AccountsViewModel.UpdateAccount(transaction.Sender);
            if (isrx)
                await AccountsViewModel.UpdateAccount(transaction.Receiver);
        }

        public static async Task RemoveTransactionAsync(string id, bool raisechangedevent = true)
        {
            var transaction = (from a in Globals.DbContext.Transactions
                               where a.Id == id
                               select a).FirstOrDefault();
            if (transaction == null)
                return;
            Globals.DbContext.Transactions.Remove(transaction);
            await Globals.DbContext.SaveChangesAsync();
            Globals.AppViewModel.TransactionsViewModel.Transactions =
                new ObservableCollection<Transaction>(Globals.DbContext.Transactions);
            Globals.AppViewModel.TransactionsViewModel.RecentTransactions =
                new ObservableCollection<Transaction>((from t in Globals.DbContext.Transactions
                                                       orderby t.Created descending
                                                       select t).Take(20));
            if (raisechangedevent)
            {
                Globals.AppViewModel.TransactionsViewModel.RaisePropertyChanged("Transactions");
                Globals.AppViewModel.TransactionsViewModel.RaisePropertyChanged("RecentTransactions");
            }
        }
    }
}