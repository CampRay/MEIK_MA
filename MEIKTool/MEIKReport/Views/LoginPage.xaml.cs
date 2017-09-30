using MEIKReport.Common;
using MEIKReport.Model;
using Newtonsoft.Json;
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

namespace MEIKReport
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LoginPage : Window
    {
        public LoginPage()
        {
            InitializeComponent();
            if (string.IsNullOrEmpty(App.reportSettingModel.CloudUser))
            {
                this.txtUserName.Focus();
            }
            else
            {
                this.txtUserName.Text = App.reportSettingModel.CloudUser;
                this.txtPwd.Focus();
            }
        }                        

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.txtUserName.Text))
            {
                MessageBox.Show(this, App.Current.FindResource("Message_67").ToString());
                this.txtUserName.Focus();
            }
            else if (string.IsNullOrEmpty(this.txtPwd.Password))
            {
                MessageBox.Show(this, App.Current.FindResource("Message_68").ToString());
                this.txtPwd.Focus();
            }
            else
            {                
                NameValueCollection postDict = new NameValueCollection();
                postDict.Add("username", this.txtUserName.Text);
                postDict.Add("password", this.txtPwd.Password);
                postDict.Add("language", App.local);
                string responseStr=null;
                try
                {
                    responseStr = HttpWebTools.Post(App.reportSettingModel.CloudPath + "/api/login", postDict);
                    if (responseStr != null)
                    {
                        var jsonObj = JObject.Parse(responseStr);
                        bool status = (bool)jsonObj["status"];
                        if (status)
                        {
                            App.reportSettingModel.CloudUser = this.txtUserName.Text;
                            var dataObj = (JObject)jsonObj["data"];
                            App.reportSettingModel.CloudToken = (string)dataObj["token"];
                            OperateIniFile.WriteIniData("Cloud", "CloudUser", this.txtUserName.Text, System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
                            this.DialogResult = (bool?)true;
                            this.Close();
                        }
                        else
                        {
                            var info = (string)jsonObj["info"];
                            MessageBox.Show(this, info);
                            this.txtUserName.Focus();
                        }
                    }
                }
                catch (Exception exe)
                {
                    MessageBox.Show(this, exe.Message);
                    this.txtUserName.Focus();
                }
                
            }
                      

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = (bool?)false;
            this.Close();
        }                
        
    }
}
