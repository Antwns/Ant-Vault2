using System;
using System.Windows;

namespace AntVault2Client.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow1_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
    }
}