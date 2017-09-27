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
        }

        public ViewImagePage(Person person,byte[] screenShotImg)
        {
            this._person = person;
            InitializeComponent();
            if (screenShotImg != null)
            {
                this.dataScreenShotImg.Source = ImageTools.GetBitmapImage(screenShotImg);
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
                    IntPtr mainWinHwnd = Win32Api.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "TfmMain", null);
                    //如果當前顯示的是主窗体                    
                    if (mainWinHwnd == IntPtr.Zero)
                    {
                        IntPtr screenBtnHwnd = Win32Api.FindWindowEx(App.splashWinHwnd, IntPtr.Zero, null, App.strScreening);
                        Win32Api.SendMessage(screenBtnHwnd, Win32Api.WM_CLICK, 0, 0);
                    }
                }
                catch { }

                this.WindowState = WindowState.Minimized;     
                //this.Owner.Visibility = Visibility.Hidden;
                this.Owner.WindowState = WindowState.Minimized;
                App.opendWin = this;
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
                    var screenShotImg = ImageTools.GetBitmapImage(imgFileName as string);
                    if (screenShotImg != null)
                    {
                        var stream = screenShotImg.StreamSource;
                        stream.Position = 0;
                        byte[] buffer = new byte[stream.Length];
                        stream.Read(buffer, 0, buffer.Length);
                        stream.Flush();
                        //this.dataScreenShotImg.Source = ImageTools.GetBitmapImage(buffer);
                        this.dataScreenShotImg.Source = ImageTools.GetBitmapImage(imgFileName as string);

                        //獲取結果狀態
                        this.getResult();
                        _person.Result = _person.LeftResult > _person.RightResult ? _person.LeftResult : _person.RightResult;
                        OperateIniFile.WriteIniData("Report", "Result", _person.Result+"", _person.IniFilePath);
                    }
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
