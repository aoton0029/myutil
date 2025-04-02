using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Models
{
    public interface IHierarchical
    {
        IHierarchical? HierarchicalParent { get; }

        IHierarchicalRoot? HierarchicalRoot { get; }

        IReadOnlyList<IHierarchical> HierarchicalChildren { get; }

        event EventHandler<HierarchyAttachmentEventArgs> AttachedToHierarchy;
        event EventHandler<HierarchyAttachmentEventArgs> DetachedFromHierarchy;
    }

    public interface IHierarchicalRoot : IHierarchical
    {
        event EventHandler<IHierarchical> DescendantAttached;

        event EventHandler<IHierarchical> DescendantDetached;

        void OnDescendantAttached(IHierarchical descendant);

        void OnDescendantDetached(IHierarchical descendant);
    }

    public interface IModifiableHierarchical : IHierarchical
    {
        void AddChild(IHierarchical child);

        void RemoveChild(IHierarchical child);

        void SetParent(IHierarchical? parent);

        void NotifyAttachedToHierarchy(in HierarchyAttachmentEventArgs e);

        void NotifyDetachedFromHierarchy(in HierarchyAttachmentEventArgs e);
    }

    public readonly struct HierarchyAttachmentEventArgs(IHierarchicalRoot root, IHierarchical? parent)
    {
        public IHierarchicalRoot Root { get; } = root;

        public IHierarchical? Parent { get; } = parent;
    }

    public abstract class Hierarchical : IHierarchical
    {
        private IHierarchical? _parent;
        private IHierarchicalRoot? _root;
        private readonly List<IHierarchical> _children = new();

        public IHierarchical? HierarchicalParent => _parent;

        public IHierarchicalRoot? HierarchicalRoot => _root;

        public IReadOnlyList<IHierarchical> HierarchicalChildren => new List<IHierarchical>(_children);

        public event EventHandler<HierarchyAttachmentEventArgs>? AttachedToHierarchy;
        public event EventHandler<HierarchyAttachmentEventArgs>? DetachedFromHierarchy;

        /// <summary>
        /// 内部的に親を設定します（Modifiable派生クラスで使用）。
        /// </summary>
        protected void SetParentInternal(IHierarchical? parent)
        {
            var oldRoot = _root;
            _parent = parent;
            _root = (parent as IHierarchicalRoot) ?? parent?.HierarchicalRoot;

            var args = new HierarchyAttachmentEventArgs(oldRoot, _root);

            if (_root != null)
                AttachedToHierarchy?.Invoke(this, args);
            else
                DetachedFromHierarchy?.Invoke(this, args);
        }

        /// <summary>
        /// 子を追加します（Modifiable派生クラスで使用）。
        /// </summary>
        protected void AddChildInternal(IHierarchical child)
        {
            _children.Add(child);
        }

        /// <summary>
        /// 子を削除します（Modifiable派生クラスで使用）。
        /// </summary>
        protected void RemoveChildInternal(IHierarchical child)
        {
            _children.Remove(child);
        }

        /// <summary>
        /// イベントを明示的に発火したい場合に使用。
        /// </summary>
        protected void RaiseAttachedToHierarchy(HierarchyAttachmentEventArgs args)
            => AttachedToHierarchy?.Invoke(this, args);

        protected void RaiseDetachedFromHierarchy(HierarchyAttachmentEventArgs args)
            => DetachedFromHierarchy?.Invoke(this, args);
    }

    public class ModifiableHierarchical : Hierarchical, IModifiableHierarchical
    {
        public void AddChild(IHierarchical child)
        {
            if (child is IModifiableHierarchical modifiable)
            {
                modifiable.SetParent(this);
            }

            AddChildInternal(child);
        }

        public void RemoveChild(IHierarchical child)
        {
            RemoveChildInternal(child);

            if (child is IModifiableHierarchical modifiable)
            {
                modifiable.SetParent(null);
            }
        }

        public void SetParent(IHierarchical? parent)
        {
            SetParentInternal(parent);
        }

        public void NotifyAttachedToHierarchy(in HierarchyAttachmentEventArgs e)
        {
            RaiseAttachedToHierarchy(e);
        }

        public void NotifyDetachedFromHierarchy(in HierarchyAttachmentEventArgs e)
        {
            RaiseDetachedFromHierarchy(e);
        }
    }

}
