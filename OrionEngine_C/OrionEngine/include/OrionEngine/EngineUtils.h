// Converted from the `public static class EngineUtils` in OrionEngine/EngineCore.cs.
// The C# file published these through `global using static OrionEngine.EngineUtils`;
// in C++ they are reached as EngineUtils::Print(...) or via the OrionEngine.h umbrella header.
#pragma once

#include <string>

#include "OrionEngine/Console.h"
#include "OrionEngine/Object.h"

namespace OrionEngine
{
    /// <summary>
    /// Represents a segment of text associated with a specific console color for display purposes.
    /// </summary>
    /// <remarks>Use the ColoredText struct to encapsulate a string and its intended foreground color
    /// when writing to the console. This type can be used to simplify colored output scenarios, such as logging or
    /// highlighting messages in command-line applications.</remarks>
    struct ColoredText
    {
    public:
        ColoredText(const std::string& text, ConsoleColor color);

        const std::string& Text() const { return _text; }
        ConsoleColor Color() const { return _color; }

        /// <summary>Helper to write directly to the console.</summary>
        void Write(char end = '\0') const;

        std::string ToString() const { return _text; }

    private:
        std::string _text;
        ConsoleColor _color;
    };

    /// <summary>
    /// Various utility functions that can be used throughout the engine.
    /// </summary>
    class EngineUtils
    {
    public:
        /// <summary>Equivalent to the C# `Print()` call with a null message: writes a newline.</summary>
        static void Print();
        static void Print(const Object& message, char end = '\0');

        static void WaitForInput();

        static void ConsoleCurrentLineClear();
        static void ConsoleWriteLoadingDots(int count, int dotCount);
    };
}
