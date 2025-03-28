using AstrofluxLauncher.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstrofluxLauncher.PageBehaviours {
    public class PatchingGameBehaviour : PageBehaviour {
        public PatchingGameBehaviour(ItemSelector pageSelector) : base(pageSelector) {
        }

        public override bool OnKeyPressed(ConsoleKey key) {
            return true;
        }

        public static ItemSelector BuildSelector(Program program, string type) {
            return new ItemSelector(program.Input!, $"Patching {type} version...", [new()], typeof(PatchingGameBehaviour));
        }
    }
}
