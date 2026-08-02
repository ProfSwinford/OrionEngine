#include "OrionEngine/EngineModules/InputModule.h"

#include <array>
#include <chrono>
#include <cstdlib>
#include <unordered_map>
#include <vector>

#if defined(_WIN32)
#    ifndef WIN32_LEAN_AND_MEAN
#        define WIN32_LEAN_AND_MEAN
#    endif
#    ifndef NOMINMAX
#        define NOMINMAX
#    endif
#    include <windows.h>
#else
#    include <termios.h>
#    include <unistd.h>
#endif

namespace OrionEngine
{
    namespace EngineModules
    {
        namespace
        {
            constexpr size_t kKeyCount = static_cast<size_t>(Keycode::Count);

            std::array<bool, kKeyCount> _current{};
            std::array<bool, kKeyCount> _previous{};

            /// <summary>
            /// Keycode to Windows virtual-key mapping. On POSIX the values are unused,
            /// but the map is still the authority on which keys the module recognises
            /// (the C# code guarded every query with `_map.ContainsKey(key)`).
            /// </summary>
            const std::unordered_map<Keycode, int>& KeyMap()
            {
                static const std::unordered_map<Keycode, int> map = []
                {
                    std::unordered_map<Keycode, int> m;

                    // Letters
                    for (int i = 0; i < 26; i++)
                    {
                        m[static_cast<Keycode>(1 + i)] = 0x41 + i;  // 'A'..'Z'
                    }

                    // Numbers (top row)
                    m[Keycode::Num0] = 0x30;
                    m[Keycode::Num1] = 0x31;
                    m[Keycode::Num2] = 0x32;
                    m[Keycode::Num3] = 0x33;
                    m[Keycode::Num4] = 0x34;
                    m[Keycode::Num5] = 0x35;
                    m[Keycode::Num6] = 0x36;
                    m[Keycode::Num7] = 0x37;
                    m[Keycode::Num8] = 0x38;
                    m[Keycode::Num9] = 0x39;

                    // Symbols / punctuation (common US virtual-key codes)
                    m[Keycode::Backtick] = 0xC0;  // VK_OEM_3
                    m[Keycode::Grave] = 0xC0;
                    m[Keycode::Tilde] = 0xC0;
                    m[Keycode::Minus] = 0xBD;  // VK_OEM_MINUS
                    m[Keycode::Plus] = 0xBB;   // VK_OEM_PLUS
                    m[Keycode::Tab] = 0x09;    // VK_TAB
                    m[Keycode::Space] = 0x20;  // VK_SPACE
                    m[Keycode::Enter] = 0x0D;  // VK_RETURN
                    m[Keycode::BSlash] = 0xDC;    // VK_OEM_5
                    m[Keycode::Backspace] = 0x08; // VK_BACK
                    m[Keycode::OpenSquareBracket] = 0xDB;  // VK_OEM_4
                    m[Keycode::CloseSquareBracket] = 0xDD; // VK_OEM_6
                    m[Keycode::Semicolon] = 0xBA;  // VK_OEM_1
                    m[Keycode::Quote] = 0xDE;      // VK_OEM_7
                    m[Keycode::LessThan] = 0xE2;   // VK_OEM_102 (may vary by layout)
                    m[Keycode::GreaterThan] = 0xE2;
                    m[Keycode::FSlash] = 0xBF;  // VK_OEM_2 '/'

                    // Arrows
                    m[Keycode::UpArrow] = 0x26;     // VK_UP
                    m[Keycode::DownArrow] = 0x28;   // VK_DOWN
                    m[Keycode::LeftArrow] = 0x25;   // VK_LEFT
                    m[Keycode::RightArrow] = 0x27;  // VK_RIGHT

                    // Numpad
                    m[Keycode::Numpad0] = 0x60;  // VK_NUMPAD0
                    m[Keycode::Numpad1] = 0x61;
                    m[Keycode::Numpad2] = 0x62;
                    m[Keycode::Numpad3] = 0x63;
                    m[Keycode::Numpad4] = 0x64;
                    m[Keycode::Numpad5] = 0x65;
                    m[Keycode::Numpad6] = 0x66;
                    m[Keycode::Numpad7] = 0x67;
                    m[Keycode::Numpad8] = 0x68;
                    m[Keycode::Numpad9] = 0x69;
                    m[Keycode::NumpadPeriod] = 0x6E;  // VK_DECIMAL
                    m[Keycode::NumpadEnter] = 0x0D;   // VK_RETURN (numpad ENTER reported as RETURN)
                    m[Keycode::NumpadPlus] = 0x6B;    // VK_ADD
                    m[Keycode::NumpadMinus] = 0x6D;   // VK_SUBTRACT
                    m[Keycode::NumpadAstarisk] = 0x6A;  // VK_MULTIPLY
                    m[Keycode::NumpadFSlash] = 0x6F;    // VK_DIVIDE

                    // Modifiers
                    m[Keycode::LeftShift] = 0xA0;     // VK_LSHIFT
                    m[Keycode::RightShift] = 0xA1;    // VK_RSHIFT
                    m[Keycode::LeftControl] = 0xA2;   // VK_LCONTROL
                    m[Keycode::RightControl] = 0xA3;  // VK_RCONTROL
                    m[Keycode::LeftAlt] = 0xA4;       // VK_LMENU
                    m[Keycode::RightAlt] = 0xA5;      // VK_RMENU

                    // Function keys and specials
                    m[Keycode::F1] = 0x70;  m[Keycode::F2] = 0x71;  m[Keycode::F3] = 0x72;  m[Keycode::F4] = 0x73;
                    m[Keycode::F5] = 0x74;  m[Keycode::F6] = 0x75;  m[Keycode::F7] = 0x76;  m[Keycode::F8] = 0x77;
                    m[Keycode::F9] = 0x78;  m[Keycode::F10] = 0x79; m[Keycode::F11] = 0x7A; m[Keycode::F12] = 0x7B;
                    m[Keycode::Insert] = 0x2D;    // VK_INSERT
                    m[Keycode::Delete] = 0x2E;    // VK_DELETE
                    m[Keycode::Home] = 0x24;      // VK_HOME
                    m[Keycode::End] = 0x23;       // VK_END
                    m[Keycode::PageUp] = 0x21;    // VK_PRIOR
                    m[Keycode::PageDown] = 0x22;  // VK_NEXT
                    m[Keycode::Escape] = 0x1B;    // VK_ESCAPE

                    return m;
                }();

                return map;
            }

#if !defined(_WIN32)
            // --- POSIX backend -------------------------------------------------
            //
            // A terminal delivers keystrokes, not key up/down transitions, so a key
            // is reported as held for a short window after its most recent byte
            // arrives. Terminal auto-repeat keeps that window refreshed while the
            // key is physically down.
            constexpr auto kKeyHoldDuration = std::chrono::milliseconds(350);

