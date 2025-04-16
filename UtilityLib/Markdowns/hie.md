```
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
```
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
```
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
```

---

✅ 拡張案

OfType<T>()：特定の型にキャストされたノードだけ列挙

BreadthFirstFlatten()：幅優先探索で平坦化

Level()：各ノードの階層深さを返す

PathToRoot()：ルートまでのパスを返す（Ancestors().Reverse().Prepend(self)）


必要であれば、これらの追加も実装できます。どれか追加しましょうか？


了解です！HierarchicalBase にて 自己参照や循環参照 を検出し、例外をスローするロジックを組み込みます。


---

✅ 追加仕様

例外条件

SetParent(this) のような 自己参照

親候補に自分自身の 子孫が含まれる → 循環参照



---

✅ 改良版 HierarchicalBase
```
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
        if (child == this)
            throw new InvalidOperationException("Cannot add self as a child.");

        if (child.Flatten().Contains(this))
            throw new InvalidOperationException("Cannot add child that would create a circular reference.");

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
        if (parent == this)
            throw new InvalidOperationException("Cannot set parent to self.");

        if (parent != null && parent.Flatten().Contains(this))
            throw new InvalidOperationException("Cannot set parent that would create a circular reference.");

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
```

---

✅ 依存拡張：Flatten()（拡張メソッド）
```
public static class HierarchicalExtensions
{
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
}
```

---

✅ テスト例
```
var a = new ProjectItem("A");
var b = new ProjectItem("B");
var c = new ProjectItem("C");

a.AddChild(b);
b.AddChild(c);

// 循環参照を試みる（例外スロー）
try
{
    c.AddChild(a); // InvalidOperationException: circular reference
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
```

---

✅ 拡張提案

bool HasCycle()：安全チェック専用メソッド（例外スローではなく true/false）

TrySetParent(IHierarchical? parent, out string? error)：安全な親設定

IHierarchical.RootPath：ルートへのパスをキャッシュ


要望があれば、より防御的なバージョンも構築できます。続けますか？


了解です！例外をスローせず、安全に操作を試みてエラーを呼び出し側に返す Try系メソッド を設計します。


---

✅ Try系の目的

TrySetParent(IHierarchical? parent, out string? error)

TryAddChild(IHierarchical child, out string? error)

TryRemoveChild(IHierarchical child, out string? error)


これにより呼び出し側は例外制御をせずに安全にチェックできます。


---

✅ HierarchicalBase への追加
```
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
        if (!TryAddChild(child, out var error))
            throw new InvalidOperationException(error);
    }

    public void RemoveChild(IHierarchical child)
    {
        if (!TryRemoveChild(child, out var error))
            throw new InvalidOperationException(error);
    }

    public void SetParent(IHierarchical? parent)
    {
        if (!TrySetParent(parent, out var error))
            throw new InvalidOperationException(error);
    }

    // Try系の実装
    public bool TryAddChild(IHierarchical child, out string? error)
    {
        error = null;

        if (child == this)
        {
            error = "Cannot add self as a child.";
            return false;
        }

        if (child.Flatten().Contains(this))
        {
            error = "Adding this child would create a circular reference.";
            return false;
        }

        if (_children.Contains(child))
        {
            error = "This child is already added.";
            return false;
        }

        if (child is IModifiableHierarchical modifiable)
        {
            if (!modifiable.TrySetParent(this, out error))
                return false;
        }

        _children.Add(child);
        return true;
    }

    public bool TryRemoveChild(IHierarchical child, out string? error)
    {
        error = null;

        if (!_children.Contains(child))
        {
            error = "The specified child is not present.";
            return false;
        }

        _children.Remove(child);

        if (child is IModifiableHierarchical modifiable)
        {
            if (!modifiable.TrySetParent(null, out error))
                return false;
        }

        return true;
    }

    public bool TrySetParent(IHierarchical? parent, out string? error)
    {
        error = null;

        if (parent == this)
        {
            error = "Cannot set parent to self.";
            return false;
        }

        if (parent != null && parent.Flatten().Contains(this))
        {
            error = "Setting this parent would create a circular reference.";
            return false;
        }

        var oldRoot = HierarchicalRoot;
        _parent = parent;
        var newRoot = HierarchicalRoot;

        var args = new HierarchyAttachmentEventArgs(oldRoot, newRoot);
        NotifyDetachedFromHierarchy(args);
        NotifyAttachedToHierarchy(args);

        return true;
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

```
---

✅ 利用例（Try系）

if (!a.TryAddChild(b, out var error))
{
    Console.WriteLine($"AddChild failed: {error}");
}

if (!b.TrySetParent(c, out var err2))
{
    Console.WriteLine($"SetParent failed: {err2}");
}


---

✅ 拡張案

TryMoveTo(IHierarchical newParent, out string? error)：子を別の親に移動

CanAddChild(IHierarchical child)：バリデーションのみ（エラー文字列なし）

ValidationResult ValidateStructure()：ツリー構造全体の整合性チェック


これらも加えましょうか？


了解です！HierarchicalBase に ClearAllChildren() メソッド（および TryClearAllChildren(out string? error)）を追加します。


---

✅ 振る舞い仕様

すべての子要素を削除

各子要素には SetParent(null) を呼ぶ（親リンク解除）

IModifiableHierarchical のみ対応

失敗があれば途中で中断、エラーを返す Try 版も提供



---

✅ 実装追加（HierarchicalBase）

public void ClearAllChildren()
{
    if (!TryClearAllChildren(out var error))
        throw new InvalidOperationException(error);
}

