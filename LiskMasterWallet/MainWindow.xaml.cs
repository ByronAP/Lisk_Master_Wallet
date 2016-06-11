using System;
using System.ComponentModel;
using System.Windows;
using FirstFloor.ModernUI.Windows.Controls;
using LiskMasterWallet.Properties;
using LiskMasterWallet.ViewModels;
using FirstFloor.ModernUI.Presentation;

namespace LiskMasterWallet
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ModernWindow
    {
        public MainWindow()
        {
            DataContext = Globals.AppViewModel;
            InitializeComponent();
        }

        private void ModernWindow_Closing(object sender, CancelEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                Settings.Default.MWLocation = RestoreBounds.Location;
                Settings.Default.MWSize = RestoreBounds.Size;
                Settings.Default.MWMaximized = true;
                Settings.Default.MWMinimized = false;
            }
            else if (WindowState == WindowState.Normal)
            {
                Settings.Default.MWLocation = new Point(Left, Top);
                Settings.Default.MWSize = new Size(Width, Height);
                Settings.Default.MWMaximized = false;
                Settings.Default.MWMinimized = false;
            }
            else
            {
                Settings.Default.MWLocation = RestoreBounds.Location;
                Settings.Default.MWSize = RestoreBounds.Size;
                Settings.Default.MWMaximized = false;
                Settings.Default.MWMinimized = true;
            }
            Settings.Default.Save();
        }

        private void ModernWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (Settings.Default.MWSize.Height > 0 && Settings.Default.MWSize.Width > 0)
            {
                if (Settings.Default.MWMaximized)
                {
                    WindowState = WindowState.Maximized;
                    Left = Settings.Default.MWLocation.X;
                    Top = Settings.Default.MWLocation.Y;
                    Width = Settings.Default.MWSize.Width;
                    Height = Settings.Default.MWSize.Height;
                }
                else if (Settings.Default.MWMinimized)
                {
                    WindowState = WindowState.Minimized;
                    Left = Settings.Default.MWLocation.X;
                    Top = Settings.Default.MWLocation.Y;
                    Width = Settings.Default.MWSize.Width;
                    Height = Settings.Default.MWSize.Height;
                }
                else
                {
                    Left = Settings.Default.MWLocation.X;
                    Top = Settings.Default.MWLocation.Y;
                    Width = Settings.Default.MWSize.Width;
                    Height = Settings.Default.MWSize.Height;
                }
            }

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
    }
}