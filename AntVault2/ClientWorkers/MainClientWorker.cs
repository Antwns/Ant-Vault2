using AntVault2Client.Pages;
using AntVault2Client.WindowControllers;
using AntVault2Client.Windows;
using SimpleTcp;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection.Emit;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Media;

namespace AntVault2Client.ClientWorkers
{
    class MainClientWorker
    {
        public static Collection<string> Usernames = new Collection<string>();
        public static Collection<Bitmap> ProfilePictures = new Collection<Bitmap>();
        public static Collection<string> FriendsList = new Collection<string>();
        static bool ReceivingProfilePictures = false;
        static bool ReceivingUsernames = false;
        static bool ReceivingFriendsList = false;
        internal static void AntVaultClient_DataReceived(object sender, DataReceivedFromServerEventArgs e)
        {
            string MessageString = AuxiliaryClientWorker.MessageString(e.Data);
            if (MessageString.StartsWith("/AcceptConnection"))
            {
                DoLogin(LoginClientWorker.CurrentUser);
                RequestUsersList(LoginClientWorker.CurrentUser);
                RequestProfilePictures(LoginClientWorker.CurrentUser);
            }
            if (MessageString.StartsWith("/SendStatus"))
            {
                SetStatus(MessageString);
                RequestFriendsList(LoginClientWorker.CurrentUser);
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
                    GetCurrentProfilePicture(LoginClientWorker.CurrentUser);
                }
                else
                {
                    //Do nothing
                }
            }
            if(MessageString.StartsWith("/SendFriendsList") || ReceivingFriendsList == true && MessageString.StartsWith("/EndSendFriendsList") == false)
            {
                if(MessageString.StartsWith("/SendFriendsList") == true && ReceivingFriendsList == false)
                {
                    ReceivingFriendsList = true;
                }
                else if(MessageString.StartsWith("/SendFriendsList") == false && ReceivingFriendsList == true)
                {
                    FriendsList = AuxiliaryClientWorker.GetStringsFromBytes(e.Data);
                    GetFriendsList();
                }
            }
            if(MessageString.StartsWith("/EndSendProfilePictures"))
            {
                ReceivingProfilePictures = false;
                RequestStatus(LoginClientWorker.CurrentUser);
            }
            if(MessageString.StartsWith("/EndSendUsernames"))
            {
                ReceivingUsernames = false;
            }
            if(MessageString.StartsWith("/EndSendFriendsList"))
            {
                ReceivingFriendsList = false;
            }
            if(MessageString.StartsWith("/Message"))
            {
                HandleMessage(MessageString);
            }
        }

        private static void HandleMessage(string MessageString)
        {
            string Sender = AuxiliaryClientWorker.GetElement(MessageString, "-U ", " -Content");
            string Message = AuxiliaryClientWorker.GetElement(MessageString, "-Content ", ".");
            MainWindowController.MainWindow.Dispatcher.Invoke(() =>
            {
                #region Profile picture before the text
                System.Windows.Shapes.Ellipse ImageToShowFrame = new System.Windows.Shapes.Ellipse();
                ImageToShowFrame.Height = 30;
                ImageToShowFrame.Width = 30;
                ImageToShowFrame.Stroke = System.Windows.Media.Brushes.Black;
                ImageToShowFrame.StrokeThickness = 1.5;
                ImageBrush ImageToShowBrush = new ImageBrush(AuxiliaryClientWorker.BitmapToBitmapImage(ProfilePictures[Usernames.IndexOf(Sender)]));
                ImageToShowFrame.Fill = ImageToShowBrush;
                MainWindowController.ChatParagraph.Inlines.Add(ImageToShowFrame);
                #endregion

                #region Text to add after the profile picture
                System.Windows.Controls.Label TextToShow = new System.Windows.Controls.Label();
                TextToShow.FontSize = 16;
                TextToShow.Foreground = System.Windows.Media.Brushes.Black;
                TextToShow.Content = "[" + Sender + "]: " + Message;
                #endregion
                MainWindowController.ChatParagraph.Inlines.Add(TextToShow);
                MainWindowController.ChatParagraph.Inlines.Add(Environment.NewLine);
                MainWindowController.ChatDocument.Blocks.Add(MainWindowController.ChatParagraph);
                MainWindowController.MainPage.ClientChatTextBox.Document = MainWindowController.ChatDocument;
            });
        }

        internal static void SendMessage(string Message)
        {
            LoginClientWorker.AntVaultClient.Send("/Message -U " + LoginClientWorker.CurrentUser + " -Content " + Message + ".");
        }

        private static void GetFriendsList()
        {
            MainWindowController.MainWindow.Dispatcher.Invoke(() =>
            {
                MainWindowController.MainPage.FriendsTextBox.Text = null;
            });
            foreach(string Friend in FriendsList)
            {
                MainWindowController.MainWindow.Dispatcher.Invoke(() =>
                {
                    MainWindowController.MainPage.FriendsTextBox.Text += Friend + Environment.NewLine;
                });
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
