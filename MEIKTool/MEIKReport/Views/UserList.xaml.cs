using MEIKReport.Common;
using MEIKReport.Model;
using MEIKReport.Views;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using System.Xml;

namespace MEIKReport
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class UserList : Window
    {
        private delegate void UpdateProgressBarDelegate(DependencyProperty dp, Object value);
        private delegate void ProgressBarGridDelegate(DependencyProperty dp, Object value);  
        private string deviceNo = OperateIniFile.ReadIniData("Device", "Device No", "000", System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
        //private string meikFolder = OperateIniFile.ReadIniData("Base", "MEIK base", "C:\\Program Files (x86)\\MEIK 5.6", System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");        
        protected MouseHook mouseHook = new MouseHook();
        private string dataFolder = AppDomain.CurrentDomain.BaseDirectory + "Data";
        private ShortFormReport shortFormReportModel = new ShortFormReport();

        private ObservableCollection<User> tempDoctorNames = new ObservableCollection<User>();                
                

        public UserList()
        {
            if (!Directory.Exists(dataFolder))
            {                
                try
                {
                    Directory.CreateDirectory(dataFolder);
                    FileHelper.SetFolderPower(dataFolder, "Everyone", "FullControl");
                    FileHelper.SetFolderPower(dataFolder, "Users", "FullControl");
                }
                catch (Exception) { }
            }
            InitializeComponent();
            languageUS.Foreground = App.local.Equals("en-US") ? new SolidColorBrush(Color.FromRgb(0x40, 0xc4, 0xe4)) : Brushes.White;
            languageHK.Foreground = App.local.Equals("zh-CN") ? new SolidColorBrush(Color.FromRgb(0x40, 0xc4, 0xe4)) : Brushes.White; 
            //listLang.SelectedIndex = App.local.Equals("en-US") ? 0 : App.local.Equals("zh-HK") ? 1 : 1;
            
            //加载初始化配置
            LoadInitConfig();
            string month = DateTime.Now.ToShortDateString();
            //修改原始MEIK程序中的档案改变日期，让原始MEIK程序运行时跨月份打开程序不会出现提示对话框
            OperateIniFile.WriteIniData("Base", "Archive change date", month, App.meikFolder + System.IO.Path.DirectorySeparatorChar + "MEIK.ini");
            ////建立当天文件夹          
            //string dayFolder = App.reportSettingModel.DataBaseFolder + System.IO.Path.DirectorySeparatorChar + DateTime.Now.ToString("MM_yyyy") + System.IO.Path.DirectorySeparatorChar + DateTime.Now.ToString("dd");
            //建立当月文件夹          
            string monthFolder = App.reportSettingModel.DataBaseFolder + System.IO.Path.DirectorySeparatorChar + DateTime.Now.ToString("yyyy_MM");
            if (!Directory.Exists(monthFolder))
            {
                Directory.CreateDirectory(monthFolder);
            }
            App.dataFolder = monthFolder;
            //加载当前档案目录数据
            if (!string.IsNullOrEmpty(App.dataFolder))
            {
                loadArchiveFolder(App.dataFolder);
            }
            //显示设备编号
            labDeviceNo.Content = App.reportSettingModel.DeviceNo;                       
            
            mouseHook.MouseUp += new System.Windows.Forms.MouseEventHandler(mouseHook_MouseUp);
           
        }                

        private void LoadInitConfig()
        {
            try
            {
                if (File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + "Config_bak.ini"))
                {
                    try
                    {
                        File.Copy(System.AppDomain.CurrentDomain.BaseDirectory + "Config_bak.ini", System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini", true);
                        File.Delete(System.AppDomain.CurrentDomain.BaseDirectory + "Config_bak.ini");
                    }
                    catch { }
                }

                if (App.reportSettingModel == null)
                {
                    App.reportSettingModel = new ReportSettingModel();
                    App.reportSettingModel.DataBaseFolder = OperateIniFile.ReadIniData("Base", "Data base", "C:\\MEIKData", System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
                    App.reportSettingModel.Version = OperateIniFile.ReadIniData("Base", "Version", "1.0.0", System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");

                    App.reportSettingModel.UseDefaultSignature = Convert.ToBoolean(OperateIniFile.ReadIniData("Report", "Use Default Signature", "false", System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini"));
                    string doctorNames = OperateIniFile.ReadIniData("Report", "Doctor Names List", "", System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
                    if (!string.IsNullOrEmpty(doctorNames))
                    {
                        var doctorList = doctorNames.Split(';').ToList<string>();
                        //doctorList.ForEach(item => App.reportSettingModel.DoctorNames.Add(item));
                        foreach (var item in doctorList)
                        {
                            User doctorUser = new User();
                            string[] arr = item.Split('|');
                            doctorUser.Name = arr[0];
                            doctorUser.License = arr[1];
                            App.reportSettingModel.DoctorNames.Add(doctorUser);
                        }
                    }

                    App.reportSettingModel.DefaultLogo =Convert.ToBoolean(OperateIniFile.ReadIniData("Report", "Use Default Logo", "true", System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini"));
                    //App.reportSettingModel.ScreenVenue = OperateIniFile.ReadIniData("Report", "Screen Venue", "", System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
                    App.reportSettingModel.ShowDoctorSignature = Convert.ToBoolean(OperateIniFile.ReadIniData("Report", "Doctor Name Required", "true", System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini"));
                    //App.reportSettingModel.NoShowTechSignature = Convert.ToBoolean(OperateIniFile.ReadIniData("Report", "Hide Technician Signature", "false", System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini"));

                    App.reportSettingModel.CloudPath = OperateIniFile.ReadIniData("Cloud", "Cloud Path", "", System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
                    App.reportSettingModel.CloudUser = OperateIniFile.ReadIniData("Cloud", "CloudUser", "", System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");

                    string transferMode = OperateIniFile.ReadIniData("Report", "Transfer Mode", "Email", System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
                    transferMode = string.IsNullOrEmpty(transferMode) ? "Email" : transferMode;
                    App.reportSettingModel.TransferMode = (TransferMode)Enum.Parse(typeof(TransferMode), transferMode, true);
					string pagesize = OperateIniFile.ReadIniData("Report", "Print Paper", "Letter", System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
                    pagesize = string.IsNullOrEmpty(pagesize) ? "Letter" : pagesize;
                    App.reportSettingModel.PrintPaper = (PageSize)Enum.Parse(typeof(PageSize), pagesize, true);
                    App.reportSettingModel.MailAddress = OperateIniFile.ReadIniData("Mail", "My Mail Address", "", System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
                    App.reportSettingModel.ToMailAddress = OperateIniFile.ReadIniData("Mail", "To Mail Address", "", System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
                    string toMialList = OperateIniFile.ReadIniData("Mail", "To Mail Address List", "", System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
                    if (!string.IsNullOrEmpty(toMialList))
                    {
                        var emailList = toMialList.Split(';').ToList<string>();
                        emailList.ForEach(item => App.reportSettingModel.ToMailAddressList.Add(item));
                    }

                    App.reportSettingModel.MailSubject = OperateIniFile.ReadIniData("Mail", "Mail Subject", "", System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
                    App.reportSettingModel.MailBody = OperateIniFile.ReadIniData("Mail", "Mail Content", "", System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
                    App.reportSettingModel.MailHost = OperateIniFile.ReadIniData("Mail", "Mail Host", "", System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
                    App.reportSettingModel.MailPort = Convert.ToInt32(OperateIniFile.ReadIniData("Mail", "Mail Port", "25", System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini"));
                    App.reportSettingModel.MailUsername = OperateIniFile.ReadIniData("Mail", "Mail Username", "", System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
                    string mailPwd = OperateIniFile.ReadIniData("Mail", "Mail Password", "", System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
                    if (!string.IsNullOrEmpty(mailPwd))
                    {
                        App.reportSettingModel.MailPwd = SecurityTools.DecryptText(mailPwd);
                    }
                    App.reportSettingModel.MailSsl = Convert.ToBoolean(OperateIniFile.ReadIniData("Mail", "Mail SSL", "false", System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini"));
                    //App.reportSettingModel.DeviceNo = OperateIniFile.ReadIniData("Device", "Device No", "000", System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");                    
                    //App.reportSettingModel.RecordDate = OperateIniFile.ReadIniData("Data", "Record Date", "", System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");

                    foreach (var item in App.reportSettingModel.DoctorNames)
                    {
                        tempDoctorNames.Add(item);
                    }

                    string logoStr = OperateIniFile.ReadIniData("Report", "Logo List", "", System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
                    if (!string.IsNullOrEmpty(logoStr))
                    {
                        string[] sp=new string[]{"&&"};
                        string[] sp1 = new string[] { "||" };
                        var logoList = logoStr.Split(sp,StringSplitOptions.RemoveEmptyEntries).ToList<string>();
                        foreach (var item in logoList)
                        {
                            Logo deviceLogo = new Logo();
                            string[] arr = item.Split(sp1, StringSplitOptions.RemoveEmptyEntries);
                            deviceLogo.Device = arr[0];
                            deviceLogo.Address = arr[1];
                            App.reportSettingModel.DeciceLogo.Add(deviceLogo);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, App.Current.FindResource("Message_20").ToString() + " " + ex.Message);
            }
        }
        
        private void Window_Closed(object sender, EventArgs e)
        {                                    
            //this.Owner.Visibility = Visibility.Visible;                
            var owner = this.Owner as MainWindow;
            owner.exitMeik();            
        }
		
		private void Window_Loaded(object sender, RoutedEventArgs e)
        {            
            updateVersion();            
        }

        private void exitReport_Click(object sender, RoutedEventArgs e)
        {
            this.Close();            
        }
        /// <summary>
        /// 立即进行版本更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void updateVersion()
        {            
            try
            {
                string resStr = HttpWebTools.Get(App.reportSettingModel.CloudPath + "/api/getMEIKToolVersion", App.reportSettingModel.CloudToken);
                var jsonObj = JObject.Parse(resStr);
                bool status = (bool)jsonObj["status"];
                if (status)
                {
                    var filename = (string)jsonObj["data"];
                    string latestVersion = "";
                    if (!string.IsNullOrEmpty(filename))
                    {
                        latestVersion = filename.Replace("MEIKToolSetup.", "").Replace(".exe", "");
                    }
                    //判斷是否有新版本
                    if (string.Compare(latestVersion, App.reportSettingModel.Version, StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        MessageBoxResult result = MessageBox.Show(this, App.Current.FindResource("Message_46").ToString(), "Update Now", MessageBoxButton.YesNo, MessageBoxImage.Information);
                        if (result == MessageBoxResult.Yes)
                        {
                            //定义委托代理
                            ProgressBarGridDelegate progressBarGridDelegate = new ProgressBarGridDelegate(progressBarGrid.SetValue);
                            UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(uploadProgressBar.SetValue);
                            //使用系统代理方式显示进度条面板
                            Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { System.Windows.Controls.ProgressBar.ValueProperty, Convert.ToDouble(5) });
                            Dispatcher.Invoke(progressBarGridDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { System.Windows.Controls.Grid.VisibilityProperty, Visibility.Visible });

                            try
                            {
                                HttpWebRequest request = WebRequest.Create(App.reportSettingModel.CloudPath + "/api/downloadMEIKTool") as HttpWebRequest;
                                request.Method = "GET";
                                request.Timeout = 120000;
                                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                                if (response.StatusCode != HttpStatusCode.Created && response.StatusCode != HttpStatusCode.OK)
                                {
                                    MessageBox.Show(this, App.Current.FindResource("Message_40").ToString());
                                }
                                else
                                {                                    
                                    Stream responseStream = response.GetResponseStream();
                                    string downloadFilePath = App.reportSettingModel.DataBaseFolder + System.IO.Path.DirectorySeparatorChar + filename;
                                    int tsize = Convert.ToInt32(response.GetResponseHeader("Content-Length")) ;
                                    //创建本地文件写入流
                                    Stream stream = new FileStream(downloadFilePath, FileMode.Create);
                                    double moveSize = 95d / (tsize / 1024);
                                    int x = 1;
                                    byte[] bArr = new byte[1024];
                                    int size = responseStream.Read(bArr, 0, bArr.Length);
                                    while (size > 0)
                                    {
                                        stream.Write(bArr, 0, size);
                                        size = responseStream.Read(bArr, 0, bArr.Length);
                                        Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { System.Windows.Controls.ProgressBar.ValueProperty, Convert.ToDouble(moveSize * x) });
                                        x++;
                                    }
                                    stream.Close();
                                    responseStream.Close();

                                    Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { System.Windows.Controls.ProgressBar.ValueProperty, Convert.ToDouble(100) });
                                    Dispatcher.Invoke(progressBarGridDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { System.Windows.Controls.Grid.VisibilityProperty, Visibility.Collapsed });
                                    MessageBox.Show(this, App.Current.FindResource("Message_48").ToString());

                                    try
                                    {
                                        //启动外部程序
                                        Process setupProc = Process.Start(downloadFilePath);
                                        if (setupProc != null)
                                        {
                                            //proc.WaitForExit();//等待外部程序退出后才能往下执行
                                            setupProc.WaitForInputIdle();
                                            File.Copy(System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini", System.AppDomain.CurrentDomain.BaseDirectory + "Config_bak.ini", true);
                                            OperateIniFile.WriteIniData("Base", "Version", latestVersion, System.AppDomain.CurrentDomain.BaseDirectory + "Config_bak.ini");
                                            //关闭当前程序
                                            App.Current.Shutdown();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show(App.Current.FindResource("Message_51").ToString() + " " + ex.Message);
                                    }
                                }                                
                                
                            }
                            catch (Exception e2)
                            {
                                MessageBox.Show(this, App.Current.FindResource("Message_40").ToString()+ e2.Message);                                
                            }
                            finally
                            {
                                Dispatcher.Invoke(progressBarGridDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { System.Windows.Controls.Grid.VisibilityProperty, Visibility.Collapsed });
                            }
                            
                        }
                    }

                }
            }
            catch {}                        
        }

        
        private void btnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            //System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            //folderBrowserDialog.SelectedPath = txtFolderPath.Text;
            //System.Windows.Forms.DialogResult res = folderBrowserDialog.ShowDialog();
            //if (res == System.Windows.Forms.DialogResult.OK)
            //{
            //    string folderName = folderBrowserDialog.SelectedPath;
            //    loadArchiveFolder(folderName);
            //}   
            OpenFolderPage ofp = new OpenFolderPage();
            ofp.SelectedPath = txtFolderPath.Text;
            ofp.Owner = this;
            ofp.ShowDialog();
        }


        public void loadArchiveFolder(string folderName)
        {
            try
            {
                //设定系统当前选择的档案文件夹
                App.dataFolder = folderName;          
                txtFolderPath.Text = folderName;
                CollectionViewSource customerSource = (CollectionViewSource)this.FindResource("CustomerSource");
                HashSet<Person> set = new HashSet<Person>();
                //遍历指定文件夹下所有文件
                HandleFolder(folderName,ref set);

                customerSource.Source = set.OrderByDescending(x=>x.Code);
                if (set.Count > 0)
                {
                    reportButtonPanel.Visibility = Visibility.Visible;                    
                }
                else
                {
                    reportButtonPanel.Visibility = Visibility.Hidden;                    
                }

                string meikiniFile = App.meikFolder + System.IO.Path.DirectorySeparatorChar + "MEIK.ini";
                var selectItem = this.CodeListBox.SelectedItem as Person;
                if (selectItem != null)
                {                                        
                    //修改原始MEIK程序中的患者档案目录，让原始MEIK程序运行后直接打开此患者档案
                    OperateIniFile.WriteIniData("Base", "Patients base", selectItem.ArchiveFolder, meikiniFile);
                }
                else
                {
                    //修改原始MEIK程序中的患者档案目录，让原始MEIK程序运行后直接打开此患者档案
                    OperateIniFile.WriteIniData("Base", "Patients base", folderName, meikiniFile);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private void HandleFolder(string folderName,ref HashSet<Person> set)
        {
            //遍历指定文件夹下所有文件
            DirectoryInfo theFolder = new DirectoryInfo(folderName);             
            try
            {
                FileInfo[] fileInfo = theFolder.GetFiles();
                //遍历文件
                foreach (FileInfo NextFile in fileInfo)
                {                    
                    if (".ini".Equals(NextFile.Extension, StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            #region 加載Person
                            Person person = new Person();
                            person.Uploaded = Visibility.Collapsed.ToString();
                            //person.ArchiveFolder = folderName;
                            person.ArchiveFolder = theFolder.FullName;                            
                            person.IniFilePath = NextFile.FullName;
                            person.CrdFilePath = person.IniFilePath.Replace(".ini",".crd");
                            person.Code = NextFile.Name.Substring(0, NextFile.Name.Length - 4);
                            person.Status = OperateIniFile.ReadIniData("Report", "Status", "", NextFile.FullName);
                            if ("DR".Equals(person.Status,StringComparison.OrdinalIgnoreCase))
                            {                                
                                person.StatusText = App.Current.FindResource("CommonStatusReceived").ToString();
                            }
                            else if ("RD".Equals(person.Status,StringComparison.OrdinalIgnoreCase))
                            {
                                person.StatusText = App.Current.FindResource("CommonStatusReportDone").ToString();                                
                            }
                            else if ("RS".Equals(person.Status, StringComparison.OrdinalIgnoreCase))
                            {
                                person.StatusText = App.Current.FindResource("CommonStatusReportSent").ToString();                                
                            }
                            else
                            {
                                person.Status = "DR";
                                person.StatusText = App.Current.FindResource("CommonStatusReceived").ToString();
                            }

                            string personalData = "Personal data";
                            string complaints = "Complaints";
                            string menses = "Menses";
                            string somatic = "Somatic";
                            string gynecologic = "Gynecologic";
                            string obstetric = "Obstetric";
                            string lactation = "Lactation";
                            string diseases = "Diseases";
                            string palpation = "Palpation";
                            string ultrasound = "Ultrasound";
                            string mammography = "Mammography";
                            string biopsy = "Biopsy";
                            //string histology = "Histology";                            

                            //Personal Data
                            person.ClientNumber = OperateIniFile.ReadIniData(personalData, "clientnumber", "", NextFile.FullName);
                            person.SurName = OperateIniFile.ReadIniData(personalData, "surname", "", NextFile.FullName);
                            person.GivenName = OperateIniFile.ReadIniData(personalData, "given name", "", NextFile.FullName);
                            person.OtherName = OperateIniFile.ReadIniData(personalData, "other name", "", NextFile.FullName);
                            person.FullName = person.SurName;
                            if (!string.IsNullOrEmpty(person.GivenName))
                            {
                                person.FullName = person.FullName + "," + person.GivenName;
                            }
                            if (!string.IsNullOrEmpty(person.OtherName))
                            {
                                person.FullName = person.FullName + " " + person.OtherName;
                            }
                            person.Address = OperateIniFile.ReadIniData(personalData, "address", "", NextFile.FullName);
                            person.Address2 = OperateIniFile.ReadIniData(personalData, "address2", "", NextFile.FullName);
                            person.City = OperateIniFile.ReadIniData(personalData, "city", "", NextFile.FullName);
                            person.Province = OperateIniFile.ReadIniData(personalData, "province", "", NextFile.FullName);
                            person.ZipCode = OperateIniFile.ReadIniData(personalData, "zip code", "", NextFile.FullName);
                            person.Country = OperateIniFile.ReadIniData(personalData, "country", "", person.IniFilePath);

                            person.Height = OperateIniFile.ReadIniData(personalData, "height", "", NextFile.FullName);
                            person.Weight = OperateIniFile.ReadIniData(personalData, "weight", "", NextFile.FullName);
                            person.Mobile = OperateIniFile.ReadIniData(personalData, "mobile", "", NextFile.FullName);
                            person.Email = OperateIniFile.ReadIniData(personalData, "email", "", NextFile.FullName);
                            person.ReportLanguage = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(personalData, "is english", "1", NextFile.FullName)));
                            person.Free = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(personalData, "is free", "0", NextFile.FullName)));
                            
                            person.BirthDate = OperateIniFile.ReadIniData(personalData, "birth date", "", NextFile.FullName);
                            person.BirthMonth = OperateIniFile.ReadIniData(personalData, "birth month", "", NextFile.FullName);
                            person.BirthYear = OperateIniFile.ReadIniData(personalData, "birth year", "", NextFile.FullName);
                            person.RegDate = OperateIniFile.ReadIniData(personalData, "registration date", "", NextFile.FullName);
                            person.RegMonth = OperateIniFile.ReadIniData(personalData, "registration month", "", NextFile.FullName);
                            person.RegYear = OperateIniFile.ReadIniData(personalData, "registration year", "", NextFile.FullName);
                            person.Remark = OperateIniFile.ReadIniData(personalData, "remark", "", NextFile.FullName);
                            person.Remark = person.Remark.Replace(";;", "\r\n");

                            person.ScreenVenue = OperateIniFile.ReadIniData("Report", "Screen Venue", "", NextFile.FullName);
                            person.TechName = OperateIniFile.ReadIniData("Report", "Technician Name", "", NextFile.FullName);
                            person.TechLicense = OperateIniFile.ReadIniData("Report", "Technician License", "", NextFile.FullName);
                            person.ShowTechSignature = Convert.ToBoolean(OperateIniFile.ReadIniData("Report", "Technician Name Required", "true", NextFile.FullName));
                            
                            try
                            {
                                if (!string.IsNullOrEmpty(person.BirthYear))
                                {                                    
                                    int m_Y1 = Convert.ToInt32(person.BirthYear);
                                    int m_Y2 = DateTime.Now.Year;
                                    person.Age = m_Y2 - m_Y1;
                                }
                                
                            }
                            catch(Exception){ }

                            try
                            {
                                //Family History
                                person.FamilyBreastCancer1 = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData("Family History", "FamilyBreastCancer1", "0", NextFile.FullName)));
                                person.FamilyBreastCancer2 = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData("Family History", "FamilyBreastCancer2", "0", NextFile.FullName)));
                                person.FamilyBreastCancer3 = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData("Family History", "FamilyBreastCancer3", "0", NextFile.FullName)));
                                person.BreastCancerDesc = OperateIniFile.ReadIniData("Family History", "Family Breast Cancer Description", "", NextFile.FullName);
                                person.BreastCancerDesc = person.BreastCancerDesc.Replace(";;", "\r\n"); 
                                person.FamilyUterineCancer1 = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData("Family History", "FamilyUterineCancer1", "0", NextFile.FullName)));
                                person.FamilyUterineCancer2 = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData("Family History", "FamilyUterineCancer2", "0", NextFile.FullName)));
                                person.FamilyUterineCancer3 = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData("Family History", "FamilyUterineCancer3", "0", NextFile.FullName)));
                                person.UterineCancerDesc = OperateIniFile.ReadIniData("Family History", "Family Uterine Cancer Description", "", NextFile.FullName);
                                person.UterineCancerDesc = person.UterineCancerDesc.Replace(";;", "\r\n"); 
                                person.FamilyCervicalCancer1 = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData("Family History", "FamilyCervicalCancer1", "0", NextFile.FullName)));
                                person.FamilyCervicalCancer2 = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData("Family History", "FamilyCervicalCancer2", "0", NextFile.FullName)));
                                person.FamilyCervicalCancer3 = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData("Family History", "FamilyCervicalCancer3", "0", NextFile.FullName)));
                                person.CervicalCancerDesc = OperateIniFile.ReadIniData("Family History", "Family Cervical Cancer Description", "", NextFile.FullName);
                                person.CervicalCancerDesc = person.CervicalCancerDesc.Replace(";;", "\r\n"); 
                                person.FamilyOvarianCancer1 = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData("Family History", "FamilyOvarianCancer1", "0", NextFile.FullName)));
                                person.FamilyOvarianCancer2 = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData("Family History", "FamilyOvarianCancer2", "0", NextFile.FullName)));
                                person.FamilyOvarianCancer3 = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData("Family History", "FamilyOvarianCancer3", "0", NextFile.FullName)));
                                person.OvarianCancerDesc = OperateIniFile.ReadIniData("Family History", "Family Ovarian Cancer Description", "", NextFile.FullName);
                                person.OvarianCancerDesc = person.OvarianCancerDesc.Replace(";;", "\r\n");
                                person.FamilyCancerDesc = OperateIniFile.ReadIniData("Family History", "Family Cancer Description", "", NextFile.FullName);
                                person.FamilyCancerDesc = person.FamilyCancerDesc.Replace(";;", "\r\n"); 
                            }
                            catch { }


                            try {
                                //Complaints
                                person.PalpableLumps = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(complaints, "palpable lumps", "0", NextFile.FullName)));
                                if (person.PalpableLumps)
                                {
                                    person.LeftPosition = Convert.ToInt32(OperateIniFile.ReadIniData(complaints, "left position", "0", NextFile.FullName));
                                    person.RightPosition = Convert.ToInt32(OperateIniFile.ReadIniData(complaints, "right position", "0", NextFile.FullName));
                                }
                                                                                                
                                person.Pain = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(complaints, "pain", "0", NextFile.FullName)));
                                if(person.Pain){
                                    person.Degree = Convert.ToInt32(OperateIniFile.ReadIniData(complaints, "degree", "0", NextFile.FullName));
                                }

                                person.Colostrum = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(complaints, "colostrum", "0", NextFile.FullName)));
                                person.SerousDischarge = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(complaints, "serous discharge", "0", NextFile.FullName)));
                                person.BloodDischarge = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(complaints, "blood discharge", "0", NextFile.FullName)));
                                person.Other = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(complaints, "other", "0", NextFile.FullName)));
                                person.Pregnancy = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(complaints, "pregnancy", "0", NextFile.FullName)));
                                person.Lactation = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(complaints, "lactation", "0", NextFile.FullName)));
                                person.OtherDesc = OperateIniFile.ReadIniData(complaints, "other description", "", NextFile.FullName);
                                person.PregnancyTerm = OperateIniFile.ReadIniData(complaints, "pregnancy term", "", NextFile.FullName);
                                person.OtherDesc = person.OtherDesc.Replace(";;", "\r\n");                                
                                person.LactationTerm = OperateIniFile.ReadIniData(complaints, "lactation term", "", NextFile.FullName);

                                person.BreastImplants = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(complaints, "BreastImplants", "0", NextFile.FullName)));
                                person.BreastImplantsLeft = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(complaints, "BreastImplantsLeft", "0", NextFile.FullName)));
                                person.BreastImplantsRight = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(complaints, "BreastImplantsRight", "0", NextFile.FullName)));
                                person.MaterialsGel = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(complaints, "MaterialsGel", "0", NextFile.FullName)));
                                person.MaterialsFat = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(complaints, "MaterialsFat", "0", NextFile.FullName)));
                                person.MaterialsOthers = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(complaints, "MaterialsOthers", "0", NextFile.FullName)));

                                person.BreastImplantsLeftYear = OperateIniFile.ReadIniData(complaints, "BreastImplantsLeftYear", "", NextFile.FullName);
                                person.BreastImplantsRightYear = OperateIniFile.ReadIniData(complaints, "BreastImplantsRightYear", "", NextFile.FullName);
                                
                            }
                            catch (Exception) { }

                            try {
                                person.DateLastMenstruation = OperateIniFile.ReadIniData(menses, "DateLastMenstruation", "", NextFile.FullName);                                
                                person.MenstrualCycleDisorder = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(menses, "menstrual cycle disorder", "0", NextFile.FullName)));
                                person.Postmenopause = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(menses, "postmenopause", "0", NextFile.FullName)));
                                person.HormonalContraceptives = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(menses, "hormonal contraceptives", "0", NextFile.FullName)));
                                person.PostmenopauseYear = OperateIniFile.ReadIniData(menses, "postmenopause year", "", NextFile.FullName);
                                person.MenstrualCycleDisorderDesc = OperateIniFile.ReadIniData(menses, "menstrual cycle disorder description", "", NextFile.FullName);
                                person.MenstrualCycleDisorderDesc = person.MenstrualCycleDisorderDesc.Replace(";;", "\r\n");
                                person.PostmenopauseDesc = OperateIniFile.ReadIniData(menses, "postmenopause description", "", NextFile.FullName);
                                person.PostmenopauseDesc = person.PostmenopauseDesc.Replace(";;", "\r\n");
                                person.HormonalContraceptivesBrandName = OperateIniFile.ReadIniData(menses, "hormonal contraceptives brand name", "", NextFile.FullName);
                                person.HormonalContraceptivesBrandName = person.HormonalContraceptivesBrandName.Replace(";;", "\r\n");
                                person.HormonalContraceptivesPeriod = OperateIniFile.ReadIniData(menses, "hormonal contraceptives period", "", NextFile.FullName);
                                person.HormonalContraceptivesPeriod = person.HormonalContraceptivesPeriod.Replace(";;", "\r\n");
                                person.MensesStatus = person.MenstrualCycleDisorder || person.Postmenopause || person.HormonalContraceptives ? true : false;
                            }
                            catch (Exception) { }

                            try { 
                                person.Adiposity = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(somatic, "adiposity", "0", NextFile.FullName)));
                                person.EssentialHypertension = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(somatic, "essential hypertension", "0", NextFile.FullName)));
                                person.Diabetes = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(somatic, "diabetes", "0", NextFile.FullName)));
                                person.ThyroidGlandDiseases = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(somatic, "thyroid gland diseases", "0", NextFile.FullName)));
                                person.SomaticOther = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(somatic, "other", "0", NextFile.FullName)));
                                person.EssentialHypertensionDesc = OperateIniFile.ReadIniData(somatic, "essential hypertension description", "", NextFile.FullName);
                                person.EssentialHypertensionDesc = person.EssentialHypertensionDesc.Replace(";;", "\r\n");
                                person.DiabetesDesc = OperateIniFile.ReadIniData(somatic, "diabetes description", "", NextFile.FullName);
                                person.DiabetesDesc = person.DiabetesDesc.Replace(";;", "\r\n");
                                person.ThyroidGlandDiseasesDesc = OperateIniFile.ReadIniData(somatic, "thyroid gland diseases description", "", NextFile.FullName);
                                person.ThyroidGlandDiseasesDesc = person.ThyroidGlandDiseasesDesc.Replace(";;", "\r\n");
                                person.SomaticOtherDesc = OperateIniFile.ReadIniData(somatic, "other description", "", NextFile.FullName);
                                person.SomaticOtherDesc = person.SomaticOtherDesc.Replace(";;", "\r\n");
                                person.SomaticStatus = person.Adiposity || person.EssentialHypertension || person.Diabetes || person.ThyroidGlandDiseases || person.SomaticOther ? true : false;
                            }
                            catch (Exception) { }

                            try
                            {
                                person.Infertility = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(gynecologic, "infertility", "0", NextFile.FullName)));
                                person.OvaryDiseases = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(gynecologic, "ovary diseases", "0", NextFile.FullName)));
                                person.OvaryCyst = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(gynecologic, "ovary cyst", "0", NextFile.FullName)));
                                person.OvaryCancer = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(gynecologic, "ovary cancer", "0", NextFile.FullName)));
                                person.OvaryEndometriosis = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(gynecologic, "ovary endometriosis", "0", NextFile.FullName)));
                                person.OvaryOther = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(gynecologic, "ovary other", "0", NextFile.FullName)));
                                person.UterusDiseases = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(gynecologic, "uterus diseases", "0", NextFile.FullName)));
                                person.UterusMyoma = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(gynecologic, "uterus myoma", "0", NextFile.FullName)));
                                person.UterusCancer = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(gynecologic, "uterus cancer", "0", NextFile.FullName)));
                                person.UterusEndometriosis = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(gynecologic, "uterus endometriosis", "0", NextFile.FullName)));
                                person.UterusOther = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(gynecologic, "uterus other", "0", NextFile.FullName)));
                                person.GynecologicOther = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(gynecologic, "other", "0", NextFile.FullName)));
                                person.InfertilityDesc = OperateIniFile.ReadIniData(gynecologic, "infertility-description", "", NextFile.FullName);
                                person.InfertilityDesc = person.InfertilityDesc.Replace(";;", "\r\n");
                                person.OvaryOtherDesc = OperateIniFile.ReadIniData(gynecologic, "ovary other description", "", NextFile.FullName);
                                person.OvaryOtherDesc = person.OvaryOtherDesc.Replace(";;", "\r\n");
                                person.UterusOtherDesc = OperateIniFile.ReadIniData(gynecologic, "uterus other description", "", NextFile.FullName);
                                person.UterusOtherDesc = person.UterusOtherDesc.Replace(";;", "\r\n");
                                person.GynecologicOtherDesc = OperateIniFile.ReadIniData(gynecologic, "other description", "", NextFile.FullName);
                                person.GynecologicOtherDesc = person.GynecologicOtherDesc.Replace(";;", "\r\n");
                                person.GynecologicStatus = person.Infertility || person.OvaryDiseases || person.OvaryCyst || person.OvaryCancer 
                                    || person.OvaryEndometriosis || person.OvaryOther || person.UterusDiseases || person.UterusMyoma
                                    || person.UterusCancer || person.UterusEndometriosis || person.UterusOther || person.GynecologicOther ? true : false;
                            }
                            catch (Exception) { }

                            try
                            {
                                person.Abortions = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(obstetric, "abortions", "0", NextFile.FullName)));
                                person.Births = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(obstetric, "births", "0", NextFile.FullName)));
                                person.AbortionsNumber = OperateIniFile.ReadIniData(obstetric, "abortions number", "", NextFile.FullName);
                                person.BirthsNumber = OperateIniFile.ReadIniData(obstetric, "births number", "", NextFile.FullName);
                                person.ObstetricStatus = person.Abortions || person.Births ? true : false;
                                person.ObstetricDesc = OperateIniFile.ReadIniData(obstetric, "obstetric description", "", NextFile.FullName);
                                person.ObstetricDesc = person.ObstetricDesc.Replace(";;", "\r\n");
                        
                            }
                            catch(Exception){ }

                            try { 
                                person.LactationTill1Month = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(lactation, "lactation till 1 month", "0", NextFile.FullName)));
                                person.LactationTill1Year = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(lactation, "lactation till 1 year", "0", NextFile.FullName)));
                                person.LactationOver1Year = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(lactation, "lactation over 1 year", "0", NextFile.FullName)));
                            }
                            catch (Exception) { }

                            try
                            {
                                person.Trauma = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(diseases, "trauma", "0", NextFile.FullName)));
                                person.Mastitis = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(diseases, "mastitis", "0", NextFile.FullName)));
                                person.FibrousCysticMastopathy = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(diseases, "fibrous- cystic mastopathy", "0", NextFile.FullName)));
                                person.Cysts = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(diseases, "cysts", "0", NextFile.FullName)));
                                person.Cancer = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(diseases, "cancer", "0", NextFile.FullName)));
                                person.DiseasesOther = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(diseases, "other", "0", NextFile.FullName)));
                                person.TraumaDesc = OperateIniFile.ReadIniData(diseases, "trauma description", "", NextFile.FullName);
                                person.TraumaDesc = person.TraumaDesc.Replace(";;", "\r\n");
                                person.MastitisDesc = OperateIniFile.ReadIniData(diseases, "mastitis description", "", NextFile.FullName);
                                person.MastitisDesc = person.MastitisDesc.Replace(";;", "\r\n");
                                person.FibrousCysticMastopathyDesc = OperateIniFile.ReadIniData(diseases, "fibrous- cystic mastopathy description", "", NextFile.FullName);
                                person.FibrousCysticMastopathyDesc = person.FibrousCysticMastopathyDesc.Replace(";;", "\r\n");
                                person.CystsDesc = OperateIniFile.ReadIniData(diseases, "cysts descriptuin", "", NextFile.FullName);
                                person.CystsDesc = person.CystsDesc.Replace(";;", "\r\n");
                                person.CancerDesc = OperateIniFile.ReadIniData(diseases, "cancer description", "", NextFile.FullName);
                                person.CancerDesc = person.CancerDesc.Replace(";;", "\r\n");
                                person.DiseasesOtherDesc = OperateIniFile.ReadIniData(diseases, "other description", "", NextFile.FullName);
                                person.DiseasesOtherDesc = person.DiseasesOtherDesc.Replace(";;", "\r\n");
                                person.DiseasesStatus = person.Trauma || person.Mastitis || person.FibrousCysticMastopathy || person.Cysts || person.Cancer || person.DiseasesOther ? true : false;
                            }
                            catch (Exception) { }

                            try {

                                person.Palpation = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(palpation, "palpation", "0", NextFile.FullName)));
                                person.PalationYear = OperateIniFile.ReadIniData(palpation, "palpation year", "", NextFile.FullName);                                

                                person.PalpationDiffuse = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(palpation, "diffuse", "0", NextFile.FullName)));
                                person.PalpationFocal = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(palpation, "focal", "0", NextFile.FullName)));
                                person.PalpationDesc = OperateIniFile.ReadIniData(palpation, "description", "", NextFile.FullName);
                                person.PalpationDesc = person.PalpationDesc.Replace(";;", "\r\n");
                                person.PalpationNormal = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(palpation, "palpation normal", "0", NextFile.FullName)));
                                person.PalpationStatus = person.PalpationDiffuse || person.PalpationFocal ? true : false;
                                //person.PalpationStatus = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(palpation, "palpation status", "0", NextFile.FullName)));
                            }
                            catch (Exception) { }

                            try
                            {
                                person.Ultrasound = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(ultrasound, "ultrasound", "0", NextFile.FullName)));
                                person.UltrasoundYear = OperateIniFile.ReadIniData(ultrasound, "ultrasound year", "", NextFile.FullName);                                
                                person.UltrasoundDiffuse = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(ultrasound, "diffuse", "0", NextFile.FullName)));
                                person.UltrasoundFocal = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(ultrasound, "focal", "0", NextFile.FullName)));
                                person.UltrasoundnDesc = OperateIniFile.ReadIniData(ultrasound, "description", "", NextFile.FullName);
                                person.UltrasoundnDesc = person.UltrasoundnDesc.Replace(";;", "\r\n");
                                person.UltrasoundNormal = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(ultrasound, "ultrasound normal", "0", NextFile.FullName)));
                                person.UltrasoundStatus = person.UltrasoundDiffuse || person.UltrasoundFocal ? true : false;
                                //person.UltrasoundStatus = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(ultrasound, "ultrasound status", "0", NextFile.FullName)));
                            }
                            catch (Exception) { }

                            try
                            {
                                person.Mammography = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(mammography, "mammography", "0", NextFile.FullName)));
                                person.MammographyYear = OperateIniFile.ReadIniData(mammography, "mammography year", "", NextFile.FullName);  
                                person.MammographyDiffuse = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(mammography, "diffuse", "0", NextFile.FullName)));
                                person.MammographyFocal = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(mammography, "focal", "0", NextFile.FullName)));
                                person.MammographyDesc = OperateIniFile.ReadIniData(mammography, "description", "", NextFile.FullName);
                                person.MammographyDesc = person.MammographyDesc.Replace(";;", "\r\n");
                                person.MammographyStatus = person.MammographyDiffuse || person.MammographyFocal ? true : false;
                                person.MammographyNormal = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(mammography, "mammography normal", "0", NextFile.FullName)));
                                //person.MammographyStatus = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(mammography, "mammography status", "0", NextFile.FullName)));
                            }
                            catch (Exception) { }

                            try
                            {
                                person.Biopsy = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(biopsy, "biopsy", "0", NextFile.FullName)));
                                person.BiopsyYear = OperateIniFile.ReadIniData(biopsy, "biopsy year", "", NextFile.FullName); 
                                person.BiopsyDiffuse = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(biopsy, "diffuse", "0", NextFile.FullName)));
                                person.BiopsyFocal = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(biopsy, "focal", "0", NextFile.FullName)));
                                person.BiopsyCancer = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(biopsy, "cancer", "0", NextFile.FullName)));
                                person.BiopsyProliferation = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(biopsy, "proliferation", "0", NextFile.FullName)));
                                person.BiopsyDysplasia = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(biopsy, "dysplasia", "0", NextFile.FullName)));
                                person.BiopsyIntraductalPapilloma = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(biopsy, "intraductal papilloma", "0", NextFile.FullName)));
                                person.BiopsyFibroadenoma = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(biopsy, "fibroadenoma", "0", NextFile.FullName)));
                                person.BiopsyOther = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(biopsy, "other", "0", NextFile.FullName)));
                                person.BiopsyOtherDesc = OperateIniFile.ReadIniData(biopsy, "other description", "", NextFile.FullName);
                                person.BiopsyOtherDesc = person.BiopsyOtherDesc.Replace(";;", "\r\n");
                                person.BiopsyNormal = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(biopsy, "biopsy normal", "0", NextFile.FullName)));
                                person.BiopsyStatus = person.BiopsyDiffuse || person.BiopsyFocal || person.BiopsyCancer || person.BiopsyProliferation
                                    || person.BiopsyDysplasia || person.BiopsyIntraductalPapilloma || person.BiopsyFibroadenoma || person.BiopsyOther ? true : false;
                                //person.BiopsyStatus = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(biopsy, "biopsy status", "0", NextFile.FullName)));                                                                

                                person.MeikScreening = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData(biopsy, "meik screen", "0", NextFile.FullName)));
                                person.MeikScreenYear = OperateIniFile.ReadIniData(biopsy, "meik screen year", "", NextFile.FullName);
                                person.MeikPoint = OperateIniFile.ReadIniData(biopsy, "meik screen point", "", NextFile.FullName); 
                                person.MeikScreenDesc = OperateIniFile.ReadIniData(biopsy, "meik screen description", "", NextFile.FullName);
                                person.MeikScreenDesc = person.MeikScreenDesc.Replace(";;", "\r\n");

                                person.ExaminationsOtherDesc = OperateIniFile.ReadIniData(biopsy, "examinations description", "", NextFile.FullName);
                                person.ExaminationsOtherDesc = person.ExaminationsOtherDesc.Replace(";;", "\r\n");
                            }
                            catch (Exception) { }

                            try
                            {
                                person.RedSwollen = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData("Visual", "red swollen", "0", NextFile.FullName)));
                                person.Palpable = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData("Visual", "palpable", "0", NextFile.FullName)));
                                person.Serous = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData("Visual", "serous discharge", "0", NextFile.FullName)));
                                person.Wounds = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData("Visual", "wounds", "0", NextFile.FullName)));
                                person.Scars = Convert.ToBoolean(Convert.ToInt32(OperateIniFile.ReadIniData("Visual", "scars", "0", NextFile.FullName)));
                                if (person.RedSwollen)
                                {
                                    person.RedSwollenLeft = Convert.ToInt32(OperateIniFile.ReadIniData("Visual", "red swollen left segment", "0", NextFile.FullName));
                                    person.RedSwollenRight = Convert.ToInt32(OperateIniFile.ReadIniData("Visual", "red swollen right segment", "0", NextFile.FullName));
                                    person.RedSwollenDesc = OperateIniFile.ReadIniData("Visual", "RedSwollen description", "", NextFile.FullName);
                                    person.RedSwollenDesc = person.RedSwollenDesc.Replace(";;", "\r\n");
                                }
                                if (person.Palpable)
                                {
                                    person.PalpableLeft = Convert.ToInt32(OperateIniFile.ReadIniData("Visual", "palpable left segment", "0", NextFile.FullName));
                                    person.PalpableRight = Convert.ToInt32(OperateIniFile.ReadIniData("Visual", "palpable right segment", "0", NextFile.FullName));
                                    person.PalpableDesc = OperateIniFile.ReadIniData("Visual", "Palpable description", "", NextFile.FullName);
                                    person.PalpableDesc = person.PalpableDesc.Replace(";;", "\r\n");
                                }
                                if (person.Serous)
                                {
                                    person.SerousLeft = Convert.ToInt32(OperateIniFile.ReadIniData("Visual", "serous left segment", "0", NextFile.FullName));
                                    person.SerousRight = Convert.ToInt32(OperateIniFile.ReadIniData("Visual", "serous right segment", "0", NextFile.FullName));
                                    person.SeriousDesc = OperateIniFile.ReadIniData("Visual", "Serious description", "", NextFile.FullName);
                                    person.SeriousDesc = person.SeriousDesc.Replace(";;", "\r\n");
                                }
                                if (person.Wounds)
                                {
                                    person.WoundsLeft = Convert.ToInt32(OperateIniFile.ReadIniData("Visual", "wounds left segment", "0", NextFile.FullName));
                                    person.WoundsRight = Convert.ToInt32(OperateIniFile.ReadIniData("Visual", "wounds right segment", "0", NextFile.FullName));
                                    person.WoundsDesc = OperateIniFile.ReadIniData("Visual", "Wounds description", "", NextFile.FullName);
                                    person.WoundsDesc = person.WoundsDesc.Replace(";;", "\r\n");
                                }
                                if (person.Scars)
                                {
                                    person.ScarsLeft = Convert.ToInt32(OperateIniFile.ReadIniData("Visual", "scars left segment", "0", NextFile.FullName));
                                    person.ScarsRight = Convert.ToInt32(OperateIniFile.ReadIniData("Visual", "scars right segment", "0", NextFile.FullName));
                                    person.ScarsDesc = OperateIniFile.ReadIniData("Visual", "Scars description", "", NextFile.FullName);
                                    person.ScarsDesc = person.ScarsDesc.Replace(";;", "\r\n");
                                }
                            }
                            catch { }                            
                            
                            set.Add(person);
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(this, string.Format(App.Current.FindResource("Message_32").ToString() + ex.Message, NextFile.FullName));
                        }
                    }
                }
            }
            catch (Exception) { }

            //Handle folder
            DirectoryInfo[] dirInfo = theFolder.GetDirectories();            
            foreach (DirectoryInfo subFolder in dirInfo)
            {
                HandleFolder(subFolder.FullName, ref set);
            }
        }

        /// <summary>
        /// Technician发送数据事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendReportButton_Click(object sender, RoutedEventArgs e)
        {            
                        
            var selectedUserList = this.CodeListBox.SelectedItems;
            if (selectedUserList == null || selectedUserList.Count == 0)
            {
                MessageBox.Show(this, App.Current.FindResource("Message_35").ToString());
                return;
            }

            MessageBoxResult result = MessageBox.Show(this, App.Current.FindResource("Message_17").ToString(), "Send Data", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (result == MessageBoxResult.Yes)
            {
                if (App.reportSettingModel.TransferMode != TransferMode.CloudServer)
                {
                    //判断报告是否已经填写Techinican名称
                    SelectTechnicianPage selectTechnicianPage = new SelectTechnicianPage();
                    selectTechnicianPage.Owner = this;
                    var dialogResult = selectTechnicianPage.ShowDialog();
                    if (!dialogResult.HasValue || !dialogResult.Value)
                    {
                        return;
                    }    
                }

                if (App.reportSettingModel.TransferMode == TransferMode.Email)
                {
                    if (string.IsNullOrEmpty(App.reportSettingModel.MailAddress) || string.IsNullOrEmpty(App.reportSettingModel.MailHost)
                    || string.IsNullOrEmpty(App.reportSettingModel.MailUsername) || string.IsNullOrEmpty(App.reportSettingModel.MailPwd)
                    || string.IsNullOrEmpty(App.reportSettingModel.ToMailAddress))
                    {
                        MessageBox.Show(this, App.Current.FindResource("Message_15").ToString());
                        return;
                    }
                }
                else if (App.reportSettingModel.TransferMode == TransferMode.CloudServer)
                {
                    if (string.IsNullOrEmpty(App.reportSettingModel.CloudUser) || string.IsNullOrEmpty(App.reportSettingModel.CloudToken))
                    {
                        LoginPage loginPage = new LoginPage();
                        var loginResult = loginPage.ShowDialog();
                        if (!loginResult.HasValue || !loginResult.Value)
                        {
                            return;
                        }
                    }
                }
                else
                {
                    MessageBox.Show(this, App.Current.FindResource("Message_66").ToString());
                    return;
                }

                //隐藏已MRN区域显示的上传过的图标
                foreach (Person selectedUser in selectedUserList)
                {                    
                    selectedUser.Uploaded = Visibility.Collapsed.ToString();
                }
                //定义委托代理
                ProgressBarGridDelegate progressBarGridDelegate = new ProgressBarGridDelegate(progressBarGrid.SetValue);
                UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(uploadProgressBar.SetValue);
                //使用系统代理方式显示进度条面板
                Dispatcher.Invoke(progressBarGridDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { System.Windows.Controls.Grid.VisibilityProperty, Visibility.Visible });
                try
                {
                    List<string> errMsg = new List<string>();
                    int n = 0;
                    double groupValue = 100 / selectedUserList.Count;
                    double sizeValue = groupValue / 10;
                    //循环处理选择的每一个患者档案
                    foreach (Person selectedUser in selectedUserList)
                    {                                                
                        string dataFile = selectedUser.ArchiveFolder + System.IO.Path.DirectorySeparatorChar + selectedUser.Code + ".dat";
                        //如果是医生模式但当前的患者档案中却没有报告数据，则不能发送数据
                        if (!File.Exists(dataFile))
                        {
                            errMsg.Add(selectedUser.Code + " :: " + App.Current.FindResource("Message_16").ToString());
                            continue;
                        }
                        //如果没有导出PDF报告，则不能发送数据
                        if (selectedUser.Status != "RD")
                        {
                            errMsg.Add(selectedUser.Code + " :: " + App.Current.FindResource("Message_74").ToString());
                            continue;
                        }
                        
                        Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { System.Windows.Controls.ProgressBar.ValueProperty, Convert.ToDouble(groupValue * n + sizeValue) });

                        //打包数据文件，并上传到FPT服务
                        string zipFile = dataFolder + System.IO.Path.DirectorySeparatorChar + selectedUser.Code + "-" + selectedUser.SurName;
                        zipFile = zipFile + (string.IsNullOrEmpty(selectedUser.GivenName) ? "" : "," + selectedUser.GivenName) + (string.IsNullOrEmpty(selectedUser.OtherName) ? "" : " " + selectedUser.OtherName) + ".zip";

                        try
                        {                            
                            ZipTools.Instance.ZipFiles(selectedUser.ArchiveFolder, zipFile, 2);
                            Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { System.Windows.Controls.ProgressBar.ValueProperty, Convert.ToDouble(groupValue * n + sizeValue * 5) });
                        }
                        catch (Exception ex1)
                        {
                            errMsg.Add(selectedUser.Code + " :: " + string.Format(App.Current.FindResource("Message_21").ToString() + " " + ex1.Message, selectedUser.ArchiveFolder));
                            continue;
                        }

                        //上传到云服务器
                        try
                        {
                            if (App.reportSettingModel.TransferMode == TransferMode.CloudServer)
                            {
                                JObject jObject = new JObject();
                                jObject["code"] = selectedUser.Code;
                                jObject["firstname"] = selectedUser.GivenName;
                                jObject["lastname"] = selectedUser.SurName;
                                jObject["othername"] = selectedUser.OtherName;
                                jObject["birthday"] = selectedUser.BirthYear + "/" + selectedUser.BirthMonth + "/" + selectedUser.BirthDate;
                                jObject["mobile"] = selectedUser.Mobile;
                                jObject["email"] = selectedUser.Email;
                                jObject["clientnumber"] = selectedUser.ClientNumber;
                                jObject["country"] = selectedUser.Country;
                                jObject["city"] = selectedUser.City;
                                jObject["province"] = selectedUser.Province;
                                jObject["zipcode"] = selectedUser.ZipCode;
                                jObject["address"] = selectedUser.Address;
                                NameValueCollection nvlist=new NameValueCollection();
                                nvlist.Add("jsonStr", jObject.ToString());
                                //先上传PDF文件
                                string strName = selectedUser.SurName + (string.IsNullOrEmpty(selectedUser.GivenName) ? "" : "," + selectedUser.GivenName) + (string.IsNullOrEmpty(selectedUser.OtherName) ? "" : " " + selectedUser.OtherName) + ".pdf";
                                string pdfFile = selectedUser.ArchiveFolder + System.IO.Path.DirectorySeparatorChar + selectedUser.Code + " SFI - " + strName;
                                if (!File.Exists(pdfFile))
                                {
                                    errMsg.Add(selectedUser.Code + " :: " + App.Current.FindResource("Message_74").ToString());
                                    continue;
                                }
                                string resStr = HttpWebTools.UploadFile(App.reportSettingModel.CloudPath + "/api/sendPDF", pdfFile, nvlist, App.reportSettingModel.CloudToken);
                                var json = JObject.Parse(resStr);
                                bool res = (bool)json["status"];
                                if (res)
                                {
                                    //上传报告数据
                                    resStr = HttpWebTools.UploadFile(App.reportSettingModel.CloudPath + "/api/sendReport", zipFile, nvlist, App.reportSettingModel.CloudToken);
                                    var jsonObj = JObject.Parse(resStr);
                                    bool status = (bool)jsonObj["status"];
                                    if (status)
                                    {

                                        selectedUser.Status = "RS";
                                        selectedUser.StatusText = App.Current.FindResource("CommonStatusReportSent").ToString();
                                        //保存已发送狀態
                                        OperateIniFile.WriteIniData("Report", "Status", "RS", selectedUser.IniFilePath);
                                        selectedUser.Uploaded = Visibility.Visible.ToString();
                                    }
                                    else
                                    {
                                        var info = (string)jsonObj["info"];
                                        errMsg.Add(selectedUser.Code + " :: " + App.Current.FindResource("Message_52").ToString() + info);
                                    }
                                }
                                else
                                {
                                    errMsg.Add(selectedUser.Code + " :: " + App.Current.FindResource("Message_52").ToString() + json["info"]);
                                }
                            }
                            else if (App.reportSettingModel.TransferMode == TransferMode.Email)
                            {
                                string senderServerIp = App.reportSettingModel.MailHost;
                                string fromMailAddress = App.reportSettingModel.MailAddress;
                                string mailUsername = App.reportSettingModel.MailUsername;
                                string mailPassword = App.reportSettingModel.MailPwd;
                                string mailPort = App.reportSettingModel.MailPort + "";
                                bool isSsl = App.reportSettingModel.MailSsl;

                                string toMailAddress = App.reportSettingModel.ToMailAddress;
                                string subjectInfo = App.reportSettingModel.MailSubject + " (" + selectedUser.Code + "-" + selectedUser.SurName + ")";
                                string bodyInfo = App.reportSettingModel.MailBody;                                
                                string attachPath = zipFile;

                                EmailHelper email = new EmailHelper(senderServerIp, toMailAddress, fromMailAddress, subjectInfo, bodyInfo, mailUsername, mailPassword, mailPort, isSsl, false);
                                email.AddAttachments(attachPath);
                                email.Send();

                                selectedUser.Status = "RS";
                                selectedUser.StatusText = App.Current.FindResource("CommonStatusReportSent").ToString();
                                //保存已发送狀態
                                OperateIniFile.WriteIniData("Report", "Status", "RS", selectedUser.IniFilePath);
                                selectedUser.Uploaded = Visibility.Visible.ToString();
                            }                                                                                   
                        }
                        catch (Exception ex2)
                        {
                            errMsg.Add(selectedUser.Code + " :: " + App.Current.FindResource("Message_52").ToString() + " " + ex2.Message);
                            continue;
                        }
                        finally
                        {
                            try
                            {
                                File.Delete(zipFile);
                            }
                            catch { }
                        }
                        Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { System.Windows.Controls.ProgressBar.ValueProperty, Convert.ToDouble(groupValue * n + sizeValue * 9) });
                        
                        n++;
                    }

                    //try
                    //{
                    //    //发送通知邮件  
                    //    SendEmail((selectedUserList.Count - errMsg.Count), true);
                    //}
                    //catch (Exception ex3)
                    //{
                    //    errMsg.Add(App.Current.FindResource("Message_19").ToString() + " " + ex3.Message);
                    //}

                    Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { System.Windows.Controls.ProgressBar.ValueProperty, Convert.ToDouble(100) });
                    //显示没有成功发送数据的错误消息
                    if (errMsg.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(string.Format(App.Current.FindResource("Message_36").ToString(), errMsg.Count));
                        foreach (var err in errMsg)
                        {
                            sb.Append("\r\n" + err);
                        }
                        MessageBox.Show(this, sb.ToString());
                    }
                    else
                    {
                        MessageBox.Show(this, App.Current.FindResource("Message_18").ToString());
                    }
                }
                finally
                {
                    Dispatcher.Invoke(progressBarGridDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { System.Windows.Controls.Grid.VisibilityProperty, Visibility.Collapsed });
                }
            }
        }
        

        /// <summary>
        /// 上传数据成功后发送的通知邮件
        /// </summary>
        /// <param name="patientNum"></param>
        /// <param name="isReport"></param>
        private void SendEmail(int patientNum,bool isReport=false)
        {
            //发送通知邮件                
            try
            {
                if (patientNum > 0)
                {
                    string senderServerIp = App.reportSettingModel.MailHost;
                    string toMailAddress = App.reportSettingModel.ToMailAddress;
                    string fromMailAddress = App.reportSettingModel.MailAddress;
                    string subjectInfo = App.reportSettingModel.MailSubject;
                    string bodyInfo = string.Format(App.reportSettingModel.MailBody, patientNum);
                    string mailUsername = App.reportSettingModel.MailUsername;
                    string mailPassword = App.reportSettingModel.MailPwd;
                    string mailPort = App.reportSettingModel.MailPort + "";                    
                    bool isSsl = App.reportSettingModel.MailSsl;
                    EmailHelper email = new EmailHelper(senderServerIp, toMailAddress, fromMailAddress, subjectInfo, bodyInfo, mailUsername, mailPassword, mailPort, isSsl, false);                    
                    email.Send();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(this, App.Current.FindResource("Message_19").ToString() + " " + ex.Message);
            }
        }

        
        
        /// <summary>
        /// Changed the icon for MRN list item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CodeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            #region 加载客户信息数据
            try
            {
                if (e.AddedItems.Count > 0)
                {
                    var selectItems = e.AddedItems;
                    foreach (Person item in selectItems)
                    {
                        item.Icon = "/Images/id_card_ok.png";
                    }
                    var selectItem = e.AddedItems[0] as Person;
                    string meikiniFile = App.meikFolder + System.IO.Path.DirectorySeparatorChar + "MEIK.ini";
                    OperateIniFile.WriteIniData("Base", "Patients base", selectItem.ArchiveFolder, meikiniFile);

                    
                    if (selectItem.PalpableLumps)
                    {
                        leftClock1.Opacity = (selectItem.LeftPosition & Convert.ToInt32(leftClock1.Tag)) > 0 ? 1 : 0;
                        leftClock2.Opacity = (selectItem.LeftPosition & Convert.ToInt32(leftClock2.Tag)) > 0 ? 1 : 0;
                        leftClock3.Opacity = (selectItem.LeftPosition & Convert.ToInt32(leftClock3.Tag)) > 0 ? 1 : 0;
                        leftClock4.Opacity = (selectItem.LeftPosition & Convert.ToInt32(leftClock4.Tag)) > 0 ? 1 : 0;
                        leftClock5.Opacity = (selectItem.LeftPosition & Convert.ToInt32(leftClock5.Tag)) > 0 ? 1 : 0;
                        leftClock6.Opacity = (selectItem.LeftPosition & Convert.ToInt32(leftClock6.Tag)) > 0 ? 1 : 0;
                        leftClock7.Opacity = (selectItem.LeftPosition & Convert.ToInt32(leftClock7.Tag)) > 0 ? 1 : 0;
                        leftClock8.Opacity = (selectItem.LeftPosition & Convert.ToInt32(leftClock8.Tag)) > 0 ? 1 : 0;
                        leftClock9.Opacity = (selectItem.LeftPosition & Convert.ToInt32(leftClock9.Tag)) > 0 ? 1 : 0;
                        leftClock10.Opacity = (selectItem.LeftPosition & Convert.ToInt32(leftClock10.Tag)) > 0 ? 1 : 0;
                        leftClock11.Opacity = (selectItem.LeftPosition & Convert.ToInt32(leftClock11.Tag)) > 0 ? 1 : 0;
                        leftClock12.Opacity = (selectItem.LeftPosition & Convert.ToInt32(leftClock12.Tag)) > 0 ? 1 : 0;

                        rightClock1.Opacity = (selectItem.RightPosition & Convert.ToInt32(rightClock1.Tag)) > 0 ? 1 : 0;
                        rightClock2.Opacity = (selectItem.RightPosition & Convert.ToInt32(rightClock2.Tag)) > 0 ? 1 : 0;
                        rightClock3.Opacity = (selectItem.RightPosition & Convert.ToInt32(rightClock3.Tag)) > 0 ? 1 : 0;
                        rightClock4.Opacity = (selectItem.RightPosition & Convert.ToInt32(rightClock4.Tag)) > 0 ? 1 : 0;
                        rightClock5.Opacity = (selectItem.RightPosition & Convert.ToInt32(rightClock5.Tag)) > 0 ? 1 : 0;
                        rightClock6.Opacity = (selectItem.RightPosition & Convert.ToInt32(rightClock6.Tag)) > 0 ? 1 : 0;
                        rightClock7.Opacity = (selectItem.RightPosition & Convert.ToInt32(rightClock7.Tag)) > 0 ? 1 : 0;
                        rightClock8.Opacity = (selectItem.RightPosition & Convert.ToInt32(rightClock8.Tag)) > 0 ? 1 : 0;
                        rightClock9.Opacity = (selectItem.RightPosition & Convert.ToInt32(rightClock9.Tag)) > 0 ? 1 : 0;
                        rightClock10.Opacity = (selectItem.RightPosition & Convert.ToInt32(rightClock10.Tag)) > 0 ? 1 : 0;
                        rightClock11.Opacity = (selectItem.RightPosition & Convert.ToInt32(rightClock11.Tag)) > 0 ? 1 : 0;
                        rightClock12.Opacity = (selectItem.RightPosition & Convert.ToInt32(rightClock12.Tag)) > 0 ? 1 : 0;

                    }
                    else
                    {
                        leftClock1.Opacity = 0;
                        leftClock2.Opacity = 0;
                        leftClock3.Opacity = 0;
                        leftClock4.Opacity = 0;
                        leftClock5.Opacity = 0;
                        leftClock6.Opacity = 0;
                        leftClock7.Opacity = 0;
                        leftClock8.Opacity = 0;
                        leftClock9.Opacity = 0;                        
                        leftClock10.Opacity = 0;
                        leftClock11.Opacity = 0;
                        leftClock12.Opacity = 0;

                        rightClock1.Opacity = 0;
                        rightClock2.Opacity = 0;
                        rightClock3.Opacity = 0;
                        rightClock4.Opacity = 0;
                        rightClock5.Opacity = 0;
                        rightClock6.Opacity = 0;
                        rightClock7.Opacity = 0;
                        rightClock8.Opacity = 0;
                        rightClock9.Opacity = 0;
                        rightClock10.Opacity = 0;
                        rightClock11.Opacity = 0;
                        rightClock12.Opacity = 0;
                    }

                    if (selectItem.Pain)
                    {
                        degree1.IsChecked = (selectItem.Degree == Convert.ToInt32(degree1.Tag)) ? true : false;
                        degree2.IsChecked = (selectItem.Degree == Convert.ToInt32(degree2.Tag)) ? true : false;
                        degree3.IsChecked = (selectItem.Degree == Convert.ToInt32(degree3.Tag)) ? true : false;
                        degree4.IsChecked = (selectItem.Degree == Convert.ToInt32(degree4.Tag)) ? true : false;
                        degree5.IsChecked = (selectItem.Degree == Convert.ToInt32(degree5.Tag)) ? true : false;
                        degree6.IsChecked = (selectItem.Degree == Convert.ToInt32(degree6.Tag)) ? true : false;
                        degree7.IsChecked = (selectItem.Degree == Convert.ToInt32(degree7.Tag)) ? true : false;
                        degree8.IsChecked = (selectItem.Degree == Convert.ToInt32(degree8.Tag)) ? true : false;
                        degree9.IsChecked = (selectItem.Degree == Convert.ToInt32(degree9.Tag)) ? true : false;
                        degree10.IsChecked = (selectItem.Degree == Convert.ToInt32(degree10.Tag)) ? true : false;
                        degree1.IsEnabled = true;
                        degree2.IsEnabled = true;
                        degree3.IsEnabled = true;
                        degree4.IsEnabled = true;
                        degree5.IsEnabled = true;
                        degree6.IsEnabled = true;
                        degree7.IsEnabled = true;
                        degree8.IsEnabled = true;
                        degree9.IsEnabled = true;
                        degree10.IsEnabled = true;
                    }
                    else
                    {
                        degree1.IsChecked = false;
                        degree2.IsChecked = false;
                        degree3.IsChecked = false;
                        degree4.IsChecked = false;
                        degree5.IsChecked = false;
                        degree6.IsChecked = false;
                        degree7.IsChecked = false;
                        degree8.IsChecked = false;
                        degree9.IsChecked = false;
                        degree10.IsChecked = false;
                    }

                    if (selectItem.RedSwollen)
                    {
                        //redSwollenLeft.IsEnabled=true;
                        //redSwollenRight.IsEnabled = true;
                        leftClockRedSwollen1.Opacity = (selectItem.RedSwollenLeft & Convert.ToInt32(leftClockRedSwollen1.Tag)) > 0 ? 1 : 0;
                        leftClockRedSwollen2.Opacity = (selectItem.RedSwollenLeft & Convert.ToInt32(leftClockRedSwollen2.Tag)) > 0 ? 1 : 0;
                        leftClockRedSwollen3.Opacity = (selectItem.RedSwollenLeft & Convert.ToInt32(leftClockRedSwollen3.Tag)) > 0 ? 1 : 0;
                        leftClockRedSwollen4.Opacity = (selectItem.RedSwollenLeft & Convert.ToInt32(leftClockRedSwollen4.Tag)) > 0 ? 1 : 0;
                        leftClockRedSwollen5.Opacity = (selectItem.RedSwollenLeft & Convert.ToInt32(leftClockRedSwollen5.Tag)) > 0 ? 1 : 0;
                        leftClockRedSwollen6.Opacity = (selectItem.RedSwollenLeft & Convert.ToInt32(leftClockRedSwollen6.Tag)) > 0 ? 1 : 0;
                        leftClockRedSwollen7.Opacity = (selectItem.RedSwollenLeft & Convert.ToInt32(leftClockRedSwollen7.Tag)) > 0 ? 1 : 0;
                        leftClockRedSwollen8.Opacity = (selectItem.RedSwollenLeft & Convert.ToInt32(leftClockRedSwollen8.Tag)) > 0 ? 1 : 0;
                        leftClockRedSwollen9.Opacity = (selectItem.RedSwollenLeft & Convert.ToInt32(leftClockRedSwollen9.Tag)) > 0 ? 1 : 0;
                        leftClockRedSwollen10.Opacity = (selectItem.RedSwollenLeft & Convert.ToInt32(leftClockRedSwollen10.Tag)) > 0 ? 1 : 0;
                        leftClockRedSwollen11.Opacity = (selectItem.RedSwollenLeft & Convert.ToInt32(leftClockRedSwollen11.Tag)) > 0 ? 1 : 0;
                        leftClockRedSwollen12.Opacity = (selectItem.RedSwollenLeft & Convert.ToInt32(leftClockRedSwollen12.Tag)) > 0 ? 1 : 0;

                        rightClockRedSwollen1.Opacity = (selectItem.RedSwollenRight & Convert.ToInt32(rightClockRedSwollen1.Tag)) > 0 ? 1 : 0;
                        rightClockRedSwollen2.Opacity = (selectItem.RedSwollenRight & Convert.ToInt32(rightClockRedSwollen2.Tag)) > 0 ? 1 : 0;
                        rightClockRedSwollen3.Opacity = (selectItem.RedSwollenRight & Convert.ToInt32(rightClockRedSwollen3.Tag)) > 0 ? 1 : 0;
                        rightClockRedSwollen4.Opacity = (selectItem.RedSwollenRight & Convert.ToInt32(rightClockRedSwollen4.Tag)) > 0 ? 1 : 0;
                        rightClockRedSwollen5.Opacity = (selectItem.RedSwollenRight & Convert.ToInt32(rightClockRedSwollen5.Tag)) > 0 ? 1 : 0;
                        rightClockRedSwollen6.Opacity = (selectItem.RedSwollenRight & Convert.ToInt32(rightClockRedSwollen6.Tag)) > 0 ? 1 : 0;
                        rightClockRedSwollen7.Opacity = (selectItem.RedSwollenRight & Convert.ToInt32(rightClockRedSwollen7.Tag)) > 0 ? 1 : 0;
                        rightClockRedSwollen8.Opacity = (selectItem.RedSwollenRight & Convert.ToInt32(rightClockRedSwollen8.Tag)) > 0 ? 1 : 0;
                        rightClockRedSwollen9.Opacity = (selectItem.RedSwollenRight & Convert.ToInt32(rightClockRedSwollen9.Tag)) > 0 ? 1 : 0;
                        rightClockRedSwollen10.Opacity = (selectItem.RedSwollenRight & Convert.ToInt32(rightClockRedSwollen10.Tag)) > 0 ? 1 : 0;
                        rightClockRedSwollen11.Opacity = (selectItem.RedSwollenRight & Convert.ToInt32(rightClockRedSwollen11.Tag)) > 0 ? 1 : 0;
                        rightClockRedSwollen12.Opacity = (selectItem.RedSwollenRight & Convert.ToInt32(rightClockRedSwollen12.Tag)) > 0 ? 1 : 0;
                    }
                    else
                    {
                        //redSwollenLeft.IsEnabled = false;
                        //redSwollenRight.IsEnabled = false;
                        leftClockRedSwollen1.Opacity = 0;
                        leftClockRedSwollen2.Opacity = 0;
                        leftClockRedSwollen3.Opacity = 0;
                        leftClockRedSwollen4.Opacity = 0;
                        leftClockRedSwollen5.Opacity = 0;
                        leftClockRedSwollen6.Opacity = 0;
                        leftClockRedSwollen7.Opacity = 0;
                        leftClockRedSwollen8.Opacity = 0;
                        leftClockRedSwollen9.Opacity = 0;
                        leftClockRedSwollen10.Opacity = 0;
                        leftClockRedSwollen11.Opacity = 0;
                        leftClockRedSwollen12.Opacity = 0;

                        rightClockRedSwollen1.Opacity = 0;
                        rightClockRedSwollen2.Opacity = 0;
                        rightClockRedSwollen3.Opacity = 0;
                        rightClockRedSwollen4.Opacity = 0;
                        rightClockRedSwollen5.Opacity = 0;
                        rightClockRedSwollen6.Opacity = 0;
                        rightClockRedSwollen7.Opacity = 0;
                        rightClockRedSwollen8.Opacity = 0;
                        rightClockRedSwollen9.Opacity = 0;
                        rightClockRedSwollen10.Opacity = 0;
                        rightClockRedSwollen11.Opacity = 0;
                        rightClockRedSwollen12.Opacity = 0;
                    }

                    if (selectItem.Palpable)
                    {
                        //palpableLeft.IsEnabled = true;
                        //palpableRight.IsEnabled = true;
                        leftClockPalpable1.Opacity = (selectItem.PalpableLeft & Convert.ToInt32(leftClockPalpable1.Tag)) > 0 ? 1 : 0;
                        leftClockPalpable2.Opacity = (selectItem.PalpableLeft & Convert.ToInt32(leftClockPalpable2.Tag)) > 0 ? 1 : 0;
                        leftClockPalpable3.Opacity = (selectItem.PalpableLeft & Convert.ToInt32(leftClockPalpable3.Tag)) > 0 ? 1 : 0;
                        leftClockPalpable4.Opacity = (selectItem.PalpableLeft & Convert.ToInt32(leftClockPalpable4.Tag)) > 0 ? 1 : 0;
                        leftClockPalpable5.Opacity = (selectItem.PalpableLeft & Convert.ToInt32(leftClockPalpable5.Tag)) > 0 ? 1 : 0;
                        leftClockPalpable6.Opacity = (selectItem.PalpableLeft & Convert.ToInt32(leftClockPalpable6.Tag)) > 0 ? 1 : 0;
                        leftClockPalpable7.Opacity = (selectItem.PalpableLeft & Convert.ToInt32(leftClockPalpable7.Tag)) > 0 ? 1 : 0;
                        leftClockPalpable8.Opacity = (selectItem.PalpableLeft & Convert.ToInt32(leftClockPalpable8.Tag)) > 0 ? 1 : 0;
                        leftClockPalpable9.Opacity = (selectItem.PalpableLeft & Convert.ToInt32(leftClockPalpable9.Tag)) > 0 ? 1 : 0;
                        leftClockPalpable10.Opacity = (selectItem.PalpableLeft & Convert.ToInt32(leftClockPalpable10.Tag)) > 0 ? 1 : 0;
                        leftClockPalpable11.Opacity = (selectItem.PalpableLeft & Convert.ToInt32(leftClockPalpable11.Tag)) > 0 ? 1 : 0;
                        leftClockPalpable12.Opacity = (selectItem.PalpableLeft & Convert.ToInt32(leftClockPalpable12.Tag)) > 0 ? 1 : 0;

                        rightClockPalpable1.Opacity = (selectItem.PalpableRight & Convert.ToInt32(rightClockPalpable1.Tag)) > 0 ? 1 : 0;
                        rightClockPalpable2.Opacity = (selectItem.PalpableRight & Convert.ToInt32(rightClockPalpable2.Tag)) > 0 ? 1 : 0;
                        rightClockPalpable3.Opacity = (selectItem.PalpableRight & Convert.ToInt32(rightClockPalpable3.Tag)) > 0 ? 1 : 0;
                        rightClockPalpable4.Opacity = (selectItem.PalpableRight & Convert.ToInt32(rightClockPalpable4.Tag)) > 0 ? 1 : 0;
                        rightClockPalpable5.Opacity = (selectItem.PalpableRight & Convert.ToInt32(rightClockPalpable5.Tag)) > 0 ? 1 : 0;
                        rightClockPalpable6.Opacity = (selectItem.PalpableRight & Convert.ToInt32(rightClockPalpable6.Tag)) > 0 ? 1 : 0;
                        rightClockPalpable7.Opacity = (selectItem.PalpableRight & Convert.ToInt32(rightClockPalpable7.Tag)) > 0 ? 1 : 0;
                        rightClockPalpable8.Opacity = (selectItem.PalpableRight & Convert.ToInt32(rightClockPalpable8.Tag)) > 0 ? 1 : 0;
                        rightClockPalpable9.Opacity = (selectItem.PalpableRight & Convert.ToInt32(rightClockPalpable9.Tag)) > 0 ? 1 : 0;
                        rightClockPalpable10.Opacity = (selectItem.PalpableRight & Convert.ToInt32(rightClockPalpable10.Tag)) > 0 ? 1 : 0;
                        rightClockPalpable11.Opacity = (selectItem.PalpableRight & Convert.ToInt32(rightClockPalpable11.Tag)) > 0 ? 1 : 0;
                        rightClockPalpable12.Opacity = (selectItem.PalpableRight & Convert.ToInt32(rightClockPalpable12.Tag)) > 0 ? 1 : 0;
                    }
                    else
                    {
                        leftClockPalpable1.Opacity = 0;
                        leftClockPalpable2.Opacity = 0;
                        leftClockPalpable3.Opacity = 0;
                        leftClockPalpable4.Opacity = 0;
                        leftClockPalpable5.Opacity = 0;
                        leftClockPalpable6.Opacity = 0;
                        leftClockPalpable7.Opacity = 0;
                        leftClockPalpable8.Opacity = 0;
                        leftClockPalpable9.Opacity = 0;
                        leftClockPalpable10.Opacity = 0;
                        leftClockPalpable11.Opacity = 0;
                        leftClockPalpable12.Opacity = 0;

                        rightClockPalpable1.Opacity = 0;
                        rightClockPalpable2.Opacity = 0;
                        rightClockPalpable3.Opacity = 0;
                        rightClockPalpable4.Opacity = 0;
                        rightClockPalpable5.Opacity = 0;
                        rightClockPalpable6.Opacity = 0;
                        rightClockPalpable7.Opacity = 0;
                        rightClockPalpable8.Opacity = 0;
                        rightClockPalpable9.Opacity = 0;
                        rightClockPalpable10.Opacity = 0;
                        rightClockPalpable11.Opacity = 0;
                        rightClockPalpable12.Opacity = 0;
                    }

                    if (selectItem.Serous)
                    {
                        //serousLeft.IsEnabled = true;
                        //serousRight.IsEnabled = true;
                        leftClockSerious1.Opacity = (selectItem.SerousLeft & Convert.ToInt32(leftClockSerious1.Tag)) > 0 ? 1 : 0;
                        leftClockSerious2.Opacity = (selectItem.SerousLeft & Convert.ToInt32(leftClockSerious2.Tag)) > 0 ? 1 : 0;
                        leftClockSerious3.Opacity = (selectItem.SerousLeft & Convert.ToInt32(leftClockSerious3.Tag)) > 0 ? 1 : 0;
                        leftClockSerious4.Opacity = (selectItem.SerousLeft & Convert.ToInt32(leftClockSerious4.Tag)) > 0 ? 1 : 0;
                        leftClockSerious5.Opacity = (selectItem.SerousLeft & Convert.ToInt32(leftClockSerious5.Tag)) > 0 ? 1 : 0;
                        leftClockSerious6.Opacity = (selectItem.SerousLeft & Convert.ToInt32(leftClockSerious6.Tag)) > 0 ? 1 : 0;
                        leftClockSerious7.Opacity = (selectItem.SerousLeft & Convert.ToInt32(leftClockSerious7.Tag)) > 0 ? 1 : 0;
                        leftClockSerious8.Opacity = (selectItem.SerousLeft & Convert.ToInt32(leftClockSerious8.Tag)) > 0 ? 1 : 0;
                        leftClockSerious9.Opacity = (selectItem.SerousLeft & Convert.ToInt32(leftClockSerious9.Tag)) > 0 ? 1 : 0;
                        leftClockSerious10.Opacity = (selectItem.SerousLeft & Convert.ToInt32(leftClockSerious10.Tag)) > 0 ? 1 : 0;
                        leftClockSerious11.Opacity = (selectItem.SerousLeft & Convert.ToInt32(leftClockSerious11.Tag)) > 0 ? 1 : 0;
                        leftClockSerious12.Opacity = (selectItem.SerousLeft & Convert.ToInt32(leftClockSerious12.Tag)) > 0 ? 1 : 0;

                        rightClockSerious1.Opacity = (selectItem.SerousRight & Convert.ToInt32(rightClockSerious1.Tag)) > 0 ? 1 : 0;
                        rightClockSerious2.Opacity = (selectItem.SerousRight & Convert.ToInt32(rightClockSerious2.Tag)) > 0 ? 1 : 0;
                        rightClockSerious3.Opacity = (selectItem.SerousRight & Convert.ToInt32(rightClockSerious3.Tag)) > 0 ? 1 : 0;
                        rightClockSerious4.Opacity = (selectItem.SerousRight & Convert.ToInt32(rightClockSerious4.Tag)) > 0 ? 1 : 0;
                        rightClockSerious5.Opacity = (selectItem.SerousRight & Convert.ToInt32(rightClockSerious5.Tag)) > 0 ? 1 : 0;
                        rightClockSerious6.Opacity = (selectItem.SerousRight & Convert.ToInt32(rightClockSerious6.Tag)) > 0 ? 1 : 0;
                        rightClockSerious7.Opacity = (selectItem.SerousRight & Convert.ToInt32(rightClockSerious7.Tag)) > 0 ? 1 : 0;
                        rightClockSerious8.Opacity = (selectItem.SerousRight & Convert.ToInt32(rightClockSerious8.Tag)) > 0 ? 1 : 0;
                        rightClockSerious9.Opacity = (selectItem.SerousRight & Convert.ToInt32(rightClockSerious9.Tag)) > 0 ? 1 : 0;
                        rightClockSerious10.Opacity = (selectItem.SerousRight & Convert.ToInt32(rightClockSerious10.Tag)) > 0 ? 1 : 0;
                        rightClockSerious11.Opacity = (selectItem.SerousRight & Convert.ToInt32(rightClockSerious11.Tag)) > 0 ? 1 : 0;
                        rightClockSerious12.Opacity = (selectItem.SerousRight & Convert.ToInt32(rightClockSerious12.Tag)) > 0 ? 1 : 0;
                    }
                    else
                    {
                        leftClockSerious1.Opacity = 0;
                        leftClockSerious2.Opacity = 0;
                        leftClockSerious3.Opacity = 0;
                        leftClockSerious4.Opacity = 0;
                        leftClockSerious5.Opacity = 0;
                        leftClockSerious6.Opacity = 0;
                        leftClockSerious7.Opacity = 0;
                        leftClockSerious8.Opacity = 0;
                        leftClockSerious9.Opacity = 0;
                        leftClockSerious10.Opacity = 0;
                        leftClockSerious11.Opacity = 0;
                        leftClockSerious12.Opacity = 0;

                        rightClockSerious1.Opacity = 0;
                        rightClockSerious2.Opacity = 0;
                        rightClockSerious3.Opacity = 0;
                        rightClockSerious4.Opacity = 0;
                        rightClockSerious5.Opacity = 0;
                        rightClockSerious6.Opacity = 0;
                        rightClockSerious7.Opacity = 0;
                        rightClockSerious8.Opacity = 0;
                        rightClockSerious9.Opacity = 0;
                        rightClockSerious10.Opacity = 0;
                        rightClockSerious11.Opacity = 0;
                        rightClockSerious12.Opacity = 0;
                    }

                    if (selectItem.Wounds)
                    {
                        //woundsLeft.IsEnabled = true;
                        //woundsRight.IsEnabled = true;
                        leftClockWounds1.Opacity = (selectItem.WoundsLeft & Convert.ToInt32(leftClockWounds1.Tag)) > 0 ? 1 : 0;
                        leftClockWounds2.Opacity = (selectItem.WoundsLeft & Convert.ToInt32(leftClockWounds2.Tag)) > 0 ? 1 : 0;
                        leftClockWounds3.Opacity = (selectItem.WoundsLeft & Convert.ToInt32(leftClockWounds3.Tag)) > 0 ? 1 : 0;
                        leftClockWounds4.Opacity = (selectItem.WoundsLeft & Convert.ToInt32(leftClockWounds4.Tag)) > 0 ? 1 : 0;
                        leftClockWounds5.Opacity = (selectItem.WoundsLeft & Convert.ToInt32(leftClockWounds5.Tag)) > 0 ? 1 : 0;
                        leftClockWounds6.Opacity = (selectItem.WoundsLeft & Convert.ToInt32(leftClockWounds6.Tag)) > 0 ? 1 : 0;
                        leftClockWounds7.Opacity = (selectItem.WoundsLeft & Convert.ToInt32(leftClockWounds7.Tag)) > 0 ? 1 : 0;
                        leftClockWounds8.Opacity = (selectItem.WoundsLeft & Convert.ToInt32(leftClockWounds8.Tag)) > 0 ? 1 : 0;
                        leftClockWounds9.Opacity = (selectItem.WoundsLeft & Convert.ToInt32(leftClockWounds9.Tag)) > 0 ? 1 : 0;
                        leftClockWounds10.Opacity = (selectItem.WoundsLeft & Convert.ToInt32(leftClockWounds10.Tag)) > 0 ? 1 : 0;
                        leftClockWounds11.Opacity = (selectItem.WoundsLeft & Convert.ToInt32(leftClockWounds11.Tag)) > 0 ? 1 : 0;
                        leftClockWounds12.Opacity = (selectItem.WoundsLeft & Convert.ToInt32(leftClockWounds12.Tag)) > 0 ? 1 : 0;

                        rightClockWounds1.Opacity = (selectItem.WoundsRight & Convert.ToInt32(rightClockWounds1.Tag)) > 0 ? 1 : 0;
                        rightClockWounds2.Opacity = (selectItem.WoundsRight & Convert.ToInt32(rightClockWounds2.Tag)) > 0 ? 1 : 0;
                        rightClockWounds3.Opacity = (selectItem.WoundsRight & Convert.ToInt32(rightClockWounds3.Tag)) > 0 ? 1 : 0;
                        rightClockWounds4.Opacity = (selectItem.WoundsRight & Convert.ToInt32(rightClockWounds4.Tag)) > 0 ? 1 : 0;
                        rightClockWounds5.Opacity = (selectItem.WoundsRight & Convert.ToInt32(rightClockWounds5.Tag)) > 0 ? 1 : 0;
                        rightClockWounds6.Opacity = (selectItem.WoundsRight & Convert.ToInt32(rightClockWounds6.Tag)) > 0 ? 1 : 0;
                        rightClockWounds7.Opacity = (selectItem.WoundsRight & Convert.ToInt32(rightClockWounds7.Tag)) > 0 ? 1 : 0;
                        rightClockWounds8.Opacity = (selectItem.WoundsRight & Convert.ToInt32(rightClockWounds8.Tag)) > 0 ? 1 : 0;
                        rightClockWounds9.Opacity = (selectItem.WoundsRight & Convert.ToInt32(rightClockWounds9.Tag)) > 0 ? 1 : 0;
                        rightClockWounds10.Opacity = (selectItem.WoundsRight & Convert.ToInt32(rightClockWounds10.Tag)) > 0 ? 1 : 0;
                        rightClockWounds11.Opacity = (selectItem.WoundsRight & Convert.ToInt32(rightClockWounds11.Tag)) > 0 ? 1 : 0;
                        rightClockWounds12.Opacity = (selectItem.WoundsRight & Convert.ToInt32(rightClockWounds12.Tag)) > 0 ? 1 : 0;
                    }
                    else
                    {
                        leftClockWounds1.Opacity = 0;
                        leftClockWounds2.Opacity = 0;
                        leftClockWounds3.Opacity = 0;
                        leftClockWounds4.Opacity = 0;
                        leftClockWounds5.Opacity = 0;
                        leftClockWounds6.Opacity = 0;
                        leftClockWounds7.Opacity = 0;
                        leftClockWounds8.Opacity = 0;
                        leftClockWounds9.Opacity = 0;
                        leftClockWounds10.Opacity = 0;
                        leftClockWounds11.Opacity = 0;
                        leftClockWounds12.Opacity = 0;

                        rightClockWounds1.Opacity = 0;
                        rightClockWounds2.Opacity = 0;
                        rightClockWounds3.Opacity = 0;
                        rightClockWounds4.Opacity = 0;
                        rightClockWounds5.Opacity = 0;
                        rightClockWounds6.Opacity = 0;
                        rightClockWounds7.Opacity = 0;
                        rightClockWounds8.Opacity = 0;
                        rightClockWounds9.Opacity = 0;
                        rightClockWounds10.Opacity = 0;
                        rightClockWounds11.Opacity = 0;
                        rightClockWounds12.Opacity = 0;
                    }

                    if (selectItem.Scars)
                    {
                        //scarsLeft.IsEnabled = true;
                        //scarsRight.IsEnabled = true;
                        leftClockScars1.Opacity = (selectItem.ScarsLeft & Convert.ToInt32(leftClockScars1.Tag)) > 0 ? 1 : 0;
                        leftClockScars2.Opacity = (selectItem.ScarsLeft & Convert.ToInt32(leftClockScars2.Tag)) > 0 ? 1 : 0;
                        leftClockScars3.Opacity = (selectItem.ScarsLeft & Convert.ToInt32(leftClockScars3.Tag)) > 0 ? 1 : 0;
                        leftClockScars4.Opacity = (selectItem.ScarsLeft & Convert.ToInt32(leftClockScars4.Tag)) > 0 ? 1 : 0;
                        leftClockScars5.Opacity = (selectItem.ScarsLeft & Convert.ToInt32(leftClockScars5.Tag)) > 0 ? 1 : 0;
                        leftClockScars6.Opacity = (selectItem.ScarsLeft & Convert.ToInt32(leftClockScars6.Tag)) > 0 ? 1 : 0;
                        leftClockScars7.Opacity = (selectItem.ScarsLeft & Convert.ToInt32(leftClockScars7.Tag)) > 0 ? 1 : 0;
                        leftClockScars8.Opacity = (selectItem.ScarsLeft & Convert.ToInt32(leftClockScars8.Tag)) > 0 ? 1 : 0;
                        leftClockScars9.Opacity = (selectItem.ScarsLeft & Convert.ToInt32(leftClockScars9.Tag)) > 0 ? 1 : 0;
                        leftClockScars10.Opacity = (selectItem.ScarsLeft & Convert.ToInt32(leftClockScars10.Tag)) > 0 ? 1 : 0;
                        leftClockScars11.Opacity = (selectItem.ScarsLeft & Convert.ToInt32(leftClockScars11.Tag)) > 0 ? 1 : 0;
                        leftClockScars12.Opacity = (selectItem.ScarsLeft & Convert.ToInt32(leftClockScars12.Tag)) > 0 ? 1 : 0;

                        rightClockScars1.Opacity = (selectItem.ScarsRight & Convert.ToInt32(rightClockScars1.Tag)) > 0 ? 1 : 0;
                        rightClockScars2.Opacity = (selectItem.ScarsRight & Convert.ToInt32(rightClockScars2.Tag)) > 0 ? 1 : 0;
                        rightClockScars3.Opacity = (selectItem.ScarsRight & Convert.ToInt32(rightClockScars3.Tag)) > 0 ? 1 : 0;
                        rightClockScars4.Opacity = (selectItem.ScarsRight & Convert.ToInt32(rightClockScars4.Tag)) > 0 ? 1 : 0;
                        rightClockScars5.Opacity = (selectItem.ScarsRight & Convert.ToInt32(rightClockScars5.Tag)) > 0 ? 1 : 0;
                        rightClockScars6.Opacity = (selectItem.ScarsRight & Convert.ToInt32(rightClockScars6.Tag)) > 0 ? 1 : 0;
                        rightClockScars7.Opacity = (selectItem.ScarsRight & Convert.ToInt32(rightClockScars7.Tag)) > 0 ? 1 : 0;
                        rightClockScars8.Opacity = (selectItem.ScarsRight & Convert.ToInt32(rightClockScars8.Tag)) > 0 ? 1 : 0;
                        rightClockScars9.Opacity = (selectItem.ScarsRight & Convert.ToInt32(rightClockScars9.Tag)) > 0 ? 1 : 0;
                        rightClockScars10.Opacity = (selectItem.ScarsRight & Convert.ToInt32(rightClockScars10.Tag)) > 0 ? 1 : 0;
                        rightClockScars11.Opacity = (selectItem.ScarsRight & Convert.ToInt32(rightClockScars11.Tag)) > 0 ? 1 : 0;
                        rightClockScars12.Opacity = (selectItem.ScarsRight & Convert.ToInt32(rightClockScars12.Tag)) > 0 ? 1 : 0;
                    }
                    else
                    {
                        leftClockScars1.Opacity = 0;
                        leftClockScars2.Opacity = 0;
                        leftClockScars3.Opacity = 0;
                        leftClockScars4.Opacity = 0;
                        leftClockScars5.Opacity = 0;
                        leftClockScars6.Opacity = 0;
                        leftClockScars7.Opacity = 0;
                        leftClockScars8.Opacity = 0;
                        leftClockScars9.Opacity = 0;
                        leftClockScars10.Opacity = 0;
                        leftClockScars11.Opacity = 0;
                        leftClockScars12.Opacity = 0;

                        rightClockScars1.Opacity = 0;
                        rightClockScars2.Opacity = 0;
                        rightClockScars3.Opacity = 0;
                        rightClockScars4.Opacity = 0;
                        rightClockScars5.Opacity = 0;
                        rightClockScars6.Opacity = 0;
                        rightClockScars7.Opacity = 0;
                        rightClockScars8.Opacity = 0;
                        rightClockScars9.Opacity = 0;
                        rightClockScars10.Opacity = 0;
                        rightClockScars11.Opacity = 0;
                        rightClockScars12.Opacity = 0;
                    }
                }
                if (e.RemovedItems.Count > 0)
                {
                    var lostItem = e.RemovedItems;
                    foreach (Person item in lostItem)
                    {
                        item.Icon = "/Images/id_card.png";
                    }
                }
            }
            catch { }
            #endregion

            //加载报表表单数据
            loadReportData();
        }               

        private void btnScreening_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Visibility = Visibility.Hidden;
                IntPtr screeningBtnHwnd = Win32Api.FindWindowEx(App.splashWinHwnd, IntPtr.Zero, null, App.strScreening);
                Win32Api.SendMessage(screeningBtnHwnd, Win32Api.WM_CLICK, 0, 0);                
                StartMouseHook();                
            }
            catch (Exception ex)
            {
                this.Visibility = Visibility.Visible;
                MessageBox.Show(this, "System Exception: " + ex.Message);
            }
        }

        private void btnDiagnostics_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Visibility = Visibility.Hidden;
                IntPtr diagnosticsBtnHwnd = Win32Api.FindWindowEx(App.splashWinHwnd, IntPtr.Zero, null, App.strDiagnostics);
                Win32Api.SendMessage(diagnosticsBtnHwnd, Win32Api.WM_CLICK, 0, 0);
                StartMouseHook();                
            }
            catch (Exception ex)
            {
                this.Visibility = Visibility.Visible;
                MessageBox.Show(this, "System Exception: " + ex.Message);                
            }
        }

        /// <summary>
        /// 统计按钮点击处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRecords_Click(object sender, RoutedEventArgs e)
        {
            RecordsWindow recordsWindow = new RecordsWindow();
            recordsWindow.Owner = this;
            recordsWindow.ShowDialog();
        }

        /// <summary>
        /// 系统设置按钮点击处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSetup_Click(object sender, RoutedEventArgs e)
        {
            PasswordPage passwordPage = new PasswordPage();
            var dialogResult=passwordPage.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value)
            {
                ReportSettingPage reportSettingPage = new ReportSettingPage();
                reportSettingPage.Owner = this;
                reportSettingPage.ShowDialog();
                reportSettingPage = null;
            }
        }

        /// <summary>
        /// 医生模式接收服务中心分配的数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReceive_Click(object sender, RoutedEventArgs e)
        {
            //判断数据传输模式
            if (App.reportSettingModel.TransferMode == TransferMode.CloudServer)
            {
                if (string.IsNullOrEmpty(App.reportSettingModel.CloudUser) || string.IsNullOrEmpty(App.reportSettingModel.CloudToken))
                {
                    LoginPage loginPage = new LoginPage();
                    var loginResult = loginPage.ShowDialog();
                    if (!loginResult.HasValue || !loginResult.Value)
                    {
                        return;
                    }
                }
            }
            else if (App.reportSettingModel.TransferMode == TransferMode.Email)
            {
                int n = 0;//成功歸檔的文件數
                //遍历指定文件夹下所有压缩档案文件
                DirectoryInfo theFolder = new DirectoryInfo(App.reportSettingModel.DataBaseFolder);             
                try
                {
                    FileInfo[] fileInfo = theFolder.GetFiles();
                    
                    //遍历文件
                    foreach (FileInfo NextFile in fileInfo)
                    {                    
                        if (".zip".Equals(NextFile.Extension, StringComparison.OrdinalIgnoreCase))
                        {
                            string pFolder = App.reportSettingModel.DataBaseFolder + System.IO.Path.DirectorySeparatorChar + "20" + NextFile.Name.Substring(0, 2)+"_" + NextFile.Name.Substring(2, 2) + System.IO.Path.DirectorySeparatorChar + NextFile.Name.Replace(".zip", "");
                            ZipTools.Instance.UnZip(NextFile.FullName, pFolder);
                            File.Delete(NextFile.FullName);
                            n++;
                        }
                    }
                }
                catch (Exception) { }
                if(n>0){
                    MessageBox.Show(this, App.Current.FindResource("Message_75").ToString());
                }
                else
                {
                    MessageBox.Show(this, string.Format(App.Current.FindResource("Message_77").ToString(),App.reportSettingModel.DataBaseFolder));
                }
                return;
            }
            else
            {
                MessageBox.Show(this, App.Current.FindResource("Message_66").ToString());
                return;
            }

            ProgressBarGridDelegate progressBarGridDelegate = new ProgressBarGridDelegate(progressBarGrid.SetValue);
            UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(uploadProgressBar.SetValue);
            //使用系统代理方式显示进度条面板
            Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { System.Windows.Controls.ProgressBar.ValueProperty, Convert.ToDouble(0) });
            Dispatcher.Invoke(progressBarGridDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { System.Windows.Controls.Grid.VisibilityProperty, Visibility.Visible });
            
            List<string> errMsg = new List<string>();
            try
            {
                int n = 0;
                int total = 0;
                List<int> jobList=new List<int>();
                string resStr = HttpWebTools.Get(App.reportSettingModel.CloudPath + "/api/getScreenDataList", App.reportSettingModel.CloudToken);
                Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { System.Windows.Controls.ProgressBar.ValueProperty, Convert.ToDouble(10) });
                var jsonObj = JObject.Parse(resStr);
                bool status = (bool)jsonObj["status"];
                if (status)
                {                    
                    var dataObj=jsonObj["data"] as JObject;
                    var jobsArr=dataObj["jobs"] as JArray;
                    //计算总文件数
                    foreach (var jobItem in jobsArr)
                    {
                        var jobObj = (JObject)jobItem;
                        var jobId = (int)jobObj["jobid"];
                        var fileArr = jobObj["files"] as JArray;
                        total += fileArr.Count;
                        jobList.Add(jobId);
                    }

                    double groupValue = 90 / (total + 1);
                    foreach (var jobItem in jobsArr)
                    {
                        var jobObj = (JObject)jobItem;
                        var jobId = (int)jobObj["jobid"];
                        var code = (string)jobObj["code"];
                        var firstname = (string)jobObj["firstname"];
                        var lastname = (string)jobObj["lastname"];
                        var othername = (string)jobObj["othername"];

                        int doneNum = 0;
                        var fileArr = jobObj["files"] as JArray;                        
                        foreach (var item in fileArr)
                        {
                            n++;
                            string patientFolder = "";
                            string downloadPath = "";
                            string fileid = "";
                            string filename = "";
                            try
                            {
                                var itemObj = (JObject)item;                                
                                fileid = (string)item["fileid"];
                                filename = (string)item["filename"];
                                //文件下载目录
                                patientFolder = App.reportSettingModel.DataBaseFolder + System.IO.Path.DirectorySeparatorChar + "20" + code.Substring(0, 2)+"_" + code.Substring(2, 2) + System.IO.Path.DirectorySeparatorChar + code + "-" + lastname;
                                if (!string.IsNullOrEmpty(firstname))
                                {
                                    patientFolder = patientFolder + "," + firstname;
                                }
                                if (!string.IsNullOrEmpty(othername))
                                {
                                    patientFolder = patientFolder + " " + othername;
                                }
                                if (!Directory.Exists(patientFolder))
                                {
                                    Directory.CreateDirectory(patientFolder);
                                }
                                //本地下载文件保存路径
                                downloadPath = patientFolder + System.IO.Path.DirectorySeparatorChar + filename;
                                Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { System.Windows.Controls.ProgressBar.ValueProperty, Convert.ToDouble(groupValue * n) });
                            }
                            catch (Exception e1)
                            {
                                //MessageBox.Show(this, e1.Message);
                                errMsg.Add(patientFolder + " :: " + App.Current.FindResource("Message_39").ToString() + e1.Message);
                                continue;
                            }
                            
                            try
                            {
                                if (!string.IsNullOrEmpty(fileid))
                                {
                                    //从服务器下载数据文件                                    
                                    NameValueCollection postDict = new NameValueCollection();
                                    postDict.Add("fileid", fileid);
                                    var resPath = HttpWebTools.DownloadFile(App.reportSettingModel.CloudPath + "/api/downloadData", patientFolder, filename, postDict, App.reportSettingModel.CloudToken, 900000);
                                    if (resPath == null)
                                    {
                                        errMsg.Add(downloadPath + " :: " + App.Current.FindResource("Message_40").ToString());
                                        continue;
                                    }
                                    else
                                    {
                                        doneNum++;
                                    }
                                }
                            }
                            catch (Exception e2)
                            {
                                errMsg.Add(downloadPath + " :: " + App.Current.FindResource("Message_40").ToString() + e2.Message);
                                continue;
                            }
                            Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { System.Windows.Controls.ProgressBar.ValueProperty, Convert.ToDouble(groupValue * n + groupValue / 2) });
                            
                        }
                        //判断是否所有数据都下载成功
                        if (doneNum == fileArr.Count)
                        {
                            jobList.Add(jobId);
                        }
                    }
                }
                else
                {
                    var info = (string)jsonObj["info"];
                    MessageBox.Show(this, info);
                    Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { System.Windows.Controls.ProgressBar.ValueProperty, Convert.ToDouble(100) });
                    Dispatcher.Invoke(progressBarGridDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { System.Windows.Controls.Grid.VisibilityProperty, Visibility.Collapsed });
                    return;
                }
                Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { System.Windows.Controls.ProgressBar.ValueProperty, Convert.ToDouble(100) });
                Dispatcher.Invoke(progressBarGridDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { System.Windows.Controls.Grid.VisibilityProperty, Visibility.Collapsed });
                if (errMsg.Count == 0)
                {
                    if (n == 0)
                    {
                        MessageBox.Show(this, App.Current.FindResource("Message_43").ToString());
                    }
                    else
                    {
                        MessageBox.Show(this, App.Current.FindResource("Message_37").ToString());
                        try
                        {
                            string jobIds = string.Join(",", jobList);
                            NameValueCollection postDict = new NameValueCollection();
                            postDict.Add("jobids", jobIds);
                            HttpWebTools.Post(App.reportSettingModel.CloudPath + "/api/closeJobs", postDict, App.reportSettingModel.CloudToken);
                        }
                        catch { }
                    }
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(App.Current.FindResource("Message_38").ToString());
                    foreach (var err in errMsg)
                    {
                        sb.Append("\r\n" + err);
                    }
                    MessageBox.Show(this, sb.ToString());
                }
                loadArchiveFolder(txtFolderPath.Text);
            }
            catch (Exception exe)
            {
                Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { System.Windows.Controls.ProgressBar.ValueProperty, Convert.ToDouble(100) });
                Dispatcher.Invoke(progressBarGridDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { System.Windows.Controls.Grid.VisibilityProperty, Visibility.Collapsed });
                MessageBox.Show(this, App.Current.FindResource("Message_42").ToString() + exe.Message);                
            }                                    
        }

        private void btnReceiveReport_Click(object sender, RoutedEventArgs e)
        {
            //判断数据传输模式
            if (App.reportSettingModel.TransferMode == TransferMode.CloudServer)
            {
                if (string.IsNullOrEmpty(App.reportSettingModel.CloudUser) || string.IsNullOrEmpty(App.reportSettingModel.CloudToken))
                {
                    LoginPage loginPage = new LoginPage();
                    var loginResult = loginPage.ShowDialog();
                    if (!loginResult.HasValue || !loginResult.Value)
                    {
                        return;
                    }
                }
            }
            else if (App.reportSettingModel.TransferMode == TransferMode.Email)
            {
                int n = 0;//成功歸檔的文件數
                //遍历指定文件夹下所有压缩档案文件
                DirectoryInfo theFolder = new DirectoryInfo(App.reportSettingModel.DataBaseFolder);
                try
                {
                    FileInfo[] fileInfo = theFolder.GetFiles();

                    //遍历文件
                    foreach (FileInfo NextFile in fileInfo)
                    {
                        if (".zip".Equals(NextFile.Extension, StringComparison.OrdinalIgnoreCase))
                        {
                            string pFolder = App.reportSettingModel.DataBaseFolder + System.IO.Path.DirectorySeparatorChar + "20" + NextFile.Name.Substring(0, 2)+"_" + NextFile.Name.Substring(2, 2) + System.IO.Path.DirectorySeparatorChar + NextFile.Name.Replace(".zip", "");
                            ZipTools.Instance.UnZip(NextFile.FullName, pFolder);
                            File.Delete(NextFile.FullName);
                            n++;
                        }
                    }
                }
                catch (Exception) { }
                if (n > 0)
                {
                    MessageBox.Show(this, App.Current.FindResource("Message_78").ToString());
                }
                else
                {
                    MessageBox.Show(this, string.Format(App.Current.FindResource("Message_77").ToString(), App.reportSettingModel.DataBaseFolder));
                }
                return;
            }
            else
            {
                MessageBox.Show(this, App.Current.FindResource("Message_66").ToString());
                return;
            }

            ProgressBarGridDelegate progressBarGridDelegate = new ProgressBarGridDelegate(progressBarGrid.SetValue);
            UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(uploadProgressBar.SetValue);
            //使用系统代理方式显示进度条面板
            Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { System.Windows.Controls.ProgressBar.ValueProperty, Convert.ToDouble(0) });
            Dispatcher.Invoke(progressBarGridDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { System.Windows.Controls.Grid.VisibilityProperty, Visibility.Visible });

            List<string> errMsg = new List<string>();
            try
            {
                int n = 0;
                int total = 0;
                List<int> jobList = new List<int>();
                string resStr = HttpWebTools.Get(App.reportSettingModel.CloudPath + "/api/getReportDataList", App.reportSettingModel.CloudToken);
                Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { System.Windows.Controls.ProgressBar.ValueProperty, Convert.ToDouble(10) });
                var jsonObj = JObject.Parse(resStr);
                bool status = (bool)jsonObj["status"];
                if (status)
                {
                    var dataObj = jsonObj["data"] as JObject;
                    var jobsArr = dataObj["jobs"] as JArray;
                    //计算总文件数
                    foreach (var jobItem in jobsArr)
                    {
                        var jobObj = (JObject)jobItem;
                        var jobId = (int)jobObj["jobid"];
                        var fileArr = jobObj["files"] as JArray;
                        total += fileArr.Count;
                        jobList.Add(jobId);
                    }

                    double groupValue = 90 / (total + 1);
                    foreach (var jobItem in jobsArr)
                    {
                        var jobObj = (JObject)jobItem;
                        var jobId = (int)jobObj["jobid"];
                        var code = (string)jobObj["code"];
                        var firstname = (string)jobObj["firstname"];
                        var lastname = (string)jobObj["lastname"];
                        var othername = (string)jobObj["othername"];

                        int doneNum = 0;
                        var fileArr = jobObj["files"] as JArray;
                        foreach (var item in fileArr)
                        {
                            n++;
                            string patientFolder = "";
                            string downloadPath = "";
                            string fileid = "";
                            string filename = "";
                            try
                            {
                                var itemObj = (JObject)item;
                                fileid = (string)item["fileid"];
                                filename = (string)item["filename"];
                                //文件下载目录
                                patientFolder = App.reportSettingModel.DataBaseFolder + System.IO.Path.DirectorySeparatorChar + "20" + code.Substring(0, 2)+"_" + code.Substring(2, 2) + System.IO.Path.DirectorySeparatorChar + code + "-" + lastname;
                                if (!string.IsNullOrEmpty(firstname))
                                {
                                    patientFolder = patientFolder + "," + firstname;
                                }
                                if (!string.IsNullOrEmpty(othername))
                                {
                                    patientFolder = patientFolder + " " + othername;
                                }
                                if (!Directory.Exists(patientFolder))
                                {
                                    Directory.CreateDirectory(patientFolder);
                                }
                                //本地下载文件保存路径
                                downloadPath = patientFolder + System.IO.Path.DirectorySeparatorChar + filename;
                                Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { System.Windows.Controls.ProgressBar.ValueProperty, Convert.ToDouble(groupValue * n) });
                            }
                            catch (Exception e1)
                            {
                                //MessageBox.Show(this, e1.Message);
                                errMsg.Add(patientFolder + " :: " + App.Current.FindResource("Message_39").ToString() + e1.Message);
                                continue;
                            }

                            try
                            {
                                if (!string.IsNullOrEmpty(fileid))
                                {
                                    //从服务器下载数据文件                                    
                                    NameValueCollection postDict = new NameValueCollection();
                                    postDict.Add("fileid", fileid);
                                    var resPath = HttpWebTools.DownloadFile(App.reportSettingModel.CloudPath + "/api/downloadData", patientFolder, filename, postDict, App.reportSettingModel.CloudToken, 900000);
                                    if (resPath == null)
                                    {
                                        errMsg.Add(downloadPath + " :: " + App.Current.FindResource("Message_40").ToString());
                                        continue;
                                    }
                                    else
                                    {
                                        doneNum++;
                                    }
                                }
                            }
                            catch (Exception e2)
                            {
                                errMsg.Add(downloadPath + " :: " + App.Current.FindResource("Message_40").ToString() + e2.Message);
                                continue;
                            }
                            Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { System.Windows.Controls.ProgressBar.ValueProperty, Convert.ToDouble(groupValue * n + groupValue / 2) });

                        }
                        //判断是否所有数据都下载成功
                        if (doneNum == fileArr.Count)
                        {
                            jobList.Add(jobId);
                        }
                    }
                }
                else
                {
                    //获取接口返回的错误信息，进度条结束
                    var info = (string)jsonObj["info"];
                    MessageBox.Show(this, info);
                    Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { System.Windows.Controls.ProgressBar.ValueProperty, Convert.ToDouble(100) });
                    Dispatcher.Invoke(progressBarGridDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { System.Windows.Controls.Grid.VisibilityProperty, Visibility.Collapsed });
                    return;
                }
                //进度条结束
                Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { System.Windows.Controls.ProgressBar.ValueProperty, Convert.ToDouble(100) });
                Dispatcher.Invoke(progressBarGridDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { System.Windows.Controls.Grid.VisibilityProperty, Visibility.Collapsed });
                if (errMsg.Count == 0)
                {
                    if (n == 0)
                    {
                        MessageBox.Show(this, App.Current.FindResource("Message_43").ToString());
                    }
                    else
                    {
                        MessageBox.Show(this, App.Current.FindResource("Message_37").ToString());
                        try
                        {
                            string jobIds = string.Join(",", jobList);
                            NameValueCollection postDict = new NameValueCollection();
                            postDict.Add("jobids", jobIds);
                            HttpWebTools.Post(App.reportSettingModel.CloudPath + "/api/closeJobs", postDict, App.reportSettingModel.CloudToken);
                        }
                        catch { }
                    }
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(App.Current.FindResource("Message_38").ToString());
                    foreach (var err in errMsg)
                    {
                        sb.Append("\r\n" + err);
                    }
                    MessageBox.Show(this, sb.ToString());
                }
                loadArchiveFolder(txtFolderPath.Text);
            }
            catch (Exception exe)
            {
                Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { System.Windows.Controls.ProgressBar.ValueProperty, Convert.ToDouble(100) });
                Dispatcher.Invoke(progressBarGridDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { System.Windows.Controls.Grid.VisibilityProperty, Visibility.Collapsed });
                MessageBox.Show(this, App.Current.FindResource("Message_45").ToString() + exe.Message);
            } 
        }

       private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedUserList = this.CodeListBox.SelectedItems;
            if (selectedUserList == null || selectedUserList.Count == 0)
            {
                MessageBox.Show(this, App.Current.FindResource("Message_35").ToString());
                return;
            }
            //确定删除？
            MessageBoxResult result = MessageBox.Show(this, App.Current.FindResource("Message_65").ToString(), "Delete selected archives", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (result == MessageBoxResult.Yes)
            {
                //循环刪除选择的每一个患者档案
                foreach (Person selectedUser in selectedUserList)
                {
                    Directory.Delete(selectedUser.ArchiveFolder,true);
                }
                loadArchiveFolder(App.dataFolder);
            }
        }

        /// <summary>
        /// 启用鼠标钩子
        /// </summary>
        public void StartMouseHook()
        {
            //启动鼠标钩子            
            mouseHook.Start();
        }
        /// <summary>
        /// 停止鼠标钩子
        /// </summary>
        public void StopMouseHook()
        {
            mouseHook.Stop();
        }

        /// <summary>
        /// 鼠标按下的钩子回调方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void mouseHook_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            try
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    IntPtr buttonHandle = Win32Api.WindowFromPoint(e.X, e.Y);
                    IntPtr winHandle = Win32Api.GetParent(buttonHandle);
                    var owner = this.Owner as MainWindow;
                    if (Win32Api.GetParent(winHandle) == owner.AppProc.MainWindowHandle)
                    {
                        StringBuilder winText = new StringBuilder(512);
                        Win32Api.GetWindowText(buttonHandle, winText, winText.Capacity);
                        if (App.strExit.Equals(winText.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            this.StopMouseHook();
                            this.WindowState = WindowState.Maximized;
                            
                        }                        
                    }
                }
            }
            catch { }
        }
        
        

        /// <summary>
        /// 选择语言
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Language_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;            
            string local = "en-US";
            if ("languageHK".Equals(button.Name))
            {
                local = "zh-HK";
                this.languageHK.Foreground = new SolidColorBrush(Color.FromRgb(0x40, 0xc4, 0xe4));
                this.languageUS.Foreground = Brushes.White;
            }
            //else if ("languageCN".Equals(button.Name))
            //{
            //    local = "zh-CN";
            //    this.languageHK.Foreground = new SolidColorBrush(Color.FromRgb(0x40, 0xc4, 0xe4));
            //    this.languageUS.Foreground = Brushes.White;
            //}
            else
            {
                this.languageUS.Foreground = new SolidColorBrush(Color.FromRgb(0x40, 0xc4, 0xe4));
                this.languageHK.Foreground = Brushes.White;
            }
            App.local = local;
            if ("zh-HK".Equals(local))
            {
                App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(@"/Resources/StringResource.zh-HK.xaml", UriKind.RelativeOrAbsolute) });
                App.Current.Resources.MergedDictionaries.Remove(new ResourceDictionary() { Source = new Uri(@"/Resources/StringResource.xaml", UriKind.RelativeOrAbsolute) });
                App.Current.Resources.MergedDictionaries.Remove(new ResourceDictionary() { Source = new Uri(@"/Resources/StringResource.zh-CN.xaml", UriKind.RelativeOrAbsolute) });

            }
            //else if ("zh-CN".Equals(local))
            //{
            //    App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(@"/Resources/StringResource.zh-CN.xaml", UriKind.RelativeOrAbsolute) });
            //    App.Current.Resources.MergedDictionaries.Remove(new ResourceDictionary() { Source = new Uri(@"/Resources/StringResource.xaml", UriKind.RelativeOrAbsolute) });
            //    App.Current.Resources.MergedDictionaries.Remove(new ResourceDictionary() { Source = new Uri(@"/Resources/StringResource.zh-HK.xaml", UriKind.RelativeOrAbsolute) });

            //}
            else
            {
                App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(@"/Resources/StringResource.xaml", UriKind.RelativeOrAbsolute) });
                App.Current.Resources.MergedDictionaries.Remove(new ResourceDictionary() { Source = new Uri(@"/Resources/StringResource.zh-CN.xaml", UriKind.RelativeOrAbsolute) });
                App.Current.Resources.MergedDictionaries.Remove(new ResourceDictionary() { Source = new Uri(@"/Resources/StringResource.zh-HK.xaml", UriKind.RelativeOrAbsolute) });

            }
            OperateIniFile.WriteIniData("Base", "Language", local, System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");

            loadReportData();
        }

        /// <summary>
        /// 点击用户资料Tab事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabClientGroup_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.tabClientGroup.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x32, 0x87, 0x87));
            this.tabReportGroup.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x76, 0x87, 0x95));
            this.tabGroup.Visibility = Visibility.Visible;
            this.reportGroup.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 点击报表Tab事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabReportGroup_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.tabClientGroup.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x76, 0x87, 0x95));
            this.tabReportGroup.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x32, 0x87, 0x87));
            this.tabGroup.Visibility = Visibility.Collapsed;
            this.reportGroup.Visibility = Visibility.Visible;
        }

        #region 用户资料编辑保存事件和方法

        /// <summary>
        /// 保存患者病历卡
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var person = this.CodeListBox.SelectedItem as Person;
                if (person == null)
                {
                    MessageBox.Show(this, App.Current.FindResource("Message_35").ToString());
                    return;
                }
                
                string personalData = "Personal data";
                string complaints = "Complaints";
                string menses =  "Menses";
                string somatic = "Somatic";
                string gynecologic = "Gynecologic";
                string obstetric = "Obstetric";
                string lactation = "Lactation";
                string diseases =  "Diseases";
                string palpation =  "Palpation";
                string ultrasound = "Ultrasound";
                string mammography = "Mammography";
                string biopsy = "Biopsy";
                             
                //Personal Data
                person.ClientNumber = this.txtClientNum.Text;
                OperateIniFile.WriteIniData(personalData, "clientnumber", this.txtClientNum.Text, person.IniFilePath);
                person.SurName = this.txtName.Text;
                OperateIniFile.WriteIniData(personalData, "surname", this.txtName.Text, person.IniFilePath);
                person.GivenName = this.txtGivenName.Text;
                OperateIniFile.WriteIniData(personalData, "given name", this.txtGivenName.Text, person.IniFilePath);
                person.OtherName = this.txtOtherName.Text;
                OperateIniFile.WriteIniData(personalData, "other name", this.txtOtherName.Text, person.IniFilePath);
                person.Address = this.txtAddress.Text;
                OperateIniFile.WriteIniData(personalData, "address", this.txtAddress.Text, person.IniFilePath);
                person.Address2 = this.txtAddress2.Text;
                OperateIniFile.WriteIniData(personalData, "address2", this.txtAddress2.Text, person.IniFilePath);
                person.City = this.txtCity.Text;
                OperateIniFile.WriteIniData(personalData, "city", this.txtCity.Text, person.IniFilePath);
                person.Province = this.txtProvince.Text;
                OperateIniFile.WriteIniData(personalData, "province", this.txtProvince.Text, person.IniFilePath);
                person.ZipCode = this.txtZipCode.Text;
                OperateIniFile.WriteIniData(personalData, "zip code", this.txtZipCode.Text, person.IniFilePath);
                person.Country = this.txtCountry.Text;
                OperateIniFile.WriteIniData(personalData, "country", this.txtCountry.Text, person.IniFilePath);

                person.Height = this.txtHeight.Text;
                OperateIniFile.WriteIniData(personalData, "height", this.txtHeight.Text, person.IniFilePath);
                person.Weight = this.txtWeight.Text;
                OperateIniFile.WriteIniData(personalData, "weight", this.txtWeight.Text, person.IniFilePath);
                person.Mobile = this.txtMobileNumber.Text;
                OperateIniFile.WriteIniData(personalData, "mobile", this.txtMobileNumber.Text, person.IniFilePath);
                person.Email = this.txtEmail.Text;
                OperateIniFile.WriteIniData(personalData, "email", this.txtEmail.Text, person.IniFilePath);               
                person.Free = this.chkFree.IsChecked.Value;
                OperateIniFile.WriteIniData(personalData, "is free", this.chkFree.IsChecked.Value ? "1" : "0", person.IniFilePath);

                var str = App.reportSettingModel.ScreenVenue;
                OperateIniFile.WriteIniData("Report", "Screen Venue", str, person.IniFilePath);

                person.BirthDate = this.txtBirthDate.Text;
                OperateIniFile.WriteIniData(personalData, "birth date", this.txtBirthDate.Text, person.IniFilePath);
                person.BirthMonth = this.txtBirthMonth.Text;
                OperateIniFile.WriteIniData(personalData, "birth month", this.txtBirthMonth.Text, person.IniFilePath);
                person.BirthYear = this.txtBirthYear.Text;
                OperateIniFile.WriteIniData(personalData, "birth year", this.txtBirthYear.Text, person.IniFilePath);
                person.RegDate = this.txtRegDate.Text;
                OperateIniFile.WriteIniData(personalData, "registration date", this.txtRegDate.Text, person.IniFilePath);
                person.RegMonth = this.txttRegMonth.Text;
                OperateIniFile.WriteIniData(personalData, "registration month", this.txttRegMonth.Text, person.IniFilePath);
                person.RegYear = this.txttRegYear.Text;
                OperateIniFile.WriteIniData(personalData, "registration year", this.txttRegYear.Text, person.IniFilePath);
                person.Remark = this.txtRemark.Text;
                string remark = this.txtRemark.Text.Replace("\r\n", ";;");
                OperateIniFile.WriteIniData(personalData, "remark", remark, person.IniFilePath);
                try
                {
                    if (!string.IsNullOrEmpty(person.BirthYear))
                    {
                        //person.BirthMonth = string.IsNullOrEmpty(person.BirthMonth) ? "1" : person.BirthMonth;
                        //person.BirthDate = string.IsNullOrEmpty(person.BirthDate) ? "1" : person.BirthDate;
                        //person.Birthday = person.BirthMonth + "/" + person.BirthDate + "/" + person.BirthYear;
                        ////person.Regdate = registrationmonth + "/" + registrationdate + "/" + registrationyear;                                
                        //if (!string.IsNullOrEmpty(person.Birthday))
                        //{
                        //    int m_Y1 = DateTime.Parse(person.Birthday).Year;
                        //    int m_Y2 = DateTime.Now.Year;
                        //    person.Age = m_Y2 - m_Y1;
                        //}

                        int m_Y1 = Convert.ToInt32(person.BirthYear);
                        int m_Y2 = DateTime.Now.Year;
                        person.Age = m_Y2 - m_Y1;
                    }

                }
                catch (Exception) { }

                //Family History
                person.FamilyBreastCancer1 = this.checkBreastCancer1.IsChecked.Value;
                OperateIniFile.WriteIniData("Family History", "FamilyBreastCancer1", this.checkBreastCancer1.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.FamilyBreastCancer2 = this.checkBreastCancer2.IsChecked.Value;
                OperateIniFile.WriteIniData("Family History", "FamilyBreastCancer2", this.checkBreastCancer2.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.FamilyBreastCancer3 = this.checkBreastCancer3.IsChecked.Value;
                OperateIniFile.WriteIniData("Family History", "FamilyBreastCancer3", this.checkBreastCancer3.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.BreastCancerDesc = this.txtBreastCancerDesc.Text;
                var breastCancerDesc = this.txtBreastCancerDesc.Text.Replace("\r\n", ";;");
                OperateIniFile.WriteIniData("Family History", "Family Breast Cancer Description", breastCancerDesc, person.IniFilePath);

                person.FamilyUterineCancer1 = this.checkUterineCancer1.IsChecked.Value;
                OperateIniFile.WriteIniData("Family History", "FamilyUterineCancer1", this.checkUterineCancer1.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.FamilyUterineCancer2 = this.checkUterineCancer2.IsChecked.Value;
                OperateIniFile.WriteIniData("Family History", "FamilyUterineCancer2", this.checkUterineCancer2.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.FamilyUterineCancer3 = this.checkUterineCancer3.IsChecked.Value;
                OperateIniFile.WriteIniData("Family History", "FamilyUterineCancer3", this.checkUterineCancer3.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.UterineCancerDesc = this.txtUterineCancerDesc.Text;
                var uterineCancerDesc = this.txtUterineCancerDesc.Text.Replace("\r\n", ";;");
                OperateIniFile.WriteIniData("Family History", "Family Uterine Cancer Description", uterineCancerDesc, person.IniFilePath);

                person.FamilyCervicalCancer1 = this.checkCervicalCancer1.IsChecked.Value;
                OperateIniFile.WriteIniData("Family History", "FamilyCervicalCancer1", this.checkCervicalCancer1.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.FamilyCervicalCancer2 = this.checkCervicalCancer2.IsChecked.Value;
                OperateIniFile.WriteIniData("Family History", "FamilyCervicalCancer2", this.checkCervicalCancer2.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.FamilyCervicalCancer3 = this.checkCervicalCancer3.IsChecked.Value;
                OperateIniFile.WriteIniData("Family History", "FamilyCervicalCancer3", this.checkCervicalCancer3.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.CervicalCancerDesc = this.txtCervicalCancerDesc.Text;
                var cervicalCancerDesc = this.txtCervicalCancerDesc.Text.Replace("\r\n", ";;");
                OperateIniFile.WriteIniData("Family History", "Family Cervical Cancer Description", cervicalCancerDesc, person.IniFilePath);

                person.FamilyOvarianCancer1 = this.checkOvarianCancer1.IsChecked.Value;
                OperateIniFile.WriteIniData("Family History", "FamilyOvarianCancer1", this.checkOvarianCancer1.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.FamilyOvarianCancer2 = this.checkOvarianCancer2.IsChecked.Value;
                OperateIniFile.WriteIniData("Family History", "FamilyOvarianCancer2", this.checkOvarianCancer2.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.FamilyOvarianCancer3 = this.checkOvarianCancer3.IsChecked.Value;
                OperateIniFile.WriteIniData("Family History", "FamilyOvarianCancer3", this.checkOvarianCancer3.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.OvarianCancerDesc = this.txtOvarianCancerDesc.Text;
                var ovarianCancerDesc = this.txtOvarianCancerDesc.Text.Replace("\r\n", ";;");
                OperateIniFile.WriteIniData("Family History", "Family Ovarian Cancer Description", ovarianCancerDesc, person.IniFilePath);

                person.FamilyCancerDesc = this.txtFamilyCancerDesc.Text;
                var familyCancerDesc = this.txtFamilyCancerDesc.Text.Replace("\r\n", ";;");
                OperateIniFile.WriteIniData("Family History", "Family Cancer Description", familyCancerDesc, person.IniFilePath);


                //Complaints
                person.PalpableLumps = this.checkPalpableLumps.IsChecked.Value;
                OperateIniFile.WriteIniData(complaints, "palpable lumps", this.checkPalpableLumps.IsChecked.Value ? "1" : "0", person.IniFilePath);
                if(this.checkPalpableLumps.IsChecked.Value){
                    person.LeftPosition = 0;
                    person.RightPosition = 0;
                    person.LeftPosition  += leftClock1.Opacity>0?Convert.ToInt32(leftClock1.Tag):0;
                    person.LeftPosition  += leftClock2.Opacity>0?Convert.ToInt32(leftClock2.Tag):0;
                    person.LeftPosition  += leftClock3.Opacity>0?Convert.ToInt32(leftClock3.Tag):0;
                    person.LeftPosition  += leftClock4.Opacity>0?Convert.ToInt32(leftClock4.Tag):0;
                    person.LeftPosition  += leftClock5.Opacity>0?Convert.ToInt32(leftClock5.Tag):0;
                    person.LeftPosition  += leftClock6.Opacity>0?Convert.ToInt32(leftClock6.Tag):0;
                    person.LeftPosition  += leftClock7.Opacity>0?Convert.ToInt32(leftClock7.Tag):0;
                    person.LeftPosition  += leftClock8.Opacity>0?Convert.ToInt32(leftClock8.Tag):0;
                    person.LeftPosition  += leftClock9.Opacity>0?Convert.ToInt32(leftClock9.Tag):0;
                    person.LeftPosition  += leftClock10.Opacity>0?Convert.ToInt32(leftClock10.Tag):0;
                    person.LeftPosition  += leftClock11.Opacity>0?Convert.ToInt32(leftClock11.Tag):0;
                    person.LeftPosition  += leftClock12.Opacity>0?Convert.ToInt32(leftClock12.Tag):0;
                    OperateIniFile.WriteIniData(complaints, "left position", person.LeftPosition.ToString(), person.IniFilePath);
                    person.RightPosition += rightClock1.Opacity > 0 ? Convert.ToInt32(rightClock1.Tag) : 0;
                    person.RightPosition += rightClock2.Opacity > 0 ? Convert.ToInt32(rightClock2.Tag) : 0;
                    person.RightPosition += rightClock3.Opacity > 0 ? Convert.ToInt32(rightClock3.Tag) : 0;
                    person.RightPosition += rightClock4.Opacity > 0 ? Convert.ToInt32(rightClock4.Tag) : 0;
                    person.RightPosition += rightClock5.Opacity > 0 ? Convert.ToInt32(rightClock5.Tag) : 0;
                    person.RightPosition += rightClock6.Opacity > 0 ? Convert.ToInt32(rightClock6.Tag) : 0;
                    person.RightPosition += rightClock7.Opacity > 0 ? Convert.ToInt32(rightClock7.Tag) : 0;
                    person.RightPosition += rightClock8.Opacity > 0 ? Convert.ToInt32(rightClock8.Tag) : 0;
                    person.RightPosition += rightClock9.Opacity > 0 ? Convert.ToInt32(rightClock9.Tag) : 0;
                    person.RightPosition += rightClock10.Opacity > 0 ? Convert.ToInt32(rightClock10.Tag) : 0;
                    person.RightPosition += rightClock11.Opacity > 0 ? Convert.ToInt32(rightClock11.Tag) : 0;
                    person.RightPosition += rightClock12.Opacity > 0 ? Convert.ToInt32(rightClock12.Tag) : 0; 
                    OperateIniFile.WriteIniData(complaints, "right position", person.RightPosition.ToString(), person.IniFilePath);
                }

                if (degree1.IsChecked.Value)
                {
                    person.Degree = Convert.ToInt32(degree1.Tag);
                }
                if (degree2.IsChecked.Value)
                {
                    person.Degree = Convert.ToInt32(degree2.Tag);
                }
                if (degree3.IsChecked.Value)
                {
                    person.Degree = Convert.ToInt32(degree3.Tag);
                }
                if (degree4.IsChecked.Value)
                {
                    person.Degree = Convert.ToInt32(degree4.Tag);
                }
                if (degree5.IsChecked.Value)
                {
                    person.Degree = Convert.ToInt32(degree5.Tag);
                }
                if (degree6.IsChecked.Value)
                {
                    person.Degree = Convert.ToInt32(degree6.Tag);
                }
                if (degree7.IsChecked.Value)
                {
                    person.Degree = Convert.ToInt32(degree7.Tag);
                }
                if (degree8.IsChecked.Value)
                {
                    person.Degree = Convert.ToInt32(degree8.Tag);
                }
                if (degree9.IsChecked.Value)
                {
                    person.Degree = Convert.ToInt32(degree9.Tag);
                }
                if (degree10.IsChecked.Value)
                {
                    person.Degree = Convert.ToInt32(degree10.Tag);
                }
                OperateIniFile.WriteIniData(complaints, "degree", person.Degree.ToString(), person.IniFilePath);

                person.Pain = this.checkPain.IsChecked.Value;
                OperateIniFile.WriteIniData(complaints, "pain", this.checkPain.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.Colostrum = this.checkColostrum.IsChecked.Value;
                OperateIniFile.WriteIniData(complaints, "colostrum", this.checkColostrum.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.SerousDischarge = this.checkSerousDischarge.IsChecked.Value;
                OperateIniFile.WriteIniData(complaints, "serous discharge", this.checkSerousDischarge.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.BloodDischarge = this.checkBloodDischarge.IsChecked.Value;
                OperateIniFile.WriteIniData(complaints, "blood discharge", this.checkBloodDischarge.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.Other = this.checkOther.IsChecked.Value;
                OperateIniFile.WriteIniData(complaints, "other", this.checkOther.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.Pregnancy = this.checkPregnancy.IsChecked.Value;
                OperateIniFile.WriteIniData(complaints, "pregnancy", this.checkPregnancy.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.Lactation = this.checkLactation.IsChecked.Value;
                OperateIniFile.WriteIniData(complaints, "lactation", this.checkLactation.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.LactationTerm = this.txtLactationTerm.Text;
                OperateIniFile.WriteIniData(complaints, "lactation term", this.txtLactationTerm.Text, person.IniFilePath);
                person.OtherDesc = this.txtOtherDesc.Text;
                var otherDesc = this.txtOtherDesc.Text.Replace("\r\n", ";;");
                OperateIniFile.WriteIniData(complaints, "other description", otherDesc, person.IniFilePath);
                person.PregnancyTerm = this.txtPregnancyTerm.Text;
                OperateIniFile.WriteIniData(complaints, "pregnancy term", this.txtPregnancyTerm.Text, person.IniFilePath);

                person.BreastImplants = this.checkBreastImplants.IsChecked.Value;
                OperateIniFile.WriteIniData(complaints, "BreastImplants", this.checkBreastImplants.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.BreastImplantsLeft = this.checkBreastImplantsLeft.IsChecked.Value;
                OperateIniFile.WriteIniData(complaints, "BreastImplantsLeft", this.checkBreastImplantsLeft.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.BreastImplantsRight = this.checkBreastImplantsRight.IsChecked.Value;
                OperateIniFile.WriteIniData(complaints, "BreastImplantsRight", this.checkBreastImplantsRight.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.MaterialsGel = this.checkMaterialsGel.IsChecked.Value;
                OperateIniFile.WriteIniData(complaints, "MaterialsGel", this.checkMaterialsGel.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.MaterialsFat = this.checkMaterialsFat.IsChecked.Value;
                OperateIniFile.WriteIniData(complaints, "MaterialsFat", this.checkMaterialsFat.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.MaterialsOthers = this.checkMaterialsOthers.IsChecked.Value;
                OperateIniFile.WriteIniData(complaints, "MaterialsOthers", this.checkMaterialsOthers.IsChecked.Value ? "1" : "0", person.IniFilePath);

                person.BreastImplantsLeftYear = this.txtBreastImplantsLeftYear.Text;
                OperateIniFile.WriteIniData(complaints, "BreastImplantsLeftYear", this.txtBreastImplantsLeftYear.Text, person.IniFilePath);
                person.BreastImplantsRightYear = this.txtBreastImplantsRightYear.Text;
                OperateIniFile.WriteIniData(complaints, "BreastImplantsRightYear", this.txtBreastImplantsRightYear.Text, person.IniFilePath);

                //Anamnesis
                person.DateLastMenstruation = this.dateLastMenstruation.Text;
                OperateIniFile.WriteIniData(menses, "DateLastMenstruation", this.dateLastMenstruation.Text, person.IniFilePath);
                person.MenstrualCycleDisorder = this.checkMenstrualCycleDisorder.IsChecked.Value;
                OperateIniFile.WriteIniData(menses, "menstrual cycle disorder", this.checkMenstrualCycleDisorder.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.Postmenopause = this.checkPostmenopause.IsChecked.Value;
                OperateIniFile.WriteIniData(menses, "postmenopause", this.checkPostmenopause.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.PostmenopauseYear = this.txtPostmenopauseYear.Text;
                OperateIniFile.WriteIniData(menses, "postmenopause year", this.txtPostmenopauseYear.Text, person.IniFilePath);

                person.HormonalContraceptives = this.checkHormonalContraceptives.IsChecked.Value;
                OperateIniFile.WriteIniData(menses, "hormonal contraceptives", this.checkHormonalContraceptives.IsChecked.Value ? "1" : "0", person.IniFilePath);

                person.MenstrualCycleDisorderDesc = this.txtMenstrualCycleDisorderDesc.Text;
                var menstrualCycleDisorderDesc = this.txtMenstrualCycleDisorderDesc.Text.Replace("\r\n", ";;");
                OperateIniFile.WriteIniData(menses, "menstrual cycle disorder description", menstrualCycleDisorderDesc, person.IniFilePath);


                person.PostmenopauseDesc = this.txtPostmenopauseDesc.Text;
                var postmenopauseDesc = this.txtPostmenopauseDesc.Text.Replace("\r\n", ";;");
                OperateIniFile.WriteIniData(menses, "postmenopause description", postmenopauseDesc, person.IniFilePath);
                person.HormonalContraceptivesBrandName = this.txtHormonalContraceptivesBrandName.Text;
                var hormonalContraceptivesBrandName = this.txtHormonalContraceptivesBrandName.Text.Replace("\r\n", ";;");
                OperateIniFile.WriteIniData(menses, "hormonal contraceptives brand name", hormonalContraceptivesBrandName, person.IniFilePath);
                person.HormonalContraceptivesPeriod = this.txtHormonalContraceptivesPeriod.Text;
                var hormonalContraceptivesPeriod = this.txtHormonalContraceptivesPeriod.Text.Replace("\r\n", ";;");
                OperateIniFile.WriteIniData(menses, "hormonal contraceptives period", hormonalContraceptivesPeriod, person.IniFilePath);

                person.Adiposity = this.checkAdiposity.IsChecked.Value;
                OperateIniFile.WriteIniData(somatic, "adiposity", this.checkAdiposity.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.EssentialHypertension = this.checkEssentialHypertension.IsChecked.Value;
                OperateIniFile.WriteIniData(somatic, "essential hypertension", this.checkEssentialHypertension.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.Diabetes = this.checkDiabetes.IsChecked.Value;
                OperateIniFile.WriteIniData(somatic, "diabetes", this.checkDiabetes.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.ThyroidGlandDiseases = this.checkThyroidGlandDiseases.IsChecked.Value;
                OperateIniFile.WriteIniData(somatic, "thyroid gland diseases", this.checkThyroidGlandDiseases.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.SomaticOther = this.checkSomaticOther.IsChecked.Value;
                OperateIniFile.WriteIniData(somatic, "other", this.checkSomaticOther.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.EssentialHypertensionDesc = this.txtEssentialHypertensionDesc.Text;
                var essentialHypertensionDesc = this.txtEssentialHypertensionDesc.Text.Replace("\r\n", ";;");
                OperateIniFile.WriteIniData(somatic, "essential hypertension description", essentialHypertensionDesc, person.IniFilePath);
                person.DiabetesDesc = this.txtDiabetesDesc.Text;
                var diabetesDesc = this.txtDiabetesDesc.Text.Replace("\r\n", ";;");
                OperateIniFile.WriteIniData(somatic, "diabetes description", diabetesDesc, person.IniFilePath);
                person.ThyroidGlandDiseasesDesc = this.txtThyroidGlandDiseasesDesc.Text;
                var thyroidGlandDiseasesDesc = this.txtThyroidGlandDiseasesDesc.Text.Replace("\r\n", ";;");
                OperateIniFile.WriteIniData(somatic, "thyroid gland diseases description", thyroidGlandDiseasesDesc, person.IniFilePath);
                person.SomaticOtherDesc = this.txtSomaticOtherDesc.Text;
                var somaticOtherDesc = this.txtSomaticOtherDesc.Text.Replace("\r\n", ";;");
                OperateIniFile.WriteIniData(somatic, "other description", somaticOtherDesc, person.IniFilePath);


                person.Infertility = this.checkInfertility.IsChecked.Value;
                OperateIniFile.WriteIniData(gynecologic, "infertility", this.checkInfertility.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.OvaryDiseases = this.checkOvaryDiseases.IsChecked.Value;
                OperateIniFile.WriteIniData(gynecologic, "ovary diseases", this.checkOvaryDiseases.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.OvaryCyst = this.checkOvaryCyst.IsChecked.Value;
                OperateIniFile.WriteIniData(gynecologic, "ovary cyst", this.checkOvaryCyst.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.OvaryCancer = this.checkOvaryCancer.IsChecked.Value;
                OperateIniFile.WriteIniData(gynecologic, "ovary cancer", this.checkOvaryCancer.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.OvaryEndometriosis = this.checkOvaryEndometriosis.IsChecked.Value;
                OperateIniFile.WriteIniData(gynecologic, "ovary endometriosis", this.checkOvaryEndometriosis.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.OvaryOther = this.checkOvaryOther.IsChecked.Value;
                OperateIniFile.WriteIniData(gynecologic, "ovary other", this.checkOvaryOther.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.UterusDiseases = this.checkUterusDiseases.IsChecked.Value;
                OperateIniFile.WriteIniData(gynecologic, "uterus diseases", this.checkUterusDiseases.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.UterusMyoma = this.checkUterusMyoma.IsChecked.Value;
                OperateIniFile.WriteIniData(gynecologic, "uterus myoma", this.checkUterusMyoma.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.UterusCancer = this.checkUterusCancer.IsChecked.Value;
                OperateIniFile.WriteIniData(gynecologic, "uterus cancer", this.checkUterusCancer.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.UterusEndometriosis = this.checkUterusEndometriosis.IsChecked.Value;
                OperateIniFile.WriteIniData(gynecologic, "uterus endometriosis", this.checkUterusEndometriosis.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.UterusOther = this.checkUterusOther.IsChecked.Value;
                OperateIniFile.WriteIniData(gynecologic, "uterus other", this.checkUterusOther.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.GynecologicOther = this.checkGynecologicOther.IsChecked.Value;
                OperateIniFile.WriteIniData(gynecologic, "other", this.checkGynecologicOther.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.InfertilityDesc = this.txtInfertility.Text;
                var infertilityDesc = this.txtInfertility.Text.Replace("\r\n", ";;");
                OperateIniFile.WriteIniData(gynecologic, "infertility-description", infertilityDesc, person.IniFilePath);
                person.OvaryOtherDesc = this.txtOvaryOtherDesc.Text;
                var ovaryOtherDesc = this.txtOvaryOtherDesc.Text.Replace("\r\n", ";;");
                OperateIniFile.WriteIniData(gynecologic, "ovary other description", ovaryOtherDesc, person.IniFilePath);
                person.UterusOtherDesc = this.txtUterusOtherDesc.Text;
                var uterusOtherDesc = this.txtUterusOtherDesc.Text.Replace("\r\n", ";;");
                OperateIniFile.WriteIniData(gynecologic, "uterus other description", uterusOtherDesc, person.IniFilePath);
                person.GynecologicOtherDesc = this.txtGynecologicOtherDesc.Text;
                var gynecologicOtherDesc = this.txtGynecologicOtherDesc.Text.Replace("\r\n", ";;");
                OperateIniFile.WriteIniData(gynecologic, "other description", gynecologicOtherDesc, person.IniFilePath);


                person.Abortions = this.checkAbortions.IsChecked.Value;
                OperateIniFile.WriteIniData(obstetric, "abortions", this.checkAbortions.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.Births = this.checkBirths.IsChecked.Value;
                OperateIniFile.WriteIniData(obstetric, "births", this.checkBirths.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.AbortionsNumber = this.txtAbortionsNumber.Text;
                OperateIniFile.WriteIniData(obstetric, "abortions number", this.txtAbortionsNumber.Text, person.IniFilePath);
                person.BirthsNumber = this.txtBirthsNumber.Text;
                OperateIniFile.WriteIniData(obstetric, "births number", this.txtBirthsNumber.Text, person.IniFilePath);
                person.ObstetricDesc = this.txtObstetricDesc.Text;
                var obstetricDesc = this.txtObstetricDesc.Text.Replace("\r\n", ";;"); ;
                OperateIniFile.WriteIniData(obstetric, "obstetric description", obstetricDesc, person.IniFilePath);

                person.LactationTill1Month = this.checkLactationTill1Month.IsChecked.Value;
                OperateIniFile.WriteIniData(lactation, "lactation till 1 month", this.checkLactationTill1Month.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.LactationTill1Year = this.checkLactationTill1Year.IsChecked.Value;
                OperateIniFile.WriteIniData(lactation, "lactation till 1 year", this.checkLactationTill1Year.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.LactationOver1Year = this.checkLactationOver1Year.IsChecked.Value;
                OperateIniFile.WriteIniData(lactation, "lactation over 1 year", this.checkLactationOver1Year.IsChecked.Value ? "1" : "0", person.IniFilePath);


                person.Trauma = this.checkTrauma.IsChecked.Value;
                OperateIniFile.WriteIniData(diseases, "trauma", this.checkTrauma.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.Mastitis = this.checkMastitis.IsChecked.Value;
                OperateIniFile.WriteIniData(diseases, "mastitis", this.checkMastitis.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.FibrousCysticMastopathy = this.checkFibrousCysticMastopathy.IsChecked.Value;
                OperateIniFile.WriteIniData(diseases, "fibrous- cystic mastopathy", this.checkFibrousCysticMastopathy.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.Cysts = this.checkCysts.IsChecked.Value;
                OperateIniFile.WriteIniData(diseases, "cysts", this.checkCysts.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.Cancer = this.checkCancer.IsChecked.Value;
                OperateIniFile.WriteIniData(diseases, "cancer", this.checkCancer.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.DiseasesOther = this.checkDiseasesOther.IsChecked.Value;
                OperateIniFile.WriteIniData(diseases, "other", this.checkDiseasesOther.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.TraumaDesc = this.txtTraumaDesc.Text;
                var traumaDesc = this.txtTraumaDesc.Text.Replace("\r\n", ";;");
                OperateIniFile.WriteIniData(diseases, "trauma description", traumaDesc, person.IniFilePath);
                person.MastitisDesc = this.txtMastitisDesc.Text;
                var mastitisDesc = this.txtMastitisDesc.Text.Replace("\r\n", ";;");
                OperateIniFile.WriteIniData(diseases, "mastitis description", mastitisDesc, person.IniFilePath);
                person.FibrousCysticMastopathyDesc = this.txtFibrousCysticMastopathyDesc.Text;
                var fibrousCysticMastopathyDesc = this.txtFibrousCysticMastopathyDesc.Text.Replace("\r\n", ";;");
                OperateIniFile.WriteIniData(diseases, "fibrous- cystic mastopathy description", fibrousCysticMastopathyDesc, person.IniFilePath);
                person.CystsDesc = this.txtCystsDesc.Text;
                var cystsDesc = this.txtCystsDesc.Text.Replace("\r\n", ";;");
                OperateIniFile.WriteIniData(diseases, "cysts descriptuin", cystsDesc, person.IniFilePath);
                person.CancerDesc = this.txtCancerDesc.Text;
                var cancerDesc = this.txtCancerDesc.Text.Replace("\r\n", ";;");
                OperateIniFile.WriteIniData(diseases, "cancer description", cancerDesc, person.IniFilePath);
                person.DiseasesOtherDesc = this.txtDiseasesOtherDesc.Text;
                var diseasesOtherDesc = this.txtDiseasesOtherDesc.Text.Replace("\r\n", ";;");
                OperateIniFile.WriteIniData(diseases, "other description", diseasesOtherDesc, person.IniFilePath);

                person.Palpation = this.chkPalpation.IsChecked.Value;
                OperateIniFile.WriteIniData(palpation, "palpation", this.chkPalpation.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.PalationYear = this.txtPalationYear.Text;
                OperateIniFile.WriteIniData(palpation, "palpation year", this.txtPalationYear.Text, person.IniFilePath);
                person.PalpationDiffuse = this.checkPalpationDiffuse.IsChecked.Value;
                OperateIniFile.WriteIniData(palpation, "diffuse", this.checkPalpationDiffuse.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.PalpationFocal = this.checkPalpationFocal.IsChecked.Value;
                OperateIniFile.WriteIniData(palpation, "focal", this.checkPalpationFocal.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.PalpationDesc = this.txtPalpationDesc.Text;
                var palpationDesc = this.txtPalpationDesc.Text.Replace("\r\n", ";;");
                OperateIniFile.WriteIniData(palpation, "palpation normal", this.radPalpationStatusNormal.IsChecked.Value ? "1" : "0", person.IniFilePath);
                OperateIniFile.WriteIniData(palpation, "palpation status", this.radPalpationStatusAbnormal.IsChecked.Value ? "1" : "0", person.IniFilePath);                

                person.Ultrasound = this.chkUltrasound.IsChecked.Value;
                OperateIniFile.WriteIniData(ultrasound, "ultrasound", this.chkUltrasound.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.UltrasoundYear = this.txtUltrasoundYear.Text;
                OperateIniFile.WriteIniData(ultrasound, "ultrasound year", this.txtUltrasoundYear.Text, person.IniFilePath);
                person.UltrasoundDiffuse = this.checkUltrasoundDiffuse.IsChecked.Value;
                OperateIniFile.WriteIniData(ultrasound, "diffuse", this.checkUltrasoundDiffuse.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.UltrasoundFocal = this.checkUltrasoundFocal.IsChecked.Value;
                OperateIniFile.WriteIniData(ultrasound, "focal", this.checkUltrasoundFocal.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.UltrasoundnDesc = this.txtUltrasoundnDesc.Text;
                var ultrasoundnDesc = this.txtUltrasoundnDesc.Text.Replace("\r\n", ";;");
                OperateIniFile.WriteIniData(ultrasound, "description", ultrasoundnDesc, person.IniFilePath);
                OperateIniFile.WriteIniData(ultrasound, "ultrasound normal", this.radUltrasoundStatusNormal.IsChecked.Value ? "1" : "0", person.IniFilePath);
                OperateIniFile.WriteIniData(ultrasound, "ultrasound status", this.radUltrasoundStatusAbnormal.IsChecked.Value ? "1" : "0", person.IniFilePath);
                

                person.Mammography = this.chkMammography.IsChecked.Value;
                OperateIniFile.WriteIniData(mammography, "mammography", this.chkMammography.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.MammographyYear = this.txtMammographyYear.Text;
                OperateIniFile.WriteIniData(mammography, "mammography year", this.txtMammographyYear.Text, person.IniFilePath);
                person.MammographyDiffuse = this.checkMammographyDiffuse.IsChecked.Value;
                OperateIniFile.WriteIniData(mammography, "diffuse", this.checkMammographyDiffuse.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.MammographyFocal = this.checkMammographyFocal.IsChecked.Value;
                OperateIniFile.WriteIniData(mammography, "focal", this.checkMammographyFocal.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.MammographyDesc = this.txtMammographyDesc.Text;
                var mammographyDesc = this.txtMammographyDesc.Text.Replace("\r\n", ";;");
                OperateIniFile.WriteIniData(mammography, "description", mammographyDesc, person.IniFilePath);
                OperateIniFile.WriteIniData(mammography, "mammography normal", this.radMammographyStatusNormal.IsChecked.Value ? "1" : "0", person.IniFilePath);
                OperateIniFile.WriteIniData(mammography, "mammography status", this.radMammographyStatusAbnormal.IsChecked.Value ? "1" : "0", person.IniFilePath);

                person.Biopsy = this.chkBiopsy.IsChecked.Value;
                OperateIniFile.WriteIniData(biopsy, "biopsy", this.chkBiopsy.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.BiopsyYear = this.txtBiopsyYear.Text;
                OperateIniFile.WriteIniData(biopsy, "biopsy year", this.txtBiopsyYear.Text, person.IniFilePath);
                person.BiopsyDiffuse = this.checkBiopsyDiffuse.IsChecked.Value;
                OperateIniFile.WriteIniData(biopsy, "diffuse", this.checkBiopsyDiffuse.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.BiopsyFocal = this.checkBiopsyFocal.IsChecked.Value;
                OperateIniFile.WriteIniData(biopsy, "focal", this.checkBiopsyFocal.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.BiopsyCancer = this.checkBiopsyCancer.IsChecked.Value;
                OperateIniFile.WriteIniData(biopsy, "cancer", this.checkBiopsyCancer.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.BiopsyProliferation = this.checkBiopsyProliferation.IsChecked.Value;
                OperateIniFile.WriteIniData(biopsy, "proliferation", this.checkBiopsyProliferation.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.BiopsyDysplasia = this.checkBiopsyDysplasia.IsChecked.Value;
                OperateIniFile.WriteIniData(biopsy, "dysplasia", this.checkBiopsyDysplasia.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.BiopsyIntraductalPapilloma = this.checkBiopsyIntraductalPapilloma.IsChecked.Value;
                OperateIniFile.WriteIniData(biopsy, "intraductal papilloma", this.checkBiopsyIntraductalPapilloma.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.BiopsyFibroadenoma = this.checkBiopsyFibroadenoma.IsChecked.Value;
                OperateIniFile.WriteIniData(biopsy, "fibroadenoma", this.checkBiopsyFibroadenoma.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.BiopsyOther = this.checkBiopsyOther.IsChecked.Value;
                OperateIniFile.WriteIniData(biopsy, "other", this.checkBiopsyOther.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.BiopsyOtherDesc = this.txtBiopsyOtherDesc.Text;
                var biopsyOtherDesc = this.txtBiopsyOtherDesc.Text.Replace("\r\n", ";;");
                OperateIniFile.WriteIniData(biopsy, "other description", biopsyOtherDesc, person.IniFilePath);
                OperateIniFile.WriteIniData(biopsy, "biopsy normal", this.radBiopsyStatusNormal.IsChecked.Value ? "1" : "0", person.IniFilePath);
                OperateIniFile.WriteIniData(biopsy, "biopsy status", this.radBiopsyStatusAbnormal.IsChecked.Value ? "1" : "0", person.IniFilePath);

                person.MeikScreening = this.chkMEIKScreen.IsChecked.Value;
                OperateIniFile.WriteIniData(biopsy, "meik screen", this.chkMEIKScreen.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.MeikScreenYear = this.txtMEIKYear.Text;
                OperateIniFile.WriteIniData(biopsy, "meik screen year", this.txtMEIKYear.Text, person.IniFilePath);
                person.MeikPoint = this.txtMEIKPoint.Text;
                OperateIniFile.WriteIniData(biopsy, "meik screen point", this.txtMEIKPoint.Text, person.IniFilePath);
                person.MeikScreenDesc = this.txtMEIKScreenDesc.Text;
                OperateIniFile.WriteIniData(biopsy, "meik screen description", this.txtMEIKScreenDesc.Text, person.IniFilePath);



                person.ExaminationsOtherDesc = this.txtExaminationsDesc.Text;
                var examinationsOtherDesc = this.txtExaminationsDesc.Text.Replace("\r\n", ";;");
                OperateIniFile.WriteIniData(biopsy, "examinations description", examinationsOtherDesc, person.IniFilePath);


                person.RedSwollen = this.chkRedSwollen.IsChecked.Value;
                OperateIniFile.WriteIniData("Visual", "red swollen", this.chkRedSwollen.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.Palpable = this.chkPalpable.IsChecked.Value;
                OperateIniFile.WriteIniData("Visual", "palpable", this.chkPalpable.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.Serous = this.chkSerous.IsChecked.Value;
                OperateIniFile.WriteIniData("Visual", "serous discharge", this.chkSerous.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.Wounds = this.chkWounds.IsChecked.Value;
                OperateIniFile.WriteIniData("Visual", "wounds", this.chkWounds.IsChecked.Value ? "1" : "0", person.IniFilePath);
                person.Scars = this.chkScars.IsChecked.Value;
                OperateIniFile.WriteIniData("Visual", "scars", this.chkScars.IsChecked.Value ? "1" : "0", person.IniFilePath);
                //person.RedSwollenLeft = this.redSwollenLeft.SelectedIndex;
                //OperateIniFile.WriteIniData("Visual", "red swollen left segment", this.redSwollenLeft.SelectedIndex.ToString(), person.IniFilePath);
                //person.RedSwollenRight = this.redSwollenRight.SelectedIndex;
                //OperateIniFile.WriteIniData("Visual", "red swollen right segment", this.redSwollenRight.SelectedIndex.ToString(), person.IniFilePath);
                if (this.chkRedSwollen.IsChecked.Value)
                {
                    person.RedSwollenLeft = 0;
                    person.RedSwollenRight = 0;
                    person.RedSwollenLeft += leftClockRedSwollen1.Opacity > 0 ? Convert.ToInt32(leftClockRedSwollen1.Tag) : 0;
                    person.RedSwollenLeft += leftClockRedSwollen2.Opacity > 0 ? Convert.ToInt32(leftClockRedSwollen2.Tag) : 0;
                    person.RedSwollenLeft += leftClockRedSwollen3.Opacity > 0 ? Convert.ToInt32(leftClockRedSwollen3.Tag) : 0;
                    person.RedSwollenLeft += leftClockRedSwollen4.Opacity > 0 ? Convert.ToInt32(leftClockRedSwollen4.Tag) : 0;
                    person.RedSwollenLeft += leftClockRedSwollen5.Opacity > 0 ? Convert.ToInt32(leftClockRedSwollen5.Tag) : 0;
                    person.RedSwollenLeft += leftClockRedSwollen6.Opacity > 0 ? Convert.ToInt32(leftClockRedSwollen6.Tag) : 0;
                    person.RedSwollenLeft += leftClockRedSwollen7.Opacity > 0 ? Convert.ToInt32(leftClockRedSwollen7.Tag) : 0;
                    person.RedSwollenLeft += leftClockRedSwollen8.Opacity > 0 ? Convert.ToInt32(leftClockRedSwollen8.Tag) : 0;
                    person.RedSwollenLeft += leftClockRedSwollen9.Opacity > 0 ? Convert.ToInt32(leftClockRedSwollen9.Tag) : 0;
                    person.RedSwollenLeft += leftClockRedSwollen10.Opacity > 0 ? Convert.ToInt32(leftClockRedSwollen10.Tag) : 0;
                    person.RedSwollenLeft += leftClockRedSwollen11.Opacity > 0 ? Convert.ToInt32(leftClockRedSwollen11.Tag) : 0;
                    person.RedSwollenLeft += leftClockRedSwollen12.Opacity > 0 ? Convert.ToInt32(leftClockRedSwollen12.Tag) : 0;
                    OperateIniFile.WriteIniData("Visual", "red swollen left segment", person.RedSwollenLeft.ToString(), person.IniFilePath);
                    person.RedSwollenRight += rightClockRedSwollen1.Opacity > 0 ? Convert.ToInt32(rightClockRedSwollen1.Tag) : 0;
                    person.RedSwollenRight += rightClockRedSwollen2.Opacity > 0 ? Convert.ToInt32(rightClockRedSwollen2.Tag) : 0;
                    person.RedSwollenRight += rightClockRedSwollen3.Opacity > 0 ? Convert.ToInt32(rightClockRedSwollen3.Tag) : 0;
                    person.RedSwollenRight += rightClockRedSwollen4.Opacity > 0 ? Convert.ToInt32(rightClockRedSwollen4.Tag) : 0;
                    person.RedSwollenRight += rightClockRedSwollen5.Opacity > 0 ? Convert.ToInt32(rightClockRedSwollen5.Tag) : 0;
                    person.RedSwollenRight += rightClockRedSwollen6.Opacity > 0 ? Convert.ToInt32(rightClockRedSwollen6.Tag) : 0;
                    person.RedSwollenRight += rightClockRedSwollen7.Opacity > 0 ? Convert.ToInt32(rightClockRedSwollen7.Tag) : 0;
                    person.RedSwollenRight += rightClockRedSwollen8.Opacity > 0 ? Convert.ToInt32(rightClockRedSwollen8.Tag) : 0;
                    person.RedSwollenRight += rightClockRedSwollen9.Opacity > 0 ? Convert.ToInt32(rightClockRedSwollen9.Tag) : 0;
                    person.RedSwollenRight += rightClockRedSwollen10.Opacity > 0 ? Convert.ToInt32(rightClockRedSwollen10.Tag) : 0;
                    person.RedSwollenRight += rightClockRedSwollen11.Opacity > 0 ? Convert.ToInt32(rightClockRedSwollen11.Tag) : 0;
                    person.RedSwollenRight += rightClockRedSwollen12.Opacity > 0 ? Convert.ToInt32(rightClockRedSwollen12.Tag) : 0;
                    OperateIniFile.WriteIniData("Visual", "red swollen right segment", person.RedSwollenRight.ToString(), person.IniFilePath);

                    person.RedSwollenDesc = this.txtRedSwollenDesc.Text;
                    var redSwollenDesc = this.txtRedSwollenDesc.Text.Replace("\r\n", ";;");
                    OperateIniFile.WriteIniData("Visual", "RedSwollen description", redSwollenDesc, person.IniFilePath);
                }

                //person.PalpableLeft = this.palpableLeft.SelectedIndex;
                //OperateIniFile.WriteIniData("Visual", "palpable left segment", this.palpableLeft.SelectedIndex.ToString(), person.IniFilePath);
                //person.PalpableRight = this.palpableRight.SelectedIndex;
                //OperateIniFile.WriteIniData("Visual", "palpable right segment", this.palpableRight.SelectedIndex.ToString(), person.IniFilePath);
                if (this.chkPalpable.IsChecked.Value)
                {
                    person.PalpableLeft = 0;
                    person.PalpableRight = 0;
                    person.PalpableLeft += leftClockPalpable1.Opacity > 0 ? Convert.ToInt32(leftClockPalpable1.Tag) : 0;
                    person.PalpableLeft += leftClockPalpable2.Opacity > 0 ? Convert.ToInt32(leftClockPalpable2.Tag) : 0;
                    person.PalpableLeft += leftClockPalpable3.Opacity > 0 ? Convert.ToInt32(leftClockPalpable3.Tag) : 0;
                    person.PalpableLeft += leftClockPalpable4.Opacity > 0 ? Convert.ToInt32(leftClockPalpable4.Tag) : 0;
                    person.PalpableLeft += leftClockPalpable5.Opacity > 0 ? Convert.ToInt32(leftClockPalpable5.Tag) : 0;
                    person.PalpableLeft += leftClockPalpable6.Opacity > 0 ? Convert.ToInt32(leftClockPalpable6.Tag) : 0;
                    person.PalpableLeft += leftClockPalpable7.Opacity > 0 ? Convert.ToInt32(leftClockPalpable7.Tag) : 0;
                    person.PalpableLeft += leftClockPalpable8.Opacity > 0 ? Convert.ToInt32(leftClockPalpable8.Tag) : 0;
                    person.PalpableLeft += leftClockPalpable9.Opacity > 0 ? Convert.ToInt32(leftClockPalpable9.Tag) : 0;
                    person.PalpableLeft += leftClockPalpable10.Opacity > 0 ? Convert.ToInt32(leftClockPalpable10.Tag) : 0;
                    person.PalpableLeft += leftClockPalpable11.Opacity > 0 ? Convert.ToInt32(leftClockPalpable11.Tag) : 0;
                    person.PalpableLeft += leftClockPalpable12.Opacity > 0 ? Convert.ToInt32(leftClockPalpable12.Tag) : 0;
                    OperateIniFile.WriteIniData("Visual", "palpable left segment", person.PalpableLeft.ToString(), person.IniFilePath);
                    person.PalpableRight += rightClockPalpable1.Opacity > 0 ? Convert.ToInt32(rightClockPalpable1.Tag) : 0;
                    person.PalpableRight += rightClockPalpable2.Opacity > 0 ? Convert.ToInt32(rightClockPalpable2.Tag) : 0;
                    person.PalpableRight += rightClockPalpable3.Opacity > 0 ? Convert.ToInt32(rightClockPalpable3.Tag) : 0;
                    person.PalpableRight += rightClockPalpable4.Opacity > 0 ? Convert.ToInt32(rightClockPalpable4.Tag) : 0;
                    person.PalpableRight += rightClockPalpable5.Opacity > 0 ? Convert.ToInt32(rightClockPalpable5.Tag) : 0;
                    person.PalpableRight += rightClockPalpable6.Opacity > 0 ? Convert.ToInt32(rightClockPalpable6.Tag) : 0;
                    person.PalpableRight += rightClockPalpable7.Opacity > 0 ? Convert.ToInt32(rightClockPalpable7.Tag) : 0;
                    person.PalpableRight += rightClockPalpable8.Opacity > 0 ? Convert.ToInt32(rightClockPalpable8.Tag) : 0;
                    person.PalpableRight += rightClockPalpable9.Opacity > 0 ? Convert.ToInt32(rightClockPalpable9.Tag) : 0;
                    person.PalpableRight += rightClockPalpable10.Opacity > 0 ? Convert.ToInt32(rightClockPalpable10.Tag) : 0;
                    person.PalpableRight += rightClockPalpable11.Opacity > 0 ? Convert.ToInt32(rightClockPalpable11.Tag) : 0;
                    person.PalpableRight += rightClockPalpable12.Opacity > 0 ? Convert.ToInt32(rightClockPalpable12.Tag) : 0;
                    OperateIniFile.WriteIniData("Visual", "palpable right segment", person.PalpableRight.ToString(), person.IniFilePath);
                    person.PalpableDesc = this.txtPalpableDesc.Text;
                    var palpableDesc = this.txtPalpableDesc.Text.Replace("\r\n", ";;");
                    OperateIniFile.WriteIniData("Visual", "Palpable description", palpableDesc, person.IniFilePath);
                }


                //person.SerousLeft = this.serousLeft.SelectedIndex;
                //OperateIniFile.WriteIniData("Visual", "serous left segment", this.serousLeft.SelectedIndex.ToString(), person.IniFilePath);
                //person.SerousRight = this.serousRight.SelectedIndex;
                //OperateIniFile.WriteIniData("Visual", "serous right segment", this.serousRight.SelectedIndex.ToString(), person.IniFilePath);
                if (this.chkSerous.IsChecked.Value)
                {
                    person.SerousLeft = 0;
                    person.SerousRight = 0;
                    person.SerousLeft += leftClockSerious1.Opacity > 0 ? Convert.ToInt32(leftClockSerious1.Tag) : 0;
                    person.SerousLeft += leftClockSerious2.Opacity > 0 ? Convert.ToInt32(leftClockSerious2.Tag) : 0;
                    person.SerousLeft += leftClockSerious3.Opacity > 0 ? Convert.ToInt32(leftClockSerious3.Tag) : 0;
                    person.SerousLeft += leftClockSerious4.Opacity > 0 ? Convert.ToInt32(leftClockSerious4.Tag) : 0;
                    person.SerousLeft += leftClockSerious5.Opacity > 0 ? Convert.ToInt32(leftClockSerious5.Tag) : 0;
                    person.SerousLeft += leftClockSerious6.Opacity > 0 ? Convert.ToInt32(leftClockSerious6.Tag) : 0;
                    person.SerousLeft += leftClockSerious7.Opacity > 0 ? Convert.ToInt32(leftClockSerious7.Tag) : 0;
                    person.SerousLeft += leftClockSerious8.Opacity > 0 ? Convert.ToInt32(leftClockSerious8.Tag) : 0;
                    person.SerousLeft += leftClockSerious9.Opacity > 0 ? Convert.ToInt32(leftClockSerious9.Tag) : 0;
                    person.SerousLeft += leftClockSerious10.Opacity > 0 ? Convert.ToInt32(leftClockSerious10.Tag) : 0;
                    person.SerousLeft += leftClockSerious11.Opacity > 0 ? Convert.ToInt32(leftClockSerious11.Tag) : 0;
                    person.SerousLeft += leftClockSerious12.Opacity > 0 ? Convert.ToInt32(leftClockSerious12.Tag) : 0;
                    OperateIniFile.WriteIniData("Visual", "serous left segment", person.SerousLeft.ToString(), person.IniFilePath);
                    person.SerousRight += rightClockSerious1.Opacity > 0 ? Convert.ToInt32(rightClockSerious1.Tag) : 0;
                    person.SerousRight += rightClockSerious2.Opacity > 0 ? Convert.ToInt32(rightClockSerious2.Tag) : 0;
                    person.SerousRight += rightClockSerious3.Opacity > 0 ? Convert.ToInt32(rightClockSerious3.Tag) : 0;
                    person.SerousRight += rightClockSerious4.Opacity > 0 ? Convert.ToInt32(rightClockSerious4.Tag) : 0;
                    person.SerousRight += rightClockSerious5.Opacity > 0 ? Convert.ToInt32(rightClockSerious5.Tag) : 0;
                    person.SerousRight += rightClockSerious6.Opacity > 0 ? Convert.ToInt32(rightClockSerious6.Tag) : 0;
                    person.SerousRight += rightClockSerious7.Opacity > 0 ? Convert.ToInt32(rightClockSerious7.Tag) : 0;
                    person.SerousRight += rightClockSerious8.Opacity > 0 ? Convert.ToInt32(rightClockSerious8.Tag) : 0;
                    person.SerousRight += rightClockSerious9.Opacity > 0 ? Convert.ToInt32(rightClockSerious9.Tag) : 0;
                    person.SerousRight += rightClockSerious10.Opacity > 0 ? Convert.ToInt32(rightClockSerious10.Tag) : 0;
                    person.SerousRight += rightClockSerious11.Opacity > 0 ? Convert.ToInt32(rightClockSerious11.Tag) : 0;
                    person.SerousRight += rightClockSerious12.Opacity > 0 ? Convert.ToInt32(rightClockSerious12.Tag) : 0;
                    OperateIniFile.WriteIniData("Visual", "serous right segment", person.SerousRight.ToString(), person.IniFilePath);
                    person.SeriousDesc = this.txtSeriousDesc.Text;
                    var seriousDesc = this.txtSeriousDesc.Text.Replace("\r\n", ";;");
                    OperateIniFile.WriteIniData("Visual", "Serious description", seriousDesc, person.IniFilePath);
                }

                //person.WoundsLeft = this.woundsLeft.SelectedIndex;
                //OperateIniFile.WriteIniData("Visual", "wounds left segment", this.woundsLeft.SelectedIndex.ToString(), person.IniFilePath);
                //person.WoundsRight = this.woundsRight.SelectedIndex;
                //OperateIniFile.WriteIniData("Visual", "wounds right segment", this.woundsRight.SelectedIndex.ToString(), person.IniFilePath);
                if (this.chkWounds.IsChecked.Value)
                {
                    person.WoundsLeft = 0;
                    person.WoundsRight = 0;
                    person.WoundsLeft += leftClockWounds1.Opacity > 0 ? Convert.ToInt32(leftClockWounds1.Tag) : 0;
                    person.WoundsLeft += leftClockWounds2.Opacity > 0 ? Convert.ToInt32(leftClockWounds2.Tag) : 0;
                    person.WoundsLeft += leftClockWounds3.Opacity > 0 ? Convert.ToInt32(leftClockWounds3.Tag) : 0;
                    person.WoundsLeft += leftClockWounds4.Opacity > 0 ? Convert.ToInt32(leftClockWounds4.Tag) : 0;
                    person.WoundsLeft += leftClockWounds5.Opacity > 0 ? Convert.ToInt32(leftClockWounds5.Tag) : 0;
                    person.WoundsLeft += leftClockWounds6.Opacity > 0 ? Convert.ToInt32(leftClockWounds6.Tag) : 0;
                    person.WoundsLeft += leftClockWounds7.Opacity > 0 ? Convert.ToInt32(leftClockWounds7.Tag) : 0;
                    person.WoundsLeft += leftClockWounds8.Opacity > 0 ? Convert.ToInt32(leftClockWounds8.Tag) : 0;
                    person.WoundsLeft += leftClockWounds9.Opacity > 0 ? Convert.ToInt32(leftClockWounds9.Tag) : 0;
                    person.WoundsLeft += leftClockWounds10.Opacity > 0 ? Convert.ToInt32(leftClockWounds10.Tag) : 0;
                    person.WoundsLeft += leftClockWounds11.Opacity > 0 ? Convert.ToInt32(leftClockWounds11.Tag) : 0;
                    person.WoundsLeft += leftClockWounds12.Opacity > 0 ? Convert.ToInt32(leftClockWounds12.Tag) : 0;
                    OperateIniFile.WriteIniData("Visual", "wounds left segment", person.WoundsLeft.ToString(), person.IniFilePath);
                    person.WoundsRight += rightClockWounds1.Opacity > 0 ? Convert.ToInt32(rightClockWounds1.Tag) : 0;
                    person.WoundsRight += rightClockWounds2.Opacity > 0 ? Convert.ToInt32(rightClockWounds2.Tag) : 0;
                    person.WoundsRight += rightClockWounds3.Opacity > 0 ? Convert.ToInt32(rightClockWounds3.Tag) : 0;
                    person.WoundsRight += rightClockWounds4.Opacity > 0 ? Convert.ToInt32(rightClockWounds4.Tag) : 0;
                    person.WoundsRight += rightClockWounds5.Opacity > 0 ? Convert.ToInt32(rightClockWounds5.Tag) : 0;
                    person.WoundsRight += rightClockWounds6.Opacity > 0 ? Convert.ToInt32(rightClockWounds6.Tag) : 0;
                    person.WoundsRight += rightClockWounds7.Opacity > 0 ? Convert.ToInt32(rightClockWounds7.Tag) : 0;
                    person.WoundsRight += rightClockWounds8.Opacity > 0 ? Convert.ToInt32(rightClockWounds8.Tag) : 0;
                    person.WoundsRight += rightClockWounds9.Opacity > 0 ? Convert.ToInt32(rightClockWounds9.Tag) : 0;
                    person.WoundsRight += rightClockWounds10.Opacity > 0 ? Convert.ToInt32(rightClockWounds10.Tag) : 0;
                    person.WoundsRight += rightClockWounds11.Opacity > 0 ? Convert.ToInt32(rightClockWounds11.Tag) : 0;
                    person.WoundsRight += rightClockWounds12.Opacity > 0 ? Convert.ToInt32(rightClockWounds12.Tag) : 0;
                    OperateIniFile.WriteIniData("Visual", "wounds right segment", person.WoundsRight.ToString(), person.IniFilePath);
                    person.WoundsDesc = this.txtWoundsDesc.Text;
                    var woundsDesc = this.txtWoundsDesc.Text.Replace("\r\n", ";;");
                    OperateIniFile.WriteIniData("Visual", "Wounds description", woundsDesc, person.IniFilePath);
                }

                //person.ScarsLeft = this.scarsLeft.SelectedIndex;
                //OperateIniFile.WriteIniData("Visual", "scars left segment", this.scarsLeft.SelectedIndex.ToString(), person.IniFilePath);
                //person.ScarsRight = this.scarsRight.SelectedIndex;
                //OperateIniFile.WriteIniData("Visual", "scars right segment", this.scarsRight.SelectedIndex.ToString(), person.IniFilePath);
                if (this.chkScars.IsChecked.Value)
                {
                    person.ScarsLeft = 0;
                    person.ScarsRight = 0;
                    person.ScarsLeft += leftClockScars1.Opacity > 0 ? Convert.ToInt32(leftClockScars1.Tag) : 0;
                    person.ScarsLeft += leftClockScars2.Opacity > 0 ? Convert.ToInt32(leftClockScars2.Tag) : 0;
                    person.ScarsLeft += leftClockScars3.Opacity > 0 ? Convert.ToInt32(leftClockScars3.Tag) : 0;
                    person.ScarsLeft += leftClockScars4.Opacity > 0 ? Convert.ToInt32(leftClockScars4.Tag) : 0;
                    person.ScarsLeft += leftClockScars5.Opacity > 0 ? Convert.ToInt32(leftClockScars5.Tag) : 0;
                    person.ScarsLeft += leftClockScars6.Opacity > 0 ? Convert.ToInt32(leftClockScars6.Tag) : 0;
                    person.ScarsLeft += leftClockScars7.Opacity > 0 ? Convert.ToInt32(leftClockScars7.Tag) : 0;
                    person.ScarsLeft += leftClockScars8.Opacity > 0 ? Convert.ToInt32(leftClockScars8.Tag) : 0;
                    person.ScarsLeft += leftClockScars9.Opacity > 0 ? Convert.ToInt32(leftClockScars9.Tag) : 0;
                    person.ScarsLeft += leftClockScars10.Opacity > 0 ? Convert.ToInt32(leftClockScars10.Tag) : 0;
                    person.ScarsLeft += leftClockScars11.Opacity > 0 ? Convert.ToInt32(leftClockScars11.Tag) : 0;
                    person.ScarsLeft += leftClockScars12.Opacity > 0 ? Convert.ToInt32(leftClockScars12.Tag) : 0;
                    OperateIniFile.WriteIniData("Visual", "scars left segment", person.ScarsLeft.ToString(), person.IniFilePath);
                    person.ScarsRight += rightClockScars1.Opacity > 0 ? Convert.ToInt32(rightClockScars1.Tag) : 0;
                    person.ScarsRight += rightClockScars2.Opacity > 0 ? Convert.ToInt32(rightClockScars2.Tag) : 0;
                    person.ScarsRight += rightClockScars3.Opacity > 0 ? Convert.ToInt32(rightClockScars3.Tag) : 0;
                    person.ScarsRight += rightClockScars4.Opacity > 0 ? Convert.ToInt32(rightClockScars4.Tag) : 0;
                    person.ScarsRight += rightClockScars5.Opacity > 0 ? Convert.ToInt32(rightClockScars5.Tag) : 0;
                    person.ScarsRight += rightClockScars6.Opacity > 0 ? Convert.ToInt32(rightClockScars6.Tag) : 0;
                    person.ScarsRight += rightClockScars7.Opacity > 0 ? Convert.ToInt32(rightClockScars7.Tag) : 0;
                    person.ScarsRight += rightClockScars8.Opacity > 0 ? Convert.ToInt32(rightClockScars8.Tag) : 0;
                    person.ScarsRight += rightClockScars9.Opacity > 0 ? Convert.ToInt32(rightClockScars9.Tag) : 0;
                    person.ScarsRight += rightClockScars10.Opacity > 0 ? Convert.ToInt32(rightClockScars10.Tag) : 0;
                    person.ScarsRight += rightClockScars11.Opacity > 0 ? Convert.ToInt32(rightClockScars11.Tag) : 0;
                    person.ScarsRight += rightClockScars12.Opacity > 0 ? Convert.ToInt32(rightClockScars12.Tag) : 0;
                    OperateIniFile.WriteIniData("Visual", "scars right segment", person.ScarsRight.ToString(), person.IniFilePath);
                    person.ScarsDesc = this.txtScarsDesc.Text;
                    var scarsDesc = this.txtScarsDesc.Text.Replace("\r\n", ";;");
                    OperateIniFile.WriteIniData("Visual", "Scars description", scarsDesc, person.IniFilePath);
                }

                MessageBox.Show(this, App.Current.FindResource("Message_30").ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, App.Current.FindResource("Message_31").ToString()+ex.Message);
            }
        }       

        private void checkMenses_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var person = this.CodeListBox.SelectedItem as Person;
                if (person != null)
                {
                    CheckBox chk = (CheckBox)sender;
                    if (chk.IsChecked.HasValue && chk.IsChecked.Value)
                    {
                        person.MensesStatus = true;
                    }
                    else
                    {
                        person.MensesStatus = this.checkMenstrualCycleDisorder.IsChecked.Value || this.checkPostmenopause.IsChecked.Value || this.checkHormonalContraceptives.IsChecked.Value ? true : false;
                    }
                }
            }
            catch { }
        }

        private void checkSomatic_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var person = this.CodeListBox.SelectedItem as Person;
                if (person != null)
                {
                    CheckBox chk = (CheckBox)sender;
                    if (chk.IsChecked.HasValue && chk.IsChecked.Value)
                    {
                        person.SomaticStatus = true;
                    }
                    else
                    {
                        person.SomaticStatus = this.checkAdiposity.IsChecked.Value || this.checkEssentialHypertension.IsChecked.Value || this.checkDiabetes.IsChecked.Value || this.checkThyroidGlandDiseases.IsChecked.Value || this.checkSomaticOther.IsChecked.Value ? true : false;
                    }
                }
            }
            catch { }
        }

        private void checkGynecologic_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var person = this.CodeListBox.SelectedItem as Person;
                if (person != null)
                {
                    CheckBox chk = (CheckBox)sender;
                    if (chk.IsChecked.HasValue && chk.IsChecked.Value)
                    {
                        person.GynecologicStatus = true;
                    }
                    else
                    {
                        person.GynecologicStatus = this.checkInfertility.IsChecked.Value || this.checkOvaryDiseases.IsChecked.Value || this.checkOvaryCyst.IsChecked.Value
                            || this.checkOvaryCancer.IsChecked.Value || this.checkOvaryEndometriosis.IsChecked.Value
                            || this.checkOvaryOther.IsChecked.Value || this.checkUterusDiseases.IsChecked.Value
                            || this.checkUterusMyoma.IsChecked.Value || this.checkUterusCancer.IsChecked.Value
                            || this.checkUterusEndometriosis.IsChecked.Value || this.checkUterusOther.IsChecked.Value
                            || this.checkGynecologicOther.IsChecked.Value ? true : false;
                    }
                }
            }
            catch { }
        } 

        private void checkObstetric_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var person = this.CodeListBox.SelectedItem as Person;
                if (person != null)
                {
                    CheckBox chk = (CheckBox)sender;
                    if (chk.IsChecked.HasValue && chk.IsChecked.Value)
                    {
                        person.ObstetricStatus = true;
                    }
                    else
                    {
                        person.ObstetricStatus = this.checkAbortions.IsChecked.Value || this.checkBirths.IsChecked.Value ? true : false;
                    }
                }
            }
            catch { }
        }

        private void checkDiseases_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var person = this.CodeListBox.SelectedItem as Person;
                if (person != null)
                {
                    CheckBox chk = (CheckBox)sender;
                    if (chk.IsChecked.HasValue && chk.IsChecked.Value)
                    {
                        person.DiseasesStatus = true;
                    }
                    else
                    {
                        person.DiseasesStatus = this.checkTrauma.IsChecked.Value || this.checkMastitis.IsChecked.Value
                            || this.checkFibrousCysticMastopathy.IsChecked.Value || this.checkCysts.IsChecked.Value
                            || this.checkCancer.IsChecked.Value || this.checkDiseasesOther.IsChecked.Value ? true : false;
                    }
                }
            }
            catch { }
        }

        private void checkPalpation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var person = this.CodeListBox.SelectedItem as Person;
                if (person != null)
                {
                    CheckBox chk = (CheckBox)sender;
                    if (chk.Name == "radPalpationStatusNormal")
                    {
                        this.radPalpationStatusAbnormal.IsChecked = false;
                        this.checkPalpationDiffuse.IsChecked = false;
                        this.checkPalpationFocal.IsChecked = false;
                    }
                    else
                    {
                        if (chk.IsChecked.HasValue && chk.IsChecked.Value)
                        {
                            person.PalpationStatus = true;
                        }
                        else
                        {
                            person.PalpationStatus = this.checkPalpationDiffuse.IsChecked.Value || this.checkPalpationFocal.IsChecked.Value ? true : false;
                        }
                    }
                    if (person.PalpationStatus)
                    {
                        person.PalpationNormal = false;
                    }                    
                }
            }
            catch { }
        }

        private void checkUltrasound_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var person = this.CodeListBox.SelectedItem as Person;
                if (person != null)
                {
                    CheckBox chk = (CheckBox)sender;
                    if (chk.Name == "radUltrasoundStatusNormal")
                    {
                        this.radUltrasoundStatusAbnormal.IsChecked = false;
                        this.checkUltrasoundDiffuse.IsChecked = false;
                        this.checkUltrasoundFocal.IsChecked = false;
                    }
                    else
                    {
                        if (chk.IsChecked.HasValue && chk.IsChecked.Value)
                        {
                            person.UltrasoundStatus = true;
                        }
                        else
                        {
                            person.UltrasoundStatus = this.checkUltrasoundDiffuse.IsChecked.Value || this.checkUltrasoundFocal.IsChecked.Value ? true : false;
                        }
                    }
                    if (person.UltrasoundStatus)
                    {
                        person.UltrasoundNormal = false;
                    } 
                }
            }
            catch { }
        }

        private void checkMammography_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var person = this.CodeListBox.SelectedItem as Person;
                if (person != null)
                {
                    CheckBox chk = (CheckBox)sender;
                    if (chk.Name == "radMammographyStatusNormal")
                    {
                        this.radMammographyStatusAbnormal.IsChecked = false;
                        this.checkMammographyDiffuse.IsChecked = false;
                        this.checkMammographyFocal.IsChecked = false;
                    }
                    else
                    {
                        if (chk.IsChecked.HasValue && chk.IsChecked.Value)
                        {
                            person.MammographyStatus = true;
                        }
                        else
                        {
                            person.MammographyStatus = this.checkMammographyDiffuse.IsChecked.Value || this.checkMammographyFocal.IsChecked.Value ? true : false;
                        }
                    }
                    if (person.MammographyStatus)
                    {
                        person.MammographyNormal = false;
                    } 
                }
            }
            catch { }
        }

        private void checkBiopsy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var person = this.CodeListBox.SelectedItem as Person;
                if (person != null)
                {
                    CheckBox chk = (CheckBox)sender;
                    if (chk.Name == "radBiopsyStatusNormal")
                    {
                        this.radBiopsyStatusAbnormal.IsChecked = false;
                        this.checkBiopsyDiffuse.IsChecked = false;
                        this.checkBiopsyFocal.IsChecked = false;
                        this.checkBiopsyCancer.IsChecked = false;
                        this.checkBiopsyProliferation.IsChecked = false;
                        this.checkBiopsyDysplasia.IsChecked = false;
                        this.checkBiopsyIntraductalPapilloma.IsChecked = false;
                        this.checkBiopsyFibroadenoma.IsChecked = false;
                        this.checkBiopsyOther.IsChecked = false;
                    }
                    else
                    {
                        if (chk.IsChecked.HasValue && chk.IsChecked.Value)
                        {
                            person.BiopsyStatus = true;
                        }
                        else
                        {
                            person.BiopsyStatus = this.checkBiopsyDiffuse.IsChecked.Value || this.checkBiopsyFocal.IsChecked.Value
                                || this.checkBiopsyCancer.IsChecked.Value || this.checkBiopsyProliferation.IsChecked.Value
                                || this.checkBiopsyDysplasia.IsChecked.Value || this.checkBiopsyIntraductalPapilloma.IsChecked.Value
                                || this.checkBiopsyFibroadenoma.IsChecked.Value || this.checkBiopsyOther.IsChecked.Value ? true : false;
                        }
                    }
                    if (person.BiopsyStatus)
                    {
                        person.BiopsyNormal = false;
                    } 
                }
            }
            catch { }
        }

        private void imgChoice_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (CodeListBox.SelectionMode.Equals(SelectionMode.Single))
                {
                    CodeListBox.SelectionMode = SelectionMode.Extended;
                    this.imgChoice.Source = new BitmapImage(new Uri("/Images/multiple_choice.png", UriKind.Relative));
                }
                else if (CodeListBox.SelectionMode.Equals(SelectionMode.Extended))
                {
                    CodeListBox.SelectionMode = SelectionMode.Single;
                    this.imgChoice.Source = new BitmapImage(new Uri("/Images/single_choice.png", UriKind.Relative));
                }
            }
            catch { }   
        }

        private void imgSort_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                CollectionViewSource customerSource = (CollectionViewSource)this.FindResource("CustomerSource");
                var set = customerSource.Source as IOrderedEnumerable<Person>;
                if (imgSort.Tag.Equals("desc"))
                {
                    customerSource.Source = set.OrderBy(x => x.Code);                    
                    imgSort.Tag = "asc";
                    imgSort.Source = new BitmapImage(new Uri("/Images/sort_asc.png", UriKind.Relative));
                    imgSort.ToolTip = "Asc";
                }
                else
                {
                    customerSource.Source = set.OrderByDescending(x => x.Code);
                    imgSort.Tag = "desc";
                    imgSort.Source = new BitmapImage(new Uri("/Images/sort_desc.png", UriKind.Relative));
                    imgSort.ToolTip = "Desc";
                }
                
            }
            catch (Exception ex) { var a=ex.Message; }
        }

        private bool isHighLight = false;
        private void Clock_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (checkPalpableLumps.IsChecked.Value)
                {
                    var clock = (Polygon)sender;
                    //var name = clock.Name;
                    //var brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x87, 0x36, 0x32));
                    //var defaultBrush = new SolidColorBrush(Color.FromArgb(0x00, 0x32, 0x87, 0x87));
                    if (!isHighLight)
                    {
                        clock.Opacity = 1;
                        isHighLight = true;
                    }
                    else
                    {
                        clock.Opacity = 0;
                        isHighLight = false;
                    }
                }
            }
            catch { }
        }
        
        private void Clock_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                if (checkPalpableLumps.IsChecked.Value)
                {
                    var clock = (Polygon)sender;
                    isHighLight = Convert.ToBoolean(clock.Opacity);
                    clock.Opacity = 1;
                }
            }
            catch { }
            
        }

        private void Clock_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                if (checkPalpableLumps.IsChecked.Value)
                {
                    var clock = (Polygon)sender;
                    if (!isHighLight)
                    {
                        clock.Opacity = 0;
                    }
                }
            }
            catch { }
        }       

        private void checkPalpableLumps_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var checkBox = (CheckBox)sender;
                if (!checkBox.IsChecked.Value)
                {
                    leftClock1.Opacity = 0;
                    leftClock2.Opacity = 0;
                    leftClock3.Opacity = 0;
                    leftClock4.Opacity = 0;
                    leftClock5.Opacity = 0;
                    leftClock6.Opacity = 0;
                    leftClock7.Opacity = 0;
                    leftClock8.Opacity = 0;
                    leftClock9.Opacity = 0;
                    leftClock9.Opacity = 0;
                    leftClock10.Opacity = 0;
                    leftClock11.Opacity = 0;
                    leftClock12.Opacity = 0;

                    rightClock1.Opacity = 0;
                    rightClock2.Opacity = 0;
                    rightClock3.Opacity = 0;
                    rightClock4.Opacity = 0;
                    rightClock5.Opacity = 0;
                    rightClock6.Opacity = 0;
                    rightClock7.Opacity = 0;
                    rightClock8.Opacity = 0;
                    rightClock9.Opacity = 0;
                    rightClock10.Opacity = 0;
                    rightClock11.Opacity = 0;
                    rightClock12.Opacity = 0;
                }
            }
            catch { }
            
        }

        private void checkPain_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var checkBox = (CheckBox)sender;
                if (!checkBox.IsChecked.Value)
                {
                    degree1.IsChecked = false;
                    degree2.IsChecked = false;
                    degree3.IsChecked = false;
                    degree4.IsChecked = false;
                    degree5.IsChecked = false;
                    degree6.IsChecked = false;
                    degree7.IsChecked = false;
                    degree8.IsChecked = false;
                    degree9.IsChecked = false;
                    degree10.IsChecked = false;
                    degree1.IsEnabled = false;
                    degree2.IsEnabled = false;
                    degree3.IsEnabled = false;
                    degree4.IsEnabled = false;
                    degree5.IsEnabled = false;
                    degree6.IsEnabled = false;
                    degree7.IsEnabled = false;
                    degree8.IsEnabled = false;
                    degree9.IsEnabled = false;
                    degree10.IsEnabled = false;
                }
                else
                {
                    degree1.IsEnabled = true;
                    degree2.IsEnabled = true;
                    degree3.IsEnabled = true;
                    degree4.IsEnabled = true;
                    degree5.IsEnabled = true;
                    degree6.IsEnabled = true;
                    degree7.IsEnabled = true;
                    degree8.IsEnabled = true;
                    degree9.IsEnabled = true;
                    degree10.IsEnabled = true;
                }
            }
            catch { }
        }

        private void txtClientNum_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox=(TextBox)sender;
            if (textBox.Text.Length > 10)
            {
                e.Handled = true;
            }
            else
            {
                e.Handled = false;
            }
           
        }

        private void Number_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                Regex re = new Regex("[0-9]+");
                if (re.IsMatch(e.Text))
                {
                    var textBox = (TextBox)sender;
                    if (!string.IsNullOrEmpty(textBox.Text))
                    {
                        if (Convert.ToInt32(textBox.Text + e.Text) < 100)
                        {
                            e.Handled = false;
                        }
                        else
                        {
                            MessageBox.Show(this, App.Current.FindResource("Message_56").ToString());
                            e.Handled = true;
                        }
                    }
                    else
                    {
                        e.Handled = false;
                    }

                }
                else
                {
                    MessageBox.Show(this, App.Current.FindResource("Message_56").ToString());
                    e.Handled = true;
                }
            }
            catch (Exception)
            {
                e.Handled = true;
            }

        }

        private void Year_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                Regex re = new Regex("[0-9]+");
                if (re.IsMatch(e.Text))
                {
                    var textBox = (TextBox)sender;
                    if (!string.IsNullOrEmpty(textBox.Text))
                    {
                        if (Convert.ToInt32(textBox.Text + e.Text) <= DateTime.Now.Year)
                        {
                            e.Handled = false;
                        }
                        else
                        {
                            MessageBox.Show(this, App.Current.FindResource("Message_59").ToString());
                            e.Handled = true;
                        }
                    }
                    else
                    {
                        e.Handled = false;
                    }

                }
                else
                {
                    MessageBox.Show(this, App.Current.FindResource("Message_53").ToString());
                    e.Handled = true;
                }
            }
            catch (Exception)
            {
                e.Handled = true;
            }

        }

        private void Month_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                Regex re = new Regex("[0-9]+");
                if (re.IsMatch(e.Text))
                {
                    var textBox = (TextBox)sender;
                    if (!string.IsNullOrEmpty(textBox.Text))
                    {
                        if (Convert.ToInt32(textBox.Text + e.Text) <= DateTime.Now.Year)
                        {
                            e.Handled = false;
                        }
                        else
                        {
                            MessageBox.Show(this, App.Current.FindResource("Message_57").ToString());
                            e.Handled = true;
                        }
                    }
                    else
                    {
                        e.Handled = false;
                    }

                }
                else
                {
                    MessageBox.Show(this, App.Current.FindResource("Message_53").ToString());
                    e.Handled = true;
                }
            }
            catch (Exception)
            {
                e.Handled = true;
            }

        }

        private void Day_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                Regex re = new Regex("[0-9]+");
                if (re.IsMatch(e.Text))
                {
                    var textBox = (TextBox)sender;
                    if (!string.IsNullOrEmpty(textBox.Text))
                    {
                        if (Convert.ToInt32(textBox.Text + e.Text) <= DateTime.Now.Year)
                        {
                            e.Handled = false;
                        }
                        else
                        {
                            MessageBox.Show(this, App.Current.FindResource("Message_58").ToString());
                            e.Handled = true;
                        }
                    }
                    else
                    {
                        e.Handled = false;
                    }

                }
                else
                {
                    MessageBox.Show(this, App.Current.FindResource("Message_53").ToString());
                    e.Handled = true;
                }
            }
            catch (Exception)
            {
                e.Handled = true;
            }

        }

        private void Weight_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                Regex re = new Regex("[0-9]+");
                if (re.IsMatch(e.Text))
                {
                    var textBox = (TextBox)sender;
                    if (!string.IsNullOrEmpty(textBox.Text))
                    {
                        if (Convert.ToInt32(textBox.Text + e.Text) <500)
                        {
                            e.Handled = false;
                        }
                        else
                        {
                            MessageBox.Show(this, App.Current.FindResource("Message_60").ToString());
                            e.Handled = true;
                        }
                    }
                    else
                    {
                        e.Handled = false;
                    }

                }
                else
                {
                    MessageBox.Show(this, App.Current.FindResource("Message_53").ToString());
                    e.Handled = true;
                }
            }
            catch (Exception)
            {
                e.Handled = true;
            }

        }

        private void Mobile_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                Regex re = new Regex("[0-9+-]+");
                if (re.IsMatch(e.Text))
                {                    
                    e.Handled = false;                    
                }
                else
                {
                    MessageBox.Show(this, App.Current.FindResource("Message_61").ToString());
                    e.Handled = true;
                }
            }
            catch (Exception)
            {
                e.Handled = true;
            }

        }        

        private void chkRedSwollen_Click(object sender, RoutedEventArgs e)
        {
            //var person = this.CodeListBox.SelectedItem as Person;
            //if (person != null)
            //{
            //    CheckBox chk = (CheckBox)sender;
            //    if (!chk.IsChecked.HasValue || !chk.IsChecked.Value)
            //    {                
            //        redSwollenLeft.SelectedIndex = 0;
            //        redSwollenRight.SelectedIndex = 0;
            //        redSwollenLeft.IsEnabled = false;
            //        redSwollenRight.IsEnabled = false;
            //    }
            //    else
            //    {
            //        redSwollenLeft.IsEnabled = true;
            //        redSwollenRight.IsEnabled = true;
            //    }
            //}
            try
            {
                var checkBox = (CheckBox)sender;
                if (!checkBox.IsChecked.Value)
                {
                    leftClockRedSwollen1.Opacity = 0;
                    leftClockRedSwollen2.Opacity = 0;
                    leftClockRedSwollen3.Opacity = 0;
                    leftClockRedSwollen4.Opacity = 0;
                    leftClockRedSwollen5.Opacity = 0;
                    leftClockRedSwollen6.Opacity = 0;
                    leftClockRedSwollen7.Opacity = 0;
                    leftClockRedSwollen8.Opacity = 0;
                    leftClockRedSwollen9.Opacity = 0;                    
                    leftClockRedSwollen10.Opacity = 0;
                    leftClockRedSwollen11.Opacity = 0;
                    leftClockRedSwollen12.Opacity = 0;

                    rightClockRedSwollen1.Opacity = 0;
                    rightClockRedSwollen2.Opacity = 0;
                    rightClockRedSwollen3.Opacity = 0;
                    rightClockRedSwollen4.Opacity = 0;
                    rightClockRedSwollen5.Opacity = 0;
                    rightClockRedSwollen6.Opacity = 0;
                    rightClockRedSwollen7.Opacity = 0;
                    rightClockRedSwollen8.Opacity = 0;
                    rightClockRedSwollen9.Opacity = 0;
                    rightClockRedSwollen10.Opacity = 0;
                    rightClockRedSwollen11.Opacity = 0;
                    rightClockRedSwollen12.Opacity = 0;
                }
            }
            catch { }
        }
        private void ClockRedSwollen_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (chkRedSwollen.IsChecked.Value)
                {
                    var clock = (Polygon)sender;
                    //var name = clock.Name;
                    //var brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x87, 0x36, 0x32));
                    //var defaultBrush = new SolidColorBrush(Color.FromArgb(0x00, 0x32, 0x87, 0x87));
                    if (!isHighLight)
                    {
                        clock.Opacity = 1;
                        isHighLight = true;
                    }
                    else
                    {
                        clock.Opacity = 0;
                        isHighLight = false;
                    }
                }
            }
            catch { }
        }

        private void ClockRedSwollen_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                if (chkRedSwollen.IsChecked.Value)
                {
                    var clock = (Polygon)sender;
                    isHighLight = Convert.ToBoolean(clock.Opacity);
                    clock.Opacity = 1;
                }
            }
            catch { }

        }

        private void ClockRedSwollen_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                if (chkRedSwollen.IsChecked.Value)
                {
                    var clock = (Polygon)sender;
                    if (!isHighLight)
                    {
                        clock.Opacity = 0;
                    }
                }
            }
            catch { }
        }   

        private void chkPalpable_Click(object sender, RoutedEventArgs e)
        {
            //var person = this.CodeListBox.SelectedItem as Person;
            //if (person != null)
            //{
            //    CheckBox chk = (CheckBox)sender;
            //    if (!chk.IsChecked.HasValue || !chk.IsChecked.Value)                
            //    {
            //        palpableLeft.SelectedIndex = 0;
            //        palpableRight.SelectedIndex = 0;
            //        palpableLeft.IsEnabled = false;
            //        palpableRight.IsEnabled = false;
            //    }
            //    else
            //    {
            //        palpableLeft.IsEnabled = true;
            //        palpableRight.IsEnabled = true;
            //    }
            //}
            try
            {
                var checkBox = (CheckBox)sender;
                if (!checkBox.IsChecked.Value)
                {
                    leftClockPalpable1.Opacity = 0;
                    leftClockPalpable2.Opacity = 0;
                    leftClockPalpable3.Opacity = 0;
                    leftClockPalpable4.Opacity = 0;
                    leftClockPalpable5.Opacity = 0;
                    leftClockPalpable6.Opacity = 0;
                    leftClockPalpable7.Opacity = 0;
                    leftClockPalpable8.Opacity = 0;
                    leftClockPalpable9.Opacity = 0;                    
                    leftClockPalpable10.Opacity = 0;
                    leftClockPalpable11.Opacity = 0;
                    leftClockPalpable12.Opacity = 0;

                    rightClockPalpable1.Opacity = 0;
                    rightClockPalpable2.Opacity = 0;
                    rightClockPalpable3.Opacity = 0;
                    rightClockPalpable4.Opacity = 0;
                    rightClockPalpable5.Opacity = 0;
                    rightClockPalpable6.Opacity = 0;
                    rightClockPalpable7.Opacity = 0;
                    rightClockPalpable8.Opacity = 0;
                    rightClockPalpable9.Opacity = 0;
                    rightClockPalpable10.Opacity = 0;
                    rightClockPalpable11.Opacity = 0;
                    rightClockPalpable12.Opacity = 0;
                }
            }
            catch { }
        }
        private void ClockPalpable_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (chkPalpable.IsChecked.Value)
                {
                    var clock = (Polygon)sender;
                    //var name = clock.Name;
                    //var brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x87, 0x36, 0x32));
                    //var defaultBrush = new SolidColorBrush(Color.FromArgb(0x00, 0x32, 0x87, 0x87));
                    if (!isHighLight)
                    {
                        clock.Opacity = 1;
                        isHighLight = true;
                    }
                    else
                    {
                        clock.Opacity = 0;
                        isHighLight = false;
                    }
                }
            }
            catch { }
        }

        private void ClockPalpable_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                if (chkPalpable.IsChecked.Value)
                {
                    var clock = (Polygon)sender;
                    isHighLight = Convert.ToBoolean(clock.Opacity);
                    clock.Opacity = 1;
                }
            }
            catch { }

        }

        private void ClockPalpable_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                if (chkPalpable.IsChecked.Value)
                {
                    var clock = (Polygon)sender;
                    if (!isHighLight)
                    {
                        clock.Opacity = 0;
                    }
                }
            }
            catch { }
        } 

        private void chkSerous_Click(object sender, RoutedEventArgs e)
        {
            //var person = this.CodeListBox.SelectedItem as Person;
            //if (person != null)
            //{
            //    CheckBox chk = (CheckBox)sender;
            //    if (!chk.IsChecked.HasValue || !chk.IsChecked.Value)
            //    {
            //        serousLeft.SelectedIndex = 0;
            //        serousRight.SelectedIndex = 0;
            //        serousLeft.IsEnabled = false;
            //        serousRight.IsEnabled = false;
            //    }
            //    else
            //    {
            //        serousLeft.IsEnabled = true;
            //        serousRight.IsEnabled = true;
            //    }
            //}
            try
            {
                var checkBox = (CheckBox)sender;
                if (!checkBox.IsChecked.Value)
                {
                    leftClockSerious1.Opacity = 0;
                    leftClockSerious2.Opacity = 0;
                    leftClockSerious3.Opacity = 0;
                    leftClockSerious4.Opacity = 0;
                    leftClockSerious5.Opacity = 0;
                    leftClockSerious6.Opacity = 0;
                    leftClockSerious7.Opacity = 0;
                    leftClockSerious8.Opacity = 0;
                    leftClockSerious9.Opacity = 0;
                    leftClockSerious10.Opacity = 0;
                    leftClockSerious11.Opacity = 0;
                    leftClockSerious12.Opacity = 0;

                    rightClockSerious1.Opacity = 0;
                    rightClockSerious2.Opacity = 0;
                    rightClockSerious3.Opacity = 0;
                    rightClockSerious4.Opacity = 0;
                    rightClockSerious5.Opacity = 0;
                    rightClockSerious6.Opacity = 0;
                    rightClockSerious7.Opacity = 0;
                    rightClockSerious8.Opacity = 0;
                    rightClockSerious9.Opacity = 0;
                    rightClockSerious10.Opacity = 0;
                    rightClockSerious11.Opacity = 0;
                    rightClockSerious12.Opacity = 0;
                }
            }
            catch { }
        }
        private void ClockSerious_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (chkSerous.IsChecked.Value)
                {
                    var clock = (Polygon)sender;
                    //var name = clock.Name;
                    //var brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x87, 0x36, 0x32));
                    //var defaultBrush = new SolidColorBrush(Color.FromArgb(0x00, 0x32, 0x87, 0x87));
                    if (!isHighLight)
                    {
                        clock.Opacity = 1;
                        isHighLight = true;
                    }
                    else
                    {
                        clock.Opacity = 0;
                        isHighLight = false;
                    }
                }
            }
            catch { }
        }

        private void ClockSerious_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                if (chkSerous.IsChecked.Value)
                {
                    var clock = (Polygon)sender;
                    isHighLight = Convert.ToBoolean(clock.Opacity);
                    clock.Opacity = 1;
                }
            }
            catch { }

        }

        private void ClockSerious_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                if (chkSerous.IsChecked.Value)
                {
                    var clock = (Polygon)sender;
                    if (!isHighLight)
                    {
                        clock.Opacity = 0;
                    }
                }
            }
            catch { }
        } 

        private void chkWounds_Click(object sender, RoutedEventArgs e)
        {
            //var person = this.CodeListBox.SelectedItem as Person;
            //if (person != null)
            //{
            //    CheckBox chk = (CheckBox)sender;
            //    if (!chk.IsChecked.HasValue || !chk.IsChecked.Value)
            //    {
            //        woundsLeft.SelectedIndex = 0;
            //        woundsRight.SelectedIndex = 0;
            //        woundsLeft.IsEnabled = false;
            //        woundsRight.IsEnabled = false;
            //    }
            //    else
            //    {
            //        woundsLeft.IsEnabled = true;
            //        woundsRight.IsEnabled = true;
            //    }
            //}
            try
            {
                var checkBox = (CheckBox)sender;
                if (!checkBox.IsChecked.Value)
                {
                    leftClockWounds1.Opacity = 0;
                    leftClockWounds2.Opacity = 0;
                    leftClockWounds3.Opacity = 0;
                    leftClockWounds4.Opacity = 0;
                    leftClockWounds5.Opacity = 0;
                    leftClockWounds6.Opacity = 0;
                    leftClockWounds7.Opacity = 0;
                    leftClockWounds8.Opacity = 0;
                    leftClockWounds9.Opacity = 0;
                    leftClockWounds10.Opacity = 0;
                    leftClockWounds11.Opacity = 0;
                    leftClockWounds12.Opacity = 0;

                    rightClockWounds1.Opacity = 0;
                    rightClockWounds2.Opacity = 0;
                    rightClockWounds3.Opacity = 0;
                    rightClockWounds4.Opacity = 0;
                    rightClockWounds5.Opacity = 0;
                    rightClockWounds6.Opacity = 0;
                    rightClockWounds7.Opacity = 0;
                    rightClockWounds8.Opacity = 0;
                    rightClockWounds9.Opacity = 0;
                    rightClockWounds10.Opacity = 0;
                    rightClockWounds11.Opacity = 0;
                    rightClockWounds12.Opacity = 0;
                }
            }
            catch { }
        }
        private void ClockWounds_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (chkWounds.IsChecked.Value)
                {
                    var clock = (Polygon)sender;
                    //var name = clock.Name;
                    //var brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x87, 0x36, 0x32));
                    //var defaultBrush = new SolidColorBrush(Color.FromArgb(0x00, 0x32, 0x87, 0x87));
                    if (!isHighLight)
                    {
                        clock.Opacity = 1;
                        isHighLight = true;
                    }
                    else
                    {
                        clock.Opacity = 0;
                        isHighLight = false;
                    }
                }
            }
            catch { }
        }

        private void ClockWounds_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                if (chkWounds.IsChecked.Value)
                {
                    var clock = (Polygon)sender;
                    isHighLight = Convert.ToBoolean(clock.Opacity);
                    clock.Opacity = 1;
                }
            }
            catch { }

        }

        private void ClockWounds_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                if (chkWounds.IsChecked.Value)
                {
                    var clock = (Polygon)sender;
                    if (!isHighLight)
                    {
                        clock.Opacity = 0;
                    }
                }
            }
            catch { }
        } 

        private void chkScars_Click(object sender, RoutedEventArgs e)
        {
            //var person = this.CodeListBox.SelectedItem as Person;
            //if (person != null)
            //{
            //    CheckBox chk = (CheckBox)sender;
            //    if (!chk.IsChecked.HasValue || !chk.IsChecked.Value)
            //    {
            //        scarsLeft.SelectedIndex = 0;
            //        scarsRight.SelectedIndex = 0;
            //        scarsLeft.IsEnabled = false;
            //        scarsRight.IsEnabled = false;
            //    }
            //    else
            //    {
            //        scarsLeft.IsEnabled = true;
            //        scarsRight.IsEnabled = true;
            //    }
            //}
            try
            {
                var checkBox = (CheckBox)sender;
                if (!checkBox.IsChecked.Value)
                {
                    leftClockScars1.Opacity = 0;
                    leftClockScars2.Opacity = 0;
                    leftClockScars3.Opacity = 0;
                    leftClockScars4.Opacity = 0;
                    leftClockScars5.Opacity = 0;
                    leftClockScars6.Opacity = 0;
                    leftClockScars7.Opacity = 0;
                    leftClockScars8.Opacity = 0;
                    leftClockScars9.Opacity = 0;                    
                    leftClockScars10.Opacity = 0;
                    leftClockScars11.Opacity = 0;
                    leftClockScars12.Opacity = 0;

                    rightClockScars1.Opacity = 0;
                    rightClockScars2.Opacity = 0;
                    rightClockScars3.Opacity = 0;
                    rightClockScars4.Opacity = 0;
                    rightClockScars5.Opacity = 0;
                    rightClockScars6.Opacity = 0;
                    rightClockScars7.Opacity = 0;
                    rightClockScars8.Opacity = 0;
                    rightClockScars9.Opacity = 0;
                    rightClockScars10.Opacity = 0;
                    rightClockScars11.Opacity = 0;
                    rightClockScars12.Opacity = 0;
                }
            }
            catch { }
        }
        private void ClockScars_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (chkScars.IsChecked.Value)
                {
                    var clock = (Polygon)sender;
                    //var name = clock.Name;
                    //var brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x87, 0x36, 0x32));
                    //var defaultBrush = new SolidColorBrush(Color.FromArgb(0x00, 0x32, 0x87, 0x87));
                    if (!isHighLight)
                    {
                        clock.Opacity = 1;
                        isHighLight = true;
                    }
                    else
                    {
                        clock.Opacity = 0;
                        isHighLight = false;
                    }
                }
            }
            catch { }
        }

        private void ClockScars_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                if (chkScars.IsChecked.Value)
                {
                    var clock = (Polygon)sender;
                    isHighLight = Convert.ToBoolean(clock.Opacity);
                    clock.Opacity = 1;
                }
            }
            catch { }

        }

        private void ClockScars_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                if (chkScars.IsChecked.Value)
                {
                    var clock = (Polygon)sender;
                    if (!isHighLight)
                    {
                        clock.Opacity = 0;
                    }
                }
            }
            catch { }
        }

        private void Hour_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                Regex re = new Regex("[0-9]+");
                if (re.IsMatch(e.Text))
                {
                    var textBox = (TextBox)sender;
                    if (!string.IsNullOrEmpty(textBox.Text))
                    {
                        if (Convert.ToInt32(textBox.Text + e.Text) < 24 && (textBox.Text.Length + 1) <= 2)
                        {
                            e.Handled = false;
                        }
                        else
                        {
                            MessageBox.Show(this, App.Current.FindResource("Message_54").ToString());
                            e.Handled = true;
                        }
                    }
                    else
                    {
                        e.Handled = false;
                    }
                }
                else
                {
                    MessageBox.Show(this, App.Current.FindResource("Message_53").ToString());
                    e.Handled = true;
                }
            }
            catch (Exception)
            {
                e.Handled = true;
            }
        }

        private void Minute_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                Regex re = new Regex("[0-9]+");
                if (re.IsMatch(e.Text))
                {
                    var textBox = (TextBox)sender;
                    if (!string.IsNullOrEmpty(textBox.Text))
                    {
                        if (Convert.ToInt32(textBox.Text + e.Text) < 60 && (textBox.Text.Length + 1) <= 2)
                        {
                            e.Handled = false;
                        }
                        else
                        {
                            MessageBox.Show(this, App.Current.FindResource("Message_55").ToString());
                            e.Handled = true;
                        }
                    }
                    else
                    {
                        e.Handled = false;
                    }
                }
                else
                {
                    MessageBox.Show(this, App.Current.FindResource("Message_53").ToString());
                    e.Handled = true;
                }
            }
            catch (Exception)
            {
                e.Handled = true;
            }
        }

        #endregion
        

        #region 报表表单相关事件和方法

        private void loadReportData()
        {           
            try
            {
                this.shortFormReportModel = new ShortFormReport();
                var person = this.CodeListBox.SelectedItem as Person;                
                if (person == null)
                {
                                      
                }
                else
                {                    
                    string dataFile = FindUserReportData(person.ArchiveFolder);
                    if (string.IsNullOrEmpty(dataFile))
                    {
                        dataFile = dataFolder + System.IO.Path.DirectorySeparatorChar + person.Code + ".dat";
                    }
                    //string dataFile = dataFolder + System.IO.Path.DirectorySeparatorChar + person.Code + ".dat";                    

                    if (File.Exists(dataFile))
                    {
                        try
                        {
                            this.shortFormReportModel = SerializeUtilities.Desrialize<ShortFormReport>(dataFile);
                        }
                        catch (Exception exe)
                        {
                            MessageBox.Show(this, App.Current.FindResource("Message_76").ToString() + exe.Message);
                        }

                        if (shortFormReportModel.DataSignImg != null)
                        {
                            this.dataSignImg.Source = ImageTools.GetBitmapImage(shortFormReportModel.DataSignImg);
                        }

                    }
                    else
                    {
                        shortFormReportModel.DataUserCode = person.Code;                        
                        if (App.reportSettingModel.ShowDoctorSignature&&App.reportSettingModel.UseDefaultSignature)
                        {
                            string imgFile = AppDomain.CurrentDomain.BaseDirectory + "/Signature/temp.jpg";
                            if (File.Exists(imgFile))
                            {
                                this.dataSignImg.Source = ImageTools.GetBitmapImage(imgFile);                                
                            }
                        }
                    }
                    if ("en-US".Equals(App.local))
                    {
                        shortFormReportModel.DataScreenDate = DateTime.ParseExact("20" + person.Code.Substring(0, 6), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture).ToString("MM/dd/yyyy");
                    }
                    else
                    {
                        shortFormReportModel.DataScreenDate = DateTime.ParseExact("20" + person.Code.Substring(0, 6), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture).ToString("yyyy年MM月dd日");
                    }
                    shortFormReportModel.DataClientNum = person.ClientNumber;
                    shortFormReportModel.DataName = person.SurName + ", " + person.GivenName + " " + person.OtherName;
                    shortFormReportModel.DataAge = person.Age + "";
                    shortFormReportModel.DataAddress = person.Address;
                    shortFormReportModel.DataHeight = person.Height;
                    shortFormReportModel.DataWeight = person.Weight;
                    shortFormReportModel.DataMobile = person.Mobile;
                    shortFormReportModel.DataEmail = person.Email;
                    shortFormReportModel.DataScreenLocation = person.ScreenVenue;
                    this.reportDataGrid.DataContext = shortFormReportModel;
                    //以下是添加处理操作员和医生的名字的选择项                    
                    User doctorUser = new User();
                    if (!string.IsNullOrEmpty(shortFormReportModel.DataDoctor))
                    {
                        doctorUser.Name = shortFormReportModel.DataDoctor;
                        doctorUser.License = shortFormReportModel.DataDoctorLicense;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(person.DoctorName))
                        {
                            doctorUser.Name = person.DoctorName;
                            doctorUser.License = person.DoctorLicense;
                            shortFormReportModel.DataDoctor =person.DoctorName;
                            shortFormReportModel.DataDoctorLicense = person.DoctorLicense;
                        }
                    }

                    this.dataDoctor.ItemsSource = App.reportSettingModel.DoctorNames;
                    if (!string.IsNullOrEmpty(doctorUser.Name))
                    {
                        for (int i = 0; i < App.reportSettingModel.DoctorNames.Count; i++)
                        {
                            var item = App.reportSettingModel.DoctorNames[i];
                            if (doctorUser.Name.Equals(item.Name) && (string.IsNullOrEmpty(doctorUser.License) == string.IsNullOrEmpty(item.License) || doctorUser.License == item.License))
                            {
                                this.dataDoctor.SelectedIndex = i;
                                break;
                            }
                        }
                        //如果没有找到匹配的用户
                        if (this.dataDoctor.SelectedIndex == -1)
                        {                            
                            tempDoctorNames.Add(doctorUser);
                            this.dataDoctor.ItemsSource = tempDoctorNames;
                            this.dataDoctor.SelectedIndex = tempDoctorNames.Count - 1;

                        }
                    }

                    
                    //if (string.IsNullOrEmpty(shortFormReportModel.DataMeikTech))
                    //{
                        if (!string.IsNullOrEmpty(person.TechName))
                        {
                            shortFormReportModel.DataMeikTech = person.TechName;
                            shortFormReportModel.DataTechLicense = person.TechLicense;
                        }
                    //}
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }
        
        private void previewBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadDataModel();
                var reportModel = CloneReportModel();
                //PrintPreviewWindow previewWnd = new PrintPreviewWindow("Views/ExaminationReportDocument.xaml", true, reportModel);
                PrintPreviewWindow previewWnd = new PrintPreviewWindow("Views/ExaminationReportFlow.xaml", false, reportModel);
                //PrintPreviewWindow previewWnd = new PrintPreviewWindow("Views/SummaryReportFlow.xaml", false, reportModel);                
                previewWnd.Owner = this;
                previewWnd.ShowInTaskbar = false;
                previewWnd.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private void previewComb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {               
                var comboBox = sender as ComboBox;            
                if (comboBox.SelectedIndex == 1)
                {
                    LoadDataModel();
                    var reportModel = CloneReportModel();
                    //PrintPreviewWindow previewWnd = new PrintPreviewWindow("Views/ExaminationReportFlow.xaml", false, reportModel);
                    PrintPreviewWindow previewWnd = new PrintPreviewWindow("Views/GoodUnionReportFlow.xaml", false, reportModel);
                    previewWnd.Owner = this;
                    previewWnd.ShowInTaskbar = false;
                    previewWnd.ShowDialog();
                }
                else if (comboBox.SelectedIndex == 2)
                {
                    LoadDataModel();
                    var reportModel = CloneClientReportModel();
                    PrintPreviewWindow previewWnd = new PrintPreviewWindow("Views/SummaryReportFlow.xaml", false, reportModel);
                    previewWnd.Owner = this;
                    previewWnd.ShowInTaskbar = false;
                    previewWnd.ShowDialog();
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private void btnOpenDiagn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                App.opendWin = this;
                IntPtr mainWinHwnd = Win32Api.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "TfmMain", null);
                //如果主窗体不存在
                if (mainWinHwnd == IntPtr.Zero)
                {
                    IntPtr diagnosticsBtnHwnd = Win32Api.FindWindowEx(App.splashWinHwnd, IntPtr.Zero, null, App.strDiagnostics);
                    Win32Api.SendMessage(diagnosticsBtnHwnd, Win32Api.WM_CLICK, 0, 0);
                }
                else
                {
                    this.StartMouseHook();
                }
                //WinMinimized();
                


            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "System Exception: " + ex.Message);
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var person = this.CodeListBox.SelectedItem as Person; 
            //判断是否已经从MEIK生成的DOC文档中导入检查数据，如果之前没有，则查找是否已在本地生成DOC文档，导入数据            
            string docFile = FindMEIKXmlReport(person.ArchiveFolder);
            if (!string.IsNullOrEmpty(docFile) && File.Exists(docFile))
            {
                //ShortFormReport shortFormReport = WordTools.ReadWordFile(docFile);
                ShortFormReport shortFormReport = XmlTools.ReadXmlFile(docFile);
                if (shortFormReport != null)
                {
                    try
                    {
                        /**因为ShortFormReport的属性没有添加依赖变更事件，所以这里为shortFormReportModel赋值不会影响页面显示效果。
                         * 不过即使为ShortFormReport的属性添加上依赖变更事件，但由于之前用户可能已经序列化过一些报表数据，
                         * 如果改变ShortFormReport对象的继承定义，会导致反序列化失败，所以只能暂时不处理，只在这里强制修改页面元素
                         * **/
                        shortFormReportModel.DataMenstrualCycle = shortFormReport.DataMenstrualCycle;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataMenstrualCycle))
                        {
                            this.dataMenstrualCycle.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataMenstrualCycle);
                        }
                        shortFormReportModel.DataHormones = shortFormReport.DataHormones;
                        this.dataHormones.Text = shortFormReportModel.DataHormones;
                        shortFormReportModel.DataSkinAffections = shortFormReport.DataSkinAffections;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataSkinAffections))
                        {
                            this.dataSkinAffections.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataSkinAffections);
                        }
                        shortFormReportModel.DataLeftChangesOfElectricalConductivity = shortFormReport.DataLeftChangesOfElectricalConductivity;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataLeftChangesOfElectricalConductivity))
                        {
                            this.dataLeftChangesOfElectricalConductivity.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataLeftChangesOfElectricalConductivity);
                        }
                        shortFormReportModel.DataRightChangesOfElectricalConductivity = shortFormReport.DataRightChangesOfElectricalConductivity;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataRightChangesOfElectricalConductivity))
                        {
                            this.dataRightChangesOfElectricalConductivity.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataRightChangesOfElectricalConductivity);
                        }

                        shortFormReportModel.DataLeftMammaryStruct = shortFormReport.DataLeftMammaryStruct;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataLeftMammaryStruct))
                        {
                            this.dataLeftMammaryStruct.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataLeftMammaryStruct);
                        }
                        shortFormReportModel.DataRightMammaryStruct = shortFormReport.DataRightMammaryStruct;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataRightMammaryStruct))
                        {
                            this.dataRightMammaryStruct.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataRightMammaryStruct);
                        }
                        shortFormReportModel.DataLeftLactiferousSinusZone = shortFormReport.DataLeftLactiferousSinusZone;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataLeftLactiferousSinusZone))
                        {
                            this.dataLeftLactiferousSinusZone.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataLeftLactiferousSinusZone);
                        }
                        shortFormReportModel.DataRightLactiferousSinusZone = shortFormReport.DataRightLactiferousSinusZone;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataRightLactiferousSinusZone))
                        {
                            this.dataRightLactiferousSinusZone.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataRightLactiferousSinusZone);
                        }
                        shortFormReportModel.DataLeftMammaryContour = shortFormReport.DataLeftMammaryContour;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataLeftMammaryContour))
                        {
                            this.dataLeftMammaryContour.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataLeftMammaryContour);
                        }
                        shortFormReportModel.DataRightMammaryContour = shortFormReport.DataRightMammaryContour;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataRightMammaryContour))
                        {
                            this.dataRightMammaryContour.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataRightMammaryContour);
                        }

                        shortFormReportModel.DataLeftNumber = shortFormReport.DataLeftNumber;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataLeftNumber))
                        {
                            this.dataLeftNumber.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataLeftNumber);
                        }
                        shortFormReportModel.DataRightNumber = shortFormReport.DataRightNumber;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataRightNumber))
                        {
                            this.dataRightNumber.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataRightNumber);
                        }

                        shortFormReportModel.DataLeftSegment = shortFormReport.DataLeftSegment;
                        this.dataLeftSegment.Text = shortFormReportModel.DataLeftSegment;
                        shortFormReportModel.DataRightSegment = shortFormReport.DataRightSegment;
                        this.dataRightSegment.Text = shortFormReportModel.DataRightSegment;
                        shortFormReportModel.DataLeftSize = shortFormReport.DataLeftSize;
                        this.dataLeftSize.Text = shortFormReportModel.DataLeftSize;
                        shortFormReportModel.DataRightSize = shortFormReport.DataRightSize;
                        this.dataRightSize.Text = shortFormReportModel.DataRightSize;

                        shortFormReportModel.DataLeftShape = shortFormReport.DataLeftShape;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataLeftShape))
                        {
                            this.dataLeftShape.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataLeftShape);
                        }
                        shortFormReportModel.DataRightShape = shortFormReport.DataRightShape;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataRightShape))
                        {
                            this.dataRightShape.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataRightShape);
                        }

                        shortFormReportModel.DataLeftContourAroundFocus = shortFormReport.DataLeftContourAroundFocus;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataLeftContourAroundFocus))
                        {
                            this.dataLeftContourAroundFocus.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataLeftContourAroundFocus);
                        }
                        shortFormReportModel.DataRightContourAroundFocus = shortFormReport.DataRightContourAroundFocus;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataRightContourAroundFocus))
                        {
                            this.dataRightContourAroundFocus.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataRightContourAroundFocus);
                        }
                        shortFormReportModel.DataLeftInternalElectricalStructure = shortFormReport.DataLeftInternalElectricalStructure;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataLeftInternalElectricalStructure))
                        {
                            this.dataLeftInternalElectricalStructure.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataLeftInternalElectricalStructure);
                        }
                        shortFormReportModel.DataRightInternalElectricalStructure = shortFormReport.DataRightInternalElectricalStructure;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataRightInternalElectricalStructure))
                        {
                            this.dataRightInternalElectricalStructure.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataRightInternalElectricalStructure);
                        }
                        shortFormReportModel.DataLeftSurroundingTissues = shortFormReport.DataLeftSurroundingTissues;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataLeftSurroundingTissues))
                        {
                            this.dataLeftSurroundingTissues.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataLeftSurroundingTissues);
                        }
                        shortFormReportModel.DataRightSurroundingTissues = shortFormReport.DataRightSurroundingTissues;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataRightSurroundingTissues))
                        {
                            this.dataRightSurroundingTissues.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataRightSurroundingTissues);
                        }

                        shortFormReportModel.DataLeftMeanElectricalConductivity1 = shortFormReport.DataLeftMeanElectricalConductivity1;
                        this.dataLeftMeanElectricalConductivity1.Text = shortFormReportModel.DataLeftMeanElectricalConductivity1;
                        shortFormReportModel.DataRightMeanElectricalConductivity1 = shortFormReport.DataRightMeanElectricalConductivity1;
                        this.dataRightMeanElectricalConductivity1.Text = shortFormReportModel.DataRightMeanElectricalConductivity1;
                        shortFormReportModel.DataLeftMeanElectricalConductivity2 = shortFormReport.DataLeftMeanElectricalConductivity2;
                        this.dataLeftMeanElectricalConductivity2.Text = shortFormReportModel.DataLeftMeanElectricalConductivity2;
                        shortFormReportModel.DataRightMeanElectricalConductivity2 = shortFormReport.DataRightMeanElectricalConductivity2;
                        this.dataRightMeanElectricalConductivity2.Text = shortFormReportModel.DataRightMeanElectricalConductivity2;
                        shortFormReportModel.DataMeanElectricalConductivity3 = shortFormReport.DataMeanElectricalConductivity3;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataMeanElectricalConductivity3))
                        {
                            this.dataMeanElectricalConductivity3.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataMeanElectricalConductivity3);
                        }
                        shortFormReportModel.DataLeftMeanElectricalConductivity3 = shortFormReport.DataLeftMeanElectricalConductivity3;
                        this.dataLeftMeanElectricalConductivity3.Text = shortFormReportModel.DataLeftMeanElectricalConductivity3;
                        shortFormReportModel.DataRightMeanElectricalConductivity3 = shortFormReport.DataRightMeanElectricalConductivity3;
                        this.dataRightMeanElectricalConductivity3.Text = shortFormReportModel.DataRightMeanElectricalConductivity3;
                        shortFormReportModel.DataLeftComparativeElectricalConductivity1 = shortFormReport.DataLeftComparativeElectricalConductivity1;
                        this.dataLeftComparativeElectricalConductivity1.Text = shortFormReportModel.DataLeftComparativeElectricalConductivity1;
                        shortFormReportModel.DataLeftComparativeElectricalConductivity2 = shortFormReport.DataLeftComparativeElectricalConductivity2;
                        this.dataLeftComparativeElectricalConductivity2.Text = shortFormReportModel.DataLeftComparativeElectricalConductivity2;
                        shortFormReportModel.DataLeftComparativeElectricalConductivity3 = shortFormReport.DataLeftComparativeElectricalConductivity3;
                        this.dataLeftComparativeElectricalConductivity3.Text = shortFormReportModel.DataLeftComparativeElectricalConductivity3;

                        shortFormReportModel.DataComparativeElectricalConductivity3 = shortFormReportModel.DataComparativeElectricalConductivity3;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataComparativeElectricalConductivity3))
                        {
                            this.dataComparativeElectricalConductivity3.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataComparativeElectricalConductivity3);
                        }

                        shortFormReportModel.DataDivergenceBetweenHistograms3 = shortFormReportModel.DataDivergenceBetweenHistograms3;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataDivergenceBetweenHistograms3))
                        {
                            this.dataDivergenceBetweenHistograms3.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataDivergenceBetweenHistograms3);
                        }

                        shortFormReportModel.DataLeftDivergenceBetweenHistograms1 = shortFormReport.DataLeftDivergenceBetweenHistograms1;
                        this.dataLeftDivergenceBetweenHistograms1.Text = shortFormReportModel.DataLeftDivergenceBetweenHistograms1;
                        shortFormReportModel.DataLeftDivergenceBetweenHistograms2 = shortFormReport.DataLeftDivergenceBetweenHistograms2;
                        this.dataLeftDivergenceBetweenHistograms2.Text = shortFormReportModel.DataLeftDivergenceBetweenHistograms2;
                        shortFormReportModel.DataLeftDivergenceBetweenHistograms3 = shortFormReport.DataLeftDivergenceBetweenHistograms3;
                        this.dataLeftDivergenceBetweenHistograms3.Text = shortFormReportModel.DataLeftDivergenceBetweenHistograms3;

                        shortFormReportModel.DataLeftPhaseElectricalConductivity = shortFormReport.DataLeftPhaseElectricalConductivity;
                        this.dataLeftPhaseElectricalConductivity.Text = shortFormReportModel.DataLeftPhaseElectricalConductivity;
                        shortFormReportModel.DataRightPhaseElectricalConductivity = shortFormReport.DataRightPhaseElectricalConductivity;
                        this.dataRightPhaseElectricalConductivity.Text = shortFormReportModel.DataRightPhaseElectricalConductivity;
                        shortFormReportModel.DataAgeElectricalConductivityReference = shortFormReport.DataAgeElectricalConductivityReference;
                        this.dataAgeElectricalConductivityReference.Text = shortFormReportModel.DataAgeElectricalConductivityReference;
                        shortFormReportModel.DataLeftAgeElectricalConductivity = shortFormReport.DataLeftAgeElectricalConductivity;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataLeftAgeElectricalConductivity))
                        {
                            this.dataLeftAgeElectricalConductivity.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataLeftAgeElectricalConductivity);
                        }
                        shortFormReportModel.DataRightAgeElectricalConductivity = shortFormReport.DataRightAgeElectricalConductivity;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataRightAgeElectricalConductivity))
                        {
                            this.dataRightAgeElectricalConductivity.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataRightAgeElectricalConductivity);
                        }
                        shortFormReportModel.DataExamConclusion = shortFormReport.DataExamConclusion;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataExamConclusion))
                        {
                            this.dataExamConclusion.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataExamConclusion);
                        }
                        shortFormReportModel.DataLeftMammaryGland = shortFormReport.DataLeftMammaryGland;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataLeftMammaryGland))
                        {
                            this.dataLeftMammaryGland.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataLeftMammaryGland);
                        }
                        shortFormReportModel.DataLeftAgeRelated = shortFormReport.DataLeftAgeRelated;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataLeftAgeRelated))
                        {
                            this.dataLeftAgeRelated.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataLeftAgeRelated);
                        }
                        shortFormReportModel.DataRightMammaryGland = shortFormReport.DataRightMammaryGland;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataRightMammaryGland))
                        {
                            this.dataRightMammaryGland.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataRightMammaryGland);
                        }
                        shortFormReportModel.DataRightAgeRelated = shortFormReport.DataRightAgeRelated;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataRightAgeRelated))
                        {
                            this.dataRightAgeRelated.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataRightAgeRelated);
                        }

                        shortFormReportModel.DataTotalPts = shortFormReport.DataTotalPts;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataTotalPts))
                        {
                            this.dataTotalPts.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataTotalPts);
                        }
                        shortFormReportModel.DataBiRadsCategory = shortFormReport.DataBiRadsCategory;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataBiRadsCategory))
                        {
                            this.dataBiRadsCategory.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataBiRadsCategory);
                        }
                        shortFormReportModel.DataFurtherExam = shortFormReport.DataFurtherExam;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataFurtherExam))
                        {
                            this.dataFurtherExam.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataFurtherExam);
                        }
                        shortFormReportModel.DataComments = shortFormReport.DataComments;
                        this.dataComments.Text = shortFormReportModel.DataComments;

                        MessageBox.Show(this, App.Current.FindResource("Message_27").ToString());
                        
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show(this, string.Format(App.Current.FindResource("Message_33").ToString(), docFile));
                }
            }
            else
            {
                MessageBox.Show(this, App.Current.FindResource("Message_26").ToString());
            }
        }

        private void btnAnalysis_Click(object sender, RoutedEventArgs e)
        {
            generatePoints();
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {            
            SaveReport(null);
        }

        private void generatePoints()
        {
            //計算診斷分數
            int leftBreast1 = 0;
            int leftBreast2 = 0;
            int leftBreast3 = 0;
            int rightBreast1 = 0;
            int rightBreast2 = 0;
            int rightBreast3 = 0;            
            int divergenceBetweenHistograms = 0;
            if (!string.IsNullOrEmpty(dataLeftDivergenceBetweenHistograms1.Text))
            {
                double val = Convert.ToDouble(dataLeftDivergenceBetweenHistograms1.Text.Replace("%",""));
                divergenceBetweenHistograms = val < 20 ? 0 : val < 30 ? 1 : val < 40 ? 2 : 3;
            }
            else if (!string.IsNullOrEmpty(dataLeftDivergenceBetweenHistograms2.Text))
            {
                double val = Convert.ToDouble(dataLeftDivergenceBetweenHistograms2.Text.Replace("%", ""));
                divergenceBetweenHistograms = val < 20 ? 0 : val < 30 ? 1 : val < 40 ? 2 : 3;
            }
            else if (!string.IsNullOrEmpty(dataLeftDivergenceBetweenHistograms3.Text))
            {
                double val = Convert.ToDouble(dataLeftDivergenceBetweenHistograms3.Text.Replace("%", ""));
                divergenceBetweenHistograms = val < 20 ? 0 : val < 30 ? 1 : val < 40 ? 2 : 3;
            }
            

            if (!string.IsNullOrEmpty(dataLeftBreastH.Text) && !string.IsNullOrEmpty(dataRightBreastH.Text))
            {
                if (dataLeftLocation.SelectedIndex > 0)
                {
                    leftBreast1 += dataLeftShape.SelectedIndex > 2 ? 2 : (dataLeftShape.SelectedIndex > 0 ? 1 : 0);
                    leftBreast1 += dataLeftContourAroundFocus.SelectedIndex > 2 ? 2 : (dataLeftContourAroundFocus.SelectedIndex > 1 ? 1 : 0);
                    leftBreast1 += dataLeftSurroundingTissues.SelectedIndex > 1 ? (dataLeftSurroundingTissues.SelectedIndex > 3 ? 2 : 1) : 0;
                    leftBreast1 += dataLeftInternalElectricalStructure.SelectedIndex > 1 ? dataLeftInternalElectricalStructure.SelectedIndex - 1 : 0;
                    leftBreast1 += divergenceBetweenHistograms;                      
                }
                if (dataRightLocation.SelectedIndex > 0)
                {
                    rightBreast1 += dataRightShape.SelectedIndex > 2 ? 2 : (dataRightShape.SelectedIndex > 0 ? 1 : 0);
                    rightBreast1 += dataRightContourAroundFocus.SelectedIndex > 2 ? 2 : (dataRightContourAroundFocus.SelectedIndex > 1 ? 1 : 0);                
                    rightBreast1 += dataRightSurroundingTissues.SelectedIndex > 1 ? (dataRightSurroundingTissues.SelectedIndex > 3 ? 2 : 1) : 0;
                    rightBreast1 += dataRightInternalElectricalStructure.SelectedIndex > 1 ? dataRightInternalElectricalStructure.SelectedIndex - 1 : 0;
                    rightBreast1 += divergenceBetweenHistograms;                
                }

                if (dataLeftLocation2.SelectedIndex > 0)
                {
                    leftBreast2 += dataLeftShape2.SelectedIndex > 2 ? 2 : (dataLeftShape2.SelectedIndex > 0 ? 1 : 0);
                    leftBreast2 += dataLeftContourAroundFocus2.SelectedIndex > 2 ? 2 : (dataLeftContourAroundFocus2.SelectedIndex > 1 ? 1 : 0);
                    leftBreast2 += dataLeftInternalElectricalStructure2.SelectedIndex > 1 ? dataLeftInternalElectricalStructure2.SelectedIndex - 1 : 0;
                    leftBreast2 += dataLeftSurroundingTissues2.SelectedIndex > 1 ? (dataLeftSurroundingTissues2.SelectedIndex > 3 ? 2 : 1) : 0;
                    leftBreast2 += divergenceBetweenHistograms;                
                }
                if (dataRightLocation2.SelectedIndex > 0)
                {
                    rightBreast2 += dataRightShape2.SelectedIndex > 2 ? 2 : (dataRightShape2.SelectedIndex > 0 ? 1 : 0);
                    rightBreast2 += dataRightContourAroundFocus2.SelectedIndex > 2 ? 2 : (dataRightContourAroundFocus2.SelectedIndex > 1 ? 1 : 0);
                    rightBreast2 += dataRightInternalElectricalStructure2.SelectedIndex > 1 ? dataRightInternalElectricalStructure2.SelectedIndex - 1 : 0;
                    rightBreast2 += dataRightSurroundingTissues2.SelectedIndex > 1 ? (dataRightSurroundingTissues2.SelectedIndex > 3 ? 2 : 1) : 0;
                    rightBreast2 += divergenceBetweenHistograms;                
                }

                if (dataLeftLocation3.SelectedIndex > 0)
                {
                    leftBreast3 += dataLeftShape3.SelectedIndex > 2 ? 2 : (dataLeftShape3.SelectedIndex > 0 ? 1 : 0);
                    leftBreast3 += dataLeftContourAroundFocus3.SelectedIndex > 2 ? 2 : (dataLeftContourAroundFocus3.SelectedIndex > 1 ? 1 : 0);
                    leftBreast3 += dataLeftInternalElectricalStructure3.SelectedIndex > 1 ? dataLeftInternalElectricalStructure3.SelectedIndex - 1 : 0;
                    leftBreast3 += dataLeftSurroundingTissues3.SelectedIndex > 1 ? (dataLeftSurroundingTissues3.SelectedIndex > 3 ? 2 : 1) : 0;
                    leftBreast3 += divergenceBetweenHistograms;                
                }
                if (dataRightLocation3.SelectedIndex > 0)
                {
                    rightBreast3 += dataRightShape3.SelectedIndex > 2 ? 2 : (dataRightShape3.SelectedIndex > 0 ? 1 : 0);
                    rightBreast3 += dataRightContourAroundFocus3.SelectedIndex > 2 ? 2 : (dataRightContourAroundFocus3.SelectedIndex > 1 ? 1 : 0);
                    rightBreast3 += dataRightInternalElectricalStructure3.SelectedIndex > 1 ? dataRightInternalElectricalStructure3.SelectedIndex - 1 : 0;
                    rightBreast3 += dataRightSurroundingTissues3.SelectedIndex > 1 ? (dataRightSurroundingTissues3.SelectedIndex > 3 ? 2 : 1) : 0;
                    rightBreast3 += divergenceBetweenHistograms;                
                }

                List<int> list = new List<int>();
                list.Add(leftBreast1);
                list.Add(leftBreast2);
                list.Add(leftBreast3);
                list.Add(rightBreast1);
                list.Add(rightBreast2);
                list.Add(rightBreast3);
                list.Sort();

                int maxPoints = list[5];
                shortFormReportModel.DataTotalPts = maxPoints > 8 ? "8" : maxPoints == 0 ? "1" : maxPoints.ToString();
                this.dataTotalPts.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataTotalPts);
                shortFormReportModel.DataBiRadsCategory = maxPoints < 2 ? "2" : maxPoints < 4 ? "3" : maxPoints < 5 ? "4" : maxPoints < 8 ? "5" : "6";
                this.dataBiRadsCategory.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataBiRadsCategory);
                
            }
            else
            {
                shortFormReportModel.DataTotalPts = "0";
                this.dataTotalPts.SelectedIndex = 0;
                shortFormReportModel.DataBiRadsCategory = "1";
                this.dataBiRadsCategory.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataBiRadsCategory);
            }                         

        }

        private void btnSignature_Click(object sender, RoutedEventArgs e)
        {
            SignatureBox signBox = new SignatureBox();
            signBox.Owner = this;
            //signBox.callbackMethod = ShowSignature;
            signBox.ShowDialog();
        }

        /// <summary>
        /// 保存报表数据到文件
        /// </summary>
        private void SaveReport(string otherDataFolder)
        {
            try
            {                
                var person = this.CodeListBox.SelectedItem as Person; 
                //保存到患者档案目录
                string datafile = null;
                if (!string.IsNullOrEmpty(otherDataFolder))
                {
                    datafile = otherDataFolder + System.IO.Path.DirectorySeparatorChar + person.Code + ".dat";
                }
                else
                {
                    //保存到用户档案目录                    
                    datafile = person.ArchiveFolder + System.IO.Path.DirectorySeparatorChar + person.Code + ".dat";
                }
                LoadDataModel();

                if (string.IsNullOrEmpty(shortFormReportModel.DataSignDate))
                {
                    if ("en-US".Equals(App.local))
                    {
                        shortFormReportModel.DataSignDate = DateTime.Today.ToString("MMMM d, yyyy");
                    }
                    else
                    {
                        shortFormReportModel.DataSignDate = DateTime.Today.ToString("yyyy年MM月dd日");
                    }
                }
                SerializeUtilities.Serialize<ShortFormReport>(shortFormReportModel, datafile);                           

                MessageBox.Show(this, App.Current.FindResource("Message_2").ToString());
            }
            catch (Exception ex)
            {                
                MessageBox.Show(this, App.Current.FindResource("Message_3").ToString() + ex.Message);
            }
        }
        

        /// <summary>
        /// 加载报表数据到模型对象
        /// </summary>
        private void LoadDataModel()
        {
            var person = this.CodeListBox.SelectedItem as Person; 
            //shortFormReportModel.DataClientNum = this.dataClientNum.Text;
            //shortFormReportModel.DataUserCode = this.dataUserCode.Text;
            //shortFormReportModel.DataAge = this.dataAge.Text;            
            //shortFormReportModel.DataName = this.dataName.Text;
            //shortFormReportModel.DataScreenDate = this.dataScreenDate.Text;
            //shortFormReportModel.DataScreenLocation = this.dataScreenLocation.Text;
            //shortFormReportModel.DataScreenLocation = person.ScreenVenue;
            //shortFormReportModel.DataAddress = this.dataAddress.Text;
            //shortFormReportModel.DataAddress = person.Address;
            //shortFormReportModel.DataGender = this.dataGender.SelectedIndex.ToString();
            //shortFormReportModel.DataHealthCard = this.dataHealthCard.Text;
            //shortFormReportModel.DataWeight = this.dataWeight.Text;
            //shortFormReportModel.DataWeightUnit = this.dataWeightUnit.SelectedIndex.ToString();
            shortFormReportModel.DataMenstrualCycle = this.dataMenstrualCycle.SelectedIndex.ToString();
            shortFormReportModel.DataHormones = this.dataHormones.Text;
            shortFormReportModel.DataSkinAffections = this.dataSkinAffections.SelectedIndex.ToString();                       
            shortFormReportModel.DataMotherUltra = this.dataMotherUltra.SelectedIndex.ToString();

            shortFormReportModel.DataLeftBreastH = this.dataLeftBreastH.Text;
            shortFormReportModel.DataRightBreastH = this.dataRightBreastH.Text;
            shortFormReportModel.DataLeftBreastM = this.dataLeftBreastM.Text;
            shortFormReportModel.DataRightBreastM = this.dataRightBreastM.Text;
            shortFormReportModel.DataLeftBreastAP = this.dataLeftBreastAP.SelectedIndex.ToString();
            shortFormReportModel.DataRightBreastAP = this.dataRightBreastAP.SelectedIndex.ToString();
            shortFormReportModel.DataLeftBreast = this.dataLeftBreastH.Text + ":" + this.dataLeftBreastM.Text + this.dataLeftBreastAP.Text;
            shortFormReportModel.DataRightBreast = this.dataRightBreastH.Text + ":" + this.dataRightBreastM.Text + this.dataRightBreastAP.Text;

            shortFormReportModel.DataLeftPalpableMass = this.dataLeftPalpableMass.SelectedIndex.ToString();
            shortFormReportModel.DataRightPalpableMass = this.dataRightPalpableMass.SelectedIndex.ToString();
            shortFormReportModel.DataLeftChangesOfElectricalConductivity = this.dataLeftChangesOfElectricalConductivity.SelectedIndex.ToString();
            shortFormReportModel.DataRightChangesOfElectricalConductivity = this.dataRightChangesOfElectricalConductivity.SelectedIndex.ToString();
            shortFormReportModel.DataLeftMammaryStruct = this.dataLeftMammaryStruct.SelectedIndex.ToString();
            shortFormReportModel.DataRightMammaryStruct = this.dataRightMammaryStruct.SelectedIndex.ToString();
            shortFormReportModel.DataLeftLactiferousSinusZone = this.dataLeftLactiferousSinusZone.SelectedIndex.ToString();
            shortFormReportModel.DataRightLactiferousSinusZone = this.dataRightLactiferousSinusZone.SelectedIndex.ToString();
            shortFormReportModel.DataLeftMammaryContour = this.dataLeftMammaryContour.SelectedIndex.ToString();
            shortFormReportModel.DataRightMammaryContour = this.dataRightMammaryContour.SelectedIndex.ToString();

            shortFormReportModel.DataLeftNumber = this.dataLeftNumber.SelectedIndex.ToString();
            shortFormReportModel.DataRightNumber = this.dataRightNumber.SelectedIndex.ToString();

            shortFormReportModel.DataLeftLocation = this.dataLeftLocation.SelectedIndex.ToString();
            shortFormReportModel.DataRightLocation = this.dataRightLocation.SelectedIndex.ToString();
            shortFormReportModel.DataLeftSize = this.dataLeftSize.Text;
            shortFormReportModel.DataRightSize = this.dataRightSize.Text;            
            shortFormReportModel.DataLeftShape = this.dataLeftShape.SelectedIndex.ToString();
            shortFormReportModel.DataRightShape = this.dataRightShape.SelectedIndex.ToString();
            shortFormReportModel.DataLeftContourAroundFocus = this.dataLeftContourAroundFocus.SelectedIndex.ToString();
            shortFormReportModel.DataRightContourAroundFocus = this.dataRightContourAroundFocus.SelectedIndex.ToString();
            shortFormReportModel.DataLeftInternalElectricalStructure = this.dataLeftInternalElectricalStructure.SelectedIndex.ToString();
            shortFormReportModel.DataRightInternalElectricalStructure = this.dataRightInternalElectricalStructure.SelectedIndex.ToString();
            shortFormReportModel.DataLeftSurroundingTissues = this.dataLeftSurroundingTissues.SelectedIndex.ToString();
            shortFormReportModel.DataRightSurroundingTissues = this.dataRightSurroundingTissues.SelectedIndex.ToString();

            shortFormReportModel.DataLeftLocation2 = this.dataLeftLocation2.SelectedIndex.ToString();
            shortFormReportModel.DataRightLocation2 = this.dataRightLocation2.SelectedIndex.ToString();
            shortFormReportModel.DataLeftSize2 = this.dataLeftSize2.Text;            
            shortFormReportModel.DataRightSize2 = this.dataRightSize2.Text;            
            shortFormReportModel.DataLeftShape2 = this.dataLeftShape2.SelectedIndex.ToString();
            shortFormReportModel.DataRightShape2 = this.dataRightShape2.SelectedIndex.ToString();
            shortFormReportModel.DataLeftContourAroundFocus2 = this.dataLeftContourAroundFocus2.SelectedIndex.ToString();
            shortFormReportModel.DataRightContourAroundFocus2 = this.dataRightContourAroundFocus2.SelectedIndex.ToString();
            shortFormReportModel.DataLeftInternalElectricalStructure2 = this.dataLeftInternalElectricalStructure2.SelectedIndex.ToString();
            shortFormReportModel.DataRightInternalElectricalStructure2 = this.dataRightInternalElectricalStructure2.SelectedIndex.ToString();
            shortFormReportModel.DataLeftSurroundingTissues2 = this.dataLeftSurroundingTissues2.SelectedIndex.ToString();
            shortFormReportModel.DataRightSurroundingTissues2 = this.dataRightSurroundingTissues2.SelectedIndex.ToString();

            shortFormReportModel.DataLeftLocation3 = this.dataLeftLocation3.SelectedIndex.ToString();
            shortFormReportModel.DataRightLocation3 = this.dataRightLocation3.SelectedIndex.ToString();
            shortFormReportModel.DataLeftSize3 = this.dataLeftSize3.Text;
            shortFormReportModel.DataRightSize3 = this.dataRightSize3.Text;;            
            shortFormReportModel.DataLeftShape3 = this.dataLeftShape3.SelectedIndex.ToString();
            shortFormReportModel.DataRightShape3 = this.dataRightShape3.SelectedIndex.ToString();
            shortFormReportModel.DataLeftContourAroundFocus3 = this.dataLeftContourAroundFocus3.SelectedIndex.ToString();
            shortFormReportModel.DataRightContourAroundFocus3 = this.dataRightContourAroundFocus3.SelectedIndex.ToString();
            shortFormReportModel.DataLeftInternalElectricalStructure3 = this.dataLeftInternalElectricalStructure3.SelectedIndex.ToString();
            shortFormReportModel.DataRightInternalElectricalStructure3 = this.dataRightInternalElectricalStructure3.SelectedIndex.ToString();
            shortFormReportModel.DataLeftSurroundingTissues3 = this.dataLeftSurroundingTissues3.SelectedIndex.ToString();
            shortFormReportModel.DataRightSurroundingTissues3 = this.dataRightSurroundingTissues3.SelectedIndex.ToString();

            shortFormReportModel.DataLeftOncomarkerHighlightBenignChanges = this.dataLeftOncomarkerHighlightBenignChanges.SelectedIndex.ToString();
            shortFormReportModel.DataRightOncomarkerHighlightBenignChanges = this.dataRightOncomarkerHighlightBenignChanges.SelectedIndex.ToString();
            shortFormReportModel.DataLeftOncomarkerHighlightSuspiciousChanges = this.dataLeftOncomarkerHighlightSuspiciousChanges.SelectedIndex.ToString();
            shortFormReportModel.DataRightOncomarkerHighlightSuspiciousChanges = this.dataRightOncomarkerHighlightSuspiciousChanges.SelectedIndex.ToString();
            shortFormReportModel.DataLeftMeanElectricalConductivity1 = this.dataLeftMeanElectricalConductivity1.Text;
            //shortFormReportModel.DataLeftMeanElectricalConductivity1N1 = this.dataLeftMeanElectricalConductivity1N1.Text;
            //shortFormReportModel.DataLeftMeanElectricalConductivity1N2 = this.dataLeftMeanElectricalConductivity1N2.Text;
            shortFormReportModel.DataRightMeanElectricalConductivity1 = this.dataRightMeanElectricalConductivity1.Text;
            //shortFormReportModel.DataRightMeanElectricalConductivity1N1 = this.dataRightMeanElectricalConductivity1N1.Text;
            //shortFormReportModel.DataRightMeanElectricalConductivity1N2 = this.dataRightMeanElectricalConductivity1N2.Text;
            if (!string.IsNullOrEmpty(shortFormReportModel.DataLeftMeanElectricalConductivity1))
            {
                shortFormReportModel.DataLeftMeanECOfLesion = shortFormReportModel.DataLeftMeanElectricalConductivity1;
            }
            if (!string.IsNullOrEmpty(shortFormReportModel.DataRightMeanElectricalConductivity1))
            {
                shortFormReportModel.DataRightMeanECOfLesion = shortFormReportModel.DataRightMeanElectricalConductivity1;
            }

            shortFormReportModel.DataLeftMeanElectricalConductivity2 = this.dataLeftMeanElectricalConductivity2.Text;
            //shortFormReportModel.DataLeftMeanElectricalConductivity2N1 = this.dataLeftMeanElectricalConductivity2N1.Text;
            //shortFormReportModel.DataLeftMeanElectricalConductivity2N2 = this.dataLeftMeanElectricalConductivity2N2.Text;
            shortFormReportModel.DataRightMeanElectricalConductivity2 = this.dataRightMeanElectricalConductivity2.Text;
            //shortFormReportModel.DataRightMeanElectricalConductivity2N1 = this.dataRightMeanElectricalConductivity2N1.Text;
            //shortFormReportModel.DataRightMeanElectricalConductivity2N2 = this.dataRightMeanElectricalConductivity2N2.Text;
            if (!string.IsNullOrEmpty(shortFormReportModel.DataLeftMeanElectricalConductivity2))
            {
                shortFormReportModel.DataLeftMeanECOfLesion = shortFormReportModel.DataLeftMeanElectricalConductivity2;
            }
            if (!string.IsNullOrEmpty(shortFormReportModel.DataRightMeanElectricalConductivity2))
            {
                shortFormReportModel.DataRightMeanECOfLesion = shortFormReportModel.DataRightMeanElectricalConductivity2;
            }

            shortFormReportModel.DataMeanElectricalConductivity3 = this.dataMeanElectricalConductivity3.SelectedIndex.ToString();
            shortFormReportModel.DataLeftMeanElectricalConductivity3 = this.dataLeftMeanElectricalConductivity3.Text;
            //shortFormReportModel.DataLeftMeanElectricalConductivity3N1 = this.dataLeftMeanElectricalConductivity3N1.Text;
            //shortFormReportModel.DataLeftMeanElectricalConductivity3N2 = this.dataLeftMeanElectricalConductivity3N2.Text;
            shortFormReportModel.DataRightMeanElectricalConductivity3 = this.dataRightMeanElectricalConductivity3.Text;
            //shortFormReportModel.DataRightMeanElectricalConductivity3N1 = this.dataRightMeanElectricalConductivity3N1.Text;
            //shortFormReportModel.DataRightMeanElectricalConductivity3N2 = this.dataRightMeanElectricalConductivity3N2.Text;
            if (!string.IsNullOrEmpty(shortFormReportModel.DataLeftMeanElectricalConductivity3))
            {
                shortFormReportModel.DataLeftMeanECOfLesion = shortFormReportModel.DataLeftMeanElectricalConductivity3;
            }
            if (!string.IsNullOrEmpty(shortFormReportModel.DataRightMeanElectricalConductivity3))
            {
                shortFormReportModel.DataRightMeanECOfLesion = shortFormReportModel.DataRightMeanElectricalConductivity3;
            }


            //shortFormReportModel.DataComparativeElectricalConductivityReference1 = this.dataComparativeElectricalConductivityReference1.Text;
            shortFormReportModel.DataLeftComparativeElectricalConductivity1 = this.dataLeftComparativeElectricalConductivity1.Text;
            //shortFormReportModel.DataRightComparativeElectricalConductivity1 = this.dataRightComparativeElectricalConductivity1.Text;
            //shortFormReportModel.DataComparativeElectricalConductivityReference2 = this.dataComparativeElectricalConductivityReference2.Text;
            shortFormReportModel.DataLeftComparativeElectricalConductivity2 = this.dataLeftComparativeElectricalConductivity2.Text;
            //shortFormReportModel.DataRightComparativeElectricalConductivity2 = this.dataRightComparativeElectricalConductivity2.Text;
            shortFormReportModel.DataComparativeElectricalConductivity3 = this.dataComparativeElectricalConductivity3.SelectedIndex.ToString();
            shortFormReportModel.DataLeftComparativeElectricalConductivity3 = this.dataLeftComparativeElectricalConductivity3.Text;
            //shortFormReportModel.DataRightComparativeElectricalConductivity3 = this.dataRightComparativeElectricalConductivity3.Text;

            //shortFormReportModel.DataDivergenceBetweenHistogramsReference1 = this.dataDivergenceBetweenHistogramsReference1.Text;
            shortFormReportModel.DataLeftDivergenceBetweenHistograms1 = this.dataLeftDivergenceBetweenHistograms1.Text;
            //shortFormReportModel.DataRightDivergenceBetweenHistograms1 = this.dataRightDivergenceBetweenHistograms1.Text;
            //shortFormReportModel.DataDivergenceBetweenHistogramsReference2 = this.dataDivergenceBetweenHistogramsReference2.Text;
            shortFormReportModel.DataLeftDivergenceBetweenHistograms2 = this.dataLeftDivergenceBetweenHistograms2.Text;
            //shortFormReportModel.DataRightDivergenceBetweenHistograms2 = this.dataRightDivergenceBetweenHistograms2.Text;
            shortFormReportModel.DataDivergenceBetweenHistograms3 = this.dataDivergenceBetweenHistograms3.SelectedIndex.ToString();
            shortFormReportModel.DataLeftDivergenceBetweenHistograms3 = this.dataLeftDivergenceBetweenHistograms3.Text;
            //shortFormReportModel.DataRightDivergenceBetweenHistograms3 = this.dataRightDivergenceBetweenHistograms3.Text;

            shortFormReportModel.DataLeftComparisonWithNorm = this.dataLeftComparisonWithNorm.Text;
            shortFormReportModel.DataRightComparisonWithNorm = this.dataRightComparisonWithNorm.Text;

            //shortFormReportModel.DataPhaseElectricalConductivityReference = this.dataPhaseElectricalConductivityReference.Text;
            shortFormReportModel.DataLeftPhaseElectricalConductivity = this.dataLeftPhaseElectricalConductivity.Text;
            shortFormReportModel.DataRightPhaseElectricalConductivity = this.dataRightPhaseElectricalConductivity.Text;

            shortFormReportModel.DataAgeElectricalConductivityReference = this.dataAgeElectricalConductivityReference.Text;
            shortFormReportModel.DataLeftAgeElectricalConductivity = this.dataLeftAgeElectricalConductivity.SelectedIndex.ToString();
            shortFormReportModel.DataRightAgeElectricalConductivity = this.dataRightAgeElectricalConductivity.SelectedIndex.ToString();
            //shortFormReportModel.DataAgeValueOfEC = this.dataAgeValueOfEC.SelectedIndex.ToString();

            shortFormReportModel.DataExamConclusion = this.dataExamConclusion.SelectedIndex.ToString();
            shortFormReportModel.DataLeftMammaryGland = this.dataLeftMammaryGland.SelectedIndex.ToString();
            shortFormReportModel.DataLeftAgeRelated = this.dataLeftAgeRelated.SelectedIndex.ToString();
            //shortFormReportModel.DataLeftMeanECOfLesion = this.dataLeftMeanECOfLesion.Text;
            //shortFormReportModel.DataLeftFindings = this.dataLeftFindings.SelectedIndex.ToString();
            //shortFormReportModel.DataLeftMammaryGlandResult = this.dataLeftMammaryGlandResult.SelectedIndex.ToString();

            shortFormReportModel.DataRightMammaryGland = this.dataRightMammaryGland.SelectedIndex.ToString();
            shortFormReportModel.DataRightAgeRelated = this.dataRightAgeRelated.SelectedIndex.ToString();
            //shortFormReportModel.DataRightMeanECOfLesion = this.dataRightMeanECOfLesion.Text;
            //shortFormReportModel.DataRightFindings = this.dataRightFindings.SelectedIndex.ToString();
            //shortFormReportModel.DataRightMammaryGlandResult = this.dataRightMammaryGlandResult.SelectedIndex.ToString();

            shortFormReportModel.DataTotalPts = this.dataTotalPts.SelectedIndex.ToString();
            //shortFormReportModel.DataPoint = this.dataPoint.SelectedIndex.ToString();

            shortFormReportModel.DataBiRadsCategory = this.dataBiRadsCategory.SelectedIndex.ToString();
            shortFormReportModel.DataRecommendation = this.dataRecommendation.SelectedIndex.ToString();
            shortFormReportModel.DataComments = this.dataComments.Text;
            shortFormReportModel.DataConclusion = this.dataConclusion.SelectedIndex.ToString();
            shortFormReportModel.DataFurtherExam = this.dataFurtherExam.SelectedIndex.ToString();
            //shortFormReportModel.DataConclusion2 = this.dataConclusion2.Text;
            if (this.dataDoctor.SelectedItem != null)
            {
                var doctor = this.dataDoctor.SelectedItem as User;
                shortFormReportModel.DataDoctor = doctor.Name;
                shortFormReportModel.DataDoctorLicense = doctor.License;
            }

            var signImg = this.dataSignImg.Source as BitmapImage;
            if (signImg != null)
            {
                var stream = signImg.StreamSource;
                stream.Position = 0;
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                stream.Flush();
                //stream.Close();
                shortFormReportModel.DataSignImg = buffer;
            }

            App.reportSettingModel.ShowTechSignature = person.ShowTechSignature;

        }

        /// <summary>
        /// 克隆数据模型对象
        /// </summary>
        private ShortFormReport CloneReportModel()
        {
            ShortFormReport reportModel = shortFormReportModel.Clone();
            reportModel = shortFormReportModel.Clone();
            //reportModel.DataGender = this.dataGender.Text;
            //reportModel.DataWeightUnit = this.dataWeightUnit.Text;
            reportModel.DataMenstrualCycle = this.dataMenstrualCycle.Text;
            reportModel.DataSkinAffections = this.dataSkinAffections.Text;
            //reportModel.DataPertinentHistory = this.dataPertinentHistory.Text;
            //reportModel.DataMotherUltra = this.dataMotherUltra.Text;
            //reportModel.DataLeftPalpableMass = this.dataLeftPalpableMass.Text;
            //reportModel.DataRightPalpableMass = this.dataRightPalpableMass.Text;

            reportModel.DataLeftBreastAP = this.dataLeftBreastAP.Text;
            reportModel.DataRightBreastAP = this.dataRightBreastAP.Text;
            reportModel.DataLeftBreast = this.dataLeftBreastH.Text + ":" + this.dataLeftBreastM.Text +" "+ this.dataLeftBreastAP.Text;
            reportModel.DataRightBreast = this.dataLeftBreastH.Text + ":" + this.dataRightBreastM.Text + " " + this.dataRightBreastAP.Text;

            reportModel.DataLeftChangesOfElectricalConductivity = this.dataLeftChangesOfElectricalConductivity.Text;
            reportModel.DataRightChangesOfElectricalConductivity = this.dataRightChangesOfElectricalConductivity.Text;
            reportModel.DataLeftMammaryStruct = this.dataLeftMammaryStruct.Text;
            reportModel.DataRightMammaryStruct = this.dataRightMammaryStruct.Text;
            reportModel.DataLeftLactiferousSinusZone = this.dataLeftLactiferousSinusZone.Text;
            reportModel.DataRightLactiferousSinusZone = this.dataRightLactiferousSinusZone.Text;
            reportModel.DataLeftMammaryContour = this.dataLeftMammaryContour.Text;
            reportModel.DataRightMammaryContour = this.dataRightMammaryContour.Text;
            reportModel.DataLeftNumber = this.dataLeftNumber.Text;
            reportModel.DataRightNumber = this.dataRightNumber.Text;

            reportModel.DataLeftLocation = this.dataLeftLocation.Text;
            reportModel.DataRightLocation = this.dataRightLocation.Text;
            reportModel.DataLeftLocation2 = this.dataLeftLocation2.Text;
            reportModel.DataRightLocation2 = this.dataRightLocation2.Text;
            reportModel.DataLeftLocation3 = this.dataLeftLocation3.Text;
            reportModel.DataRightLocation3 = this.dataRightLocation3.Text;

            reportModel.DataLeftSize = string.IsNullOrEmpty(this.dataLeftSize.Text) ? "" : this.dataLeftSize.Text + " mm";
            reportModel.DataRightSize = string.IsNullOrEmpty(this.dataRightSize.Text) ? "" : this.dataRightSize.Text + " mm";
            reportModel.DataLeftSize2 = string.IsNullOrEmpty(this.dataLeftSize2.Text) ? "" : this.dataLeftSize2.Text + " mm";
            reportModel.DataRightSize2 = string.IsNullOrEmpty(this.dataRightSize2.Text) ? "" : this.dataRightSize2.Text + " mm";
            reportModel.DataLeftSize3 = string.IsNullOrEmpty(this.dataLeftSize3.Text) ? "" : this.dataLeftSize3.Text + " mm";
            reportModel.DataRightSize3 = string.IsNullOrEmpty(this.dataRightSize3.Text) ? "":this.dataRightSize3.Text +" mm";

            reportModel.DataLeftShape = this.dataLeftShape.Text;
            reportModel.DataRightShape = this.dataRightShape.Text;
            reportModel.DataLeftShape2 = this.dataLeftShape2.Text;
            reportModel.DataRightShape2 = this.dataRightShape2.Text;
            reportModel.DataLeftShape3 = this.dataLeftShape3.Text;
            reportModel.DataRightShape3 = this.dataRightShape3.Text;

            reportModel.DataLeftContourAroundFocus = this.dataLeftContourAroundFocus.Text;
            reportModel.DataRightContourAroundFocus = this.dataRightContourAroundFocus.Text;
            reportModel.DataLeftContourAroundFocus2 = this.dataLeftContourAroundFocus2.Text;
            reportModel.DataRightContourAroundFocus2 = this.dataRightContourAroundFocus2.Text;
            reportModel.DataLeftContourAroundFocus3 = this.dataLeftContourAroundFocus3.Text;
            reportModel.DataRightContourAroundFocus3 = this.dataRightContourAroundFocus3.Text;

            reportModel.DataLeftInternalElectricalStructure = this.dataLeftInternalElectricalStructure.Text;
            reportModel.DataRightInternalElectricalStructure = this.dataRightInternalElectricalStructure.Text;
            reportModel.DataLeftInternalElectricalStructure2 = this.dataLeftInternalElectricalStructure2.Text;
            reportModel.DataRightInternalElectricalStructure2 = this.dataRightInternalElectricalStructure2.Text;
            reportModel.DataLeftInternalElectricalStructure3 = this.dataLeftInternalElectricalStructure3.Text;
            reportModel.DataRightInternalElectricalStructure3 = this.dataRightInternalElectricalStructure3.Text;

            reportModel.DataLeftSurroundingTissues = this.dataLeftSurroundingTissues.Text;
            reportModel.DataRightSurroundingTissues = this.dataRightSurroundingTissues.Text;
            reportModel.DataLeftSurroundingTissues2 = this.dataLeftSurroundingTissues2.Text;
            reportModel.DataRightSurroundingTissues2 = this.dataRightSurroundingTissues2.Text;
            reportModel.DataLeftSurroundingTissues3 = this.dataLeftSurroundingTissues3.Text;
            reportModel.DataRightSurroundingTissues3 = this.dataRightSurroundingTissues3.Text;

            reportModel.DataLeftOncomarkerHighlightBenignChanges = this.dataLeftOncomarkerHighlightBenignChanges.Text;
            reportModel.DataRightOncomarkerHighlightBenignChanges = this.dataRightOncomarkerHighlightBenignChanges.Text;
            reportModel.DataLeftOncomarkerHighlightSuspiciousChanges = this.dataLeftOncomarkerHighlightSuspiciousChanges.Text;
            reportModel.DataRightOncomarkerHighlightSuspiciousChanges = this.dataRightOncomarkerHighlightSuspiciousChanges.Text;
            reportModel.DataMeanElectricalConductivity3 = this.dataMeanElectricalConductivity3.Text;
            reportModel.DataComparativeElectricalConductivity3 = this.dataComparativeElectricalConductivity3.Text;
            reportModel.DataDivergenceBetweenHistograms3 = this.dataDivergenceBetweenHistograms3.Text;
            reportModel.DataLeftAgeElectricalConductivity = this.dataLeftAgeElectricalConductivity.Text;
            reportModel.DataRightAgeElectricalConductivity = this.dataRightAgeElectricalConductivity.Text;
            //reportModel.DataAgeValueOfEC = this.dataAgeValueOfEC.Text;
            reportModel.DataExamConclusion = this.dataExamConclusion.Text;
            reportModel.DataLeftMammaryGland = this.dataLeftMammaryGland.Text;
            reportModel.DataLeftAgeRelated = this.dataLeftAgeRelated.Text;
            //reportModel.DataLeftFindings = this.dataLeftFindings.Text;
            reportModel.DataLeftMammaryGlandResult = this.dataLeftMammaryGlandResult.Text;
            reportModel.DataRightMammaryGland = this.dataRightMammaryGland.Text;
            reportModel.DataRightAgeRelated = this.dataRightAgeRelated.Text;
            //reportModel.DataRightFindings = this.dataRightFindings.Text;
            reportModel.DataRightMammaryGlandResult = this.dataRightMammaryGlandResult.Text;
            reportModel.DataTotalPts = this.dataTotalPts.Text;
            //reportModel.DataLeftTotalPts = this.dataLeftTotalPts.Text;
            //reportModel.DataRightTotalPts = this.dataRightTotalPts.Text;
            //reportModel.DataPoint = this.dataPoint.Text;

            reportModel.DataBiRadsCategory = this.dataBiRadsCategory.Text;

            if (this.dataBiRadsCategory.Text.StartsWith("0"))
            {
                reportModel.DataPoint="";
            }
            else if (this.dataBiRadsCategory.Text.StartsWith("1"))
            {
                reportModel.DataPoint=App.Current.FindResource("ReportContext_144").ToString();
            }
            else if (this.dataBiRadsCategory.Text.StartsWith("2"))
            {
                reportModel.DataPoint=App.Current.FindResource("ReportContext_145").ToString();
            }
            else if (this.dataBiRadsCategory.Text.StartsWith("3"))
            {
                reportModel.DataPoint=App.Current.FindResource("ReportContext_146").ToString();
            }
            else if (this.dataBiRadsCategory.Text.StartsWith("4"))
            {
                reportModel.DataPoint=App.Current.FindResource("ReportContext_147").ToString();
            }
            else if (this.dataBiRadsCategory.Text.StartsWith("5"))
            {
                reportModel.DataPoint=App.Current.FindResource("ReportContext_148").ToString();
            }
            

            
            //reportModel.DataLeftBiRadsCategory = this.dataLeftBiRadsCategory.Text;
            //reportModel.DataRightBiRadsCategory = this.dataRightBiRadsCategory.Text;
            reportModel.DataRecommendation = this.dataRecommendation.Text;
            reportModel.DataConclusion = this.dataConclusion.Text;
            reportModel.DataFurtherExam = this.dataFurtherExam.Text;


            return reportModel;
        }

        private ShortFormReport CloneClientReportModel()
        {
            ShortFormReport reportModel = shortFormReportModel.Clone();
            reportModel = shortFormReportModel.Clone();
            //reportModel.DataGender = this.dataGender.Text;
            //reportModel.DataWeightUnit = this.dataWeightUnit.Text;
            reportModel.DataMenstrualCycle = this.dataMenstrualCycle.Text;
            reportModel.DataSkinAffections = this.dataSkinAffections.Text;
            //reportModel.DataPertinentHistory = this.dataPertinentHistory.Text;
            //reportModel.DataMotherUltra = this.dataMotherUltra.Text;
            //reportModel.DataLeftPalpableMass = this.dataLeftPalpableMass.Text;
            //reportModel.DataRightPalpableMass = this.dataRightPalpableMass.Text;

            reportModel.DataLeftBreastAP = this.dataLeftBreastAP.Text;
            reportModel.DataRightBreastAP = this.dataRightBreastAP.Text;
            reportModel.DataLeftBreast = this.dataLeftBreastH.Text + ":" + this.dataLeftBreastM.Text + " " + this.dataLeftBreastAP.Text;
            reportModel.DataRightBreast = this.dataLeftBreastH.Text + ":" + this.dataRightBreastM.Text + " " + this.dataRightBreastAP.Text;

            reportModel.DataLeftChangesOfElectricalConductivity = this.dataLeftChangesOfElectricalConductivity.Text;
            reportModel.DataRightChangesOfElectricalConductivity = this.dataRightChangesOfElectricalConductivity.Text;
            reportModel.DataLeftMammaryStruct = this.dataLeftMammaryStruct.Text;
            reportModel.DataRightMammaryStruct = this.dataRightMammaryStruct.Text;
            reportModel.DataLeftLactiferousSinusZone = this.dataLeftLactiferousSinusZone.Text;
            reportModel.DataRightLactiferousSinusZone = this.dataRightLactiferousSinusZone.Text;
            reportModel.DataLeftMammaryContour = this.dataLeftMammaryContour.Text;
            reportModel.DataRightMammaryContour = this.dataRightMammaryContour.Text;
            reportModel.DataLeftNumber = this.dataLeftNumber.Text;
            reportModel.DataRightNumber = this.dataRightNumber.Text;

            reportModel.DataLeftLocation = this.dataLeftLocation.Text;
            reportModel.DataRightLocation = this.dataRightLocation.Text;
            reportModel.DataLeftLocation2 = this.dataLeftLocation2.Text;
            reportModel.DataRightLocation2 = this.dataRightLocation2.Text;
            reportModel.DataLeftLocation3 = this.dataLeftLocation3.Text;
            reportModel.DataRightLocation3 = this.dataRightLocation3.Text;

            reportModel.DataLeftSize = string.IsNullOrEmpty(this.dataLeftSize.Text) ? "" : this.dataLeftSize.Text + " mm";
            reportModel.DataRightSize = string.IsNullOrEmpty(this.dataRightSize.Text) ? "" : this.dataRightSize.Text + " mm";
            reportModel.DataLeftSize2 = string.IsNullOrEmpty(this.dataLeftSize2.Text) ? "" : this.dataLeftSize2.Text + " mm";
            reportModel.DataRightSize2 = string.IsNullOrEmpty(this.dataRightSize2.Text) ? "" : this.dataRightSize2.Text + " mm";
            reportModel.DataLeftSize3 = string.IsNullOrEmpty(this.dataLeftSize3.Text) ? "" : this.dataLeftSize3.Text + " mm";
            reportModel.DataRightSize3 = string.IsNullOrEmpty(this.dataRightSize3.Text) ? "" : this.dataRightSize3.Text + " mm";

            reportModel.DataLeftShape = getSharpStr(reportModel.DataLeftShape);
            reportModel.DataRightShape = getSharpStr(reportModel.DataRightShape);
            reportModel.DataLeftShape2 = getSharpStr(reportModel.DataLeftShape2);
            reportModel.DataRightShape2 = getSharpStr(reportModel.DataRightShape2);
            reportModel.DataLeftShape3 = getSharpStr(reportModel.DataLeftShape3);
            reportModel.DataRightShape3 = getSharpStr(reportModel.DataRightShape3);

            reportModel.DataLeftContourAroundFocus = this.dataLeftContourAroundFocus.Text;
            reportModel.DataRightContourAroundFocus = this.dataRightContourAroundFocus.Text;
            reportModel.DataLeftContourAroundFocus2 = this.dataLeftContourAroundFocus2.Text;
            reportModel.DataRightContourAroundFocus2 = this.dataRightContourAroundFocus2.Text;
            reportModel.DataLeftContourAroundFocus3 = this.dataLeftContourAroundFocus3.Text;
            reportModel.DataRightContourAroundFocus3 = this.dataRightContourAroundFocus3.Text;

            reportModel.DataLeftInternalElectricalStructure = this.dataLeftInternalElectricalStructure.Text;
            reportModel.DataRightInternalElectricalStructure = this.dataRightInternalElectricalStructure.Text;
            reportModel.DataLeftInternalElectricalStructure2 = this.dataLeftInternalElectricalStructure2.Text;
            reportModel.DataRightInternalElectricalStructure2 = this.dataRightInternalElectricalStructure2.Text;
            reportModel.DataLeftInternalElectricalStructure3 = this.dataLeftInternalElectricalStructure3.Text;
            reportModel.DataRightInternalElectricalStructure3 = this.dataRightInternalElectricalStructure3.Text;

            reportModel.DataLeftSurroundingTissues = this.dataLeftSurroundingTissues.Text;
            reportModel.DataRightSurroundingTissues = this.dataRightSurroundingTissues.Text;
            reportModel.DataLeftSurroundingTissues2 = this.dataLeftSurroundingTissues2.Text;
            reportModel.DataRightSurroundingTissues2 = this.dataRightSurroundingTissues2.Text;
            reportModel.DataLeftSurroundingTissues3 = this.dataLeftSurroundingTissues3.Text;
            reportModel.DataRightSurroundingTissues3 = this.dataRightSurroundingTissues3.Text;

            reportModel.DataLeftOncomarkerHighlightBenignChanges = this.dataLeftOncomarkerHighlightBenignChanges.Text;
            reportModel.DataRightOncomarkerHighlightBenignChanges = this.dataRightOncomarkerHighlightBenignChanges.Text;
            reportModel.DataLeftOncomarkerHighlightSuspiciousChanges = this.dataLeftOncomarkerHighlightSuspiciousChanges.Text;
            reportModel.DataRightOncomarkerHighlightSuspiciousChanges = this.dataRightOncomarkerHighlightSuspiciousChanges.Text;
            reportModel.DataMeanElectricalConductivity3 = this.dataMeanElectricalConductivity3.Text;
            reportModel.DataComparativeElectricalConductivity3 = this.dataComparativeElectricalConductivity3.Text;
            reportModel.DataDivergenceBetweenHistograms3 = this.dataDivergenceBetweenHistograms3.Text;
            reportModel.DataLeftAgeElectricalConductivity = this.dataLeftAgeElectricalConductivity.Text;
            reportModel.DataRightAgeElectricalConductivity = this.dataRightAgeElectricalConductivity.Text;
            //reportModel.DataAgeValueOfEC = this.dataAgeValueOfEC.Text;
            reportModel.DataExamConclusion = this.dataExamConclusion.Text;
            reportModel.DataLeftMammaryGland = this.dataLeftMammaryGland.Text;
            reportModel.DataLeftAgeRelated = this.dataLeftAgeRelated.Text;
            //reportModel.DataLeftFindings = this.dataLeftFindings.Text;
            reportModel.DataLeftMammaryGlandResult = this.dataLeftMammaryGlandResult.Text;
            reportModel.DataRightMammaryGland = this.dataRightMammaryGland.Text;
            reportModel.DataRightAgeRelated = this.dataRightAgeRelated.Text;
            //reportModel.DataRightFindings = this.dataRightFindings.Text;
            reportModel.DataRightMammaryGlandResult = this.dataRightMammaryGlandResult.Text;
            reportModel.DataTotalPts = this.dataTotalPts.Text;
            //reportModel.DataLeftTotalPts = this.dataLeftTotalPts.Text;
            //reportModel.DataRightTotalPts = this.dataRightTotalPts.Text;
            //reportModel.DataPoint = this.dataPoint.Text;

            reportModel.DataBiRadsCategory = this.dataBiRadsCategory.Text;

            if (this.dataBiRadsCategory.Text.StartsWith("0"))
            {
                reportModel.DataPoint = "";
            }
            else if (this.dataBiRadsCategory.Text.StartsWith("1"))
            {
                reportModel.DataPoint = App.Current.FindResource("ReportContext_144").ToString();
            }
            else if (this.dataBiRadsCategory.Text.StartsWith("2"))
            {
                reportModel.DataPoint = App.Current.FindResource("ReportContext_145").ToString();
            }
            else if (this.dataBiRadsCategory.Text.StartsWith("3"))
            {
                reportModel.DataPoint = App.Current.FindResource("ReportContext_146").ToString();
            }
            else if (this.dataBiRadsCategory.Text.StartsWith("4"))
            {
                reportModel.DataPoint = App.Current.FindResource("ReportContext_147").ToString();
            }
            else if (this.dataBiRadsCategory.Text.StartsWith("5"))
            {
                reportModel.DataPoint = App.Current.FindResource("ReportContext_148").ToString();
            }

            reportModel.DataRecommendation = getCommentsStr(shortFormReportModel.DataRecommendation, true);
            //暫時保存DataRecommendation字段的英文翻譯到reportModel.DataComments中（只針對ClientReport，因為ClientReport沒有顯示DataComments內容）
            reportModel.DataComments = getCommentsStr(shortFormReportModel.DataRecommendation, false);

            reportModel.DataConclusion = getAnalysisStr(shortFormReportModel.DataConclusion, true);
            reportModel.DataConclusion2 = getAnalysisStr(shortFormReportModel.DataConclusion, false);
            reportModel.DataFurtherExam = this.dataFurtherExam.Text;


            return reportModel;
        }

        /// <summary>
        /// 根据形状选项index获取对应的中英文字符串
        /// </summary>
        /// <param name="shapeIndex"></param>
        /// <returns></returns>
        private string getSharpStr(string shapeIndex)
        {
            string sharpStr = "";
            switch (shapeIndex)
            {
                case "1":
                    sharpStr = "圓形 Round";
                    break;
                case "2":
                    sharpStr = "橢圓形 Oval";
                    break;
                case "3":
                    sharpStr = "分葉狀 Lobular";
                    break;
                case "4":
                    sharpStr = "不規則 Irregular";
                    break;
                default:                    
                    break;

            }
            return sharpStr;
        }

        /// <summary>
        /// 根据结果分析选项index获取对应的中英文字符串
        /// </summary>
        /// <param name="analysisIndex"></param>
        /// <returns></returns>
        private string getAnalysisStr(string analysisIndex, bool isChinese)
        {
            string analysisStr = "";
            switch (analysisIndex)
            {
                case "1":

                    analysisStr = isChinese ? "照片影像模糊, 需重新掃描" : "Poor Image, need to re-screen";
                    break;
                case "2":
                    analysisStr = isChinese ? "沒有發現病理性改變" : "No abnormal changes detected";
                    break;
                case "3":
                    analysisStr = isChinese ? "沒有發現病理性改變，（平均電導率）屬於高風險類型" : "No abnormal changes detected, (MEC) belong to high risk group";
                    break;
                case "4":
                    analysisStr = isChinese ? "發現良性病理性改變" : "Benign Changes";
                    break;
                case "5":
                    analysisStr = isChinese ? "發現良性病理性改變 – (平均電導率) 屬於高風險型" : "Benign Changes, (MEC) belong to high risk group";
                    break;
                case "6":
                    analysisStr = isChinese ? "發現疑似良性病理性改變" : "Benign lesion detected";
                    break;
                case "7":
                    analysisStr = isChinese ? "發現疑似良性病理性改變，（平均電導率）屬於高風險類型" : "Benign lesion detected, (MEC) belong to high risk group";
                    break;
                case "8":
                    analysisStr = isChinese ? "發現可疑病理性改變" : "Suspicious abnormality detected";
                    break;
                case "9":
                    analysisStr = isChinese ? "發現高度可疑病理性改變" : "Highly suspicious abnormality detected";
                    break;
                default:
                    break;

            }
            return analysisStr;
        }

        /// <summary>
        /// 根据建议选项index获取对应的中英文字符串
        /// </summary>
        /// <param name="analysisIndex"></param>
        /// <returns></returns>
        private string getCommentsStr(string commentsIndex,bool isChinese)
        {
            string commentsStr = "";
            switch (commentsIndex)
            {
                case "1":
                    commentsStr =isChinese ?  "在第1階段月經週期內進行MEIK檢查":"MEIK re-examination in 1 phase menstrual cycle";
                    break;
                case "2":
                    commentsStr =isChinese ?  "在第2階段月經週期內進行MEIK檢查":"MEIK re-examination in 2 phase menstrual cycle";
                    break;
                case "3":
                    commentsStr = isChinese ? "MEIK乳腺電阻抗檢測 (三月內)" : "MEIK examination in 3 months";
                    break;
                case "4":
                    commentsStr = isChinese ? "MEIK乳腺電阻抗檢測 (六月內)" : "MEIK examination in 6 months";
                    break;
                case "5":
                    commentsStr = isChinese ? "MEIK乳腺電阻抗檢測 (年度)" : "MEIK examination in 1 year";
                    break;
                default:
                    break;

            }
            return commentsStr;
        }

        public void ShowSignature(Object obj)
        {
            dataSignImg.Source = ImageTools.GetBitmapImage(AppDomain.CurrentDomain.BaseDirectory + "Signature" + System.IO.Path.DirectorySeparatorChar + "temp.jpg");
        }

        private void savePdfBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var person = this.CodeListBox.SelectedItem as Person;

                if (App.reportSettingModel.ShowDoctorSignature)
                {
                    if (this.dataDoctor.SelectedIndex==-1)
                    {
                        MessageBox.Show(this, App.Current.FindResource("Message_83").ToString());
                        return;
                    }
                }
                
                LoadDataModel();
                //Clone生成全文本的报表数据对象模型
                var reportModel = CloneReportModel();

                string folderName = person.ArchiveFolder;
                string strName = person.SurName + (string.IsNullOrEmpty(person.GivenName) ? "" : "," + person.GivenName) + (string.IsNullOrEmpty(person.OtherName) ? "" : " " + person.OtherName) + ".pdf";
                //生成Examination报告的PDF文件
                string lfPdfFile = folderName + System.IO.Path.DirectorySeparatorChar + person.Code + " LF - " + strName;
                //string lfReportTempl = "Views/ExaminationReportFlow.xaml";
                string lfReportTempl = "Views/GoodUnionReportFlow.xaml";
                ExportFlowDocumentPDF(lfReportTempl, lfPdfFile, reportModel);

                //生成Summary报告的PDF文件
                string sfPdfFile = folderName + System.IO.Path.DirectorySeparatorChar + person.Code + " SFI - " + strName;
                //string sfReportTempl = "Views/SummaryReportDocument.xaml";
                string sfReportTempl = "Views/SummaryReportFlow.xaml";
                //if (shortFormReportModel.DataScreenShotImg != null)
                //{
                //    sfPdfFile = folderName + System.IO.Path.DirectorySeparatorChar + person.Code + "SFI - " + strName;
                //    sfReportTempl = "Views/SummaryReportImageDocument.xaml";
                //}
                //ExportPDF(sfReportTempl, sfPdfFile, reportModel);

                var clientReportModel = CloneClientReportModel();
                ExportFlowDocumentPDF(sfReportTempl, sfPdfFile, clientReportModel, "A4");
                //保存報表完成狀態
                person.Status = "RD";
                person.StatusText = App.Current.FindResource("CommonStatusReportDone").ToString();                
                OperateIniFile.WriteIniData("Report", "Status", "RD", person.IniFilePath);

                ////导出到excel文件
                //strName = person.SurName + (string.IsNullOrEmpty(person.GivenName) ? "" : "," + person.GivenName) + (string.IsNullOrEmpty(person.OtherName) ? "" : " " + person.OtherName) + ".xls";
                //string xlsFile = person.ArchiveFolder + System.IO.Path.DirectorySeparatorChar + person.Code + " - " + strName;
                //ExportExcel(xlsFile, reportModel);

                //導出到csv文件                
                string csvFile = person.ArchiveFolder + System.IO.Path.DirectorySeparatorChar + person.Code + ".csv ";
                ExportCSV(csvFile, reportModel);

                MessageBox.Show(this, App.Current.FindResource("Message_5").ToString());
            }
            catch (Exception ex)
            {               
                MessageBox.Show(this, ex.Message);
            }
        }

        /// <summary>
        ///针对FixedPage对象生成PDF 
        /// </summary>
        /// <param name="reportTempl"></param>
        /// <param name="pdfFile"></param>
        /// <param name="reportModel"></param>
        private void ExportPDF(string reportTempl, string pdfFile, ShortFormReport reportModel)
        {
            var person = this.CodeListBox.SelectedItem as Person; 
            string xpsFile = dataFolder + System.IO.Path.DirectorySeparatorChar + person.Code + ".xps";
            //string userTempPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            //string xpsFile = userTempPath + System.IO.Path.DirectorySeparatorChar + person.Code + ".xps";
            if (File.Exists(xpsFile))
            {
                File.Delete(xpsFile);
            }

            FixedPage page = (FixedPage)PrintPreviewWindow.LoadFixedDocumentAndRender(reportTempl, reportModel);

            XpsDocument xpsDocument = new XpsDocument(xpsFile, FileAccess.ReadWrite);
            //将flow document写入基于内存的xps document中去
            XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
            writer.Write(page);
            xpsDocument.Close();
            if (File.Exists(pdfFile))
            {
                File.Delete(pdfFile);
            }
            PDFTools.SavePDFFile(xpsFile, pdfFile);
        }

        /// <summary>
        ///导出Long Form到Excel文件 
        /// </summary>
        /// <param name="reportTempl"></param>
        /// <param name="pdfFile"></param>
        /// <param name="reportModel"></param>
        private void ExportExcel(string excelFile, ShortFormReport reportModel)
        {                      
            try
            {
                if (File.Exists(excelFile))
                {
                    File.Delete(excelFile);
                }
                NameValueCollection nvc = new NameValueCollection();
                nvc.Add(App.Current.FindResource("ReportContext_202").ToString(), reportModel.DataClientNum);
                nvc.Add(App.Current.FindResource("ReportContext_9").ToString(), reportModel.DataUserCode);
                nvc.Add(App.Current.FindResource("ReportContext_2").ToString(), reportModel.DataScreenDate);
                nvc.Add(App.Current.FindResource("ReportContext_1").ToString(), reportModel.DataName);
                nvc.Add(App.Current.FindResource("ReportContext_10").ToString(), reportModel.DataAge);
                nvc.Add(App.Current.FindResource("ReportContext_203").ToString(), reportModel.DataHeight);
                nvc.Add(App.Current.FindResource("ReportContext_11").ToString(), reportModel.DataWeight+" Kgs");
                nvc.Add(App.Current.FindResource("ReportContext_204").ToString(), reportModel.DataMobile);
                nvc.Add(App.Current.FindResource("ReportContext_205").ToString(), reportModel.DataEmail);
                nvc.Add(App.Current.FindResource("ReportContext_6").ToString(), reportModel.DataScreenLocation);
                nvc.Add(App.Current.FindResource("ReportContext_149").ToString(), reportModel.DataMeikTech);
                nvc.Add(App.Current.FindResource("ReportContext_14").ToString(), reportModel.DataMenstrualCycle);
                nvc.Add(App.Current.FindResource("ReportContext_23").ToString(), reportModel.DataSkinAffections);
                nvc.Add(App.Current.FindResource("ReportContext_22").ToString(), reportModel.DataHormones);
                nvc.Add(App.Current.FindResource("ReportContext_40").ToString() +"("+ App.Current.FindResource("ReportContext_44").ToString()+")", reportModel.DataLeftBreast);
                nvc.Add(App.Current.FindResource("ReportContext_40").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightBreast);
                nvc.Add(App.Current.FindResource("ReportContext_47").ToString() +"("+ App.Current.FindResource("ReportContext_44").ToString()+")", reportModel.DataLeftChangesOfElectricalConductivity);
                nvc.Add(App.Current.FindResource("ReportContext_47").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightChangesOfElectricalConductivity);
                nvc.Add(App.Current.FindResource("ReportContext_51").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftMammaryStruct);
                nvc.Add(App.Current.FindResource("ReportContext_51").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightMammaryStruct);
                nvc.Add(App.Current.FindResource("ReportContext_54").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftLactiferousSinusZone);
                nvc.Add(App.Current.FindResource("ReportContext_54").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightLactiferousSinusZone);
                nvc.Add(App.Current.FindResource("ReportContext_59").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftMammaryContour);
                nvc.Add(App.Current.FindResource("ReportContext_59").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightMammaryContour);
                nvc.Add(App.Current.FindResource("ReportContext_65").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftNumber);
                nvc.Add(App.Current.FindResource("ReportContext_65").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightNumber);
                nvc.Add(App.Current.FindResource("ReportContext_64").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftLocation);
                nvc.Add(App.Current.FindResource("ReportContext_64").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightLocation);
                nvc.Add(App.Current.FindResource("ReportContext_66").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", string.IsNullOrEmpty(reportModel.DataLeftSize) ? "" : reportModel.DataLeftSize + " mm");
                nvc.Add(App.Current.FindResource("ReportContext_66").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", string.IsNullOrEmpty(reportModel.DataRightSize) ? "" : reportModel.DataLeftSize + " mm");
                nvc.Add(App.Current.FindResource("ReportContext_67").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftShape);
                nvc.Add(App.Current.FindResource("ReportContext_67").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightShape);
                nvc.Add(App.Current.FindResource("ReportContext_72").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftContourAroundFocus);
                nvc.Add(App.Current.FindResource("ReportContext_72").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightContourAroundFocus);
                nvc.Add(App.Current.FindResource("ReportContext_76").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftInternalElectricalStructure);
                nvc.Add(App.Current.FindResource("ReportContext_76").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightInternalElectricalStructure);
                nvc.Add(App.Current.FindResource("ReportContext_81").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftSurroundingTissues);
                nvc.Add(App.Current.FindResource("ReportContext_81").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightSurroundingTissues);

                nvc.Add(App.Current.FindResource("ReportContext_100").ToString() + " - " + App.Current.FindResource("ReportContext_101").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftMeanElectricalConductivity1);
                nvc.Add(App.Current.FindResource("ReportContext_100").ToString() + " - " + App.Current.FindResource("ReportContext_101").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightMeanElectricalConductivity1);
                nvc.Add(App.Current.FindResource("ReportContext_100").ToString() + " - " + App.Current.FindResource("ReportContext_102").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftMeanElectricalConductivity2);
                nvc.Add(App.Current.FindResource("ReportContext_100").ToString() + " - " + App.Current.FindResource("ReportContext_102").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightMeanElectricalConductivity2);
                nvc.Add(App.Current.FindResource("ReportContext_100").ToString() + " - " + reportModel.DataMeanElectricalConductivity3 + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftMeanElectricalConductivity3);
                nvc.Add(App.Current.FindResource("ReportContext_100").ToString() + " - " + reportModel.DataMeanElectricalConductivity3 + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightMeanElectricalConductivity3);
                nvc.Add(App.Current.FindResource("ReportContext_106").ToString() + " - " + App.Current.FindResource("ReportContext_101").ToString() , reportModel.DataLeftComparativeElectricalConductivity1);                
                nvc.Add(App.Current.FindResource("ReportContext_106").ToString() + " - " + App.Current.FindResource("ReportContext_102").ToString(), reportModel.DataLeftComparativeElectricalConductivity2);               
                nvc.Add(App.Current.FindResource("ReportContext_106").ToString() + " - " + reportModel.DataComparativeElectricalConductivity3 , reportModel.DataLeftComparativeElectricalConductivity3);
                nvc.Add(App.Current.FindResource("ReportContext_107").ToString() + " - " + App.Current.FindResource("ReportContext_101").ToString(), reportModel.DataLeftDivergenceBetweenHistograms1);
                nvc.Add(App.Current.FindResource("ReportContext_107").ToString() + " - " + App.Current.FindResource("ReportContext_102").ToString(), reportModel.DataLeftDivergenceBetweenHistograms2);
                nvc.Add(App.Current.FindResource("ReportContext_107").ToString() + " - " + reportModel.DataDivergenceBetweenHistograms3, reportModel.DataLeftDivergenceBetweenHistograms3);
                nvc.Add(App.Current.FindResource("ReportContext_109").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftPhaseElectricalConductivity);
                nvc.Add(App.Current.FindResource("ReportContext_109").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightPhaseElectricalConductivity);
                nvc.Add(App.Current.FindResource("ReportContext_110").ToString() + "[" + App.Current.FindResource("ReportContext_110").ToString() + "]" + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftAgeElectricalConductivity);
                nvc.Add(App.Current.FindResource("ReportContext_110").ToString() + "[" + App.Current.FindResource("ReportContext_110").ToString() + "]" + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightAgeElectricalConductivity);
                
                nvc.Add(App.Current.FindResource("ReportContext_115").ToString(), reportModel.DataExamConclusion);

                nvc.Add(App.Current.FindResource("ReportContext_121").ToString(), reportModel.DataLeftMammaryGland);
                nvc.Add(App.Current.FindResource("ReportContext_139").ToString(), reportModel.DataRightMammaryGland);

                nvc.Add(App.Current.FindResource("ReportContext_127").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftAgeRelated);
                nvc.Add(App.Current.FindResource("ReportContext_127").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightAgeRelated);

                nvc.Add(App.Current.FindResource("ReportContext_182").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftMammaryGlandResult);
                nvc.Add(App.Current.FindResource("ReportContext_182").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightMammaryGlandResult);

                nvc.Add(App.Current.FindResource("ReportContext_141").ToString() + App.Current.FindResource("ReportContext_142").ToString(), reportModel.DataTotalPts);
                nvc.Add(App.Current.FindResource("ReportContext_150").ToString(), reportModel.DataBiRadsCategory);
                nvc.Add(App.Current.FindResource("ReportContext_226").ToString(), reportModel.DataConclusion);
                nvc.Add(App.Current.FindResource("ReportContext_157").ToString(), reportModel.DataRecommendation);
                nvc.Add(App.Current.FindResource("ReportContext_227").ToString(), reportModel.DataComments);
                nvc.Add(App.Current.FindResource("ReportContext_173").ToString(), reportModel.DataDoctor);
                nvc.Add(App.Current.FindResource("ReportContext_200").ToString(), reportModel.DataDoctorLicense);

                nvc.Add(App.Current.FindResource("ReportContext_190").ToString(), reportModel.DataSignDate);                

                var excelApp = new Microsoft.Office.Interop.Excel.Application();
                var books = (Microsoft.Office.Interop.Excel.Workbooks)excelApp.Workbooks;
                var book = (Microsoft.Office.Interop.Excel._Workbook)(books.Add(System.Type.Missing));
                var sheets = (Microsoft.Office.Interop.Excel.Sheets)book.Worksheets;
                var sheet = (Microsoft.Office.Interop.Excel._Worksheet)(sheets.get_Item(1));

                Microsoft.Office.Interop.Excel.Range range = (Microsoft.Office.Interop.Excel.Range)sheet.get_Range("A1", "A100");
                range.ColumnWidth = 80;                
                range = (Microsoft.Office.Interop.Excel.Range)sheet.get_Range("B1", "B100");
                range.ColumnWidth = 80;                
                range.NumberFormatLocal = "@"; //设置单元格为文本

                sheet.Cells[1, 1] = "Field Name";
                sheet.Cells[2, 1] = "Field Value";
                for (int i = 0; i < nvc.Count; i++)
                {                    
                    var value = nvc[i];
                    sheet.Cells[1, i + 2] = nvc.GetKey(i); ;
                    sheet.Cells[2, i + 2] = value;                    
                }
                book.SaveAs(excelFile, Microsoft.Office.Interop.Excel.XlFileFormat.xlExcel8, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                book.Close();
                excelApp.Quit();
            }
            catch (Exception exe)
            {
                MessageBox.Show(exe.Message);
            }
        }

        /// <summary>
        ///导出Long Form到Excel文件 
        /// </summary>
        /// <param name="reportTempl"></param>
        /// <param name="pdfFile"></param>
        /// <param name="reportModel"></param>
        private void ExportCSV(string csvFile, ShortFormReport reportModel)
        {
            try
            {
                if (File.Exists(csvFile))
                {
                    File.Delete(csvFile);
                }
                NameValueCollection nvc = new NameValueCollection();
                nvc.Add(App.Current.FindResource("ReportContext_202").ToString(), reportModel.DataClientNum);
                nvc.Add(App.Current.FindResource("ReportContext_9").ToString(), reportModel.DataUserCode);
                nvc.Add(App.Current.FindResource("ReportContext_2").ToString(), reportModel.DataScreenDate);
                nvc.Add(App.Current.FindResource("ReportContext_1").ToString(), reportModel.DataName);
                nvc.Add(App.Current.FindResource("ReportContext_10").ToString(), reportModel.DataAge);
                nvc.Add(App.Current.FindResource("ReportContext_203").ToString(), reportModel.DataHeight);
                nvc.Add(App.Current.FindResource("ReportContext_11").ToString(), reportModel.DataWeight + " Kgs");
                nvc.Add(App.Current.FindResource("ReportContext_204").ToString(), reportModel.DataMobile);
                nvc.Add(App.Current.FindResource("ReportContext_205").ToString(), reportModel.DataEmail);
                nvc.Add(App.Current.FindResource("ReportContext_6").ToString(), reportModel.DataScreenLocation);
                nvc.Add(App.Current.FindResource("ReportContext_149").ToString(), reportModel.DataMeikTech);
                nvc.Add(App.Current.FindResource("ReportContext_14").ToString(), reportModel.DataMenstrualCycle);
                nvc.Add(App.Current.FindResource("ReportContext_23").ToString(), reportModel.DataSkinAffections);
                nvc.Add(App.Current.FindResource("ReportContext_22").ToString(), reportModel.DataHormones);
                nvc.Add(App.Current.FindResource("ReportContext_40").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftBreast);
                nvc.Add(App.Current.FindResource("ReportContext_40").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightBreast);
                nvc.Add(App.Current.FindResource("ReportContext_47").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftChangesOfElectricalConductivity);
                nvc.Add(App.Current.FindResource("ReportContext_47").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightChangesOfElectricalConductivity);
                nvc.Add(App.Current.FindResource("ReportContext_51").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftMammaryStruct);
                nvc.Add(App.Current.FindResource("ReportContext_51").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightMammaryStruct);
                nvc.Add(App.Current.FindResource("ReportContext_54").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftLactiferousSinusZone);
                nvc.Add(App.Current.FindResource("ReportContext_54").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightLactiferousSinusZone);
                nvc.Add(App.Current.FindResource("ReportContext_59").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftMammaryContour);
                nvc.Add(App.Current.FindResource("ReportContext_59").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightMammaryContour);
                nvc.Add(App.Current.FindResource("ReportContext_65").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftNumber);
                nvc.Add(App.Current.FindResource("ReportContext_65").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightNumber);
                nvc.Add(App.Current.FindResource("ReportContext_64").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftLocation);
                nvc.Add(App.Current.FindResource("ReportContext_64").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightLocation);
                nvc.Add(App.Current.FindResource("ReportContext_66").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", string.IsNullOrEmpty(reportModel.DataLeftSize) ? "" : reportModel.DataLeftSize + " mm");
                nvc.Add(App.Current.FindResource("ReportContext_66").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", string.IsNullOrEmpty(reportModel.DataRightSize) ? "" : reportModel.DataLeftSize + " mm");
                nvc.Add(App.Current.FindResource("ReportContext_67").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftShape);
                nvc.Add(App.Current.FindResource("ReportContext_67").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightShape);
                nvc.Add(App.Current.FindResource("ReportContext_72").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftContourAroundFocus);
                nvc.Add(App.Current.FindResource("ReportContext_72").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightContourAroundFocus);
                nvc.Add(App.Current.FindResource("ReportContext_76").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftInternalElectricalStructure);
                nvc.Add(App.Current.FindResource("ReportContext_76").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightInternalElectricalStructure);
                nvc.Add(App.Current.FindResource("ReportContext_81").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftSurroundingTissues);
                nvc.Add(App.Current.FindResource("ReportContext_81").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightSurroundingTissues);

                nvc.Add(App.Current.FindResource("ReportContext_100").ToString() + " - " + App.Current.FindResource("ReportContext_101").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftMeanElectricalConductivity1);
                nvc.Add(App.Current.FindResource("ReportContext_100").ToString() + " - " + App.Current.FindResource("ReportContext_101").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightMeanElectricalConductivity1);
                nvc.Add(App.Current.FindResource("ReportContext_100").ToString() + " - " + App.Current.FindResource("ReportContext_102").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftMeanElectricalConductivity2);
                nvc.Add(App.Current.FindResource("ReportContext_100").ToString() + " - " + App.Current.FindResource("ReportContext_102").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightMeanElectricalConductivity2);
                nvc.Add(App.Current.FindResource("ReportContext_100").ToString() + " - " + reportModel.DataMeanElectricalConductivity3 + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftMeanElectricalConductivity3);
                nvc.Add(App.Current.FindResource("ReportContext_100").ToString() + " - " + reportModel.DataMeanElectricalConductivity3 + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightMeanElectricalConductivity3);
                nvc.Add(App.Current.FindResource("ReportContext_106").ToString() + " - " + App.Current.FindResource("ReportContext_101").ToString(), reportModel.DataLeftComparativeElectricalConductivity1);
                nvc.Add(App.Current.FindResource("ReportContext_106").ToString() + " - " + App.Current.FindResource("ReportContext_102").ToString(), reportModel.DataLeftComparativeElectricalConductivity2);
                nvc.Add(App.Current.FindResource("ReportContext_106").ToString() + " - " + reportModel.DataComparativeElectricalConductivity3, reportModel.DataLeftComparativeElectricalConductivity3);
                nvc.Add(App.Current.FindResource("ReportContext_107").ToString() + " - " + App.Current.FindResource("ReportContext_101").ToString(), reportModel.DataLeftDivergenceBetweenHistograms1);
                nvc.Add(App.Current.FindResource("ReportContext_107").ToString() + " - " + App.Current.FindResource("ReportContext_102").ToString(), reportModel.DataLeftDivergenceBetweenHistograms2);
                nvc.Add(App.Current.FindResource("ReportContext_107").ToString() + " - " + reportModel.DataDivergenceBetweenHistograms3, reportModel.DataLeftDivergenceBetweenHistograms3);
                nvc.Add(App.Current.FindResource("ReportContext_109").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftPhaseElectricalConductivity);
                nvc.Add(App.Current.FindResource("ReportContext_109").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightPhaseElectricalConductivity);
                nvc.Add(App.Current.FindResource("ReportContext_110").ToString() + "[" + App.Current.FindResource("ReportContext_110").ToString() + "]" + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftAgeElectricalConductivity);
                nvc.Add(App.Current.FindResource("ReportContext_110").ToString() + "[" + App.Current.FindResource("ReportContext_110").ToString() + "]" + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightAgeElectricalConductivity);

                nvc.Add(App.Current.FindResource("ReportContext_115").ToString(), reportModel.DataExamConclusion);

                nvc.Add(App.Current.FindResource("ReportContext_121").ToString(), reportModel.DataLeftMammaryGland);
                nvc.Add(App.Current.FindResource("ReportContext_139").ToString(), reportModel.DataRightMammaryGland);

                nvc.Add(App.Current.FindResource("ReportContext_127").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftAgeRelated);
                nvc.Add(App.Current.FindResource("ReportContext_127").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightAgeRelated);

                nvc.Add(App.Current.FindResource("ReportContext_182").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftMammaryGlandResult);
                nvc.Add(App.Current.FindResource("ReportContext_182").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightMammaryGlandResult);

                nvc.Add(App.Current.FindResource("ReportContext_141").ToString() + App.Current.FindResource("ReportContext_142").ToString(), reportModel.DataTotalPts);
                nvc.Add(App.Current.FindResource("ReportContext_150").ToString(), reportModel.DataBiRadsCategory);
                nvc.Add(App.Current.FindResource("ReportContext_226").ToString(), reportModel.DataConclusion);
                nvc.Add(App.Current.FindResource("ReportContext_157").ToString(), reportModel.DataRecommendation);
                nvc.Add(App.Current.FindResource("ReportContext_227").ToString(), reportModel.DataComments);
                nvc.Add(App.Current.FindResource("ReportContext_173").ToString(), reportModel.DataDoctor);
                nvc.Add(App.Current.FindResource("ReportContext_200").ToString(), reportModel.DataDoctorLicense);

                nvc.Add(App.Current.FindResource("ReportContext_190").ToString(), reportModel.DataSignDate);

                StringBuilder sb = new StringBuilder();
                StringBuilder sb1 = new StringBuilder();
                for (int i = 0; i < nvc.Count; i++)
                {
                    var key = nvc.GetKey(i);
                    var value = nvc[i];
                    if (key.Contains('"'))
                    {
                        key = key.Replace("\"", "\"\"");//替换英文冒号 英文冒号需要换成两个冒号                  
                    }
                    if (key.Contains('"') || key.Contains('\r') || key.Contains('\n') || key.Contains(' '))
                    {
                        sb.Append("\"" + key + "\",");
                    }
                    else
                    {
                        sb.Append(key + ",");
                    }

                    if (value.Contains('"'))
                    {
                        value = value.Replace("\"", "\"\"");//替换英文冒号 英文冒号需要换成两个冒号                  
                    }
                    if (value.Contains('"') || value.Contains('\r') || value.Contains('\n') || value.Contains(' '))
                    {
                        sb1.Append("\"" + value + "\",");
                    }
                    else
                    {
                        sb1.Append(value + ",");
                    }                                       
                }
                FileStream fs = new FileStream(csvFile, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
                sw.WriteLine(sb.ToString());
                sw.WriteLine(sb1.ToString());
                sw.Close();
                fs.Close();
            }
            catch (Exception exe)
            {
                MessageBox.Show(exe.Message);
            }
        }

        /// <summary>
        /// 针对FLowDocument对象生成PDF文件
        /// </summary>
        /// <param name="reportTempl"></param>
        /// <param name="pdfFile"></param>
        /// <param name="reportModel"></param>
        private void ExportFlowDocumentPDF(string reportTempl, string pdfFile, ShortFormReport reportModel, string pagesize = null)
        {
            var person = this.CodeListBox.SelectedItem as Person; 
            string xpsFile = dataFolder + System.IO.Path.DirectorySeparatorChar + person.Code + ".xps";
            if (File.Exists(xpsFile))
            {
                File.Delete(xpsFile);
            }

            FlowDocument page = (FlowDocument)PrintPreviewWindow.LoadFlowDocumentAndRender(reportTempl, reportModel, pagesize);
            try
            {
                LoadDataForFlowDocument(page, reportModel);
            }
            catch { }
            XpsDocument xpsDocument = new XpsDocument(xpsFile, FileAccess.ReadWrite);
            //将flow document写入基于内存的xps document中去
            XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
            writer.Write(((IDocumentPaginatorSource)page).DocumentPaginator);
            xpsDocument.Close();
            if (File.Exists(pdfFile))
            {
                File.Delete(pdfFile);
            }
            PDFTools.SavePDFFile(xpsFile, pdfFile);
        }

        private void LoadDataForFlowDocument(FlowDocument page, ShortFormReport reportModel)
        {
            if (reportModel != null)
            {
                var textBlock1 = page.FindName("dataClientNum") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataClientNum) ? "N/A" : reportModel.DataClientNum;
                }
                textBlock1 = page.FindName("dataUserCode") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataUserCode;
                }
                textBlock1 = page.FindName("dataScreenDate") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataScreenDate;
                }
                textBlock1 = page.FindName("dataName") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataName;
                }
                textBlock1 = page.FindName("dataAge") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataAge) ? "N/A" : reportModel.DataAge;
                }
                textBlock1 = page.FindName("dataHeight") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataHeight) ? "N/A" : reportModel.DataHeight;
                }
                textBlock1 = page.FindName("dataWeight") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataWeight) ? "N/A" : reportModel.DataWeight; 
                }
                textBlock1 = page.FindName("dataScreenLocation") as TextBlock;
                if (textBlock1 != null)
                {                    
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataScreenLocation) ? "N/A" : reportModel.DataScreenLocation;
                }
                textBlock1 = page.FindName("dataMobile") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataMobile) ? "N/A" : reportModel.DataMobile; 
                }
                textBlock1 = page.FindName("dataEmail") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataEmail) ? "N/A" : reportModel.DataEmail; 
                }
                textBlock1 = page.FindName("dataLeftBreast") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftBreast;
                }
                textBlock1 = page.FindName("dataRightBreast") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightBreast;
                }
                textBlock1 = page.FindName("dataLeftPalpableMass") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftPalpableMass;
                }
                textBlock1 = page.FindName("dataRightPalpableMass") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightPalpableMass;
                }
                textBlock1 = page.FindName("dataLeftChangesOfElectricalConductivity") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftChangesOfElectricalConductivity;
                }
                textBlock1 = page.FindName("dataRightChangesOfElectricalConductivity") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightChangesOfElectricalConductivity;
                }
                textBlock1 = page.FindName("dataLeftMammaryStruct") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftMammaryStruct;
                }
                textBlock1 = page.FindName("dataRightMammaryStruct") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightMammaryStruct;
                }
                textBlock1 = page.FindName("dataLeftLactiferousSinusZone") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftLactiferousSinusZone;
                }
                textBlock1 = page.FindName("dataRightLactiferousSinusZone") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightLactiferousSinusZone;
                }
                textBlock1 = page.FindName("dataLeftMammaryContour") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftMammaryContour;
                }
                textBlock1 = page.FindName("dataRightMammaryContour") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightMammaryContour;
                }
                textBlock1 = page.FindName("dataLeftNumber") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftNumber;
                }
                textBlock1 = page.FindName("dataRightNumber") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightNumber;
                }
                textBlock1 = page.FindName("dataLeftLocation") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataLeftLocation) ? "Nil" : reportModel.DataLeftLocation;
                }
                textBlock1 = page.FindName("dataRightLocation") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataRightLocation) ? "Nil" : reportModel.DataRightLocation;
                }
                textBlock1 = page.FindName("dataLeftSize") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataLeftSize) ? "Nil" : reportModel.DataLeftSize;
                }
                textBlock1 = page.FindName("dataRightSize") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataRightSize) ? "Nil" : reportModel.DataRightSize;
                }
                textBlock1 = page.FindName("dataLeftShape") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataLeftShape) ? "Nil" : reportModel.DataLeftShape;
                }
                textBlock1 = page.FindName("dataRightShape") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataRightShape) ? "Nil" : reportModel.DataRightShape;
                }
                textBlock1 = page.FindName("dataLeftContourAroundFocus") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataLeftContourAroundFocus) ? "Nil" : reportModel.DataLeftContourAroundFocus;
                }
                textBlock1 = page.FindName("dataRightContourAroundFocus") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataRightContourAroundFocus) ? "Nil" : reportModel.DataRightContourAroundFocus;
                }
                textBlock1 = page.FindName("dataLeftInternalElectricalStructure") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataLeftInternalElectricalStructure) ? "Nil" : reportModel.DataLeftInternalElectricalStructure;
                }
                textBlock1 = page.FindName("dataRightInternalElectricalStructure") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataRightInternalElectricalStructure) ? "Nil" : reportModel.DataRightInternalElectricalStructure;
                }
                textBlock1 = page.FindName("dataLeftSurroundingTissues") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataLeftSurroundingTissues) ? "Nil" : reportModel.DataLeftSurroundingTissues;
                }
                textBlock1 = page.FindName("dataRightSurroundingTissues") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataRightSurroundingTissues) ? "Nil" : reportModel.DataRightSurroundingTissues;
                }
                textBlock1 = page.FindName("dataLeftLocation2") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataLeftLocation2) ? "Nil" : reportModel.DataLeftLocation2;
                }
                textBlock1 = page.FindName("dataRightLocation2") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataRightLocation2) ? "Nil" : reportModel.DataRightLocation2;
                }
                textBlock1 = page.FindName("dataLeftSize2") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataLeftSize2) ? "Nil" : reportModel.DataLeftSize2;
                }
                textBlock1 = page.FindName("dataRightSize2") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataRightSize2) ? "Nil" : reportModel.DataRightSize2;
                }
                textBlock1 = page.FindName("dataLeftShape2") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataLeftShape2) ? "Nil" : reportModel.DataLeftShape2;
                }
                textBlock1 = page.FindName("dataRightShape2") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataRightShape2) ? "Nil" : reportModel.DataRightShape2;
                }
                textBlock1 = page.FindName("dataLeftContourAroundFocus2") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataLeftContourAroundFocus2) ? "Nil" : reportModel.DataLeftContourAroundFocus2;
                }
                textBlock1 = page.FindName("dataRightContourAroundFocus2") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataRightContourAroundFocus2) ? "Nil" : reportModel.DataRightContourAroundFocus2;
                }
                textBlock1 = page.FindName("dataLeftInternalElectricalStructure2") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataLeftInternalElectricalStructure2) ? "Nil" : reportModel.DataLeftInternalElectricalStructure2;
                }
                textBlock1 = page.FindName("dataRightInternalElectricalStructure2") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataRightInternalElectricalStructure2) ? "Nil" : reportModel.DataRightInternalElectricalStructure2;
                }
                textBlock1 = page.FindName("dataLeftSurroundingTissues2") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataLeftSurroundingTissues2) ? "Nil" : reportModel.DataLeftSurroundingTissues2;
                }
                textBlock1 = page.FindName("dataRightSurroundingTissues2") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataRightSurroundingTissues2) ? "Nil" : reportModel.DataRightSurroundingTissues2;
                }
                textBlock1 = page.FindName("dataLeftLocation3") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataLeftLocation3) ? "Nil" : reportModel.DataLeftLocation3;
                }
                textBlock1 = page.FindName("dataRightLocation3") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataRightLocation3) ? "Nil" : reportModel.DataRightLocation3;
                }
                textBlock1 = page.FindName("dataLeftSize3") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataLeftSize3) ? "Nil" : reportModel.DataLeftSize3;
                }
                textBlock1 = page.FindName("dataRightSize3") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataRightSize3) ? "Nil" : reportModel.DataRightSize3;
                }
                textBlock1 = page.FindName("dataLeftShape3") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataLeftShape3) ? "Nil" : reportModel.DataLeftShape3;
                }
                textBlock1 = page.FindName("dataRightShape3") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataRightShape3) ? "Nil" : reportModel.DataRightShape3;
                }
                textBlock1 = page.FindName("dataLeftContourAroundFocus3") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataLeftContourAroundFocus3) ? "Nil" : reportModel.DataLeftContourAroundFocus3;
                }
                textBlock1 = page.FindName("dataRightContourAroundFocus3") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataRightContourAroundFocus3) ? "Nil" : reportModel.DataRightContourAroundFocus3;
                }
                textBlock1 = page.FindName("dataLeftInternalElectricalStructure3") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataLeftInternalElectricalStructure3) ? "Nil" : reportModel.DataLeftInternalElectricalStructure3;
                }
                textBlock1 = page.FindName("dataRightInternalElectricalStructure3") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataRightInternalElectricalStructure3) ? "Nil" : reportModel.DataRightInternalElectricalStructure3;
                }
                textBlock1 = page.FindName("dataLeftSurroundingTissues3") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataLeftSurroundingTissues3) ? "Nil" : reportModel.DataLeftSurroundingTissues3;
                }
                textBlock1 = page.FindName("dataRightSurroundingTissues3") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataRightSurroundingTissues3) ? "Nil" : reportModel.DataRightSurroundingTissues3;
                }
                textBlock1 = page.FindName("dataLeftOncomarkerHighlightBenignChanges") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftOncomarkerHighlightBenignChanges;
                }
                textBlock1 = page.FindName("dataRightOncomarkerHighlightBenignChanges") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightOncomarkerHighlightBenignChanges;
                }
                textBlock1 = page.FindName("dataLeftOncomarkerHighlightSuspiciousChanges") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftOncomarkerHighlightSuspiciousChanges;
                }
                textBlock1 = page.FindName("dataRightOncomarkerHighlightSuspiciousChanges") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightOncomarkerHighlightSuspiciousChanges;
                }
                textBlock1 = page.FindName("dataLeftMeanElectricalConductivity1") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftMeanElectricalConductivity1;
                }
                textBlock1 = page.FindName("dataRightMeanElectricalConductivity1") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightMeanElectricalConductivity1;
                }
                textBlock1 = page.FindName("dataLeftMeanElectricalConductivity2") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftMeanElectricalConductivity2;
                }
                textBlock1 = page.FindName("dataRightMeanElectricalConductivity2") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightMeanElectricalConductivity2;
                }
                textBlock1 = page.FindName("dataMeanElectricalConductivity3") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataMeanElectricalConductivity3;
                }
                textBlock1 = page.FindName("dataLeftMeanElectricalConductivity3") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftMeanElectricalConductivity3;
                }
                textBlock1 = page.FindName("dataRightMeanElectricalConductivity3") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightMeanElectricalConductivity3;
                }
                textBlock1 = page.FindName("dataLeftComparativeElectricalConductivity1") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftComparativeElectricalConductivity1;
                }
                textBlock1 = page.FindName("dataLeftComparativeElectricalConductivity2") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftComparativeElectricalConductivity2;
                }
                textBlock1 = page.FindName("dataComparativeElectricalConductivity3") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataComparativeElectricalConductivity3;
                }
                textBlock1 = page.FindName("dataLeftComparativeElectricalConductivity3") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftComparativeElectricalConductivity3;
                }
                textBlock1 = page.FindName("dataLeftDivergenceBetweenHistograms1") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftDivergenceBetweenHistograms1;
                }
                textBlock1 = page.FindName("dataLeftDivergenceBetweenHistograms2") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftDivergenceBetweenHistograms2;
                }
                textBlock1 = page.FindName("dataDivergenceBetweenHistograms3") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataDivergenceBetweenHistograms3;
                }
                textBlock1 = page.FindName("dataLeftDivergenceBetweenHistograms3") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftDivergenceBetweenHistograms3;
                }
                textBlock1 = page.FindName("dataLeftComparisonWithNorm") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftComparisonWithNorm;
                }
                textBlock1 = page.FindName("dataRightComparisonWithNorm") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightComparisonWithNorm;
                }
                textBlock1 = page.FindName("dataLeftPhaseElectricalConductivity") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftPhaseElectricalConductivity;
                }
                textBlock1 = page.FindName("dataRightPhaseElectricalConductivity") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightPhaseElectricalConductivity;
                }
                textBlock1 = page.FindName("dataAgeElectricalConductivityReference") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataAgeElectricalConductivityReference;
                }
                textBlock1 = page.FindName("dataLeftAgeElectricalConductivity") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftAgeElectricalConductivity;
                }
                textBlock1 = page.FindName("dataRightAgeElectricalConductivity") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightAgeElectricalConductivity;
                }
                textBlock1 = page.FindName("dataExamConclusion") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataExamConclusion;
                }
                textBlock1 = page.FindName("dataLeftMammaryGland") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftMammaryGland;
                }

                textBlock1 = page.FindName("dataLeftMeanECOfLesion") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftMeanECOfLesion;
                }

                textBlock1 = page.FindName("dataLeftAgeRelated") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftAgeRelated;
                }

                textBlock1 = page.FindName("dataLeftMammaryGlandResult") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftMammaryGlandResult;
                }
                textBlock1 = page.FindName("dataRightMammaryGland") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightMammaryGland;
                }

                textBlock1 = page.FindName("dataRightMeanECOfLesion") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightMeanECOfLesion;
                }

                textBlock1 = page.FindName("dataRightAgeRelated") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightAgeRelated;
                }

                textBlock1 = page.FindName("dataRightMammaryGlandResult") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightMammaryGlandResult;
                }
                textBlock1 = page.FindName("dataTotalPts") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataTotalPts;
                }
                textBlock1 = page.FindName("dataLeftTotalPts") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftTotalPts;
                }
                textBlock1 = page.FindName("dataRightTotalPts") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightTotalPts;
                }
                textBlock1 = page.FindName("dataPoint") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataPoint;
                }
                textBlock1 = page.FindName("dataBiRadsCategory") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataBiRadsCategory;
                }
                textBlock1 = page.FindName("dataLeftBiRadsCategory") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftBiRadsCategory;
                }
                textBlock1 = page.FindName("dataRightBiRadsCategory") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightBiRadsCategory;
                }
                textBlock1 = page.FindName("dataRecommendation") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRecommendation;
                }
                textBlock1 = page.FindName("dataRecommendationEN") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataComments;
                }
                textBlock1 = page.FindName("dataFurtherExam") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataFurtherExam) ? "N/A" : reportModel.DataFurtherExam;
                }
                textBlock1 = page.FindName("dataConclusion") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataConclusion;
                }
                textBlock1 = page.FindName("dataConclusionEN") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataConclusion2;
                }
                textBlock1 = page.FindName("dataComments") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataComments) ? "N/A" : reportModel.DataComments;
                }
                textBlock1 = page.FindName("dataMeikTech") as TextBlock;
                if (textBlock1 != null)
                {                    
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataMeikTech) ? "N/A" : reportModel.DataMeikTech;
                }
                textBlock1 = page.FindName("dataDoctor") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataDoctor;
                }
                textBlock1 = page.FindName("dataSignDate") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataSignDate;
                }

                if (!string.IsNullOrEmpty(reportModel.DataLeftLocation) && page.FindName("L1_Canvas") != null)
                {

                    if (reportModel.DataLeftLocation.StartsWith("12"))
                    {
                        textBlock1 = page.FindName("L1_12") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation.StartsWith("11"))
                    {
                        textBlock1 = page.FindName("L1_11") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation.StartsWith("10"))
                    {
                        textBlock1 = page.FindName("L1_10") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation.StartsWith("9"))
                    {
                        textBlock1 = page.FindName("L1_9") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation.StartsWith("8"))
                    {
                        textBlock1 = page.FindName("L1_8") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation.StartsWith("7"))
                    {
                        textBlock1 = page.FindName("L1_7") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation.StartsWith("6"))
                    {
                        textBlock1 = page.FindName("L1_6") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation.StartsWith("5"))
                    {
                        textBlock1 = page.FindName("L1_5") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation.StartsWith("4"))
                    {
                        textBlock1 = page.FindName("L1_4") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation.StartsWith("3"))
                    {
                        textBlock1 = page.FindName("L1_3") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation.StartsWith("2"))
                    {
                        textBlock1 = page.FindName("L1_2") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation.StartsWith("1"))
                    {
                        textBlock1 = page.FindName("L1_1") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    //if ("L1".Equals(reportModel.DataLeftMaxFlag))
                    //{
                    //    textBlock1.Text = "●";
                    //}
                }

                if (!string.IsNullOrEmpty(reportModel.DataRightLocation) && page.FindName("L1_Canvas") != null)
                {
                    if (reportModel.DataRightLocation.StartsWith("12"))
                    {
                        textBlock1 = page.FindName("R1_12") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation.StartsWith("11"))
                    {
                        textBlock1 = page.FindName("R1_11") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation.StartsWith("10"))
                    {
                        textBlock1 = page.FindName("R1_10") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation.StartsWith("9"))
                    {
                        textBlock1 = page.FindName("R1_9") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation.StartsWith("8"))
                    {
                        textBlock1 = page.FindName("R1_8") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation.StartsWith("7"))
                    {
                        textBlock1 = page.FindName("R1_7") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation.StartsWith("6"))
                    {
                        textBlock1 = page.FindName("R1_6") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation.StartsWith("5"))
                    {
                        textBlock1 = page.FindName("R1_5") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation.StartsWith("4"))
                    {
                        textBlock1 = page.FindName("R1_4") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation.StartsWith("3"))
                    {
                        textBlock1 = page.FindName("R1_3") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation.StartsWith("2"))
                    {
                        textBlock1 = page.FindName("R1_2") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation.StartsWith("1"))
                    {
                        textBlock1 = page.FindName("R1_1") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    //if ("R1".Equals(reportModel.DataRightMaxFlag))
                    //{
                    //    textBlock1.Text = "●";
                    //}

                }

                if (!string.IsNullOrEmpty(reportModel.DataLeftLocation2) && page.FindName("L1_Canvas") != null)
                {
                    if (reportModel.DataLeftLocation2.StartsWith("12"))
                    {
                        textBlock1 = page.FindName("L2_12") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation2.StartsWith("11"))
                    {
                        textBlock1 = page.FindName("L2_11") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation2.StartsWith("10"))
                    {
                        textBlock1 = page.FindName("L2_10") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation2.StartsWith("9"))
                    {
                        textBlock1 = page.FindName("L2_9") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation2.StartsWith("8"))
                    {
                        textBlock1 = page.FindName("L2_8") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation2.StartsWith("7"))
                    {
                        textBlock1 = page.FindName("L2_7") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation2.StartsWith("6"))
                    {
                        textBlock1 = page.FindName("L2_6") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation2.StartsWith("5"))
                    {
                        textBlock1 = page.FindName("L2_5") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation2.StartsWith("4"))
                    {
                        textBlock1 = page.FindName("L2_4") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation2.StartsWith("3"))
                    {
                        textBlock1 = page.FindName("L2_3") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation2.StartsWith("2"))
                    {
                        textBlock1 = page.FindName("L2_2") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation2.StartsWith("1"))
                    {
                        textBlock1 = page.FindName("L2_1") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    //if ("L2".Equals(reportModel.DataLeftMaxFlag))
                    //{
                    //    textBlock1.Text = "●";
                    //}

                }

                if (!string.IsNullOrEmpty(reportModel.DataRightLocation2) && page.FindName("L1_Canvas") != null)
                {
                    if (reportModel.DataRightLocation2.StartsWith("12"))
                    {
                        textBlock1 = page.FindName("R2_12") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation2.StartsWith("11"))
                    {
                        textBlock1 = page.FindName("R2_11") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation2.StartsWith("10"))
                    {
                        textBlock1 = page.FindName("R2_10") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation2.StartsWith("9"))
                    {
                        textBlock1 = page.FindName("R2_9") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation2.StartsWith("8"))
                    {
                        textBlock1 = page.FindName("R2_8") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation2.StartsWith("7"))
                    {
                        textBlock1 = page.FindName("R2_7") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation2.StartsWith("6"))
                    {
                        textBlock1 = page.FindName("R2_6") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation2.StartsWith("5"))
                    {
                        textBlock1 = page.FindName("R2_5") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation2.StartsWith("4"))
                    {
                        textBlock1 = page.FindName("R2_4") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation2.StartsWith("3"))
                    {
                        textBlock1 = page.FindName("R2_3") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation2.StartsWith("2"))
                    {
                        textBlock1 = page.FindName("R2_2") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation2.StartsWith("1"))
                    {
                        textBlock1 = page.FindName("R2_1") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    //if ("R2".Equals(reportModel.DataRightMaxFlag))
                    //{
                    //    textBlock1.Text = "●";
                    //}

                }

                if (!string.IsNullOrEmpty(reportModel.DataLeftLocation3) && page.FindName("L1_Canvas") != null)
                {
                    if (reportModel.DataLeftLocation3.StartsWith("12"))
                    {
                        textBlock1 = page.FindName("L3_12") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation3.StartsWith("11"))
                    {
                        textBlock1 = page.FindName("L3_11") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation3.StartsWith("10"))
                    {
                        textBlock1 = page.FindName("L3_10") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation3.StartsWith("9"))
                    {
                        textBlock1 = page.FindName("L3_9") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation3.StartsWith("8"))
                    {
                        textBlock1 = page.FindName("L3_8") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation3.StartsWith("7"))
                    {
                        textBlock1 = page.FindName("L3_7") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation3.StartsWith("6"))
                    {
                        textBlock1 = page.FindName("L3_6") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation3.StartsWith("5"))
                    {
                        textBlock1 = page.FindName("L3_5") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation3.StartsWith("4"))
                    {
                        textBlock1 = page.FindName("L3_4") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation3.StartsWith("3"))
                    {
                        textBlock1 = page.FindName("L3_3") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation3.StartsWith("2"))
                    {
                        textBlock1 = page.FindName("L3_2") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation3.StartsWith("1"))
                    {
                        textBlock1 = page.FindName("L3_1") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    //if ("L3".Equals(reportModel.DataLeftMaxFlag))
                    //{
                    //    textBlock1.Text = "●";
                    //}
                }


                if (!string.IsNullOrEmpty(reportModel.DataRightLocation3) && page.FindName("L1_Canvas") != null)
                {
                    if (reportModel.DataRightLocation3.StartsWith("12"))
                    {
                        textBlock1 = page.FindName("R3_12") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation3.StartsWith("11"))
                    {
                        textBlock1 = page.FindName("R3_11") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation3.StartsWith("10"))
                    {
                        textBlock1 = page.FindName("R3_10") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation3.StartsWith("9"))
                    {
                        textBlock1 = page.FindName("R3_9") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation3.StartsWith("8"))
                    {
                        textBlock1 = page.FindName("R3_8") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation3.StartsWith("7"))
                    {
                        textBlock1 = page.FindName("R3_7") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation3.StartsWith("6"))
                    {
                        textBlock1 = page.FindName("R3_6") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation3.StartsWith("5"))
                    {
                        textBlock1 = page.FindName("R3_5") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation3.StartsWith("4"))
                    {
                        textBlock1 = page.FindName("R3_4") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation3.StartsWith("3"))
                    {
                        textBlock1 = page.FindName("R3_3") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation3.StartsWith("2"))
                    {
                        textBlock1 = page.FindName("R3_2") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation3.StartsWith("1"))
                    {
                        textBlock1 = page.FindName("R3_1") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    //if ("R3".Equals(reportModel.DataRightMaxFlag))
                    //{
                    //    textBlock1.Text = "●";
                    //}

                }
            }
        }

        private void saveAsBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog.ShowDialog();
            string folderName = folderBrowserDialog.SelectedPath;
            if (!string.IsNullOrEmpty(folderName))
            {
                SaveReport(folderName);
                MessageBox.Show(this, App.Current.FindResource("Message_6").ToString());
            }

        }

        private string FindUserReportData(string folderName)
        {
            var person = this.CodeListBox.SelectedItem as Person; 
            //遍历指定文件夹下所有文件
            DirectoryInfo theFolder = new DirectoryInfo(folderName);
            DirectoryInfo[] dirInfo = theFolder.GetDirectories();
            List<DirectoryInfo> list = dirInfo.ToList();
            list.Add(theFolder);
            dirInfo = list.ToArray();
            foreach (DirectoryInfo NextFolder in dirInfo)
            {
                FileInfo[] fileInfo = null;
                try
                {
                    fileInfo = NextFolder.GetFiles();
                    //遍历文件
                    foreach (FileInfo NextFile in fileInfo)
                    {
                        if ((person.Code + ".dat").Equals(NextFile.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            return NextFile.FullName;
                        }
                    }
                }
                catch (Exception) { }
            }
            return null;
        }

        private string FindMEIKXmlReport(string folderName)
        {
            var person = this.CodeListBox.SelectedItem as Person; 
            //遍历指定文件夹下所有文件
            DirectoryInfo theFolder = new DirectoryInfo(folderName);
            DirectoryInfo[] dirInfo = theFolder.GetDirectories();
            List<DirectoryInfo> list = dirInfo.ToList();
            list.Add(theFolder);
            dirInfo = list.ToArray();
            foreach (DirectoryInfo NextFolder in dirInfo)
            {
                FileInfo[] fileInfo = null;
                try
                {
                    fileInfo = NextFolder.GetFiles();
                    //遍历文件
                    foreach (FileInfo NextFile in fileInfo)
                    {
                        if ((person.Code + ".xml").Equals(NextFile.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            return NextFile.FullName;
                        }
                    }
                }
                catch (Exception) { }
            }
            return null;
        }

        private void btnScreenShot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var person = this.CodeListBox.SelectedItem as Person; 
                this.Owner.Visibility = Visibility.Hidden;
                this.WindowState = WindowState.Minimized;
                App.opendWin = this;
                MEIKReport.Views.ScreenCapture screenCaptureWin = new MEIKReport.Views.ScreenCapture(person);
                screenCaptureWin.callbackMethod = LoadScreenShot;
                screenCaptureWin.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "System Exception: " + ex.Message);
            }
        }

        private void LoadScreenShot(Object imgFileName)
        {
            var screenShotImg = ImageTools.GetBitmapImage(imgFileName as string);
            if (screenShotImg != null)
            {
                var stream = screenShotImg.StreamSource;
                stream.Position = 0;
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                stream.Flush();
                shortFormReportModel.DataScreenShotImg = buffer;

                //this.btnScreenShot.Content = App.Current.FindResource("ReportContext_170").ToString();                
                //this.btnRemoveImg.Visibility = Visibility.Visible;
            }

            //dataScreenShotImg.Source = ImageTools.GetBitmapImage(imgFileName as string);
        }

        private void btnViewImg_Click(object sender, RoutedEventArgs e)
        {
            if (shortFormReportModel.DataScreenShotImg == null)
            {
                MessageBox.Show(this, App.Current.FindResource("Message_34").ToString());
            }
            else
            {
                ViewImagePage viewImagePage = new ViewImagePage(shortFormReportModel.DataScreenShotImg);
                viewImagePage.ShowDialog();
            }
        }

        /// <summary>
        /// 選擇生成PDF報表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void savePdfComb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var person = this.CodeListBox.SelectedItem as Person; 
            var comboBox = sender as ComboBox;
            if (comboBox.SelectedIndex == 2)
            {
                try
                {
                    if (!Directory.Exists(dataFolder))
                    {
                        Directory.CreateDirectory(dataFolder);
                    }

                    LoadDataModel();
                    //Clone生成全文本的报表数据对象模型
                    var reportModel = CloneReportModel();
                    //打开文件夹对话框，选择要保存的目录
                    System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
                    folderBrowserDialog.SelectedPath = person.ArchiveFolder;
                    System.Windows.Forms.DialogResult res = folderBrowserDialog.ShowDialog();
                    if (res == System.Windows.Forms.DialogResult.OK)
                    {
                        string folderName = folderBrowserDialog.SelectedPath;
                        string strName = person.SurName + (string.IsNullOrEmpty(person.GivenName) ? "" : "," + person.GivenName) + (string.IsNullOrEmpty(person.OtherName) ? "" : " " + person.OtherName) + ".pdf";
                        //生成Examination报告的PDF文件
                        string lfPdfFile = folderName + System.IO.Path.DirectorySeparatorChar + person.Code + " LF - " + strName;
                        string lfReportTempl = "Views/GoodUnionReportFlow.xaml";
                        ExportFlowDocumentPDF(lfReportTempl, lfPdfFile, reportModel);

                        //生成Summary报告的PDF文件
                        string sfPdfFile = folderName + System.IO.Path.DirectorySeparatorChar + person.Code + " SFI - " + strName;
                        string sfReportTempl = "Views/SummaryReportFlow.xaml";
                        var clientReportModel = CloneClientReportModel();
                        ExportFlowDocumentPDF(sfReportTempl, sfPdfFile, clientReportModel, "A4");

                        MessageBox.Show(this, App.Current.FindResource("Message_5").ToString());
                    }
                }
                catch (Exception ex)
                {
                    FileHelper.SetFolderPower(dataFolder, "Everyone", "FullControl");
                    FileHelper.SetFolderPower(dataFolder, "Users", "FullControl");
                    MessageBox.Show(this, ex.Message);
                }
            }
            else if (comboBox.SelectedIndex == 1)
            {
                try
                {
                    if (!Directory.Exists(dataFolder))
                    {
                        Directory.CreateDirectory(dataFolder);
                    }

                    LoadDataModel();
                    //Clone生成全文本的报表数据对象模型
                    var reportModel = CloneReportModel();
                    //打开文件夹对话框，选择要保存的目录
                    System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
                    folderBrowserDialog.SelectedPath = person.ArchiveFolder;
                    System.Windows.Forms.DialogResult res = folderBrowserDialog.ShowDialog();
                    if (res == System.Windows.Forms.DialogResult.OK)
                    {
                        string folderName = folderBrowserDialog.SelectedPath;
                        string strName = person.SurName + (string.IsNullOrEmpty(person.GivenName) ? "" : "," + person.GivenName) + (string.IsNullOrEmpty(person.OtherName) ? "" : " " + person.OtherName) + ".pdf";
                        //生成Examination报告的PDF文件
                        string lfPdfFile = folderName + System.IO.Path.DirectorySeparatorChar + person.Code + " LF - " + strName;
                        string lfReportTempl = "Views/GoodUnionReportFlow.xaml";
                        ExportFlowDocumentPDF(lfReportTempl, lfPdfFile, reportModel);

                        //生成Summary报告的PDF文件
                        string sfPdfFile = folderName + System.IO.Path.DirectorySeparatorChar + person.Code + " SFI - " + strName;
                        string sfReportTempl = "Views/SummaryReportDocument.xaml";
                        if (shortFormReportModel.DataScreenShotImg != null)
                        {
                            sfReportTempl = "Views/SummaryReportNuvoTekDocument.xaml";
                        }

                        ExportPDF(sfReportTempl, sfPdfFile, reportModel);

                        MessageBox.Show(this, App.Current.FindResource("Message_5").ToString());
                    }
                }
                catch (Exception ex)
                {
                    FileHelper.SetFolderPower(dataFolder, "Everyone", "FullControl");
                    FileHelper.SetFolderPower(dataFolder, "Users", "FullControl");
                    MessageBox.Show(this, ex.Message);
                }
            }

        }

        #endregion

        /// <summary>
        /// 显示自动化检查结果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnResult_Click(object sender, RoutedEventArgs e)
        {
           
            var person = this.CodeListBox.SelectedItem as Person;
            var reportModel = CloneReportModel();
            ScreenResultReport screenResultReport = new ScreenResultReport(person, reportModel);
            screenResultReport.ShowDialog();
            
        }

               
    }
}
