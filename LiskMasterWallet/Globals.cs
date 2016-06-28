using System;
using System.Security;
using System.Threading.Tasks;
using Lisk.API;
using Lisk.API.Responses;
using LiskMasterWallet.ViewModels;
using Newtonsoft.Json;

namespace LiskMasterWallet
{
    public static class Globals
    {
        public delegate void DelegateTimerTick();
        public delegate void NewBlockReceived(Block_Object block);
        public delegate void NewTransactionsReceived(Transaction_Object[] transactions);
        public delegate void MasterPasswordReceived(SecureString masterpassword);

        internal const string CreateTransactionsTableSQL =
            "CREATE TABLE [Transactions] ([Id] nvarchar(256) PRIMARY KEY NOT NULL,[Block] bigint NOT NULL,[Sender] nvarchar(256) NOT NULL,[Receiver] nvarchar(256) NOT NULL,[Amount] decimal NOT NULL,[TType] int NOT NULL,[Created] datetime NOT NULL,[Fee] decimal NOT NULL);";

        internal const string CreateAccountsTableSQL =
            "CREATE TABLE [Accounts] ([Address] nvarchar(256) PRIMARY KEY NOT NULL,[Balance] decimal NOT NULL,[Block] bigint NOT NULL,[FriendlyName] nvarchar(64) NOT NULL,[LastUpdate] datetime NOT NULL,[PublicKey] nvarchar(256) NOT NULL,[SecretHash] nvarchar(256) NOT NULL,[LastTransactionsUpdate] datetime NOT NULL);";

        internal const string CreateUserSettingsTableSQL =
            "CREATE TABLE [UserSettings] ([MasterPasswordHash] nvarchar(256) PRIMARY KEY NOT NULL,[CBCVector] nvarchar(256) NOT NULL);";

        internal static LiskAPI API;
        internal static WebSocketSharp.WebSocket WS;

        internal static long CurrentBlockHeight;

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

        public static event NewBlockReceived OnNewBlockReceived;
        public static event NewTransactionsReceived OnNewTransactionsReceived;

#pragma warning disable 1998
        internal static async Task On_WS_Open()
#pragma warning restore 1998
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
            var json = await args.Text.ReadToEndAsync();
            var res = JsonConvert.DeserializeObject<Types.WSMessage>(json);
            switch (res.messageType)
            {
                case "newUnconfirmedTransactions":
                    return;
                case "newBlock":
                    Console.WriteLine("New WS Block " + json);
                    var block = JsonConvert.DeserializeObject<Block_Object>(res.payload.ToString());
                    CurrentBlockHeight = block.height;
                    if (OnNewBlockReceived != null)
                        OnNewBlockReceived(block);
                    break;
                case "newTransactions":
                    Console.WriteLine("New WS Transactions " + json);
                    var txs = JsonConvert.DeserializeObject<Transaction_Object[]>(res.payload.ToString());
                    if (OnNewTransactionsReceived != null)
                        OnNewTransactionsReceived(txs);
                    break;
                default:
                    return;
            }
        }
    }
}