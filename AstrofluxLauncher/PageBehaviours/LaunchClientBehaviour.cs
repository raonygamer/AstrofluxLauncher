using AstrofluxLauncher.Common;
using AstrofluxLauncher.UI;
using AstrofluxLauncher.Utils;
using SharpFileDialog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstrofluxLauncher.PageBehaviours {
    public class LaunchClientBehaviour : PageBehaviour {
        public class ItemData {
            public string Name { get; set; } = "";
            public string Version { get; set; } = "";
            public string FullPath { get; set; } = "";
        }

        public LaunchClientBehaviour(ItemSelector pageSelector) : base(pageSelector) {
        }

        public override void OnPageRender() {
            if (!(PageSelector.Data?.ContainsKey("Type") == true && PageSelector.Data!["Type"].GetType() == typeof(GameType)))
                return;
            GameType game = (GameType)PageSelector.Data!["Type"];
            Log.Trace($"Press L to launch {(game == GameType.Steam ? "Steam" : "Itch.io")} client", true);
            Log.Write("", ConsoleColor.White, true);
        }

        public override bool OnKeyPressed(ConsoleKey key) {
            if (key == ConsoleKey.Escape) {
                Program.Instance.SwitchSelector(Program.Instance.Selectors["SelectGameToWork"] = SelectGameToWorkBehaviour.BuildSelector(Program.Instance), true);
                return true;
            }

            if (!(PageSelector.Data?.ContainsKey("Type") == true && PageSelector.Data!["Type"].GetType() == typeof(GameType)))
                return false;
            GameType game = (GameType)PageSelector.Data!["Type"];

            if (key == ConsoleKey.L) {
                switch (game) {
                    case GameType.Steam:
                        Process.Start(new ProcessStartInfo {
                            FileName = $"steam://rungameid/{Program.AstrofluxGameId}",
                            UseShellExecute = true
                        });
                        return true;
                    case GameType.Itch:
                        Process.Start(new ProcessStartInfo {
                            FileName = Path.Combine(Path.GetDirectoryName(GameVersion.GetItchVersionPath())!, "Astroflux.exe"),
                        });
                        return true;
                }
            }
            return false;
        }

        public override void OnItemSelected(Item item, int index) {
            if (!(item.Data?.ContainsKey("Client") == true && item.Data!["Client"].GetType() == typeof(ItemData)))
                return;
            if (!(PageSelector.Data?.ContainsKey("Type") == true && PageSelector.Data!["Type"].GetType() == typeof(GameType)))
                return;
            GameType game = (GameType)PageSelector.Data!["Type"];
            var data = (ItemData)item.Data!["Client"];
            switch (item.Id) {
                case "new_client":
                    if (NativeFileDialog.OpenDialog([
                        new NativeFileDialog.Filter {
                            Name = "SWF Files",
                            Extensions = ["swf"]
                        }
                    ], null, out string? path) && File.Exists(path)) {
                        Directory.CreateDirectory(Program.LauncherClientPath);
                        File.Copy(path, Path.Combine(Program.LauncherClientPath, Path.GetFileName(path)), true);
                    }
                    Program.Instance.SwitchSelector(Program.Instance.Selectors["LaunchClient"] = BuildSelector(Program.Instance, game), true);
                    break;
                default:
                    Program.Instance.CurrentConfig.CurrentSelectedClientID = item.Id;
                    Program.Instance.CurrentConfig.SwfRemoteUrl = $"file://{data.FullPath.Replace("\\", "/")}";
                    Program.Instance.SaveConfig();
                    break;
            }
        }

        public static ItemSelector BuildSelector(Program program, GameType type) {
            List<string> clientFilepaths = [];
            if (Directory.Exists(Program.LegacyLauncherClientPath)) {
                clientFilepaths.AddRange(Directory.GetFiles(Program.LegacyLauncherClientPath, "*.swf", SearchOption.TopDirectoryOnly));
            }

            if (Directory.Exists(Program.LauncherClientPath)) {
                clientFilepaths.AddRange(Directory.GetFiles(Program.LauncherClientPath, "*.swf", SearchOption.TopDirectoryOnly));
            }

            List<ItemData> clients = clientFilepaths.Select(x => {
                string[] parts = Path.GetFileNameWithoutExtension(x).Split('@');
                if (parts.Length != 2) {
                    return new ItemData {
                        Name = Path.GetFileNameWithoutExtension(x),
                        Version = "Unknown",
                        FullPath = x
                    };
                }
                string name = parts[0];
                string version = parts[1];
                return new ItemData {
                    Name = name,
                    Version = version,
                    FullPath = x
                };
            }).ToList();

            List<Item> items = clients.Select(x => new Item(Path.GetFileNameWithoutExtension(x.FullPath).ToLower(), $"{x.Name} ({(x.Version == "Unknown" ? x.Version : $"v{x.Version}")})", false, true, null, new Dictionary<string, object> {
                { "Client", x }
            })).ToList();

            int currentSelectedClient = -1;
            foreach (var item in items) {
                if (item.Id == program.CurrentConfig.CurrentSelectedClientID) {
                    currentSelectedClient = items.IndexOf(item);
                    break;
                }
            }

            return new ItemSelector(program.Input!, "Select client to launch...", [
                ..items,
                new("new_client", "Import new client", false, false, null, new Dictionary<string, object> {
                    { "Client", new ItemData() }
                })
            ], typeof(LaunchClientBehaviour), new Dictionary<string, object> {
                { "Type", type }
            }, currentSelectedClient);
        }
    }
}
