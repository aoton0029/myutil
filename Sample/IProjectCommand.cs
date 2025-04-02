using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
    public interface IProjectCommand
    {
        void Execute(Project project);
        void Undo(Project project);
    }
}
