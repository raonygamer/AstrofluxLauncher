using AstrofluxInspector.Common;

namespace AstrofluxLauncher
{
    public class Program
    {
        public const string Version = "1.0.0";
        public const string License = "GPLv3";
        public const string Developer = "ryd3v";
        #region Singleton
        private static Program? _Instance = null;
        public static Program Instance {
            get => _Instance ??= new Program();
        }

        public static int Main() {
            return Instance.Start().GetAwaiter().GetResult();
        }
        #endregion

        public async Task<int> Start() {
            string myTemp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString().Replace("-", ""));
            Directory.CreateDirectory(myTemp);

            Log.Trace($"Starting Astroflux Launcher v{Version}...");
            Log.Trace($"This launcher was created by {Developer} with the licence {License}");

            string steamAstrofluxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam", "steamapps", "common", "Astroflux");
            string steamAstrofluxSwf = Path.Combine(steamAstrofluxPath, "Astroflux.swf");
            string itchAstrofluxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Astroflux");
            string itchAstrofluxSwf = Path.Combine(itchAstrofluxPath, "AstrofluxDesktop.swf");

            bool isSteamAstrofluxInstalled = File.Exists(steamAstrofluxSwf);
            bool isItchAstrofluxInstalled = File.Exists(itchAstrofluxSwf);

            if (!isSteamAstrofluxInstalled && !isItchAstrofluxInstalled) {
                Log.Error("Astroflux is not installed on this computer.");
                return 1;
            }

            Log.Trace(isSteamAstrofluxInstalled ? "Astroflux steam is installed at: " + steamAstrofluxPath : "Astroflux steam is not installed.");
            Log.Trace(isItchAstrofluxInstalled ? "Astroflux itch.io is installed at: " + itchAstrofluxPath : "Astroflux itch.io is not installed.");


            Directory.Delete(myTemp, true);
            return 0;
        }
    }
}
