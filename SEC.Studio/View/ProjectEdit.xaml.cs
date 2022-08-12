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

namespace SEC.Studio.View
{
    /// <summary>
    /// ProjectEdit.xaml 的交互逻辑
    /// </summary>
    public partial class ProjectEdit : Page
    {
        public ProjectEdit()
        {
            InitializeComponent();
        }

        private void Ctrl_Click(object sender, RoutedEventArgs e)
        {
            new DriveConfig().ShowDialog();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
