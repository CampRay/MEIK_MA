using MEIKScreen.Common;
using MEIKScreen.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MEIKScreen
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public static IntPtr splashWinHwnd = IntPtr.Zero;
        public static IntPtr meikWinHwnd = IntPtr.Zero;
        public static Window opendWin = null;
        public static string dataFolder = null;
        public static ReportSettingModel reportSettingModel = null;
        //原始MIEK程序的根目录
        //public static string meikFolder = OperateIniFile.ReadIniData("Base", "MEIK base", "C:\\Program Files (x86)\\MEIK 5.6", System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
        //MEIKMA程序的根目錄
        public static string meikFolder = System.AppDomain.CurrentDomain.BaseDirectory + System.IO.Path.DirectorySeparatorChar+"MEIKMA";
        public static string meikIniFilePath = App.meikFolder + System.IO.Path.DirectorySeparatorChar + "MEIKMA.ini";
        //public static string meikFolder = "C:\\Program Files (x86)\\MEIK 5.6";
        //public static string meikIniFilePath =App.meikFolder + System.IO.Path.DirectorySeparatorChar + "MEIK.ini";
        
        //统计扫描次数的字典
        public static SortedDictionary<string, List<long>> countDictionary = new SortedDictionary<string, List<long>>();

        public static string local = "en-US";
        public static string strScreening = "Screening";
        public static string strDiagnostics = "Diagnostics";
        public static string strList = "List";
        public static string strExit = "Exit";
        public static string strMeasurement = "Measurement";
        public static string strStart = "Start";
//        public static List<string> uploadedCodeList = new List<string>();
             
    }
}
