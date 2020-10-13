using AntVault2Client.WindowControllers;
using SimpleTcp;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace AntVault2Client.ClientWorkers
{
    class MainClientWorker
    {
        public static Collection<string> Usernames = new Collection<string>();
        public static Collection<Bitmap> ProfilePictures = new Collection<Bitmap>();
        static bool ReceivingProfilePictures = false;
        static bool ReceivingUsernames = false;
        internal static void AntVaultClient_DataReceived(object sender, DataReceivedFromServerEventArgs e)
        {
            string MessageString = AuxiliaryClientWorker.MessageString(e.Data);
            if (MessageString.StartsWith("/AcceptConnection"))
            {
                DoLogin(LoginClientWorker.CurrentUser);
                Thread.Sleep(100);
                RequestUsersList(LoginClientWorker.CurrentUser);
                Thread.Sleep(100);
                RequestProfilePictures(LoginClientWorker.CurrentUser);
                Thread.Sleep(100);
                RequestStatus(LoginClientWorker.CurrentUser);
                Thread.Sleep(100);
                RequestFriendsList(LoginClientWorker.CurrentUser);
            }
            if (MessageString.StartsWith("/SendStatus"))
            {
                SetStatus(MessageString);
            }
            if(MessageString.StartsWith("/SendUsernames") || ReceivingUsernames == true && MessageString.StartsWith("/EndSendUsernames") == false)
            {
                if (MessageString.StartsWith("/SendUsernames") == true && ReceivingUsernames == false)
                {
                    ReceivingUsernames = true;
                }
                else if (MessageString.StartsWith("/SendUsernames") == false && ReceivingUsernames == true)
                {
                    Usernames = AuxiliaryClientWorker.GetStringsFromBytes(e.Data);
                }
                else
                {
                    //Do nothing
                }
            }
            if(MessageString.StartsWith("/SendProfilePictures") || ReceivingProfilePictures == true && MessageString.StartsWith("/EndSendProfilePictures") == false)
            {
                if (MessageString.StartsWith("/SendProfilePictures") == true && ReceivingProfilePictures == false)
                {
                    ReceivingProfilePictures = true;
                }
                else if (MessageString.StartsWith("/SendProfilePicures") == false && ReceivingProfilePictures == true)
                {
                    ProfilePictures = AuxiliaryClientWorker.GetPicturesFromBytes(e.Data);
                    MessageBox.Show(Usernames.Count.ToString() + " " + ProfilePictures.Count.ToString());
                    GetCurrentProfilePicture(LoginClientWorker.CurrentUser);
                }
                else
                {
                    //Do nothing
                }
            }
            if(MessageString.StartsWith("/EndSendProfilePictures"))
            {
                ReceivingProfilePictures = false;
            }
            if(MessageString.StartsWith("/EndSendUsernames"))
            {
                ReceivingUsernames = false;
            }

        }
        internal static void GetCurrentProfilePicture(string CurrentUser)
        {
            Bitmap CurrentProfilePicture = ProfilePictures[Usernames.IndexOf(CurrentUser)];
            CurrentProfilePicture.Save(AppDomain.CurrentDomain.BaseDirectory + "CurrentProfilePicture.png",ImageFormat.Png);
            MainWindowController.MainWindow.Dispatcher.Invoke(() =>
            {
                ImageBrush ProfilePictureBrush = new ImageBrush(AuxiliaryClientWorker.BitmapToBitmapImage(CurrentProfilePicture));
                ProfilePictureBrush.Stretch = Stretch.UniformToFill; 
                MainWindowController.MainPage.UserProfilePicture.Fill = ProfilePictureBrush;
            });
        }
        internal static void DoLogin(string CurrentUser)
        {
            MessageBox.Show("Successfully authenticated to the AntVault server!", "Success!", MessageBoxButton.OK, MessageBoxImage.Information);
            MainWindowController.MainWindow.Dispatcher.Invoke(() =>
            {
                Animations.LoginAnimations.LoginAnimation(MainWindowController.LoginPage.ConnectButton, MainWindowController.LoginPage.LoginGroupBox, MainWindowController.LoginPage.ServerStatusLabel, MainWindowController.LoginPage.ServerStatusEllipse, MainWindowController.LoginPage.WelcomeLabel);
                MainWindowController.MainPage.UserNameLabel.Content = CurrentUser;
            });
        }

        internal static void RequestUsersList(string CurrentUser)
        {
            LoginClientWorker.AntVaultClient.Send("/RequestUsers -U " + CurrentUser + ".");//RequestUsersList -U Username.
        }

        internal static void SetStatus(string MessageString)//SendStatus -Content Status.
        {
            string CurrentStatus = AuxiliaryClientWorker.GetElement(MessageString, "-Content ", ".");
            MainWindowController.MainWindow.Dispatcher.Invoke(() =>
            {
                MainWindowController.MainPage.StatusTextBox.Text = CurrentStatus;
            });
        }

        internal static void RequestProfilePictures(string CurrentUser)//RequestProfilePictures -U Username.
        {
            LoginClientWorker.AntVaultClient.Send("/RequestProfilePictures -U " + CurrentUser + ".");
        }

        internal static void RequestFriendsList(string CurrentUser)
        {
            LoginClientWorker.AntVaultClient.Send("/RequestFriendsList -U " + CurrentUser + ".");//RequestFriendsList -U Username.
        }

        internal static void RequestStatus(string CurrentUser)
        {
            LoginClientWorker.AntVaultClient.Send("/RequestStatus -U " + CurrentUser + ".");//RequestStatus -U Username.
        }
    }
}
