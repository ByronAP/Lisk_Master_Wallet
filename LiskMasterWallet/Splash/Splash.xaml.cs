using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
            Splash_OnLoaded(null, null);
            InitializeComponent();
            this.Width = 300;
            this.Height = 500;
            Splash_OnLoaded(null, null);
        }

        private void Splash_OnLoaded(object sender, RoutedEventArgs e)
        {
            //calculate it manually since CenterScreen substracts taskbar height from available area
            Left = (SystemParameters.PrimaryScreenWidth - this.Width) / 2 + 12;
            Top = (SystemParameters.PrimaryScreenHeight - this.Height) / 2 + 34;
        }

        public void Capture(string filePath)
        {
#if DEBUG
            Capture(filePath, new PngBitmapEncoder());
#endif
        }

        public void Capture(string filePath, BitmapEncoder encoder)
        {
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)this.Width, (int)this.Height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(this);
            encoder.Frames.Add(BitmapFrame.Create(bmp));
            using (Stream stm = File.Create(filePath))
            {
                encoder.Save(stm);
            }
        }

        private void Splash_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Capture("d:\\StaticSplashScreen.png");
        }
    }
}
