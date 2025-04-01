using AstrofluxLauncher.Common;
using AstrofluxLauncher.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstrofluxLauncher.Pages {
    [Page("blank_page", "BlankPage")]
    public class BlankPage : AbstractPage {
        public override async Task Draw(PageDrawer drawer, double dt) {
            Log.TraceLine("This is a blank page.");
        }
    }
}
