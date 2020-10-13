using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace AntVault2Client.Animations
{
    class ObjectAnimations
    {
        public static Thickness OriginalThickness = new Thickness(0, 0, 0, 0);
        public static void ButtonMouseOverAnimation(Button CurrentControll)
        {
            Thread MouseOverAnimationThread = new Thread(() => ButtonMouseOverAnimationWork(CurrentControll));
            MouseOverAnimationThread.Start();
        }

        public static void ButtonMouseOverAnimationWork(Button CurrentControll)
        {
                WindowControllers.MainWindowController.MainWindow.Dispatcher.Invoke(() =>
                {
                    Thickness NewButtonThickness = new Thickness(CurrentControll.Margin.Left, CurrentControll.Margin.Top + 10, CurrentControll.Margin.Right, CurrentControll.Margin.Bottom);
                    ThicknessAnimation ButtonMouseOverAnimation = new ThicknessAnimation(NewButtonThickness, TimeSpan.FromMilliseconds(200));
                    ButtonMouseOverAnimation.Completed += (sender, e) => ButtonMouseOverAnimation_Completed(sender, e, CurrentControll);
                    CurrentControll.BeginAnimation(Button.MarginProperty, ButtonMouseOverAnimation);
                });
        }

        private static void ButtonMouseOverAnimation_Completed(object sender, EventArgs e, Button CurrentControll)
        {
            WindowControllers.MainWindowController.MainWindow.Dispatcher.Invoke(() =>
            {
                Thickness OriginalButtonThickness = new Thickness(0 ,0 ,0 ,0);
                ThicknessAnimation ButtonResetAnimation = new ThicknessAnimation(OriginalButtonThickness, TimeSpan.FromMilliseconds(200));
                CurrentControll.BeginAnimation(Button.MarginProperty, ButtonResetAnimation);
            });
        }
    }
}
