using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SEC.Studio
{
    /// <summary>
    /// FrmLoading.xaml 的交互逻辑
    /// </summary>
    public partial class FrmLoading : Window
    {
        public FrmLoading()
        { 
            InitializeComponent();
            timer.Tick += Engine;
            timer.Interval = TimeSpan.FromMilliseconds(20);
            backgroundBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/R-C.jpg"));
            background.Fill = backgroundBrush;
            background2.Fill = backgroundBrush;
            Start();
        }
        //创建定时器
        DispatcherTimer timer = new DispatcherTimer();
        //定义图像画刷
        ImageBrush backgroundBrush = new ImageBrush();

        //构造




        private void Engine(object? sender, EventArgs e)
        {
            var backgroundLeft = Canvas.GetLeft(background) - 3;
            var background2Left = Canvas.GetLeft(background2) - 3;

            Canvas.SetLeft(background, backgroundLeft);
            Canvas.SetLeft(background2, background2Left);
            if (backgroundLeft <= -1262)
            {
                timer.Stop();
                // Start();
                this.Close();
            }

        }
        private void Start()
        {
            Canvas.SetLeft(background, 0);
            Canvas.SetLeft(background2, 1262);
            timer.Start();
        }
    }
}
