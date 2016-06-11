using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FirstFloor.ModernUI.Windows.Controls;
using LiskMasterWallet.Helpers;
using LiskMasterWallet.ViewModels;

namespace LiskMasterWallet
{
    /// <summary>
    ///     Interaction logic for AuthRequestDialog.xaml
    /// </summary>
    public partial class AuthRequestDialog : ModernDialog
    {
        public AuthRequestDialog(AuthViewModel vm)
        {
            DataContext = vm;
            InitializeComponent();

            // define the dialog buttons
            Buttons = new Button[] {};
        }

        private void AcceptModernButton_OnClick(object sender, RoutedEventArgs e)
        {
            var pwh = (from p in Globals.DbContext.UserSettings select p.MasterPasswordHash).First();
            var vpr = AppHelpers.ValidateHash(MasterPasswordTextBox.Password.Trim(), pwh);
            if (!vpr)
            {
                MessageBox.Show("Incorrect master password.\r\nPlease try again.", "Master Password",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var avm = (AuthViewModel) DataContext;
            avm.Password = MasterPasswordTextBox.Password.Trim();
            avm.Accepted = true;

            DialogResult = true;
        }

        private void CancelModernButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}