using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Tasks
{
    public delegate void PercentProgressCallback(int percent);

    public sealed class PercentageTask([Localizable(true)] string name, Action<PercentProgressCallback> work, Action? cancellationCallback = null) : TaskBase
    {
        /// <inheritdoc/>
        public override string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));

        /// <inheritdoc/>
        public override bool CanCancel => cancellationCallback != null;

        /// <inheritdoc/>
        protected override bool UnitsByte => false;

        /// <inheritdoc/>
        protected override void Execute()
        {
            CancellationTokenRegistration? RegisterCancellationCallBack()
                => cancellationCallback?.To(CancellationToken.Register);

            UnitsTotal = 100;
            State = TaskState.Data;

            using (RegisterCancellationCallBack())
                work(percent => UnitsProcessed = percent);
        }
    }
}
