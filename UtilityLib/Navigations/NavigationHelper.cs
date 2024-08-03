using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace UtilityLib.Navigations
{
    public class NavigationHelper : DependencyObject
    {
        private Page Page { get; set; }
        private Frame Frame { get { return this.Page.Frame; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationHelper"/> class.
        /// </summary>
        /// <param name="page">A reference to the current page used for navigation.
        /// This reference allows for frame manipulation.</param>
        public NavigationHelper(Page page)
        {
            this.Page = page;
        }

        #region Process lifetime management

        private string _pageKey;

        public event LoadStateEventHandler LoadState;
        public event SaveStateEventHandler SaveState;

        public void OnNavigatedTo(NavigationEventArgs e)
        {
            var frameState = SuspensionManager.SessionStateForFrame(this.Frame);
            this._pageKey = "Page-" + this.Frame.BackStackDepth;

            if (e.NavigationMode == NavigationMode.New)
            {
                // Clear existing state for forward navigation when adding a new page to the
                // navigation stack
                var nextPageKey = this._pageKey;
                int nextPageIndex = this.Frame.BackStackDepth;
                while (frameState.Remove(nextPageKey))
                {
                    nextPageIndex++;
                    nextPageKey = "Page-" + nextPageIndex;
                }

                // Pass the navigation parameter to the new page
                this.LoadState?.Invoke(this, new LoadStateEventArgs(e.Parameter, null));
            }
            else
            {
                // Pass the navigation parameter and preserved page state to the page, using
                // the same strategy for loading suspended state and recreating pages discarded
                // from cache
                this.LoadState?.Invoke(this, new LoadStateEventArgs(e.Parameter, (Dictionary<string, object>)frameState[this._pageKey]));
            }
        }

        public void OnNavigatedFrom(NavigationEventArgs e)
        {
            var frameState = SuspensionManager.SessionStateForFrame(this.Frame);
            var pageState = new Dictionary<string, object>();
            this.SaveState?.Invoke(this, new SaveStateEventArgs(pageState));
            frameState[_pageKey] = pageState;
        }

        #endregion
    }


    public delegate void LoadStateEventHandler(object sender, LoadStateEventArgs e);

    public delegate void SaveStateEventHandler(object sender, SaveStateEventArgs e);

    public class LoadStateEventArgs : EventArgs
    {

        public object NavigationParameter { get; private set; }

        public Dictionary<string, object> PageState { get; private set; }

        public LoadStateEventArgs(object navigationParameter, Dictionary<string, object> pageState) : base()
        {
            this.NavigationParameter = navigationParameter;
            this.PageState = pageState;
        }
    }

    public class SaveStateEventArgs : EventArgs
    {
        public Dictionary<string, object> PageState { get; private set; }

        public SaveStateEventArgs(Dictionary<string, object> pageState) : base()
        {
            this.PageState = pageState;
        }
    }
}
}
