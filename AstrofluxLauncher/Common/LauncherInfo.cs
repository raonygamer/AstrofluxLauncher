using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstrofluxLauncher.Common {
    public static class LauncherInfo {
        public const string Author = "ryd3v";
        public const string Version = "1.0.4";
        public const string Licence = "GPLv3";
        public const string Repository = "https://github.com/raonygamer/AstrofluxLauncher";
        public const string Branch = "main";
        public const string AstrofluxGameId = "489560";
        public static readonly string TempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString().Replace("-", ""));
        public static readonly string LauncherDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData\\Roaming\\AstrofluxLauncher");
#if DEBUG
        public const bool IsDebug = true;
#else
        public const bool IsDebug = false;
#endif
    }
}
