using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectSample.Core
{
    public interface IProjectPersistence
    {
        Project Load(string filePath);
        void Save(Project project, string filePath);
    }

}
