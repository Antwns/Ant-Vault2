using AntVault2Client.WindowControllers;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using SimpleSockets;
using SimpleSockets.Client;

namespace AntVault2Client.ClientWorkers
{
    class MainClientWorker
    {
        public static Collection<string> Usernames = new Collection<string>();
        public static Collection<Bitmap> ProfilePictures = new Collection<Bitmap>();
        public static Collection<string> FriendsList = new Collection<string>();

        internal static void AntVaultClient_BytesReceived(SimpleSocketClient AntVaultClient, byte[] MessageBytes)
        {
            string MessageString = AuxiliaryClientWorker.MessageString(MessageBytes);
            if (MessageString.StartsWith("/AcceptConnection"))
            {
                Thread LoginThread = new Thread(() => DoLogin(LoginClientWorker.CurrentUser));
                LoginThread.Start();
                Thread UsersListThread = new Thread(() => RequestUsersList(AntVaultClient, LoginClientWorker.CurrentUser));
                UsersListThread.Start();
            }
            if (MessageString.StartsWith("/SendStatus"))
            {
                SetStatus(MessageString);
                Thread.Sleep(200);
                RequestFriendsList(AntVaultClient, LoginClientWorker.CurrentUser);
            }
            if (MessageString.StartsWith("/SendUsernames") || ReceivingUsernames == true)
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
                    Usernames = AuxiliaryClientWorker.GetStringsFromBytes(MessageBytes);
                    RequestProfilePictures(AntVaultClient, LoginClientWorker.CurrentUser);
                }
            }
            if (MessageString.StartsWith("/SendProfilePictures") || ReceivingProfilePictures == true)
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
                    ProfilePictures = AuxiliaryClientWorker.GetPicturesFromBytes(MessageBytes);//Last data request
                    GetCurrentProfilePicture(LoginClientWorker.CurrentUser);
                    RequestStatus(AntVaultClient, LoginClientWorker.CurrentUser);
                }
            }
            if (MessageString.StartsWith("/SendFriendsList") || ReceivingFriendsList == true)
            {
                if (MessageString.StartsWith("/SendFriendsList") == true && ReceivingFriendsList == false)
                {
                    ReceivingProfilePictureUpdatePulse = false;
                    ReceivingProfilePictures = false;
                    ReceivingUsernames = false;
                    ReceivingFriendsList = true;
                }
                else if (MessageString.StartsWith("/SendFriendsList") == false && ReceivingFriendsList == true)
                {
                    ReceivingFriendsList = false;
                    FriendsList = AuxiliaryClientWorker.GetStringsFromBytes(MessageBytes);
                    GetFriendsList();
                }
            }
            if (MessageString.StartsWith("/Message"))
            {
                Thread MessageThread = new Thread(() => HandleMessage(MessageString));
                MessageThread.Start();
            }
            if (MessageString.StartsWith("/UpdateStatus"))//UpdateStatus -U Username -Content Msg.
            {
                HandleStatus(MessageString);
            }
            if (MessageString.StartsWith("/UserUpdatedPicture") || ReceivingProfilePictureUpdatePulse == true)//UserUpdatedPicture -U  + UsernameToUpdate.
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
                    UpdateUserProfilePicture(UpdateSender, MessageBytes);
                    ReceivingProfilePictureUpdatePulse = false;
                    SenderDefined = false;
                }
            }
            if (MessageString.StartsWith("/UserConnected"))
            {
                HandleNewUser(AntVaultClient, MessageString);
            }
        }

        static string UpdateSender = null;

        static bool ReceivingProfilePictures = false;
        static bool ReceivingUsernames = false;
        static bool ReceivingProfilePictureUpdatePulse = false;
        static bool ReceivingFriendsList = false;
        static bool SenderDefined = false;

        private static void HandleNewUser(SimpleSocketClient AntVaultClient,string MessageString)
        {
            string Sender = AuxiliaryClientWorker.GetElement(MessageString, "-U ", ".");
            RequestUsersList(AntVaultClient, LoginClientWorker.CurrentUser);
            MainWindowController.MainWindow.Dispatcher.Invoke(() =>
            {
                #region New user joined pulse
                System.Windows.Controls.Label StatusLabel = new System.Windows.Controls.Label();
                StatusLabel.FontSize = 18;
                StatusLabel.FontStyle = FontStyles.Oblique;
                StatusLabel.FontStyle = FontStyles.Italic;
                StatusLabel.Foreground = System.Windows.Media.Brushes.Black;
                StatusLabel.MouseLeftButtonDown += This;
                StatusLabel.Content = Sender + " has joined the vault!";
                #endregion
                MainWindowController.ChatParagraph.Inlines.Add(StatusLabel);
                MainWindowController.ChatParagraph.Inlines.Add(Environment.NewLine);
                MainWindowController.ChatDocument.Blocks.Add(MainWindowController.ChatParagraph);
                MainWindowController.MainPage.ClientChatTextBox.Document = MainWindowController.ChatDocument;
            });

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
        internal static void UpdateProfilePicture(SimpleSocketClient AntVaultClient, byte[] NewProfilePictureBytes)//UpdateProfilePicture //byte[] //EndUpdateProfilePicture
        {
            AntVaultClient.SendBytes(AuxiliaryClientWorker.MessageByte("/UpdateProfilePicture"));
            Thread.Sleep(500);
            AntVaultClient.SendBytes(NewProfilePictureBytes);
        }

        internal static void UpdatePassword(SimpleSocketClient AntVaultClient, string OldPassword, string NewPassword)//UpdatePassword -P OldPassword -Content NewPassword.
        {
            AntVaultClient.SendBytes(AuxiliaryClientWorker.MessageByte("/UpdatePassword -P " + OldPassword + " -Content " + NewPassword + "."));
        }

        internal static void UpdateUsername(SimpleSocketClient AntVaultClient, string CurrentUsername, string NewUsername)//UpdateUsername -U OldUsername -Content NewUsername.
        {
            AntVaultClient.SendBytes(AuxiliaryClientWorker.MessageByte("/UpdateUsername -U " + CurrentUsername + " -Content " + NewUsername + "."));
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
            LoginClientWorker.AntVaultClient.SendBytes(AuxiliaryClientWorker.MessageByte("/UpdateStatus -U " + LoginClientWorker.CurrentUser + " -Content " + NewStatus + "."));
        }

        internal static void SendMessage(string Message)
        {
            LoginClientWorker.AntVaultClient.SendBytes(AuxiliaryClientWorker.MessageByte("/Message -U " + LoginClientWorker.CurrentUser + " -Content " + Message + "."));
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
            Bitmap CurrentProfilePicture = new Bitmap(ProfilePictures[Usernames.IndexOf(CurrentUser)]);
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
                MainWindowController.LoginPage.PasswordTextBox.Password = null;
                MainWindowController.MainWindow.Width = 800;
                MainWindowController.MainWindow.Height = 450;
                Animations.LoginAnimations.LogoutAnimation(MainWindowController.LoginPage.ConnectButton, MainWindowController.LoginPage.LoginGroupBox, MainWindowController.LoginPage.ServerStatusLabel, MainWindowController.LoginPage.ServerStatusEllipse, MainWindowController.LoginPage.WelcomeLabel);
            });
            FriendsList.Clear();
            Usernames.Clear();
            ProfilePictures.Clear();
        }

        internal static void RequestUsersList(SimpleSocketClient AntVaultClient, string CurrentUser)
        {
            AntVaultClient.SendBytes(AuxiliaryClientWorker.MessageByte("/RequestUsers -U " + CurrentUser + "."));//RequestUsersList -U Username.
        }

        internal static void SetStatus(string MessageString)//SendStatus -Content Status.
        {
            string CurrentStatus = AuxiliaryClientWorker.GetElement(MessageString, "-Content ", ".");
            MainWindowController.MainWindow.Dispatcher.Invoke(() =>
            {
                MainWindowController.MainPage.StatusTextBox.Text = CurrentStatus;
            });
        }

        internal static void RequestProfilePictures(SimpleSocketClient AntVaultClient, string CurrentUser)//RequestProfilePictures -U Username.
        {
            AntVaultClient.SendBytes(AuxiliaryClientWorker.MessageByte("/RequestProfilePictures -U " + CurrentUser + "."));
        }

        internal static void RequestFriendsList(SimpleSocketClient AntVaultClient, string CurrentUser)
        {
            AntVaultClient.SendBytes(AuxiliaryClientWorker.MessageByte("/RequestFriendsList -U " + CurrentUser + "."));//RequestFriendsList -U Username.
        }

        internal static void RequestStatus(SimpleSocketClient AntVaultClient, string CurrentUser)
        {
            AntVaultClient.SendBytes(AuxiliaryClientWorker.MessageByte("/RequestStatus -U " + CurrentUser + "."));//RequestStatus -U Username.
        }
    }
}
