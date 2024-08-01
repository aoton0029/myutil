using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace UtilityLib.Tasks
{
    public enum Verbosity
    {
        /// <summary>Automatically answer questions with defaults when possible. Avoid non-essential output and questions.</summary>
        Batch = -1,

        /// <summary>Normal interactive operation.</summary>
        Normal = 0,

        /// <summary>Display additional information for troubleshooting.</summary>
        Verbose = 1,

        /// <summary>Display detailed information for debugging.</summary>
        Debug = 2
    }

    public interface ITaskHandler : IDisposable
    {
        CancellationToken CancellationToken { get; }

        void RunTask(ITask task);

        Verbosity Verbosity { get; set; }

        bool Ask([Localizable(true)] string question, bool? defaultAnswer = null, [Localizable(true)] string? alternateMessage = null);

        void Output([Localizable(true)] string title, [Localizable(true)] string message);

        void Output<T>([Localizable(true)] string title, IEnumerable<T> data);

        void Error(Exception exception);
    }
}
