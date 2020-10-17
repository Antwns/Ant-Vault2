using AntVault2Server.DataAndResources;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Controls;

namespace AntVault2Server.ServerWorkers
{
    class AuxiliaryServerWorker
    {
        public static Collection<string> Usernames = new Collection<string>();
        public static Collection<string> Passwords = new Collection<string>();
        public static Collection<Bitmap> ProfilePictures = new Collection<Bitmap>();
        public static Collection<string> Statuses = new Collection<string>();
        public static Collection<string> FriendsList = new Collection<string>();
        public static Collection<Session> Sessions = new Collection<Session>();

        public static string UserDatabaseDir = AppDomain.CurrentDomain.BaseDirectory + "AntVaultServer.users";
        public static string ConfigFileDir = AppDomain.CurrentDomain.BaseDirectory + "AntVaultServer.config";
        public static string GetElement(string SourceString, string Start, string End)
        {
            if (SourceString.Contains(Start) && SourceString.Contains(End))
            {
                int StartPos, EndPos;
                StartPos = SourceString.IndexOf(Start, 0) + Start.Length;
                EndPos = SourceString.IndexOf(End, StartPos);
                return SourceString.Substring(StartPos, EndPos - StartPos);
            }
            return "";
        }
        public static string ReadFromConfig(bool ReadPort, bool ReadUserDirectories)
        {
            StreamReader ConfigStreamReader = new StreamReader(ConfigFileDir);
            string CurrentLine;
            int LineNumber = 0;
            while ((CurrentLine = ConfigStreamReader.ReadLine()) != null)
            {
                if(ReadPort == true && CurrentLine.StartsWith("/Port"))
                {
                    return GetElement(CurrentLine, "/Port " ,".");
                }
                if(ReadUserDirectories == true && CurrentLine.StartsWith("/UserDirectories"))
                {
                    return GetElement(CurrentLine, "/UserDirectories ", ".");
                }
                LineNumber++;
            }
            ConfigStreamReader.Close();
            ConfigStreamReader.Dispose();
            WriteToConsole("[INFO] Successfully appended " + LineNumber + " lines from config.");
            return null;
        }

