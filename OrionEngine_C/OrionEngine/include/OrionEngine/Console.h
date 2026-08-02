// C++ replacement for the .NET `System.Console` API used throughout the original
// C# OrionEngine. The C# engine leaned on System.Console for cursor control,
// colors and window metrics; none of that exists in the C++ standard library, so
// this header provides the same surface on top of the Win32 console API
// (Windows) and ANSI escape sequences + termios (POSIX).
#pragma once

#include <stdexcept>
#include <string>

namespace OrionEngine
{
    /// <summary>
    /// Direct equivalent of System.ConsoleColor. The numeric values match the .NET
    /// enum so that casts like `(ConsoleColor)(color % 15) + 1` behave identically.
    /// </summary>
    enum class ConsoleColor : int
    {
        Black = 0,
        DarkBlue = 1,
        DarkGreen = 2,
        DarkCyan = 3,
        DarkRed = 4,
        DarkMagenta = 5,
        DarkYellow = 6,
        Gray = 7,
        DarkGray = 8,
        Blue = 9,
        Green = 10,
        Cyan = 11,
        Red = 12,
        Magenta = 13,
        Yellow = 14,
        White = 15,
    };

    /// <summary>
    /// Thrown by the cursor setters when a coordinate falls outside the console
    /// buffer, mirroring System.ArgumentOutOfRangeException. ConsoleCanvas::Render
    /// catches this exactly like the C# renderer does.
    /// </summary>
    class ArgumentOutOfRangeException : public std::out_of_range
    {
    public:
        explicit ArgumentOutOfRangeException(const std::string& message)
            : std::out_of_range(message)
        {
        }
    };

    /// <summary>
    /// The character type stored in a Pixel. C# `char` is UTF-16; char32_t is used
    /// here so that the box drawing characters used by ConsoleCanvas::CreateBorder
    /// survive the round trip on every platform.
    /// </summary>
    using OrionChar = char32_t;

    namespace Console
    {
        // --- Window metrics (System.Console.WindowWidth / WindowHeight) ---
        int WindowWidth();
        int WindowHeight();

        // --- Cursor (System.Console.CursorLeft / CursorTop / CursorVisible) ---
        int CursorLeft();
        int CursorTop();
        void SetCursorLeft(int left);
        void SetCursorTop(int top);
        void SetCursorPosition(int left, int top);
        void SetCursorVisible(bool visible);

        // --- Colors (System.Console.ForegroundColor / BackgroundColor) ---
        ConsoleColor ForegroundColor();
        ConsoleColor BackgroundColor();
        void SetForegroundColor(ConsoleColor color);
        void SetBackgroundColor(ConsoleColor color);
        void ResetColor();

        // --- Output (System.Console.Write) ---
        void Write(const std::string& utf8Text);
        void Write(OrionChar character);
        void Write(char character);

        /// <summary>
        /// Pushes any buffered output to the terminal. The POSIX backend batches
        /// escape sequences for speed, so a flush is required before the frame is
        /// visible. Called automatically at the end of ConsoleCanvas::Render.
        /// </summary>
        void Flush();

        /// <summary>
        /// Restores the terminal to the state it had at start-up (colors, cursor
        /// visibility, and raw-mode settings owned by InputModule).
        /// </summary>
        void Shutdown();
    }

    // --- UTF-8 helpers, used wherever the C# code indexed into a string by char ---
    std::u32string Utf8Decode(const std::string& utf8Text);
    std::string Utf8Encode(OrionChar character);
    std::string Utf8Encode(const std::u32string& text);
}
