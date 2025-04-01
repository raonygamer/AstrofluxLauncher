using DamienG.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstrofluxLauncher.Utils {
    public static class CRC {
        public static bool Get64(string path, out ulong hash) {
            hash = 0x0;
            if (!File.Exists(path)) {
                return false;
            }

            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            var crc = new Crc64(0xC96C5795D7870F42, 0xFFFFFFFFFFFFFFFF);
            var result = crc.ComputeHash(stream);
            var stringHash = BitConverter.ToString(result).Replace("-", "").ToLower();
            if (!ulong.TryParse(stringHash, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out hash)) {
                return false;
            }
            return true;
        }

        public static bool Get64(byte[] bytes, out ulong hash) {
            hash = 0x0;
            using var stream = new MemoryStream(bytes);
            var crc = new Crc64(0xC96C5795D7870F42, 0xFFFFFFFFFFFFFFFF);
            var result = crc.ComputeHash(stream);
            var stringHash = BitConverter.ToString(result).Replace("-", "").ToLower();
            if (!ulong.TryParse(stringHash, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out hash)) {
                return false;
            }
            return true;
        }
    }
}
