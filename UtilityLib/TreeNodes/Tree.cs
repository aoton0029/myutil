using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.TreeNodes
{
    public class TreeNode<T> where T : TreeNode<T>
    {
        List<T> _chilren;

        public T Root { get; set; }

        public T Parent { get; set; }
        public T Child { get; set; }

        public T Next { get; set; }
        public T Previous { get; set; }

        public T Top { get; set; }
        public T Bottom { get; set; }

        public TreeNode() { }

        public T AddParent(T parent)
        {

        }

        public T AddChild(T child)
        {

        }

        public T RemoveChild(T child)
        {

        }

        public bool setParent(T parent)
        {
            if (this.Parent == parent) return false;
            this.Parent = parent;
            return true;
        }

        private T insertChild(T child)
        {
            var p = child?.Parent;
            child?.SetParent(Self);
            p?.RemoveChildProcess(child);
            action(ChildNodes, index, child);
        }
    }
}
