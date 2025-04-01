using AstrofluxLauncher.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AstrofluxLauncher.Common {
    public partial class Input {
        [LibraryImport("user32.dll")]
        private static partial short GetAsyncKeyState(ConsoleKey key);

        public readonly ConsoleKey[] AllKeys = Enum.GetValues<ConsoleKey>();

        private readonly Dictionary<ConsoleKey, bool> PrevKeyStates = new();
        private readonly Dictionary<ConsoleKey, bool> KeyStates = new();

        private readonly Dictionary<ConsoleKey, int> FrameKeyState = new();

        public delegate Task<bool> OnKeyEventDelegate(ConsoleKeyInfo keyInfo);

        private List<OnKeyEventDelegate> OnKeyDown = [];
        private List<OnKeyEventDelegate> OnKeyUp = [];

        public Input() {
            KeyStates = AllKeys.ToDictionary(key => key, key => false);
            PrevKeyStates = AllKeys.ToDictionary(key => key, key => false);
            FrameKeyState = AllKeys.ToDictionary(key => key, key => 0);
        }

        public async Task SynchronousPoolInputs()
        {
            if (!Util.WindowIsInFocus())
                return;
            foreach (var k in AllKeys) {
                FrameKeyState[k] = 0;
                var keyState = GetAsyncKeyState(k);
                if ((keyState & 0b1000000000000000) != 0) {
                    KeyStates[k] = true;
                }
                else {
                    KeyStates[k] = false;
                }

                if (!PrevKeyStates[k] && KeyStates[k]) {
                    await TriggerKeyDown(new ConsoleKeyInfo((char)k, k, false, false, false));
                    FrameKeyState[k] = 1;
                }
                else if (PrevKeyStates[k] && !KeyStates[k]) {
                    await TriggerKeyUp(new ConsoleKeyInfo((char)k, k, false, false, false));
                    FrameKeyState[k] = -1;
                }

                PrevKeyStates[k] = KeyStates[k];
            }
        }

        private async Task TriggerKeyDown(ConsoleKeyInfo key) {
            for (int i = 0; i < OnKeyDown.Count; i++) {
                if (await OnKeyDown[i](key))
                    break;
            }
        }

        private async Task TriggerKeyUp(ConsoleKeyInfo key) {
            for (int i = 0; i < OnKeyUp.Count; i++) {
                if (await OnKeyUp[i](key))
                    break;
            }
        }

        public void AddOnKeyDown(OnKeyEventDelegate func) {
            OnKeyDown.Add(func);
        }

        public void RemoveOnKeyDown(OnKeyEventDelegate func) {
            OnKeyDown.Remove(func);
        }

        public void AddOnKeyUp(OnKeyEventDelegate func) {
            OnKeyUp.Add(func);
        }

        public void RemoveOnKeyUp(OnKeyEventDelegate func) {
            OnKeyUp.Remove(func);
        }

        public bool GetKeyDown(ConsoleKey key) {
            return FrameKeyState[key] == 1;
        }

        public bool GetKey(ConsoleKey key) {
            return KeyStates[key];
        }

        public bool GetKeyUp(ConsoleKey key) {
            return FrameKeyState[key] == -1;
        }
    }
}
