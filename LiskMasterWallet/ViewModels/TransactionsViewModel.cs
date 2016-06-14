using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lisk.API.Responses;
using LiskMasterWallet.Helpers;

namespace LiskMasterWallet.ViewModels
{
    public class TransactionsViewModel : INotifyPropertyChanged
    {
        public TransactionsViewModel()
        {
            Transactions = new ObservableCollection<Transaction>(Globals.DbContext.Transactions);
            RecentTransactions = new ObservableCollection<Transaction>((from t in Transactions
                orderby t.Created descending
                select t).Take(20));
            Transactions.CollectionChanged += CollectionChanged;
        }

        public ObservableCollection<Transaction> Transactions { get; set; }
        public ObservableCollection<Transaction> RecentTransactions { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

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

        public static async Task AddTransactionAsync(Transaction transaction)
        {
            Globals.DbContext.Transactions.Add(transaction);
            await Globals.DbContext.SaveChangesAsync();
            Globals.AppViewModel.TransactionsViewModel.Transactions = new ObservableCollection<Transaction>(Globals.DbContext.Transactions);
            Globals.AppViewModel.TransactionsViewModel.RecentTransactions = new ObservableCollection<Transaction>((from t in Globals.DbContext.Transactions
                orderby t.Created descending
                select t).Take(20));
            Globals.AppViewModel.TransactionsViewModel.RaisePropertyChanged("Transactions");
            Globals.AppViewModel.TransactionsViewModel.RaisePropertyChanged("RecentTransactions");
        }

        public static async Task RemoveTransactionAsync(string id)
        {
            var transaction = (from a in Globals.DbContext.Transactions
                where a.Id == id
                select a).FirstOrDefault();
            if (transaction == null)
                return;
            Globals.DbContext.Transactions.Remove(transaction);
            await Globals.DbContext.SaveChangesAsync();
            Globals.AppViewModel.TransactionsViewModel.Transactions = new ObservableCollection<Transaction>(Globals.DbContext.Transactions);
            Globals.AppViewModel.TransactionsViewModel.RecentTransactions = new ObservableCollection<Transaction>((from t in Globals.DbContext.Transactions
                orderby t.Created descending
                select t).Take(20));
            Globals.AppViewModel.TransactionsViewModel.RaisePropertyChanged("Transactions");
            Globals.AppViewModel.TransactionsViewModel.RaisePropertyChanged("RecentTransactions");
        }

        public static async Task UpdateTransactions()
        {
            try
            {
                var cdt = DateTime.UtcNow.AddMinutes(-2);
                var accounts = (from a in Globals.DbContext.Accounts where a.LastTransactionsUpdate < cdt select a.Address).Take(20);
                if (!accounts.Any())
                    return;
                foreach (var a in accounts)
                {
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
                        Globals.AppViewModel.TransactionsViewModel.Transactions = new ObservableCollection<Transaction>(Globals.DbContext.Transactions);
                        if ((from c in Globals.DbContext.Transactions where c.Id == t.id select c).Any())
                            continue;
                        var ni = new Transaction
                        {
                            Id = t.id,
                            Created = AppHelpers.TimestampToDateTime(t.timestamp),
                            Amount = Lisk.API.LiskAPI.LSKLongToDecimal(t.amount),
                            Block = (await Globals.API.Blocks_GetHeight()).height - long.Parse(t.confirmations),
                            Receiver = t.recipientId,
                            Sender = t.senderId,
                            TType = int.Parse(t.type),
                            Fee = Lisk.API.LiskAPI.LSKLongToDecimal(t.fee)
                        };
                        if (string.IsNullOrEmpty(ni.Receiver))
                            ni.Receiver = "";
                        try
                        {
                            Globals.AppViewModel.TransactionsViewModel.Transactions = new ObservableCollection<Transaction>(Globals.DbContext.Transactions);
                            if ((from r in Globals.DbContext.Transactions where r.Id == ni.Id select r).Any())
                                continue;
                            await AddTransactionAsync(ni);
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
                Globals.AppViewModel.TransactionsViewModel.Transactions = new ObservableCollection<Transaction>(Globals.DbContext.Transactions);
                Globals.AppViewModel.TransactionsViewModel.RecentTransactions = new ObservableCollection<Transaction>((from t in Globals.DbContext.Transactions
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
    }
}