WinFormsなどでイベントバスを使う際に、イベントの登録や解除を適切に管理するクラス構成は、以下のように構築できます。


---

■ 基本構成案

1. IEvent インターフェース

イベントのマーカー。

public interface IEvent { }

2. IEventHandler<T> インターフェース

型安全なイベントハンドラー。

public interface IEventHandler<in T> where T : IEvent
{
    void Handle(T @event);
}


---

3. EventBus クラス

イベントの登録・発行・解除を管理する中心クラス。

public class EventBus
{
    private readonly Dictionary<Type, List<object>> _handlers = new();

    public void Register<T>(IEventHandler<T> handler) where T : IEvent
    {
        var type = typeof(T);
        if (!_handlers.ContainsKey(type))
        {
            _handlers[type] = new List<object>();
        }
        _handlers[type].Add(handler);
    }

    public void Unregister<T>(IEventHandler<T> handler) where T : IEvent
    {
        var type = typeof(T);
        if (_handlers.TryGetValue(type, out var list))
        {
            list.Remove(handler);
            if (list.Count == 0)
                _handlers.Remove(type);
        }
    }

    public void Publish<T>(T @event) where T : IEvent
    {
        var type = typeof(T);
        if (_handlers.TryGetValue(type, out var list))
        {
            foreach (var handler in list.Cast<IEventHandler<T>>())
            {
                handler.Handle(@event);
            }
        }
    }
}


---

4. EventHandlerManager クラス

登録と解除をまとめて管理。

public class EventHandlerManager
{
    private readonly EventBus _eventBus;
    private readonly List<(Type type, object handler)> _registeredHandlers = new();

    public EventHandlerManager(EventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public void Register<T>(IEventHandler<T> handler) where T : IEvent
    {
        _eventBus.Register(handler);
        _registeredHandlers.Add((typeof(T), handler));
    }

    public void UnregisterAll()
    {
        foreach (var (type, handler) in _registeredHandlers)
        {
            var method = typeof(EventBus).GetMethod("Unregister")!.MakeGenericMethod(type);
            method.Invoke(_eventBus, new object[] { handler });
        }
        _registeredHandlers.Clear();
    }
}


---

■ 使用例

public class MyEvent : IEvent
{
    public string Message { get; set; }
}

public class MyEventHandler : IEventHandler<MyEvent>
{
    public void Handle(MyEvent @event)
    {
        Console.WriteLine("Handled: " + @event.Message);
    }
}

var bus = new EventBus();
var manager = new EventHandlerManager(bus);

var handler = new MyEventHandler();
manager.Register(handler);

bus.Publish(new MyEvent { Message = "Hello World" });

manager.UnregisterAll();


---

■ 拡張案

WeakReference にしてメモリリーク対策（WPF・WinFormsで特に有効）

async対応（Task HandleAsync(T @event)）

Subscribe<T>(Action<T>) による簡易登録

グローバルイベントとスコープイベントの分離

Priority/Filter付きハンドラー管理



---

要望があれば、WinFormsでのUI連動例も出せます。次に何を追加したい？

アプリ全体で使えるイベント管理機構として、シンプルかつ拡張可能な EventBus パターン を提案します。WinForms + MVVM アーキテクチャにも適合し、疎結合な通信が可能になります。


---

■ 基本構成：EventBus（イベントの購読・発行・解除）

1. IEvent: イベントのマーカーインターフェース

public interface IEvent { }


---

2. IEventHandler<T>: 各イベントに対応するハンドラー

public interface IEventHandler<T> where T : IEvent
{
    void Handle(T e);
}


---

3. EventBus 実装

public class EventBus
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();

    public void Subscribe<T>(Action<T> handler) where T : IEvent
    {
        var type = typeof(T);
        if (!_handlers.ContainsKey(type))
        {
            _handlers[type] = new List<Delegate>();
        }
        _handlers[type].Add(handler);
    }

    public void Unsubscribe<T>(Action<T> handler) where T : IEvent
    {
        var type = typeof(T);
        if (_handlers.TryGetValue(type, out var list))
        {
            list.Remove(handler);
        }
    }

    public void Publish<T>(T eventData) where T : IEvent
    {
        var type = typeof(T);
        if (_handlers.TryGetValue(type, out var list))
        {
            foreach (var handler in list.OfType<Action<T>>())
            {
                handler.Invoke(eventData);
            }
        }
    }

    public void Clear()
    {
        _handlers.Clear();
    }
}


---

■ 利用例

1. イベント定義

public class ProjectChangedEvent : IEvent
{
    public string PropertyName { get; }
    public ProjectChangedEvent(string propertyName)
    {
        PropertyName = propertyName;
    }
}


---

2. 購読（Subscribe）

_eventBus.Subscribe<ProjectChangedEvent>(e =>
{
    Console.WriteLine($"Changed: {e.PropertyName}");
});


---

3. 発行（Publish）

_eventBus.Publish(new ProjectChangedEvent("Name"));


---

4. 購読解除（Unsubscribe）

_eventBus.Unsubscribe<ProjectChangedEvent>(handler);


---

■ AppContextでの管理例

public class AppContext
{
    public EventBus EventBus { get; } = new();
}

すべてのフォームやViewModelに AppContext を渡して、EventBus を共有できます。


---

■ 拡張案

WeakReference を使ってメモリリーク対策

AsyncEventBus に拡張（async/await対応）

優先度付きイベント (PriorityEventBus)

ログやデバッグ出力を仕込むことで、イベントの流れを追跡可能に



---

必要であれば、イベント種別ごとの管理クラスや ICommand 連携もできます。
フォーム間通知や非同期データ処理のトリガーとしても活用可能です。

続きでフォーム統合やMVVM連携のサンプルを出しましょうか？

