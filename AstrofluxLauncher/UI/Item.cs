using AstrofluxLauncher.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstrofluxLauncher.UI {
    public class Item {
        private string _Id = "";
        public string Id {
            get => _Id;
            set {
                _Id = value;
                TriggerRedrawFunction?.Invoke();
            }
        }

        private string _Name = "";
        public string Name {
            get => _Name;
            set {
                _Name = value;
                TriggerRedrawFunction?.Invoke();
            }
        }

        private bool _Disabled = false;
        public bool Disabled {
            get => _Disabled;
            set {
                _Disabled = value;
                TriggerRedrawFunction?.Invoke();
            }
        }

        private bool _Selectable = true;
        public bool Selectable {
            get => _Selectable;
            set {
                _Selectable = value;
                TriggerRedrawFunction?.Invoke();
            }
        }

        private Action<ItemSelector, Item>? _SelectAction = null;
        public Action<ItemSelector, Item>? SelectAction {
            get => _SelectAction;
            set {
                _SelectAction = value;
                TriggerRedrawFunction?.Invoke();
            }
        }

        private Action<ItemSelector, Item>? _UnselectAction = null;
        public Action<ItemSelector, Item>? UnselectAction {
            get => _UnselectAction;
            set {
                _UnselectAction = value;
                TriggerRedrawFunction?.Invoke();
            }
        }

        private ConsoleColor _DefaultColor = ConsoleColor.White;
        public ConsoleColor DefaultColor {
            get => _DefaultColor;
            set {
                _DefaultColor = value;
                TriggerRedrawFunction?.Invoke();
            }
        }

        private ConsoleColor _CursorOnColor = ConsoleColor.Yellow;
        public ConsoleColor CursorOnColor {
            get => _CursorOnColor;
            set {
                _CursorOnColor = value;
                TriggerRedrawFunction?.Invoke();
            }
        }

        private ConsoleColor _SelectedColor = ConsoleColor.Cyan;
        public ConsoleColor SelectedColor {
            get => _SelectedColor;
            set {
                _SelectedColor = value;
                TriggerRedrawFunction?.Invoke();
            }
        }

        private ConsoleColor _DisabledColor = ConsoleColor.DarkGray;
        public ConsoleColor DisabledColor {
            get => _DisabledColor;
            set {
                _DisabledColor = value;
                TriggerRedrawFunction?.Invoke();
            }
        }

        public Action? TriggerRedrawFunction { get; set; } = null;

        public Item(string id = "", string name = "", bool disabled = false, bool selectable = true, Action<ItemSelector, Item>? selectAction = null) {
            Id = id;
            Name = name;
            Disabled = disabled;
            Selectable = selectable;
            SelectAction = selectAction ?? SelectAction;
        }

        public virtual void Draw(ItemSelector selector, int itemIndex) {
            string txt = $"{Name}";
            ConsoleColor color = Disabled ? DisabledColor : DefaultColor;

            if (itemIndex == selector.SelectedIndex) {
                txt = "* " + txt;
                color = SelectedColor;
            }
            else if (itemIndex == selector.NavigationIndex) {
                txt = "> " + txt;
                color = CursorOnColor;
            }
            else {
                txt = "  " + txt;
            }
            Log.Write(txt, color, true);
        }
    }
}
