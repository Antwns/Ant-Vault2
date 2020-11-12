using AntVault2Client.ClientWorkers;
using AntVault2Client.WindowControllers;
using AntVault2Client.Windows;
using System.Windows;
using System.Windows.Controls;

namespace AntVault2Client.Pages
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }
        #region Animations
        private void LogOutButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Animations.ObjectAnimations.ButtonMouseOverAnimation(LogOutButton);
        }

        private void YourFilesButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Animations.ObjectAnimations.ButtonMouseOverAnimation(YourFilesButton);
        }

        private void HelpButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Animations.ObjectAnimations.ButtonMouseOverAnimation(HelpButton);
        }

        private void CreditsButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Animations.ObjectAnimations.ButtonMouseOverAnimation(CreditsButton);
        }

        private void AboutButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Animations.ObjectAnimations.ButtonMouseOverAnimation(AboutButton);
        }
        private void OptionsButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Animations.ObjectAnimations.ButtonMouseOverAnimation(OptionsButton);
        }

        #endregion

        private void UserProfilePicture_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        private void StatusTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Enter)
            {
                MainClientWorker.UpdateStatus(StatusTextBox.Text);
            }
        }

        private void ClientChatTextBoxInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Enter)
            {
                MainClientWorker.SendMessage(ClientChatTextBoxInput.Text);
                ClientChatTextBoxInput.Text = null;
            }
        }

        private void LogOutButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MainClientWorker.DoLogout();
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            MainClientWorker.ShowOptions();
        }
    }
}
