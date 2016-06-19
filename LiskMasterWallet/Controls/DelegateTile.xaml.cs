using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
            var dc = (Delegate_Class) DataContext;
            this.AddressQR.Source = AppHelpers.GenerateQRCodeBMP(dc.Address);
        }
    }
}
