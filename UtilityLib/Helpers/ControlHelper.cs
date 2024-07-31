using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UtilityLib
{
    /// <summary>
    /// Class ControlHelper.
    /// </summary>
    public static class ControlHelper
    {
        /// <summary>
        /// 功能描述:设置控件Enabled，切不改变控件颜色
        /// 作　　者:HZH
        /// 创建日期:2019-03-04 13:43:32
        /// 任务编号:POS
        /// </summary>
        /// <param name="c">c</param>
        /// <param name="enabled">enabled</param>
        public static void SetControlEnabled(this Control c, bool enabled)
        {
            if (!c.IsDisposed)
            {
                if (enabled)
                {
                    //ControlHelper.SetWindowLong(c.Handle, -16, -134217729 & ControlHelper.GetWindowLong(c.Handle, -16));
                }
                else
                {
                    //ControlHelper.SetWindowLong(c.Handle, -16, 134217728 + ControlHelper.GetWindowLong(c.Handle, -16));
                }
            }
        }

        /// <summary>
        /// 功能描述:设置控件Enabled，切不改变控件颜色
        /// 作　　者:HZH
        /// 创建日期:2019-03-04 13:43:32
        /// 任务编号:POS
        /// </summary>
        /// <param name="cs">cs</param>
        /// <param name="enabled">enabled</param>
        public static void SetControlEnableds(Control[] cs, bool enabled)
        {
            for (int i = 0; i < cs.Length; i++)
            {
                Control c = cs[i];
                SetControlEnabled(c, enabled);
            }
        }

        /// <summary>
        /// Threads the base call back.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="obj">The object.</param>
        private static void ThreadBaseCallBack(Control parent, object obj)
        {
            if (obj is Exception)
            {
                if (parent != null)
                {
                    ThreadInvokerControl(parent, delegate
                    {
                        Exception ex = obj as Exception;
                    });
                }
            }
        }
        /// <summary>
        /// 委托调用主线程控件
        /// </summary>
        /// <param name="parent">主线程控件</param>
        /// <param name="action">修改控件方法</param>
        public static void ThreadInvokerControl(Control parent, Action action)
        {
            if (parent != null)
            {
                if (parent.InvokeRequired)
                {
                    parent.BeginInvoke(action);
                }
                else
                {
                    action();
                    SetForegroundWindow(parent.Handle);
                }
            }
        }
        /// <summary>
        /// 使用一个线程执行一个操作
        /// </summary>
        /// <param name="parent">父控件</param>
        /// <param name="func">执行内容</param>
        /// <param name="callback">执行后回调</param>
        /// <param name="enableControl">执行期间禁用控件列表</param>
        /// <param name="blnShowSplashScreen">执行期间是否显示等待提示</param>
        /// <param name="strMsg">执行期间等待提示内容，默认为“正在处理，请稍候...”</param>
        /// <param name="intSplashScreenDelayTime">延迟显示等待提示时间</param>
        public static void ThreadRunExt(Control parent, Action func,Action<object> callback,
          Control enableControl = null,
          bool blnShowSplashScreen = true,
          string strMsg = null,
          int intSplashScreenDelayTime = 200)
        {
            ThreadRunExt(parent, func, callback, new Control[] { enableControl }, blnShowSplashScreen, strMsg, intSplashScreenDelayTime);
        }
        /// <summary>
        /// 使用一个线程执行一个操作
        /// </summary>
        /// <param name="parent">父控件</param>
        /// <param name="func">执行内容</param>
        /// <param name="callback">执行后回调</param>
        /// <param name="enableControl">执行期间禁用控件列表</param>
        /// <param name="blnShowSplashScreen">执行期间是否显示等待提示</param>
        /// <param name="strMsg">执行期间等待提示内容，默认为“正在处理，请稍候...”</param>
        /// <param name="intSplashScreenDelayTime">延迟显示等待提示时间</param>
        public static void ThreadRunExt(
            Control parent,
            Action func,
            Action<object> callback,
            Control[] enableControl = null,
            bool blnShowSplashScreen = true,
            string strMsg = null,
            int intSplashScreenDelayTime = 200)
        {
            if (blnShowSplashScreen)
            {
                if (string.IsNullOrEmpty(strMsg))
                {
                    strMsg = "正在处理，请稍候...";
                }
                if (parent != null)
                {
                    ShowProcessPanel(parent, strMsg, intSplashScreenDelayTime);
                }
            }
            if (enableControl != null)
            {
                List<Control> lstCs = new List<Control>();
                foreach (var c in enableControl)
                {
                    if (c == null)
                        continue;
                    if (c is Form)
                    {
                        lstCs.AddRange(c.Controls.ToArray());
                    }
                    else
                    {
                        lstCs.Add(c);
                    }
                }
                SetControlEnableds(lstCs.ToArray(), false);
            }
            ThreadPool.QueueUserWorkItem(delegate (object a)
            {
                try
                {
                    func();
                    if (callback != null)
                    {
                        callback(null);
                    }
                }
                catch (Exception obj)
                {
                    if (callback != null)
                    {
                        callback(obj);
                    }
                    else
                    {
                        ThreadBaseCallBack(parent, obj);
                    }
                }
                finally
                {
                    if (parent != null)
                    {
                        ThreadInvokerControl(parent, delegate
                        {
                            CloseProcessPanel(parent);
                            SetForegroundWindow(parent.Handle);
                        });
                    }
                    if (enableControl != null)
                    {
                        if (parent != null)
                        {
                            ThreadInvokerControl(parent, delegate
                            {
                                List<Control> lstCs = new List<Control>();
                                foreach (var c in enableControl)
                                {
                                    if (c == null)
                                        continue;
                                    if (c is Form)
                                    {
                                        lstCs.AddRange(c.Controls.ToArray());
                                    }
                                    else
                                    {
                                        lstCs.Add(c);
                                    }
                                }
                                SetControlEnableds(lstCs.ToArray(), true);
                            });
                        }
                    }
                }
            });
        }


        /// <summary>
        /// Sets the foreground window.
        /// </summary>
        /// <param name="hWnd">The h WND.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        /// Shows the process panel.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="strMessage">The string message.</param>
        /// <param name="intSplashScreenDelayTime">The int splash screen delay time.</param>
        public static void ShowProcessPanel(Control parent, string strMessage, int intSplashScreenDelayTime = 0)
        {
            if (parent.InvokeRequired)
            {
                parent.BeginInvoke(new System.Windows.Forms.MethodInvoker(delegate
                {
                    ShowProcessPanel(parent, strMessage, intSplashScreenDelayTime);
                }));
            }
            else
            {
                parent.VisibleChanged -= new EventHandler(parent_VisibleChanged);
                parent.VisibleChanged += new EventHandler(parent_VisibleChanged);
                parent.FindForm().FormClosing -= ControlHelper_FormClosing;
                parent.FindForm().FormClosing += ControlHelper_FormClosing;
                Control control = null;
                lock (parent)
                {
                    control = HaveProcessPanelControl(parent);
                    if (control == null)
                    {
                        control = CreateProgressPanel();
                        parent.Controls.Add(control);
                    }
                }
                Forms.FormWaiting frmWaitingEx = control.Tag as Forms.FormWaiting;
                frmWaitingEx.Msg = strMessage;
                frmWaitingEx.ShowForm(intSplashScreenDelayTime);
            }
        }

        /// <summary>
        /// Handles the FormClosing event of the ControlHelper control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FormClosingEventArgs" /> instance containing the event data.</param>
        static void ControlHelper_FormClosing(object sender, FormClosingEventArgs e)
        {
            Control control = sender as Control;
            control.FindForm().FormClosing -= ControlHelper_FormClosing;
            CloseWaiting(control);
        }

        /// <summary>
        /// Handles the VisibleChanged event of the parent control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void parent_VisibleChanged(object sender, EventArgs e)
        {
            Control control = sender as Control;
            control.VisibleChanged -= new EventHandler(parent_VisibleChanged);
            if (!control.Visible)
            {
                CloseWaiting(control);
            }
        }

        /// <summary>
        /// Closes the waiting.
        /// </summary>
        /// <param name="control">The control.</param>
        private static void CloseWaiting(Control control)
        {
            Control[] array = control.Controls.Find("myprogressPanelext", false);
            if (array.Length > 0)
            {
                Control control2 = array[0];
                if (control2.Tag != null && control2.Tag is Forms.FormWaiting)
                {
                    Forms.FormWaiting frmWaitingEx = control2.Tag as Forms.FormWaiting;
                    if (frmWaitingEx != null && !frmWaitingEx.IsDisposed && frmWaitingEx.Visible)
                    {
                        frmWaitingEx.Hide();
                    }
                }
            }
        }

        /// <summary>
        /// Closes the process panel.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public static void CloseProcessPanel(Control parent)
        {
            if (parent.InvokeRequired)
            {
                parent.BeginInvoke(new System.Windows.Forms.MethodInvoker(delegate
                {
                    CloseProcessPanel(parent);
                }));
            }
            else if (parent != null)
            {
                Control control = HaveProcessPanelControl(parent);
                if (control != null)
                {
                    Form frm = control.Tag as Form;
                    if (frm != null && !frm.IsDisposed && frm.Visible)
                    {
                        if (frm.InvokeRequired)
                        {
                            frm.BeginInvoke(new System.Windows.Forms.MethodInvoker(delegate
                            {
                                frm.Hide();
                            }));
                        }
                        else
                        {
                            frm.Hide();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Haves the process panel control.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <returns>Control.</returns>
        public static Control HaveProcessPanelControl(Control parent)
        {
            Control[] array = parent.Controls.Find("myprogressPanelext", false);
            Control result;
            if (array.Length > 0)
            {
                result = array[0];
            }
            else
            {
                result = null;
            }
            return result;
        }

        /// <summary>
        /// Creates the progress panel.
        /// </summary>
        /// <returns>Control.</returns>
        public static Control CreateProgressPanel()
        {
            return new Label
            {
                Name = "myprogressPanelext",
                Visible = false,
                Tag = new Forms.FormWaiting
                {
                    TopMost = true,
                    Opacity = 0.0
                }
            };
        }

        /// <summary>
        /// Converts to array.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns>Control[].</returns>
        public static Control[] ToArray(this System.Windows.Forms.Control.ControlCollection controls)
        {
            if (controls == null || controls.Count <= 0)
                return new Control[0];
            List<Control> lst = new List<Control>();
            foreach (Control item in controls)
            {
                lst.Add(item);
            }
            return lst.ToArray();
        }


        #region 根据控件宽度截取字符串
        /// <summary>
        /// 功能描述:根据控件宽度截取字符串
        /// 作　　者:HZH
        /// 创建日期:2019-06-27 10:49:10
        /// 任务编号:POS
        /// </summary>
        /// <param name="strSource">字符串</param>
        /// <param name="fltControlWidth">控件宽度</param>
        /// <param name="g">Graphics</param>
        /// <param name="font">字体</param>
        /// <returns>截取后的字符串</returns>
        public static string GetSubString(
            string strSource,
            float fltControlWidth,
            System.Drawing.Graphics g,
            System.Drawing.Font font)
        {
            try
            {
                fltControlWidth = fltControlWidth - 20;
                strSource = strSource.Trim();
                while (true)
                {

                    System.Drawing.SizeF sizeF = g.MeasureString(strSource.Replace(" ", "A"), font);
                    if (sizeF.Width > fltControlWidth)
                    {
                        strSource = strSource.TrimEnd('…');
                        if (strSource.Length <= 1)
                            return "";
                        strSource = strSource.Substring(0, strSource.Length - 1).Trim() + "…";
                    }
                    else
                    {
                        return strSource;
                    }
                }
            }
            finally
            {
                g.Dispose();
            }
        }
        #endregion

        #region 获取字符串宽度
        /// <summary>
        /// 功能描述:获取字符串宽度
        /// 作　　者:HZH
        /// 创建日期:2019-06-27 11:54:50
        /// 任务编号:POS
        /// </summary>
        /// <param name="strSource">strSource</param>
        /// <param name="g">g</param>
        /// <param name="font">font</param>
        /// <returns>返回值</returns>
        public static int GetStringWidth(
           string strSource,
           System.Drawing.Graphics g,
           System.Drawing.Font font)
        {
            string[] strs = strSource.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            float fltWidth = 0;
            foreach (var item in strs)
            {
                System.Drawing.SizeF sizeF = g.MeasureString(strSource.Replace(" ", "A"), font);
                if (sizeF.Width > fltWidth)
                    fltWidth = sizeF.Width;
            }

            return (int)fltWidth;
        }
        #endregion


        public static Point GetScreenLocation(Screen screen, int left, int top)
        {
            return new Point(screen.Bounds.Left + left, screen.Bounds.Top + top);
        }
    }
}
