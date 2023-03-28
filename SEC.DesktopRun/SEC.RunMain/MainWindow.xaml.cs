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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SEC.RunMain
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //var Config = TagHandle.Instance.UploadConfig("{\"Name\":\"www\",\"EQU\":\"f\",\"DriverType\":\"f\",\"Tags\":[{\"TagName\":\"uuu\",\"Address\":\"D0000010\",\"DataType\":\"Short\",\"ClientAccess\":\"R/W\",\"EngUnits\":\"kg\",\"Description\":\"www\",\"DataLength\":1},{\"TagName\":\"uuu\",\"Address\":\"D0000017\",\"DataType\":\"Short\",\"ClientAccess\":\"R/W\",\"EngUnits\":\"kg\",\"Description\":\"www\",\"DataLength\":1},{\"TagName\":\"uuu\",\"Address\":\"D0000024\",\"DataType\":\"Short\",\"ClientAccess\":\"R/W\",\"EngUnits\":\"kg\",\"Description\":\"www\",\"DataLength\":1},{\"TagName\":\"uuu\",\"Address\":\"Y0A00\",\"DataType\":\"Short\",\"ClientAccess\":\"R/W\",\"EngUnits\":\"kg\",\"Description\":\"www\",\"DataLength\":1},{\"TagName\":\"uuu\",\"Address\":\"Y0100\",\"DataType\":\"Short\",\"ClientAccess\":\"R/W\",\"EngUnits\":\"kg\",\"Description\":\"www\",\"DataLength\":1},{\"TagName\":\"uuu\",\"Address\":\"D0000064\",\"DataType\":\"Short\",\"ClientAccess\":\"R/W\",\"EngUnits\":\"kg\",\"Description\":\"www\",\"DataLength\":1},{\"TagName\":\"uuu\",\"Address\":\"X0100\",\"DataType\":\"Short\",\"ClientAccess\":\"R/W\",\"EngUnits\":\"kg\",\"Description\":\"www\",\"DataLength\":1}]}");

        }
    }
}
