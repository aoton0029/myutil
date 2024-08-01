using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UtilityLib.Tasks;
using static UtilityLib.Tasks.TaskState;
using static UtilityLib.Native.WindowsTaskbar.ProgressBarState;


namespace Sample.Controls
{
    public sealed class TaskProgressBar : ProgressBar, IProgress<TaskSnapshot>
    {
        public TaskProgressBar()
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

                Value = value.Value switch
                {
                    _ when value.State == TaskState.Complete => 100,
                    <= 0 => 0,
                    >= 1 => 100,
                    _ => (int)(value.Value * 100)
                };

                var state = value.State switch
                {
                    TaskState.Started or TaskState.Header => Indeterminate,
                    TaskState.Data when value.UnitsTotal == -1 => Indeterminate,
                    TaskState.Data => Normal,
                    TaskState.Complete => NoProgress,
                    TaskState.IOError => Error,
                    _ => NoProgress,
                };
                UpdateTaskbar(state);
                Style = state == Indeterminate
                    ? ProgressBarStyle.Marquee
                    : ProgressBarStyle.Continuous;
            }
            catch (Exception ex) when (ex is InvalidOperationException or Win32Exception)
            {

            }
        }

    }
}
