using AstrofluxLauncher.Common;
using AstrofluxLauncher.UI;
using AstrofluxLauncher.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstrofluxLauncher.PageBehaviours {
    public class SelectGameToWorkBehaviour : PageBehaviour {
        public SelectGameToWorkBehaviour(ItemSelector pageSelector) : base(pageSelector) {
        }

        public override void OnItemSelected(Item item, int index) {
            switch (item.Id) {
                case "steam_item":
                    break;
                case "itch_item":
                    break;
                case "refresh_item":
                    Program.Instance.Selectors["SelectGameToWork"] = BuildSelector(Program.Instance);
                    Program.Instance.SwitchSelector(Program.Instance.Selectors["SelectGameToWork"], false);
                    break;
            }
        }

        public static ItemSelector BuildSelector(Program program) {
            return new ItemSelector(program.Input!, "Select game to work on:", [
                new("steam_item", $"Steam Version ({GameVersion.GetSteamVersionPath()})", !GameVersion.IsSteamVersionInstalled(), false),
                new("itch_item", $"Itch.io Version ({GameVersion.GetItchVersionPath()})", !GameVersion.IsItchVersionInstalled(), false),
                new("refresh_item", "Refresh list...", false, false)
            ], typeof(SelectGameToWorkBehaviour));
        }
    }
}
