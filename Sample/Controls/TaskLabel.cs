using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityLib.Tasks;

namespace Sample.Controls
{
    public sealed class TaskLabel : Label, IProgress<TaskSnapshot>
    {
        public TaskLabel()
        {
            CreateHandle();
        }

        /// <inheritdoc/>
        public void Report(TaskSnapshot value)
        {
            try
            {
                // Ensure execution on GUI thread
                if (InvokeRequired)
                {
                    BeginInvoke(Report, value);
                    return;
                }

                Text = value.ToString();

                ForeColor = value.State switch
                {
                    TaskState.Complete => Color.Green,
                    TaskState.IOError => Color.Red,
                    _ => SystemColors.ControlText
                };
            }
            catch (Exception ex) when (ex is InvalidOperationException or Win32Exception)
            {

            }
        }
    }

}
