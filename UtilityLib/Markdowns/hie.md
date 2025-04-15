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


以下は、IHierarchical を対象とした便利な LINQ拡張メソッド です。
主に以下の操作に対応しています：

Flatten()：ツリー構造を平坦化（自己 + 子孫を列挙）

Find(Predicate)：条件に一致する最初のノードを探索

Filter(Predicate)：条件に一致するすべてのノードを列挙

Ancestors()：親をたどる列挙

Descendants()：子孫を列挙（自己を含まない）



---

✅ 拡張クラス：HierarchicalExtensions
```
public static class HierarchicalExtensions
{
    // 自身 + 全子孫を列挙
    public static IEnumerable<IHierarchical> Flatten(this IHierarchical node)
    {
        yield return node;
        foreach (var child in node.HierarchicalChildren)
        {
            foreach (var descendant in child.Flatten())
            {
                yield return descendant;
            }
        }
    }

    // 条件に一致する最初のノードを返す
    public static IHierarchical? Find(this IHierarchical node, Predicate<IHierarchical> predicate)
    {
        if (predicate(node)) return node;
        foreach (var child in node.HierarchicalChildren)
        {
            var found = child.Find(predicate);
            if (found != null) return found;
        }
        return null;
    }

    // 条件に一致するすべてのノードを列挙
    public static IEnumerable<IHierarchical> Filter(this IHierarchical node, Predicate<IHierarchical> predicate)
    {
        if (predicate(node)) yield return node;
        foreach (var child in node.HierarchicalChildren)
        {
            foreach (var match in child.Filter(predicate))
            {
                yield return match;
            }
        }
    }

    // 親をたどる（自己は含まない）
    public static IEnumerable<IHierarchical> Ancestors(this IHierarchical node)
    {
        var current = node.HierarchicalParent;
        while (current != null)
        {
            yield return current;
            current = current.HierarchicalParent;
        }
    }

    // 全子孫（自己は含まない）
    public static IEnumerable<IHierarchical> Descendants(this IHierarchical node)
    {
        foreach (var child in node.HierarchicalChildren)
        {
            yield return child;
            foreach (var descendant in child.Descendants())
            {
                yield return descendant;
            }
        }
    }
}

```
---

✅ 使用例

var root = new ProjectItem("Root");
var child = new ProjectItem("Child");
var grandChild = new ProjectItem("GrandChild");

root.AddChild(child);
child.AddChild(grandChild);

// Flatten
foreach (var item in root.Flatten())
    Console.WriteLine(item.Name);

// Find
var target = root.Find(x => x.Name == "GrandChild");

// Filter
var allWithG = root.Filter(x => x.Name.StartsWith("G"));

// Ancestors
foreach (var ancestor in grandChild.Ancestors())
    Console.WriteLine("Ancestor: " + ancestor.Name);

// Descendants
foreach (var desc in root.Descendants())
    Console.WriteLine("Descendant: " + desc.Name);


---

✅ 拡張案

OfType<T>()：特定の型にキャストされたノードだけ列挙

BreadthFirstFlatten()：幅優先探索で平坦化

Level()：各ノードの階層深さを返す

PathToRoot()：ルートまでのパスを返す（Ancestors().Reverse().Prepend(self)）


必要であれば、これらの追加も実装できます。どれか追加しましょうか？


