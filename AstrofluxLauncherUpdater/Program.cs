using System.Diagnostics;
using System.IO.Compression;

namespace AstrofluxLauncherUpdater {
    internal class Program {
        static int Main(string[] args) {
            if (args.Length < 3 || args[0] != "update" || !int.TryParse(args[1], out int procPid) ||
                !File.Exists(args[2].Replace('\\', '/')))
            {
                Console.WriteLine("Arguments invalid: " + string.Join(" ", args));
                return -1;
            }
            
            string cwd = Directory.GetCurrentDirectory();
            string updatePath = Path.Combine(Directory.GetCurrentDirectory(), "update/");
            
            try
            {
                var proc = Process.GetProcessById(procPid);
                if (!proc.HasExited)
                {
                    Console.WriteLine("Waiting...");
                    int time = 0;
                    int interval = 20;
                    int limit = 2000;
                    while (!proc.HasExited)
                    {
                        Thread.Sleep(interval);
                        time += interval;
                        if (time >= limit)
                        {
                            proc.Kill();
                            Console.WriteLine("AstrofluxLauncher took too long to finish and was killed.");
                            break;
                        }
                    }
                }
            }
            catch (ArgumentException e)
            {
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            Console.WriteLine("Process finished");
            Console.WriteLine($"Updating AstrofluxLauncher...");
            List<string> filesToDelete = [];
            filesToDelete.AddRange(Directory.GetFiles(cwd, "*.exe", SearchOption.TopDirectoryOnly));
            filesToDelete.AddRange(Directory.GetFiles(cwd, "*.dll", SearchOption.TopDirectoryOnly));
            filesToDelete.AddRange(Directory.GetFiles(cwd, "*.pdb", SearchOption.TopDirectoryOnly));
            filesToDelete.AddRange(Directory.GetFiles(cwd, "*.runtimeconfig.json", SearchOption.TopDirectoryOnly));
            filesToDelete.AddRange(Directory.GetFiles(cwd, "*.deps.json", SearchOption.TopDirectoryOnly));

            foreach (string file in filesToDelete)
            {
                try
                {
                    File.Delete(file);
                }
                catch {}
            }

            ZipFile.ExtractToDirectory(args[2], updatePath, true);
            foreach (var dir in Directory.GetDirectories(updatePath, "AstrofluxLauncher_*", SearchOption.TopDirectoryOnly))
            {
                string[] files = Directory.GetFiles(dir, "*", SearchOption.TopDirectoryOnly);
                foreach (string file in files)
                {
                    File.Copy(file, Path.Combine(updatePath, Path.GetFileName(file)), true);
                }
                Directory.Delete(dir, true);
            }
            return 0;
        }
    }
}
