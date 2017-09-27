using MEIKScreen.Common;
using MEIKScreen.Model;
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

namespace MEIKScreen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class SelectTechnicianPage : Window
    {              
        public SelectTechnicianPage()
        {
            InitializeComponent();                                     
            //this.txtToMailAddress.Text = App.reportSettingModel.ToMailAddress;
            this.listToMailAddress.ItemsSource = App.reportSettingModel.ToMailAddressList;
            this.txtMailSubject.Text = App.reportSettingModel.MailSubject;
            this.txtMailBody.Text = App.reportSettingModel.MailBody;            
        }                        

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {

            if (this.listToMailAddress.SelectedIndex == -1)
            {
                MessageBox.Show(this, App.Current.FindResource("Message_82").ToString());
                this.listToMailAddress.Focus();                
            }
            else
            {               
                App.reportSettingModel.ToMailAddress = this.listToMailAddress.Text;
                App.reportSettingModel.MailSubject = this.txtMailSubject.Text;
                App.reportSettingModel.MailBody = this.txtMailBody.Text;
                this.DialogResult = (bool?)true;
                this.Close();
            }            

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            App.reportSettingModel.ReportTechName = "";
            this.DialogResult = (bool?)false;
            this.Close();
        }              
        
    }
}
