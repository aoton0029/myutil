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

