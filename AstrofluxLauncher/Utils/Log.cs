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

        public static int CurrentTopPosition {
            get => Console.CursorTop;
        }

        public static void Write(object obj, ConsoleColor color = ConsoleColor.White) {
            var oldColor = Console.ForegroundColor;
            Color = color;
            Console.WriteLine(obj);
            Color = oldColor;
        }

        public static void Clear(int fromTop, int toTop) {
            for (int i = fromTop; i < toTop; i++) {
                Console.SetCursorPosition(0, i);
                Console.Write(new string(' ', Console.WindowWidth));
            }
            Console.SetCursorPosition(0, fromTop);
        }

        public static void Trace(object obj) => Write($"{obj}", ConsoleColor.White);
        public static void Warn(object obj) => Write($"{obj}", ConsoleColor.Yellow);
        public static void Error(object obj) => Write($"{obj}", ConsoleColor.Red);
    }
}