public bool TryClearAllChildren(out string? error)
{
    error = null;

    // 一時リスト化（foreachでの変更回避）
    var childrenSnapshot = _children.ToList();

    foreach (var child in childrenSnapshot)
    {
        if (child is IModifiableHierarchical modifiable)
        {
            if (!modifiable.TrySetParent(null, out error))
                return false;
        }

        _children.Remove(child);
    }

    return true;
}


---

✅ 使用例

var root = new ProjectItem("Root");
var child1 = new ProjectItem("Child1");
var child2 = new ProjectItem("Child2");

root.AddChild(child1);
root.AddChild(child2);

// Try版で安全にクリア
if (!root.TryClearAllChildren(out var error))
{
    Console.WriteLine("Failed to clear: " + error);
}
else
{
    Console.WriteLine("All children removed.");
}


---

✅ 拡張提案

RemoveChildRecursive(IHierarchical target)：ツリー全体から特定ノードを除去

DetachFromParent()：SetParent(null) の簡略化版

MoveAllChildrenTo(IModifiableHierarchical newParent)：子を一括移動

ReplaceChildren(IEnumerable<IHierarchical>)：ツリー構造の置き換え


どこまで機能を広げたいかに応じて、拡張していきましょうか？


了解です！HierarchicalBase に ClearAllChildren() メソッド（および TryClearAllChildren(out string? error)）を追加します。


---

✅ 振る舞い仕様

すべての子要素を削除

各子要素には SetParent(null) を呼ぶ（親リンク解除）

IModifiableHierarchical のみ対応

失敗があれば途中で中断、エラーを返す Try 版も提供



---

✅ 実装追加（HierarchicalBase）

public void ClearAllChildren()
{
    if (!TryClearAllChildren(out var error))
        throw new InvalidOperationException(error);
}

public bool TryClearAllChildren(out string? error)
{
    error = null;

    // 一時リスト化（foreachでの変更回避）
    var childrenSnapshot = _children.ToList();

    foreach (var child in childrenSnapshot)
    {
        if (child is IModifiableHierarchical modifiable)
        {
            if (!modifiable.TrySetParent(null, out error))
                return false;
        }

        _children.Remove(child);
    }

    return true;
}


---

✅ 使用例

var root = new ProjectItem("Root");
var child1 = new ProjectItem("Child1");
var child2 = new ProjectItem("Child2");

root.AddChild(child1);
root.AddChild(child2);

// Try版で安全にクリア
if (!root.TryClearAllChildren(out var error))
{
    Console.WriteLine("Failed to clear: " + error);
}
else
{
    Console.WriteLine("All children removed.");
}


---

✅ 拡張提案

RemoveChildRecursive(IHierarchical target)：ツリー全体から特定ノードを除去

DetachFromParent()：SetParent(null) の簡略化版

MoveAllChildrenTo(IModifiableHierarchical newParent)：子を一括移動

ReplaceChildren(IEnumerable<IHierarchical>)：ツリー構造の置き換え


どこまで機能を広げたいかに応じて、拡張していきましょうか？

了解しました！
1つのクラスでルート・中間ノード・リーフノードの全ての役割を担えるようにするには、以下のように IHierarchicalRoot, IModifiableHierarchical のすべてを実装する統合クラスを用意します。


---

✅ 統合クラス設計：HierarchicalNode

public class HierarchicalNode : HierarchicalBase, IHierarchicalRoot
{
    public string Name { get; }

    public HierarchicalNode(string name)
    {
        Name = name;
    }

    public event EventHandler<IHierarchical>? DescendantAttached;
    public event EventHandler<IHierarchical>? DescendantDetached;

    public void OnDescendantAttached(IHierarchical descendant)
    {
        DescendantAttached?.Invoke(this, descendant);
        // さらに親のルートにも伝播させる（多段階ルート）
        if (HierarchicalParent is IHierarchicalRoot parentRoot)
            parentRoot.OnDescendantAttached(descendant);
    }

    public void OnDescendantDetached(IHierarchical descendant)
    {
        DescendantDetached?.Invoke(this, descendant);
        if (HierarchicalParent is IHierarchicalRoot parentRoot)
            parentRoot.OnDescendantDetached(descendant);
    }

    // 自分自身がルートとして機能する場合はこれをオーバーライド
    public override IHierarchicalRoot? HierarchicalRoot => this;
}


---

✅ 使用例（単一クラスでツリー構築）

var root = new HierarchicalNode("Root");
var child1 = new HierarchicalNode("Child1");
var child2 = new HierarchicalNode("Child2");
var grandChild = new HierarchicalNode("GrandChild");

root.AddChild(child1);
child1.AddChild(child2);
child2.AddChild(grandChild);

// イベントハンドリングも可能
root.DescendantAttached += (s, e) => Console.WriteLine($"{((HierarchicalNode)e).Name} attached");
root.DescendantDetached += (s, e) => Console.WriteLine($"{((HierarchicalNode)e).Name} detached");

// テスト: 循環を試みる
if (!grandChild.TryAddChild(root, out var error))
    Console.WriteLine($"循環防止成功: {error}");

// 子一覧
foreach (var item in root.Flatten())
    Console.WriteLine(item is HierarchicalNode node ? node.Name : "Unknown");


---

✅ 特徴


---

✅ 拡張提案

IsRoot => HierarchicalParent == null

Depth => Ancestors().Count()

Path => Ancestors().Reverse().Append(this)

JSONツリー構造への変換（ToJson()）

ツリー変更通知イベント（INotifyTreeChanged）


必要であれば、それらも追加できます。
次はどの機能を強化しましょうか？



