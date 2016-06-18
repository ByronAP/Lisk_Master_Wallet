using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using LiskMasterWallet.ViewModels;
using QRCoder;

namespace LiskMasterWallet.Pages.Accounts
{
    /// <summary>
    ///     Interaction logic for History.xaml
    /// </summary>
    public partial class Overview : UserControl
    {
        public Overview()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = (from a in Globals.AppViewModel.AccountsViewModel.Accounts
                where a.FriendlyName == AppViewModel.SelectedAccountFriendlyName
                select a).First();
            var act = (Account) DataContext;
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(act.Address, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrCodeData);
            QRCodeImage.Source = BitmapToImageSource(qrCode.GetGraphic(64, "#000000", "#FFFFFF"));
        }

        public BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }
    }
}