using AstrofluxLauncher.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;




namespace AstrofluxLauncher.Utils {
    public static partial class Util {
        [LibraryImport("user32.dll")]
        private static partial nint GetForegroundWindow();

        [LibraryImport("kernel32.dll")]
        private static partial nint GetConsoleWindow();

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        public static extern nint FindWindowByCaption(nint zeroOnly, string lpWindowName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool AttachConsole(uint dwProcessId);

        public static bool WindowIsInFocus()
        {
#pragma warning disable CA1416
            return LauncherInfo.IsDebug || 
                GetForegroundWindow() == GetConsoleWindow() ||
                GetForegroundWindow() == Process.GetCurrentProcess().MainWindowHandle ||
                GetForegroundWindow() == FindWindowByCaption(nint.Zero, Console.Title ?? Path.Combine(Environment.CurrentDirectory, "AstrofluxLauncher.exe"));
#pragma warning restore CA1416
        }

        public static async Task<bool> DownloadFileAsync(string url, string destination) {
            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode) {
                if (Path.GetDirectoryName(destination) is string dir && !Directory.Exists(dir)) {
                    Directory.CreateDirectory(dir);
                }
                using var fs = new FileStream(destination, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                await response.Content.CopyToAsync(fs);
                fs.Close();
            }
            return response.IsSuccessStatusCode;
        }

        public static bool DownloadFile(string url, string destination) {
            return DownloadFileAsync(url, destination).Result;
        }

        public static async Task<bool> DownloadFileToStreamAsync(string url, Stream destination) {
            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode) {
                await response.Content.CopyToAsync(destination);
            }
            return response.IsSuccessStatusCode;
        }

        public static bool DownloadFileToStream(string url, Stream destination) {
            return DownloadFileToStreamAsync(url, destination).Result;
        }

        public static async Task<string?> DownloadFileToStringAsync(string url) {
            using var ms = new MemoryStream();
            if (await DownloadFileToStreamAsync(url, ms)) {
                return Encoding.UTF8.GetString(ms.ToArray());
            }
            return null;
        }

        public static string? DownloadFileToString(string url) {
            return DownloadFileToStringAsync(url).Result;
        }
    }
}
