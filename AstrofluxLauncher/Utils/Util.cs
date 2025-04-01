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

        [LibraryImport("user32.dll", SetLastError = true)]
        private static partial int GetWindowThreadProcessId(nint handle, out int processId);

        public static bool WindowIsInFocus()
        {
            return Process.GetCurrentProcess().MainWindowHandle == GetForegroundWindow() || LauncherInfo.IsDebug;
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
