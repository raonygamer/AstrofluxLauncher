using AstrofluxLauncher.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AstrofluxLauncher.Utils;

namespace AstrofluxLauncher.Pages;

[Page("should_patch_page", "ShouldPatchPage", true, false)]
public class ShouldPatchQuestionPage : ItemListSelectPageBase {
    public ShouldPatchQuestionPage() {
        
    }

    public override async Task Draw(PageDrawer drawer, double dt)
    {
        await base.Draw(drawer, dt);
    }

    public override async Task OnPageEnter(PageDrawer drawer, AbstractPage? prevPage, bool recompose, Dictionary<string, object>? customData) {
        await base.OnPageEnter(drawer, prevPage, recompose, customData);
    }

    public override async Task<bool> OnItemSelected(PageDrawer drawer, ItemListSelectPageBase pg, SelectorItem item, int index) {
        await base.OnItemSelected(drawer, pg, item, index);
        if (item.CustomData is null || !item.CustomData.TryGetValue("GameType", out var type) || type is not GameType gameType)
            return false;
        
        switch (item.Id) {
            case "yes_item":
                await drawer.Launcher.GameContext.PatchGameAsync(gameType);
                drawer.Launcher.Config.Save();
                await drawer.ChangePage("client_selector_page", true, item.CustomData);
                break;
            case "no_item":
                await drawer.ChangePage("main_page", true, null);
                break;
        }

        return false;
    }

    public override async Task ComposePage(PageDrawer drawer, Dictionary<string, object>? customData)
    {
        if (customData is null || !customData.TryGetValue("GameType", out var type) || type is not GameType gameType || !customData.TryGetValue("GameState", out var state) || state is not GameState gameState)
            return;
        
        Title = gameState == GameState.InstalledPatchedOutdated ? 
            $"Seems like {(gameType == GameType.Steam ? "Steam Astroflux" : "Itch.io Astroflux")} patches are outdated, do you want to update them now?" : 
            $"Seems like {(gameType == GameType.Steam ? "Steam Astroflux" : "Itch.io Astroflux")} wasn't patched yet, do you want to patch it now?";
        
        SetProperty<ItemListSelectPageBase>(pg => {
            pg.SelectorItems.Clear();
            pg.SelectorItems.Add(new SelectorItem("yes_item", "Yes", false, false, customData) { HoverTextColor = ConsoleColor.Black, HoverBackColor = ConsoleColor.Green, CanBeSelected = false });
            pg.SelectorItems.Add(new SelectorItem("no_item", "No", false, false, customData) { HoverTextColor = ConsoleColor.Black, HoverBackColor = ConsoleColor.Red, CanBeSelected = false });
            pg.NavigationIndex = pg.FindNextNavigationIndex(0, 1);
        });
    }
}