using AstrofluxLauncher.Common;
using AstrofluxLauncher.UI;
using AstrofluxLauncher.Utils;

namespace AstrofluxLauncher.Pages;

[Page("main_page", "MainPage")]
public class MainPage : ItemListSelectPageBase
{
    public override async Task<bool> OnItemSelected(PageDrawer drawer, ItemListSelectPageBase pg, SelectorItem item, int index)
    {
        await base.OnItemSelected(drawer, pg, item, index);
        switch (item.Id)
        {
            case "steam_game":
            case "itch_game":
            {
                var gameType = item.Id switch
                {
                    "steam_game" => GameType.Steam,
                    "itch_game" => GameType.Itch,
                    _ => throw new ArgumentOutOfRangeException()
                };
                
                var gameState = drawer.Launcher.GameContext.GetState(gameType);
                if (gameState is (GameState.InstalledCanBePatched or GameState.InstalledPatched))
                    await drawer.ChangePage(gameState is GameState.InstalledCanBePatched ? "should_patch_page" : "client_selector_page", true, new() {
                        { "GameType", gameType },
                        { "GameState", gameState }
                    });
                break;
            }
            case "refresh":
                await ComposePage(drawer, null);
                break;
            default:
                return true;
        }
        return true;
    }

    public override async Task ComposePage(PageDrawer drawer, Dictionary<string, object>? customData)
    {
        SetProperty<ItemListSelectPageBase>(pg =>
        {
            Title = "Select the game type to work with:";
            
            var steamGameState = drawer.Launcher.GameContext.GetState(GameType.Steam);
            var itchGameState = drawer.Launcher.GameContext.GetState(GameType.Itch);
            
            pg.SelectorItems = [
                new(
                    "steam_game", 
                    $"Astroflux Steam ({GetGameStateText(steamGameState)})", 
                    steamGameState is (GameState.NotInstalled or GameState.InstalledCannotBePatched), 
                    false)
                {
                    HoverBackColor = GetHoverGameStateBackColor(steamGameState), 
                    HoverTextColor = GetHoverGameStateTextColor(steamGameState), 
                    DefaultTextColor = GetDefaultGameStateTextColor(steamGameState),
                    DisabledTextColor = GetDisabledGameStateTextColor(steamGameState),
                },
                new(
                    "itch_game", 
                    $"Astroflux Itch.io ({GetGameStateText(itchGameState)})", 
                    itchGameState is (GameState.NotInstalled or GameState.InstalledCannotBePatched), 
                    false) 
                {
                    HoverBackColor = GetHoverGameStateBackColor(itchGameState), 
                    HoverTextColor = GetHoverGameStateTextColor(itchGameState), 
                    DefaultTextColor = GetDefaultGameStateTextColor(itchGameState),
                    DisabledTextColor = GetDisabledGameStateTextColor(itchGameState)
                },
                new("refresh", $"Refresh...", false, false),
            ];

            if (pg.NavigationIndex == -1)
                pg.NavigationIndex = pg.FindNextNavigationIndex(0, 1);
        });
    }

    private string GetGameStateText(GameState state)
    {
        return state switch
        {
            GameState.InstalledPatched => "Installed, patched",
            GameState.InstalledPatchedOutdated => "Installed, patched, outdated patches",
            GameState.InstalledCanBePatched => "Installed, can be patched",
            GameState.InstalledCannotBePatched => "Installed, cannot be patched",
            _ => "Not installed"
        };
    }

    private ConsoleColor GetHoverGameStateBackColor(GameState state)
    {
        return state switch
        {
            GameState.InstalledPatched => ConsoleColor.DarkCyan,
            GameState.InstalledPatchedOutdated => ConsoleColor.DarkYellow,
            GameState.InstalledCanBePatched => ConsoleColor.Yellow,
            GameState.InstalledCannotBePatched => ConsoleColor.Red,
            _ => ConsoleColor.Red
        };
    }
    
    private ConsoleColor GetHoverGameStateTextColor(GameState state)
    {
        return state switch
        {
            GameState.InstalledPatched => ConsoleColor.Black,
            GameState.InstalledPatchedOutdated => ConsoleColor.Black,
            GameState.InstalledCanBePatched => ConsoleColor.Black,
            GameState.InstalledCannotBePatched => ConsoleColor.Red,
            _ => ConsoleColor.Red
        };
    }
    
    private ConsoleColor GetDefaultGameStateTextColor(GameState state)
    {
        return state switch
        {
            GameState.InstalledPatched => ConsoleColor.White,
            GameState.InstalledPatchedOutdated => ConsoleColor.White,
            GameState.InstalledCanBePatched => ConsoleColor.White,
            GameState.InstalledCannotBePatched => ConsoleColor.Red,
            _ => ConsoleColor.Red
        };
    }

    public override async Task<bool> OnKeyDown(PageDrawer drawer, Input input, ConsoleKeyInfo keyInfo)
    {
        bool handled = await base.OnKeyDown(drawer, input, keyInfo);
        if (keyInfo.Key == ConsoleKey.Escape)
        {
            await drawer.ChangePage("exit_page", true);
            return true;
        }
        return handled;
    }

    private ConsoleColor GetDisabledGameStateTextColor(GameState state)
    {
        return state switch
        {
            GameState.InstalledPatched => ConsoleColor.DarkGray,
            GameState.InstalledPatchedOutdated => ConsoleColor.DarkGray,
            GameState.InstalledCanBePatched => ConsoleColor.DarkGray,
            GameState.InstalledCannotBePatched => ConsoleColor.DarkRed,
            _ => ConsoleColor.DarkRed
        };
    }
}