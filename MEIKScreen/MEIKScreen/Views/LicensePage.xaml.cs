using MEIKScreen.Common;
using MEIKScreen.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    public partial class LicensePage : Window
    {
        public LicensePage()
        {
            InitializeComponent();            
            this.txtLicense.Focus();
        }                        

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.txtLicense.Text))
            {                
                MessageBox.Show(this, App.Current.FindResource("Message_71").ToString());                               
            }
            else
            {
                try
                {
                    NameValueCollection nvlist = new NameValueCollection();
                    nvlist.Add("license", this.txtLicense.Text);
                    nvlist.Add("cpuid", ComputerInfoTools.GetCPUId());
                    var res = HttpWebTools.Post(App.reportSettingModel.CloudPath + "/api/active", nvlist);
                    var jsonObj = JObject.Parse(res);
                    bool status = (bool)jsonObj["status"];
                    if (status)
                    {
                        App.reportSettingModel.License = this.txtLicense.Text;
                        OperateIniFile.WriteIniData("Base", "License", this.txtLicense.Text, System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
                        string filePath = System.AppDomain.CurrentDomain.BaseDirectory + "license.dat";
                        SecurityTools.EncryptTextToFile(App.reportSettingModel.License + ComputerInfoTools.GetCPUId(), filePath);
                        MessageBox.Show(this, App.Current.FindResource("Message_73").ToString());
                        this.DialogResult = (bool?)true;
                        this.Close();
                    }
                    else
                    {
                        string info = (string)jsonObj["info"];
                        MessageBox.Show(this, info);                        
                    }
                }
                catch (Exception exe)
                {
                    MessageBox.Show(this, App.Current.FindResource("Message_72").ToString()+exe.Message);
                }
                
            }
            this.txtLicense.Focus();

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = (bool?)false;
            this.Close();
        }                
        
    }
}
