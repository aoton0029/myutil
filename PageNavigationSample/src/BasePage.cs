using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageNavigationSample
{
    public class BasePage : UserControl
    {
        protected readonly SharedStateManager<SharedData> Mediator;
        protected readonly PageManager ScreenManager;

        protected BasePage(SharedStateManager<SharedData> mediator, PageManager screenManager)
        {
            Mediator = mediator;
            ScreenManager = screenManager;
        }

        public virtual void Display()
        {

        }

        public void GoToNextScreen(UserControl nextScreen)
        {
            ScreenManager?.NavigateTo(nextScreen);
        }

        public void GoToPreviousScreen()
        {
            ScreenManager?.GoBack();
        }
    }

}
