using AstrofluxLauncher.Common;
using AstrofluxLauncher.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstrofluxLauncher.Utils {
    public class LauncherConfig {
        public static readonly string LauncherConfigPath = Path.Combine(LauncherInfo.LauncherDirectory, "config.json");

        public Config Config { get; private set; } = new Config();

        public LauncherConfig() {
            if (!File.Exists(LauncherConfigPath)) {
                Save();
            }
            Load();
        }

        public void Load() {
            Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(LauncherConfigPath)) ?? new Config();
        }

        public void Save() {
            File.WriteAllText(LauncherConfigPath, JsonConvert.SerializeObject(Config, Formatting.Indented));
        }
    }
}
