// Converted from the `namespace OrionEngine { struct Pixel }` block at the bottom
// of OrionEngine/EngineModules/Rendering/ConsoleRendererModule.cs.
#pragma once

#include <vector>

#include "OrionEngine/Console.h"

namespace OrionEngine
{
    struct Pixel
    {
        OrionChar Character;
        ConsoleColor Foreground;
        ConsoleColor Background;

        explicit Pixel(OrionChar Character = U'*',
                       ConsoleColor Foreground = ConsoleColor::White,
                       ConsoleColor Background = ConsoleColor::Black)
            : Character(Character)
            , Foreground(Foreground)
            , Background(Background)
        {
        }

        bool Equals(const Pixel& other) const;
    };

    bool operator==(const Pixel& p1, const Pixel& p2);
    bool operator!=(const Pixel& p1, const Pixel& p2);

    /// <summary>
    /// Stand-in for the C# rectangular array type `Pixel[,]`, which has no direct
    /// C++ equivalent. Indexed as grid[y][x], matching GameObject2D::DrawImage in
    /// the original, and still constructible from a nested initializer list so that
    /// sprite literals convert one-for-one.
    /// </summary>
    using PixelGrid = std::vector<std::vector<Pixel>>;
}
