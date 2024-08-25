using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.ShortcutKeys
{
    public class ShortcutMacro
    {
        public string Name { get; set; }
        public List<Keys> ShortcutKeys { get; set; }
        public List<Action> Actions { get; set; }

        public ShortcutMacro(string name)
        {
            Name = name;
            ShortcutKeys = new List<Keys>();
            Actions = new List<Action>();
        }

        public void AddStep(Keys shortcutKey, Action action)
        {
            ShortcutKeys.Add(shortcutKey);
            Actions.Add(action);
        }

        public void Execute()
        {
            for (int i = 0; i < Actions.Count; i++)
            {
                Actions[i]?.Invoke();
            }
        }
    }
}
