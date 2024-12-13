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

public partial class ResultScreenControl : UserControl, IObserver<string>
{
    private ScreenState state;

    public ResultScreenControl(ScreenState state)
    {
        InitializeComponent();
        this.state = state;
    }

    public void OnNext(string value)
    {
        if (value == "ResultScreen")
        {
            label1.Text = "結果画面: 処理結果を表示します";
        }
    }

    private void btnNext_Click(object sender, EventArgs e)
    {
        state.CurrentState = "EndScreen";
    }

    public void OnError(Exception error) { }
    public void OnCompleted() { }
}

public partial class EndProcessingControl : UserControl, IObserver<string>
{
    public EndProcessingControl()
    {
        InitializeComponent();
    }

    public void OnNext(string value)
    {
        if (value == "EndScreen")
        {
            label1.Text = "終了時処理を実行します...";
        }
    }

    public void OnError(Exception error) { }
    public void OnCompleted() { }
}

using System;
using System.Windows.Forms;

public partial class MainForm : Form
{
    private ScreenState screenState = new ScreenState();

    private StartPreProcessingControl preProcessingControl;
    private StartInputControl inputControl;
    private ResultScreenControl resultControl;
    private EndProcessingControl endControl;

    public MainForm()
    {
        InitializeComponent();
        InitializeControls();
        screenState.CurrentState = "StartPreProcessing"; // 初期状態
    }

    private void InitializeControls()
    {
        preProcessingControl = new StartPreProcessingControl();
        inputControl = new StartInputControl(screenState);
        resultControl = new ResultScreenControl(screenState);
        endControl = new EndProcessingControl();

        screenState.Subscribe(preProcessingControl);
        screenState.Subscribe(inputControl);
        screenState.Subscribe(resultControl);
        screenState.Subscribe(endControl);

        LoadControl(preProcessingControl);
        screenState.Subscribe(new ScreenObserver(this));
    }

    public void LoadControl(UserControl control)
    {
        Controls.Clear();
        control.Dock = DockStyle.Fill;
        Controls.Add(control);
    }

    private class ScreenObserver : IObserver<string>
    {
        private MainForm form;

        public ScreenObserver(MainForm form)
        {
            this.form = form;
        }

        public void OnNext(string value)
        {
            switch (value)
            {
                case "StartPreProcessing":
                    form.LoadControl(form.preProcessingControl);
                    break;
                case "StartScreen":
                    form.LoadControl(form.inputControl);
                    break;
                case "ResultScreen":
                    form.LoadControl(form.resultControl);
                    break;
                case "EndScreen":
                    form.LoadControl(form.endControl);
                    break;
            }
        }

        public void OnError(Exception error) { }
        public void OnCompleted() { }
    }
}