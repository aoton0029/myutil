using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Workers.WorkerQueue
{
    public abstract class WorkItem
    {
        public Guid Id { get; }
        public Type RequesterType { get; }

        protected WorkItem(object requester)
            : this(Guid.NewGuid(), requester.GetType())
        {
        }

        protected WorkItem(Guid id, Type requesterType)
        {
            Id = id;
            RequesterType = requesterType;
        }

        public abstract Task<WorkResult> DoWorkAsync(CancellationToken token);
        public abstract void Cancel();
        public abstract WorkItem Clone();

    }
}
