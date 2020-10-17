using AntVault2Client.WindowControllers;
using SimpleTcp;
using System;
using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace AntVault2Client.ClientWorkers
{
    class MainClientWorker
    {
        public static Collection<string> Usernames = new Collection<string>();
        public static Collection<Bitmap> ProfilePictures = new Collection<Bitmap>();
        public static Collection<string> FriendsList = new Collection<string>();

        static string UpdateSender = null;

        static bool ReceivingProfilePictures = false;
        static bool ReceivingUsernames = false;
        static bool ReceivingProfilePictureUpdatePulse = false;
        static bool ReceivingFriendsList = false;
        static bool SenderDefined = false;
        internal static void AntVaultClient_DataReceived(object sender, DataReceivedFromServerEventArgs e)
        {
            string MessageString = AuxiliaryClientWorker.MessageString(e.Data);
            if (MessageString.StartsWith("/AcceptConnection"))
            {
                DoLogin(LoginClientWorker.CurrentUser);
                RequestUsersList(LoginClientWorker.CurrentUser);
            }
            if (MessageString.StartsWith("/SendStatus"))
            {
                SetStatus(MessageString);
                Thread.Sleep(200);
                RequestFriendsList(LoginClientWorker.CurrentUser);
            }
            if(MessageString.StartsWith("/SendUsernames") || ReceivingUsernames == true)
            {
                if (MessageString.StartsWith("/SendUsernames") == true && ReceivingUsernames == false)
                {
                    ReceivingProfilePictureUpdatePulse = false;
                    ReceivingFriendsList = false;
                    ReceivingProfilePictures = false;
                    ReceivingUsernames = true;
                }
                else if (MessageString.StartsWith("/SendUsernames") == false && ReceivingUsernames == true)
                {
                    ReceivingUsernames = false;
                    Usernames = AuxiliaryClientWorker.GetStringsFromBytes(e.Data);
                    RequestProfilePictures(LoginClientWorker.CurrentUser);
                }
            }
            if(MessageString.StartsWith("/SendProfilePictures") || ReceivingProfilePictures == true)
            {
                if (MessageString.StartsWith("/SendProfilePictures") == true && ReceivingProfilePictures == false)
                {
                    ReceivingProfilePictureUpdatePulse = false;
                    ReceivingFriendsList = false;
                    ReceivingUsernames = false;
                    ReceivingProfilePictures = true;
                }
                else if (MessageString.StartsWith("/SendProfilePicures") == false && ReceivingProfilePictures == true)
                {
                    ReceivingProfilePictures = false;
                    File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "This.txt", MessageString);
                    ProfilePictures = AuxiliaryClientWorker.GetPicturesFromBytes(e.Data);//
                    GetCurrentProfilePicture(LoginClientWorker.CurrentUser);
                    RequestStatus(LoginClientWorker.CurrentUser);
                }
            }
            if(MessageString.StartsWith("/SendFriendsList") || ReceivingFriendsList == true)
            {
                if(MessageString.StartsWith("/SendFriendsList") == true && ReceivingFriendsList == false)
                {
                    ReceivingProfilePictureUpdatePulse = false;
                    ReceivingProfilePictures = false;
                    ReceivingUsernames = false;
                    ReceivingFriendsList = true;
                }
                else if(MessageString.StartsWith("/SendFriendsList") == false && ReceivingFriendsList == true)
                {
                    ReceivingFriendsList = false;
                    FriendsList = AuxiliaryClientWorker.GetStringsFromBytes(e.Data);
                    GetFriendsList();
                }
            }
            if(MessageString.StartsWith("/Message"))
            {
                HandleMessage(MessageString);
            }
            if(MessageString.StartsWith("/UpdateStatus"))//UpdateStatus -U Username -Content Msg.
            {
                HandleStatus(MessageString);
            }
            if(MessageString.StartsWith("/UserUpdatedPicture") || ReceivingProfilePictureUpdatePulse == true)//UserUpdatedPicture -U  + UsernameToUpdate.
            {
                if (SenderDefined == false)
                {
                    UpdateSender = AuxiliaryClientWorker.GetElement(MessageString, "-U ", ".");
                    SenderDefined = true;
                }
                if (MessageString.StartsWith("/UserUpdatedPicture") == true && ReceivingProfilePictureUpdatePulse == false)
                {
                    ReceivingFriendsList = false;
                    ReceivingProfilePictures = false;
                    ReceivingUsernames = false;
                    ReceivingProfilePictureUpdatePulse = true;
                }
                else if (MessageString.StartsWith("/UserUpdatedPicture") == false && ReceivingProfilePictureUpdatePulse == true)
                {
                    UpdateUserProfilePicture(UpdateSender, e.Data);
                    ReceivingProfilePictureUpdatePulse = false;
                    SenderDefined = false;
                }
            }
        }

        internal static void UpdateUserProfilePicture(string UserToUpdate, byte[] Data)
        {
            byte[] DataToUse = Data;
            ProfilePictures[Usernames.IndexOf(UserToUpdate)] = AuxiliaryClientWorker.GetBitmapFromBytes(Data);
            if(UserToUpdate == LoginClientWorker.CurrentUser)
            {
                MainWindowController.MainWindow.Dispatcher.Invoke(() =>
                {
                    ImageBrush NewProfilePictureBrush = new ImageBrush(AuxiliaryClientWorker.BitmapToBitmapImage(AuxiliaryClientWorker.GetBitmapFromBytes(DataToUse)));
                    MainWindowController.MainPage.UserProfilePicture.Fill = NewProfilePictureBrush;
                });
            }
        }
        internal static void UpdateProfilePicture(byte[] NewProfilePictureBytes)//UpdateProfilePicture //byte[] //EndUpdateProfilePicture
        {
            LoginClientWorker.AntVaultClient.Send("/UpdateProfilePicture");
            Thread.Sleep(200);
            LoginClientWorker.AntVaultClient.Send(NewProfilePictureBytes);
        }

        internal static void UpdatePassword(string OldPassword, string NewPassword)//UpdatePassword -P OldPassword -Content NewPassword.
        {
            LoginClientWorker.AntVaultClient.Send("/UpdatePassword -P " + OldPassword + " -Content " + NewPassword + ".");
        }

        internal static void UpdateUsername(string CurrentUsername, string NewUsername)//UpdateUsername -U OldUsername -Content NewUsername.
        {
            LoginClientWorker.AntVaultClient.Send("/UpdateUsername -U " + CurrentUsername + " -Content " + NewUsername + ".");
        }

        internal static void ShowOptions()
        {
            MainWindowController.MainWindow.Dispatcher.Invoke(() =>
            {
                MainWindowController.MainWindow.Height = 450;
                MainWindowController.MainWindow.Width = 800;
                MainWindowController.MainWindow.Content = MainWindowController.OptionsPage;
                MainWindowController.OptionsPage.UsernameTextBox.Text = LoginClientWorker.CurrentUser;
                MainWindowController.OptionsPage.OldPasswordTextBox.Text = null;
                MainWindowController.OptionsPage.NewPasswordTextBox.Text = null;
            });
        }

        internal static void ShowMainMenu()
        {
            MainWindowController.MainWindow.Dispatcher.Invoke(() => 
            {
                MainWindowController.MainWindow.Height = 1024;
                MainWindowController.MainWindow.Width = 1280;
                MainWindowController.MainWindow.Content = MainWindowController.MainPage;
                MainWindowController.MainPage.UserNameLabel.Content = LoginClientWorker.CurrentUser;
            });
        }

        private static void HandleStatus(string MessageString)
        {
            string Sender = AuxiliaryClientWorker.GetElement(MessageString, "-U ", " -Content");
            string Message = AuxiliaryClientWorker.GetElement(MessageString, "-Content ", ".");
            MainWindowController.MainWindow.Dispatcher.Invoke(() =>
            {
                #region Status updater
                System.Windows.Controls.Label StatusLabel = new System.Windows.Controls.Label();
                StatusLabel.FontSize = 18;
                StatusLabel.FontStyle = FontStyles.Oblique;
                StatusLabel.FontStyle = FontStyles.Italic;
                StatusLabel.Foreground = System.Windows.Media.Brushes.Black;
                StatusLabel.MouseLeftButtonDown += This;
                StatusLabel.Content = Sender + " has updated their status to " + Message;
                #endregion
                MainWindowController.ChatParagraph.Inlines.Add(StatusLabel);
                MainWindowController.ChatParagraph.Inlines.Add(Environment.NewLine);
                MainWindowController.ChatDocument.Blocks.Add(MainWindowController.ChatParagraph);
                MainWindowController.MainPage.ClientChatTextBox.Document = MainWindowController.ChatDocument;
            });
        }

        private static void This(object sender, System.Windows.Input.MouseEventArgs e)
        {
            MessageBox.Show("This");
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
                ImageToShowFrame.MouseLeftButtonDown += This;
                #endregion
                MainWindowController.ChatParagraph.Inlines.Add(ImageToShowFrame);

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

        internal static void UpdateStatus(string NewStatus)
        {
            LoginClientWorker.AntVaultClient.Send("/UpdateStatus -U " + LoginClientWorker.CurrentUser + " -Content " + NewStatus + ".");
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

        internal static void DoLogout()
        {
            MessageBox.Show("You will now be logged out", "Logout", MessageBoxButton.OK, MessageBoxImage.Information);
            MainWindowController.MainWindow.Dispatcher.Invoke(() =>
            {
                MainWindowController.MainWindow.Content = MainWindowController.LoginPage;
                MainWindowController.LoginPage.UserNameTextBox.Text = null;
                MainWindowController.LoginPage.PasswordTextBox.Text = null;
                MainWindowController.MainWindow.Width = 800;
                MainWindowController.MainWindow.Height = 450;
                Animations.LoginAnimations.LogoutAnimation(MainWindowController.LoginPage.ConnectButton, MainWindowController.LoginPage.LoginGroupBox, MainWindowController.LoginPage.ServerStatusLabel, MainWindowController.LoginPage.ServerStatusEllipse, MainWindowController.LoginPage.WelcomeLabel);
            });
            FriendsList.Clear();
            Usernames.Clear();
            ProfilePictures.Clear();
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
