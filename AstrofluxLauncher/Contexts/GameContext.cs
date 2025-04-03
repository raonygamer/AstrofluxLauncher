using AstrofluxLauncher.Common;
using AstrofluxLauncher.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AstrofluxLauncher.Contexts {
    public class PatchData {
        public byte[]? AstrofluxSteamData { get; set; }
        public byte[]? AstrofluxDesktopData { get; set; }
    }

    public class GameContext {
        public static readonly string PatchedSteamFileUrl = $"https://github.com/raonygamer/AstrofluxLauncher/raw/refs/heads/{LauncherInfo.Branch}/AstrofluxLoader/artifacts/Astroflux.swf";
        public static readonly string PatchedItchFileUrl = $"https://github.com/raonygamer/AstrofluxLauncher/raw/refs/heads/{LauncherInfo.Branch}/AstrofluxLoader/artifacts/AstrofluxDesktop.swf";
        public static readonly string DefaultCrcFileUrl = $"https://raw.githubusercontent.com/raonygamer/AstrofluxLauncher/refs/heads/{LauncherInfo.Branch}/default_crc.json";
        public const string AstrofluxSteamPathPart = "Steam\\steamapps\\common\\Astroflux\\Astroflux.swf";
        public const string AstrofluxItchPathPart = "Astroflux\\AstrofluxDesktop.swf";
        public static readonly string LegacyLauncherClientPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AstrofluxClients");
        public static readonly string LauncherClientPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData\\Roaming\\AstrofluxLauncher\\Clients");
        public static bool IsSteamVersionInstalled => File.Exists(FormatGamePath(Environment.SpecialFolder.ProgramFilesX86, GameType.Steam)) || File.Exists(FormatGamePath(Environment.SpecialFolder.ProgramFiles, GameType.Steam));
        public static bool IsItchVersionInstalled => File.Exists(FormatGamePath(Environment.SpecialFolder.ProgramFilesX86, GameType.Itch)) || File.Exists(FormatGamePath(Environment.SpecialFolder.ProgramFiles, GameType.Itch));
        public static string? SteamVersionPath {
            get {
                string firstPath = FormatGamePath(Environment.SpecialFolder.ProgramFilesX86, GameType.Steam);
                string secondPath = FormatGamePath(Environment.SpecialFolder.ProgramFiles, GameType.Steam);
                if (!IsSteamVersionInstalled)
                    return null;
                if (File.Exists(firstPath))
                    return firstPath;
                else
                    return secondPath;
            }
        }
        public static string? ItchVersionPath {
            get {
                string firstPath = FormatGamePath(Environment.SpecialFolder.ProgramFilesX86, GameType.Itch);
                string secondPath = FormatGamePath(Environment.SpecialFolder.ProgramFiles, GameType.Itch);
                if (!IsItchVersionInstalled)
                    return null;
                if (File.Exists(firstPath))
                    return firstPath;
                else
                    return secondPath;
            }
        }

        public Launcher Launcher { get; private set; }
        public Dictionary<string, ulong> VanillaChecksums { get; private set; }
        public PatchData PatchData { get; private set; }

        private GameContext(Launcher launcher, Dictionary<string, ulong> vanillaChecksums, PatchData patchData) {
            Launcher = launcher;
            VanillaChecksums = vanillaChecksums;
            PatchData = patchData;
        }

        #region Member Functions
        public GameState GetState(GameType type) {
            switch (type) {
                case GameType.Steam: {
                    if (!IsSteamVersionInstalled)
                        return GameState.NotInstalled;
                    
                    bool isPatched = CRC.Get64(SteamVersionPath!, out ulong hash) &&
                                     VanillaChecksums.TryGetValue("AstrofluxSteam", out ulong vanillaHash) &&
                                     vanillaHash != hash;

                    if (isPatched)
                    {
                        bool isOutdated = PatchData.AstrofluxSteamData is not null && CRC.Get64(PatchData.AstrofluxSteamData, out ulong patchHash) && patchHash != hash;
                        if (isOutdated)
                            return GameState.InstalledPatchedOutdated;
                        return GameState.InstalledPatched;
                    }

                    if (PatchData.AstrofluxSteamData is null)
                        return GameState.InstalledCannotBePatched;

                    return GameState.InstalledCanBePatched;
                }
                case GameType.Itch: {
                    if (!IsItchVersionInstalled)
                        return GameState.NotInstalled;
                    
                    bool isPatched = CRC.Get64(ItchVersionPath!, out ulong hash) &&
                                     VanillaChecksums.TryGetValue("AstrofluxDesktop", out ulong vanillaHash) &&
                                     vanillaHash != hash;
                    
                    if (isPatched)
                    {
                        bool isOutdated = PatchData.AstrofluxDesktopData is not null && CRC.Get64(PatchData.AstrofluxDesktopData, out ulong patchHash) && patchHash != hash;
                        if (isOutdated)
                            return GameState.InstalledPatchedOutdated;
                        return GameState.InstalledPatched;
                    }

                    if (PatchData.AstrofluxDesktopData is null)
                        return GameState.InstalledCannotBePatched;

                    return GameState.InstalledCanBePatched;
                }
                default:
                    return GameState.Unknown;
            }
        }

        public async Task<bool> PatchGameAsync(GameType type)
        {
            switch (type)
            {
                case GameType.Steam:
                {
                    if (GetState(GameType.Steam) is not (GameState.InstalledCanBePatched or GameState.InstalledPatchedOutdated) || PatchData.AstrofluxSteamData is null)
                        return false;
                    var path = SteamVersionPath!;
                    await File.WriteAllBytesAsync(path, PatchData.AstrofluxSteamData);
                    return true;
                }
                case GameType.Itch:
                {
                    if (GetState(GameType.Itch) is not (GameState.InstalledCanBePatched or GameState.InstalledPatchedOutdated) || PatchData.AstrofluxDesktopData is null)
                        return false;
                    var path = ItchVersionPath!;
                    await File.WriteAllBytesAsync(path, PatchData.AstrofluxDesktopData);
                    return true;
                }
                default:
                    return false;
            }
        }
        
        #endregion
        #region Static Functions
        public static string FormatGamePath(Environment.SpecialFolder folder, GameType type) {
            return type switch {
                GameType.Steam => Path.Combine(Environment.GetFolderPath(folder), AstrofluxSteamPathPart),
                GameType.Itch => Path.Combine(Environment.GetFolderPath(folder), AstrofluxItchPathPart),
                _ => "Unknown"
            };
        }

        private static async Task<Dictionary<string, ulong>?> DownloadVanillaChecksums() {
            var jsonString = await Util.DownloadFileToStringAsync(DefaultCrcFileUrl);
            if (jsonString is null)
                return null;
            
            var checksums = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString)?
                .ToDictionary(k => k.Key, k => ulong.Parse(k.Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture));
            return checksums;
        }

        private static async Task<PatchData?> DownloadPatchedFiles() {
            var data = new PatchData();

            // Steam
            {
                using var ms = new MemoryStream();
                if (await Util.DownloadFileToStreamAsync(PatchedSteamFileUrl, ms))
                    data.AstrofluxSteamData = ms.ToArray();
            }

            // Itch.io
            {
                using var ms = new MemoryStream();
                if (await Util.DownloadFileToStreamAsync(PatchedItchFileUrl, ms))
                    data.AstrofluxDesktopData = ms.ToArray();
            }

            return data.AstrofluxSteamData is null && data.AstrofluxDesktopData is null ? null : data;
        }

        public static async Task<GameContext?> CreateAsync(Launcher launcher) {
            var checksums = await DownloadVanillaChecksums() ?? throw new Exception("Failed to download checksums from the repository.");
            var dataOfPatches = await DownloadPatchedFiles() ?? throw new Exception("Failed to download patches from the repository.");
            var context = new GameContext(launcher, checksums, dataOfPatches);
            return context;
        }

        public static GameContext? Create(Launcher launcher) {
            return CreateAsync(launcher).Result;
        }

        public static async Task<string?> GetCurrentGameVersionUrl(
            string mainUrl = "http://r.playerio.com/r/rymdenrunt-k9qmg7cvt0ylialudmldvg/Preload.swf")
        {
            using var client = new HttpClient();
            var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, mainUrl));
            if (response.IsSuccessStatusCode)
            {
                return response.RequestMessage?.RequestUri?.ToString()!;
            }

            return null;
        }

        public static IEnumerable<string> GetAllClientFiles()
        {
            List<string> files = new();
            if (Directory.Exists(LegacyLauncherClientPath))
                files.AddRange(Directory.GetFiles(LegacyLauncherClientPath, "*.swf", SearchOption.AllDirectories));
            if (Directory.Exists(LauncherClientPath))
                files.AddRange(Directory.GetFiles(LauncherClientPath, "*.swf", SearchOption.AllDirectories));
            return files;
        }
        #endregion
    }
}
