#include "OrionEngine/Console.h"

#include <cstdio>
#include <string>

#if defined(_WIN32)
#    ifndef WIN32_LEAN_AND_MEAN
#        define WIN32_LEAN_AND_MEAN
#    endif
#    ifndef NOMINMAX
#        define NOMINMAX
#    endif
#    include <windows.h>
#else
#    include <sys/ioctl.h>
#    include <unistd.h>
#endif

namespace OrionEngine
{
    namespace
    {
        constexpr int kFallbackWidth = 80;
        constexpr int kFallbackHeight = 25;

        ConsoleColor g_foreground = ConsoleColor::Gray;
        ConsoleColor g_background = ConsoleColor::Black;
        bool g_initialized = false;

#if defined(_WIN32)
        HANDLE StdOut()
        {
            static HANDLE handle = GetStdHandle(STD_OUTPUT_HANDLE);
            return handle;
        }

        CONSOLE_SCREEN_BUFFER_INFO ScreenBufferInfo()
        {
            CONSOLE_SCREEN_BUFFER_INFO info{};
            if (!GetConsoleScreenBufferInfo(StdOut(), &info))
            {
                info.srWindow.Left = 0;
                info.srWindow.Top = 0;
                info.srWindow.Right = static_cast<SHORT>(kFallbackWidth - 1);
                info.srWindow.Bottom = static_cast<SHORT>(kFallbackHeight - 1);
                info.dwCursorPosition.X = 0;
                info.dwCursorPosition.Y = 0;
            }
            return info;
        }

        WORD ToAttribute(ConsoleColor foreground, ConsoleColor background)
        {
            // The Win32 attribute nibbles use the same ordering as ConsoleColor.
            return static_cast<WORD>((static_cast<int>(background) << 4) | static_cast<int>(foreground));
        }

        void ApplyColors()
        {
            SetConsoleTextAttribute(StdOut(), ToAttribute(g_foreground, g_background));
        }
#else
        // The POSIX backend batches every escape sequence into this buffer so a
        // full frame reaches the terminal in a single write().
        std::string g_buffer;

        // System.Console cursor coordinates are readable properties. A terminal
        // cannot be queried cheaply, so the position is tracked locally instead.
        int g_cursorLeft = 0;
        int g_cursorTop = 0;
        bool g_cursorVisible = true;

        void Emit(const std::string& text)
        {
            g_buffer += text;
            if (g_buffer.size() >= 32u * 1024u)
            {
                Console::Flush();
            }
        }

        int AnsiColorCode(ConsoleColor color, bool foreground)
        {
            // ConsoleColor orders the low three bits as blue/green/red, while ANSI
            // orders them red/green/blue, so the red and blue bits are swapped.
            const int value = static_cast<int>(color);
            const int bright = (value & 0x08) != 0 ? 1 : 0;
            const int red = (value & 0x04) != 0 ? 1 : 0;
            const int green = (value & 0x02) != 0 ? 1 : 0;
            const int blue = (value & 0x01) != 0 ? 1 : 0;
            const int ansiIndex = (red << 2) | (green << 1) | blue;

            const int base = foreground ? 30 : 40;
            const int brightBase = foreground ? 90 : 100;
            return (bright != 0 ? brightBase : base) + ansiIndex;
        }
#endif

        void EnsureInitialized()
        {
            if (g_initialized)
            {
                return;
            }
            g_initialized = true;

#if defined(_WIN32)
            // Opt into UTF-8 output so the box drawing characters render.
            SetConsoleOutputCP(CP_UTF8);
            const CONSOLE_SCREEN_BUFFER_INFO info = ScreenBufferInfo();
            g_foreground = static_cast<ConsoleColor>(info.wAttributes & 0x0F);
            g_background = static_cast<ConsoleColor>((info.wAttributes >> 4) & 0x0F);
#else
            g_foreground = ConsoleColor::Gray;
            g_background = ConsoleColor::Black;
            Emit("\x1b[0m");
#endif
        }
    }

    namespace Console
    {
        int WindowWidth()
        {
            EnsureInitialized();
#if defined(_WIN32)
            const CONSOLE_SCREEN_BUFFER_INFO info = ScreenBufferInfo();
            const int width = info.srWindow.Right - info.srWindow.Left + 1;
            return width > 0 ? width : kFallbackWidth;
#else
            winsize size{};
            if (ioctl(STDOUT_FILENO, TIOCGWINSZ, &size) == 0 && size.ws_col > 0)
            {
                return static_cast<int>(size.ws_col);
            }
            return kFallbackWidth;
#endif
        }

