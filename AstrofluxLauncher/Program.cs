using AstrofluxInspector.Common;
using AstrofluxLauncher.Utils;
using SharpFileDialog;
using System.Diagnostics;
using System.Net;
using System.Numerics;

namespace AstrofluxLauncher
{
    public enum GameType {
        Unknown,
        Steam,
        Itch
    }

    public class Program
    {
        public const string Version = "1.0.0";
        public const string License = "GPLv3";
        public const string Developer = "ryd3v";
        public const string Branch = "dev";
        public const string SteamActionScriptFilename = "AstrofluxSteam.as";
        public const string ItchActionScriptFilename = "AstrofluxDesktop.as";
        public const string VanillaClientUrl = "http://r.playerio.com/r/rymdenrunt-k9qmg7cvt0ylialudmldvg/Preload.swf";
        public const string SteamGameID = "489560";
        public static string ClientsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AstrofluxClients");
        public static int StartConsoleTop = 0;

        #region Singleton
        private static Program? _Instance = null;
        public static Program Instance {
            get => _Instance ??= new Program();
        }

        public static int Main() {
            return Instance.Start().GetAwaiter().GetResult();
        }
        #endregion

        public static string[] GetDownloadedClients() {
            return Directory.GetFiles(ClientsPath, "*.swf", SearchOption.AllDirectories);
        }

        public static async Task<int> AskSelectClient() {
            Log.Clear(StartConsoleTop, Log.CurrentTopPosition);
            var installedClients = GetDownloadedClients();
            Log.Trace("Select a client: ");

            using var request = new HttpClient();
            var result = await request.SendAsync(new HttpRequestMessage(HttpMethod.Get, VanillaClientUrl));
            int indexOffset = 0;
            int nextIndex = 1;
            if (result.IsSuccessStatusCode) {
                Log.Trace($"1. Vanilla Client ({result.RequestMessage?.RequestUri})");
                indexOffset++;
                nextIndex++;
            }

            for (int i = 0; i < installedClients.Length; i++) {
                Log.Trace($"{i + indexOffset + 1}. {Path.GetFileName(installedClients[i])}");
                nextIndex++;
            }

            Log.Trace($"{nextIndex++}. Install a new client");
            Log.Trace($"{nextIndex++}. Go Back");
            int option = 0;
            while (true) {
                Console.Write("Option: ");
                string? input = Console.ReadLine();
                if (input is null || !int.TryParse(input, out option)) {
                    Log.Error("Invalid option.");
                    continue;
                }
                if (option < 1 || option > nextIndex) {
                    Log.Error("Invalid option.");
                    continue;
                }
                break;
            }
            return option;
        }

        public static int AskFirstOption() {
            Log.Clear(StartConsoleTop, Log.CurrentTopPosition);
            Log.Trace("Select an option: ");
            Log.Trace("1. Patch Astroflux");
            Log.Trace("2. Launch Astroflux");
            Log.Trace("3. Exit");
            int firstOption = 0;
            while (true) {
                Console.Write("Option: ");
                string? option = Console.ReadLine();
                if (option is null || !int.TryParse(option, out firstOption)) {
                    Log.Error("Invalid option.");
                    continue;
                }

                if (firstOption < 1 || firstOption > 3) {
                    Log.Error("Invalid option.");
                    continue;
                }

                break;
            }
            return firstOption;
        }

        public static int AskToSelectGameType(bool steamInstalled, bool itchInstalled) {
            Log.Clear(StartConsoleTop, Log.CurrentTopPosition);
            Log.Trace("Select the game type: ");
            if (steamInstalled) {
                Log.Trace("1. Steam");
            }
            else {
                Log.Write("1. Steam (Not Installed)", ConsoleColor.DarkGray);
            }

            if (itchInstalled) {
                Log.Trace("2. Itch.io");
            }
            else {
                Log.Write("2. Itch.io (Not Installed)", ConsoleColor.DarkGray);
            }
            Log.Trace("3. Go Back");
            int optInt = 0;
            while (true) {
                Console.Write("Option: ");
                string? option = Console.ReadLine();
                if (option is null || !int.TryParse(option, out optInt)) {
                    Log.Error("Invalid option.");
                    continue;
                }

                if (optInt < 1 || optInt > 3) {
                    Log.Error("Invalid option.");
                    continue;
                }

                if ((optInt == 1 && !steamInstalled) || (optInt == 2 && !itchInstalled)) {
                    Log.Error("Invalid option.");
                    continue;
                }
                break;
            }
            return optInt;
        }

