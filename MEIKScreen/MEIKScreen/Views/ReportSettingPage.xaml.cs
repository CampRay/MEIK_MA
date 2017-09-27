using MEIKScreen.Common;
using MEIKScreen.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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
    public partial class ReportSettingPage : Window
    {
        private delegate void UpdateProgressBarDelegate(DependencyProperty dp, Object value);
        private delegate void ProgressBarGridDelegate(DependencyProperty dp, Object value); 
        public ReportSettingPage()
        {
            InitializeComponent();            
            this.tabSetting.DataContext = App.reportSettingModel;            
        }

        

        private void Window_Closed(object sender, EventArgs e)
        {
            this.Owner.Visibility = Visibility.Visible;
        }

        private void btnReportSettingSave_Click(object sender, RoutedEventArgs e)
        {            
            Regex re = new Regex("[0-9]{3}");
            if (!re.IsMatch(txtDeviceId.Text))
            {
                MessageBox.Show(this, App.Current.FindResource("Message_64").ToString());
                txtDeviceId.Focus();
                return;
            }            
            
            try
            {                

                if (!string.IsNullOrEmpty(txtMailPwd.Password))
                {
                    App.reportSettingModel.MailPwd = txtMailPwd.Password;
                }
                OperateIniFile.WriteIniData("Base", "MEIK base", App.reportSettingModel.MeikBase, System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
                OperateIniFile.WriteIniData("Report", "Use Default Signature", App.reportSettingModel.UseDefaultSignature.ToString(), System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
                
                User[] techUsers = App.reportSettingModel.TechNames.ToArray<User>();
                List<string> techArr = new List<string>();
                foreach (var item in techUsers)
                {
                    techArr.Add(item.Name + "|" + item.License);
                }
                OperateIniFile.WriteIniData("Report", "Technician Names List", string.Join(";", techArr.ToArray()), System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
                OperateIniFile.WriteIniData("Report", "Screen Venue", App.reportSettingModel.ScreenVenue.ToString(), System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");

                OperateIniFile.WriteIniData("Report", "Technician Name Required", App.reportSettingModel.ShowTechSignature.ToString(), System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");

                OperateIniFile.WriteIniData("Report", "Transfer Mode", App.reportSettingModel.TransferMode.ToString(), System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");                                            
                
                OperateIniFile.WriteIniData("Mail", "My Mail Address", App.reportSettingModel.MailAddress, System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
                OperateIniFile.WriteIniData("Mail", "To Mail Address", App.reportSettingModel.ToMailAddress, System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");

                OperateIniFile.WriteIniData("Mail", "To Mail Address List", string.Join(";", App.reportSettingModel.ToMailAddressList), System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
                OperateIniFile.WriteIniData("Mail", "Mail Subject", App.reportSettingModel.MailSubject, System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
                OperateIniFile.WriteIniData("Mail", "Mail Content", App.reportSettingModel.MailSubject, System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
                OperateIniFile.WriteIniData("Mail", "Mail Host", App.reportSettingModel.MailHost, System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
                OperateIniFile.WriteIniData("Mail", "Mail Port", App.reportSettingModel.MailPort.ToString(), System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
                OperateIniFile.WriteIniData("Mail", "Mail Username", App.reportSettingModel.MailUsername, System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
                string mailPwd = App.reportSettingModel.MailPwd;
                if (!string.IsNullOrEmpty(mailPwd))
                {
                    mailPwd = SecurityTools.EncryptText(mailPwd);
                }
                OperateIniFile.WriteIniData("Mail", "Mail Password", mailPwd, System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
                OperateIniFile.WriteIniData("Mail", "Mail SSL", App.reportSettingModel.MailSsl.ToString(), System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");

                OperateIniFile.WriteIniData("Device", "Device No", App.reportSettingModel.DeviceNo.ToString(), System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
                
                var userListWin = this.Owner as UserList;
                userListWin.labDeviceNo.Content = App.reportSettingModel.DeviceNo;
                //if (App.reportSettingModel.TransferMode == TransferMode.CloudServer)
                //{
                //    userListWin.btnReceivePdf.Visibility = Visibility.Visible;
                //}
                //else
                //{
                //    userListWin.btnReceivePdf.Visibility = Visibility.Collapsed;
                //}

                MessageBox.Show(this, App.Current.FindResource("Message_14").ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, App.Current.FindResource("Message_13").ToString() + " " + ex.Message);
            }
        }

        private void btnReportSettingClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            this.Owner.Visibility = Visibility.Visible;
        }                      

        private void btnAddTech_Click(object sender, RoutedEventArgs e)
        {
            AddNamePage addNamePage = new AddNamePage(App.reportSettingModel.TechNames);            
            addNamePage.ShowDialog();

        }

        private void btnDelTech_Click(object sender, RoutedEventArgs e)
        {
            if (this.listTechnicianName.SelectedIndex != -1)
            {
                App.reportSettingModel.TechNames.RemoveAt(this.listTechnicianName.SelectedIndex);
            }
        }

        private void btnAddEmail_Click(object sender, RoutedEventArgs e)
        {
            AddEmailPage addEmailPage = new AddEmailPage(App.reportSettingModel.ToMailAddressList);
            addEmailPage.ShowDialog();
        }

        private void btnDelEmail_Click(object sender, RoutedEventArgs e)
        {
            if (this.listToMailAddress.SelectedIndex != -1)
            {
                App.reportSettingModel.ToMailAddressList.RemoveAt(this.listToMailAddress.SelectedIndex);
            }
        }        
                     
    }
}
