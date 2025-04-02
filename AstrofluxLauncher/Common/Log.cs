using Gdk;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AstrofluxLauncher.Common {
    public static class Log {
        public struct Colors(ConsoleColor textColor = ConsoleColor.Gray, ConsoleColor backColor = ConsoleColor.Black) {
            public readonly ConsoleColor TextColor = textColor;
            public readonly ConsoleColor BackColor = backColor;
        }

        public struct CursorPosition(int x = 0, int y = 0) {
            public readonly int X = x;
            public readonly int Y = y;
        }

        public static char FillingCharacter = '\0';

        public static void DebugLine(object obj) {
            using var logFile = new StreamWriter(Path.Combine(LauncherInfo.LauncherDirectory, "launcher-log.txt"), true);
            logFile.WriteLine($"[{new StackFrame(1, true).GetMethod()?.Name ?? "Unknown Function"}] [{DateTime.Now:HH:mm:ss}] {obj}");
            logFile.Close();
        }

        public static void Write(object? obj = null, Colors? colors = null, CursorPosition? position = null, bool moveNext = true, bool resetColors = true) {
            colors ??= new(ConsoleColor.Gray, ConsoleColor.Black);
            position ??= new(Console.CursorLeft, Console.CursorTop);
            var shouldChangeColors = Console.ForegroundColor != colors.Value.TextColor || Console.BackgroundColor != colors.Value.BackColor;
            var shouldChangePositions = Console.CursorLeft != position.Value.X || Console.CursorTop != position.Value.Y;

            // Clamp position values
            position = new(Math.Max(0, Math.Min(Console.BufferWidth - 1, position.Value.X)), Math.Max(0, Math.Min(Console.BufferHeight - 1, position.Value.Y)));

            // Save old colors and positions
            var oldTextColor = Console.ForegroundColor;
            var oldBackColor = Console.BackgroundColor;
            var oldX = Console.CursorLeft;
            var oldY = Console.CursorTop;

            // Set new colors
            if (shouldChangeColors) {
                Console.ForegroundColor = colors.Value.TextColor;
                Console.BackgroundColor = colors.Value.BackColor;
            }

            // Set new positions
            if (shouldChangePositions) {
                Console.SetCursorPosition(position.Value.X, position.Value.Y);
            }

            // Write text
            Console.Write(obj?.ToString() ?? "");

            // Reset colors
            if (resetColors && shouldChangeColors) {
                Console.ForegroundColor = oldTextColor;
                Console.BackgroundColor = oldBackColor;
            }

            // Reset position
            if (!moveNext) {
                Console.SetCursorPosition(oldX, oldY);
            }
        }

        public static void WriteLine(object? obj = null, Colors? colors = null, CursorPosition? position = null, bool fill = true, bool moveNext = true, bool resetColors = true) {
            var str = obj?.ToString() ?? "";
            var oldCursorLeft = Console.CursorLeft;
            Write(str, colors, position, moveNext, resetColors);
            if (fill)
                Write(new string(FillingCharacter, Math.Max(0, Console.WindowWidth - oldCursorLeft - str.Length - 1)));
            Write('\n');
        }

        public static void ClearLines(int from, int to, bool setToFrom = false) {
            for (int i = from; i < to; i++) {
                Console.SetCursorPosition(0, i);
                Console.Write(new string(FillingCharacter, Console.WindowWidth));
            }
            Console.Out.Flush();
            if (setToFrom)
                Console.SetCursorPosition(0, from);
            else
                Console.SetCursorPosition(0, 0);
        }

        public static void ClearEverything() {
            Console.Clear();
            Console.SetCursorPosition(0, 0);
        }

        public static void TraceLine(object obj, bool fill = true, bool resetPos = false) => 
            WriteLine(
                $"{obj}", 
                new Colors(ConsoleColor.Gray, ConsoleColor.Black),
                null,
                fill,
                !resetPos,
                true);

        public static void WarnLine(object obj, bool fill = true, bool resetPos = false) => 
            WriteLine(
                $"{obj}",
                new Colors(ConsoleColor.Yellow, ConsoleColor.Black),
                null,
                fill,
                !resetPos,
                true);

        public static void ErrorLine(object obj, bool fill = true, bool resetPos = false) =>
            WriteLine(
                $"{obj}",
                new Colors(ConsoleColor.Red, ConsoleColor.Black),
                null,
                fill,
                !resetPos,
                true);
    }
}
