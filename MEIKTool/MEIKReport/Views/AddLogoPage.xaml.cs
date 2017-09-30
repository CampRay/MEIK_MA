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
    public partial class AddLogoPage : Window
    {        
        public AddLogoPage()
        {
            InitializeComponent();
            this.txtDeviceNo.Focus();            
        }                        

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.txtDeviceNo.Text))
            {
                MessageBox.Show(this, App.Current.FindResource("Message_84").ToString());
                this.txtDeviceNo.Focus();
            }
            //else if (string.IsNullOrEmpty(this.txtLogoPath.Text))
            //{
            //    MessageBox.Show(this, App.Current.FindResource("Message_85").ToString());               
            //}            
            else
            {
                foreach (var item in App.reportSettingModel.DeciceLogo)
                {
                    if (item.Device.Equals(this.txtDeviceNo.Text))
                    {
                        MessageBox.Show(this, App.Current.FindResource("Message_86").ToString());
                        return;
                    }
                }
                Logo logo = new Logo();
                logo.Device = this.txtDeviceNo.Text;
                logo.Address = this.txtDeviceAddress.Text;
                App.reportSettingModel.DeciceLogo.Add(logo);
                this.Close();       
            }

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void logoSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.txtDeviceNo.Text))
            {
                MessageBox.Show(this, App.Current.FindResource("Message_11").ToString());
                this.txtDeviceNo.Focus();
            }
            try
            {
                var dlg = new Microsoft.Win32.OpenFileDialog() { Filter = "png|*.png" };
                if (dlg.ShowDialog(this) == true)
                {
                    string logoPath = AppDomain.CurrentDomain.BaseDirectory + this.txtDeviceNo.Text+".png";
                    this.txtLogoPath.Text = dlg.FileName;
                    File.Copy(dlg.FileName, logoPath, true);                  
                    this.logoImg.Source = ImageTools.GetBitmapImage(logoPath);
                }
            }
            catch (Exception)
            {
                FileHelper.SetFolderPower(AppDomain.CurrentDomain.BaseDirectory, "Everyone", "FullControl");
                FileHelper.SetFolderPower(AppDomain.CurrentDomain.BaseDirectory, "Users", "FullControl");
            }
        }

        private void sealSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.txtDeviceNo.Text))
            {
                MessageBox.Show(this, App.Current.FindResource("Message_11").ToString());
                this.txtDeviceNo.Focus();
            }
            try
            {
                var dlg = new Microsoft.Win32.OpenFileDialog() { Filter = "png|*.png" };
                if (dlg.ShowDialog(this) == true)
                {
                    string sealPath = AppDomain.CurrentDomain.BaseDirectory + this.txtDeviceNo.Text + "_s.png";
                    this.txtSealPath.Text = dlg.FileName;
                    File.Copy(dlg.FileName, sealPath, true);
                    this.sealImg.Source = ImageTools.GetBitmapImage(sealPath);
                }
            }
            catch (Exception)
            {
                FileHelper.SetFolderPower(AppDomain.CurrentDomain.BaseDirectory, "Everyone", "FullControl");
                FileHelper.SetFolderPower(AppDomain.CurrentDomain.BaseDirectory, "Users", "FullControl");
            }
        }                
        
    }
}
