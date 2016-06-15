using System.Windows;
using FirstFloor.ModernUI.Windows.Controls;
using System.Windows.Controls;

namespace LiskMasterWallet
{
    /// <summary>
    /// Interaction logic for NoticeDialog.xaml
    /// </summary>
    public partial class ProcessingDialog : ModernDialog
    {
        public ProcessingDialog(string title, string message)
        {
            InitializeComponent();

            // define the dialog buttons
            this.Buttons = new Button[] {};
            Title = title;
            MessageTextBlock.Text = message;
        }
    }
}
