using System;
using System.IO;
using AntVault2Server.DataAndResources;
using SimpleSockets;
using System.Threading;
using System.Windows;
using SimpleSockets.Server;
using SimpleSockets.Messaging.Metadata;
using System.Linq;
using System.Windows.Controls.Primitives;

namespace AntVault2Server.ServerWorkers
{
    class MainServerWorker
    {
        public static string DefaultConfig = Properties.Resources.DefaultConfig;
        public static bool SetUpEvents;
        public static string UserDirectories;
        public static bool UpdatingProfilePicture;

        internal static SimpleSocketListener AntVaultServer = new SimpleSocketTcpListener();

        #region Server setup and controlls
        public static void StartAntVaultServer()
        {
            if (SetUpEvents == false)
            {
                AntVaultServer.ClientConnected += Events_ClientConnected;
                AntVaultServer.ClientDisconnected += Events_ClientDisconnected;
                AntVaultServer.BytesReceived += Events_DataReceived;
                SetUpEvents = true;
            }
            CheckConfig(AuxiliaryServerWorker.ConfigFileDir);
            CheckUserDatabase(AuxiliaryServerWorker.UserDatabaseDir);
            CheckFileFolders();
            AntVaultServer.StartListening("192.168.1.39", 8910);
            AuxiliaryServerWorker.WriteToConsole("[INFO] Server started on port " + AuxiliaryServerWorker.ReadFromConfig(true,false));
        }

        private static void CheckUserDatabase(string UserDatabaseDir)
        {
            AuxiliaryServerWorker.WriteToConsole("[INFO] Checking user database...");
            if(File.Exists(UserDatabaseDir))
            {
                AuxiliaryServerWorker.WriteToConsole("[INFO] Located user database file at " + UserDatabaseDir);
                AuxiliaryServerWorker.WriteToConsole("[INFO] Reading data from database...");
                AuxiliaryServerWorker.ReadDataBase();
            }
            else
            {
                AuxiliaryServerWorker.WriteToConsole("[ERROR] Could not locate databse file... Creating new database with default username and password");
                try
                {
                    File.AppendAllText(UserDatabaseDir, Properties.Resources.DefaultUserDatabase);
                    AuxiliaryServerWorker.WriteToConsole("[INFO] Created new default database file successfully at " + UserDatabaseDir);
                }
                catch (Exception exc)
                {
                    AuxiliaryServerWorker.WriteToConsole("[ERROR] Could not create new default database file due to " + exc);
                }
            }
        }

        internal static void CheckConfig(string ConfigFileDir)
        {
            if (File.Exists(ConfigFileDir))
            {
                UserDirectories = AuxiliaryServerWorker.ReadFromConfig(false, true);
                if (UserDirectories != null)
                {
                    AuxiliaryServerWorker.WriteToConsole("[INFO] Found user directories entry in config file.");
                    if (UserDirectories != "$UserDirectories$")
                    {
                        AuxiliaryServerWorker.WriteToConsole("[INFO] User directories set to " + UserDirectories);
                        if (Directory.Exists(UserDirectories) == false)
                        {
                            AuxiliaryServerWorker.WriteToConsole("[ERROR] Directory was not found. Creating it now...");
                            try
                            {
                                Directory.CreateDirectory(UserDirectories);
                                AuxiliaryServerWorker.WriteToConsole("[INFO] Directory " + UserDirectories + " was created successfully");
                            }
                            catch (Exception exc)
                            {
                                AuxiliaryServerWorker.WriteToConsole("[ERROR] Directory " + UserDirectories + " could not be created due to " + exc);
                            }
                        }
                        else
                        {
                            AuxiliaryServerWorker.WriteToConsole("[INFO] Custom user directories folder was found on " + UserDirectories);
                        }
                    }
                    else
                    {
                        UserDirectories = AppDomain.CurrentDomain.BaseDirectory + "\\UserDirectories\\";
                        AuxiliaryServerWorker.WriteToConsole("[INFO] Default user directories folder was selected (" + AppDomain.CurrentDomain.BaseDirectory + "\\UserDirectories)");
                        if (Directory.Exists(UserDirectories) == false)
                        {
                            AuxiliaryServerWorker.WriteToConsole("[ERROR] Directory was not found. Creating it now...");
                            try
                            {
                                Directory.CreateDirectory(UserDirectories);
                                AuxiliaryServerWorker.WriteToConsole("[INFO] Directory " + UserDirectories + " was created successfully");
                            }
                            catch (Exception exc)
                            {
                                AuxiliaryServerWorker.WriteToConsole("[ERROR] Directory " + UserDirectories + " could not be created due to " + exc);
                            }
                        }
                        else
                        {
                            AuxiliaryServerWorker.WriteToConsole("[INFO] Default user directories folder was found");
                        }
                    }
                }
            }
            else
            {
                AuxiliaryServerWorker.WriteToConsole("[ERROR] Could not validate the config file. Creating the deault config file now...");
                try
                {
                    File.AppendAllText(ConfigFileDir, DefaultConfig);
                    AuxiliaryServerWorker.WriteToConsole("[INFO] Default config file was created successfully.");
                }
                catch (Exception exc)
                {
                    AuxiliaryServerWorker.WriteToConsole("[ERROR] Default config file could not be created due to " + exc);
                }
            }
        }

