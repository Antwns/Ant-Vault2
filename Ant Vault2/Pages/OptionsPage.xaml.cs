using AntVault2Client.ClientWorkers;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;

namespace AntVault2Client.Pages
{
    /// <summary>
    /// Interaction logic for OptionsPage.xaml
    /// </summary>
    public partial class OptionsPage : Page
    {
        public OptionsPage()
        {
            InitializeComponent();
        }

        internal static byte[] NewProfilePictureBytes;
        internal static bool ChangedProfilePicture = false;

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainClientWorker.ShowMainMenu();
        }

        private void PlaySoundsButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlaySoundsCheckBox.IsChecked == true)
            {
                PlaySoundsCheckBox.IsChecked = false;
            }
            else
            {
                PlaySoundsCheckBox.IsChecked = true;
            }
        }

        private void ApplyBUtton_Click(object sender, RoutedEventArgs e)
        {
            /*if(UsernameTextBox.Text != LoginClientWorker.CurrentUser)
            {
                MainClientWorker.UpdateUsername(LoginClientWorker.CurrentUser, UsernameTextBox.Text);
            }
            if(NewPasswordTextBox.Text != "")
            {
                MainClientWorker.UpdatePassword(OldPasswordTextBox.Text, NewPasswordTextBox.Text);
            }
            if(ChangedProfilePicture == true)
            {
                MainClientWorker.UpdateProfilePicture(NewProfilePictureBytes);
            }*/
        }

        private void BrowseForNewProfilePictureBtton_Click(object sender, RoutedEventArgs e)
        {
            NewProfilePictureBytes = AuxiliaryClientWorker.GetNewProfilePicture();//Returns resized 500x500 PNG picture and validates it
        }
    }
}
