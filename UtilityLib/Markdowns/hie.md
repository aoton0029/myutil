public interface IHierarchical { IHierarchical? HierarchicalParent { get; }

IHierarchicalRoot? HierarchicalRoot { get; }

IReadOnlyList HierarchicalChildren { get; }

event EventHandler AttachedToHierarchy;

event EventHandler DetachedFromHierarchy;

}

public interface IHierarchicalRoot : IHierarchical { event EventHandler DescendantAttached;

event EventHandler DescendantDetached;

void OnDescendantAttached(IHierarchical descendant);

void OnDescendantDetached(IHierarchical descendant);

}

public interface IModifiableHierarchical : IHierarchical { void AddChild(IHierarchical child);

void RemoveChild(IHierarchical child);

void SetParent(IHierarchical? parent);

void NotifyAttachedToHierarchy(in HierarchyAttachmentEventArgs e);

void NotifyDetachedFromHierarchy(in HierarchyAttachmentEventArgs e);

}

以下は、あなたの定義に基づく IHierarchical, IHierarchicalRoot, IModifiableHierarchical の実装と、それを活かした拡張案になります。


---

✅ 改良ポイント

1. 型パラメータでの汎用性: IHierarchical<T> にして、子や親を特定の型にできるように。


2. HierarchyAttachmentEventArgs の定義補完。


3. 抽象クラス HierarchicalBase の用意。


4. 双方向リンクの維持: AddChild / SetParent の整合性を保証。


5. ルートの更新検知: AttachedToHierarchy イベントを発火。




---

✅ インターフェース（修正型）

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


---

✅ HierarchyAttachmentEventArgs の例

public class HierarchyAttachmentEventArgs : EventArgs
{
    public IHierarchical? OldRoot { get; }
    public IHierarchical? NewRoot { get; }

    public HierarchyAttachmentEventArgs(IHierarchical? oldRoot, IHierarchical? newRoot)
    {
        OldRoot = oldRoot;
        NewRoot = newRoot;
    }
}


---

✅ 抽象クラスによる基本実装

public abstract class HierarchicalBase : IModifiableHierarchical
{
    private IHierarchical? _parent;
    private readonly List<IHierarchical> _children = new();

    public IHierarchical? HierarchicalParent => _parent;
    public virtual IHierarchicalRoot? HierarchicalRoot => _parent?.HierarchicalRoot;

    public IReadOnlyList<IHierarchical> HierarchicalChildren => _children;

    public event EventHandler<HierarchyAttachmentEventArgs>? AttachedToHierarchy;
    public event EventHandler<HierarchyAttachmentEventArgs>? DetachedFromHierarchy;

    public void AddChild(IHierarchical child)
    {
        if (!_children.Contains(child))
        {
            if (child is IModifiableHierarchical modifiable)
                modifiable.SetParent(this);

            _children.Add(child);
        }
    }

    public void RemoveChild(IHierarchical child)
    {
        if (_children.Remove(child) && child is IModifiableHierarchical modifiable)
        {
            modifiable.SetParent(null);
        }
    }

    public void SetParent(IHierarchical? parent)
    {
        if (_parent == parent) return;

        var oldRoot = HierarchicalRoot;
        _parent = parent;
        var newRoot = HierarchicalRoot;

        var args = new HierarchyAttachmentEventArgs(oldRoot, newRoot);
        NotifyDetachedFromHierarchy(args);
        NotifyAttachedToHierarchy(args);
    }

    public void NotifyAttachedToHierarchy(in HierarchyAttachmentEventArgs e)
    {
        AttachedToHierarchy?.Invoke(this, e);
        HierarchicalRoot?.OnDescendantAttached(this);
    }

    public void NotifyDetachedFromHierarchy(in HierarchyAttachmentEventArgs e)
    {
        DetachedFromHierarchy?.Invoke(this, e);
        HierarchicalRoot?.OnDescendantDetached(this);
    }
}


---

✅ 拡張案

1. 型指定（ジェネリック）：IHierarchical<T> にすることで、ツリーのノード型を固定可能。


2. 双方向イベント伝播：親変更時に 旧親→新親 のイベントを連鎖的に発火。


3. Undo対応：IUndoableHierarchical を追加し、親・子の変更履歴をスタック保存。


4. シリアライズ対応：ツリー構造を JSON/XML に保存・復元するための ISerializableHierarchical。


5. LINQ操作拡張：IEnumerable<IHierarchical> に .Descendants() / .Ancestors() / .Flatten() のような拡張メソッドを定義。




---

必要なら、具体的な利用例（例えば ProjectItem や TreeNode 実装）も提示できます。どこまで拡張したいですか？

