using ProjectSample.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectSample.Services
{
    public class ProjectContext
    {
        public Project Current { get; private set; }
        public string CurrentFilePath { get; private set; }
        private string _lastSavedJson;

        public bool HasChanges => Current.ToJson() != _lastSavedJson;

        public void Load(Project project, string path)
        {
            Current = project;
            CurrentFilePath = path;
            _lastSavedJson = project.ToJson();
        }

        public void MarkSaved()
        {
            _lastSavedJson = Current.ToJson();
        }
    }

}
