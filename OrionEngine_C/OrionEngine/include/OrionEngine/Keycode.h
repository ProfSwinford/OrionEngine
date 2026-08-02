// Converted from the `namespace OrionEngine { enum Keycode }` block at the bottom
// of OrionEngine/EngineModules/InputModule.cs.
#pragma once

namespace OrionEngine
{
    enum class Keycode : int
    {
        Unknown = 0,
        // Alpha
        A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
        // Numeric
        Num0, Num1, Num2, Num3, Num4, Num5, Num6, Num7, Num8, Num9,
        // Symbols
        Backtick, Grave, Tilde, Minus, Plus, Tab, Space, Enter, BSlash, Backspace,
        OpenSquareBracket, CloseSquareBracket, Semicolon, Quote, LessThan, GreaterThan,
        FSlash, UpArrow, DownArrow, LeftArrow, RightArrow,
        // All Numpad (Num + Symbols)
        Numpad0, Numpad1, Numpad2, Numpad3, Numpad4, Numpad5, Numpad6, Numpad7, Numpad8,
        Numpad9, NumpadPeriod, NumpadEnter, NumpadPlus, NumpadMinus, NumpadAstarisk, NumpadFSlash,
        // Modifier Buttons
        LeftShift, RightShift, LeftControl, RightControl, LeftAlt, RightAlt,
        // Special Keys
        F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, Insert, Delete, Home, End,
        PageUp, PageDown, Escape,

        /// <summary>
        /// Not part of the C# enum. C++ has no `Enum.GetValues(...).Length`, so this
        /// terminator provides the array size InputModule needs for its key buffers.
        /// </summary>
        Count
    };
}
