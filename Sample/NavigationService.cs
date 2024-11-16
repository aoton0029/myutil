using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
    internal class NavigationService
    {
        private static object _lockObj = new object();
        private static NavigationService _instance;
        public static NavigationService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObj)
                    {
                        if (_instance == null)
                        {
                            _instance = new NavigationService();
                        }
                    }
                }
                return _instance;
            }
        }

        private Stack<UserControl> _history = new Stack<UserControl>();
        private Control _ParentControl;
        public UserControl CurrentPage { get; set; }


        public void NavigateTo(UserControl uc)
        {
            changePage(uc);
        }

        public void Initialize(Control control)
        {
            _ParentControl = control;
        }

        private void changePage(UserControl uc)
        {
            CurrentPage = uc;
            uc.Dock = DockStyle.Fill;
            _ParentControl.Controls.Clear();
            _ParentControl.Controls.Add(uc);
        }
    }
}
