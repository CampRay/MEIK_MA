using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MEIKReport.Common
{
    /// <summary>
    /// Abstract base class for Mouse and Keyboard hooks
    /// 鼠标和键盘钩子的抽象类
    /// </summary>
    public abstract class GlobalHook
    {

        #region Windows API Code

        [StructLayout(LayoutKind.Sequential)]
        protected class POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        protected class MouseHookStruct
        {
            public POINT pt;
            public int hwnd;
            public int wHitTestCode;
            public int dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        protected class MouseLLHookStruct
        {
            public POINT pt;
            public int mouseData;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]//声明键盘钩子的封送结构类型
        protected class KeyboardHookStruct
        {
            public int vkCode;//表示一个在1到254间的虚似键盘码
            public int scanCode;//表示硬件扫描码
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        //安装钩子的函数
        [DllImport("user32.dll", CharSet = CharSet.Auto,CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        protected static extern int SetWindowsHookEx(int idHook,HookProc lpfn,IntPtr hMod,int dwThreadId);

        //卸下钩子的函数
        [DllImport("user32.dll", CharSet = CharSet.Auto,CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        protected static extern int UnhookWindowsHookEx(int idHook);

        //下一个钩挂的函数
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        protected static extern int CallNextHookEx(int idHook,int nCode,int wParam,IntPtr lParam);

        [DllImport("user32")]
        protected static extern int ToAscii(int uVirtKey,int uScanCode,byte[] lpbKeyState,byte[] lpwTransKey,int fuState);

        [DllImport("user32")]
        protected static extern int GetKeyboardState(byte[] pbKeyState);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        protected static extern short GetKeyState(int vKey);
        

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);


        //处理钩子的委托函数
        protected delegate int HookProc(int nCode, int wParam, IntPtr lParam);
        protected const int WH_MOUSE_LL = 14;
        protected const int WH_KEYBOARD_LL = 13;

        protected const int WH_MOUSE = 7;
        protected const int WH_KEYBOARD = 2;
        protected const int WM_MOUSEMOVE = 0x200;
        protected const int WM_LBUTTONDOWN = 0x201;
        protected const int WM_RBUTTONDOWN = 0x204;
        protected const int WM_MBUTTONDOWN = 0x207;
        protected const int WM_LBUTTONUP = 0x202;
        protected const int WM_RBUTTONUP = 0x205;
        protected const int WM_MBUTTONUP = 0x208;
        protected const int WM_LBUTTONDBLCLK = 0x203;
        protected const int WM_RBUTTONDBLCLK = 0x206;
        protected const int WM_MBUTTONDBLCLK = 0x209;
        protected const int WM_MOUSEWHEEL = 0x020A;
        protected const int WM_KEYDOWN = 0x100;
        protected const int WM_KEYUP = 0x101;
        protected const int WM_SYSKEYDOWN = 0x104;
        protected const int WM_SYSKEYUP = 0x105;

        protected const byte VK_SHIFT = 0x10;
        protected const byte VK_CAPITAL = 0x14;
        protected const byte VK_NUMLOCK = 0x90;

        protected const byte VK_LSHIFT = 0xA0;
        protected const byte VK_RSHIFT = 0xA1;
        protected const byte VK_LCONTROL = 0xA2;
        protected const byte VK_RCONTROL = 0x3;
        protected const byte VK_LALT = 0xA4;
        protected const byte VK_RALT = 0xA5;

        protected const byte LLKHF_ALTDOWN = 0x20;

        #endregion

        #region Private Variables

        protected int _hookType;
        protected int _handleToHook;
        protected bool _isStarted;
        protected HookProc _hookCallback;

        #endregion

        #region Properties

        public bool IsStarted
        {
            get
            {
                return _isStarted;
            }
        }

        #endregion

        #region Constructor

        public GlobalHook()
        {

            Application.ApplicationExit += new EventHandler(Application_ApplicationExit);

        }

        #endregion

        #region Methods

        public void Start()
        {

            if (!_isStarted &&_hookType != 0)
            {

                // Make sure we keep a reference to this delegate!
                // If not, GC randomly collects it, and a NullReference exception is thrown
                _hookCallback = new HookProc(HookCallbackProcedure);
               
                //_handleToHook = SetWindowsHookEx(_hookType,_hookCallback,
                //    Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]),
                //    0);
                
                ProcessModule cModule = Process.GetCurrentProcess().MainModule;
                var mh = GetModuleHandle(cModule.ModuleName);
                _handleToHook = SetWindowsHookEx(_hookType, _hookCallback,mh,0);

                // Were we able to sucessfully start hook?
                if (_handleToHook != 0)
                {
                    _isStarted = true;
                }

            }

        }

        public void Stop()
        {

            if (_isStarted)
            {

                UnhookWindowsHookEx(_handleToHook);

                _isStarted = false;

            }

        }

        protected virtual int HookCallbackProcedure(int nCode, Int32 wParam, IntPtr lParam)
        {

            // This method must be overriden by each extending hook
            return 0;

        }

        protected void Application_ApplicationExit(object sender, EventArgs e)
        {

            if (_isStarted)
            {
                Stop();
            }

        }

        #endregion

    }
}
