using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows.Media;
using LiskMasterWallet.Properties;

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
            if (Settings.Default.Testnet)
            {
                var tm = this.Template;
                var stb = (TextBlock) tm.FindName("StatusTextBlock", this);
                stb.Foreground = new SolidColorBrush(Colors.Red);
            }

    }
    }
}