#include "OrionEngine/EngineModules/Rendering/ConsoleRendererModule.h"

#include <cmath>
#include <stdexcept>

#include "OrionEngine/EngineStatistics.h"
#include "OrionEngine/Object.h"

namespace OrionEngine
{
    namespace EngineModules
    {
        namespace Rendering
        {
            ConsoleRendererModule::ConsoleRendererModule()
            {
                EngineStatistics::disableConsoleOutput = false;
            }

            ConsoleCanvas& ConsoleRendererModule::canvas()
            {
                static ConsoleCanvas _canvas(Console::WindowWidth(), Console::WindowHeight());
                return _canvas;
            }

            ConsoleCanvas::ConsoleCanvas(int width, int height, bool drawBorder, bool interlaced, bool autoResize)
                : DefaultForegroundColor(Console::ForegroundColor())
                , DefaultBackgroundColor(Console::BackgroundColor())
                , AutoResize(autoResize)
                , Interlaced(interlaced)
                , _drawBorder(drawBorder)
            {
                _width = width;
                _height = height;

                Resize(width, height);
            }

            // NOTE: the C# delegating constructor forwards (width, height, interlaced,
            // autoResize) into the five-parameter overload, so `interlaced` lands on
            // `drawBorder` and `autoResize` on `interlaced`. Preserved verbatim.
            ConsoleCanvas::ConsoleCanvas(bool interlaced, bool autoResize)
                : ConsoleCanvas(Console::WindowWidth(), Console::WindowHeight(), interlaced, autoResize)
            {
            }

            void ConsoleCanvas::AddToGameObjectList(GameObject2D* go)
            {
                gameObjects.push_back(go);
            }

            void ConsoleCanvas::ClearGameObjectList()
            {
                gameObjects.clear();
            }

            void ConsoleCanvas::AddOnRender(const std::function<void()>& callback)
            {
                OnRender.push_back(callback);
            }

            void ConsoleCanvas::ClearOnRender()
            {
                OnRender.clear();
            }

            ConsoleCanvas& ConsoleCanvas::Clear()
            {
                return Fill(_emptyCharacter, DefaultForegroundColor, DefaultBackgroundColor);
            }

            ConsoleCanvas& ConsoleCanvas::Fill(OrionChar character, ConsoleColor foreground, ConsoleColor background)
            {
                for (int y = 0; y < _height; y++)
                {
                    for (int x = 0; x < _width; x++)
                    {
                        Set(x, y, character, foreground, background);
                    }
                }

                return *this;
            }

            ConsoleCanvas& ConsoleCanvas::CreateBorder(std::optional<OrionChar> character)
            {
                return CreateBorder(character, DefaultForegroundColor, DefaultBackgroundColor);
            }

            ConsoleCanvas& ConsoleCanvas::CreateBorder(std::optional<OrionChar> character, ConsoleColor foreground, ConsoleColor background)
            {
                return CreateBorder(0, 0, _width, _height, character, foreground, background);
            }

            ConsoleCanvas& ConsoleCanvas::CreateBorder(int startX, int startY, int width, int height, std::optional<OrionChar> character)
            {
                return CreateBorder(startX, startY, width, height, character, DefaultForegroundColor, DefaultBackgroundColor);
            }

            ConsoleCanvas& ConsoleCanvas::CreateBorder(int startX, int startY, int width, int height, std::optional<OrionChar> character,
                                                       ConsoleColor foreground, ConsoleColor background)
            {
                for (int y = startY; y < startY + height; y++)
                {
                    for (int x = startX; x < startX + width; x++)
                    {
                        if (y != startY && y + 1 != startY + height && x != startX && x + 1 != startX + width)
                        {
                            continue;
                        }

                        OrionChar fallback = U' ';
                        if (y == startY)
                        {
                            if (x == startX)
                            {
                                fallback = U'╔';
                            }
                            else if (x + 1 == startX + width)
                            {
                                fallback = U'╗';
                            }
                            else
                            {
                                fallback = U'═';
                            }
                        }
                        else if (y + 1 == startY + height)
                        {
                            if (x == startX)
                            {
                                fallback = U'╚';
                            }
                            else if (x + 1 == startX + width)
                            {
                                fallback = U'╝';
                            }
                            else
                            {
                                fallback = U'═';
                            }
                        }
                        else if (x == startX || x + 1 == startX + width)
                        {
                            fallback = U'║';
                        }

                        Set(x, y, character.value_or(fallback), foreground, background);
                    }
                }

                return *this;
            }

