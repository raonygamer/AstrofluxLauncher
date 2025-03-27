using DamienG.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstrofluxLauncher.Utils {
    public static class CRC {
        public static bool Get64(string path, out string? hash) {
            hash = null;
            if (!File.Exists(path)) {
                return false;
            }

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                var crc = new Crc64(0xC96C5795D7870F42, 0xFFFFFFFFFFFFFFFF);
                var result = crc.ComputeHash(stream);
                hash = BitConverter.ToString(result).Replace("-", "").ToLower();
                return true;
            }
        }

        public static bool Get64(byte[] bytes, out string? hash) {
            hash = null;
            using (var stream = new MemoryStream(bytes)) {
                var crc = new Crc64(0xC96C5795D7870F42, 0xFFFFFFFFFFFFFFFF);
                var result = crc.ComputeHash(stream);
                hash = BitConverter.ToString(result).Replace("-", "").ToLower();
                return true;
            }
        }
    }
}
