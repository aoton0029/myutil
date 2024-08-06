using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.DesignPatterns.Observer
{
    public interface IObserver
    {
        void Update(string? message);
    }

    internal class Observer
    {
    }
}