            ConsoleCanvas& ConsoleCanvas::CreateRectangle(int startX, int startY, int width, int height, OrionChar character)
            {
                return CreateRectangle(startX, startY, width, height, character, DefaultForegroundColor, DefaultBackgroundColor);
            }

            ConsoleCanvas& ConsoleCanvas::CreateRectangle(int startX, int startY, int width, int height, OrionChar character,
                                                          ConsoleColor foreground, ConsoleColor background)
            {
                for (int y = startY; y < _height && y - startY < height; y++)
                {
                    for (int x = startX; x < _width && x - startX < width; x++)
                    {
                        Set(x, y, character, foreground, background);
                    }
                }

                return *this;
            }

            ConsoleCanvas& ConsoleCanvas::Render()
            {
                Console::SetCursorPosition(0, 0);

                // Raise the event after rendering
                for (const std::function<void()>& callback : OnRender)
                {
                    if (callback)
                    {
                        callback();
                    }
                }

                for (GameObject2D* const go : gameObjects)
                {
                    go->DrawImage();
                }

                // Temporary variables to track Console attributes like size, position and color
                int cursorTop = 0;
                int cursorLeft = 0;
                const int windowWidth = Console::WindowWidth();
                const int windowHeight = Console::WindowHeight();
                ConsoleColor foregroundColor = Console::ForegroundColor();
                ConsoleColor backgroundColor = Console::BackgroundColor();

                if (AutoResize)
                {
                    if (_previousWidth != windowWidth || _previousHeight != windowHeight)
                    {
                        Resize(windowWidth, windowHeight);
                    }

                    ClearPixelCache();

                    _previousWidth = windowWidth;
                    _previousHeight = windowHeight;
                }
                ClearPixelCache();

                int leftOperations = 0;
                int backgroundOperations = 0;

                for (int y = 0; y < _height; y++)
                {
                    // See if this is one of the rows we should skip in Interlaced mode
                    if (Interlaced && ((_oddRows && y % 2 == 0) || (!_oddRows && y % 2 != 0)))
                    {
                        continue;
                    }

                    for (int x = 0; x < _width; x++)
                    {
                        if (_pixels[static_cast<size_t>(y)][static_cast<size_t>(x)] ==
                            _previous[static_cast<size_t>(y)][static_cast<size_t>(x)])
                        {
                            continue;
                        }

                        if (x >= windowWidth)
                        {
                            continue;
                        }

                        if (y >= windowHeight)
                        {
                            continue;
                        }

                        if (cursorLeft != x)
                        {
                            try
                            {
                                Console::SetCursorLeft(x);
                            }
                            catch (const ArgumentOutOfRangeException&)
                            {
                                return Render();
                            }

                            cursorLeft = x;
                            leftOperations++;
                        }

                        if (cursorTop != y)
                        {
                            try
                            {
                                Console::SetCursorTop(y);
                            }
                            catch (const ArgumentOutOfRangeException&)
                            {
                                return Render();
                            }

                            cursorTop = y;
                        }

                        const Pixel& pixel = _pixels[static_cast<size_t>(y)][static_cast<size_t>(x)];

                        if (pixel.Character != U' ' && pixel.Foreground != foregroundColor)
                        {
                            Console::SetForegroundColor(pixel.Foreground);
                            foregroundColor = pixel.Foreground;
                        }

                        if (pixel.Background != backgroundColor)
                        {
                            Console::SetBackgroundColor(pixel.Background);
                            backgroundColor = pixel.Background;
                            backgroundOperations++;
                        }

                        Console::Write(pixel.Character);
                        cursorLeft++;

                        _previous[static_cast<size_t>(y)][static_cast<size_t>(x)] = pixel;

                        // After writing the last character on the bottom right, reposition the cursor to prevent
                        // an unintended newline, which may shift the screen downwards, causing jitter
                        if (cursorLeft == windowWidth && cursorTop == windowHeight - 1)
                        {
                            Console::SetCursorPosition(0, 0);
                            cursorLeft = 0;
                            cursorTop = 0;
                        }
                    }
                }

                (void)leftOperations;
                (void)backgroundOperations;

                // Push the completed frame out in one go (the POSIX backend batches writes).
                Console::Flush();

                ConsoleRendererModule::canvas().Clear();

                if (_drawBorder)
                {
                    ConsoleRendererModule::canvas().CreateBorder();
                }

                // Swap whether we render odd or even rows next frame
                _oddRows = !_oddRows;

                return *this;
            }

