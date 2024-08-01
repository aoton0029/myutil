using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Tasks
{
    public sealed class WaitTask([Localizable(true)] string name, WaitHandle? waitHandle = null, int millisecondsTimeout = Timeout.Infinite) : TaskBase
    {
        /// <summary>The <see cref="WaitHandle"/> to wait for; <c>null</c> to wait for <see cref="CancellationToken"/>.</summary>
        private readonly WaitHandle? _waitHandle = waitHandle;

        /// <summary>The number of milliseconds to wait before raising <see cref="TimeoutException"/>; <see cref="Timeout.Infinite"/> to wait indefinitely</summary>
        private readonly int _millisecondsTimeout = millisecondsTimeout;

        /// <inheritdoc/>
        public override string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));

        /// <inheritdoc/>
        protected override bool PreventIdle => false;

        /// <inheritdoc/>
        protected override bool UnitsByte => false;

        /// <inheritdoc/>
        protected override void Execute()
        {
            if (_waitHandle == null) CancellationToken.WaitHandle.WaitOne(_millisecondsTimeout);
            else _waitHandle.WaitOne(CancellationToken, _millisecondsTimeout);
        }
    }

    public static class WaitHandleExtensions
    {
        public static void WaitOne(this WaitHandle handle, CancellationToken cancellationToken, int millisecondsTimeout = -1)
        {
            try
            {
                switch (WaitHandle.WaitAny([
                            handle ?? throw new ArgumentNullException(nameof(handle)),
                        cancellationToken.WaitHandle
                        ], millisecondsTimeout, exitContext: false))
                {
                    case 0:
                        return;
                    case 1:
                        throw new OperationCanceledException();
                    default:
                    case WaitHandle.WaitTimeout:
                        throw new TimeoutException();
                }
            }
            catch (AbandonedMutexException)
            {

            }
        }
    }

}
