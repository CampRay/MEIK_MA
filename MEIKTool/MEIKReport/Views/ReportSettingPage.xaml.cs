using MEIKReport.Common;
using MEIKReport.Model;
using MEIKReport.Views;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
    public partial class ReportSettingPage : Window
    {
        private delegate void UpdateProgressBarDelegate(DependencyProperty dp, Object value);
        private delegate void ProgressBarGridDelegate(DependencyProperty dp, Object value); 
        public ReportSettingPage()
        {
            InitializeComponent();            
            this.tabSetting.DataContext = App.reportSettingModel;
            if (!App.reportSettingModel.DefaultLogo)
            {
                this.logoImg.Source = ImageTools.GetBitmapImage(AppDomain.CurrentDomain.BaseDirectory + "logo.png");                
            }
        }

        

        private void Window_Closed(object sender, EventArgs e)
        {
            this.Owner.Visibility = Visibility.Visible;
            this.IconImg.Source = null;
        }

        private void btnReportSettingSave_Click(object sender, RoutedEventArgs e)
        {                                              
            try
            {                

                if (!string.IsNullOrEmpty(txtMailPwd.Password))
                {
                    App.reportSettingModel.MailPwd = txtMailPwd.Password;
                }
                OperateIniFile.WriteIniData("Base", "MEIK base", App.reportSettingModel.MeikBase, System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
                OperateIniFile.WriteIniData("Report", "Use Default Signature", App.reportSettingModel.UseDefaultSignature.ToString(), System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
                User[] doctorUsers=App.reportSettingModel.DoctorNames.ToArray<User>();
                List<string> doctorsArr = new List<string>();
                foreach (var item in doctorUsers)
                {
                    doctorsArr.Add(item.Name + "|" + item.License);
                }
                OperateIniFile.WriteIniData("Report", "Doctor Names List", string.Join(";", doctorsArr.ToArray()), System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");

                OperateIniFile.WriteIniData("Report", "Doctor Name Required", App.reportSettingModel.ShowDoctorSignature.ToString(), System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
                OperateIniFile.WriteIniData("Report", "Use Default Logo", App.reportSettingModel.DefaultLogo.ToString(), System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
               
			    OperateIniFile.WriteIniData("Report", "Transfer Mode", App.reportSettingModel.TransferMode.ToString(), System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");                                                            

                OperateIniFile.WriteIniData("Report", "Print Paper", App.reportSettingModel.PrintPaper.ToString(), System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
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


                Logo[] deviceLogos = App.reportSettingModel.DeciceLogo.ToArray<Logo>();
                List<string> logoArr = new List<string>();
                foreach (var item in deviceLogos)
                {
                    logoArr.Add(item.Device + "||" + item.Address);
                }
                OperateIniFile.WriteIniData("Report", "Logo List", string.Join("&&", logoArr.ToArray()), System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");

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

        private void btnAddDoctor_Click(object sender, RoutedEventArgs e)
        {
            AddNamePage addNamePage = new AddNamePage(App.reportSettingModel.DoctorNames);            
            addNamePage.ShowDialog();
        }

        private void btnDelDoctor_Click(object sender, RoutedEventArgs e)
        {
            if (this.listDoctorName.SelectedIndex != -1)
            {
                App.reportSettingModel.DoctorNames.RemoveAt(this.listDoctorName.SelectedIndex);
            }
        }

        private void logoSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new Microsoft.Win32.OpenFileDialog() { Filter = "png|*.png" };
                if (dlg.ShowDialog(this) == true)
                {
                    string logoPath = AppDomain.CurrentDomain.BaseDirectory + "logo.png";
                    this.txtLogoPath.Text = logoPath;
                    File.Copy(dlg.FileName, logoPath, true);
                    App.reportSettingModel.DefaultLogo = false;
                    this.logoImg.Source = ImageTools.GetBitmapImage(logoPath);
                }
            }
            catch (Exception)
            {
                FileHelper.SetFolderPower(AppDomain.CurrentDomain.BaseDirectory, "Everyone", "FullControl");
                FileHelper.SetFolderPower(AppDomain.CurrentDomain.BaseDirectory, "Users", "FullControl");
            }
        }

        private void logoDefaultBtn_Click(object sender, RoutedEventArgs e)
        {
            this.txtLogoPath.Text = "";
            App.reportSettingModel.DefaultLogo =true;
            this.logoImg.Source = new BitmapImage(new Uri("/Images/title.png", UriKind.Relative)); 
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

        private void btnAddDeviceLogo_Click(object sender, RoutedEventArgs e)
        {
            AddLogoPage addLogoPage = new AddLogoPage();
            addLogoPage.ShowDialog();
        }

        private void btnDelDeviceLogo_Click(object sender, RoutedEventArgs e)
        {
            if (this.listDeciceNo.SelectedIndex != -1)
            {
                try
                {
                    var logo = this.listDeciceNo.SelectedItem as Logo;
                    if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + logo.Device + ".png"))
                    {
                        File.Delete(AppDomain.CurrentDomain.BaseDirectory + logo.Device + ".png");
                    }
                    if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + logo.Device + "_s.png"))
                    {
                        File.Delete(AppDomain.CurrentDomain.BaseDirectory + logo.Device + "_s.png");
                    }
                    this.IconImg.Source = null;
                    this.SealImg.Source = null;
                    App.reportSettingModel.DeciceLogo.RemoveAt(this.listDeciceNo.SelectedIndex);

                }
                catch { }
            }
        }

        private void listDeciceNo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox.SelectedIndex!=-1)
            {
                var logo=comboBox.SelectedItem as Logo;   
                //解決Image組件綁定圖片後，此圖片不能在其它地方訪問的問題
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + logo.Device + ".png"))
                {
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + logo.Device + ".png");
                    image.EndInit();
                    this.IconImg.Source = image;
                }
                else
                {
                    this.IconImg.Source = null;
                }
                //印章圖片
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + logo.Device + "_s.png"))
                {
                    BitmapImage sealImage = new BitmapImage();
                    sealImage.BeginInit();
                    sealImage.CacheOption = BitmapCacheOption.OnLoad;
                    sealImage.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + logo.Device + "_s.png");
                    sealImage.EndInit();
                    this.SealImg.Source = sealImage;
                }
                else
                {
                    this.SealImg.Source = null;
                }
            }
        }        
        
                       
    }
}