        public async Task<int> Start() {
            if (!Environment.IsPrivilegedProcess) {
                Log.Error("This program must be run as an administrator.");
                return 1;
            }

            Directory.CreateDirectory(ClientsPath);
            string myTemp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString().Replace("-", ""));
            Directory.CreateDirectory(myTemp);

            Log.Trace($"Starting Astroflux Launcher v{Version}...");
            Log.Trace($"This launcher was created by {Developer} with the licence {License}");
            Log.Trace($"Temporary directory is: {myTemp}");
            Console.WriteLine();
            StartConsoleTop = Console.GetCursorPosition().Top;

            string steamAstrofluxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam", "steamapps", "common", "Astroflux");
            string steamAstrofluxSwf = Path.Combine(steamAstrofluxPath, "Astroflux.swf");
            string itchAstrofluxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Astroflux");
            if (!Directory.Exists(itchAstrofluxPath))
                itchAstrofluxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Astroflux");
            string itchAstrofluxSwf = Path.Combine(itchAstrofluxPath, "AstrofluxDesktop.swf");
            string itchAstrofluxExe = Path.Combine(itchAstrofluxPath, "Astroflux.exe");

            bool isSteamAstrofluxInstalled = File.Exists(steamAstrofluxSwf);
            bool isItchAstrofluxInstalled = File.Exists(itchAstrofluxSwf);
            bool canPatchSteam = isSteamAstrofluxInstalled;
            bool canPatchItch = isItchAstrofluxInstalled;

            if (!isSteamAstrofluxInstalled && !isItchAstrofluxInstalled) {
                Log.Error("Astroflux is not installed on this computer.");
                return 1;
            }

            Log.Trace(isSteamAstrofluxInstalled ? "Astroflux steam is installed at: " + steamAstrofluxPath : "Astroflux steam is not installed.");
            Log.Trace(isItchAstrofluxInstalled ? "Astroflux itch.io is installed at: " + itchAstrofluxPath : "Astroflux itch.io is not installed.");
            Console.WriteLine();
            Log.Trace("Downloading the template action script files from the repository...");
            string templatesDirectory = Path.Combine(myTemp, "templates");
            Directory.CreateDirectory(templatesDirectory);

            string steamActionScriptUrl = $"https://raw.githubusercontent.com/raonygamer/AstrofluxLauncher/refs/heads/{Branch}/templates/{SteamActionScriptFilename}";
            string steamActionScriptTemplateFile = Path.Combine(templatesDirectory, SteamActionScriptFilename);
            string itchActionScriptUrl = $"https://raw.githubusercontent.com/raonygamer/AstrofluxLauncher/refs/heads/{Branch}/templates/{ItchActionScriptFilename}";
            string itchActionScriptTemplateFile = Path.Combine(templatesDirectory, ItchActionScriptFilename);

            using var httpClient = new HttpClient();
            {
                Log.Trace($"Downloading {SteamActionScriptFilename} from {steamActionScriptUrl}.");
                var result = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, steamActionScriptUrl));
                if (!result.IsSuccessStatusCode) {
                    Log.Error($"Failed to download {SteamActionScriptFilename} from {steamActionScriptUrl}.");
                    canPatchSteam = false;
                }

