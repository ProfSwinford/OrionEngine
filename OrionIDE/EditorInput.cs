////https://learn.microsoft.com/en-us/dotnet/api/system.windows.input.keyboard?view=windowsdesktop-10.0

//using System;
//using System.Collections.Generic;
//using System.Runtime.InteropServices;
//using System.Windows.Input; //Add "WindowsBase" reference to project


//namespace OrionEngine.EngineModules
//{
//    internal class EditorInput
//    {
//        private readonly IntPtr _hwnd;

//        // Current key state cache.
//        private readonly HashSet<Keycode> _downKeys = new();

//        // Transient per-frame caches.
//        private readonly HashSet<Keycode> _pressedThisFrame = new();
//        private readonly HashSet<Keycode> _releasedThisFrame = new();

//        // Optional event stream for engine systems.
//        public event Action<KeyEvent>? KeyChanged;

//        public EditorInput(IntPtr hwnd)
//        {
//            _hwnd = hwnd;
//        }

//        /// <summary>
//        /// Clears per-frame transient input data.
//        /// Call once at the beginning of each frame.
//        /// </summary>
//        public void BeginFrame()
//        {
//            _pressedThisFrame.Clear();
//            _releasedThisFrame.Clear();
//        }

//        /// <summary>
//        /// Register the keyboard as a raw input device for this window.
//        /// RIDEV_INPUTSINK lets the window continue receiving input even when not focused.
//        /// If you only want focused input, remove that flag.
//        /// </summary>
//        public void RegisterKeyboard(bool receiveInputWhenUnfocused = false)
//        {
//            const ushort HID_USAGE_PAGE_GENERIC = 0x01;
//            const ushort HID_USAGE_GENERIC_KEYBOARD = 0x06;

//            uint flags = receiveInputWhenUnfocused ? RawInputDeviceFlags.RIDEV_INPUTSINK : 0u;

//            RAWINPUTDEVICE[] devices =
//            {
//                new RAWINPUTDEVICE
//                {
//                    usUsagePage = HID_USAGE_PAGE_GENERIC,
//                    usUsage = HID_USAGE_GENERIC_KEYBOARD,
//                    dwFlags = flags,
//                    hwndTarget = _hwnd
//                }
//            };

//            if (!RegisterRawInputDevices(devices, (uint)devices.Length, (uint)Marshal.SizeOf<RAWINPUTDEVICE>()))
//            {
//                throw new InvalidOperationException(
//                    $"RegisterRawInputDevices failed. Win32 Error: {Marshal.GetLastWin32Error()}");
//            }
//        }

//        /// <summary>
//        /// Forward your window messages here from WndProc.
//        /// Returns true if this handler processed the message.
//        /// </summary>
//        public bool ProcessMessage(uint msg, IntPtr wParam, IntPtr lParam)
//        {
//            if (msg != WM_INPUT)
//                return false;

//            HandleRawInput(lParam);
//            return true;
//        }

//        public bool IsDown(Keycode key) => _downKeys.Contains(key);
//        public bool WasPressedThisFrame(Keycode key) => _pressedThisFrame.Contains(key);
//        public bool WasReleasedThisFrame(Keycode key) => _releasedThisFrame.Contains(key);

//        private void HandleRawInput(IntPtr hRawInput)
//        {
//            uint dwSize = 0;

//            // First call asks Windows how large the RAWINPUT block is.
//            uint result = GetRawInputData(
//                hRawInput,
//                RID_INPUT,
//                IntPtr.Zero,
//                ref dwSize,
//                (uint)Marshal.SizeOf<RAWINPUTHEADER>());

//            if (result == 0xFFFFFFFF || dwSize == 0)
//                return;

//            IntPtr buffer = Marshal.AllocHGlobal((int)dwSize);

//            try
//            {
//                result = GetRawInputData(
//                    hRawInput,
//                    RID_INPUT,
//                    buffer,
//                    ref dwSize,
//                    (uint)Marshal.SizeOf<RAWINPUTHEADER>());

//                if (result == 0xFFFFFFFF)
//                    return;

//                RAWINPUT raw = Marshal.PtrToStructure<RAWINPUT>(buffer);

//                if (raw.header.dwType != RawInputType.RIM_TYPEKEYBOARD)
//                    return;

//                RAWKEYBOARD keyboard = raw.data.keyboard;

