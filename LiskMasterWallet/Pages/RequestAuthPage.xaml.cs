﻿using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LiskMasterWallet.Helpers;
using LiskMasterWallet.ViewModels;

namespace LiskMasterWallet.Pages
{
    /// <summary>
    ///     Interaction logic for RequestAuthPage.xaml
    /// </summary>
    public partial class RequestAuthPage : UserControl
    {
        public RequestAuthPage()
        {
            InitializeComponent();
        }

        private void AcceptButton_OnClick(object sender, RoutedEventArgs e)
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
        }
    }
}