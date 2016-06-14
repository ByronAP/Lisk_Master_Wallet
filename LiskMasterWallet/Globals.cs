using System;
using System.Security;
using System.Threading;
using System.Windows;
using Lisk.API;
using LiskMasterWallet.ViewModels;

namespace LiskMasterWallet
{
    public static class Globals
    {
        public delegate void DelegateTimerTick();

        public delegate void MasterPasswordReceived(SecureString masterpassword);

        internal const string CreateTransactionsTableSQL =
            "CREATE TABLE [Transactions] ([Id] nvarchar(256) PRIMARY KEY NOT NULL,[Block] bigint NOT NULL,[Sender] nvarchar(256) NOT NULL,[Receiver] nvarchar(256) NOT NULL,[Amount] decimal NOT NULL,[TType] int NOT NULL,[Created] datetime NOT NULL,[Fee] decimal NOT NULL);";

        internal const string CreateAccountsTableSQL =
            "CREATE TABLE [Accounts] ([Address] nvarchar(256) PRIMARY KEY NOT NULL,[Balance] decimal NOT NULL,[Block] bigint NOT NULL,[FriendlyName] nvarchar(64) NOT NULL,[LastUpdate] datetime NOT NULL,[PublicKey] nvarchar(256) NOT NULL,[SecretHash] nvarchar(256) NOT NULL,[LastTransactionsUpdate] datetime NOT NULL);";

        internal const string CreateUserSettingsTableSQL =
            "CREATE TABLE [UserSettings] ([MasterPasswordHash] nvarchar(256) PRIMARY KEY NOT NULL,[CBCVector] nvarchar(256) NOT NULL);";

        internal static LiskAPI API;
        //internal static string PSecret = "";
        internal static Timer Timer60Seconds;
        internal static Timer Timer30Seconds;
        internal static Timer Timer10Seconds;
        internal static long CurrentBlockHeight;

        private static bool Initializing = true;

        private static masterwalletEntities _dbEntities = new masterwalletEntities();
        internal static masterwalletEntities DbContext { get { return _dbEntities; } }

        private static AppViewModel _appView = new AppViewModel();
        public static AppViewModel AppViewModel {get { return _appView; } }

        public static event DelegateTimerTick OnDelegate60SecondTimerTick;
        public static event DelegateTimerTick OnDelegate30SecondTimerTick;
        public static event DelegateTimerTick OnDelegate10SecondTimerTick;

        public static void StartTimers()
        {
            Console.WriteLine("Starting timers");
            Timer10Seconds = new Timer(Timer10Seconds_Tick, new AutoResetEvent(false), 0, 10000);
            Timer30Seconds = new Timer(Timer30Seconds_Tick, new AutoResetEvent(false), 0, 30000);
            Timer60Seconds = new Timer(Timer60Seconds_Tick, new AutoResetEvent(false), 0, 60000);
        }

        private static void Timer60Seconds_Tick(object stateInfo)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                OnDelegate60SecondTimerTick?.Invoke();
            });
        }

        private static void Timer30Seconds_Tick(object stateInfo)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                OnDelegate30SecondTimerTick?.Invoke();
            });
        }

        private static void Timer10Seconds_Tick(object stateInfo)
        {
            Application.Current.Dispatcher.Invoke(new Action(async () =>
            {
                CurrentBlockHeight = (await API.Blocks_GetHeight()).height;
                OnDelegate10SecondTimerTick?.Invoke();
                if (Initializing)
                {
                    Initializing = false;
                    OnDelegate30SecondTimerTick?.Invoke();
                    OnDelegate60SecondTimerTick?.Invoke();
                }
            }));
        }
    }
}