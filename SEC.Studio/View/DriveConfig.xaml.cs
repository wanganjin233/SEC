using System;
using System.Collections.Generic;
using System.IO;
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

namespace SEC.Studio.View
{
    /// <summary>
    /// DriveConfig.xaml 的交互逻辑
    /// </summary>
    public partial class DriveConfig : Window
    {
        public DriveConfig()
        {
            InitializeComponent(); 
            string DrivePath = $"{AppDomain.CurrentDomain.BaseDirectory}Drivers";
            var DriverDlls = Directory.GetFiles(DrivePath, "SEC.Driver.*.dll");
            foreach (var DriverDll in DriverDlls)
            {
                DriveName.Items.Add(DriverDll.Replace($"{DrivePath}\\SEC.Driver.", "").Replace(".dll",""));
            }

        }
    }
}
