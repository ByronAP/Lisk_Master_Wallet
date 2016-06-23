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

namespace LiskMasterWallet.Controls
{
    /// <summary>
    /// Interaction logic for ProgressControlWithHeader.xaml
    /// </summary>
    public partial class ProgressControlWithHeader : UserControl
    {
        public string HeaderText { get { return HeaderTextBlock.Text; } set { HeaderTextBlock.Text = value; } }
        public double ProgressValue
        {
            get { return (double)GetValue(ProgressValueProperty); }
            set { SetValue(ProgressValueProperty, value); }
        }
        
        public static readonly DependencyProperty ProgressValueProperty =
             DependencyProperty.Register("ProgressValue", typeof(double), typeof(ProgressControlWithHeader));

        public ProgressControlWithHeader()
        {
            InitializeComponent();
        }
    }
}