//                // Ignore fake/overflow keys if desired.
//                if (keyboard.VKey == 255)
//                    return;

//                bool isKeyUp = (keyboard.Flags & RawKeyboardFlags.RI_KEY_BREAK) != 0;
//                bool isKeyDown = !isKeyUp;

//                // For a real engine, prefer mapping scan codes for gameplay bindings
//                // and VKs for UI/editor shortcuts if needed.
//                Keycode key = MapVirtualKeyToKeyCode((int)keyboard.VKey);

//                if (key == Keycode.Unknown)
//                    return;

//                if (isKeyDown)
//                {
//                    bool wasAlreadyDown = _downKeys.Contains(key);
//                    _downKeys.Add(key);

//                    // Only report a "pressed this frame" on the transition.
//                    // This suppresses held-key repeat spam.
//                    if (!wasAlreadyDown)
//                    {
//                        _pressedThisFrame.Add(key);
//                        KeyChanged?.Invoke(new KeyEvent(key, true, false, keyboard.MakeCode, keyboard.Flags));
//                    }
//                    else
//                    {
//                        // Optional repeat event
//                        KeyChanged?.Invoke(new KeyEvent(key, true, true, keyboard.MakeCode, keyboard.Flags));
//                    }
//                }
//                else
//                {
//                    if (_downKeys.Remove(key))
//                    {
//                        _releasedThisFrame.Add(key);
//                    }

//                    KeyChanged?.Invoke(new KeyEvent(key, false, false, keyboard.MakeCode, keyboard.Flags));
//                }
//            }
//            finally
//            {
//                Marshal.FreeHGlobal(buffer);
//            }
//        }

//        private static Keycode MapVirtualKeyToKeyCode(int vk)
//        {
//            return vk switch
//            {
//                0x41 => Keycode.A,
//                0x42 => Keycode.B,
//                0x43 => Keycode.C,
//                0x44 => Keycode.D,
//                0x45 => Keycode.E,
//                0x46 => Keycode.F,
//                0x47 => Keycode.G,
//                0x48 => Keycode.H,
//                0x49 => Keycode.I,
//                0x4A => Keycode.J,
//                0x4B => Keycode.K,
//                0x4C => Keycode.L,
//                0x4D => Keycode.M,
//                0x4E => Keycode.N,
//                0x4F => Keycode.O,
//                0x50 => Keycode.P,
//                0x51 => Keycode.Q,
//                0x52 => Keycode.R,
//                0x53 => Keycode.S,
//                0x54 => Keycode.T,
//                0x55 => Keycode.U,
//                0x56 => Keycode.V,
//                0x57 => Keycode.W,
//                0x58 => Keycode.X,
//                0x59 => Keycode.Y,
//                0x5A => Keycode.Z,

//                0x30 => Keycode.Num0,
//                0x31 => Keycode.Num1,
//                0x32 => Keycode.Num2,
//                0x33 => Keycode.Num3,
//                0x34 => Keycode.Num4,
//                0x35 => Keycode.Num5,
//                0x36 => Keycode.Num6,
//                0x37 => Keycode.Num7,
//                0x38 => Keycode.Num8,
//                0x39 => Keycode.Num9,

//                0x20 => Keycode.Space,
//                0x1B => Keycode.Escape,
//                0x0D => Keycode.Enter,
//                0x09 => Keycode.Tab,
//                0x08 => Keycode.Backspace,

//                0x25 => Keycode.LeftArrow,
//                0x26 => Keycode.UpArrow,
//                0x27 => Keycode.RightArrow,
//                0x28 => Keycode.DownArrow,

//                0x10 => Keycode.LeftShift,
//                0x11 => Keycode.LeftControl,
//                0x12 => Keycode.LeftAlt,

//                0x70 => Keycode.F1,
//                0x71 => Keycode.F2,
//                0x72 => Keycode.F3,
//                0x73 => Keycode.F4,
//                0x74 => Keycode.F5,
//                0x75 => Keycode.F6,
//                0x76 => Keycode.F7,
//                0x77 => Keycode.F8,
//                0x78 => Keycode.F9,
//                0x79 => Keycode.F10,
//                0x7A => Keycode.F11,
//                0x7B => Keycode.F12,

//                _ => Keycode.Unknown
//            };
//        }

