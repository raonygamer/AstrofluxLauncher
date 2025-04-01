using AstrofluxLauncher.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AstrofluxLauncher.UI
{
    public abstract class AbstractPage
    {
        public uint DrawingCount { get; protected set; } = 0;
        public int? CalculatedHeight { get; protected set; } = null;
        private bool EnableDrawInfo { get; set; } = false;

        public virtual async Task OnPageEnter(PageDrawer drawer, AbstractPage? prevPage, bool shouldRecompose, Dictionary<string, object>? customData)
        {
            if (shouldRecompose)
                await ComposePage(drawer, customData);
        }

        public virtual async Task OnPageExit(PageDrawer drawer, AbstractPage? nextPage)
        {
            await Task.CompletedTask;
        }

        public virtual async Task EarlyDraw(PageDrawer drawer, double dt)
        {
            await Task.CompletedTask;
        }

        public virtual async Task Draw(PageDrawer drawer, double dt)
        {
            DrawingCount++;
            var pageAttribute = GetType().GetCustomAttribute<PageAttribute>()!;
            if (EnableDrawInfo)
            {
                Log.TraceLine($"");
                Log.TraceLine($"Page: '{GetType().Name}', '{pageAttribute.DisplayName}', '{pageAttribute.ID}'.");
                Log.TraceLine($"Redraw count: {DrawingCount}");
                Log.TraceLine($"");
            }

            await Task.CompletedTask;
        }

        public virtual async Task LateDraw(PageDrawer drawer, double dt)
        {
            CalculatedHeight = Console.CursorTop - drawer.StartY;
            await Task.CompletedTask;
        }

        public virtual async Task Update(PageDrawer drawer, double dt)
        {
            await Task.CompletedTask;
        }

        public virtual async Task<bool> OnKeyDown(PageDrawer drawer, Input input, ConsoleKeyInfo keyInfo)
        {
            if (keyInfo.Key != ConsoleKey.Escape || drawer.PreviousPage is null)
                return false;
            await drawer.ChangePage(drawer.PreviousPage, true, null);
            return true;
        }

        public virtual async Task<bool> OnKeyUp(PageDrawer drawer, Input input, ConsoleKeyInfo keyInfo)
        {
            await Task.CompletedTask;
            return false;
        }

        public virtual void SetProperty<T>(Action<T> setterFunc) where T : AbstractPage
        {
            setterFunc((T)(object)this);
        }

        public virtual T GetProperty<TClass, T>(Func<TClass, T> getterFunc) where TClass : AbstractPage
        {
            return getterFunc((TClass)(object)this);
        }
        
        public virtual async Task SetPropertyAsync<T>(Func<T, Task> setterFunc) where T : AbstractPage
        {
            await setterFunc((T)(object)this);
        }

        public virtual async Task<T> GetPropertyAsync<TClass, T>(Func<TClass, Task<T>> getterFunc) where TClass : AbstractPage
        {
            return await getterFunc((TClass)(object)this);
        }

        public virtual async Task ComposePage(PageDrawer drawer, Dictionary<string, object>? customData)
        {
            await Task.CompletedTask;
        }
    }
}