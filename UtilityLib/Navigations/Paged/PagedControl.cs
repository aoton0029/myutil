using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Navigations.Paged
{
    public partial class PagedControl : Control
    {
        protected internal virtual void OnPageAdded(PageEventArgs e) { PageAdded?.Invoke(this, e); }
        protected internal virtual void OnPageRemoved(PageEventArgs e) { PageRemoved?.Invoke(this, e); }
        protected internal virtual void OnPageChanging(PageChangingEventArgs e) { PageChanging?.Invoke(this, e); }
        protected internal virtual void OnPageChanged(PageChangedEventArgs e) { PageChanged?.Invoke(this, e); }
        protected internal virtual void OnPageValidating(PageValidatingEventArgs e) { PageValidating?.Invoke(this, e); }
        protected internal virtual void OnPageValidated(PageEventArgs e) { PageValidated?.Invoke(this, e); }
        protected internal virtual void OnPageHidden(PageEventArgs e) { PageHidden?.Invoke(this, e); }
        protected internal virtual void OnPageShown(PageEventArgs e) { PageShown?.Invoke(this, e); }
        protected internal virtual void OnPagePaint(PagePaintEventArgs e) { PagePaint?.Invoke(this, e); }
        protected internal virtual void OnCreateUIControls(CreateUIControlsEventArgs e)
        {
            CreateUIControls?.Invoke(this, e);

            creatingUIControls = true;
            int i = 0;
            foreach (Control control in e.Controls)
            {
                Controls.Add(control);
                Controls.SetChildIndex(control, i);
                i++;
            }
            creatingUIControls = false;

            uiControlCount = e.Controls.Length;
        }

        protected internal virtual void OnUpdateUIControls(EventArgs e) { UpdateUIControls?.Invoke(this, e); }
        public event EventHandler<PageEventArgs> PageAdded;
        public event EventHandler<PageEventArgs> PageRemoved;
        public event EventHandler<PageChangingEventArgs> PageChanging;
        public event EventHandler<PageChangedEventArgs> PageChanged;
        public event EventHandler<PageValidatingEventArgs> PageValidating;
        public event EventHandler<PageEventArgs> PageValidated;
        public event EventHandler<PageEventArgs> PageHidden;
        public event EventHandler<PageEventArgs> PageShown;
        public event EventHandler<PagePaintEventArgs> PagePaint;
        public event EventHandler<CreateUIControlsEventArgs> CreateUIControls;
        public event EventHandler UpdateUIControls;

        private int selectedIndex;
        private Page lastSelectedPage;
        private Page selectedPage;
        private BorderStyle borderStyle;
        private bool creatingUIControls;
        private int uiControlCount;

        internal int FirstPageIndex => uiControlCount;
        internal int PageCount => Controls.Count - uiControlCount;
        public virtual Page SelectedPage
        {
            get => selectedPage;
            set => ChangePage(value);
        }

        public virtual int SelectedIndex
        {
            get => selectedIndex;
            set => ChangePage(value == -1 ? null : Pages[value]);
        }

        public virtual PageCollection Pages { get; }

        public new Rectangle ClientRectangle
        {
            get
            {
                var rect = base.ClientRectangle;
                if (BorderStyle != BorderStyle.None)
                    rect.Inflate(-1, -1);
                return rect;
            }
        }

        public BorderStyle BorderStyle { get => borderStyle; set { borderStyle = value; Invalidate(); } }

        protected override Size DefaultSize => new Size(300, 200);

        public virtual bool CanGoBack => (Pages.Count != 0) && (SelectedIndex != -1) && !(ReferenceEquals(SelectedPage, Pages[0]));

        public virtual bool CanGoNext => (Pages.Count != 0) && (SelectedIndex != -1) && !(ReferenceEquals(SelectedPage, Pages[Pages.Count - 1]));

        public override Rectangle DisplayRectangle => new Rectangle(ClientRectangle.Left, ClientRectangle.Top, ClientRectangle.Width, ClientRectangle.Height);

        public override string Text { get => base.Text; set => base.Text = value; }
        public new ControlCollection Controls => base.Controls;
        public new event EventHandler TextChanged;

        public PagedControl()
        {
            creatingUIControls = false;
            uiControlCount = 0;

            Pages = new PageCollection(this);

            selectedIndex = -1;
            lastSelectedPage = null;
            selectedPage = null;

            borderStyle = BorderStyle.FixedSingle;

            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint | ControlStyles.Opaque | ControlStyles.ResizeRedraw, true);
        }

        public void GoBack()
        {
            if (CanGoBack)
                SelectedIndex = SelectedIndex - 1;
        }

        public void GoNext()
        {
            if (CanGoNext)
                SelectedIndex = SelectedIndex + 1;
        }

        protected void UpdatePages()
        {
            for (int i = 0; i < Pages.Count; i++)
            {
                var page = Pages[i];

                if (i == SelectedIndex)
                {
                    page.Bounds = DisplayRectangle;
                    page.Invalidate();

                    page.Visible = true;
                }
                else
                {
                    page.Visible = false;
                }
            }
        }

        protected void ChangePage(Page page, bool allowModify = true)
        {
            int index = (page == null) ? -1 : Pages.IndexOf(page);

            if (page != null && index == -1)
                throw new ArgumentException("Page is not found in the page collection.");
            else if (page == null && Pages.Count != 0)
                throw new ArgumentException("Cannot set SelectedPage to null if the control has at least one page.");
            else if (selectedPage != null && page != null && selectedPage == page)
                return;

            if (selectedPage != null && selectedPage.CausesValidation)
            {
                PageValidatingEventArgs pve = new PageValidatingEventArgs(selectedPage);
                OnPageValidating(pve);
                if (allowModify && pve.Cancel) return;

                OnPageValidated(new PageEventArgs(selectedPage));
            }

            PageChangingEventArgs pce = new PageChangingEventArgs(selectedPage, page);
            OnPageChanging(pce);
            // Check if the page change is cancelled by user
            if (allowModify && pce.Cancel) return;

            // Check if the current page is modified by user
            if (allowModify)
            {
                page = pce.NewPage;
                index = (page == null) ? -1 : Pages.IndexOf(page);

                if (page != null && index == -1)
                    throw new ArgumentException("Page is not found in the page collection.");
                else if (page == null && Pages.Count != 0)
                    throw new ArgumentException("Cannot set SelectedPage to null if the control has at least one page.");
            }

            lastSelectedPage = selectedPage;
            selectedPage = page;
            selectedIndex = index;

            OnUpdateUIControls(new EventArgs());
            UpdatePages();

            if (lastSelectedPage != null)
                OnPageHidden(new PageEventArgs(lastSelectedPage));

            if (selectedPage != null)
                OnPageShown(new PageEventArgs(selectedPage));

            OnPageChanged(new PageChangedEventArgs(lastSelectedPage, selectedPage));
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            OnCreateUIControls(new CreateUIControlsEventArgs());
            OnUpdateUIControls(new EventArgs());
            UpdatePages();
        }

        protected override ControlCollection CreateControlsInstance()
        {
            return new PagedControlControlCollection(this);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);

            if (BorderStyle == BorderStyle.FixedSingle)
                ControlPaint.DrawBorder(e.Graphics, base.ClientRectangle, Color.Black, ButtonBorderStyle.Solid);
            else if (BorderStyle == BorderStyle.Fixed3D)
                ControlPaint.DrawBorder3D(e.Graphics, base.ClientRectangle, Border3DStyle.SunkenOuter);

            base.OnPaint(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdatePages();
            OnUpdateUIControls(new EventArgs());
        }

        protected override void OnInvalidated(InvalidateEventArgs e)
        {
            base.OnInvalidated(e);

            if (SelectedPage != null)
                SelectedPage.Invalidate(e.InvalidRect);
        }
    }
}
