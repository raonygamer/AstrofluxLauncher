using AstrofluxLauncher.Common;
using AstrofluxLauncher.UI;
using AstrofluxLauncher.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstrofluxLauncher.PageBehaviours {
    public class ShouldPatchBehaviour : PageBehaviour {
        public ShouldPatchBehaviour(ItemSelector pageSelector) : base(pageSelector) { }

        public override void OnPageEnter(ItemSelector? prevPage) {
            
        }

        public override void OnItemSelected(Item item, int index) {
            if (!(PageSelector.Data?.ContainsKey("Type") == true && PageSelector.Data!["Type"].GetType() == typeof(GameType)))
                return;
            GameType game = (GameType)PageSelector.Data!["Type"];
            switch (item.Id) {
                case "yes_item":
                    Program.Instance.SwitchSelector(PatchingGameBehaviour.BuildSelector(Program.Instance, ((GameType)PageSelector.Data!["Type"]) == GameType.Steam ? "Steam" : "Itch.io"), true);
                    switch (game) {
                        case GameType.Steam:
                            File.Copy(Program.Instance.SteamLoaderSwfFile, GameVersion.GetSteamVersionPath(), true);
                            break;
                        case GameType.Itch:
                            File.Copy(Program.Instance.ItchLoaderSwfFile, GameVersion.GetItchVersionPath(), true);
                            break;
                    }
                    Thread.Sleep(300);
                    Program.Instance.SwitchSelector(Program.Instance.Selectors["LaunchClient"] = LaunchClientBehaviour.BuildSelector(Program.Instance, game), true);
                    break;
                case "no_item":
                    Program.Instance.SwitchSelector(Program.Instance.Selectors["SelectGameToWork"], false);
                    break;
            }
        }

        public static ItemSelector BuildSelector(Program program, GameType type) {
            return new ItemSelector(program.Input!, $"Seems like the {(type == GameType.Steam ? "Steam" : "Itch.io")} version wasn't patched yet, do you want to patch it now?", [
                new("yes_item", "Yes", false, false) { DefaultColor = ConsoleColor.Green, SelectedColor = ConsoleColor.Green, CursorOnColor = ConsoleColor.Green },
                new("no_item", "No", false, false) { DefaultColor = ConsoleColor.Red, SelectedColor = ConsoleColor.Red, CursorOnColor = ConsoleColor.Red }
            ], typeof(ShouldPatchBehaviour), new Dictionary<string, object> {
                { "Type", type }
            });
        }
    }
}