        int WindowHeight()
        {
            EnsureInitialized();
#if defined(_WIN32)
            const CONSOLE_SCREEN_BUFFER_INFO info = ScreenBufferInfo();
            const int height = info.srWindow.Bottom - info.srWindow.Top + 1;
            return height > 0 ? height : kFallbackHeight;
#else
            winsize size{};
            if (ioctl(STDOUT_FILENO, TIOCGWINSZ, &size) == 0 && size.ws_row > 0)
            {
                return static_cast<int>(size.ws_row);
            }
            return kFallbackHeight;
#endif
        }

        int CursorLeft()
        {
            EnsureInitialized();
#if defined(_WIN32)
            return ScreenBufferInfo().dwCursorPosition.X;
#else
            return g_cursorLeft;
#endif
        }

        int CursorTop()
        {
            EnsureInitialized();
#if defined(_WIN32)
            return ScreenBufferInfo().dwCursorPosition.Y;
#else
            return g_cursorTop;
#endif
        }

        void SetCursorLeft(int left)
        {
            SetCursorPosition(left, CursorTop());
        }

        void SetCursorTop(int top)
        {
            SetCursorPosition(CursorLeft(), top);
        }

        void SetCursorPosition(int left, int top)
        {
            EnsureInitialized();

            if (left < 0 || top < 0 || left >= WindowWidth() || top >= WindowHeight())
            {
                throw ArgumentOutOfRangeException(
                    "Cursor position (" + std::to_string(left) + ", " + std::to_string(top) + ") is outside the console buffer.");
            }

#if defined(_WIN32)
            COORD position{};
            position.X = static_cast<SHORT>(left);
            position.Y = static_cast<SHORT>(top);
            SetConsoleCursorPosition(StdOut(), position);
#else
            Emit("\x1b[" + std::to_string(top + 1) + ";" + std::to_string(left + 1) + "H");
            g_cursorLeft = left;
            g_cursorTop = top;
#endif
        }

        void SetCursorVisible(bool visible)
        {
            EnsureInitialized();
#if defined(_WIN32)
            CONSOLE_CURSOR_INFO info{};
            if (GetConsoleCursorInfo(StdOut(), &info))
            {
                info.bVisible = visible ? TRUE : FALSE;
                SetConsoleCursorInfo(StdOut(), &info);
            }
#else
            g_cursorVisible = visible;
            Emit(visible ? "\x1b[?25h" : "\x1b[?25l");
            Flush();
#endif
        }

        ConsoleColor ForegroundColor()
        {
            EnsureInitialized();
            return g_foreground;
        }

        ConsoleColor BackgroundColor()
        {
            EnsureInitialized();
            return g_background;
        }

        void SetForegroundColor(ConsoleColor color)
        {
            EnsureInitialized();
            g_foreground = color;
#if defined(_WIN32)
            ApplyColors();
#else
            Emit("\x1b[" + std::to_string(AnsiColorCode(color, true)) + "m");
#endif
        }

        void SetBackgroundColor(ConsoleColor color)
        {
            EnsureInitialized();
            g_background = color;
#if defined(_WIN32)
            ApplyColors();
#else
            Emit("\x1b[" + std::to_string(AnsiColorCode(color, false)) + "m");
#endif
        }

        void ResetColor()
        {
            EnsureInitialized();
            g_foreground = ConsoleColor::Gray;
            g_background = ConsoleColor::Black;
#if defined(_WIN32)
            ApplyColors();
#else
            Emit("\x1b[0m");
#endif
        }

