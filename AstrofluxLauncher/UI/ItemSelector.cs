using AstrofluxLauncher.Common;
using AstrofluxLauncher.PageBehaviours;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstrofluxLauncher.UI {
    public class ItemSelector {
        private string _Title = "";
        public string Title {
            get => _Title;
            set {
                _Title = value;
                NeedsRedraw = true;
            }
        }

        private List<Item> _Items = [];
        public List<Item> Items {
            get => _Items;
            set {
                _Items = value;
                NeedsRedraw = true;
            }
        }

        private int _NavigationIndex = 0;
        public int NavigationIndex {
            get => _NavigationIndex;
            set {
                _NavigationIndex = value;
                NeedsRedraw = true;
            }
        }

        private int _SelectedIndex = -1;
        public int SelectedIndex {
            get => _SelectedIndex;
            set {
                _SelectedIndex = value;
                NeedsRedraw = true;
            }
        }

        private PageBehaviour? _PageBehaviour = null;
        public PageBehaviour? PageBehaviour {
            get => _PageBehaviour;
            set {
                _PageBehaviour = value;
                NeedsRedraw = true;
            }
        }

        public bool NeedsRedraw { get; set; } = true;

        public ItemSelector(Input input, string title, List<Item> items, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type? pageBehaviourType = null) {
            input.AddOnKeyPressed((key) => {
                if (!Program.Instance.OnSelector(this))
                    return false;
                return OnKeyPressed(key);
            });

            Title = title;
            Items = items;
            foreach (var item in Items) {
                item.TriggerRedrawFunction = () => NeedsRedraw = true;
            }

            if (pageBehaviourType is not null && pageBehaviourType.IsSubclassOf(typeof(PageBehaviour)))
                _PageBehaviour = Activator.CreateInstance(pageBehaviourType, this) as PageBehaviour;

            NeedsRedraw = true;
        }

        public void AddItem(Item item) {
            item.TriggerRedrawFunction = () => NeedsRedraw = true;
            Items.Add(item);
        }

        public void RemoveItem(Item item) {
            item.TriggerRedrawFunction = null;
            Items.Remove(item);
        }

        public void ClearItems() {
            Items.Clear();
        }

        public void Draw(int startYPosition) {
            if (!NeedsRedraw)
                return;

            NeedsRedraw = false;
            if (SelectedIndex >= 0 && SelectedIndex <= Items.Count - 1 && !Items[SelectedIndex].Selectable)
                SelectedIndex = -1;

            if (NavigationIndex >= 0 && NavigationIndex <= Items.Count - 1 && Items[NavigationIndex].Disabled) {
                while (NavigationIndex >= 0 && NavigationIndex <= Items.Count - 1 && Items[NavigationIndex].Disabled) {
                    NavigationIndex++;
                }
            }

            if (!(NavigationIndex >= 0 && NavigationIndex <= Items.Count - 1))
                NavigationIndex = -1;

            Log.CurrentCursorYPosition = startYPosition;
            Log.Trace(Title, true);
            for (int i = 0; i < Items.Count; i++) {
                Items[i].Draw(this, i);
            }
            _PageBehaviour?.OnPageRender();
        }

        private bool OnKeyPressed(ConsoleKey key) {
            if (_PageBehaviour is not null) {
                if (_PageBehaviour.OnKeyPressed(key))
                    return true;
            }
            switch (key) {
                case ConsoleKey.UpArrow:
                    NavigationIndex--;
                    if (NavigationIndex < 0)
                        NavigationIndex = Items.Count - 1;
                    while (Items[NavigationIndex].Disabled) {
                        NavigationIndex--;
                        if (NavigationIndex < 0)
                            NavigationIndex = Items.Count - 1;
                    }
                    NeedsRedraw = true;
                    return true;
                case ConsoleKey.DownArrow:
                    NavigationIndex++;
                    if (NavigationIndex >= Items.Count)
                        NavigationIndex = 0;
                    while (Items[NavigationIndex].Disabled) {
                        NavigationIndex++;
                        if (NavigationIndex >= Items.Count)
                            NavigationIndex = 0;
                    }
                    NeedsRedraw = true;
                    return true;
                case ConsoleKey.Enter:
                    if (SelectedIndex == NavigationIndex && Items[NavigationIndex].Selectable) {
                        Items[SelectedIndex].UnselectAction?.Invoke(this, Items[SelectedIndex]);
                        _PageBehaviour?.OnItemUnselected(Items[SelectedIndex], SelectedIndex);
                        SelectedIndex = -1;
                        NeedsRedraw = true;
                        return true;
                    }

                    if (Items[NavigationIndex].Disabled || !Items[NavigationIndex].Selectable) {
                        Items[NavigationIndex].SelectAction?.Invoke(this, Items[NavigationIndex]);
                        _PageBehaviour?.OnItemSelected(Items[NavigationIndex], NavigationIndex);
                        return false;
                    }
                    SelectedIndex = NavigationIndex;
                    Items[SelectedIndex].SelectAction?.Invoke(this, Items[SelectedIndex]);
                    _PageBehaviour?.OnItemSelected(Items[SelectedIndex], SelectedIndex);
                    NeedsRedraw = true;
                    return true;
            }
            return false;
        }

        public void Reset() {
            NavigationIndex = 0;
            SelectedIndex = -1;
            NeedsRedraw = true;
        }
    }
}
