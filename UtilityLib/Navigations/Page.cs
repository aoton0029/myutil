using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityLib.Navigations.MVVC;

namespace UtilityLib.Navigations
{
    public interface IController { }


    public abstract class BaseController : IController, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void InvokePropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }

    public partial class Page : UserControl, INotifyPropertyChanged
    {
        public delegate void ShowHarness(object sender, EventArgs e);
        public event ShowHarness OnShow;

        public delegate void LeaveHarness(object sender, EventArgs e);
        public new event LeaveHarness OnLeave;

        public delegate void MessageHandler(object sender, MessageEventArgs e);
        public event MessageHandler MessageReceived;

        public event PropertyChangedEventHandler PropertyChanged;

        public BaseController Controller { get; set; }

        public PageSizeModeDef PageSizeMode { get; set; }

        public enum PageSizeModeDef
        {
            Manual,
            Auto
        }

        public Page()
        {
            if (this.Parent != null)
                this.Size = this.Parent.Size;
        }

        public Page(BaseController pageController)
        {
            Controller = pageController;
        }

        public new virtual void Show()
        {
            if (PageSizeMode == PageSizeModeDef.Auto)
                this.Size = Parent.Size;

            AddBindings();
            OnShow?.Invoke(this, new EventArgs());
        }

        public new virtual void Leave()
        {
            RemoveBindings();
            OnLeave?.Invoke(this, new EventArgs());
        }

        public void InvokePropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        internal void ReceiveMessage(string key, object data)
        {
            MessageReceived?.Invoke(this, new MessageEventArgs(key, data));
        }

        public virtual void AddBindings() { }

        public virtual void RemoveBindings() { }
    }

    public class MessageEventArgs : EventArgs
    {
        public string Key { get; }
        public object Data { get; }

        public MessageEventArgs(string key, object data) : base()
        {
            Key = key;
            Data = data;
        }
    }
}
