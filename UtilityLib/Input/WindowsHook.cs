using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Input
{
    public class WindowsHook
    {
        public enum HookType : int
        {
            WH_MSGFILTER = -1,
            WH_JOURNALRECORD = 0,
            WH_JOURNALPLAYBACK = 1,
            WH_KEYBOARD = 2,
            WH_GETMESSAGE = 3,
            WH_CALLWNDPROC = 4,
            WH_CBT = 5,
            WH_SYSMSGFILTER = 6,
            WH_MOUSE = 7,
            WH_DEBUG = 9,
            WH_SHELL = 10,
            WH_FOREGROUNDIDLE = 11,
            WH_CALLWNDPROCRET = 12,
            WH_KEYBOARD_LL = 13,
            WH_MOUSE_LL = 14,
        }

        public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, int hInstance, int threadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        /// <summary>
        /// Delegate HookMsgHandler
        /// </summary>
        /// <param name="strHookName">钩子名称</param>
        /// <param name="msg">消息值</param>
        public delegate void HookMsgHandler(string strHookName, int nCode, IntPtr msg, IntPtr lParam);

        /// <summary>
        /// 钩子消息事件
        /// </summary>
        public static event HookMsgHandler HookMsgChanged;

        /// <summary>
        /// 启动一个钩子
        /// </summary>
        /// <param name="hookType">钩子类型</param>
        /// <param name="wParam">模块句柄，为空则为当前模块</param>
        /// <param name="pid">进程句柄，默认为0则表示当前进程</param>
        /// <param name="strHookName">钩子名称</param>
        /// <returns>钩子句柄（消耗钩子时需要使用）</returns>
        /// <exception cref="Exception">SetWindowsHookEx failed.</exception>
        public static int StartHook(HookType hookType, int wParam = 0, int pid = 0, string strHookName = "")
        {
            int _hHook = 0;
            // 生成一个HookProc的实例.
            var _hookProcedure = new HookProc((nCode, msg, lParam) =>
            {

                if (HookMsgChanged != null)
                {
                    try
                    {
                        HookMsgChanged(strHookName, nCode, msg, lParam);
                    }
                    catch { }
                }

                int inext = CallNextHookEx(_hHook, nCode, msg, lParam);
                return inext;
            });
            if (pid == 0)
                pid = AppDomain.GetCurrentThreadId();
            _hHook = SetWindowsHookEx((int)hookType, _hookProcedure, wParam, pid);

            //假设装置失败停止钩子
            if (_hHook == 0)
            {
                StopHook(_hHook);
            }
            return _hHook;
        }

        /// <summary>
        /// 停止钩子
        /// </summary>
        /// <param name="_hHook">StartHook函数返回的钩子句柄</param>
        /// <returns><c>true</c> if 停止成功, <c>false</c> 否则.</returns>
        public static bool StopHook(int _hHook)
        {
            bool ret = true;

            if (_hHook != 0)
            {
                ret = UnhookWindowsHookEx(_hHook);
            }

            // 假设卸下钩子失败
            if (!ret)
                return false;
            return true;
        }
    }
}
