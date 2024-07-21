using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.AxHost;


namespace UtilityLib.PageNavigation
{

    public interface IMyObserver<T>
    {
        void OnUpdate(T value);
        void OnNext(string state);
        void OnBack();
        void OnError(Exception error);
        void OnComplete();
        void OnCanceled();
        void OnTerminate();

        void NotifySubject(string state);
    }

    public interface IMySubject
    {

    }

    public class Subject<T>
    {
        private List<IMyObserver<T>> observers = new List<IMyObserver<T>>();
        private string state;
        public string State
        {
            get { return state; }
            set
            {
                state = value;
                NotifyOnNext();
            }
        }
        

        public void Attach(IMyObserver<T> observer)
        {
            observers.Add(observer);
        }

        public void Detach(IMyObserver<T> observer)
        {
            observers.Remove(observer);
        }

        private void NotifyOnNext()
        {
            foreach (var observer in observers)
            {
                observer.OnNext(state);
            }
        }

        public void NotifyOnBack()
        {
            foreach (var observer in observers)
            {
                observer.OnBack();
            }
        }

        public void NotifyOnError(Exception error)
        {
            foreach (var observer in observers)
            {
                observer.OnError(error);
            }
        }

        public void NotifyOnComplete()
        {
            foreach (var observer in observers)
            {
                observer.OnComplete();
            }
        }

        public void NotifyOnTerminate()
        {
            foreach (var observer in observers)
            {
                observer.OnTerminate();
            }
        }

        public void NotifyOnUpdate(T data)
        {
            foreach (var observer in observers)
            {
                observer.OnUpdate(data);
            }
        }

        // Observerからの通知を受け取るメソッド
        public void ReceiveNotificationFromObserver(string state)
        {
            this.State = state;
        }

    }


    public partial class BidirectionalObserverControl<T> : UserControl, IMyObserver<T>
    {
        private Subject<T> subject;

        public BidirectionalObserverControl(Subject<T> subject)
        {
            InitializeComponent();
            this.subject = subject;
        }

        public void OnNext(string state)
        {

        }

        public void OnBack()
        {

        }

        public void OnError(Exception error)
        {

        }

        public void OnComplete()
        {

        }

        public void OnTerminate()
        {

        }

        public void OnCanceled()
        {

        }

        public void OnUpdate(T data)
        {

        }

        public void NotifySubject(string state)
        {
            subject.ReceiveNotificationFromObserver(state);
        }
    }

}
