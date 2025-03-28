using AstrofluxLauncher.Common;
using AstrofluxLauncher.UI;
using AstrofluxLauncher.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstrofluxLauncher.PageBehaviours {
    public enum GameType {
        Steam,
        Itch
    }

    public enum GameState {
        NotInstalled,
        InstalledCannotBePatched,
        InstalledCanBePatched,
        InstalledPatched,
        Unknown
    }

    public class SelectGameToWorkBehaviour : PageBehaviour {
        public SelectGameToWorkBehaviour(ItemSelector pageSelector) : base(pageSelector) {
        }

        public override void OnItemSelected(Item item, int index) {
            switch (item.Id) {
                case "steam_item":
                    GameState steamGameState = GetGameState(GameType.Steam);
                    switch (steamGameState) {
                        case GameState.InstalledCanBePatched:
                            Program.Instance.SwitchSelector(ShouldPatchBehaviour.BuildSelector(Program.Instance, GameType.Steam), true);
                            break;
                        case GameState.InstalledPatched:
                            Program.Instance.SwitchSelector(Program.Instance.Selectors["LaunchClient"] = LaunchClientBehaviour.BuildSelector(Program.Instance, GameType.Steam), true);
                            break;
                    }
                    break;
                case "itch_item":
                    GameState itchGameState = GetGameState(GameType.Itch);
                    switch (itchGameState) {
                        case GameState.InstalledCanBePatched:
                            Program.Instance.SwitchSelector(ShouldPatchBehaviour.BuildSelector(Program.Instance, GameType.Itch), true);
                            break;
                        case GameState.InstalledPatched:
                            Program.Instance.SwitchSelector(Program.Instance.Selectors["LaunchClient"] = LaunchClientBehaviour.BuildSelector(Program.Instance, GameType.Itch), true);
                            break;
                    }
                    break;
                case "refresh_item":
                    Program.Instance.Selectors["SelectGameToWork"] = BuildSelector(Program.Instance);
                    Program.Instance.SwitchSelector(Program.Instance.Selectors["SelectGameToWork"], true);
                    break;
            }
        }

        public override bool OnKeyPressed(ConsoleKey key) {
            if (key == ConsoleKey.Escape) {
                Program.Instance.SwitchSelector(Program.Instance.Selectors["Exit"], false);
                return true;
            }
            return false;
        }

        public static GameState GetGameState(GameType type) {
            switch (type) {
                case GameType.Steam: {
                    bool isInstalled = GameVersion.IsSteamVersionInstalled();

                    if (!isInstalled)
                        return GameState.NotInstalled;
                    bool patched = CRC.Get64(GameVersion.GetSteamVersionPath(), out string? hash) &&
                        ulong.TryParse(hash, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong currentNumberHash) &&
                        ulong.TryParse(Program.Instance.DefaultChecksums!["AstrofluxSteam"], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong defaultNumberHash) &&
                        currentNumberHash != defaultNumberHash;

                    if (patched)
                        return GameState.InstalledPatched;

                    if (!Program.Instance.SteamPatchAvailable)
                        return GameState.InstalledCannotBePatched;

                    if (Program.Instance.SteamPatchAvailable && !patched)
                        return GameState.InstalledCanBePatched;
                    return GameState.Unknown;
                }
                case GameType.Itch: {
                    bool isInstalled = GameVersion.IsItchVersionInstalled();

                    if (!isInstalled)
                        return GameState.NotInstalled;
                    bool patched = CRC.Get64(GameVersion.GetItchVersionPath(), out string? hash) &&
                        ulong.TryParse(hash, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong currentNumberHash) &&
                        ulong.TryParse(Program.Instance.DefaultChecksums!["AstrofluxDesktop"], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong defaultNumberHash) &&
                        currentNumberHash != defaultNumberHash;

                    if (patched)
                        return GameState.InstalledPatched;

                    if (!Program.Instance.ItchPatchAvailable)
                        return GameState.InstalledCannotBePatched;

                    if (Program.Instance.ItchPatchAvailable && !patched)
                        return GameState.InstalledCanBePatched;
                    return GameState.Unknown;
                }
            }
            return GameState.Unknown;
        }

        public static ItemSelector BuildSelector(Program program) {
            bool steamInstalled = GameVersion.IsSteamVersionInstalled();
            string steamString = "Steam Version (Not Installed)";
            bool steamEnabled = true;
            ConsoleColor steamDefaultColor = ConsoleColor.White;
            GameState steamGameState = GetGameState(GameType.Steam);
            switch (steamGameState) {
                case GameState.NotInstalled:
                    steamString = "Steam Version (Not Installed)";
                    steamEnabled = false;
                    break;
                case GameState.InstalledCannotBePatched:
                    steamString = "Steam Version (Installed, cannot be patched)";
                    steamEnabled = false;
                    break;
                case GameState.InstalledCanBePatched:
                    steamString = "Steam Version (Installed, can be patched)";
                    steamDefaultColor = ConsoleColor.Gray;
                    break;
                case GameState.InstalledPatched:
                    steamString = "Steam Version (Installed, patched)";
                    steamDefaultColor = ConsoleColor.Green;
                    break;
                case GameState.Unknown:
                    steamString = "Steam Version (Unknown)";
                    steamEnabled = false;
                    break;
            }

            bool itchInstalled = GameVersion.IsItchVersionInstalled();
            string itchString = "Itch.io Version (Not Installed)";
            bool itchEnabled = true;
            ConsoleColor itchDefaultColor = ConsoleColor.White;
            GameState itchGameState = GetGameState(GameType.Itch);
            switch (itchGameState) {
                case GameState.NotInstalled:
                    itchString = "Itch.io Version (Not Installed)";
                    itchEnabled = false;
                    break;
                case GameState.InstalledCannotBePatched:
                    itchString = "Itch.io Version (Installed, cannot be patched)";
                    itchEnabled = false;
                    break;
                case GameState.InstalledCanBePatched:
                    itchString = "Itch.io Version (Installed, can be patched)";
                    itchDefaultColor = ConsoleColor.Gray;
                    break;
                case GameState.InstalledPatched:
                    itchString = "Itch.io Version (Installed, patched)";
                    itchDefaultColor = ConsoleColor.Green;
                    break;
                case GameState.Unknown:
                    itchString = "Itch.io Version (Unknown)";
                    itchEnabled = false;
                    break;
            }

            return new ItemSelector(program.Input!, "Select game to work on:", [
                new("steam_item", steamString, !steamEnabled, false) { DefaultColor = steamDefaultColor, DisabledColor = ConsoleColor.DarkRed },
                new("itch_item", itchString, !itchEnabled, false) { DefaultColor = itchDefaultColor, DisabledColor = ConsoleColor.DarkRed },
                new("refresh_item", "Refresh list...", false, false)
            ], typeof(SelectGameToWorkBehaviour));
        }
    }
}
