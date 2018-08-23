using MEIKReport.Common;
using MEIKReport.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MEIKReport
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //private const int MINIMUM_SPLASH_TIME = 500; // Miliseconds
        //private const int SPLASH_FADE_TIME = 500;     // Miliseconds
        public static IntPtr splashWinHwnd = IntPtr.Zero;
        public static IntPtr meikWinHwnd = IntPtr.Zero;
        public static Window opendWin = null;
        public static string dataFolder = null;
        public static ReportSettingModel reportSettingModel = null;
        //原始MIEK程序的根目录
        //public static string meikFolder = OperateIniFile.ReadIniData("Base", "MEIK base", "C:\\Program Files (x86)\\MEIK 5.6", System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
        //MEIKMA程序的根目錄
        public static string meikFolder = System.AppDomain.CurrentDomain.BaseDirectory + System.IO.Path.DirectorySeparatorChar + "MEIKMD";
        public static string meikIniFilePath = App.meikFolder + System.IO.Path.DirectorySeparatorChar + "MEIKMD.ini";
        //统计扫描次数的字典
        public static SortedDictionary<string, List<long>> countDictionary = new SortedDictionary<string, List<long>>();

        public static string local = "en-US";
        public static string strScreening = "Screening";
        public static string strDiagnostics = "Diagnostics";
        public static string strExit = "Exit";
        //public static string strMeasurement = "Measurement";
        public static string strStart = "Start";
        public static List<string> uploadedCodeList = new List<string>();

        protected override void OnStartup(StartupEventArgs e)
        {
            //// Step 1 - Load the splash screen
            //SplashScreen splash = new SplashScreen("splash.png");
            //splash.Show(false, true);

            //// Step 2 - Start a stop watch
            //Stopwatch timer = new Stopwatch();
            //timer.Start();

            //// Step 3 - Load your windows but don't show it yet
            //base.OnStartup(e);
            //MainWindow1 main = new MainWindow1();

            //// Step 4 - Make sure that the splash screen lasts at least two seconds
            //timer.Stop();
            //int remainingTimeToShowSplash = MINIMUM_SPLASH_TIME - (int)timer.ElapsedMilliseconds;
            //if (remainingTimeToShowSplash > 0)
            //    Thread.Sleep(remainingTimeToShowSplash);

            //// Step 5 - show the page
            //splash.Close(TimeSpan.FromMilliseconds(SPLASH_FADE_TIME));
            //main.Show();

            //RunAsAdministrator(); 
 

            //if (e.Args != null && e.Args.Count() > 0)
            //{
            //    this.Properties["startexe"] = e.Args[0];
            //    this.Properties["version"] = e.Args[1];
            //}
            //base.OnStartup(e); 

        }

        private void RunAsAdministrator()
        {
            /** 
            * 当前用户是管理员的时候，直接启动应用程序 
            * 如果不是管理员，则使用启动对象启动程序，以确保使用管理员身份运行 
            */
            //获得当前登录的Windows用户标示 
            System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            //创建Windows用户主题 
            System.Windows.Forms.Application.EnableVisualStyles();

            System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
            //判断当前登录用户是否为管理员 
            if (principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator))
            {                
                //如果是管理员，则直接运行 
                MainWindow main = new MainWindow();
                main.Show();                         
            }
            else
            {                
                //创建启动对象 
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                //设置运行文件 
                startInfo.FileName = System.Windows.Forms.Application.ExecutablePath;                
                //设置启动参数 
                //startInfo.Arguments = String.Join(" ", Args);
                //设置启动动作,确保以管理员身份运行 
                startInfo.Verb = "runas";
                //如果不是管理员，则启动UAC 
                System.Diagnostics.Process.Start(startInfo);
                //退出 
                this.Shutdown();
                //System.Windows.Forms.Application.Exit();
            } 
        }

        /// <summary>
        /// 获取当前操作系统版本
        /// </summary>
        private void GetOsVersion()
        {
            // Get OperatingSystem information from the system namespace.
            System.OperatingSystem osInfo = System.Environment.OSVersion;

            // Determine the platform.
            switch (osInfo.Platform)
            {
                // Platform is Windows 95, Windows 98, 
                // Windows 98 Second Edition, or Windows Me.
                case System.PlatformID.Win32Windows:

                    switch (osInfo.Version.Minor)
                    {
                        case 0:
                            Console.WriteLine("Windows 95");
                            break;
                        case 10:
                            if (osInfo.Version.Revision.ToString() == "2222A")
                                Console.WriteLine("Windows 98 Second Edition");
                            else
                                Console.WriteLine("Windows 98");
                            break;
                        case 90:
                            Console.WriteLine("Windows Me");
                            break;
                    }
                    break;

                // Platform is Windows NT 3.51, Windows NT 4.0, Windows 2000,
                // or Windows XP.
                case System.PlatformID.Win32NT:

                    switch (osInfo.Version.Major)
                    {
                        case 3:
                            Console.WriteLine("Windows NT 3.51");
                            break;
                        case 4:
                            Console.WriteLine("Windows NT 4.0");
                            break;
                        case 5:
                            if (osInfo.Version.Minor == 0)
                                Console.WriteLine("Windows 2000");
                            else
                                Console.WriteLine("Windows XP");
                            break;
                    } break;
            }
        }       
    }
}
