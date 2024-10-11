using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace UtilityLib.Tasks
{
    public abstract class TaskHandlerBase : ITaskHandler
    {
        protected TaskHandlerBase()
        {

        }

        public virtual void Dispose()
        {
            CancellationTokenSource.Dispose();
        }

        protected CancellationTokenSource CancellationTokenSource { get; init; } = new();

        public CancellationToken CancellationToken => CancellationTokenSource.Token;

        protected virtual ICredentialProvider? CredentialProvider => null;

        public Verbosity Verbosity { get; set; }

        protected virtual bool IsInteractive => Verbosity != Verbosity.Batch;

        public virtual void RunTask(ITask task)
        {
            task.Run(CancellationToken, CredentialProvider);
        }

        private readonly object _askInteractiveLock = new();

        public bool Ask(string question, bool? defaultAnswer = null, string? alternateMessage = null)
        {
            if (IsInteractive)
            {
                lock (_askInteractiveLock)
                {
                    try
                    {
                        bool answer = AskInteractive(question, defaultAnswer ?? false);
                        return answer;
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                }
            }
            else if (defaultAnswer is { } answer)
            {
                return answer;
            }
            else throw new OperationCanceledException($"Unable to ask question in non-interactive mode: {question}");
        }

        protected abstract bool AskInteractive(string question, bool defaultAnswer);

        public abstract void Output(string title, string message);

        public virtual void Output<T>(string title, IEnumerable<T> data)
        {
            Output(title, string.Join(Environment.NewLine, data.Select(x => x?.ToString() ?? "")));
        }

        public abstract void Error(Exception exception);
    }
}