        public static void CheckFileFolders()
        {
            int Validated = 0;
            int Created = 0;
            AuxiliaryServerWorker.WriteToConsole("[INFO] Checking user file folders...");
            foreach(string User in AuxiliaryServerWorker.Usernames)
            {
                if(Directory.Exists(UserDirectories + User + "\\Files") == false)
                {
                    AuxiliaryServerWorker.WriteToConsole("[WARN] Could not find file directory for user " + User + " creating it now...");
                    Directory.CreateDirectory(UserDirectories + User + "\\Files");
                    Created++;
                }
                else
                {
                    AuxiliaryServerWorker.WriteToConsole("[INFO] Validated files directory for user " + User);
                    Validated++;
                }
            }
            AuxiliaryServerWorker.WriteToConsole("[INFO] Validated " + Validated + " file directories and created " + Created + " new ones that were missing");
        }

        public static void StopAntVaultServer(SimpleSocketListener Server)
        {
            foreach (string Client in AuxiliaryServerWorker.Clients)
            {
                AuxiliaryServerWorker.WriteToConsole("[INFO] Disconnecting client " + Client);
            }
            AuxiliaryServerWorker.WriteToConsole("[INFO] Clearing arrays...");
            AuxiliaryServerWorker.Usernames.Clear();
            AuxiliaryServerWorker.Passwords.Clear();
            AuxiliaryServerWorker.FriendsList.Clear();
            AuxiliaryServerWorker.ProfilePictures.Clear();
            AuxiliaryServerWorker.Sessions.Clear();
            AuxiliaryServerWorker.Statuses.Clear();
            AuxiliaryServerWorker.WriteToConsole("[INFO] Arrays cleared");
            if (Server.IsRunning == true)
            {
                try
                {
                    //Server.();
                }
                catch(Exception exc)
                {
                    MessageBox.Show(exc.ToString());
                }
            }
            AuxiliaryServerWorker.WriteToConsole("[INFO] Server stopped");
            SetUpEvents = false;
        }

        #endregion

        #region Server events
        private static void Events_ClientDisconnected(IClientInfo Client, SimpleSockets.DisconnectReason e)
        {
            string Sender = null;
            foreach (Session Sess in AuxiliaryServerWorker.Sessions)
            {
                if(Sess.IpPort == Client.RemoteIPv4)
                {
                    Sender = Sess.Username;
                }
            }
            AuxiliaryServerWorker.WriteToConsole("[INFO] Client " + Client.RemoteIPv4 + " disconnected due to " + e);
            foreach (Session Sess in AuxiliaryServerWorker.Sessions)
            {
                if (Sess.IpPort != Client.RemoteIPv4)
                {
                    AntVaultServer.SendBytes(AuxiliaryServerWorker.GetClientID(Client), AuxiliaryServerWorker.MessageByte("/UserDisonnected -U " + Sender + "."));
                }
            }
            foreach(Session Sess in AuxiliaryServerWorker.Sessions)
            {
                if (Sess.IpPort == Client.RemoteIPv4)
                {
                    Sess.LogoutTime = DateTime.Now;
                    try
                    {
                        File.Delete(UserDirectories + Sess.Username + "\\" + Sess.Username + "_lastsession.AntSession");
                        AuxiliaryServerWorker.WriteToConsole("[INFO] Updating session entry for " + Sess.Username);
                        File.WriteAllBytes(UserDirectories + Sess.Username + "\\" + Sess.Username + "_lastsession.AntSession", AuxiliaryServerWorker.GetSessionBytes(Sess));
                        AuxiliaryServerWorker.WriteToConsole("[INFO] Updated session for " + Sess.Username + " successfully");
                        AuxiliaryServerWorker.Sessions.Remove(Sess);
                    }
                    catch (Exception exc)
                    {
                        AuxiliaryServerWorker.WriteToConsole("[ERROR] Could not update session entry for " + Sess.Username + " due to" + exc);
                    }
                    break; //No wasted resources
                }
            }
        }

