using MEIKReport.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MEIKReport.Common
{
    public class WindowListen
    {
        //Volatile的作用是告知编译器此数据将被多个线程访问
        private volatile bool _shouldStop = false;        
        public Process AppProc { get; set;}

        private int left =(int) (System.Windows.SystemParameters.PrimaryScreenWidth-300);
        public IntPtr backWinHwnd { get; set; }
        
        public void Run()
        {
            //IntPtr buttonWinHwnd = Win32Api.FindWindow(null, "BackButtonWindow");
            IntPtr appMainWinHwnd = AppProc.MainWindowHandle;
            IntPtr firstWinHwnd = Win32Api.GetForegroundWindow();
            //StringBuilder text=new StringBuilder(512);
            //Win32Api.GetClassName(firstWinHwnd, text, text.Capacity);
            //MessageBox.Show(text.ToString());
            //MessageBox.Show("MainWin:" + appMainWinHwnd + ", FirstWin:" + firstWinHwnd + ", ParentWin:" + Win32Api.GetParent(firstWinHwnd) + ", Z序列最前:" + Win32Api.GetWindow(appMainWinHwnd, 0) + ", Z序列最后:" + Win32Api.GetWindow(appMainWinHwnd, 1)
            //    + ", 下一个:" + Win32Api.GetWindow(appMainWinHwnd, 2) + ", 同级别最顶层:" + Win32Api.GetWindow(appMainWinHwnd, 3) + ", 父代最顶层:" + Win32Api.GetWindow(appMainWinHwnd, 4) + ", 后代最顶层:" + Win32Api.GetWindow(appMainWinHwnd, 5));

            IntPtr splashWinHwnd = Win32Api.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "TfmSplash", null);
            IntPtr screeningBtnHwnd = Win32Api.FindWindowEx(splashWinHwnd, IntPtr.Zero, null, "Screening");
            IntPtr screeningBtnHwnd1 = Win32Api.FindWindowEx(splashWinHwnd, IntPtr.Zero, null, "Diagnostics");
            Win32Api.SendMessage(screeningBtnHwnd, Win32Api.WM_CLICK, 0, 0);
            Win32Api.SendMessage(screeningBtnHwnd1, Win32Api.WM_CLICK, 0, 0);
            while (!_shouldStop)
            {
                Thread.Sleep(5000);
                //Win32Api.ShowWindow(buttonWinHwnd, 0);
                if (AppProc.HasExited) break;
                IntPtr positionHwnd=Win32Api.WindowFromPoint(0, 50);
                StringBuilder winText=new StringBuilder(512);
                Win32Api.GetWindowText(positionHwnd, winText, winText.Capacity);
                
                IntPtr buttonHwnd = Win32Api.WindowFromPoint(left, 0);
                IntPtr hwnd3 = Win32Api.GetParent(Win32Api.GetParent(buttonHwnd));
                if (appMainWinHwnd == hwnd3)
                {
                    Win32Api.ShowWindow(backWinHwnd, 1);
                }
                else
                {
                    Win32Api.ShowWindow(backWinHwnd, 0);
                }

                //IntPtr currWinHwnd = Win32Api.GetForegroundWindow();
                //if (firstWinHwnd.ToInt32() == 0) firstWinHwnd = currWinHwnd;
                //IntPtr parentWinHwnd = Win32Api.GetParent(Win32Api.GetForegroundWindow());

                //MessageBox.Show("MainWin:" + appMainWinHwnd + ", ParentWin:" + hwnd3 + ", TopWin:" + Win32Api.GetTopWindow(appMainWinHwnd));
                
                //IntPtr hwnd3 = Win32Api.GetParent(Win32Api.GetParent(Win32Api.WindowFromPoint(0, 0)));
                //MessageBox.Show("hwnd3:" + hwnd3);
                //if (currWinHwnd != firstWinHwnd && appMainWinHwnd == parentWinHwnd)
                //{

                //    MessageBox.Show("Show:" + Win32Api.ShowWindow(buttonWinHwnd, 1) + "," + buttonWinHwnd.ToInt32());                
                //}
                //else
                //{
                //    MessageBox.Show("Hide:" + Win32Api.ShowWindow(buttonWinHwnd, 0) + "," + buttonWinHwnd.ToInt32());                                   
                //}

                
            }
            
        }

        public void Stop()
        {
            _shouldStop = true;
        }
        
    }
}
