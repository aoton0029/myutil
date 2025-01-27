using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Models
{
    public class TreeNode<T>
    {
        public T Value { get; set; }
        public TreeNode<T> Parent { get; private set; }
        public List<TreeNode<T>> Children { get; set; }

        public TreeNode(T value)
        {
            Value = value;
            Children = new List<TreeNode<T>>();
            Parent = null;
        }

        public void AddChild(TreeNode<T> child)
        {
            child.Parent = this;
            Children.Add(child);
        }

        public void RemoveChild(TreeNode<T> child)
        {
            if (Children.Remove(child))
            {
                child.Parent = null; // 親ノードとの関係を切る
            }
        }

        public int GetDepth()
        {
            int depth = 0;
            TreeNode<T> current = this;
            while (current.Parent != null)
            {
                depth++;
                current = current.Parent;
            }
            return depth;
        }

        public override string ToString()
        {
            return Value?.ToString() ?? "null";
        }
    }

    public class Tree<T>
    {
        public TreeNode<T> Root { get; private set; }

        public Tree(T rootValue)
        {
            Root = new TreeNode<T>(rootValue);
        }

        // ツリーを深さ優先でトラバース
        public void TraverseDFS(TreeNode<T> node, Action<TreeNode<T>> action)
        {
            if (node == null) return;

            action(node);

            foreach (var child in node.Children)
            {
                TraverseDFS(child, action);
            }
        }

        // ツリーを幅優先でトラバース
        public void TraverseBFS(TreeNode<T> node, Action<TreeNode<T>> action)
        {
            if (node == null) return;

            Queue<TreeNode<T>> queue = new Queue<TreeNode<T>>();
            queue.Enqueue(node);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                action(current);

                foreach (var child in current.Children)
                {
                    queue.Enqueue(child);
                }
            }
        }

        // 特定の条件でノードを検索 (深さ優先)
        public TreeNode<T> Find(TreeNode<T> node, Predicate<T> match)
        {
            if (node == null) return null;

            if (match(node.Value)) return node;

            foreach (var child in node.Children)
            {
                var result = Find(child, match);
                if (result != null) return result;
            }

            return null;
        }
    }
}