            using Clock = std::chrono::steady_clock;

            std::array<Clock::time_point, kKeyCount> _lastPressed{};
            termios _originalTermios{};
            bool _rawModeEnabled = false;

            void RestoreTerminal()
            {
                if (!_rawModeEnabled)
                {
                    return;
                }

                tcsetattr(STDIN_FILENO, TCSAFLUSH, &_originalTermios);
                _rawModeEnabled = false;
            }

            void EnableRawMode()
            {
                if (_rawModeEnabled || isatty(STDIN_FILENO) == 0)
                {
                    return;
                }

                if (tcgetattr(STDIN_FILENO, &_originalTermios) != 0)
                {
                    return;
                }

                termios raw = _originalTermios;
                raw.c_lflag &= static_cast<tcflag_t>(~(ICANON | ECHO));
                raw.c_cc[VMIN] = 0;   // non-blocking reads
                raw.c_cc[VTIME] = 0;

                if (tcsetattr(STDIN_FILENO, TCSAFLUSH, &raw) != 0)
                {
                    return;
                }

                _rawModeEnabled = true;
                std::atexit(&RestoreTerminal);
            }

            void MarkPressed(Keycode key)
            {
                if (key == Keycode::Unknown)
                {
                    return;
                }
                _lastPressed[static_cast<size_t>(key)] = Clock::now();
            }

