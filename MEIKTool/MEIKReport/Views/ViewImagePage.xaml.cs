using MEIKReport.Common;
using MEIKReport.Model;
using MEIKReport.Views;
using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace MEIKReport
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ViewImagePage : Window
    {
        
        public ViewImagePage()
        {
            InitializeComponent();           
        }

        public ViewImagePage(byte[] screenShotImg)
        {
            InitializeComponent();
            if (screenShotImg != null)
            {
                this.dataScreenShotImg.Source = ImageTools.GetBitmapImage(screenShotImg);
            }
        } 
        
    }
}
