using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows.Navigation;
using LiskMasterWallet.ViewModels;

namespace LiskMasterWallet.Pages
{
    /// <summary>
    ///     Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : UserControl
    {
        public Home()
        {
            DataContext = Globals.AppViewModel;
            InitializeComponent();
            Globals.AppViewModel.PropertyChanged += AppViewModel_PropertyChanged;
        }

        private void AppViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Console.WriteLine("Property Changed " + e.PropertyName);
            //HACK the view receives the change notification but the property tag is different so force a complete refresh
            if (e.PropertyName == "AccountsViewModel" || e.PropertyName == "Accounts")
            {
                Dispatcher.Invoke(() =>
                {
                    var dc = DataContext;
                    foreach (var act in ((AppViewModel) dc).AccountsViewModel.Accounts)
                    {
                        Console.WriteLine("Found act " + act.FriendlyName);
                    }
                    AccountsItemsControl.DataContext = DataContext;
                    AccountsItemsControl.Items.Refresh();
                });
            }
        }

        private void ModernButton_Click(object sender, RoutedEventArgs e)
        {
            var url = "/Pages/AddAccountPage.xaml";
            var bb = new BBCodeBlock();
            bb.LinkNavigator.Navigate(new Uri(url, UriKind.Relative), this, NavigationHelper.FrameSelf);
        }
    }
}