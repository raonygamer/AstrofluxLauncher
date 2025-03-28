using AstrofluxLauncher.Common;
using AstrofluxLauncher.Models;
using AstrofluxLauncher.PageBehaviours;
using AstrofluxLauncher.UI;
using AstrofluxLauncher.Utils;
using Newtonsoft.Json;
using SharpFileDialog;
using System.Diagnostics;
using System.IO;
using static System.Net.Mime.MediaTypeNames;

namespace AstrofluxLauncher
{
    public class Program
    {
        #region Singleton
        private static Program? _Instance;
        public static Program Instance => _Instance ??= new Program();

        static int Main(string[] args) {
            return Instance.Start().GetAwaiter().GetResult();
        }
        #endregion

        public const string Author = "ryd3v";
        public const string Version = "0.0.1-alpha";
        public const string Licence = "GPLv3";
        public const string Repository = "https://github.com/raonygamer/AstrofluxLauncher";
        public const string Branch = "main";
        public const string AstrofluxGameId = "489560";

        public static readonly string DefaultCrcFileUrl = $"https://raw.githubusercontent.com/raonygamer/AstrofluxLauncher/refs/heads/{Branch}/default_crc.json";
        public static readonly string PatchedItchFileUrl = $"https://github.com/raonygamer/AstrofluxLauncher/raw/refs/heads/{Branch}/Loaders/AstrofluxDesktop/AstrofluxDesktop.swf";
        public static readonly string PatchedSteamFileUrl = $"https://github.com/raonygamer/AstrofluxLauncher/raw/refs/heads/{Branch}/Loaders/AstrofluxSteam/Astroflux.swf";

        public Input? Input { get; private set; }
        public ItemSelector? CurrentSelector { get; private set; }
        public ItemSelector? PreviousSelector { get; private set; }
        public Dictionary<string, ItemSelector> Selectors { get; private set; } = [];

        private int _PrevWindowWidth = 0;
        private int _PrevWindowHeight = 0;

        public int DrawingStart = 0;

        public bool SteamPatchAvailable { get; private set; }
        public bool ItchPatchAvailable { get; private set; }
        public Dictionary<string, string>? DefaultChecksums { get; private set; }

        public string SteamLoaderSwfFile { get; private set; } = "";
        public string ItchLoaderSwfFile { get; private set; } = "";
        public string LauncherConfigPath { get; private set; } = "";
        public Config CurrentConfig { get; private set; } = new Config();

        public static string LegacyLauncherClientPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AstrofluxClients");
        public static string LauncherClientPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData\\Roaming\\AstrofluxLauncher\\Clients");

        public async Task<int> Start() {
            Console.CursorVisible = false;
            CheckElevation();

            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString().Replace("-", ""));
            Directory.CreateDirectory(tempPath);

            string launcherPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData\\Roaming\\AstrofluxLauncher");
            Directory.CreateDirectory(launcherPath);

            LauncherConfigPath = Path.Combine(launcherPath, "config.json");
            if (!File.Exists(LauncherConfigPath)) {
                File.WriteAllText(LauncherConfigPath, JsonConvert.SerializeObject(CurrentConfig, Formatting.Indented));
            }
            LoadConfig();

            SteamLoaderSwfFile = Path.Combine(tempPath, "CustomLoaders\\Steam\\Astroflux.swf");
            string steamLoaderSwfPath = Path.GetDirectoryName(SteamLoaderSwfFile)!;
            Directory.CreateDirectory(steamLoaderSwfPath);
            SteamPatchAvailable = await Utils.Utils.DownloadFileAsync(PatchedSteamFileUrl, SteamLoaderSwfFile);

            ItchLoaderSwfFile = Path.Combine(tempPath, "CustomLoaders\\Itch\\AstrofluxDesktop.swf");
            string itchLoaderSwfPath = Path.GetDirectoryName(ItchLoaderSwfFile)!;
            Directory.CreateDirectory(itchLoaderSwfPath);
            ItchPatchAvailable = await Utils.Utils.DownloadFileAsync(PatchedItchFileUrl, ItchLoaderSwfFile);

            string defaultCrcFile = Path.Combine(tempPath, "default_crc.json");
            if (!await Utils.Utils.DownloadFileAsync(DefaultCrcFileUrl, defaultCrcFile)) {
                Log.Error("Failed to download default_crc.json file from repository.");
                ExitGracefully();
                return 0;
            }

