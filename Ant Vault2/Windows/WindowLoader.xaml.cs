using AntVault2Client.WindowControllers;
using System.Windows;

namespace AntVault2Client.Windows
{
    /// <summary>
    /// Interaction logic for WindowLoader.xaml
    /// </summary>
    public partial class WindowLoader : Window
    {
        public WindowLoader()
        {
            InitializeComponent();
        }

        private void WindowLoader1_Loaded(object sender, RoutedEventArgs e)
        {
            this.Hide();
            WindowControllers.MainWindowController.MainWindow.Show();
            WindowControllers.MainWindowController.MainWindow.Content = MainWindowController.LoginPage;
        }
    }
}
