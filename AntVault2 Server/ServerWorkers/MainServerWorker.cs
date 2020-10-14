using System;
using System.IO;
using AntVault2Server.DataAndResources;
using SimpleTcp;
using System.Threading;

namespace AntVault2Server.ServerWorkers
{
    class MainServerWorker
    {
        public static string DefaultConfig = Properties.Resources.DefaultConfig;
        public static bool SetUpEvents;
        public static string UserDirectories;

        public static SimpleTcpServer AntVaultServer = new SimpleTcpServer("*", Convert.ToInt32(AuxiliaryServerWorker.ReadFromConfig(true, false)), false, null, null);
        public static SimpleTcpServer AntVaultStatusServer = new SimpleTcpServer("*", Convert.ToInt32(AuxiliaryServerWorker.ReadFromConfig(true, false)) + 1, false, null, null);

        #region Server setup and controlls
        public static void StartAntVaultStatusServer()
        {
            Thread StatusServerThread = new Thread(StatusThreadWork);
            StatusServerThread.Start();
        }

        public static void StatusThreadWork()
        {
            AntVaultStatusServer.Start();
            AuxiliaryServerWorker.WriteToConsole("[INFO] Status server started on port " + AuxiliaryServerWorker.ReadFromConfig(true, false) + 1);
        }
        public static void StartAntVaultServer()
        {
            if (SetUpEvents == false)
            {
                AntVaultServer.Events.ClientConnected += Events_ClientConnected;
                AntVaultServer.Events.ClientDisconnected += Events_ClientDisconnected;
                AntVaultServer.Events.DataReceived += Events_DataReceived;
                SetUpEvents = true;
            }
            CheckConfig(AuxiliaryServerWorker.ConfigFileDir);
            CheckUserDatabase(AuxiliaryServerWorker.UserDatabaseDir);
            CheckFileFolders();
            AntVaultServer.Start();
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

        private static void CheckConfig(string ConfigFileDir)
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

        public static void StopAntVaultServer()
        {
            foreach (string Client in AntVaultServer.GetClients())
            {
                AuxiliaryServerWorker.WriteToConsole("[INFO] Disconnecting client " + Client);
                AntVaultServer.DisconnectClient(Client);
            }
            AuxiliaryServerWorker.WriteToConsole("[INFO] Clearing arrays...");
            AuxiliaryServerWorker.Usernames.Clear();
            AuxiliaryServerWorker.Passwords.Clear();
            AuxiliaryServerWorker.FriendsList.Clear();
            AuxiliaryServerWorker.ProfilePictures.Clear();
            AuxiliaryServerWorker.Sessions.Clear();
            AuxiliaryServerWorker.Statuses.Clear();
            AuxiliaryServerWorker.WriteToConsole("[INFO] Arrays cleared");
            AntVaultServer.Stop();
            AuxiliaryServerWorker.WriteToConsole("[INFO] Server stopped");
            SetUpEvents = false;
        }
        internal static void StopAntVaultStatusServer()
        {
            AntVaultStatusServer.Stop();
            AuxiliaryServerWorker.WriteToConsole("[INFO] Status server stopped");
        }

        #endregion

        #region Server events
        private static void Events_ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            AuxiliaryServerWorker.WriteToConsole("[INFO] Client " + e.IpPort + " disconnected due to " + e.Reason);
            foreach(Session Sess in AuxiliaryServerWorker.Sessions)
            {
                if (Sess.IpPort == e.IpPort)
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

        private static void Events_ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            AuxiliaryServerWorker.WriteToConsole("[INFO] Received new connection from " + e.IpPort);
        }

        private static void Events_DataReceived(object sender, DataReceivedFromClientEventArgs e)
        {
            string MessageString = AuxiliaryServerWorker.MessageString(e.Data);
            AuxiliaryServerWorker.WriteToConsole("[DEBUG] " + MessageString);
            if (MessageString.StartsWith("/NewConnection"))//NewConnection -U Username -P Password.
            {
                DoAuthentication(MessageString, e);
            }
            if(MessageString.StartsWith("/EndConnection"))//EndConnection -U Username -P Password.
            {
                EndSession(MessageString, e);
            }
            if(MessageString.StartsWith("/Message"))//Message -U Username -Content Msg..
            {
                HandleMessage(MessageString, e);
            }
            if(MessageString.StartsWith("/UpdateStatus"))//UpdateStatus -U Username -Content Msg.
            {
                UpdateStatus(MessageString, e);
            }
            if(MessageString.StartsWith("/UpdateProfilePicture"))//UdateProfilePicture -U Username -Content byte[].
            {
                UpdateProfilePicture(MessageString, e);
            }
            if(MessageString.StartsWith("/SendFriendRequest"))//SendFriendRequest -U Username -Content UserToAdd.
            {
                SendFriendRequest(MessageString, e);
            }
            if(MessageString.StartsWith("/RequestFriendsList"))//RequestFriendsList -U Username.
            {
                SendFriendsList(MessageString, e);
            }
            if(MessageString.StartsWith("/RequestProfilePictures"))//RequestProfilePictures -U Username.
            {
                SendProfilePictures(MessageString, e);
            }
            if(MessageString.StartsWith("/RequestStatus"))//RequestStatus -U Username.
            {
                SendStatus(MessageString, e);
            }
            if(MessageString.StartsWith("/RequestUsers"))//RequestUsers -U Username.
            {
                SendUsers(MessageString, e);
            }
        }

        #endregion

        #region Server handlers

        private static void SendUsers(string MessageString, DataReceivedFromClientEventArgs Client)//SendUsers, byte[] UsersList, //EndSendUsers
        {
            string Sender = AuxiliaryServerWorker.GetElement(MessageString, "-U ", ".");
            AuxiliaryServerWorker.WriteToConsole("[INFO] Sending users list to " + Sender);
            AntVaultServer.Send(Client.IpPort, "/SendUsernames");
            Thread.Sleep(100);
            AntVaultServer.Send(Client.IpPort, AuxiliaryServerWorker.ReturnByteArrayFromStringCollection(AuxiliaryServerWorker.Usernames));
            Thread.Sleep(100);
            AntVaultServer.Send(Client.IpPort, "/EndSendUsernames");
            AuxiliaryServerWorker.WriteToConsole("[INFO] Successfully sent users list to " + Sender);
        }
        private static void SendStatus(string MessageString, DataReceivedFromClientEventArgs Client)
        {
            string Sender = AuxiliaryServerWorker.GetElement(MessageString, "-U ", ".");
            string CurrentStatus = AuxiliaryServerWorker.GetStatus(Sender);
            AntVaultServer.Send(Client.IpPort, AuxiliaryServerWorker.MessageByte("/SendStatus -Content " + CurrentStatus +"."));//SendStatus -Content Status.
            AuxiliaryServerWorker.WriteToConsole("[INFO] Successully sent " + Sender + "'s status");
        }

        private static void SendProfilePictures(string MessageString, DataReceivedFromClientEventArgs Client)
        {
            string Sender = AuxiliaryServerWorker.GetElement(MessageString, "-U ", ".");
            AuxiliaryServerWorker.WriteToConsole("[INFO] User " + Sender + " requested to update their profile picture cache");
            AntVaultServer.Send(Client.IpPort, AuxiliaryServerWorker.MessageByte("/SendProfilePictures")); //SendProfilePictures
            Thread.Sleep(100);
            AntVaultServer.Send(Client.IpPort, AuxiliaryServerWorker.ProfilePictureBytes(AuxiliaryServerWorker.ProfilePictures));
            Thread.Sleep(100);
            AntVaultServer.Send(Client.IpPort, AuxiliaryServerWorker.MessageByte("/EndSendProfilePictures"));
            AuxiliaryServerWorker.WriteToConsole("[INFO] Successfully sent profile picture cache to user " + Sender);
        }

        private static void SendFriendsList(string MessageString, DataReceivedFromClientEventArgs Client)
        {
            string Sender = AuxiliaryServerWorker.GetElement(MessageString, "-U ", ".");
            AuxiliaryServerWorker.WriteToConsole("[INFO] User " + Sender + " requested to update their friends list");
            AntVaultServer.Send(Client.IpPort, AuxiliaryServerWorker.MessageByte("/SendFriendsList"));
            Thread.Sleep(100);
            AntVaultServer.Send(Client.IpPort, AuxiliaryServerWorker.FriendsListBytes(AuxiliaryServerWorker.GetFriendsList(Sender)));
            Thread.Sleep(100);
            AntVaultServer.Send(Client.IpPort,"/EndSendFriendsList");
            AuxiliaryServerWorker.WriteToConsole("[INFO Successfully updated friends list for user " + Sender);
        }

        private static void SendFriendRequest(string MessageString, DataReceivedFromClientEventArgs Client)
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
                AntVaultServer.Send(ReceiverIpPort, AuxiliaryServerWorker.MessageByte(MessageString));
                AuxiliaryServerWorker.WriteToConsole("[INFO] Receiver " + Receiver + " received friend request successfully!");
                AntVaultServer.Send(Client.IpPort, "/FriendRequestSent -U " + Receiver);
            }
            else
            {
                AuxiliaryServerWorker.WriteToConsole("[WARN] Receiver " + Receiver + " was not online to receive their friend request");
                AntVaultServer.Send(Client.IpPort, "/FriendRequestOffline -U " + Receiver);
                //Send offline friend req method here?
            }
            
        }

