using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.TreeNodes
{
    internal class Treenode_4
    {

        public interface ITreeNode<T>
        {
            T Value { get; set; }
            ITreeNode<T> Parent { get; set; }
            IList<ITreeNode<T>> Children { get; }

            void AddChild(ITreeNode<T> child);
            void RemoveChild(ITreeNode<T> child);
            void Traverse(Action<ITreeNode<T>> visit);
        }


        public class TreeNode<T> : ITreeNode<T>
        {
            public T Value { get; set; }
            public ITreeNode<T> Parent { get; set; }
            public IList<ITreeNode<T>> Children { get; private set; }

            public TreeNode(T value)
            {
                Value = value;
                Children = new List<ITreeNode<T>>();
            }

            public void AddChild(ITreeNode<T> child)
            {
                child.Parent = this;
                Children.Add(child);
            }

            public void RemoveChild(ITreeNode<T> child)
            {
                child.Parent = null;
                Children.Remove(child);
            }

            public void Traverse(Action<ITreeNode<T>> visit)
            {
                visit(this);
                foreach (var child in Children)
                {
                    child.Traverse(visit);
                }
            }
        }
    }
    
}
