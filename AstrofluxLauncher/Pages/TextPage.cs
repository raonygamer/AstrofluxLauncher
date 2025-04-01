using AstrofluxLauncher.Common;
using AstrofluxLauncher.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstrofluxLauncher.Pages {
    public class TextPage : AbstractPage {
        public List<string> Texts { get; private set; } = [];

        public override async Task Draw(PageDrawer drawer, double dt) {
            await base.Draw(drawer, dt);
            foreach (var text in Texts) {
                Log.WriteLine(text, new Log.Colors(ConsoleColor.DarkCyan, ConsoleColor.Black));
            }
        }

        public void SetTexts(List<string> texts) {
            SetProperty<TextPage>(page => {
                page.Texts = texts;
            });
        }

        public void AddText(string text) {
            SetProperty<TextPage>(page => {
                page.Texts.Add(text);
            });
        }

        public void RemoveText(string text) {
            SetProperty<TextPage>(page => {
                page.Texts.Remove(text);
            });
        }
    }
}