        private static void UpdateProfilePicture(string MessageString, DataReceivedFromClientEventArgs Client)
        {
            string Sender = AuxiliaryServerWorker.GetElement(MessageString, "-U ", " -Content");
            AuxiliaryServerWorker.WriteToConsole("[INFO] User " + Sender + " sent a profile picture update request");
            AntVaultServer.Send("*", AuxiliaryServerWorker.MessageByte(MessageString));
            AuxiliaryServerWorker.WriteToConsole("[INFO] Profile picture update request finsihed successfully for " + Sender);
        }

        private static void UpdateStatus(string MessageString, DataReceivedFromClientEventArgs Client)
        {
            AntVaultServer.Send("*", AuxiliaryServerWorker.MessageByte(MessageString));
        }

        private static void HandleMessage(string MessageString, DataReceivedFromClientEventArgs Client)
        {
            string Sender = AuxiliaryServerWorker.GetElement(MessageString, "-U ", " -Content");
            string Message = AuxiliaryServerWorker.GetElement(MessageString, "-Content ", ".");
            AuxiliaryServerWorker.WriteToConsole("[" + Sender + "]: " + Message);
            foreach (Session Sess in AuxiliaryServerWorker.Sessions)
            {
                AntVaultServer.Send(Sess.IpPort, AuxiliaryServerWorker.MessageByte(MessageString));
            }
        }

