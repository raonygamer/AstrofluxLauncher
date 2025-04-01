using AstrofluxLauncher.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AstrofluxLauncher.UI {
    public class PageDrawer {
        public static Dictionary<string, AbstractPage> SharedPages = [];

        public Launcher Launcher { get; private set; }
        public AbstractPage? PreviousPage { get; private set; }
        public AbstractPage? CurrentPage { get; private set; }
        public Dictionary<string, AbstractPage> Pages { get; private set; } = [];
        public Dictionary<string, object>? CurrentPageData { get; private set; } = null;
        public int StartY { get; set; } = 0;

        private bool ShouldRedraw = false;

        public PageDrawer(Launcher launcher, Input input) {
            Launcher = launcher;
            input.AddOnKeyDown(OnKeyDown);
            input.AddOnKeyUp(OnKeyUp);
            var types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => !t.IsAbstract && t.GetCustomAttribute<PageAttribute>() is not null && t.GetConstructor([]) is not null).ToList();

            foreach (var type in types)
            {
                AbstractPage? page = null;
                var pageAttribute = type.GetCustomAttribute<PageAttribute>()!;
                if (pageAttribute.SingleInstance) {
                    if (!SharedPages.TryGetValue(pageAttribute.ID, out page)) {
                        page = Activator.CreateInstance(type, true) as AbstractPage;
                        if (page is not null)
                            SharedPages.Add(pageAttribute.ID, page);
                    }
                }
                    
                page ??= Activator.CreateInstance(type, true) as AbstractPage;
                if (page is not null) {
                    Pages.Add(pageAttribute.ID, page);
                }
            }
        }

        public async Task ChangePage(AbstractPage page, bool recompose = false, Dictionary<string, object>? customData = null) {
            if (CurrentPage != null) {
                await CurrentPage.OnPageExit(this, page);
            }
            
            var attribute = CurrentPage?.GetType().GetCustomAttribute<PageAttribute>();
            if (attribute is not null && attribute.ShouldRegisterAsPreviousPage)
                PreviousPage = CurrentPage;
            CurrentPage = page;
            bool willRecompose = recompose || CurrentPage.DrawingCount == 0;
            CurrentPageData = customData;
            await CurrentPage.OnPageEnter(this, PreviousPage, willRecompose, customData);
            if (willRecompose)
                EnqueueRedraw();
            Launcher.RedrawEverything = true;
        }

        public async Task ChangePage(string? pageId, bool recompose = false, Dictionary<string, object>? customData = null) {
            if (pageId is null || !Pages.TryGetValue(pageId, out var queriedPage))
            {
                var attribute = CurrentPage?.GetType().GetCustomAttribute<PageAttribute>()!;
                if (attribute.ShouldRegisterAsPreviousPage)
                    PreviousPage = CurrentPage;
                CurrentPage = null;
                CurrentPageData = null;
                await (CurrentPage?.OnPageExit(this, null) ?? Task.CompletedTask);
                EnqueueRedraw();
                return;
            }

            await ChangePage(queriedPage, recompose, customData);
        }

        public async Task Draw(double deltaTime) {
            if (CurrentPage != null) {
                await CurrentPage.Update(this, deltaTime);
                if (ShouldRedraw) {
                    ShouldRedraw = false;
                    Console.CursorTop = StartY;
                    await CurrentPage.EarlyDraw(this, deltaTime);
                    await CurrentPage.Draw(this, deltaTime);
                    await CurrentPage.LateDraw(this, deltaTime);
                }
            }
        }

        public async Task<bool> OnKeyDown(ConsoleKeyInfo keyInfo) {
            if (CurrentPage != null) {
                return await CurrentPage.OnKeyDown(this, Launcher.Input, keyInfo);
            }
            return false;
        }

        public async Task<bool> OnKeyUp(ConsoleKeyInfo keyInfo) {
            if (CurrentPage != null) {
                return await CurrentPage.OnKeyUp(this, Launcher.Input, keyInfo);
            }
            return false;
        }

        public void SetStartY(int y) {
            StartY = y;
            Console.CursorTop = y;
        }

        public void EnqueueRedraw() {
            ShouldRedraw = true;
        }
    }
}
