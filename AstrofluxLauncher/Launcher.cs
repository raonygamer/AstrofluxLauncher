using AstrofluxLauncher.Common;
using AstrofluxLauncher.Contexts;
using AstrofluxLauncher.Models;
using AstrofluxLauncher.UI;
using AstrofluxLauncher.Utils;
using Newtonsoft.Json;
using Semver;
using SharpFileDialog;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Timers;

namespace AstrofluxLauncher
{
    public class Launcher
    {
        #region Singleton
        private static Launcher? _Instance;
        public static Launcher Instance => _Instance ??= new Launcher();

        static int Main(string[] args) {
            return Instance.Start().GetAwaiter().GetResult();
        }
        #endregion
        public const uint RequestedDrawerLayers = 3;

        public Stopwatch Time { get; private set; }
        public List<PageDrawer> LayeredPageDrawers { get; private set; }
        public Input Input { get; private set; }
        public LauncherConfig Config { get; private set; }
        public GameContext GameContext { get; private set; }

        private double LastFrameTime { get; set; }
        private double DeltaTime { get; set; }
        public double LoopTime { get; private set; } = 0.0;

        public int StartDrawLine { get; private set; }
        public int EndDrawLine { get; private set; }

        public bool ShowProfiler { get; private set; } = false;
        public static double RequestedFramesPerSecond = 30d;

        public int LastWindowWidth { get; private set; }
        public int LastWindowHeight { get; private set; }

        public bool RedrawEverything { get; set; } = false;

        public Launcher() {
            Time = Stopwatch.StartNew();

            Console.CursorVisible = false;
            CheckElevation();

            Directory.CreateDirectory(LauncherInfo.TempDirectory);
            Directory.CreateDirectory(LauncherInfo.LauncherDirectory);

            Input = new Input();
            LayeredPageDrawers = [];
            for (var i = 0; i < RequestedDrawerLayers; i++) {
                LayeredPageDrawers.Add(new PageDrawer(this, Input));
            }

            Config = new LauncherConfig();
            var gameCtx = GameContext.Create(this);
            if (gameCtx is null) {
                ExitGracefully();
                GameContext = null!;
                return;
            }

            GameContext = gameCtx!;
            ShowLauncherMessages();
        }

        public void ShowLauncherMessages() {
            Log.TraceLine("Initializing AstrofluxLauncher...");
            Log.TraceLine($"This launcher was created by {LauncherInfo.Author}.");
            Log.TraceLine($"Temporary folder is: {LauncherInfo.TempDirectory}");
            Log.WriteLine();
            Log.TraceLine("Press ESC to go back one page or exit at any time.");
            Log.TraceLine("Navigate using the arrow keys and press ENTER to select.");
            Log.TraceLine("Press P to open performance profiler.");
            Log.WriteLine();
        }

        public async Task<int> Start()
        {
            await CheckUpdates();
            
            LastFrameTime = Time.Elapsed.TotalSeconds;
            DeltaTime = 1.0 / RequestedFramesPerSecond;
            StartDrawLine = Console.CursorTop;

            await (LayeredPageDrawers[1]?.ChangePage("main_page") ?? Task.CompletedTask);
            while (true) {
                await EarlyUpdate();
                await Update();
                await LateUpdate();
            }
        }

        private async Task EarlyUpdate() {
            DeltaTime = Time.Elapsed.TotalSeconds - LastFrameTime;
            LoopTime += DeltaTime;
            LastFrameTime = Time.Elapsed.TotalSeconds;
            await Input.SynchronousPoolInputs();
            if (ShouldRedrawEverything()) {
                Log.ClearEverything();
                ShowLauncherMessages();
                StartDrawLine = Console.CursorTop;
                foreach (var drawer in LayeredPageDrawers) {
                    drawer.EnqueueRedraw();
                }
            }
        }