        private static void Events_ClientConnected(IClientInfo e)
        {
            AuxiliaryServerWorker.WriteToConsole("[INFO] Received new connection from " + e.RemoteIPv4);
        }

        private static void Events_DataReceived(IClientInfo Client, byte[] MessageBytes)
        {
            string MessageString = AuxiliaryServerWorker.MessageString(MessageBytes);
            if (MessageString.StartsWith("�PNG") == false)
            {
                //Do nothing
            }
            else
            {
                MessageString = "[Image]";
            }
            AuxiliaryServerWorker.WriteToConsole("[DEBUG] " + MessageString);
            if (MessageString.StartsWith("/NewConnection"))//NewConnection -U Username -P Password.
            {
                Thread AuthenticationThread = new Thread(() => DoAuthentication(MessageString, Client));
                AuthenticationThread.Start();
            }
            if(MessageString.StartsWith("/EndConnection"))//EndConnection -U Username -P Password.
            {
                Thread EndSessionThread = new Thread(() => EndSession(MessageString, Client));
                EndSessionThread.Start();
            }
            if(MessageString.StartsWith("/Message"))//Message -U Username -Content Msg..
            {
                Thread MessageThread = new Thread(() => HandleMessage(MessageString, Client));
                MessageThread.Start();
            }
            if(MessageString.StartsWith("/UpdateStatus"))//UpdateStatus -U Username -Content Msg.
            {
                Thread StatusUpdater = new Thread(() => UpdateStatus(MessageString, Client));
                StatusUpdater.Start();
            }
            if(MessageString.StartsWith("/UpdateProfilePicture"))//UdateProfilePicture
            {
                Thread ProfilePictureMode = new Thread(() => UpdateProfilePictureModeStart(Client));
                ProfilePictureMode.Start();
            }
            if(MessageString.StartsWith("/SendFriendRequest"))//SendFriendRequest -U Username -Content UserToAdd.
            {
                SendFriendRequest(MessageString, Client);
            }
            if(MessageString.StartsWith("/RequestFriendsList"))//RequestFriendsList -U Username.
            {
                Thread FriendsListSender = new Thread(() => SendFriendsList(MessageString, Client));//Last important info sent <----
                FriendsListSender.Start();
            }
            if(MessageString.StartsWith("/RequestProfilePictures"))//RequestProfilePictures -U Username.
            {
                Thread ProfilePictureSender = new Thread(() => SendProfilePictures(MessageString, Client));
                ProfilePictureSender.Start();
            }
            if(MessageString.StartsWith("/RequestStatus"))//RequestStatus -U Username.
            {
                Thread StatusSender = new Thread(() => SendStatus(MessageString, Client));
                StatusSender.Start();
            }
            if(MessageString.StartsWith("/RequestUsers"))//RequestUsers -U Username.
            {
                Thread UsersSender = new Thread(() => SendUsers(MessageString, Client));
                UsersSender.Start();
            }
            if(UpdatingProfilePicture == true && MessageString.StartsWith("/UpdateProfilePicture") == false)//UpdateProfilePicture
            {
                Thread ProfilepictureUpdater = new Thread(() => UpdateProfilePicture(Client, MessageBytes));
                ProfilepictureUpdater.Start();
                Thread ProfilePictureUpdaterPulse = new Thread(() => ProfilePictureUpdatePulse(Client));
                ProfilePictureUpdaterPulse.Start();
                UpdatingProfilePicture = false;
            }
        }

        #endregion

