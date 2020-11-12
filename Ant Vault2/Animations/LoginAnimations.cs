using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace AntVault2Client.Animations
{
    public class LoginAnimations
    {
        public static void LoginAnimation(Button ConnectButton, GroupBox LoginGroupBox, TextBlock ServerStatusLabel, Ellipse ServerStatusEllipse, TextBlock WelcomeLabel)
        {
            WindowControllers.MainWindowController.MainWindow.Dispatcher.Invoke(() =>
        {
            #region ConnectButton Animations

            ConnectButton.IsEnabled = true;

            double LeftMargin = ConnectButton.Margin.Left;
            double RightMargin = ConnectButton.Margin.Right;
            double TopMargin = ConnectButton.Margin.Right;
            double BottomMargin = ConnectButton.Margin.Bottom;

            Thickness NewThickness = new Thickness(LeftMargin, TopMargin, RightMargin + 500, BottomMargin);
            ThicknessAnimation ButtonLoginAnimation = new ThicknessAnimation(NewThickness, TimeSpan.FromSeconds(1));

            ConnectButton.BeginAnimation(Button.MarginProperty, ButtonLoginAnimation);

            #endregion

            #region LoginGroupBox Animations

            LoginGroupBox.IsEnabled = true;

            LeftMargin = LoginGroupBox.Margin.Left;
            RightMargin = LoginGroupBox.Margin.Right;
            TopMargin = LoginGroupBox.Margin.Top;
            BottomMargin = LoginGroupBox.Margin.Bottom;

            NewThickness = new Thickness(LeftMargin, TopMargin, RightMargin + 500, BottomMargin);
            ThicknessAnimation LoginGroupBoxAnimation = new ThicknessAnimation(NewThickness, TimeSpan.FromSeconds(1));

            LoginGroupBox.BeginAnimation(GroupBox.MarginProperty, LoginGroupBoxAnimation);

            #endregion

            #region ServerStatusLabel Animations

            LeftMargin = ServerStatusLabel.Margin.Left;
            RightMargin = ServerStatusLabel.Margin.Right;
            TopMargin = ServerStatusLabel.Margin.Top;
            BottomMargin = ServerStatusLabel.Margin.Bottom;

            NewThickness = new Thickness(LeftMargin, TopMargin, RightMargin + 500, BottomMargin);
            ThicknessAnimation ServerStatusLabelAnimation = new ThicknessAnimation(NewThickness, TimeSpan.FromSeconds(1));

            ServerStatusLabel.BeginAnimation(Label.MarginProperty, ServerStatusLabelAnimation);

            #endregion

            #region ServerStatusElipse Animations

            LeftMargin = ServerStatusEllipse.Margin.Left;
            RightMargin = ServerStatusEllipse.Margin.Right;
            TopMargin = ServerStatusEllipse.Margin.Top;
            BottomMargin = ServerStatusEllipse.Margin.Bottom;

            NewThickness = new Thickness(LeftMargin, TopMargin, RightMargin + 500, BottomMargin);
            ThicknessAnimation ServerStatusEllipseAnimation = new ThicknessAnimation(NewThickness, TimeSpan.FromSeconds(1));

            ServerStatusEllipse.BeginAnimation(Ellipse.MarginProperty, ServerStatusEllipseAnimation);

            #endregion

            #region WelcomeLabel Animations

            LeftMargin = WelcomeLabel.Margin.Left;
            RightMargin = WelcomeLabel.Margin.Right;
            TopMargin = WelcomeLabel.Margin.Top;
            BottomMargin = WelcomeLabel.Margin.Bottom;

            NewThickness = new Thickness(LeftMargin, TopMargin, RightMargin + 500, BottomMargin);
            ThicknessAnimation WelcomeLabelAnimation = new ThicknessAnimation(NewThickness, TimeSpan.FromSeconds(1));
            WelcomeLabelAnimation.Completed += WelcomeLabelAnimation_Completed;

            WelcomeLabel.BeginAnimation(Label.MarginProperty, WelcomeLabelAnimation);

            #endregion
        });

        }

        public static void LogoutAnimation(Button ConnectButton, GroupBox LoginGroupBox, TextBlock ServerStatusLabel, Ellipse ServerStatusEllipse, TextBlock WelcomeLabel)
        {
            WindowControllers.MainWindowController.MainWindow.Dispatcher.Invoke(() =>
            {
                #region ConnectButton Animations

                ConnectButton.IsEnabled = true;

                double LeftMargin = ConnectButton.Margin.Left;
                double RightMargin = ConnectButton.Margin.Right;
                double TopMargin = ConnectButton.Margin.Right;
                double BottomMargin = ConnectButton.Margin.Bottom;

                Thickness NewThickness = new Thickness(0, 0, 0, 0);
                ThicknessAnimation ButtonLoginAnimation = new ThicknessAnimation(NewThickness, TimeSpan.FromSeconds(1));

                ConnectButton.BeginAnimation(Button.MarginProperty, ButtonLoginAnimation);

                #endregion

                #region LoginGroupBox Animations

                LoginGroupBox.IsEnabled = true;

                LeftMargin = LoginGroupBox.Margin.Left;
                RightMargin = LoginGroupBox.Margin.Right;
                TopMargin = LoginGroupBox.Margin.Top;
                BottomMargin = LoginGroupBox.Margin.Bottom;

                NewThickness = new Thickness(LeftMargin, TopMargin, RightMargin - 500, BottomMargin);
                ThicknessAnimation LoginGroupBoxAnimation = new ThicknessAnimation(NewThickness, TimeSpan.FromSeconds(1));

                LoginGroupBox.BeginAnimation(GroupBox.MarginProperty, LoginGroupBoxAnimation);

                #endregion

                #region ServerStatusLabel Animations

                LeftMargin = ServerStatusLabel.Margin.Left;
                RightMargin = ServerStatusLabel.Margin.Right;
                TopMargin = ServerStatusLabel.Margin.Top;
                BottomMargin = ServerStatusLabel.Margin.Bottom;

                NewThickness = new Thickness(LeftMargin, TopMargin, RightMargin - 500, BottomMargin);
                ThicknessAnimation ServerStatusLabelAnimation = new ThicknessAnimation(NewThickness, TimeSpan.FromSeconds(1));

                ServerStatusLabel.BeginAnimation(Label.MarginProperty, ServerStatusLabelAnimation);

                #endregion

                #region ServerStatusElipse Animations

                LeftMargin = ServerStatusEllipse.Margin.Left;
                RightMargin = ServerStatusEllipse.Margin.Right;
                TopMargin = ServerStatusEllipse.Margin.Top;
                BottomMargin = ServerStatusEllipse.Margin.Bottom;

                NewThickness = new Thickness(LeftMargin, TopMargin, RightMargin - 500, BottomMargin);
                ThicknessAnimation ServerStatusEllipseAnimation = new ThicknessAnimation(NewThickness, TimeSpan.FromSeconds(1));

                ServerStatusEllipse.BeginAnimation(Ellipse.MarginProperty, ServerStatusEllipseAnimation);

                #endregion

                #region WelcomeLabel Animations

                LeftMargin = WelcomeLabel.Margin.Left;
                RightMargin = WelcomeLabel.Margin.Right;
                TopMargin = WelcomeLabel.Margin.Top;
                BottomMargin = WelcomeLabel.Margin.Bottom;

                NewThickness = new Thickness(LeftMargin, TopMargin, RightMargin - 500, BottomMargin);
                ThicknessAnimation WelcomeLabelAnimation = new ThicknessAnimation(NewThickness, TimeSpan.FromSeconds(1));

                WelcomeLabel.BeginAnimation(Label.MarginProperty, WelcomeLabelAnimation);

                #endregion
            });

        }

        private static void WelcomeLabelAnimation_Completed(object sender, EventArgs e)
        {
            WindowControllers.MainWindowController.MainWindow.Width = 1280;
            WindowControllers.MainWindowController.MainWindow.Height = 1024;
            WindowControllers.MainWindowController.MainWindow.Content = WindowControllers.MainWindowController.MainPage;
        }
    }
}