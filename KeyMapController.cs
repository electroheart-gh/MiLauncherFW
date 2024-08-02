using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace MiLauncherFW
{
    internal class KeyMapController
    {
        private readonly Dictionary<(Keys key, Keys modifiers), Action> keyActions;
        private readonly Dictionary<string, Action> actionDictionary;

        public KeyMapController()
        {
            keyActions = new Dictionary<(Keys key, Keys modifiers), Action>();
            actionDictionary = new Dictionary<string, Action>();
        }

        public void MapKeyToAction(Keys key, Keys modifiers, Action action)
        {
            var keyCombo = (key, modifiers);
            keyActions[keyCombo] = action;
        }

        public void UnmapKey(Keys key, Keys modifiers)
        {
            var keyCombo = (key, modifiers);
            keyActions.Remove(keyCombo);
        }

        public void OnKeyDown(object sender, KeyEventArgs e)
        {
            var keyCombo = (e.KeyCode, e.Modifiers);
            if (keyActions.TryGetValue(keyCombo, out var action)) {
                action.Invoke();
            }
        }

        public void AttachTo(Form form)
        {
            form.KeyDown += OnKeyDown;
        }

        public void AttachTo(Control control)
        {
            control.KeyDown += OnKeyDown;
        }

        public void DetachFrom(Form form)
        {
            form.KeyDown -= OnKeyDown;
        }
        public void DetachFrom(Control control)
        {
            control.KeyDown -= OnKeyDown;
        }

        public void LoadKeyMapFromFile(string filePath)
        {
            var keyMapData = File.ReadAllText(filePath);
            var keyMap = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(keyMapData);

            if (keyMap == null) {
                throw new InvalidOperationException("Invalid key map configuration.");
            }

            keyActions.Clear();
            foreach (var entry in keyMap) {
                var actionName = entry.Key;
                var keyCombos = entry.Value;

                if (actionDictionary.TryGetValue(actionName, out var action)) {
                    foreach (var keyComboString in keyCombos) {
                        var parts = keyComboString.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                        var key = (Keys)Enum.Parse(typeof(Keys), parts[parts.Length - 1], true);

                        Keys modifiers = Keys.None;
                        for (int i = 0; i < parts.Length - 1; i++) {
                            if (parts[i] == "C") {
                                modifiers |= Keys.Control;
                            }
                            else if (parts[i] == "M") {
                                modifiers |= Keys.Alt;
                            }
                            else if (parts[i] == "S") {
                                modifiers |= Keys.Shift;
                            }
                        }
                        MapKeyToAction(key, modifiers, action);
                    }
                }
            }
        }

        public void RegisterAction(string actionName, Action action)
        {
            actionDictionary[actionName] = action;
        }
    }

}
