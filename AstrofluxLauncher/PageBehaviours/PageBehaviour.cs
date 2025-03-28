using AstrofluxLauncher.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstrofluxLauncher.PageBehaviours {
    public abstract class PageBehaviour {
        public ItemSelector PageSelector { get; private set; }
        public bool WasRendered = false;

        public PageBehaviour(ItemSelector pageSelector) {
            PageSelector = pageSelector;
        }

        public virtual void OnPageEnter(ItemSelector? prevPage) {

        }

        public virtual void OnPageExit(ItemSelector? newPage) {
            WasRendered = false;
        }

        public virtual bool OnKeyPressed(ConsoleKey key) {
            if (key == ConsoleKey.Escape) {
                Program.Instance.BackSelector(true);
                return true;
            }
            return false;
        }

        public virtual void OnPageRender() {
            WasRendered = true;
        }

        public virtual void OnItemSelected(Item item, int index) {

        }

        public virtual void OnItemUnselected(Item item, int index) {

        }
    }
}
