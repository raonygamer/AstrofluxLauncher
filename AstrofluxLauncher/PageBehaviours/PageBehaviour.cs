using AstrofluxLauncher.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstrofluxLauncher.PageBehaviours {
    public abstract class PageBehaviour {
        public ItemSelector PageSelector { get; private set; }

        public PageBehaviour(ItemSelector pageSelector) {
            PageSelector = pageSelector;
        }

        public virtual void OnPageEnter(ItemSelector? prevPage) { 
        
        }

        public virtual void OnPageExit(ItemSelector? newPage) { 
        
        }

        public virtual bool OnKeyPressed(ConsoleKey key) {
            return false;
        }

        public virtual void OnPageRender() {

        }

        public virtual void OnItemSelected(Item item, int index) {

        }

        public virtual void OnItemUnselected(Item item, int index) {

        }
    }
}
