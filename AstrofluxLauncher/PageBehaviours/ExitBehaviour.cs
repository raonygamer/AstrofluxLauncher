using AstrofluxLauncher.Common;
using AstrofluxLauncher.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstrofluxLauncher.PageBehaviours {
    public class ExitBehaviour : PageBehaviour {
        public ExitBehaviour(ItemSelector pageSelector) : base(pageSelector) { }

        public override void OnItemSelected(Item item, int index) {
            switch (item.Id) {
                case "yes_item":
                    Program.Instance.ExitGracefully();
                    break;
                case "no_item":
                    Program.Instance.BackSelector();
                    break;
            }
        }

        public static ItemSelector BuildSelector(Program program) {
            return new ItemSelector(program.Input!, "Are you sure you want to exit?", [
                new("yes_item", "Yes", false, false) { DefaultColor = ConsoleColor.Red, SelectedColor = ConsoleColor.Red, CursorOnColor = ConsoleColor.Red },
                new("no_item", "No", false, false)
            ], typeof(ExitBehaviour));
        }
    }
}
