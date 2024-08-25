using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.ShortcutKeys
{
    public class MacroManager
    {
        private Dictionary<string, ShortcutMacro> macros;

        public MacroManager()
        {
            macros = new Dictionary<string, ShortcutMacro>();
        }

        // マクロの新規登録
        public void RegisterMacro(string name, ShortcutMacro macro)
        {
            if (!macros.ContainsKey(name))
            {
                macros.Add(name, macro);
            }
            else
            {
                throw new InvalidOperationException("同じ名前のマクロが既に存在します。");
            }
        }

        // マクロの削除
        public void DeleteMacro(string name)
        {
            if (macros.ContainsKey(name))
            {
                macros.Remove(name);
            }
            else
            {
                throw new KeyNotFoundException("マクロが見つかりません。");
            }
        }

        // マクロの実行
        public void ExecuteMacro(string name)
        {
            if (macros.ContainsKey(name))
            {
                macros[name].Execute();
            }
            else
            {
                throw new KeyNotFoundException("マクロが見つかりません。");
            }
        }

        // マクロの存在確認
        public bool MacroExists(string name)
        {
            return macros.ContainsKey(name);
        }
    }
}
