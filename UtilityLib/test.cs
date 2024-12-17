using System;
using System.Collections.Generic;

public class ScreenState : IObservable<string>
{
    private List<IObserver<string>> observers = new List<IObserver<string>>();
    private string _currentState;

    public string CurrentState
    {
        get => _currentState;
        set
        {
            _currentState = value;
            NotifyObservers();
        }
    }

    public IDisposable Subscribe(IObserver<string> observer)
    {
        if (!observers.Contains(observer))
            observers.Add(observer);
        
        observer.OnNext(_currentState); // 初期状態を通知
        return new Unsubscriber(observers, observer);
    }

    private void NotifyObservers()
    {
        foreach (var observer in observers)
        {
            observer.OnNext(_currentState);
        }
    }

    private class Unsubscriber : IDisposable
    {
        private List<IObserver<string>> _observers;
        private IObserver<string> _observer;

        public Unsubscriber(List<IObserver<string>> observers, IObserver<string> observer)
        {
            _observers = observers;
            _observer = observer;
        }

        public void Dispose()
        {
            if (_observer != null && _observers.Contains(_observer))
                _observers.Remove(_observer);
        }
    }
}

public partial class StartPreProcessingControl : UserControl, IObserver<string>
{
    public StartPreProcessingControl()
    {
        InitializeComponent();
    }

    public void OnNext(string value)
    {
        if (value == "StartPreProcessing")
        {
            label1.Text = "開始前処理中...";
            // ここで初期処理を実行する
        }
    }
    public void OnError(Exception error) { }
    public void OnCompleted() { }
}

