using MEIKScreen.Common;
using MEIKScreen.Model;
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
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using System.Xml;

namespace MEIKScreen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ViewImagePage : Window
    {
        private Person _person;        

        public ViewImagePage(Person person)
        {
            this._person = person;
            InitializeComponent();

            string imgFileName = person.ArchiveFolder + System.IO.Path.DirectorySeparatorChar + person.Code + ".png";
            if (File.Exists(imgFileName))
            {
                this.dataScreenShotImg.Source = ImageTools.GetBitmapImage(imgFileName);
            }
            
        }

        /// <summary>
        /// 屏幕截圖
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnScreenShot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                try
                {
                    string archiveFolder = OperateIniFile.ReadIniData("Base", "Patients base", "", App.meikIniFilePath);                    
                    IntPtr mainWinHwnd = Win32Api.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "TfmMain", null);
                    //如果沒有找到MEIKMA主窗体显示，则重启MEIKMA                    
                    if (mainWinHwnd == IntPtr.Zero)
                    {
                        var mainWin=this.Owner.Owner as MainWindow;
                        mainWin.StartApp(_person.ArchiveFolder);
                    }
                    else
                    {
                        
                        if (!archiveFolder.Equals(_person.ArchiveFolder))
                        {
                            var mainWin = this.Owner.Owner as MainWindow;
                            mainWin.StartApp(_person.ArchiveFolder);
                        }
                    }
                }
                catch { }

                

                this.WindowState = WindowState.Minimized;     
                //this.Owner.Visibility = Visibility.Hidden;
                this.Owner.WindowState = WindowState.Minimized;
                App.opendWin = this;
                //让MEIKMA窗口显示在最前面
                if (App.meikWinHwnd != IntPtr.Zero)
                {
                    Win32Api.ShowWindow(App.meikWinHwnd, 3);
                    Win32Api.SetForegroundWindow(App.meikWinHwnd);
                }

                MEIKScreen.Views.ScreenCapture screenCaptureWin = new MEIKScreen.Views.ScreenCapture(_person);
                screenCaptureWin.callbackMethod = LoadScreenShot;
                screenCaptureWin.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "System Exception: " + ex.Message);
            }
        }
        /// <summary>
        /// 截圖後回調方法
        /// </summary>
        /// <param name="imgFileName"></param>
        private void LoadScreenShot(Object imgFileName)
        {
            try
            {
                if (File.Exists((string)imgFileName))
                {
                    this.dataScreenShotImg.Source = ImageTools.GetBitmapImage(imgFileName as string);                    

                    //獲取結果狀態
                    //this.getResult();
                    //_person.Result = _person.LeftResult > _person.RightResult ? _person.LeftResult : _person.RightResult;
                    //OperateIniFile.WriteIniData("Report", "Result", _person.Result+"", _person.IniFilePath);
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "System Exception: " + ex.Message);
            }
        }

        /// <summary>
        /// 計算自動分析結果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void getResult()
        {
            try
            {
                var dir = new DirectoryInfo(_person.ArchiveFolder);
                FileSystemInfo[] files = dir.GetFileSystemInfos();
                for (int i = 0; i < files.Length; i++)
                {
                    FileInfo file = files[i] as FileInfo;
                    //是文件 
                    if (file != null)
                    {
                        if (".xml".Equals(file.Extension, StringComparison.OrdinalIgnoreCase))
                        {
                            XmlDocument doc = new XmlDocument();
                            doc.Load(file.FullName);
                            var breastNodeList = doc.GetElementsByTagName("Breast");
                            var breastNode = breastNodeList.Item(breastNodeList.Count - 1);
                            string breast = breastNode.InnerText;
                            var oncoNodeList = doc.GetElementsByTagName("Onco");
                            var oncoNode = oncoNodeList.Item(oncoNodeList.Count - 1);
                            int onco = 0;
                            try
                            {
                                onco = Convert.ToInt32(oncoNode.InnerText);
                            }
                            catch { }

                            if ("L".Equals(breast, StringComparison.OrdinalIgnoreCase))
                            {
                                switch (onco)
                                {
                                    case 1:
                                        if (onco > _person.LeftResult)
                                        {
                                            _person.LeftResult = 1;
                                        }
                                        break;
                                    case 2:
                                        if (onco > _person.LeftResult)
                                        {
                                            _person.LeftResult = 2;
                                        }
                                        break;
                                }
                            }
                            if ("R".Equals(breast, StringComparison.OrdinalIgnoreCase))
                            {
                                switch (onco)
                                {
                                    case 1:
                                        if (onco > _person.RightResult)
                                        {
                                            _person.RightResult = 1;
                                        }
                                        break;
                                    case 2:
                                        if (onco > _person.RightResult)
                                        {
                                            _person.RightResult = 2;
                                        }
                                        break;
                                }
                            }
                        }
                    }

                }
            }
            catch(Exception ex) {
                MessageBox.Show(this, "Failed to generate the preliminary result: " + ex.Message);
            }
            
        }

        
        
    }
}
