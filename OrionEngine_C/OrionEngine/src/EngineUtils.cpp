#include "OrionEngine/EngineUtils.h"

#include <algorithm>

#include "OrionEngine/EngineModules/InputModule.h"
#include "OrionEngine/EngineStatistics.h"

namespace OrionEngine
{
    ColoredText::ColoredText(const std::string& text, ConsoleColor color)
        : _text(text)
        , _color(color)
    {
    }

    void ColoredText::Write(char end) const
    {
        const ConsoleColor prev = Console::ForegroundColor();  // Store current console text color
        Console::SetForegroundColor(_color);                   // Set console text color to the specified color
        EngineUtils::Print(_text, end);
        Console::SetForegroundColor(prev);                     // Restore previous console text color
    }

    void EngineUtils::Print()
    {
        if (EngineStatistics::disableConsoleOutput)
        {
            return;
        }

        Console::Write(std::string("\n"));
        Console::Flush();
    }

    void EngineUtils::Print(const Object& message, char end)
    {
        if (EngineStatistics::disableConsoleOutput)
        {
            return;
        }

        std::string text = message.ToString();
        if (end != '\0')
        {
            text.push_back(end);
        }

        Console::Write(text);
        Console::Flush();
    }

    void EngineUtils::WaitForInput()
    {
        // NOTE: converted as-is from the C# original, which also never pumps the
        // input module inside the loop.
        while (!EngineModules::InputModule::AnyKeyDown())
        {
            Print("continuing...");
            continue;
        }
    }

    void EngineUtils::ConsoleCurrentLineClear()
    {
        const int line = Console::CursorTop();
        // Protect against very small window widths
        const int width = std::max(1, Console::WindowWidth());
        // move back to start of line
        Console::SetCursorPosition(0, line);
        // set all line values to space for the width of the console
        Console::Write(std::string(static_cast<size_t>(width), ' '));
        // move back to start of line again
        Console::SetCursorPosition(0, line);
    }

    void EngineUtils::ConsoleWriteLoadingDots(int count, int dotCount)
    {
        ConsoleCurrentLineClear();
        if (dotCount == 0)
        {
            return;
        }
        Console::Write(std::string(static_cast<size_t>(count % dotCount + 1), '.'));
        Console::Flush();
    }
}