        void Write(const std::string& utf8Text)
        {
            if (utf8Text.empty())
            {
                return;
            }

            EnsureInitialized();

#if defined(_WIN32)
            const int wideLength = MultiByteToWideChar(CP_UTF8, 0, utf8Text.c_str(), static_cast<int>(utf8Text.size()), nullptr, 0);
            if (wideLength <= 0)
            {
                return;
            }

            std::wstring wide(static_cast<size_t>(wideLength), L'\0');
            MultiByteToWideChar(CP_UTF8, 0, utf8Text.c_str(), static_cast<int>(utf8Text.size()), wide.data(), wideLength);

            DWORD written = 0;
            WriteConsoleW(StdOut(), wide.c_str(), static_cast<DWORD>(wide.size()), &written, nullptr);
#else
            Emit(utf8Text);
            if (utf8Text.find('\n') != std::string::npos)
            {
                Flush();
            }
            else
            {
                // Keep the tracked column in step with plain text output.
                g_cursorLeft += static_cast<int>(Utf8Decode(utf8Text).size());
            }
#endif
        }

        void Write(OrionChar character)
        {
            Write(Utf8Encode(character));
        }

        void Write(char character)
        {
            Write(Utf8Encode(static_cast<OrionChar>(static_cast<unsigned char>(character))));
        }

        void Flush()
        {
#if defined(_WIN32)
            // Win32 console writes are unbuffered.
#else
            if (!g_buffer.empty())
            {
                const std::string payload = g_buffer;
                g_buffer.clear();
                ::fwrite(payload.data(), 1, payload.size(), stdout);
            }
            ::fflush(stdout);
#endif
        }

        void Shutdown()
        {
            if (!g_initialized)
            {
                return;
            }

            ResetColor();
            SetCursorVisible(true);
            Flush();
        }
    }

    std::u32string Utf8Decode(const std::string& utf8Text)
    {
        std::u32string result;
        result.reserve(utf8Text.size());

        size_t index = 0;
        while (index < utf8Text.size())
        {
            const auto lead = static_cast<unsigned char>(utf8Text[index]);
            char32_t codePoint = 0;
            size_t extraBytes = 0;

            if (lead < 0x80u)
            {
                codePoint = lead;
            }
            else if ((lead & 0xE0u) == 0xC0u)
            {
                codePoint = lead & 0x1Fu;
                extraBytes = 1;
            }
            else if ((lead & 0xF0u) == 0xE0u)
            {
                codePoint = lead & 0x0Fu;
                extraBytes = 2;
            }
            else if ((lead & 0xF8u) == 0xF0u)
            {
                codePoint = lead & 0x07u;
                extraBytes = 3;
            }
            else
            {
                // Invalid lead byte: emit the replacement character and resynchronize.
                result.push_back(U'�');
                ++index;
                continue;
            }

            if (index + extraBytes >= utf8Text.size() && extraBytes > 0)
            {
                result.push_back(U'�');
                break;
            }

            for (size_t offset = 1; offset <= extraBytes; ++offset)
            {
                const auto continuation = static_cast<unsigned char>(utf8Text[index + offset]);
                if ((continuation & 0xC0u) != 0x80u)
                {
                    codePoint = U'�';
                    break;
                }
                codePoint = (codePoint << 6) | (continuation & 0x3Fu);
            }

            result.push_back(codePoint);
            index += extraBytes + 1;
        }

        return result;
    }

    std::string Utf8Encode(OrionChar character)
    {
        std::string result;
        const auto value = static_cast<char32_t>(character);

        if (value < 0x80u)
        {
            result.push_back(static_cast<char>(value));
        }
        else if (value < 0x800u)
        {
            result.push_back(static_cast<char>(0xC0u | (value >> 6)));
            result.push_back(static_cast<char>(0x80u | (value & 0x3Fu)));
        }
        else if (value < 0x10000u)
        {
            result.push_back(static_cast<char>(0xE0u | (value >> 12)));
            result.push_back(static_cast<char>(0x80u | ((value >> 6) & 0x3Fu)));
            result.push_back(static_cast<char>(0x80u | (value & 0x3Fu)));
        }
        else
        {
            result.push_back(static_cast<char>(0xF0u | (value >> 18)));
            result.push_back(static_cast<char>(0x80u | ((value >> 12) & 0x3Fu)));
            result.push_back(static_cast<char>(0x80u | ((value >> 6) & 0x3Fu)));
            result.push_back(static_cast<char>(0x80u | (value & 0x3Fu)));
        }

        return result;
    }

    std::string Utf8Encode(const std::u32string& text)
    {
        std::string result;
        result.reserve(text.size());
        for (const char32_t character : text)
        {
            result += Utf8Encode(character);
        }
        return result;
    }
}