        private static void EndSession(string MessageString, DataReceivedFromClientEventArgs Client)
        {
            AntVaultServer.Send("*", AuxiliaryServerWorker.MessageByte(MessageString));
        }

        private static void DoAuthentication(string MessageString, DataReceivedFromClientEventArgs Client) //NewConnection -U Username -P Password.
        {
            string UsernameC = AuxiliaryServerWorker.GetElement(MessageString, "-U ", " -P");
            string Password = AuxiliaryServerWorker.GetElement(MessageString, "-P ", ".");
            if (AuxiliaryServerWorker.Usernames.Contains(UsernameC))
            {
                if (AuxiliaryServerWorker.Passwords[AuxiliaryServerWorker.Usernames.IndexOf(UsernameC)] == Password)
                {
                    Session NewSession = new Session()
                    {
                        IpPort = Client.IpPort,
                        Username = UsernameC,
                        Friends = AuxiliaryServerWorker.GetFriendsList(UsernameC),
                        LoginTime = DateTime.Now,
                        ProfilePicture = AuxiliaryServerWorker.GetProfilePicture(UsernameC),
                        Status = AuxiliaryServerWorker.Statuses[AuxiliaryServerWorker.Usernames.IndexOf(UsernameC)]
                    };
                    AuxiliaryServerWorker.Sessions.Add(NewSession);
                    AntVaultServer.Send(Client.IpPort, AuxiliaryServerWorker.MessageByte("/AcceptConnection"));//AcceptConnection
                    AuxiliaryServerWorker.WriteToConsole("[INFO] Client " + UsernameC + " successfully authenticated! Creating new session file...");
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
                AuxiliaryServerWorker.WriteToConsole("[WARN] Address " + Client.IpPort + " tried to authenticate with a non-existant username credential!");
            }
        }

        #endregion

        #region Server commands

        internal static void DoServerCommand(string Command)
        {
            if (Command.StartsWith("/") == false)//Basic send message command identifier
            {
                AuxiliaryServerWorker.WriteToConsole("[Server]: " + Command);
                AntVaultServer.Send("*", AuxiliaryServerWorker.MessageByte("/Message -U Server -Content" + Command + "."));
            }
            else
            {

            }
        }

        #endregion
    }
}
