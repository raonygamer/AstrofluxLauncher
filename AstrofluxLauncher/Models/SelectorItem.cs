using AstrofluxLauncher.Common;
using AstrofluxLauncher.Pages;
using AstrofluxLauncher.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstrofluxLauncher {
    public class SelectorItem {
        private bool _shouldTriggerRedraw = false;
        private string _id;
        private string _text;
        private ConsoleColor _defaultTextColor = ConsoleColor.Gray;
        private ConsoleColor _defaultBackColor = ConsoleColor.Black;
        private ConsoleColor _hoverTextColor = ConsoleColor.Black;
        private ConsoleColor _hoverBackColor = ConsoleColor.Gray;
        private ConsoleColor _selectedTextColor = ConsoleColor.Black;
        private ConsoleColor _selectedBackColor = ConsoleColor.Red;
        private ConsoleColor _disabledTextColor = ConsoleColor.DarkGray;
        private ConsoleColor _disabledBackColor = ConsoleColor.Black;
        private string _defaultNavigatorFormat = "[{0}] {1}";
        private string _hoverNavigatorFormat = "[{0}] {1}";
        private string _selectedNavigatorFormat = "[{0}] {1}";
        private string _disabledNavigatorFormat = " {0}  {1}";
        private char _defaultNavigatorCharacter = ' ';
        private char _hoverNavigatorCharacter = '>';
        private char _selectedNavigatorCharacter = 'o';
        private char _disabledNavigatorCharacter = ' ';
        private bool _disabled = false;
        private bool _canBeSelected = true;
        public Dictionary<string, object>? CustomData { get; set; } = null;
        #region Properties
        public bool ShouldTriggerRedraw {
            get => _shouldTriggerRedraw;
            set => _shouldTriggerRedraw = value;
        }

        public string Id {
            get => _id;
            set {
                _id = value;
                ShouldTriggerRedraw = true;
            }
        }

        public string Text {
            get => _text;
            set {
                _text = value;
                ShouldTriggerRedraw = true;
            }
        }

        public ConsoleColor DefaultTextColor {
            get => _defaultTextColor;
            set {
                _defaultTextColor = value;
                ShouldTriggerRedraw = true;
            }
        }

        public ConsoleColor DefaultBackColor {
            get => _defaultBackColor;
            set {
                _defaultBackColor = value;
                ShouldTriggerRedraw = true;
            }
        }

        public ConsoleColor HoverTextColor {
            get => _hoverTextColor;
            set {
                _hoverTextColor = value;
                ShouldTriggerRedraw = true;
            }
        }

        public ConsoleColor HoverBackColor {
            get => _hoverBackColor;
            set {
                _hoverBackColor = value;
                ShouldTriggerRedraw = true;
            }
        }

        public ConsoleColor SelectedTextColor {
            get => _selectedTextColor;
            set {
                _selectedTextColor = value;
                ShouldTriggerRedraw = true;
            }
        }

        public ConsoleColor SelectedBackColor {
            get => _selectedBackColor;
            set {
                _selectedBackColor = value;
                ShouldTriggerRedraw = true;
            }
        }

        public ConsoleColor DisabledTextColor {
            get => _disabledTextColor;
            set {
                _disabledTextColor = value;
                ShouldTriggerRedraw = true;
            }
        }

        public ConsoleColor DisabledBackColor {
            get => _disabledBackColor;
            set {
                _disabledBackColor = value;
                ShouldTriggerRedraw = true;
            }
        }

        public string DefaultNavigatorFormat {
            get => _defaultNavigatorFormat;
            set {
                _defaultNavigatorFormat = value;
                ShouldTriggerRedraw = true;
            }
        }

        public string HoverNavigatorFormat {
            get => _hoverNavigatorFormat;
            set {
                _hoverNavigatorFormat = value;
                ShouldTriggerRedraw = true;
            }
        }

        public string SelectedNavigatorFormat {
            get => _selectedNavigatorFormat;
            set {
                _selectedNavigatorFormat = value;
                ShouldTriggerRedraw = true;
            }
        }

        public string DisabledNavigatorFormat {
            get => _disabledNavigatorFormat;
            set {
                _disabledNavigatorFormat = value;
                ShouldTriggerRedraw = true;
            }
        }

        public char DefaultNavigatorCharacter {
            get => _defaultNavigatorCharacter;
            set {
                _defaultNavigatorCharacter = value;
                ShouldTriggerRedraw = true;
            }
        }

        public char HoverNavigatorCharacter {
            get => _hoverNavigatorCharacter;
            set {
                _hoverNavigatorCharacter = value;
                ShouldTriggerRedraw = true;
            }
        }

        public char SelectedNavigatorCharacter {
            get => _selectedNavigatorCharacter;
            set {
                _selectedNavigatorCharacter = value;
                ShouldTriggerRedraw = true;
            }
        }

        public char DisabledNavigatorCharacter {
            get => _disabledNavigatorCharacter;
            set {
                _disabledNavigatorCharacter = value;
                ShouldTriggerRedraw = true;
            }
        }

        public bool Disabled {
            get => _disabled;
            set {
                _disabled = value;
                ShouldTriggerRedraw = true;
            }
        }

        public bool CanBeSelected {
            get => _canBeSelected;
            set {
                _canBeSelected = value;
                ShouldTriggerRedraw = true;
            }
        }
        #endregion

        public SelectorItem(string id, string text, bool disabled = false, bool canBeSelected = true, Dictionary<string, object>? customData = null) {
            _id = id;
            _text = text;
            _disabled = disabled;
            _canBeSelected = canBeSelected;
            CustomData = customData;
        }

        public virtual async Task Draw(ItemListSelectPageBase page, PageDrawer drawer, int index, double dt) {
            if (page.SelectedIndex == index) {
                Log.Write(string.Format(SelectedNavigatorFormat, SelectedNavigatorCharacter, ""));
                Log.WriteLine(Text, new Log.Colors(SelectedTextColor, SelectedBackColor));
            }
            else if (page.NavigationIndex == index) {
                Log.Write(string.Format(HoverNavigatorFormat, HoverNavigatorCharacter, ""));
                Log.WriteLine(Text, new Log.Colors(HoverTextColor, HoverBackColor));
            }
            else if (Disabled) {
                Log.Write(string.Format(DisabledNavigatorFormat, DisabledNavigatorCharacter, ""));
                Log.WriteLine(Text, new Log.Colors(DisabledTextColor, DisabledBackColor));
            }
            else {
                Log.Write(string.Format(DefaultNavigatorFormat, DefaultNavigatorCharacter, ""));
                Log.WriteLine(Text, new Log.Colors(DefaultTextColor, DefaultBackColor));
            }
        }

        public virtual async Task Update(ItemListSelectPageBase page, PageDrawer drawer, int index, double dt) {
            if (ShouldTriggerRedraw) {
                ShouldTriggerRedraw = false;
                drawer.EnqueueRedraw();
            }
        }
    }
}
