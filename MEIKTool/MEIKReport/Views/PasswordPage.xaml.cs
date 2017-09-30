using MEIKReport.Common;
using MEIKReport.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class PasswordPage : Window
    {        
        public PasswordPage()
        {
            InitializeComponent();            
            this.txtPwd.Focus();
        }                        

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if ("admin".Equals(this.txtPwd.Password))
            {
                this.DialogResult = (bool?)true;
                this.Close();
            }
            else
            {                
                MessageBox.Show(this, App.Current.FindResource("Message_49").ToString());
                this.txtPwd.Focus();
            }            

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = (bool?)false;
            this.Close();
        }                
        
    }
}
