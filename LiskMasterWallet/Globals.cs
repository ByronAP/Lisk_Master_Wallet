using System;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Lisk.API;
using Lisk.API.Responses;
using LiskMasterWallet.ViewModels;
using Newtonsoft.Json;

namespace LiskMasterWallet
{
    public static class Globals
    {
        public delegate void DelegateTimerTick();
        public delegate void NewBlockReceived(Lisk.API.Responses.Block_Object block);
        public delegate void NewTransactionsReceived(Lisk.API.Responses.Transaction_Object[] transactions);
        public delegate void MasterPasswordReceived(SecureString masterpassword);

        internal const string CreateTransactionsTableSQL =
            "CREATE TABLE [Transactions] ([Id] nvarchar(256) PRIMARY KEY NOT NULL,[Block] bigint NOT NULL,[Sender] nvarchar(256) NOT NULL,[Receiver] nvarchar(256) NOT NULL,[Amount] decimal NOT NULL,[TType] int NOT NULL,[Created] datetime NOT NULL,[Fee] decimal NOT NULL);";

        internal const string CreateAccountsTableSQL =
            "CREATE TABLE [Accounts] ([Address] nvarchar(256) PRIMARY KEY NOT NULL,[Balance] decimal NOT NULL,[Block] bigint NOT NULL,[FriendlyName] nvarchar(64) NOT NULL,[LastUpdate] datetime NOT NULL,[PublicKey] nvarchar(256) NOT NULL,[SecretHash] nvarchar(256) NOT NULL,[LastTransactionsUpdate] datetime NOT NULL);";

        internal const string CreateUserSettingsTableSQL =
            "CREATE TABLE [UserSettings] ([MasterPasswordHash] nvarchar(256) PRIMARY KEY NOT NULL,[CBCVector] nvarchar(256) NOT NULL);";

        internal static LiskAPI API;
        internal static WebSocketSharp.WebSocket WS;

        internal static Timer Timer60Seconds;
        internal static Timer Timer30Seconds;
        internal static Timer Timer10Seconds;

        internal static long CurrentBlockHeight;

        private static bool Initializing = true;

        private static readonly masterwalletEntities _dbEntities = new masterwalletEntities();

        private static readonly AppViewModel _appView = new AppViewModel();

        internal static masterwalletEntities DbContext
        {
            get { return _dbEntities; }
        }

        public static AppViewModel AppViewModel
        {
            get { return _appView; }
        }

        public static event DelegateTimerTick OnDelegate60SecondTimerTick;
        public static event DelegateTimerTick OnDelegate30SecondTimerTick;
        public static event DelegateTimerTick OnDelegate10SecondTimerTick;
        public static event NewBlockReceived OnNewBlockReceived;
        public static event NewTransactionsReceived OnNewTransactionsReceived;

        public static void StartTimers()
        {
            Console.WriteLine("Starting timers");
            Timer10Seconds = new Timer(Timer10Seconds_Tick, new AutoResetEvent(false), 5, 10000);
            Timer30Seconds = new Timer(Timer30Seconds_Tick, new AutoResetEvent(false), 10, 30000);
            Timer60Seconds = new Timer(Timer60Seconds_Tick, new AutoResetEvent(false), 15, 60000);
        }

        private static void Timer60Seconds_Tick(object stateInfo)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (OnDelegate60SecondTimerTick != null)
                    OnDelegate60SecondTimerTick.Invoke();
            });
        }

        private static void Timer30Seconds_Tick(object stateInfo)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (OnDelegate30SecondTimerTick != null)
                    OnDelegate30SecondTimerTick.Invoke();
            });
        }

        private static void Timer10Seconds_Tick(object stateInfo)
        {
            Application.Current.Dispatcher.Invoke(new Action(async () =>
            {
                CurrentBlockHeight = (await API.Blocks_GetHeight()).height;
                if (OnDelegate10SecondTimerTick != null)
                    OnDelegate10SecondTimerTick.Invoke();
                if (Initializing)
                {
                    Initializing = false;
                    if (OnDelegate30SecondTimerTick != null)
                        OnDelegate30SecondTimerTick.Invoke();
                    if (OnDelegate60SecondTimerTick != null)
                        OnDelegate60SecondTimerTick.Invoke();
                }
            }));
        }

        internal static async Task On_WS_Open()
        {
            Console.WriteLine("WebSocket Opened");
        }

        internal static async Task On_WS_Close(WebSocketSharp.CloseEventArgs args)
        {
            Console.WriteLine("WebSocket Closed");
            await WS.Connect();
        }

        internal static async Task On_WS_Message(WebSocketSharp.MessageEventArgs args)
        {
            var json = args.Text.ReadToEnd();
            Console.WriteLine("New WS Message " + json);
            var res = JsonConvert.DeserializeObject<Types.WSMessage>(json);
            if (res.messageType == "newBlock")
            {
                var block = JsonConvert.DeserializeObject<Lisk.API.Responses.Block_Object>(res.payload.ToString());
                CurrentBlockHeight = block.height;
                if (OnNewBlockReceived != null)
                    OnNewBlockReceived(block);
            }
            else if (res.messageType == "newTransactions")
            {
                var txs = JsonConvert.DeserializeObject<Lisk.API.Responses.Transaction_Object[]>(res.payload.ToString());
                if (OnNewTransactionsReceived != null)
                    OnNewTransactionsReceived(txs);
            }
        }
    }
}