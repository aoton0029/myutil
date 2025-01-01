using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageNavigationSample
{
    public class PageManager
    {
        private readonly Stack<UserControl> _screenStack = new();

        public void NavigateTo(UserControl nextScreen)
        {
            _screenStack.Push(nextScreen);
            ShowScreen(nextScreen);
        }

        public void GoBack()
        {
            if (_screenStack.Count > 1)
            {
                _screenStack.Pop();
                ShowScreen(_screenStack.Peek());
            }
            else
            {
                Console.WriteLine("No previous screen to navigate to.");
            }
        }

        private void ShowScreen(UserControl screen)
        {
            Console.WriteLine($"Showing screen: {screen.GetType().Name}");
        }
    }
}
