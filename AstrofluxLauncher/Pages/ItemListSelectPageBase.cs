using AstrofluxLauncher.Common;
using AstrofluxLauncher.Models;
using AstrofluxLauncher.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AstrofluxLauncher.Contexts;

namespace AstrofluxLauncher.Pages {
    public class ItemListSelectPageBase : TextPage {
        public string Title {
            get {
                return GetProperty<TextPage, string>(pg => pg.Texts.FirstOrDefault(""));
            }
            set {
                if (Texts.FirstOrDefault() is null) {
                    AddText(value);
                }
                else {
                    SetProperty<TextPage>(pg => {
                        pg.Texts[0] = value;
                    });
                }
            }
        }

        public List<SelectorItem> SelectorItems { get; set; } = [];
        public int NavigationIndex { get; set; } = -1;
        public int SelectedIndex { get; set; } = -1;

        public virtual async Task<bool> OnItemSelected(PageDrawer drawer, ItemListSelectPageBase pg, SelectorItem item, int index)
        {
            return true;
        }

        public virtual async Task<bool> OnItemUnselected(PageDrawer drawer, ItemListSelectPageBase pg, SelectorItem item, int index)
        {
            return true;
        }

        public virtual async Task OnItemNavigated(PageDrawer drawer, ItemListSelectPageBase pg, SelectorItem item, int index) {
            await Task.CompletedTask;
        }

        public override async Task Draw(PageDrawer drawer, double dt) {
            await base.Draw(drawer, dt);
            for (int i = 0; i < SelectorItems.Count; i++) {
                var item = SelectorItems[i];
                await item.Draw(this, drawer, i, dt);
            }

            if (NavigationIndex >= 0 && NavigationIndex < SelectorItems.Count) {
                var item = SelectorItems[NavigationIndex];
                if (item.Disabled)
                    NavigationIndex = FindNextNavigationIndex(0, 1);
            }

            if (SelectedIndex >= 0 && SelectedIndex < SelectorItems.Count) {
                var item = SelectorItems[SelectedIndex];
                if (item.Disabled || !item.CanBeSelected)
                    SelectedIndex = -1;
            }
        }

        public override async Task Update(PageDrawer drawer, double dt) {
            await base.Update(drawer, dt);
            for (int i = 0; i < SelectorItems.Count; i++) {
                var item = SelectorItems[i];
                await item.Update(this, drawer, i, dt);
            }
        }

        public override async Task<bool> OnKeyDown(PageDrawer drawer, Input input, ConsoleKeyInfo keyInfo) {
            var lastDownClick = await base.OnKeyDown(drawer, input, keyInfo);
            switch (keyInfo.Key) {
                case ConsoleKey.UpArrow: {
                    var oldIndex = --NavigationIndex;
                    NavigationIndex = FindNextNavigationIndex(NavigationIndex, -1);
                    await OnItemNavigated(drawer, this, SelectorItems[NavigationIndex], NavigationIndex);
                    drawer.EnqueueRedraw();
                    return true;
                }
                case ConsoleKey.DownArrow: {
                    var oldIndex = ++NavigationIndex;
                    NavigationIndex = FindNextNavigationIndex(NavigationIndex, 1);
                    await OnItemNavigated(drawer, this, SelectorItems[NavigationIndex], NavigationIndex);
                    drawer.EnqueueRedraw();
                    return true;
                }
                case ConsoleKey.Enter:
                    if (SelectedIndex == NavigationIndex)
                    {
                        bool unselect = true;
                        if (SelectedIndex >= 0 && SelectedIndex < SelectorItems.Count) {
                            unselect = await OnItemUnselected(drawer, this, SelectorItems[SelectedIndex], SelectedIndex);
                        }
                        SelectedIndex = unselect ? -1 : SelectedIndex;
                    }
                    else if (NavigationIndex >= 0 && NavigationIndex < SelectorItems.Count) {
                        var select = await OnItemSelected(drawer, this, SelectorItems[NavigationIndex], NavigationIndex);
                        if (select && SelectorItems[NavigationIndex] is { Disabled: false } and { CanBeSelected: true })
                            SelectedIndex = NavigationIndex;
                    }
                    drawer.EnqueueRedraw();
                    return true;
                default:
                    return lastDownClick;
            }
        }

        public int FindNextNavigationIndex(int index, int direction) {
            if (direction is not (1 or -1))
                return -1;

            if (direction == 1) {
                for (var i = 0; i < SelectorItems.Count; i++) {
                    if (!SelectorItems[i].Disabled && index <= i)
                        return i;
                }
                return FindNextNavigationIndex(0, 1);
            }
            else {
                for (var i = SelectorItems.Count - 1; i >= 0; i--) {
                    if (!SelectorItems[i].Disabled && i <= index)
                        return i;
                }
                return FindNextNavigationIndex(SelectorItems.Count - 1, -1);
            }
        }
    }
}
