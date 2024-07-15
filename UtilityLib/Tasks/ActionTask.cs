using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Tasks
{
    public sealed class ActionTask([Localizable(true)] string name, Action work, Action? cancellationCallback = null) : TaskBase
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
            if (cancellationCallback == null) work();
            else
            {
                using (CancellationToken.Register(cancellationCallback))
                    work();
            }
        }
    }
}