            ConsoleCanvas& ConsoleCanvas::Resize(int width, int height)
            {
                _width = width;
                _height = height;
                _pixels.clear();
                _previous.clear();

                for (int y = 0; y < _height; y++)
                {
                    std::vector<Pixel> row;
                    std::vector<Pixel> previousRow;
                    for (int x = 0; x < _width; x++)
                    {
                        const Pixel pixel(_emptyCharacter, DefaultForegroundColor, DefaultBackgroundColor);

                        row.push_back(pixel);
                        previousRow.push_back(pixel);
                    }

                    _pixels.push_back(row);
                    _previous.push_back(previousRow);
                }

                return *this;
            }

            ConsoleCanvas& ConsoleCanvas::Set(int x, int y, OrionChar character)
            {
                return Set(x, y, character, DefaultForegroundColor);
            }

            ConsoleCanvas& ConsoleCanvas::Set(int x, int y, ConsoleColor color)
            {
                return Set(x, y, _defaultCharacter, color);
            }

            ConsoleCanvas& ConsoleCanvas::Set(int x, int y, OrionChar character, ConsoleColor color)
            {
                return Set(x, y, character, color, DefaultBackgroundColor);
            }

            ConsoleCanvas& ConsoleCanvas::Set(int x, int y, OrionChar character, ConsoleColor foreground, ConsoleColor background)
            {
                return Set(x, y, Pixel(character, foreground, background));
            }

            ConsoleCanvas& ConsoleCanvas::Set(int x, int y, const Pixel& pixel)
            {
                if (x >= 0 && x < _width && y >= 0 && y < _height)
                {
                    _pixels[static_cast<size_t>(y)][static_cast<size_t>(x)] = pixel;
                }

                return *this;
            }

            ConsoleCanvas& ConsoleCanvas::Set(int x, int y, const std::vector<Pixel>& pixels)
            {
                for (size_t t = 0; t < pixels.size(); t++)
                {
                    Set(x + static_cast<int>(t), y, pixels[t]);
                }

                return *this;
            }

            ConsoleCanvas& ConsoleCanvas::Text(int x, int y, const std::string& text, bool centered,
                                               std::optional<ConsoleColor> foreground, std::optional<ConsoleColor> background)
            {
                const std::u32string characters = Utf8Decode(text);

                // If the text should be centered, deduct half the text length from the x coordinate
                const int startX = centered
                                       ? x - static_cast<int>(std::floor(static_cast<double>(characters.size()) / 2.0))
                                       : x;

                for (size_t t = 0; t < characters.size() && static_cast<int>(t) < _width; t++)
                {
                    Set(startX + static_cast<int>(t), y,
                        Pixel(characters[t],
                              foreground.value_or(DefaultForegroundColor),
                              background.value_or(DefaultBackgroundColor)));
                }

                return *this;
            }

            Pixel ConsoleCanvas::Get(int x, int y, bool backBuffer) const
            {
                if (x < 0 || y < 0 || x >= _width || y >= _height)
                {
                    throw std::out_of_range("The coordinates " + std::to_string(x) + "," + std::to_string(y) +
                                            " need to be positive and less than " + std::to_string(_width) + " and " +
                                            std::to_string(_height));
                }

                return backBuffer ? _previous[static_cast<size_t>(y)][static_cast<size_t>(x)]
                                  : _pixels[static_cast<size_t>(y)][static_cast<size_t>(x)];
            }

            void ConsoleCanvas::ClearPixelCache()
            {
                // U+00A0 is the sentinel used by the C# original: it differs from every
                // real cell, so the next Render redraws the whole canvas.
                const Pixel defaultPixel(U'\u00A0', DefaultForegroundColor, DefaultBackgroundColor);

                for (int y = 0; y < _height; y++)
                {
                    for (int x = 0; x < _width; x++)
                    {
                        _previous[static_cast<size_t>(y)][static_cast<size_t>(x)] = defaultPixel;
                    }
                }
            }
        }
    }
}
