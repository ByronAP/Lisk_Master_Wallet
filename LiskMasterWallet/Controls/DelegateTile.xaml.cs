using System.Windows;
using System.Windows.Controls;
using LiskMasterWallet.Helpers;

namespace LiskMasterWallet.Controls
{
    /// <summary>
    /// Interaction logic for DelegateTile.xaml
    /// </summary>
    public partial class DelegateTile : UserControl
    {
        public DelegateTile()
        {
            InitializeComponent();
        }

        private void DelegateTile_OnLoaded(object sender, RoutedEventArgs e)
        {
            var dc = DataContext as Delegate_Class;
            if (dc == null)
                return;
            //this.AddressQR.Source = AppHelpers.GenerateQRCodeBMP(dc.Address);

        }
    }
}