                var templateFileContents = await result.Content.ReadAsStringAsync();
                await File.WriteAllTextAsync(steamActionScriptTemplateFile, templateFileContents);
            }

            {
                Log.Trace($"Downloading {ItchActionScriptFilename} from {itchActionScriptUrl}.");
                var result = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, itchActionScriptUrl));
                if (!result.IsSuccessStatusCode) {
                    Log.Error($"Failed to download {ItchActionScriptFilename} from {itchActionScriptUrl}.");
                    canPatchItch = false;
                }

                var templateFileContents = await result.Content.ReadAsStringAsync();
                await File.WriteAllTextAsync(itchActionScriptTemplateFile, templateFileContents);
            }

            Log.Clear(StartConsoleTop, Log.CurrentTopPosition);
            StartConsoleTop = Console.GetCursorPosition().Top;

        startingOption:
            int firstOption = AskFirstOption();
            switch (firstOption) {
                case 1: {
                askSelectGame:
                    var selectGameOption = AskToSelectGameType(canPatchSteam, canPatchItch);
                    if (selectGameOption == 3) {
                        goto startingOption;
                    }

                    GameType gameType = selectGameOption switch {
                        1 => GameType.Steam,
                        2 => GameType.Itch,
                        _ => GameType.Unknown
                    };

                    string astrofluxSwf = gameType switch {
                        GameType.Steam => steamAstrofluxSwf,
                        GameType.Itch => itchAstrofluxSwf,
                        _ => string.Empty
                    };

                    string templateActionScriptFilename = gameType switch {
                        GameType.Steam => SteamActionScriptFilename,
                        GameType.Itch => ItchActionScriptFilename,
                        _ => string.Empty
                    };

                    string templateActionScriptText = gameType switch {
                        GameType.Steam => await File.ReadAllTextAsync(steamActionScriptTemplateFile),
                        GameType.Itch => await File.ReadAllTextAsync(itchActionScriptTemplateFile),
                        _ => string.Empty
                    };

                askSelectClient:
                    int selected = await AskSelectClient();
                    var installedClients = GetDownloadedClients();
                    int newClientIndex = installedClients.Length + 2;

                    if (selected == 1) {
                        // Vanilla Client
                        Log.Clear(StartConsoleTop, Log.CurrentTopPosition);
                        Log.Trace("Starting patching process...");
                        Log.Trace("Replacing @PRELOAD_SWF_URL@ with the client specified location...");
                        string newActionScriptContent = templateActionScriptText.Replace("@PRELOAD_SWF_URL@", VanillaClientUrl);
                        string newActionScriptFile = Path.Combine(myTemp, "AstrofluxPatched.as");
                        string tempSwf = Path.Combine(myTemp, "Temp" + Guid.NewGuid().ToString().Replace("-", "") + ".swf");
                        File.Copy(astrofluxSwf, tempSwf, true);
                        await File.WriteAllTextAsync(newActionScriptFile, newActionScriptContent);
                        var ffdecProcess = FFDec.Invoke("-replace", tempSwf, tempSwf, Path.GetFileNameWithoutExtension(templateActionScriptFilename), newActionScriptFile);
                        while (!ffdecProcess.HasExited) {
                            Log.Trace((await ffdecProcess.StandardOutput.ReadLineAsync()) ?? "");
                        }
                        File.Copy(tempSwf, astrofluxSwf, true);
                        File.Delete(tempSwf);

                        if (ffdecProcess.ExitCode != 0) {
                            Log.Error("Failed to patch the client.");
                            Log.Error(ffdecProcess.StandardError.ReadToEnd());
                        }
                        else {
                            Log.Write("Patching process completed.", ConsoleColor.Green);
                        }

                        await Task.Delay(2000);
                        goto startingOption;
                    }
                    else if (selected == newClientIndex) {
                        // New client
                        if (!NativeFileDialog.OpenDialog([
                            new NativeFileDialog.Filter() {
                                Name = "SWF File",
                                Extensions = ["swf"]
                            }
                        ], null, out string? newClientSwf) || !File.Exists(newClientSwf)) {
                            goto askSelectClient;
                        }

                        string newClientSwfPath = Path.Combine(ClientsPath, Path.GetFileName(newClientSwf));
                        File.Copy(newClientSwf, newClientSwfPath, true);
                        Log.Clear(StartConsoleTop, Log.CurrentTopPosition);
                        Log.Trace("Starting patching process...");
                        Log.Trace("Replacing @PRELOAD_SWF_URL@ with the client specified location...");
                        string newActionScriptContent = templateActionScriptText.Replace("@PRELOAD_SWF_URL@", $"file://{newClientSwfPath.Replace("\\", "/")}");
                        string newActionScriptFile = Path.Combine(myTemp, "AstrofluxPatched.as");
                        string tempSwf = Path.Combine(myTemp, "Temp" + Guid.NewGuid().ToString().Replace("-", "") + ".swf");
                        File.Copy(astrofluxSwf, tempSwf, true);
                        await File.WriteAllTextAsync(newActionScriptFile, newActionScriptContent);
                        var ffdecProcess = FFDec.Invoke("-replace", tempSwf, tempSwf, Path.GetFileNameWithoutExtension(templateActionScriptFilename), newActionScriptFile);
                        while (!ffdecProcess.HasExited) {
                            Log.Trace((await ffdecProcess.StandardOutput.ReadLineAsync()) ?? "");
                        }
                        File.Copy(tempSwf, astrofluxSwf, true);
                        File.Delete(tempSwf);

                        if (ffdecProcess.ExitCode != 0) {
                            Log.Error("Failed to patch the client.");
                            Log.Error(ffdecProcess.StandardError.ReadToEnd());
                        }
                        else {
                            Log.Write("Patching process completed.", ConsoleColor.Green);
                        }

                        await Task.Delay(2000);
                        goto startingOption;
                    }
                    else if (selected != newClientIndex + 1) {
                        // Existing client
                        Log.Clear(StartConsoleTop, Log.CurrentTopPosition);
                        Log.Trace("Starting patching process...");
                        Log.Trace("Replacing @PRELOAD_SWF_URL@ with the client specified location...");
                        string newActionScriptContent = templateActionScriptText.Replace("@PRELOAD_SWF_URL@", $"file://{installedClients[selected - 2].Replace("\\", "/")}");
                        string newActionScriptFile = Path.Combine(myTemp, "AstrofluxPatched.as");
                        string tempSwf = Path.Combine(myTemp, "Temp" + Guid.NewGuid().ToString().Replace("-", "") + ".swf");
                        File.Copy(astrofluxSwf, tempSwf, true);
                        await File.WriteAllTextAsync(newActionScriptFile, newActionScriptContent);
                        var ffdecProcess = FFDec.Invoke("-replace", tempSwf, tempSwf, Path.GetFileNameWithoutExtension(templateActionScriptFilename), newActionScriptFile);
                        while (!ffdecProcess.HasExited) {
                            Log.Trace((await ffdecProcess.StandardOutput.ReadLineAsync()) ?? "");
                        }
                        File.Copy(tempSwf, astrofluxSwf, true);
                        File.Delete(tempSwf);

                        if (ffdecProcess.ExitCode != 0) {
                            Log.Error("Failed to patch the client.");
                            Log.Error(ffdecProcess.StandardError.ReadToEnd());
                        }
                        else {
                            Log.Write("Patching process completed.", ConsoleColor.Green);
                        }

                        await Task.Delay(2000);
                        goto startingOption;
                    }
                    else {
                        goto askSelectGame;
                    }
                }
                case 2: {
                askSelectGame:
                    var selectGameOption = AskToSelectGameType(canPatchSteam, canPatchItch);
                    if (selectGameOption == 3) {
                        goto startingOption;
                    }

                    GameType gameType = selectGameOption switch {
                        1 => GameType.Steam,
                        2 => GameType.Itch,
                        _ => GameType.Unknown
                    };

                    if (gameType == GameType.Steam) {
                        Process.Start(new ProcessStartInfo {
                            FileName = "steam://rungameid/" + SteamGameID,
                            UseShellExecute = true
                        });
                        goto startingOption;
                    }
                    else if (gameType == GameType.Itch) {
                        Process.Start(itchAstrofluxExe);
                        goto startingOption;
                    }
                    else {
                        goto askSelectGame;
                    }
                }
                case 3:
                    break;
                default:
                    Log.Error("Invalid option.");
                    break;
            }

            Directory.Delete(myTemp, true);
            return 0;
        }
    }
}