        #region Server handlers
        private static void ProfilePictureUpdatePulse(IClientInfo Client)
        {
            string UsernameToUpdate = null;
            foreach(Session Sess in AuxiliaryServerWorker.Sessions)
            {
                if (Sess.IpPort == Client.RemoteIPv4)
                {
                    UsernameToUpdate = Sess.Username;
                }
            }
            foreach(Session Sess in AuxiliaryServerWorker.Sessions)
            {
                AntVaultServer.SendBytes(AuxiliaryServerWorker.GetClientIDFromIPPort(Sess.IpPort), AuxiliaryServerWorker.MessageByte("/UserUpdatedPicture -U " + UsernameToUpdate + "."));
                Thread.Sleep(200);
                AntVaultServer.SendBytes(AuxiliaryServerWorker.GetClientIDFromIPPort(Sess.IpPort), AuxiliaryServerWorker.GetBytesFromBitmap(AuxiliaryServerWorker.ProfilePictures[AuxiliaryServerWorker.Usernames.IndexOf(UsernameToUpdate)]));
            }
        }

        private static void UpdateProfilePicture(IClientInfo Client, byte[] Data)
        {
            string Sender = null;
            foreach(Session Sess in AuxiliaryServerWorker.Sessions)
            {
                if (Sess.IpPort == Client.RemoteIPv4)
                {
                    Sender = Sess.Username;
                    Sess.ProfilePicture = AuxiliaryServerWorker.GetBitmapFromBytes(Data);
                    AuxiliaryServerWorker.ProfilePictures[AuxiliaryServerWorker.Usernames.IndexOf(Sender)] = Sess.ProfilePicture;
                    try
                    {
                        AuxiliaryServerWorker.WriteToConsole("[INFO] Previous entry found for " + Sess.Username + " updating it now...");
                        File.Delete(UserDirectories + Sess.Username + "\\ProfilePicture_" + Sess.Username + ".png");
                        File.WriteAllBytes(UserDirectories + Sess.Username + "\\ProfilePicture_" + Sess.Username + ".png", Data);
                    }
                    catch (Exception exc)
                    {
                        AuxiliaryServerWorker.WriteToConsole("[ERROR] Could not update profile picture for user " + Sess.Username + " due to " + exc);
                    }
                    AuxiliaryServerWorker.WriteToConsole("[INFO] Profile picture update request finsihed successfully for " + Sender);
                }
            }
        }

        private static void SendUsers(string MessageString, IClientInfo Client)//SendUsers, byte[] UsersList
        {
            string Sender = AuxiliaryServerWorker.GetElement(MessageString, "-U ", ".");
            AuxiliaryServerWorker.WriteToConsole("[INFO] Sending users list to " + Sender);
            AntVaultServer.SendBytes(AuxiliaryServerWorker.GetClientID(Client), AuxiliaryServerWorker.MessageByte("/SendUsernames"));
            Thread.Sleep(100);
            AntVaultServer.SendBytes(AuxiliaryServerWorker.GetClientID(Client), AuxiliaryServerWorker.ReturnByteArrayFromStringCollection(AuxiliaryServerWorker.Usernames));
            AuxiliaryServerWorker.WriteToConsole("[INFO] Successfully sent users list to " + Sender);
        }
        private static void SendStatus(string MessageString, IClientInfo Client)
        {
            string Sender = AuxiliaryServerWorker.GetElement(MessageString, "-U ", ".");
            string CurrentStatus = AuxiliaryServerWorker.GetStatus(Sender);
            AntVaultServer.SendBytes(AuxiliaryServerWorker.GetClientID(Client), AuxiliaryServerWorker.MessageByte("/SendStatus -Content " + CurrentStatus +"."));//SendStatus -Content Status.
            AuxiliaryServerWorker.WriteToConsole("[INFO] Successully sent " + Sender + "'s status");
        }

        private static void SendProfilePictures(string MessageString, IClientInfo Client)
        {
            string Sender = AuxiliaryServerWorker.GetElement(MessageString, "-U ", ".");
            AuxiliaryServerWorker.WriteToConsole("[INFO] User " + Sender + " requested to update their profile picture cache");
            AntVaultServer.SendBytes(AuxiliaryServerWorker.GetClientID(Client), AuxiliaryServerWorker.MessageByte("/SendProfilePictures")); //SendProfilePictures
            Thread.Sleep(500);
            AntVaultServer.SendBytes(AuxiliaryServerWorker.GetClientID(Client), AuxiliaryServerWorker.ProfilePictureBytes(AuxiliaryServerWorker.ProfilePictures));
            AuxiliaryServerWorker.WriteToConsole("[INFO] Successfully sent profile picture cache to user " + Sender);
        }

