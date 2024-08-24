using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static PageNavigationSample.Sample1.NavigationService;

namespace PageNavigationSample.Sample1
{
    public partial class Form1 : Form
    {
        private MainController _controller;

        public MainForm()
        {
            InitializeComponent();

            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(mainPanel);

            _controller = new MainController(mainPanel, NavigationMode.UseExistingInstance);

            _controller.RegisterPage(PageKey.Home, typeof(ParentUserControl));
            _controller.RegisterPage(PageKey.Child1, typeof(ChildUserControl));
            _controller.RegisterPage(PageKey.Child2, typeof(ChildUserControl));

            _controller.NavigateTo(PageKey.Home);
        }

        private void OnNavigateToChild1ButtonClick(object sender, EventArgs e)
        {
            _controller.UpdateSharedData("Child1のデータ");
            _controller.NavigateTo(PageKey.Child1);
        }

        private void OnNavigateToChild2ButtonClick(object sender, EventArgs e)
        {
            _controller.UpdateSharedData("Child2のデータ");
            _controller.NavigateTo(PageKey.Child2, NavigationMode.CreateNewInstance);
        }

        private void OnBackButtonClick(object sender, EventArgs e)
        {
            _controller.GoBack();
        }
    }
}
