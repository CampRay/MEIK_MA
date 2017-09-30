using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MEIKReport.Common
{
    public class Win32Api
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
        private static Hashtable processWnd = null;
        //定义一个委托，去回调本地的一个函数
        public delegate bool WNDENUMPROC(IntPtr hwnd, uint IParam);

        //定义一个委托，去回调本地的一个窗口消息处理函数
        public delegate int DEFWNDPROC(IntPtr hwnd, int uMsg, uint wParam, uint lParam);

        static Win32Api()
        {
            if (processWnd == null)
           {
                processWnd = new Hashtable();
            }
        }

        public static Int32 WM_CLICK = 0xF5;
        
        //枚举所有屏幕上的顶层窗体,
        [DllImport("user32.dll", EntryPoint = "EnumWindows",SetLastError=true)]
        public static extern bool EnumWindows(WNDENUMPROC lpEnumFunc,uint LParam);

        //枚举指定窗体的所有子窗体,
        [DllImport("user32.dll", EntryPoint = "EnumChildWindows", SetLastError = true)]
        public static extern bool EnumChildWindows(IntPtr hWnd, WNDENUMPROC lpEnumFunc, uint LParam);

        //获取指定窗体所在进程的ID
        [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId")]
        public static extern bool GetWindowThreadProcessId(IntPtr hWnd, ref uint LpdwProcessId);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);
        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT rect);

        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        //该函数获得一个窗口的句柄，该窗口的类名和窗口名与给定的字符串相匹配。这个函数查找子窗口，从排在给定的子窗口后面的下一个子窗口开始。在查找时不区分大小写
        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        //获取指定坐标处窗体句柄
        [DllImport("user32.dll", EntryPoint = "WindowFromPoint")]
        public static extern IntPtr WindowFromPoint(int xPoint, int yPoint);
        //获取指定子窗体的父窗体的句柄
        [DllImport("user32.dll", EntryPoint = "GetParent", SetLastError = true)]
        public static extern IntPtr GetParent(IntPtr hWnd);
        //获取前台窗体（当前工作的窗体）
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
        //获取指定父窗体下显示在最前的子窗体
        [DllImport("user32.dll")]
        public static extern IntPtr GetTopWindow(IntPtr parentHwnd);
        //返回与指定窗口有特定关系的窗口句柄,uCmd参数是窗口关系，0表示Z序列最前,1表示Z序列最后,2表示下一个,3表示相同级别最顶层，4表示父级最顶层，5表示子级最顶层
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        //测试是否父窗体
        [DllImport("user32.dll")]
        public static extern bool IsWindow(IntPtr hWnd);

        //测试一个窗体是否指定父窗体的的子窗体或后代窗体
        [DllImport("user32.dll")]
        public static extern bool IsChild(IntPtr parentHwnd,IntPtr hWnd);
        //测试一个窗体是否最大化窗体
        [DllImport("user32.dll")]
        public static extern bool IsZoomed(IntPtr hWnd);

        //设置一个窗体的显示状态,0隐藏，1显示
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd,int nCmdShow);
        //设置一个窗体的显示状态
        [DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
        //设置一个窗体的显示状态
        [DllImport("user32.dll")]
        public static extern bool ShowOwnedPopups(IntPtr hWnd, bool fshow);

        //设置一个窗体显示到最前面
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        //该函数获得有关指定窗口的信息，函数也获得在额外窗口内存中指定偏移位地址的32 位度整型值
        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nlndex);
  
        //该函数改变指定窗口的属性。函数也将在指定偏移地址的一个32 位值存入窗口的额外窗口存
        [DllImport("user32.dll",EntryPoint="SetWindowLong")]
        public static extern int SetWindowLong(IntPtr hWnd,int nlndex,int dwNewLong);

        [DllImport("user32.dll")]
        public static extern bool MoveWindow(IntPtr hWnd,int x,int y,int nWidth,int nHeight,bool BRePaint);

        //获取窗口标题
        [DllImport("user32", SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        //设置指定窗体的父窗体
        [DllImport("user32.dll", SetLastError = true)]
        public static extern long SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        //获取类的名字
        [DllImport("user32.dll")]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        //向指定窗体句柄发送消息
        [DllImport("user32.dll", EntryPoint = "SendMessageA")]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
                
        //该函数CallWindowProc 将消息信息传送给指定的窗口过程
        [DllImport("user32.dll", EntryPoint = "CallWindowProc", SetLastError = true)]
        public static extern int CallWindowProc(DEFWNDPROC lpPrevWndFunc,IntPtr hWnd,int Msg, uint wParam,uint IParam);
        //该函数CallWindowProc 将消息信息传送给指定的窗口过程
        [DllImport("user32.dll", EntryPoint = "DefWindowProc", SetLastError = true)]
        public static extern int DefWindowProc(IntPtr hWnd, int Msg, uint wParam, uint IParam);


        //调用GetCurrentWindowHandle()即可返回当前进程的主窗口句柄，如果获取失败则返回IntPtr.Zero
        public static IntPtr GetCurrentWindowHandle()
        {
            IntPtr ptrWnd = IntPtr.Zero;
            uint uiPid = (uint)Process.GetCurrentProcess().Id;  // 当前进程 ID
            object objWnd = processWnd[uiPid];

            if (objWnd != null)
            {
                ptrWnd = (IntPtr)objWnd;
                if (ptrWnd != IntPtr.Zero && IsWindow(ptrWnd))  // 从缓存中获取句柄
                {
                    return ptrWnd;
                }
                else
                {
                    ptrWnd = IntPtr.Zero;
                }
            }
            WNDENUMPROC callback=new WNDENUMPROC(EnumWindowsProc);
            bool bResult = EnumWindows(callback, uiPid);
            GC.KeepAlive(callback);
            // 枚举窗口返回 false 并且没有错误号时表明获取成功
            if (!bResult && Marshal.GetLastWin32Error() == 0)
            {
                objWnd = processWnd[uiPid];
                if (objWnd != null)
                {
                    ptrWnd = (IntPtr)objWnd;
                }
            }

            return ptrWnd;
        }

        //枚举所有窗体的回调函数,hwnd是枚举出来的顶层窗口句柄，LParam是之前调用EnumWindows函数时传入的参数
        private static bool EnumWindowsProc(IntPtr hwnd, uint lParam)
        {
            uint uiPid = 0;

            if (GetParent(hwnd) == IntPtr.Zero)
            {
                GetWindowThreadProcessId(hwnd, ref uiPid);
                if (uiPid == lParam)    // 找到进程对应的主窗口句柄
                {
                    processWnd[uiPid] = hwnd;   // 把句柄缓存起来
                    SetLastError(0);    // 设置无错误
                    return false;   // 返回 false 以终止枚举窗口
                }
            }

            return true;
        }

        //传递消息到指定窗口
        public static void CallWindowProcHandle(IntPtr hwnd)
        {
            DEFWNDPROC callback = new DEFWNDPROC(MainWindowsProc);
            //int result=CallWindowProc(callback,hwnd,Msg, wParam,IParam);            
            //GC.KeepAlive(callback);            
            //if (result==0 && Marshal.GetLastWin32Error() == 0)
            //{
                
            //}
            
        }

        //该函数是一个本地程序定义的函数，它处理发送给窗口的消息
        private static int MainWindowsProc(IntPtr hwnd,int uMsg, uint wParam,uint lParam)
        {            
            switch (uMsg)
            {
                case 0x00A1:                    
                    return 0;                
                default:
                    return DefWindowProc(hwnd, uMsg, wParam, lParam);
            }            
        }


        [DllImport("gdi32.dll")]
        public static extern UInt64 BitBlt(IntPtr hDestDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, System.Int32 dwRop);

        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        public static extern int SetLastError(uint dwErrCode);

    }
}
