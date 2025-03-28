using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstrofluxLauncher.Utils {
    public static class GameVersion {
        public static bool IsSteamVersionInstalled() {
            return File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + "\\Steam\\steamapps\\common\\Astroflux\\Astroflux.swf");
        }

        public static bool IsItchVersionInstalled() {
            return File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + "\\Astroflux\\AstrofluxDesktop.swf") || File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\Astroflux\\AstrofluxDesktop.swf");
        }

        public static string GetSteamVersionPath() {
            if (!IsSteamVersionInstalled())
                return "Not Installed";
            return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + "\\Steam\\steamapps\\common\\Astroflux\\Astroflux.swf";
        }

        public static string GetItchVersionPath() {
            if (!IsItchVersionInstalled())
                return "Not Installed";
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + "\\Astroflux\\AstrofluxDesktop.swf"))
                return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + "\\Astroflux\\AstrofluxDesktop.swf";
            return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\Astroflux\\AstrofluxDesktop.swf";
        }
    }
}