        private static void SendFriendsList(string MessageString, IClientInfo Client)
        {
            string Sender = AuxiliaryServerWorker.GetElement(MessageString, "-U ", ".");
            AuxiliaryServerWorker.WriteToConsole("[INFO] User " + Sender + " requested to update their friends list");
            AntVaultServer.SendBytes(AuxiliaryServerWorker.GetClientID(Client), AuxiliaryServerWorker.MessageByte("/SendFriendsList"));
            Thread.Sleep(100);
            AntVaultServer.SendBytes(AuxiliaryServerWorker.GetClientID(Client), AuxiliaryServerWorker.FriendsListBytes(AuxiliaryServerWorker.GetFriendsList(Sender)));
            AuxiliaryServerWorker.WriteToConsole("[INFO Successfully updated friends list for user " + Sender);
        }

        private static void SendFriendRequest(string MessageString, IClientInfo Client)
        {
            string Sender = AuxiliaryServerWorker.GetElement(MessageString, "-U ", " -Content");
            string Receiver = AuxiliaryServerWorker.GetElement(MessageString, "-Content ", ".");
            AuxiliaryServerWorker.WriteToConsole("[INFO] User " + Sender + " sent a friend request to " + Receiver);
            string ReceiverIpPort = null;
            bool FoundOnline = false;
            foreach(Session Sess in AuxiliaryServerWorker.Sessions)
            {
                if (Sess.Username == Receiver)
                {
                    ReceiverIpPort = Sess.IpPort;
                    FoundOnline = true;
                }
            }
            if (FoundOnline == true)
            {
                AntVaultServer.SendBytes(AuxiliaryServerWorker.GetClientIDFromIPPort(ReceiverIpPort), AuxiliaryServerWorker.MessageByte(MessageString));
                AuxiliaryServerWorker.WriteToConsole("[INFO] Receiver " + Receiver + " received friend request successfully!");
                AntVaultServer.SendBytes(AuxiliaryServerWorker.GetClientID(Client), AuxiliaryServerWorker.MessageByte("/FriendRequestSent -U " + Receiver));
            }
            else
            {
                AuxiliaryServerWorker.WriteToConsole("[WARN] Receiver " + Receiver + " was not online to receive their friend request");
                AntVaultServer.SendBytes(AuxiliaryServerWorker.GetClientID(Client), AuxiliaryServerWorker.MessageByte("/FriendRequestOffline -U " + Receiver));
                //Send offline friend req method here?
            }
            
        }

        private static void UpdateProfilePictureModeStart(IClientInfo Client)
        {
            UpdatingProfilePicture = true;
            string Sender = null;
            foreach(Session Sess in AuxiliaryServerWorker.Sessions)
            {
                if(Sess.IpPort == Client.RemoteIPv4)
                {
                    Sender = Sess.Username;
                }
            }
            AuxiliaryServerWorker.WriteToConsole("[INFO] User " + Sender + " sent a profile picture update request");
        }

        private static void UpdateStatus(string MessageString, IClientInfo Client)//UpdateStatus -U Username -Content Msg.
        {
            string Sender = AuxiliaryServerWorker.GetElement(MessageString, "-U ", " -Content");
            string NewStatus = AuxiliaryServerWorker.GetElement(MessageString, "-Content ", ".");
            AuxiliaryServerWorker.WriteToConsole("[INFO] User " + Sender + " updated their status to " + NewStatus);
            foreach (Session Sess in AuxiliaryServerWorker.Sessions)
            {
                if(Sess.Username == Sender)
                {
                    Sess.Status = NewStatus;
                    AuxiliaryServerWorker.WriteToConfig(Sender, NewStatus, false, false, true);
                }
                AntVaultServer.SendBytes(AuxiliaryServerWorker.GetClientIDFromIPPort(Sess.IpPort), AuxiliaryServerWorker.MessageByte(MessageString));
            }
        }

        private static void HandleMessage(string MessageString, IClientInfo Client)
        {
            string Sender = AuxiliaryServerWorker.GetElement(MessageString, "-U ", " -Content");
            string Message = AuxiliaryServerWorker.GetElement(MessageString, "-Content ", ".");
            AuxiliaryServerWorker.WriteToConsole("[" + Sender + "]: " + Message);
            foreach (Session Sess in AuxiliaryServerWorker.Sessions)
            {
                AntVaultServer.SendBytes(AuxiliaryServerWorker.GetClientIDFromIPPort(Sess.IpPort), AuxiliaryServerWorker.MessageByte(MessageString));
            }
        }

