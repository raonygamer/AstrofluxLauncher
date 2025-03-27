using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstrofluxLauncher.Common {
    public class Input {
        public readonly ConsoleKey[] AllKeys = Enum.GetValues<ConsoleKey>();

        private List<Func<ConsoleKey, bool>> OnKeyPressed = [];

        public Input() {
            Task.Run(PoolInputs);
        }

        private void PoolInputs() {
            while (true) {
                ConsoleKeyInfo key = Console.ReadKey(true);
                UpdateKeyState(key.Key);
            }
        }

        private void UpdateKeyState(ConsoleKey key) {
            for (int i = 0; i < OnKeyPressed.Count; i++) {
                if (OnKeyPressed[i](key))
                    break;
            }
        }

        public void AddOnKeyPressed(Func<ConsoleKey, bool> func) {
            OnKeyPressed.Add(func);
        }

        public void RemoveOnKeyPressed(Func<ConsoleKey, bool> func) {
            OnKeyPressed.Remove(func);
        }
    }
}
