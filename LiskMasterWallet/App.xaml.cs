﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using FirstFloor.ModernUI.Presentation;
using Lisk.API;
using LiskMasterWallet.Helpers;
using LiskMasterWallet.Properties;
using Application = System.Windows.Application;

namespace LiskMasterWallet
{
    //TODO: get rid of all the magic strings
    public partial class App : Application
    {
        private static Splash.Splash splashScreen;
        private async void App_Startup(object sender, StartupEventArgs e)
        {
            AppDomain.CurrentDomain.SetData("DataDirectory",
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

            SetupEnv();

            splashScreen = new Splash.Splash();
            splashScreen.Show();

            SetupConsole();

            CheckSettings();

            CheckUserSettings();

            await SetupAPI();

            Globals.StartTimers();

            WriteLine("Starting application MainWindow");
            var mainWindow = new MainWindow();
            mainWindow.Loaded += (sendername, args) => splashScreen.Close();
            mainWindow.Show();
        }

        private static void SetupEnv()
        {
            var fs = Settings.Default.FontSize;
            switch (fs)
            {
                case "large":
                    AppearanceManager.Current.FontSize = FirstFloor.ModernUI.Presentation.FontSize.Large;
                    break;
                case "small":
                    AppearanceManager.Current.FontSize = FirstFloor.ModernUI.Presentation.FontSize.Small;
                    break;
                default:
                    AppearanceManager.Current.FontSize = FirstFloor.ModernUI.Presentation.FontSize.Large;
                    break;
            }
            AppearanceManager.Current.AccentColor = Settings.Default.AccentColor;
            AppearanceManager.Current.ThemeSource = Settings.Default.Theme;
        }

        private static void SetupConsole()
        {
            if (Settings.Default.ShowDebugConsole)
                DebugConsole.ShowConsoleWindow();
            Console.WriteLine("************* Lisk Master Wallet ***************");
            Console.WriteLine("*************         by         ***************");
            Console.WriteLine("       ______  ______  ____  _   ______ ");
            Console.WriteLine("      / __ ) \\/ / __ \\/ __ \\/ | / / __ \\");
            Console.WriteLine("     / __  |\\  / /_/ / / / /  |/ / /_/ /");
            Console.WriteLine("    / /_/ / / / _, _/ /_/ / /|  / ____/ ");
            Console.WriteLine("   /_____/ /_/_/ |_|\\____/_/ |_/_/   ");
            Console.WriteLine("remember to vote byronp as a delegate, thank you");
            Console.WriteLine("************************************************");
            Console.WriteLine(AppHelpers.GNUGPLNotice);
            Console.WriteLine(AppHelpers.CopyrightNotice);
            Console.WriteLine("Version: " + Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine("Build Date: " + Assembly.GetExecutingAssembly().GetLinkerTime() + " UTC\r\n");
            Console.WriteLine(AppHelpers.CreatorNotice);

            Console.WriteLine("Startup Date: " + DateTime.UtcNow + " UTC");
        }

        private static void CheckSettings()
        {
            if (Settings.Default.UpgradeRequired)
            {
                WriteLine("Upgrading settings to new app version.");
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();
                WriteLine("Settings upgrade success.");
            }
        }

        private static void CheckUserSettings()
        {
            var wfilloc = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                          "\\LiskMasterWallet\\wallet.dat";
            var _dbcontext = new masterwalletEntities();
            if (!File.Exists(wfilloc))
            {
                if (!File.Exists(wfilloc))
                {
                    
                    WriteLine("Creating new wallet");
                    _dbcontext.Database.ExecuteSqlCommand(Globals.CreateAccountsTableSQL);
                    _dbcontext.Database.ExecuteSqlCommand(Globals.CreateTransactionsTableSQL);
                    _dbcontext.Database.ExecuteSqlCommand(Globals.CreateUserSettingsTableSQL);
                    WriteLine("New wallet created");
                }
            }
            RETRY:
            var nv = AppHelpers.GetNewCBCVector();
            if (!(from s in _dbcontext.UserSettings select s).Any())
            {
                WriteLine("Initializing user settings");
                WriteLine("Creating new CBCVector");
                var ni = new UserSetting
                {
                    CBCVector = nv,
                    MasterPasswordHash = RequestCreatMasterPassword()
                };
                _dbcontext.UserSettings.Add(ni);
                _dbcontext.SaveChanges();
                WriteLine("CBCVector creation success");
                WriteLine("User settings update success");
                WriteLine("Restarting Lisk Master Wallet");
                ForceRestart();
            }
            if (!(from s in _dbcontext.UserSettings select s).Any())
            {
                WriteLine("User settings initialization failed");
                WriteLine("Retrying user settings initialization");
                goto RETRY;
            }
            _dbcontext.Dispose();
        }

        private static string RequestCreatMasterPassword()
        {
            WriteLine("Requesting master password creation");
            // set a new master password
            var cmpw = new SetMasterPasswordWindow();

            cmpw.ShowDialog();
            var mpwdh = cmpw.mpwdh;
            if (string.IsNullOrEmpty(mpwdh))
            {
                WriteLine("Quiting no master password");
                ForceExit();
            }
            WriteLine("Master password creation success");
            return mpwdh;
        }

        internal static void ForceExit()
        {
            try
            {
                Current.Shutdown();
                Environment.Exit(0);
            }
            catch
            {
            }
        }

        internal static void ForceRestart()
        {
            Process.Start(ResourceAssembly.Location);
            Current.Shutdown();
        }

        internal static async Task SetupAPI()
        {
            WriteLine("Setting up API system");
            if(Settings.Default.Testnet)
                Console.WriteLine("USING TESTNET");
            // find the fastest server
            var fastserver = "";
            var fastservertime = 0L;
            if (Settings.Default.AutoFindServer)
            {
                WriteLine("Finding best node");
                var servers = Settings.Default.Testnet ? Settings.Default.TestnetServers : Settings.Default.Servers;
                foreach (var s in servers)
                {
                    var rt = AppHelpers.GetServerResponseTime(s);
                    Console.WriteLine("tested server " + s + " response time " + rt + " ms");
                    if (rt < fastservertime || fastservertime <= 0)
                    {
                        try
                        {
                            Console.WriteLine("checking server " + s + " status");
                            // check that the server is synced
                            var _api = new LiskAPI(s);
                            var ls = await _api.Loader_Status();
                            if (ls == null || !ls.success || !ls.loaded || ls.blocksCount != 0)
                            {
                                Console.WriteLine("server " + s + " is not synced, skipping");
                                continue;
                            }
                            var ss = await _api.Loader_SyncStatus();
                            if (ss == null || !ss.success || ss.syncing || ss.blocks != 0)
                            {
                                Console.WriteLine("server " + s + " is not synced, skipping");
                                continue;
                            }
                            fastservertime = rt;
                            fastserver = s;
                        }
                        catch
                        {
                            Console.WriteLine("server " + s + " failed status query request, skipping");
                        }
                    }
                }

                if (fastservertime > 0 && !string.IsNullOrEmpty(fastserver))
                {
                    Console.WriteLine("Selected server " + fastserver + " response time " + fastservertime + " ms");
                    Globals.API = new LiskAPI(fastserver);
                    Globals.CurrentBlockHeight = (await Globals.API.Blocks_GetHeight()).height;
                    Console.WriteLine("API Server block height " + Globals.CurrentBlockHeight);
                    WriteLine("API system setup complete");
                }
                else
                {
                    WriteLine("API system setup failed, could not find a valid server.");
                    var mbr = MessageBox.Show("Error: Could not find a valid server node.\r\nWould you like retry?", "Server Error", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (mbr == MessageBoxResult.Yes)
                        await SetupAPI();
                    else
                        ForceExit();
                }
            }
            else
            {
                fastservertime = AppHelpers.GetServerResponseTime(Settings.Default.Servers[0]);
                WriteLine("Selected first server " + Settings.Default.Servers[0] + " response time " +
                                  fastservertime + " ms");
                if (fastservertime <= 0)
                    WriteLine("API server did not respond to ping request, status unknown, attempting test call");
                Globals.API = new LiskAPI(Settings.Default.Servers[0]);
                try
                {
                    var res = await Globals.API.Loader_Status();
                    if (res == null || !res.loaded)
                    {
                        WriteLine("API server failed to respond, fatal exiting");
                        Console.WriteLine("Press any key to quit");
                        Console.ReadLine();
                        ForceExit();
                    }
                }
                catch (Exception crap)
                {
                    WriteLine("Error: " + crap.Message);
                    Console.WriteLine("Press any key to quit");
                    Console.ReadLine();
                    ForceExit();
                }
                Globals.CurrentBlockHeight = (await Globals.API.Blocks_GetHeight()).height;
                Console.WriteLine("API Server block height " + Globals.CurrentBlockHeight);
                WriteLine("API system setup complete");
            }
        }

        internal static void WriteLine(string txt)
        {
            Console.WriteLine(txt);
            if (splashScreen != null)
                splashScreen.MessageTextBlock.Text = txt;
        }
    }
}