using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib
{
    public class VersionUtility
    {
        public static Version Format(string s)
        {
            Version version = null;
            try
            {
                version = new Version(s);
            }
            catch (Exception e) { }
            return version;
        }
        public static Version GetVersion(string appFile)
        {
            try
            {
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(appFile);
                return Format(fileVersionInfo.FileVersion);
            }
            catch
            {
                return null;
            }
        }
    }
}
