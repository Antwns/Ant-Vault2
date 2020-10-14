using System.Windows;
using System.Windows.Documents;

namespace AntVault2Client.WindowControllers
{
    class MainWindowController
    {
        public static Window MainWindow = new Windows.MainWindow();
        public static Pages.MainPage MainPage = new Pages.MainPage();
        public static Pages.LoginPage LoginPage = new Pages.LoginPage();
        public static FlowDocument ChatDocument = new FlowDocument();
        public static Paragraph ChatParagraph = new Paragraph();
    }
}
