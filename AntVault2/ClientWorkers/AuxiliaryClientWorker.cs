using System.Collections.ObjectModel;
using System.Text;
using System.Drawing;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System;
using Microsoft.Win32;
using System.Windows;

namespace AntVault2Client.ClientWorkers
{
    class AuxiliaryClientWorker
    {
        public static byte[] MessageByte(string Text)
        {
            return Encoding.ASCII.GetBytes(Text);
        }

        public static string MessageString(byte[] Data)
        {
            return Encoding.UTF8.GetString(Data);
        }

        public static Collection<string> GetCollectionFromBytes(byte[] ArrayToConvert)
        {
            return null;
        }

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

        internal static Collection<Bitmap> GetPicturesFromBytes(byte[] ArrayToConvert)
        {
            BinaryFormatter ProfilePictureFormatter = new BinaryFormatter();
            using (MemoryStream ProfilePictureStream = new MemoryStream())
            {
                ProfilePictureStream.Write(ArrayToConvert, 0, ArrayToConvert.Length);
                ProfilePictureStream.Seek(0, SeekOrigin.Begin);
                try
                {
                    Collection<Bitmap> PicturesToReturn = (Collection<Bitmap>)ProfilePictureFormatter.Deserialize(ProfilePictureStream);
                    return PicturesToReturn;
                }
                catch
                {
                    return null;
                }
            }
        }

        internal static byte[] GetNewProfilePicture()
        {
            byte[] NewProfilePictureBytes = null;
            OpenFileDialog NewProfilePictureSelector = new OpenFileDialog()
            {
                Filter = "png files (*.png)|*.png",
            };
            NewProfilePictureSelector.ShowDialog();
            try
            {
                NewProfilePictureBytes = File.ReadAllBytes(NewProfilePictureSelector.FileName);
            }
            catch
            {
                MessageBox.Show("You did not select an item! Process will be canceled!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                Pages.OptionsPage.ChangedProfilePicture = false;
            }
            MemoryStream ImageCheckerMemoryStream = new MemoryStream(NewProfilePictureBytes);
            try
            {
                Image ImageChecker = Image.FromStream(ImageCheckerMemoryStream);
                if(ImageChecker.Height < 400 && ImageChecker.Width < 400)
                {
                    Pages.OptionsPage.ChangedProfilePicture = true;
                    MessageBox.Show("Selected item was of correct format!", "Success!", MessageBoxButton.OK, MessageBoxImage.Information);
                    return NewProfilePictureBytes;
                }
                else
                {
                    Pages.OptionsPage.ChangedProfilePicture = false;
                    MessageBox.Show("Selected item was not of correct format, max allowed sizes are 400x400", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

            }
            catch
            {
                MessageBox.Show("Selected item was not of image type, please try again", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                Pages.OptionsPage.ChangedProfilePicture = false;
                return null;
            }
        }

        internal static Collection<string> GetStringsFromBytes(byte[] ArrayToConvert)
        {
            BinaryFormatter StringFormatter = new BinaryFormatter();
            using (MemoryStream StringStream = new MemoryStream())
            {
                StringStream.Write(ArrayToConvert, 0, ArrayToConvert.Length);
                StringStream.Seek(0, SeekOrigin.Begin);
                Collection<string> StringsToReturn = (Collection<string>)StringFormatter.Deserialize(StringStream);
                return StringsToReturn;
            }
        }

        internal static Bitmap GetBitmapFromBytes(byte[] Data)
        {
            MemoryStream BitmapConverterStream = new MemoryStream(Data);
            Bitmap BitmapToReturn = new Bitmap(BitmapConverterStream);
            return BitmapToReturn;
        }

        internal static BitmapImage BitmapToBitmapImage(Bitmap InputBitmap)
        {
            MemoryStream BitMapConverterStream = new MemoryStream();
            InputBitmap.Save(BitMapConverterStream, ImageFormat.Png);
            BitmapImage ConvertedBitmapImage = new BitmapImage();
            ConvertedBitmapImage.BeginInit();
            ConvertedBitmapImage.StreamSource = BitMapConverterStream;
            ConvertedBitmapImage.EndInit();
            return ConvertedBitmapImage;
        }
    }
}