            /// <summary>Maps a single printable byte onto a Keycode, flagging shift where implied.</summary>
            void TranslateByte(unsigned char byte)
            {
                if (byte >= 'a' && byte <= 'z')
                {
                    MarkPressed(static_cast<Keycode>(static_cast<int>(Keycode::A) + (byte - 'a')));
                    return;
                }
                if (byte >= 'A' && byte <= 'Z')
                {
                    MarkPressed(static_cast<Keycode>(static_cast<int>(Keycode::A) + (byte - 'A')));
                    MarkPressed(Keycode::LeftShift);
                    return;
                }
                if (byte >= '0' && byte <= '9')
                {
                    MarkPressed(static_cast<Keycode>(static_cast<int>(Keycode::Num0) + (byte - '0')));
                    return;
                }

                switch (byte)
                {
                    case ' ':  MarkPressed(Keycode::Space); break;
                    case '\r':
                    case '\n': MarkPressed(Keycode::Enter); break;
                    case '\t': MarkPressed(Keycode::Tab); break;
                    case 0x7F:
                    case 0x08: MarkPressed(Keycode::Backspace); break;
                    case '`':  MarkPressed(Keycode::Backtick); MarkPressed(Keycode::Grave); break;
                    case '~':  MarkPressed(Keycode::Tilde); MarkPressed(Keycode::LeftShift); break;
                    case '-':  MarkPressed(Keycode::Minus); break;
                    case '+':  MarkPressed(Keycode::Plus); break;
                    case '\\': MarkPressed(Keycode::BSlash); break;
                    case '[':  MarkPressed(Keycode::OpenSquareBracket); break;
                    case ']':  MarkPressed(Keycode::CloseSquareBracket); break;
                    case ';':  MarkPressed(Keycode::Semicolon); break;
                    case '\'': MarkPressed(Keycode::Quote); break;
                    case '<':  MarkPressed(Keycode::LessThan); break;
                    case '>':  MarkPressed(Keycode::GreaterThan); break;
                    case '/':  MarkPressed(Keycode::FSlash); break;
                    case '*':  MarkPressed(Keycode::NumpadAstarisk); break;
                    case '.':  MarkPressed(Keycode::NumpadPeriod); break;
                    default: break;
                }
            }

