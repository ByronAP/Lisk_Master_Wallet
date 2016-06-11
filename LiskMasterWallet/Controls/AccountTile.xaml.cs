using System;
using System.Windows;
using System.Windows.Controls;
using Devcorner.NIdenticon;
using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows.Navigation;
using LiskMasterWallet.Helpers;
using LiskMasterWallet.ViewModels;
using Size = System.Drawing.Size;

namespace LiskMasterWallet.Controls
{
    /// <summary>
    ///     Interaction logic for AccountTile.xaml
    /// </summary>
    public partial class AccountTile : UserControl
    {
        public AccountTile()
        {
            InitializeComponent();
        }

        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
            var g = new IdenticonGenerator();
            AccountIdentImage.Source =
                AppHelpers.BitmapToImageSource(g.Create(FriendlyNameTextBlock.Text,
                    new Size((int) AccountIdentImage.Width, (int) AccountIdentImage.Height)));
            var dc = (Account) DataContext;
            Console.WriteLine(dc.FriendlyName);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AppViewModel.SelectedAccountFriendlyName = FriendlyNameTextBlock.Text;
            var url = "/Pages/AccountPage.xaml";
            var bb = new BBCodeBlock();
            bb.LinkNavigator.Navigate(new Uri(url, UriKind.Relative), this, NavigationHelper.FrameSelf);
        }
    }
}