        internal static void ReadDataBase()
        {
            StreamReader DataBaseStreamReader = new StreamReader(UserDatabaseDir);
            bool ErrorsFound = false;
            string CurrentLine;
            int LineNumber = 0;
            while ((CurrentLine = DataBaseStreamReader.ReadLine()) != null)
            {
                if(CurrentLine.StartsWith("/u") && CurrentLine.EndsWith("."))
                {
                    Usernames.Add(GetElement(CurrentLine, "/u ", " /p"));
                    Passwords.Add(GetElement(CurrentLine, "/p ", " /s"));
                    Statuses.Add(GetElement(CurrentLine, "/s ", "."));
                    try
                    {
                        ProfilePictures.Add(GetBitmapFromBytes(File.ReadAllBytes(MainServerWorker.UserDirectories + Usernames[LineNumber] + "\\ProfilePicture_" + Usernames[LineNumber] + ".png")));
                    }
                    catch
                    {
                        Bitmap ProfPicToAdd = new Bitmap(Properties.Resources.DefaultProfilePicture);
                        WriteToConsole("[ERROR] Could not find profile picture for " + Usernames[LineNumber] + " using the default picture instead");
                        try
                        {
                            ProfPicToAdd.Save(MainServerWorker.UserDirectories + Usernames[LineNumber] + "\\ProfilePicture_" + Usernames[LineNumber] + ".png");
                        }
                        catch(Exception exc)
                        {
                            WriteToConsole("[ERROR] Could not find profile picture for " + Usernames[LineNumber] + " due to " + exc);
                        }
                        ProfilePictures.Add(ProfPicToAdd);
                        //use default pic
                    }
                    if (Usernames[LineNumber] != null && Passwords[LineNumber] != null && Statuses[LineNumber] != null)
                    {
                        WriteToConsole("[INFO] Successfully verified account " + Usernames[LineNumber]);
                    }
                    else
                    {
                        WriteToConsole("[ERROR] Could not verify account for " + Usernames[LineNumber]);
                        ErrorsFound = true;
                    }
                    WriteToConsole("[INFO] Checking user directory for " + Usernames[LineNumber]);
                    if(Directory.Exists(MainServerWorker.UserDirectories + Usernames[LineNumber]) == false)
                    {
                        WriteToConsole("[ERROR] Couldn't validate directory for user " + Usernames[LineNumber] + " creating it now...");
                        try
                        {
                            Directory.CreateDirectory(MainServerWorker.UserDirectories + Usernames[LineNumber]);
                            WriteToConsole("[INFO] Succcessfully created user directory for " + Usernames[LineNumber]);
                            WriteToConsole("[INFO] Creating default profile picture for " + Usernames[LineNumber]);
                            try
                            {
                                Bitmap DeaultProfilePicture = new Bitmap(Properties.Resources.DefaultProfilePicture);
                                System.Drawing.Image DefaultProfilePictureImage = DeaultProfilePicture;
                                DefaultProfilePictureImage.Save(MainServerWorker.UserDirectories + Usernames[LineNumber] + "\\ProfilePicture_" + Usernames[LineNumber] + ".png",ImageFormat.Png);
                                WriteToConsole("[INFO] Successfully created user profile picture for " + Usernames[LineNumber]);
                                DeaultProfilePicture.Dispose();
                            }
                            catch (Exception exc)
                            {
                                WriteToConsole("[INFO] Could not create default profile picture for user " + Usernames[LineNumber] + " due to " + exc);
                            }
                        }
                        catch
                        {
                            WriteToConsole("[ERROR] Could not create directory for " + Usernames[LineNumber]);
                        }
                    }
                    else
                    {
                        WriteToConsole("[INFO] Successfully validated directory for user " + Usernames[LineNumber]);
                    }
                }
                else
                {
                    WriteToConsole("[INFO] Incorrect initialization in the database format");
                    ErrorsFound = true;
                    break;
                }
                LineNumber++;
            }
            DataBaseStreamReader.Close();
            DataBaseStreamReader.Dispose();
            if (ErrorsFound == false)
            {
                WriteToConsole("[INFO] Successfully appended " + LineNumber + " accounts from the database.");
            }
            else
            {
                WriteToConsole("[ERROR] There were one or more errors in the database format. If you know how to fix them then please inspect the database, otherwise delete it so a new one can generate");
            }
        }

        internal static void UpdateFriendsList(string Username, Collection<string> NewFriendsList, string UserToAdd)//updated friends list FOR a user
        {
            WriteToConsole("[INFO] Attempting to update friends list file for " + Username);
            NewFriendsList.Add(UserToAdd);
            if (File.Exists(MainServerWorker.UserDirectories + Username + "\\FriendsList_" + Username + ".AntFriends"))
            {
                try
                {
                    File.Delete(MainServerWorker.UserDirectories + Username + "\\FriendsList_" + Username + ".AntFriends");
                    File.WriteAllBytes(MainServerWorker.UserDirectories + Username + "\\FriendsList_" + Username + ".AntFriends", FriendsListBytes(NewFriendsList));
                    WriteToConsole("[INFO] Friends list successfully updated for " + Username);
                }
                catch (Exception exc)
                {
                    WriteToConsole("[ERROR] Could not update friends list for " + Username + " due to " + exc);
                }
            }
            else
            {
                WriteToConsole("[WARN] Friends list file not found for user " + Username + " creating default friends list file now...");
                Collection<string> FriendsList = new Collection<string>();
                FriendsList.Add("User");
                try
                {
                    File.WriteAllBytes(MainServerWorker.UserDirectories + Username + "\\FriendsList_" + Username + ".AntFriends", FriendsListBytes(FriendsList));
                    WriteToConsole("[INFO] New friends list file created successfully for " + Username);
                }
                catch (Exception exc)
                {
                    WriteToConsole("[ERROR] Could not create new default friends list file for " + Username + " due to " + exc);
                }
            }
        }

