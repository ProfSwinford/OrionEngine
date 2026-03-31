//https://learn.microsoft.com/en-us/dotnet/api/system.windows.input.keyboard?view=windowsdesktop-10.0

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Input; //Add "WindowsBase" reference to project


namespace OrionEngine.EngineModules
{
    public class InputModule
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        private static readonly Dictionary<Keycode, int> _map;
        private static bool[] _current;
        private static bool[] _previous;
        #region singleton
        private static InputModule inputModule = new InputModule();
        public static InputModule input => inputModule;
        public static bool InitializeInputModule()
        {
            int attempts = 0;
        INPUTINIT:
            if (input != null)
            {
                Debug.Log("Behaviour Control Module Initialized Successfully.");
                return true;
            }
            else if (attempts < 3)
            {
                Debug.LogError("Failed to Initialize Behaviour Control Module. Retrying");
                attempts++;
                goto INPUTINIT;
            }
            else
            {
                Debug.LogError("Failed to Initialize Behaviour Control Module after 3 attempts. Aborting.");
                return false;
            }
        }
        #endregion
        static InputModule()
        {
            _map = new Dictionary<Keycode, int>();

            // Letters
            for (int i = 0; i < 26; i++)
                _map[(Keycode)(1 + i)] = 0x41 + i; // 'A'..'Z'

            // Numbers (top row)
            _map[Keycode.Num0] = 0x30;
            _map[Keycode.Num1] = 0x31;
            _map[Keycode.Num2] = 0x32;
            _map[Keycode.Num3] = 0x33;
            _map[Keycode.Num4] = 0x34;
            _map[Keycode.Num5] = 0x35;
            _map[Keycode.Num6] = 0x36;
            _map[Keycode.Num7] = 0x37;
            _map[Keycode.Num8] = 0x38;
            _map[Keycode.Num9] = 0x39;

            // Symbols / punctuation (common US virtual-key codes)
            _map[Keycode.Backtick] = 0xC0; // VK_OEM_3
            _map[Keycode.Grave] = 0xC0;
            _map[Keycode.Tilde] = 0xC0;
            _map[Keycode.Minus] = 0xBD; // VK_OEM_MINUS
            _map[Keycode.Plus] = 0xBB;  // VK_OEM_PLUS
            _map[Keycode.Tab] = 0x09;   // VK_TAB
            _map[Keycode.Space] = 0x20; // VK_SPACE
            _map[Keycode.Enter] = 0x0D; // VK_RETURN
            _map[Keycode.BSlash] = 0xDC; // VK_OEM_5
            _map[Keycode.Backspace] = 0x08; // VK_BACK
            _map[Keycode.OpenSquareBracket] = 0xDB; // VK_OEM_4
            _map[Keycode.CloseSquareBracket] = 0xDD; // VK_OEM_6
            _map[Keycode.Semicolon] = 0xBA; // VK_OEM_1
            _map[Keycode.Quote] = 0xDE; // VK_OEM_7
            _map[Keycode.LessThan] = 0xE2; // VK_OEM_102 (may vary by layout)
            _map[Keycode.GreaterThan] = 0xE2;
            _map[Keycode.FSlash] = 0xBF; // VK_OEM_2 '/'

            // Arrows
            _map[Keycode.UpArrow] = 0x26; // VK_UP
            _map[Keycode.DownArrow] = 0x28; // VK_DOWN
            _map[Keycode.LeftArrow] = 0x25; // VK_LEFT
            _map[Keycode.RightArrow] = 0x27; // VK_RIGHT

            // Numpad
            _map[Keycode.Numpad0] = 0x60; // VK_NUMPAD0
            _map[Keycode.Numpad1] = 0x61;
            _map[Keycode.Numpad2] = 0x62;
            _map[Keycode.Numpad3] = 0x63;
            _map[Keycode.Numpad4] = 0x64;
            _map[Keycode.Numpad5] = 0x65;
            _map[Keycode.Numpad6] = 0x66;
            _map[Keycode.Numpad7] = 0x67;
            _map[Keycode.Numpad8] = 0x68;
            _map[Keycode.Numpad9] = 0x69;
            _map[Keycode.NumpadPeriod] = 0x6E; // VK_DECIMAL
            _map[Keycode.NumpadEnter] = 0x0D; // VK_RETURN (numpad ENTER reported as RETURN)
            _map[Keycode.NumpadPlus] = 0x6B; // VK_ADD
            _map[Keycode.NumpadMinus] = 0x6D; // VK_SUBTRACT
            _map[Keycode.NumpadAstarisk] = 0x6A; // VK_MULTIPLY
            _map[Keycode.NumpadFSlash] = 0x6F; // VK_DIVIDE

            // Modifiers
            _map[Keycode.LeftShift] = 0xA0; // VK_LSHIFT
            _map[Keycode.RightShift] = 0xA1; // VK_RSHIFT
            _map[Keycode.LeftControl] = 0xA2; // VK_LCONTROL
            _map[Keycode.RightControl] = 0xA3; // VK_RCONTROL
            _map[Keycode.LeftAlt] = 0xA4; // VK_LMENU
            _map[Keycode.RightAlt] = 0xA5; // VK_RMENU

            // Function keys and specials
            _map[Keycode.F1] = 0x70; _map[Keycode.F2] = 0x71; _map[Keycode.F3] = 0x72; _map[Keycode.F4] = 0x73;
            _map[Keycode.F5] = 0x74; _map[Keycode.F6] = 0x75; _map[Keycode.F7] = 0x76; _map[Keycode.F8] = 0x77;
            _map[Keycode.F9] = 0x78; _map[Keycode.F10] = 0x79; _map[Keycode.F11] = 0x7A; _map[Keycode.F12] = 0x7B;
            _map[Keycode.Insert] = 0x2D; // VK_INSERT
            _map[Keycode.Delete] = 0x2E; // VK_DELETE
            _map[Keycode.Home] = 0x24; // VK_HOME
            _map[Keycode.End] = 0x23; // VK_END
            _map[Keycode.PageUp] = 0x21; // VK_PRIOR
            _map[Keycode.PageDown] = 0x22; // VK_NEXT
            _map[Keycode.Escape] = 0x1B; // VK_ESCAPE

            int count = Enum.GetValues(typeof(Keycode)).Length;
            _current = new bool[count];
            _previous = new bool[count];
        }