            string defaultCrcJson = File.ReadAllText(defaultCrcFile);
            DefaultChecksums = JsonConvert.DeserializeObject<Dictionary<string, string>>(defaultCrcJson);
            File.Delete(defaultCrcFile);

            StartMessages(tempPath, launcherPath);

            Input = new Input();
            Input.AddOnKeyPressed(OnKeyPressed);

            _PrevWindowWidth = Console.WindowWidth;
            _PrevWindowHeight = Console.WindowHeight;

            DrawingStart = Console.GetCursorPosition().Top;

            BuildSelectors(Input);
            while (true) {
                if (_PrevWindowWidth != Console.WindowWidth || _PrevWindowHeight != Console.WindowHeight) {
                    Console.Clear();
                    StartMessages(tempPath, launcherPath);
                    DrawingStart = Console.GetCursorPosition().Top;

                    if (CurrentSelector != null) {
                        CurrentSelector.NeedsRedraw = true;
                    }
                    _PrevWindowWidth = Console.WindowWidth;
                    _PrevWindowHeight = Console.WindowHeight;
                }

                await Task.Delay((int)(1f / 60f));
                CurrentSelector?.Draw(DrawingStart);
            }
        }

        public Config LoadConfig() {
            CurrentConfig = JsonConvert.DeserializeObject<Config>(File.ReadAllText(LauncherConfigPath)) ?? new Config();
            return CurrentConfig;
        }

        public void SaveConfig() {
            File.WriteAllText(LauncherConfigPath, JsonConvert.SerializeObject(CurrentConfig, Formatting.Indented));
        }

        private void StartMessages(string tempPath, string launcherPath) {
            Log.Trace("Initializing AstrofluxLauncher...");
            Log.Trace($"This launcher was created by {Author}.");
            Log.Trace($"Version: {Version}");
            Log.Trace($"Licence: {Licence}");
            Log.Trace($"Repository: {Repository}");
            Log.Trace($"Temporary folder is: {tempPath}");
            Log.Trace($"Launcher folder is: {launcherPath}");
            Log.Write();
            Log.Trace("Press ESC to go back one page or exit at any time.");
            Log.Trace("Navigate using the arrow keys and press ENTER to select.");
            Log.Write();
        }

        private bool OnKeyPressed(ConsoleKey key) {
            return false;
        }

        public void BuildSelectors(Input input) {
            Selectors.Add("Exit", ExitBehaviour.BuildSelector(this));
            Selectors.Add("SelectGameToWork", SelectGameToWorkBehaviour.BuildSelector(this));
            SwitchSelector(Selectors["SelectGameToWork"]);
        }

        public void BackSelector(bool reset = false, bool goToTop = true) {
            if (CurrentSelector == PreviousSelector)
                return;

            if (PreviousSelector != null) {
                SwitchSelector(PreviousSelector, reset, goToTop);
            }
        }

        public void SwitchSelector(ItemSelector? selector, bool reset = false, bool goToTop = true) {
            Log.ClearVertical(DrawingStart, Console.LargestWindowHeight, false);
            if (CurrentSelector != null) {
                CurrentSelector.OnExit(selector);
                PreviousSelector = CurrentSelector;
            }

            Console.SetCursorPosition(0, goToTop ? 0 : DrawingStart);
            CurrentSelector = selector;
            if (reset) {
                CurrentSelector?.Reset();
            }

            if (CurrentSelector is not null) {
                CurrentSelector.NeedsRedraw = true;
                CurrentSelector.OnEnter(PreviousSelector);
            }
        }

        public bool OnSelector(ItemSelector? selector) {
            return CurrentSelector == selector;
        }

        public void ExitGracefully() {
            Log.ClearVertical(DrawingStart, Log.CurrentCursorYPosition, true);
            Log.Trace("Exiting gracefully...");
            Thread.Sleep(200);
            Environment.Exit(0);
        }

        public void CheckElevation() {
            if (!Environment.IsPrivilegedProcess) {
                Log.Trace("Restarting this program in elevated mode...");
                Thread.Sleep(200);
                var exeName = Process.GetCurrentProcess().MainModule?.FileName;
                ProcessStartInfo startInfo = new(exeName ?? "") {
                    UseShellExecute = true,
                    Verb = "runas"
                };
                Process.Start(startInfo);
                ExitGracefully();
            }
        }
    }
}
