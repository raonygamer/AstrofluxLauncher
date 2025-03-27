using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstrofluxLauncher.Common {
    public static class Log {
        public static ConsoleColor Color {
            get => Console.ForegroundColor;
            set => Console.ForegroundColor = value;
        }

        public static int CurrentCursorYPosition {
            get => Console.GetCursorPosition().Top;
            set => Console.SetCursorPosition(Console.CursorLeft, value);
        }

        public static int MaxYDrawed = 0;

        public static void Write(object? obj = null, ConsoleColor color = ConsoleColor.White, bool spaced = false) {
            if (obj is null) {
                Console.WriteLine();
                MaxYDrawed = Math.Max(MaxYDrawed, Console.GetCursorPosition().Top);
                return;
            }

            var oldColor = Console.ForegroundColor;
            Color = color;
            string str = obj?.ToString() ?? "";
            if (spaced) {
                str += new string(' ', Math.Max(0, Console.WindowWidth - str.Length - 2));
            }
            Console.WriteLine(str);
            MaxYDrawed = Math.Max(MaxYDrawed, Console.GetCursorPosition().Top);
            Color = oldColor;
        }

        public static void ClearVertical(int from, int to, bool setToFrom = false) {
            for (int i = from; i < to; i++) {
                Console.SetCursorPosition(0, i);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, i);
            }
            Console.Out.Flush();
            Console.Error.Flush();
            if (setToFrom)
                Console.SetCursorPosition(0, from);
        }

        public static void Trace(object obj, bool spaced = false) => Write($"{obj}", ConsoleColor.White, spaced);
        public static void Warn(object obj, bool spaced = false) => Write($"{obj}", ConsoleColor.Yellow, spaced);
        public static void Error(object obj, bool spaced = false) => Write($"{obj}", ConsoleColor.Red, spaced);
    }
}
