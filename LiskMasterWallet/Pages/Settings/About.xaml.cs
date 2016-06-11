using System.Windows.Controls;
using LiskMasterWallet.Helpers;

namespace LiskMasterWallet.Pages.Settings
{
    /// <summary>
    ///     Interaction logic for About.xaml
    /// </summary>
    public partial class About : UserControl
    {
        public About()
        {
            InitializeComponent();
            AboutText.Text = AppHelpers.CopyrightNotice + "\r\n\r\n" + AppHelpers.CreatorNotice + "\r\n\r\n" +
                             AppHelpers.GNUGPLNotice;
        }
    }
}