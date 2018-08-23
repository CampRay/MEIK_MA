using MEIKReport.Common;
using MEIKReport.Views;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Interop;
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
    public partial class MainWindow : Window
    {        
        //定义将要运行的原始的MEIK程序的进程
        public Process AppProc { get; set; }
        //public IntPtr splashWinHwnd = IntPtr.Zero;
        protected MouseHook mouseHook = new MouseHook();
        protected KeyboardHook keyboardHook = new KeyboardHook();  
        //private WindowListen windowListen = new WindowListen();               
        
        
        public MainWindow()
        {
            string local = OperateIniFile.ReadIniData("Base", "Language", "en-US", System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
            if (string.IsNullOrEmpty(local))
            {
                local = "en-US";
            }
            if (File.Exists(App.meikFolder +System.IO.Path.DirectorySeparatorChar+ "Language.CHN"))
            {                
                App.strScreening = "筛选";
                App.strDiagnostics = "诊断";
                App.strExit = "退出";
                App.strStart = "开始";
                local = "zh-CN";
            }            
            //else
            //{
            //    App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(@"/Resources/StringResource.xaml", UriKind.RelativeOrAbsolute) });
            //}
            App.local = local;
            if ("zh-HK".Equals(local))
            {
                App.Current.Resources.MergedDictionaries.Remove(new ResourceDictionary() { Source = new Uri(@"/Resources/StringResource.xaml", UriKind.RelativeOrAbsolute) });
                App.Current.Resources.MergedDictionaries.Remove(new ResourceDictionary() { Source = new Uri(@"/Resources/StringResource.zh-CN.xaml", UriKind.RelativeOrAbsolute) });
                App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(@"/Resources/StringResource.zh-HK.xaml", UriKind.RelativeOrAbsolute) });
            }
            else if ("zh-CN".Equals(local))
            {
                App.Current.Resources.MergedDictionaries.Remove(new ResourceDictionary() { Source = new Uri(@"/Resources/StringResource.xaml", UriKind.RelativeOrAbsolute) });
                App.Current.Resources.MergedDictionaries.Remove(new ResourceDictionary() { Source = new Uri(@"/Resources/StringResource.zh-HK.xaml", UriKind.RelativeOrAbsolute) });
                App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(@"/Resources/StringResource.zh-CN.xaml", UriKind.RelativeOrAbsolute) });
            }
            else
            {
                App.Current.Resources.MergedDictionaries.Remove(new ResourceDictionary() { Source = new Uri(@"/Resources/StringResource.zh-CN.xaml", UriKind.RelativeOrAbsolute) });
                App.Current.Resources.MergedDictionaries.Remove(new ResourceDictionary() { Source = new Uri(@"/Resources/StringResource.zh-HK.xaml", UriKind.RelativeOrAbsolute) });
                App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(@"/Resources/StringResource.xaml", UriKind.RelativeOrAbsolute) });
            }
            InitializeComponent();            
            this.Visibility = Visibility.Collapsed;
        }
        
               
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {                        
            //StartApp();
            mouseHook.MouseUp += new System.Windows.Forms.MouseEventHandler(mouseHook_MouseUp);
            btnReport_Click(null, null);
            //启用键盘钩子
            //keyboardHook.KeyDown += new System.Windows.Forms.KeyEventHandler(keyboardHook_KeyDown);
            //keyboardHook.Start();            
            //添加全局消息过滤方法
            //GlobalMouseHandler globalClick = new GlobalMouseHandler();
            //Application.AddMessageFilter(globalClick);
        }

        /// <summary>
        /// 启用鼠标钩子
        /// </summary>
        public void StartMouseHook(){
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

        public void StartApp(string archiveFolder)
        {
            try
            {
                //判斷是否有MEIKMD的主介面
                App.meikWinHwnd = Win32Api.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "TfmMain", null);
                if (Win32Api.IsWindow(App.meikWinHwnd))//MEIK程序已经启动时
                {
                    try
                    {
                        IntPtr exitBtnHwnd = Win32Api.FindWindowEx(App.meikWinHwnd, IntPtr.Zero, null, App.strExit);
                        Win32Api.SendMessage(exitBtnHwnd, Win32Api.WM_CLICK, 0, 0);
                    }
                    catch { }
                }
                StartMeik(archiveFolder);
            }
            catch (Exception ex)
            {
                MessageBox.Show("System Exception: " + ex.Message);
            }
        }

        private void StartMeik(string archiveFolder)
        {            
            //再修改原始MEIK程序中的患者档案目录，让原始MEIK程序运行后直接打开此患者档案
            OperateIniFile.WriteIniData("Base", "Patients base", archiveFolder, App.meikIniFilePath);
            string meikPath = App.meikFolder + System.IO.Path.DirectorySeparatorChar + "MEIKMD.exe";            
            bool meikExists = File.Exists(meikPath);            
            if (meikExists)
            {
                try
                {
                    //启动外部程序
                    //AppProc = Process.Start(meikPath);
                    AppProc = new Process();
                    AppProc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    AppProc.StartInfo.FileName = meikPath;
                    if (System.Environment.OSVersion.Version.Major >= 6)
                    {
                        AppProc.StartInfo.Verb = "runas";//以管理員權限運行
                    }
                    if (AppProc.Start()) {                       
                        AppProc.WaitForInputIdle();                                             
                        App.meikWinHwnd = Win32Api.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "TfmMain", null); 
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(App.Current.FindResource("Message_1").ToString() +" "+ ex.Message);
                }                
            }
            else
            {
                MessageBox.Show("Failed to run MEIK program, file is missing or corrupt.");
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.DragMove();            
        }

        //private void btnScreening_Loaded(object sender, RoutedEventArgs e)
        //{
        //    Button button = (Button)sender;
        //    button.Focus();
        //}

        //private void btnScreening_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        IntPtr screeningBtnHwnd = Win32Api.FindWindowEx(App.splashWinHwnd, IntPtr.Zero, null, App.strScreening);
        //        Win32Api.SendMessage(screeningBtnHwnd, Win32Api.WM_CLICK, 0, 0);
        //        this.StartMouseHook();
        //        this.Visibility = Visibility.Hidden;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("System Exception: "+ex.Message);
        //    }
            
        //}

        //private void btnDiagnostics_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        IntPtr diagnosticsBtnHwnd = Win32Api.FindWindowEx(App.splashWinHwnd, IntPtr.Zero, null, App.strDiagnostics);
        //        Win32Api.SendMessage(diagnosticsBtnHwnd, Win32Api.WM_CLICK, 0, 0);
        //        this.StartMouseHook();
        //        this.Visibility = Visibility.Hidden;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("System Exception: " + ex.Message);
        //    }
                 
        //}         

        private void btnReport_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            UserList userList = new UserList();
            userList.Owner = this;
            userList.Show();

        }

        public void exitMeik()
        {
            try
            {                
                if (App.meikWinHwnd != IntPtr.Zero)
                {
                    IntPtr exitBtnHwnd = Win32Api.FindWindowEx(App.meikWinHwnd, IntPtr.Zero, null, App.strExit);
                    Win32Api.SendMessage(exitBtnHwnd, Win32Api.WM_CLICK, 0, 0);
                }

            }
            catch (Exception)
            {
                //this.Close();
                if (!AppProc.HasExited)
                {
                    AppProc.Kill();
                }
            }
            finally
            {
                this.Close();
            }
        }

        

        /// <summary>
        /// 鼠标按下的钩子回调方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void mouseHook_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                IntPtr exitButtonHandle= Win32Api.WindowFromPoint(e.X, e.Y);
                IntPtr winHandle = Win32Api.GetParent(exitButtonHandle);                
                if (Win32Api.GetParent(winHandle) == AppProc.MainWindowHandle)
                {                    
                    StringBuilder winText = new StringBuilder(512);
                    Win32Api.GetWindowText(exitButtonHandle, winText, winText.Capacity);
                    if (App.strExit.Equals(winText.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        if (App.opendWin != null)
                        {
                            App.opendWin.WindowState = WindowState.Maximized;                                                    
                        }
                        else
                        {
                            this.Visibility = Visibility.Visible;                            
                        }
                        this.StopMouseHook();
                    }
                }                
            }                        
        }

        /// <summary>
        /// 键盘按下的钩子回调方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void keyboardHook_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            MessageBox.Show(sender.GetType().ToString());
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (!AppProc.HasExited)
            {
                AppProc.Kill();
            }
        }

        //private void btnSetting_Click(object sender, RoutedEventArgs e)
        //{
        //    this.Visibility = Visibility.Hidden;
        //    ReportSettingPage reportSettingPage = new ReportSettingPage();
        //    reportSettingPage.Owner = this;
        //    reportSettingPage.Show();
        //}
                
    }
}
