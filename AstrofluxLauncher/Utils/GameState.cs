using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstrofluxLauncher.Utils {
    public enum GameState {
        NotInstalled,
        InstalledCannotBePatched,
        InstalledCanBePatched,
        InstalledPatched,
        InstalledPatchedOutdated,
        Unknown
    }
}
