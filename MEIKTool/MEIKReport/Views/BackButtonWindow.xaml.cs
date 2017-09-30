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

namespace MEIKReport.Views
{
    /// <summary>
    /// BackButtonWindow.xaml 的交互逻辑
    /// </summary>
    public partial class BackButtonWindow : Window
    {
        public BackButtonWindow()
        {
            InitializeComponent();
            this.Left = System.Windows.SystemParameters.PrimaryScreenWidth - 300;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //this.Close();
            this.Owner.Show();
        }
    }
}
