using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstrofluxInspector.Common {
    public static class Log {
        public static ConsoleColor Color {
            get => Console.ForegroundColor;
            set => Console.ForegroundColor = value;
        }

        public static void Write(object obj, ConsoleColor color = ConsoleColor.White) {
            var oldColor = Console.ForegroundColor;
            Color = color;
            Console.WriteLine(obj);
            Color = oldColor;
        }

        public static void Trace(object obj) => Write($"{obj}", ConsoleColor.White);
        public static void Warn(object obj) => Write($"{obj}", ConsoleColor.Yellow);
        public static void Error(object obj) => Write($"{obj}", ConsoleColor.Red);
    }
}
