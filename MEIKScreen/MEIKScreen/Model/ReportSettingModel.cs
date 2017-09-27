using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MEIKScreen.Model
{
    public class ReportSettingModel : ViewModelBase
    {
        private string version = null;
        public string Version
        {
            get { return version; }
            set { version = value; OnPropertyChanged("Version"); }
        }

        private string license = null;
        public string License
        {
            get { return license; }
            set { license = value; }
        } 

        private string dataBaseFolder = null;
        public string DataBaseFolder
        {
            get { return dataBaseFolder; }
            set { dataBaseFolder = value; OnPropertyChanged("DataBaseFolder"); }
        }

        private string cloudPath = null;
        public string CloudPath
        {
            get { return cloudPath; }
            set { cloudPath = value; OnPropertyChanged("CloudPath"); }
        }
        private string cloudUser = null;
        public string CloudUser
        {
            get { return cloudUser; }
            set { cloudUser = value; }
        }

        private string cloudToken = null;
        public string CloudToken
        {
            get { return cloudToken; }
            set { cloudToken = value; }
        }
        
        private ObservableCollection<User> techNames = new ObservableCollection<User>();

        public ObservableCollection<User> TechNames
        {
            get { return techNames; }
            set { techNames = value; OnPropertyChanged("TechNames"); }
        }

        private bool showDoctorSignature;

        public bool ShowDoctorSignature
        {
            get { return showDoctorSignature; }
            set { showDoctorSignature = value; OnPropertyChanged("ShowDoctorSignature"); }
        }

        private bool showTechSignature;

        public bool ShowTechSignature
        {
            get { return showTechSignature; }
            set { showTechSignature = value; OnPropertyChanged("ShowTechSignature"); }
        }

        private string meikBase = @"C:\Program Files (x86)\MEIK 5.6";

        public string MeikBase
        {
            get { return meikBase; }
            set { meikBase = value; OnPropertyChanged("MeikBase"); }
        }

        private bool useDefaultSignature;

        public bool UseDefaultSignature
        {
            get { return useDefaultSignature; }
            set { useDefaultSignature = value; OnPropertyChanged("UseDefaultSignature"); }
        }

        private TransferMode transferMode;

        public TransferMode TransferMode
        {
            get { return transferMode; }
            set { transferMode = value; OnPropertyChanged("TransferMode"); }
        }
        private string mailAddress = null;

        public string MailAddress
        {
            get { return mailAddress; }
            set { mailAddress = value; OnPropertyChanged("MailAddress"); }
        }
        private string toMailAddress = null;

        public string ToMailAddress
        {
            get { return toMailAddress; }
            set { toMailAddress = value; OnPropertyChanged("ToMailAddress"); }
        }

        private ObservableCollection<string> toMailAddressList = new ObservableCollection<string>();
        public ObservableCollection<string> ToMailAddressList
        {
            get { return toMailAddressList; }
            set { toMailAddressList = value; OnPropertyChanged("ToMailAddressList"); }
        }

        private string mailSubject = null;

        public string MailSubject
        {
            get { return mailSubject; }
            set { mailSubject = value; OnPropertyChanged("MailSubject"); }
        }

        private string mailBody = null;

        public string MailBody
        {
            get { return mailBody; }
            set { mailBody = value; OnPropertyChanged("MailBody"); }
        }

        private string mailHost = null;

        public string MailHost
        {
            get { return mailHost; }
            set { mailHost = value; OnPropertyChanged("MailHost"); }
        }
        private int mailPort = 25;

        public int MailPort
        {
            get { return mailPort; }
            set { mailPort = value; OnPropertyChanged("MailPort"); }
        }
        private string mailUsername = null;

        public string MailUsername
        {
            get { return mailUsername; }
            set { mailUsername = value; OnPropertyChanged("MailUsername"); }
        }
        private string mailPwd = null;

        public string MailPwd
        {
            get { return mailPwd; }
            set { mailPwd = value; OnPropertyChanged("MailPwd"); }
        }

        private bool mailSsl;

        public bool MailSsl
        {
            get { return mailSsl; }
            set { mailSsl = value; OnPropertyChanged("MailSsl"); }
        }

        private string deviceNo = "000";

        public string DeviceNo
        {
            get { return deviceNo; }
            set { deviceNo = value; OnPropertyChanged("DeviceNo"); }
        }
        

        private string recordDate;
        public string RecordDate
        {
            get { return recordDate; }
            set { recordDate = value; OnPropertyChanged("RecordDate"); }
        }


        /// <summary>
        /// 临时变量，保存提供当前数据的检测员名称
        /// </summary>
        private string reportTechName = "";
        public string ReportTechName
        {
            get { return reportTechName; }
            set { reportTechName = value; OnPropertyChanged("ReportTechName"); }
        }

        /// <summary>
        /// 临时变量，保存提供当前数据的检测员证书
        /// </summary>
        private string reportTechLicense = "";
        public string ReportTechLicense
        {
            get { return reportTechLicense; }
            set { reportTechLicense = value; OnPropertyChanged("ReportTechLicense"); }
        }

        //檢查地點
        private string screenVenue;
        public string ScreenVenue
        {
            get { return screenVenue; }
            set { screenVenue = value; OnPropertyChanged("ScreenVenue"); }
        }

        //是否開啟搜索用戶功能
        private bool enableSearchUser;
        public bool EableSearchUser
        {
            get { return enableSearchUser; }
            set { enableSearchUser = value; OnPropertyChanged("EableSearchUser"); }
        }

    }
}