        // Call once per frame to poll keyboard state
        public static void Update()
        {
            // copy current to previous
            Array.Copy(_current, _previous, _current.Length);

            foreach (var kv in _map)
            {
                var key = kv.Key;
                var vk = kv.Value;
                short state = GetAsyncKeyState(vk);
                bool down = (state & 0x8000) != 0;
                _current[(int)key] = down;
            }
        }

        public static bool IsKeyDown(Keycode key)
        {
            if (!_map.ContainsKey(key)) return false;
            return _current[(int)key];
        }

        public static bool IsKeyPressed(Keycode key)
        {
            if (!_map.ContainsKey(key)) return false;
            int idx = (int)key;
            return _current[idx] && !_previous[idx];
        }

        public static bool IsKeyReleased(Keycode key)
        {
            if (!_map.ContainsKey(key)) return false;
            int idx = (int)key;
            return !_current[idx] && _previous[idx];
        }
    }

    public enum Keycode
    {
        Unknown = 0,
        //Alpha
        A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
        //Numeric
        Num0, Num1, Num2, Num3, Num4, Num5, Num6, Num7, Num8, Num9,
        //Symbols
        Backtick, Grave, Tilde, Minus, Plus, Tab, Space, Enter, BSlash, Backspace, OpenSquareBracket, CloseSquareBracket, Semicolon, Quote, LessThan, GreaterThan, FSlash, UpArrow, DownArrow, LeftArrow, RightArrow,
        //All Numpad (Num + Symbols)
        Numpad0, Numpad1, Numpad2, Numpad3, Numpad4, Numpad5, Numpad6, Numpad7, Numpad8, Numpad9, NumpadPeriod, NumpadEnter, NumpadPlus, NumpadMinus, NumpadAstarisk, NumpadFSlash,
        //Modifier Buttons
        LeftShift, RightShift, LeftControl, RightControl, LeftAlt, RightAlt,
        //Special Keys
        F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, Insert, Delete, Home, End, PageUp, PageDown, Escape,
    }


}
