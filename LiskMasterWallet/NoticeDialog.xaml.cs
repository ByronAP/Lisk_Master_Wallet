using System.Windows;
using System.Windows.Controls;
using FirstFloor.ModernUI.Windows.Controls;

namespace LiskMasterWallet
{
    /// <summary>
    ///     Interaction logic for NoticeDialog.xaml
    /// </summary>
    public partial class NoticeDialog : ModernDialog
    {
        public NoticeDialog(string title, string message)
        {
            InitializeComponent();

            // define the dialog buttons
            Buttons = new Button[] {};
            Title = title;
            MessageTextBlock.Text = message;
        }

        private void OKButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}