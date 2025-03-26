using AstrofluxInspector.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstrofluxLauncher.Utils {
    public static class FFDec {
        public static Process Invoke(params string[] args) {
            var process = new Process {
                StartInfo = {
                    FileName = "./ffdec/ffdec-cli.exe",
                    Arguments = string.Join(" ", args),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            process.Start();
            return process;
        }
    }
}
