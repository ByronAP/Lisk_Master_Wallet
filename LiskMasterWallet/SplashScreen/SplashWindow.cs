using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LiskMasterWallet.SplashScreen
{
    public class SplashWindow : Window
    {
        public SplashWindow()
        {
            ShowInTaskbar = false;
            WindowStartupLocation = WindowStartupLocation.Manual;
            ResizeMode = ResizeMode.NoResize;
            WindowStyle = WindowStyle.None;
            Topmost = true;

            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            //calculate it manually since CenterScreen substracts taskbar height from available area
            Left = (SystemParameters.PrimaryScreenWidth - this.Width) / 2;
            Top = (SystemParameters.PrimaryScreenHeight - this.Height) / 2;
        }

        public void Capture(string filePath)
        {
            Capture(filePath, new PngBitmapEncoder());
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
    }
}
