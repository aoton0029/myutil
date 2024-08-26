using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageNavigationSample.Sample1
{
    public static class PageRegistry
    {
        private static readonly Dictionary<PageKey, Type> _pageRegistry = new Dictionary<PageKey, Type>();

        public static void RegisterPage(PageKey key, Type pageType)
        {
            if (!_pageRegistry.ContainsKey(key))
            {
                _pageRegistry.Add(key, pageType);
            }
        }

        public static Type GetPageType(PageKey key)
        {
            if (_pageRegistry.ContainsKey(key))
            {
                return _pageRegistry[key];
            }

            throw new ArgumentException("指定されたページキーは登録されていません。", nameof(key));
        }
    }
}
