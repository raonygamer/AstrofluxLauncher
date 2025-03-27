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
        public const string Branch = "dev";

        public Input? Input { get; private set; }
        public ItemSelector? CurrentSelector { get; private set; }
        public ItemSelector? PreviousSelector { get; private set; }
        public Dictionary<string, ItemSelector> Selectors { get; private set; } = [];

        private int _PrevWindowWidth = 0;
        private int _PrevWindowHeight = 0;

        public int DrawingStart = 0;

        public async Task<int> Start() {
            Console.CursorVisible = false;
            CheckElevation();

            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString().Replace("-", ""));
            Directory.CreateDirectory(tempPath);

            string launcherPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData\\Roaming\\AstrofluxLauncher");
            Directory.CreateDirectory(launcherPath);

            string launcherConfigPath = Path.Combine(launcherPath, "config.json");
            if (!File.Exists(launcherConfigPath)) {
                File.WriteAllText(launcherConfigPath, JsonConvert.SerializeObject(new Config(), Formatting.Indented));
            }

            Log.Trace("Initializing AstrofluxLauncher...");
            Log.Trace($"This launcher was created by {Author}.");
            Log.Trace($"Version: {Version}");
            Log.Trace($"Licence: {Licence}");
            Log.Trace($"Repository: {Repository}");
            Log.Write();
            Log.Trace("Press ESC to go back one page or exit at any time.");
            Log.Trace("Navigate using the arrow keys and press ENTER to select.");
            Log.Write();
            Log.Trace($"Temporary folder is: {tempPath}");
            Log.Trace($"Launcher folder is: {launcherPath}");
            Log.Write();

            Input = new Input();
            Input.AddOnKeyPressed(OnKeyPressed);

            _PrevWindowWidth = Console.WindowWidth;
            _PrevWindowHeight = Console.WindowHeight;

            DrawingStart = Console.GetCursorPosition().Top;

            BuildSelectors(Input);
            while (true) {
                if (_PrevWindowWidth != Console.WindowWidth || _PrevWindowHeight != Console.WindowHeight) {
                    Log.ClearVertical(DrawingStart, Log.CurrentCursorYPosition);
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

        private bool OnKeyPressed(ConsoleKey key) {
            if (key == ConsoleKey.Escape) {
                SwitchSelector(Selectors["Exit"], false);
                return true;
            }
            return false;
        }

        public void BuildSelectors(Input input) {
            Selectors.Add("Exit", ExitBehaviour.BuildSelector(this));
            Selectors.Add("SelectGameToWork", SelectGameToWorkBehaviour.BuildSelector(this));
            SwitchSelector(Selectors["SelectGameToWork"]);
        }

        public void BackSelector() {
            if (CurrentSelector == PreviousSelector)
                return;

            if (PreviousSelector != null) {
                SwitchSelector(PreviousSelector, true);
            }
        }

        public void SwitchSelector(ItemSelector? selector, bool reset = false, bool goToTop = true) {
            Log.ClearVertical(DrawingStart, Console.LargestWindowHeight);
            if (CurrentSelector != null) {
                CurrentSelector.PageBehaviour?.OnPageExit(selector);
                PreviousSelector = CurrentSelector;
            }

            CurrentSelector = selector;
            if (reset) {
                CurrentSelector?.Reset();
            }

            if (CurrentSelector is not null) {
                CurrentSelector.NeedsRedraw = true;
                CurrentSelector.PageBehaviour?.OnPageEnter(PreviousSelector);
            }
            Console.SetCursorPosition(0, goToTop ? 0 : DrawingStart);
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
