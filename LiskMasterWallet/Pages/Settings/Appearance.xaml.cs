using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FirstFloor.ModernUI.Presentation;
using LiskMasterWallet.ViewModels;

namespace LiskMasterWallet.Pages.Settings
{
    /// <summary>
    ///     Interaction logic for Appearance.xaml
    /// </summary>
    public partial class Appearance : UserControl
    {
        public Appearance()
        {
            InitializeComponent();

            // create and assign the appearance view model
            DataContext = new AppearanceViewModel();
        }

        private void Appearance_OnLoaded(object sender, RoutedEventArgs e)
        {
            var acb = (SolidColorBrush) FindResource("AccentColorBrush");
            acb.Color = AppearanceManager.Current.AccentColor;
        }
    }
}