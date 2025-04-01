using AstrofluxLauncher.Common;
using AstrofluxLauncher.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstrofluxLauncher.Pages {
    [Page("profiler_page", "ProfilerPage", true)]
    public class ProfilerPage : AbstractPage {
        public double ProfilerTime { get; protected set; } = 0.0;
        public double NextRedrawTime { get; protected set; } = 0.0;
        public double RedrawInterval { get; protected set; } = 0.5;

        public override async Task Draw(PageDrawer drawer, double dt) {
            await base.Draw(drawer, dt);
            var fps = 1.0 / dt;
            var color = fps >= Launcher.RequestedFramesPerSecond ? ConsoleColor.DarkGreen : (fps < Launcher.RequestedFramesPerSecond ? ConsoleColor.DarkYellow : (fps < Launcher.RequestedFramesPerSecond / 2.0 ? ConsoleColor.Red : ConsoleColor.Black));
            Log.Write($"FPS: ");
            Log.WriteLine(fps.ToString("0.0", CultureInfo.InvariantCulture), new Log.Colors(ConsoleColor.White, color));
            Log.Write($"Frame Time: ");
            Log.WriteLine($"{dt * 1000.0:n3}ms", new Log.Colors(ConsoleColor.White, color));
            Log.WriteLine();
        }

        public override async Task Update(PageDrawer drawer, double dt) {
            await base.Update(drawer, dt);
            ProfilerTime += dt;
            if (ProfilerTime >= NextRedrawTime) {
                NextRedrawTime += RedrawInterval;
                drawer.EnqueueRedraw();
            }
        }
    }
}