        private async Task Update() {
            if (Input.GetKeyDown(ConsoleKey.P)) {
                ShowProfiler = !ShowProfiler;
                LayeredPageDrawers[0]?.ChangePage(ShowProfiler ? "profiler_page" : null);
                RedrawEverything = true;
            }

            if (Input.GetKeyDown(ConsoleKey.M))
            {
                Util.WindowIsInFocus();
            }

            AbstractPage? lastAbstractPage = null;
            EndDrawLine = StartDrawLine;
            foreach (var drawer in LayeredPageDrawers) {
                drawer.SetStartY(StartDrawLine + (lastAbstractPage?.CalculatedHeight ?? 0));
                await drawer.Draw(DeltaTime);
                lastAbstractPage = drawer.CurrentPage;
                EndDrawLine += drawer.CurrentPage?.CalculatedHeight ?? 0;
            }
            Console.SetCursorPosition(0, EndDrawLine + 1);
        }

        public bool ShouldRedrawEverything() {
            if (LastWindowWidth == Console.WindowWidth && LastWindowHeight == Console.WindowHeight && !RedrawEverything) 
                return false;
            RedrawEverything = false;
            LastWindowWidth = Console.WindowWidth;
            LastWindowHeight = Console.WindowHeight;
            return true;
        }

        private async Task LateUpdate() {
            if (RequestedFramesPerSecond > 0) {
                Thread.Sleep((int)(1000.0 / RequestedFramesPerSecond) - 15);
            }
        }

        public void ExitGracefully() {
            Log.TraceLine("Exiting gracefully...");
            Thread.Sleep(500);
            Environment.Exit(0);
        }

        public void CheckElevation() {
            if (!Environment.IsPrivilegedProcess) {
                Log.TraceLine("Restarting this program in elevated mode...");
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

        public async Task<bool> CheckUpdates()
        {
            const string releasePart = $"{LauncherInfo.Repository}/releases/latest";
            
            using var client = new HttpClient();
            var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, releasePart));
            if (!response.IsSuccessStatusCode)
                return false;
            
            var redirectionUrl = response.RequestMessage?.RequestUri?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(redirectionUrl))
                return false;

            var releaseVersionString = redirectionUrl.Split('/').Last();
            var releaseVersion = SemVersion.Parse(releaseVersionString);
            var currentVersion = SemVersion.Parse(LauncherInfo.Version);

            if (currentVersion.CompareSortOrderTo(releaseVersion) >= 0)
                return false;

            var zipFile = Path.Combine(LauncherInfo.LauncherDirectory, "UpdateCache/Update.zip");
            var launcherFile = Path.Combine(LauncherInfo.LauncherDirectory, "AstrofluxLauncher.exe");
            if (File.Exists(zipFile))
                File.Delete(zipFile);
            if (!await Util.DownloadFileAsync($"{releasePart}/download/AstrofluxLauncher_{releaseVersionString}.zip", zipFile))
                return false;

            var batchFile = Path.Combine(Environment.CurrentDirectory, "updater_run.cmd");
            await File.WriteAllTextAsync(batchFile, "" +
                $"@echo off\n" +
                $"call \"AstrofluxLauncherUpdater.exe\" \"update\" \"{Environment.ProcessId}\" \"{zipFile}\"\n" +
                $"del \"*.exe\" /f /q\n" +
                $"del \"*.dll\" /f /q\n" +
                $"del \"*.pdb\" /f /q\n" +
                $"xcopy \"{Environment.CurrentDirectory}/update\" \"{Environment.CurrentDirectory}\" /q /c /s /e /y\n" +
                $"del \"{Environment.CurrentDirectory}/update\" /f /q\n" +
                $"echo Astroflux launcher was updated...\n" +
                $"start \"\" \"./AstrofluxLauncher.exe\"\n" +
                $"(goto) 2>nul & del \"%~f0\"\n");
            
            ProcessStartInfo startInfo = new(batchFile) {
                UseShellExecute = true,
                Verb = "runas",
                WorkingDirectory = Environment.CurrentDirectory
            };
            Process.Start(startInfo);
            ExitGracefully();
            return true;
        }
    }
}