            /// <summary>Decodes one CSI/SS3 escape sequence, returning how many bytes it consumed.</summary>
            size_t TranslateEscapeSequence(const std::vector<unsigned char>& bytes, size_t index)
            {
                const size_t remaining = bytes.size() - index;

                if (remaining >= 3 && bytes[index + 1] == '[')
                {
                    switch (bytes[index + 2])
                    {
                        case 'A': MarkPressed(Keycode::UpArrow); return 3;
                        case 'B': MarkPressed(Keycode::DownArrow); return 3;
                        case 'C': MarkPressed(Keycode::RightArrow); return 3;
                        case 'D': MarkPressed(Keycode::LeftArrow); return 3;
                        case 'H': MarkPressed(Keycode::Home); return 3;
                        case 'F': MarkPressed(Keycode::End); return 3;
                        default: break;
                    }

                    if (remaining >= 4 && bytes[index + 3] == '~')
                    {
                        switch (bytes[index + 2])
                        {
                            case '1': MarkPressed(Keycode::Home); return 4;
                            case '2': MarkPressed(Keycode::Insert); return 4;
                            case '3': MarkPressed(Keycode::Delete); return 4;
                            case '4': MarkPressed(Keycode::End); return 4;
                            case '5': MarkPressed(Keycode::PageUp); return 4;
                            case '6': MarkPressed(Keycode::PageDown); return 4;
                            default: break;
                        }
                    }

                    if (remaining >= 5 && bytes[index + 4] == '~')
                    {
                        const int code = (bytes[index + 2] - '0') * 10 + (bytes[index + 3] - '0');
                        switch (code)
                        {
                            case 15: MarkPressed(Keycode::F5); return 5;
                            case 17: MarkPressed(Keycode::F6); return 5;
                            case 18: MarkPressed(Keycode::F7); return 5;
                            case 19: MarkPressed(Keycode::F8); return 5;
                            case 20: MarkPressed(Keycode::F9); return 5;
                            case 21: MarkPressed(Keycode::F10); return 5;
                            case 23: MarkPressed(Keycode::F11); return 5;
                            case 24: MarkPressed(Keycode::F12); return 5;
                            default: break;
                        }
                    }

                    return 3;  // Unrecognised CSI sequence: skip the introducer.
                }

                if (remaining >= 3 && bytes[index + 1] == 'O')
                {
                    switch (bytes[index + 2])
                    {
                        case 'P': MarkPressed(Keycode::F1); return 3;
                        case 'Q': MarkPressed(Keycode::F2); return 3;
                        case 'R': MarkPressed(Keycode::F3); return 3;
                        case 'S': MarkPressed(Keycode::F4); return 3;
                        default: return 3;
                    }
                }

                // A lone ESC is the Escape key.
                MarkPressed(Keycode::Escape);
                return 1;
            }
#endif
        }

        void InputModule::Update()
        {
            // copy current to previous
            _previous = _current;

#if defined(_WIN32)
            for (const auto& kv : KeyMap())
            {
                const Keycode key = kv.first;
                const int vk = kv.second;
                const short state = GetAsyncKeyState(vk);
                const bool down = (state & 0x8000) != 0;
                _current[static_cast<size_t>(key)] = down;
            }
#else
            EnableRawMode();

            std::vector<unsigned char> bytes;
            unsigned char chunk[64];
            ssize_t read_count = 0;
            while ((read_count = ::read(STDIN_FILENO, chunk, sizeof(chunk))) > 0)
            {
                bytes.insert(bytes.end(), chunk, chunk + read_count);
                if (static_cast<size_t>(read_count) < sizeof(chunk))
                {
                    break;
                }
            }

            for (size_t index = 0; index < bytes.size();)
            {
                if (bytes[index] == 0x1B)
                {
                    index += TranslateEscapeSequence(bytes, index);
                }
                else
                {
                    TranslateByte(bytes[index]);
                    ++index;
                }
            }

            const auto now = Clock::now();
            for (const auto& kv : KeyMap())
            {
                const size_t index = static_cast<size_t>(kv.first);
                const auto pressedAt = _lastPressed[index];
                _current[index] = pressedAt.time_since_epoch().count() != 0 && (now - pressedAt) <= kKeyHoldDuration;
            }
#endif
        }

        bool InputModule::GetKey(Keycode key)
        {
            if (KeyMap().find(key) == KeyMap().end())
            {
                return false;
            }
            return _current[static_cast<size_t>(key)];
        }

        bool InputModule::AnyKeyDown()
        {
            for (const auto& kv : KeyMap())
            {
                if (GetKeyDown(kv.first))
                {
                    return true;
                }
            }
            return false;
        }

        bool InputModule::GetKeyDown(Keycode key)
        {
            if (KeyMap().find(key) == KeyMap().end())
            {
                return false;
            }
            const size_t idx = static_cast<size_t>(key);
            return _current[idx] && !_previous[idx];
        }

        bool InputModule::GetKeyUp(Keycode key)
        {
            if (KeyMap().find(key) == KeyMap().end())
            {
                return false;
            }
            const size_t idx = static_cast<size_t>(key);
            return !_current[idx] && _previous[idx];
        }

        void InputModule::Shutdown()
        {
#if !defined(_WIN32)
            RestoreTerminal();
#endif
        }
    }
}
