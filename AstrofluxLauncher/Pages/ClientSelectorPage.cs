using System.Diagnostics;
using AstrofluxLauncher.Common;
using AstrofluxLauncher.Contexts;
using AstrofluxLauncher.UI;
using AstrofluxLauncher.Utils;
using SharpFileDialog;

namespace AstrofluxLauncher.Pages;

[Page("client_selector_page", "ClientSelectorPage")]
public class ClientSelectorPage : ItemListSelectPageBase
{
    public override async Task Draw(PageDrawer drawer, double dt)
    {
        Log.TraceLine("Press L to launch the game.");
        Log.WriteLine();
        await base.Draw(drawer, dt);
    }

    public override async Task<bool> OnItemSelected(PageDrawer drawer, ItemListSelectPageBase pg, SelectorItem item, int index)
    {
        if (item.CustomData is null)
            return false;
        
        if (item.Id is ("add_new" or "refresh"))
        {
            if (!item.CustomData.TryGetValue("GameType", out var type) || type is not GameType gameType)
                return false;
            
            switch (item.Id)
            {
                case "add_new":
                {
                    if (!NativeFileDialog.OpenDialog([
                            new()
                            {
                                Extensions = ["swf"],
                                Name = "Shockwave Files"
                            }
                        ], null, out var file) || !File.Exists(file))
                        return false;

                    Directory.CreateDirectory(GameContext.LauncherClientPath);
                    await File.WriteAllBytesAsync(
                        Path.Combine(GameContext.LauncherClientPath, Path.GetFileName(file)), 
                        await File.ReadAllBytesAsync(file));
                    await ComposePage(drawer, item.CustomData);
                    drawer.EnqueueRedraw();
                    break;
                }
                case "refresh":
                {
                    await ComposePage(drawer, item.CustomData);
                    drawer.EnqueueRedraw();
                    break;
                }
            }
            return false;
        }

        if (!item.CustomData.TryGetValue("Url", out var urlObj) || urlObj is not string url)
            return false;

        drawer.Launcher.Config.Config.SwfRemoteUrl = url;
        drawer.Launcher.Config.Save();
        return true;
    }

    public override async Task<bool> OnKeyDown(PageDrawer drawer, Input input, ConsoleKeyInfo keyInfo)
    {
        var handled = await base.OnKeyDown(drawer, input, keyInfo);
        if (drawer.CurrentPageData is null || !drawer.CurrentPageData.TryGetValue("GameType", out var gameTypeObj) || gameTypeObj is not GameType gameType)
            return handled;
        
        if (keyInfo.Key == ConsoleKey.L)
        {
            switch (gameType)
            {
                case GameType.Steam:
                    Process.Start(new ProcessStartInfo($"steam://rungameid/{LauncherInfo.AstrofluxGameId}")
                        { UseShellExecute = true });
                    break;
                case GameType.Itch:
                    Process.Start(new ProcessStartInfo(Path.Combine(Path.GetDirectoryName(GameContext.ItchVersionPath!)!, "Astroflux.exe"))
                        { UseShellExecute = false });
                    break;
            }
            return true;
        }
        return handled;
    }

    public override async Task ComposePage(PageDrawer drawer, Dictionary<string, object>? customData)
    {
        if (customData is null || !customData.TryGetValue("GameType", out var type) || type is not GameType gameType)
            return;
        
        await base.ComposePage(drawer, customData);
        await SetPropertyAsync<ItemListSelectPageBase>(async pg =>
        {
            var vanillaUrl = await GameContext.GetCurrentGameVersionUrl();
            pg.Title = $"Select client for {(gameType == GameType.Steam ? "Steam Astroflux" : "Itch.io Astroflux")}";
            pg.SelectorItems.Clear();
            pg.SelectorItems.Add(new("vanilla_astroflux", $"Vanilla ({vanillaUrl})", false, true, 
                new () {{"Url", "http://r.playerio.com/r/rymdenrunt-k9qmg7cvt0ylialudmldvg/Preload.swf"}}));
            
            foreach (var clientFile in GameContext.GetAllClientFiles())
            {
                var fileName = Path.GetFileNameWithoutExtension(clientFile);
                var parts = fileName.Split('@');
                if (parts.Length < 1)
                    continue;
                var name = parts[0];
                var version = "";
                for (var i = 1; i < parts.Length; i++)
                {
                    version += "v" + parts[i].TrimStart('v') + " / ";
                }
                if (version.EndsWith(" / "))
                    version = version.Substring(0, version.Length - 3);
                
                if (version is null or { Length: 0 } or "")
                    version = "Unknown Version";
                
                pg.SelectorItems.Add(new(fileName.ToLower(), $"{name} ({version})", false, true, 
                    new () {{"Url", $"file://{clientFile.Replace('\\', '/')}"}}));
            }
            
            pg.SelectorItems.Add(new("add_new", $"Add new client", false, false, customData)
            {
                HoverTextColor = ConsoleColor.DarkCyan,
                HoverNavigatorCharacter = '+'
            });
            
            pg.SelectorItems.Add(new("refresh", $"Refresh...", false, false, customData)
            {
                HoverTextColor = ConsoleColor.Yellow,
            });
            
            drawer.Launcher.Config.Load();
            pg.SelectedIndex = pg.SelectorItems.IndexOf(pg.SelectorItems.FirstOrDefault(c =>
                c.CustomData is not null &&
                c.CustomData.TryGetValue("Url", out var urlStr) &&
                urlStr is string url &&
                url == drawer.Launcher.Config.Config.SwfRemoteUrl)!);

            if (pg.SelectedIndex == -1)
            {
                pg.SelectedIndex = 0;
                await OnItemSelected(drawer, this, pg.SelectorItems[0], 0);
            }
            if (pg.NavigationIndex == -1)
                pg.NavigationIndex = pg.FindNextNavigationIndex(0, 1);
            drawer.EnqueueRedraw();
        });
    }
}