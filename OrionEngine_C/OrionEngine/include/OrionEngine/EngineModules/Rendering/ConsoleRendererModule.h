// Converted from OrionEngine/EngineModules/Rendering/ConsoleRendererModule.cs.
// Derived from https://github.com/NinovanderMark/ConsoleRenderer
// https://github.com/NinovanderMark/ConsoleRenderer/tree/main/ConsoleRenderer.Examples/Programs
#pragma once

#include <functional>
#include <optional>
#include <string>
#include <vector>

#include "OrionEngine/Console.h"
#include "OrionEngine/Pixel.h"

namespace OrionEngine
{
    class GameObject2D;

    namespace EngineModules
    {
        namespace Rendering
        {
            class ConsoleCanvas;

            class ConsoleRendererModule
            {
            public:
                ConsoleRendererModule();

                /// <summary>
                /// The C# `public static ConsoleCanvas canvas` field. Exposed as a
                /// function so construction is ordered after Console initialisation.
                /// </summary>
                static ConsoleCanvas& canvas();
            };

            class ConsoleCanvas
            {
            public:
                ConsoleCanvas(int width, int height, bool drawBorder = true, bool interlaced = false, bool autoResize = false);
                explicit ConsoleCanvas(bool interlaced = false, bool autoResize = false);

                int Width() const { return _width; }
                int Height() const { return _height; }

                ConsoleColor DefaultForegroundColor = ConsoleColor::Gray;
                ConsoleColor DefaultBackgroundColor = ConsoleColor::Black;
                bool AutoResize = false;
                bool Interlaced = false;

                void AddToGameObjectList(GameObject2D* go);
                /// <summary>Drops the tracked GameObjects during engine shutdown so no
                /// dangling pointers survive the registry teardown. Not in the C# version,
                /// which left this to the garbage collector.</summary>
                void ClearGameObjectList();

                /// <summary>C# `event Action? OnRender`, as a list of subscribers.</summary>
                void AddOnRender(const std::function<void()>& callback);
                void ClearOnRender();

                ConsoleCanvas& Clear();
                ConsoleCanvas& Fill(OrionChar character, ConsoleColor foreground, ConsoleColor background);

                ConsoleCanvas& CreateBorder(std::optional<OrionChar> character = std::nullopt);
                ConsoleCanvas& CreateBorder(std::optional<OrionChar> character, ConsoleColor foreground, ConsoleColor background);
                ConsoleCanvas& CreateBorder(int startX, int startY, int width, int height, std::optional<OrionChar> character = std::nullopt);
                ConsoleCanvas& CreateBorder(int startX, int startY, int width, int height, std::optional<OrionChar> character,
                                            ConsoleColor foreground, ConsoleColor background);

                ConsoleCanvas& CreateRectangle(int startX, int startY, int width, int height, OrionChar character = U'*');
                ConsoleCanvas& CreateRectangle(int startX, int startY, int width, int height, OrionChar character,
                                               ConsoleColor foreground, ConsoleColor background);

                ConsoleCanvas& Render();
                ConsoleCanvas& Resize(int width, int height);

                ConsoleCanvas& Set(int x, int y, OrionChar character = U'*');
                ConsoleCanvas& Set(int x, int y, ConsoleColor color);
                ConsoleCanvas& Set(int x, int y, OrionChar character, ConsoleColor color);
                ConsoleCanvas& Set(int x, int y, OrionChar character, ConsoleColor foreground, ConsoleColor background);
                ConsoleCanvas& Set(int x, int y, const Pixel& pixel);
                ConsoleCanvas& Set(int x, int y, const std::vector<Pixel>& pixels);

                ConsoleCanvas& Text(int x, int y, const std::string& text, bool centered = false,
                                    std::optional<ConsoleColor> foreground = std::nullopt,
                                    std::optional<ConsoleColor> background = std::nullopt);

                Pixel Get(int x, int y, bool backBuffer = true) const;

            private:
                void ClearPixelCache();

                static constexpr OrionChar _defaultCharacter = U'*';
                static constexpr OrionChar _emptyCharacter = U' ';

                int _width = 0;
                int _height = 0;
                int _previousWidth = 0;
                int _previousHeight = 0;
                bool _oddRows = false;
                bool _drawBorder = true;
                PixelGrid _pixels;
                PixelGrid _previous;

                std::vector<GameObject2D*> gameObjects;
                std::vector<std::function<void()>> OnRender;
            };
        }
    }
}
