using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
    public class AddItemCommand : IProjectCommand
    {
        private ProjectItem _item;

        public AddItemCommand(ProjectItem item)
        {
            _item = item;
        }

        public void Execute(Project project)
        {
            project.Items.Add(_item);
        }

        public void Undo(Project project)
        {
            project.Items.Remove(_item);
        }
    }

}
