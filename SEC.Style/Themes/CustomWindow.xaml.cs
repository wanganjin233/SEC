using System;
using System.Windows;
using System.Windows.Input;

namespace SEC.Style
{
    public partial class CustomWindow
    {

        // 拖动
        private void CustomWindow_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
              if (e.LeftButton == MouseButtonState.Pressed)
              {
                  Window win = (Window)((FrameworkElement)sender).TemplatedParent;
                  win.DragMove();
              }
        }

        // 关闭
        private void CustomWindowBtnClose_Click(object sender, RoutedEventArgs e)
        {
            Window win = (Window)((FrameworkElement)sender).TemplatedParent;
            win.Close();
        }

        // 最小化
        private void CustomWindowBtnMinimized_Click(object sender, RoutedEventArgs e)
        {
            Window win = (Window)((FrameworkElement)sender).TemplatedParent;
            win.WindowState = WindowState.Minimized;
        }

        // 最大化、还原
        private void CustomWindowBtnMaxNormal_Click(object sender, RoutedEventArgs e)
        {
            Window win = (Window)((FrameworkElement)sender).TemplatedParent;
            WindowStateCut(win);
        }
        private void WindowStateCut(Window win)
        {
            if (win.WindowState == WindowState.Maximized)
            {
                win.WindowState = WindowState.Normal;
            }
            else
            {
                // 不覆盖任务栏
                win.MaxWidth = SystemParameters.WorkArea.Width;
                win.MaxHeight = SystemParameters.WorkArea.Height;
                win.WindowState = WindowState.Maximized;

            }
        }


        DateTime doubleClickInterval = DateTime.MinValue; 
        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        { 
            if (DateTime.Now - doubleClickInterval < TimeSpan.FromMilliseconds(400))
            { 
                Window win = (Window)((FrameworkElement)sender).TemplatedParent;
                WindowStateCut(win);
            }
            else
            {
                doubleClickInterval = DateTime.Now;
            }
        }
         
    }
}
