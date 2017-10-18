using MEIKScreen.Common;
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

namespace MEIKScreen
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
            //labDeviceNo.Content = deviceNo;
            this.Visibility = Visibility.Collapsed;
            //string MeikDataBaseFolder = OperateIniFile.ReadIniData("Base", "Data base", "C:\MEIKData", System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
            //string dayFolder = MeikDataBaseFolder + System.IO.Path.DirectorySeparatorChar + DateTime.Now.ToString("MM_yyyy") + System.IO.Path.DirectorySeparatorChar + DateTime.Now.ToString("dd");
            //if (!Directory.Exists(dayFolder))
            //{
            //    Directory.CreateDirectory(dayFolder);
            //}
            //App.dataFolder = dayFolder;
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

        public void StartApp(string archiveFolder)
        {
            try
            {
                //App.splashWinHwnd = Win32Api.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "TfmSplash", null);
                //判斷是否有MEIKMA的主介面
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
            string meikPath = App.meikFolder + System.IO.Path.DirectorySeparatorChar + "MEIKMA.exe";            
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
                    //if (AppProc != null)
                    //{
                        
                        
                        //AppProc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden; //隐藏
                        //if (System.Environment.OSVersion.Version.Major >= 6)
                        //{
                        //    AppProc.StartInfo.Verb = "runas";
                        //}
                        //proc.WaitForExit();//等待外部程序退出后才能往下执行
                        AppProc.WaitForInputIdle();
                        /**这段代码是把其他程序的窗体嵌入到当前窗体中**/
                        IntPtr appWinHandle = AppProc.MainWindowHandle;
                        //App.splashWinHwnd = Win32Api.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "TfmSplash", null);                        
                        App.meikWinHwnd = Win32Api.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "TfmMain", null);
                        //最小化MEIKMA窗口
                        //Win32Api.ShowWindow(App.meikWinHwnd, 6);
                        
                        //监视进程退出
                        //AppProc.EnableRaisingEvents = false;
                        //指定退出事件方法
                        //AppProc.Exited += new EventHandler(proc_Exited);
                        ////设置程序嵌入到当前窗体
                        //Win32Api.MoveWindow(App.splashWinHwnd, 0, 0, 720, 576, true);
                        //// Set new process's parent to this window   
                        //Win32Api.SetParent(App.splashWinHwnd, meikPanel.Handle);
                        ////Add WS_CHILD window style to child window 
                        //const int GWL_STYLE = -16;
                        //const int WS_CHILD = 0x40000000;
                        //int style = Win32Api.GetWindowLong(App.splashWinHwnd, GWL_STYLE);
                        //style = style | WS_CHILD;
                        //Win32Api.SetWindowLong(App.splashWinHwnd, GWL_STYLE, style);

                        

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(App.Current.FindResource("Message_1").ToString() + " " + ex.Message);
                }                
            }
            else
            {
                MessageBox.Show("Failed to run MEIK program, file MEIKMA.exe is missing or corrupt.");
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.DragMove();
        }
        
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
        ///启动外部程序退出事件
        /// </summary>
        void proc_Exited(object sender, EventArgs e)
        {
            //this.Dispatcher.Invoke(new Action(delegate { this.Close(); }));
            //this.Dispatcher.Invoke(new Action(delegate {                
            //    App.opendWin.WindowState = WindowState.Maximized; 
            //}));            
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
                IntPtr exitButtonHandle = Win32Api.WindowFromPoint(e.X, e.Y);
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
                        //this.StopMouseHook();
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
