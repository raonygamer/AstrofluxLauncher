using AstrofluxLauncher.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstrofluxLauncher.PageBehaviours {
    public class ShouldPatchBehaviour : PageBehaviour {
        public ShouldPatchBehaviour(ItemSelector pageSelector) : base(pageSelector) { }

        public override void OnItemSelected(Item item, int index) {
            switch (item.Id) {
                case "yes_item":
                    
                    break;
                case "no_item":
                    
                    break;
            }
        }

        public static ItemSelector BuildSelector(Program program) {
            return new ItemSelector(program.Input!, "Seems like your game wasn't patched yet, do you want to patch it now?", [
                new("yes_item", "Yes", false, false) { DefaultColor = ConsoleColor.Green, SelectedColor = ConsoleColor.Green, CursorOnColor = ConsoleColor.Green },
                new("no_item", "No", false, false) { DefaultColor = ConsoleColor.Red, SelectedColor = ConsoleColor.Red, CursorOnColor = ConsoleColor.Red }
            ], typeof(ShouldPatchBehaviour));
        }
    }
}