        private static void EndSession(string MessageString, IClientInfo Client)
        {
            foreach (IClientInfo CL in AntVaultServer.GetConnectedClients().Values)
            {
                AntVaultServer.SendBytes(CL.Id, AuxiliaryServerWorker.MessageByte(MessageString));
            }
        }

        private static void DoAuthentication(string MessageString, IClientInfo Client) //NewConnection -U Username -P Password.
        {
            string UsernameC = AuxiliaryServerWorker.GetElement(MessageString, "-U ", " -P");
            string Password = AuxiliaryServerWorker.GetElement(MessageString, "-P ", ".");
            if (AuxiliaryServerWorker.Usernames.Contains(UsernameC))
            {
                if (AuxiliaryServerWorker.Passwords[AuxiliaryServerWorker.Usernames.IndexOf(UsernameC)] == Password)//Client authenticated successfully
                {
                    Session NewSession = new Session()
                    {
                        IpPort = Client.RemoteIPv4,
                        Username = UsernameC,
                        Friends = AuxiliaryServerWorker.GetFriendsList(UsernameC),
                        LoginTime = DateTime.Now,
                        ProfilePicture = AuxiliaryServerWorker.GetProfilePicture(UsernameC),
                        Status = AuxiliaryServerWorker.Statuses[AuxiliaryServerWorker.Usernames.IndexOf(UsernameC)]
                    };
                    AuxiliaryServerWorker.Sessions.Add(NewSession);
                    AntVaultServer.SendBytes(AuxiliaryServerWorker.GetClientID(Client), AuxiliaryServerWorker.MessageByte("/AcceptConnection"));//AcceptConnection
                    AuxiliaryServerWorker.WriteToConsole("[INFO] Client " + UsernameC + " successfully authenticated! Creating new session file...");
                    foreach(Session Sess in AuxiliaryServerWorker.Sessions)
                    {
                        AntVaultServer.SendBytes(AuxiliaryServerWorker.GetClientIDFromIPPort(Sess.IpPort), AuxiliaryServerWorker.MessageByte("/UserConnected -U " + UsernameC + "."));
                    }
                    if (File.Exists(UserDirectories + UsernameC + "\\" + UsernameC + "_lastsession.AntSession"))
                    {
                        try
                        {
                            AuxiliaryServerWorker.WriteToConsole("[INFO] Previous session file located. Updating it now...");
                            File.Delete(UserDirectories + UsernameC + "\\" + UsernameC + "_lastsession.AntSession");
                        }
                        catch (Exception exc)
                        {
                            AuxiliaryServerWorker.WriteToConsole("[ERROR] Could not update session file for " + UsernameC + " due to " + exc);
                        }
                    }
                    try
                    {
                        File.WriteAllBytes(UserDirectories + UsernameC + "\\" + UsernameC + "_lastsession.AntSession", AuxiliaryServerWorker.GetSessionBytes(NewSession));
                        AuxiliaryServerWorker.WriteToConsole("[INFO] Successfully appended session file for " + UsernameC);
                    }
                    catch (Exception exc)
                    {
                        AuxiliaryServerWorker.WriteToConsole("[ERROR] Could not append session file for " + UsernameC + " due to " + exc);
                    }
                }
                else
                {
                    AuxiliaryServerWorker.WriteToConsole("[WARN] Client " + UsernameC + " tried to authenticate with a wrong password!");
                }
            }
            else
            {
                AuxiliaryServerWorker.WriteToConsole("[WARN] Address " + Client.RemoteIPv4 + " tried to authenticate with a non-existant username credential!");
            }
        }

        #endregion

        #region Server commands

        internal static void DoServerCommand(string Command)
        {
            if (Command.StartsWith("/") == false)//Basic send message command identifier
            {
                AuxiliaryServerWorker.WriteToConsole("[Server]: " + Command);
                foreach(IClientInfo Client in AntVaultServer.GetConnectedClients().Values)
                AntVaultServer.SendBytes(Client.Id, AuxiliaryServerWorker.MessageByte("/Message -U Server -Content" + Command + "."));
            }
            else
            {

            }
        }

        #endregion
    }
}
