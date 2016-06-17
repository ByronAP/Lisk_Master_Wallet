using FirstFloor.ModernUI.Windows.Controls;

namespace LiskMasterWallet.Splash
{
    /// <summary>
    /// Interaction logic for Splash.xaml
    /// </summary>
    public partial class Splash : ModernDialog
    {
        public Splash()
        {
            Buttons = null;
            this.Width = 300;
            this.Height = 500;
            InitializeComponent();
        }
    }
}