        internal static string GetStatus(string Sender)
        {
            WriteToConsole("[INFO] " + Sender + " requested their current status");
            string CurrentStatus = null;
            foreach(Session Sess in Sessions)
            {
                if(Sess.Username == Sender)
                {
                    CurrentStatus = Sess.Status;
                    break;
                }
            }
            return CurrentStatus;
        }

        internal static Collection<string> GetFriendsList(string Sender)
        {
            WriteToConsole("[INFO] Acquiring friends list for " + Sender);
            if (File.Exists(MainServerWorker.UserDirectories + Sender + "\\FriendsList_" + Sender + ".AntFriends"))
            {
                try
                {
                    Collection<string> FriendsList = ReadStringCollectionFromByteArrray(File.ReadAllBytes(MainServerWorker.UserDirectories + Sender + "\\FriendsList_" + Sender + ".AntFriends"));
                    WriteToConsole("[INFO] Successfully acquired friends list for " + Sender);
                    return FriendsList;
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                WriteToConsole("[WARN] Friends list file not found for user " + Sender + " creating default friends list file now...");
                Collection<string> FriendsList = new Collection<string>();
                FriendsList.Add("User");
                try
                {
                    File.WriteAllBytes(MainServerWorker.UserDirectories + Sender + "\\FriendsList_" + Sender + ".AntFriends" ,FriendsListBytes(FriendsList));
                    WriteToConsole("[INFO] New friends list file created successfully for " + Sender);
                    return FriendsList;
                }
                catch (Exception exc)
                {
                    WriteToConsole("[ERROR] Could not create new default friends list file for " + Sender + " due to " + exc);
                    return null;
                }
            }
        }

        internal static Bitmap GetProfilePicture(string Username)
        {
            Bitmap ProfilePictureForUser = GetBitmapFromBytes(File.ReadAllBytes(MainServerWorker.UserDirectories + Username + "\\ProfilePicture_" + Username + ".png"));
            return ProfilePictureForUser;
        }

        public static void WriteToConsole(string Text)
        {
            WindowControllers.MainWindowController.MainWindow.Dispatcher.Invoke(() =>
            {
                WindowControllers.MainWindowController.MainWindow.ServerConsoleTextBox.Text += Text + Environment.NewLine;
            });
        }

        internal static byte[] GetSessionBytes(Session Session)
        {
            BinaryFormatter SessionFormatter = new BinaryFormatter();
            using (MemoryStream SessionStream = new MemoryStream())
            {
                SessionFormatter.Serialize(SessionStream, Session);
                byte[] SessionArray = SessionStream.ToArray();
                SessionStream.Dispose();
                return SessionArray;
            }
        }

        public static byte[] MessageByte(string Text)
        {
            return Encoding.ASCII.GetBytes(Text);
        }

        public static string MessageString(byte[] Data)
        {
            return Encoding.UTF8.GetString(Data);
        }

        internal static byte[] FriendsListBytes(Collection<string> CollectionToConvert)
        {
            BinaryFormatter CollectionFormatter = new BinaryFormatter();
            using (MemoryStream CollectionStream = new MemoryStream())
            {
                CollectionFormatter.Serialize(CollectionStream, CollectionToConvert);
                byte[] CollectionArray = CollectionStream.ToArray();
                CollectionStream.Dispose();
                return CollectionArray;
            }
        }
        internal static byte[] ProfilePictureBytes(Collection<Bitmap> CollectionToConvert)
        {
            BinaryFormatter CollectionFormatter = new BinaryFormatter();
            using (MemoryStream CollectionStream = new MemoryStream())
            {
                CollectionFormatter.Serialize(CollectionStream, CollectionToConvert);
                byte[] CollectionArray = CollectionStream.ToArray();
                CollectionStream.Dispose();
                return CollectionArray;
            }
        }

        internal static byte[] GetBytesFromBitmap(Bitmap BitmapToRead)
        {
            byte[] BytesToreturn = null;
            MemoryStream BitmapConverterStream = new MemoryStream();
            BitmapToRead.Save(BitmapConverterStream, ImageFormat.Png);
            BytesToreturn = BitmapConverterStream.ToArray();
            return BytesToreturn;
        }

        internal static Bitmap GetBitmapFromBytes(byte[] Data)
        {
            MemoryStream ImageStreamConverter = new MemoryStream(Data);
            System.Drawing.Image ImageToSave = System.Drawing.Image.FromStream(ImageStreamConverter);
            Bitmap BitmapToReturn = new Bitmap(ImageToSave);
            return BitmapToReturn;
        }

        internal static byte[] ReturnByteArrayFromStringCollection(Collection<string> CollectionToConvert)
        {
            BinaryFormatter CollectionFormatter = new BinaryFormatter();
            using (MemoryStream CollectionStream = new MemoryStream())
            {
                CollectionFormatter.Serialize(CollectionStream, CollectionToConvert);
                byte[] CollectionArray = CollectionStream.ToArray();
                CollectionStream.Dispose();
                return CollectionArray;
            }
        }
        internal static Collection<string> ReadStringCollectionFromByteArrray(byte[] FileData)
        {
            if (FileData == null)
            {
                return null;
            }
            else
            {
                BinaryFormatter CollectionFormatter = new BinaryFormatter();
                using (MemoryStream StreamConverter = new MemoryStream(FileData))
                {
                    Collection<string> CollectionToReturn = (Collection<string>)CollectionFormatter.Deserialize(StreamConverter);
                    return CollectionToReturn;
                }
            }
        }

        internal static void WriteToConfig(string UsernameToWorkOn, string NewEntry, bool Username, bool Password, bool Status)
        {
            if (Username == true)
            {
                string OldFile = File.ReadAllText(UserDatabaseDir);
                string OldLine = "/u " + UsernameToWorkOn + " /p " + Passwords[Usernames.IndexOf(UsernameToWorkOn)] + " /s " + Statuses[Usernames.IndexOf(UsernameToWorkOn)] + ".";
                string NewLine = "/u " + NewEntry + " /p " + Passwords[Usernames.IndexOf(UsernameToWorkOn)] + " /s " + Statuses[Usernames.IndexOf(UsernameToWorkOn)] + ".";
                Usernames[Usernames.IndexOf(UsernameToWorkOn)] = NewEntry;
                string NewFile = OldFile.Replace(OldLine, NewLine);
                File.WriteAllText(UserDatabaseDir, NewFile);
                WriteToConsole("[INFO] Successfully updated username for " + UsernameToWorkOn + ", new username is " + NewEntry);
            }
            if(Password == true)
            {
                string OldFile = File.ReadAllText(UserDatabaseDir);
                string OldLine = "/u " + UsernameToWorkOn + " /p " + Passwords[Usernames.IndexOf(UsernameToWorkOn)] + " /s " + Statuses[Usernames.IndexOf(UsernameToWorkOn)] + ".";
                string NewLine = "/u " + Usernames[Usernames.IndexOf(UsernameToWorkOn)] + " /p " + NewEntry + " /s " + Statuses[Usernames.IndexOf(UsernameToWorkOn)] + ".";
                Passwords[Usernames.IndexOf(UsernameToWorkOn)] = NewEntry;
                string NewFile = OldFile.Replace(OldLine, NewLine);
                File.WriteAllText(UserDatabaseDir, NewFile);
                WriteToConsole("[INFO] Successfully updated password for " + UsernameToWorkOn);
            }
            if(Status == true)
            {
                string OldFile = File.ReadAllText(UserDatabaseDir);
                string OldLine = "/u " + UsernameToWorkOn + " /p " + Passwords[Usernames.IndexOf(UsernameToWorkOn)] + " /s " + Statuses[Usernames.IndexOf(UsernameToWorkOn)] + ".";
                string NewLine = "/u " + Usernames[Usernames.IndexOf(UsernameToWorkOn)] + " /p " + Passwords[Usernames.IndexOf(UsernameToWorkOn)] + " /s " + NewEntry + ".";
                Statuses[Usernames.IndexOf(UsernameToWorkOn)] = NewEntry;
                string NewFile = OldFile.Replace(OldLine, NewLine);
                File.WriteAllText(UserDatabaseDir, NewFile);
                WriteToConsole("[INFO] Successfully updated status for " + UsernameToWorkOn + ", new status is " + NewEntry);
            }
        }
    }
}
