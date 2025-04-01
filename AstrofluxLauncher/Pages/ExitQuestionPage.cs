using AstrofluxLauncher.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AstrofluxLauncher.Common;

namespace AstrofluxLauncher.Pages {
    [Page("exit_page", "ExitPage", true, false)]
    public class ExitQuestionPage : ItemListSelectPageBase {
        public ExitQuestionPage() {
            Title = "Are you sure you want to exit?";
            SetProperty<ItemListSelectPageBase>(pg => {
                pg.SelectorItems.Add(new SelectorItem("yes_item", "Yes") { HoverTextColor = ConsoleColor.Black, HoverBackColor = ConsoleColor.Red, CanBeSelected = false });
                pg.SelectorItems.Add(new SelectorItem("no_item", "No") { HoverTextColor = ConsoleColor.Black, HoverBackColor = ConsoleColor.White, CanBeSelected = false });
                pg.NavigationIndex = pg.FindNextNavigationIndex(1, 1);
            });
        }

        public override async Task OnPageEnter(PageDrawer drawer, AbstractPage? prevPage, bool recompose, Dictionary<string, object>? customData) {
            await base.OnPageEnter(drawer, prevPage, recompose, customData);
            NavigationIndex = FindNextNavigationIndex(1, 1);
            SelectedIndex = -1;
        }

        public override async Task<bool> OnKeyDown(PageDrawer drawer, Input input, ConsoleKeyInfo keyInfo)
        {
            bool handled = await base.OnKeyDown(drawer, input, keyInfo);
            if (keyInfo.Key == ConsoleKey.Escape)
            {
                await drawer.ChangePage("main_page", true);
                return true;
            }
            return handled;
        }
        
        public override async Task<bool> OnItemSelected(PageDrawer drawer, ItemListSelectPageBase pg, SelectorItem item, int index) {
            await base.OnItemSelected(drawer, pg, item, index);
            switch (item.Id) {
                case "yes_item":
                    drawer.Launcher.ExitGracefully();
                    break;
                case "no_item":
                    await drawer.ChangePage("main_page", true, null);
                    break;
            }

            return false;
        }
    }
}
