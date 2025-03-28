using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AstrofluxLauncher.Utils {
    public static class Utils {
        public static async Task<bool> DownloadFileAsync(string url, string destination) {
            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode) {
                using var fs = new FileStream(destination, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                await response.Content.CopyToAsync(fs);
            }
            return response.IsSuccessStatusCode;
        }
    }
}
