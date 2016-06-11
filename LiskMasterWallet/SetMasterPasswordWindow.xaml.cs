using System.Windows;
using System.Windows.Media;
using FirstFloor.ModernUI.Windows.Controls;
using LiskMasterWallet.Helpers;

namespace LiskMasterWallet
{
    /// <summary>
    ///     Interaction logic for SetMasterPasswordWindow.xaml
    /// </summary>
    public partial class SetMasterPasswordWindow : ModernWindow
    {
        internal string mpwdh = "";

        public SetMasterPasswordWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var tmp = MasterPasswordTextBox.Password;
            MasterPasswordTextBox.Password = "";
            if (string.IsNullOrEmpty(tmp) || tmp.Contains(" ") || tmp.Trim().Length < 6 || tmp.Trim().Length > 16)
            {
                MasterPasswordTextBox.Background = new SolidColorBrush(Colors.DarkRed);
                return;
            }

            mpwdh = AppHelpers.CreateHash(tmp);

            Close();
        }
    }
}