//        public readonly struct KeyEvent(
//            Keycode Key,
//            bool IsDown,
//            bool IsRepeat,
//            ushort ScanCode,
//            ushort RawFlags);

//        private const uint WM_INPUT = 0x00FF;
//        private const uint RID_INPUT = 0x10000003;

//        private static class RawInputType
//        {
//            public const uint RIM_TYPEMOUSE = 0;
//            public const uint RIM_TYPEKEYBOARD = 1;
//            public const uint RIM_TYPEHID = 2;
//        }

//        private static class RawInputDeviceFlags
//        {
//            public const uint RIDEV_INPUTSINK = 0x00000100;
//        }

//        private static class RawKeyboardFlags
//        {
//            public const ushort RI_KEY_MAKE = 0x0000;
//            public const ushort RI_KEY_BREAK = 0x0001;
//            public const ushort RI_KEY_E0 = 0x0002;
//            public const ushort RI_KEY_E1 = 0x0004;
//        }

//        [StructLayout(LayoutKind.Sequential)]
//        private struct RAWINPUTDEVICE
//        {
//            public ushort usUsagePage;
//            public ushort usUsage;
//            public uint dwFlags;
//            public IntPtr hwndTarget;
//        }

//        [StructLayout(LayoutKind.Sequential)]
//        private struct RAWINPUTHEADER
//        {
//            public uint dwType;
//            public uint dwSize;
//            public IntPtr hDevice;
//            public IntPtr wParam;
//        }

//        [StructLayout(LayoutKind.Sequential)]
//        private struct RAWINPUT
//        {
//            public RAWINPUTHEADER header;
//            public RAWINPUTDATA data;
//        }

//        [StructLayout(LayoutKind.Explicit)]
//        private struct RAWINPUTDATA
//        {
//            [FieldOffset(0)] public RAWMOUSE mouse;
//            [FieldOffset(0)] public RAWKEYBOARD keyboard;
//            [FieldOffset(0)] public RAWHID hid;
//        }

//        [StructLayout(LayoutKind.Sequential)]
//        private struct RAWMOUSE
//        {
//            public ushort usFlags;
//            public uint ulButtons;
//            public uint ulRawButtons;
//            public int lLastX;
//            public int lLastY;
//            public uint ulExtraInformation;
//        }

//        [StructLayout(LayoutKind.Sequential)]
//        private struct RAWKEYBOARD
//        {
//            public ushort MakeCode;
//            public ushort Flags;
//            public ushort Reserved;
//            public ushort VKey;
//            public uint Message;
//            public uint ExtraInformation;
//        }

//        [StructLayout(LayoutKind.Sequential)]
//        private struct RAWHID
//        {
//            public uint dwSizeHid;
//            public uint dwCount;
//            public IntPtr bRawData;
//        }

//        [DllImport("User32.dll", SetLastError = true)]
//        private static extern bool RegisterRawInputDevices(
//            [In] RAWINPUTDEVICE[] pRawInputDevices,
//            uint uiNumDevices,
//            uint cbSize);

//        [DllImport("User32.dll", SetLastError = true)]
//        private static extern uint GetRawInputData(
//            IntPtr hRawInput,
//            uint uiCommand,
//            IntPtr pData,
//            ref uint pcbSize,
//            uint cbSizeHeader);
//    }

//    public enum Keycode
//    {
//        Unknown = 0,
//        //Alpha
//        A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
//        //Numeric
//        Num0, Num1, Num2, Num3, Num4, Num5, Num6, Num7, Num8, Num9,
//        //Symbols
//        Backtick, Grave, Tilde, Minus, Plus, Tab, Space, Enter, BSlash, Backspace, OpenSquareBracket, CloseSquareBracket, Semicolon, Quote, LessThan, GreaterThan, FSlash, UpArrow, DownArrow, LeftArrow, RightArrow,
//        //All Numpad (Num + Symbols)
//        Numpad0, Numpad1, Numpad2, Numpad3, Numpad4, Numpad5, Numpad6, Numpad7, Numpad8, Numpad9, NumpadPeriod, NumpadEnter, NumpadPlus, NumpadMinus, NumpadAstarisk, NumpadFSlash,
//        //Modifier Buttons
//        LeftShift, RightShift, LeftControl, RightControl, LeftAlt, RightAlt,
//        //Special Keys
//        F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, Insert, Delete, Home, End, PageUp, PageDown, Escape,
//    }
//}
