using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace ProjectSample
{
    public static class LocalizedStrings
    {
        private static readonly ResourceManager _resourceManager =
            new ResourceManager("YourApp.Resources.Strings", Assembly.GetExecutingAssembly());

        public static string Get(string key)
        {
            return _resourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? $"!{key}!";
        }

        public static void SetCulture(string cultureCode)
        {
            CultureInfo culture = new CultureInfo(cultureCode);
            Thread.CurrentThread.CurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
        }
    }
}
