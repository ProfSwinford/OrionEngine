// Derrived from https://github.com/NinovanderMark/ConsoleRenderer
// https://github.com/NinovanderMark/ConsoleRenderer/tree/main/ConsoleRenderer.Examples/Programs
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Drawing;
using System.Windows.Media;

namespace OrionEngine.EngineModules.Rendering
{
    public class ConsoleRendererModule
    {
        private static ConsoleCanvas _canvas = new ConsoleCanvas(Console.WindowWidth, Console.WindowHeight);
        public static ConsoleCanvas canvas = _canvas;

        public ConsoleRendererModule() 
        {
            EngineStatistics.disableConsoleOutput = false;
        }
    }

    public class ConsoleCanvas
    {
        public System.Windows.Controls.RichTextBox formRender;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public ConsoleColor DefaultForegroundColor { get; set; }
        public ConsoleColor DefaultBackgroundColor { get; set; }
        public bool AutoResize { get; set; }
        public bool Interlaced { get; set; }

        private const char _defaultCharacter = '*';
        private const char _emptyCharacter = ' ';

        private int _previousWidth;
        private int _previousHeight;
        private bool _oddRows;
        private bool _drawBorder;
        private List<List<Pixel>> _pixels;
        private List<List<Pixel>> _previous;

        private List<GameObject2D> gameObjects = new List<GameObject2D>();

        // Add this event
        public event Action? OnRender;

        public void AddToGameObjectList(GameObject2D go) 
        {
            gameObjects.Add(go);
        }

        public ConsoleCanvas(int width, int height, bool drawBorder = true, bool interlaced = false, bool autoResize = false)
        {
            Width = width;
            Height = height;
            Interlaced = interlaced;
            AutoResize = autoResize;
            _drawBorder = drawBorder;

            DefaultForegroundColor = Console.ForegroundColor;
            DefaultBackgroundColor = Console.BackgroundColor;

            _pixels = new List<List<Pixel>>();
            _previous = new List<List<Pixel>>();

            Resize(width, height);
        }
        public ConsoleCanvas(bool interlaced = false, bool autoResize = false)
            : this(Console.WindowWidth, Console.WindowHeight, interlaced, autoResize){}
        public ConsoleCanvas Clear()
        {
            return Fill(_emptyCharacter, DefaultForegroundColor, DefaultBackgroundColor);
        }
        public ConsoleCanvas Fill(char character, ConsoleColor foreground, ConsoleColor background)
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    Set(x, y, character, foreground, background);

            return this;
        }
        public ConsoleCanvas CreateBorder(char? character = null)
        {
            return CreateBorder(character, DefaultForegroundColor, DefaultBackgroundColor);
        }
        public ConsoleCanvas CreateBorder(char? character, ConsoleColor foreground, ConsoleColor background)
        {
            return CreateBorder(0, 0, Width, Height, character, foreground, background);
        }
        public ConsoleCanvas CreateBorder(int startX, int startY, int width, int height, char? character = null)
        {
            return CreateBorder(startX, startY, width, height, character, DefaultForegroundColor, DefaultBackgroundColor);
        }
        public ConsoleCanvas CreateBorder(int startX, int startY, int width, int height, char? character, ConsoleColor foreground, ConsoleColor background)
        {
            for (int y = startY; y < startY + height; y++)
            {
                for (int x = startX; x < startX + width; x++)
                {
                    if (y != startY && y + 1 != startY + height && x != startX && x + 1 != startX + width)
                    {
                        continue;
                    }

                    char fallback = ' ';
                    if (y == startY)
                    {
                        if (x == startX)
                        {
                            fallback = '╔';
                        }
                        else if (x + 1 == startX + width)
                        {
                            fallback = '╗';
                        }
                        else
                        {
                            fallback = '═';
                        }

                    }
                    else if (y + 1 == startY + height)
                    {
                        if (x == startX)
                        {
                            fallback = '╚';
                        }
                        else if (x + 1 == startX + width)
                        {
                            fallback = '╝';
                        }
                        else
                        {
                            fallback = '═';
                        }
                    }
                    else if (x == startX || x + 1 == startX + width)
                    {
                        fallback = '║';
                    }

                    Set(x, y, character ?? fallback, foreground, background);
                }
            }

            return this;
        }
        public ConsoleCanvas CreateRectangle(int startX, int startY, int width, int height, char character = _defaultCharacter)
        {
            return CreateRectangle(startX, startY, width, height, character, DefaultForegroundColor, DefaultBackgroundColor);
        }
        public ConsoleCanvas CreateRectangle(int startX, int startY, int width, int height, char character, ConsoleColor foreground, ConsoleColor background)
        {
            for (int y = startY; y < Height && y - startY < height; y++)
            {
                for (int x = startX; x < Width && x - startX < width; x++)
                    Set(x, y, character, foreground, background);
            }

            return this;
        }
        public ConsoleCanvas Render()
        {
            Console.CursorTop = 0;
            Console.CursorLeft = 0;
            

            // Raise the event after rendering
            OnRender?.Invoke();

            foreach (GameObject2D go in gameObjects) 
            {
                go.DrawImage();
            }

            // Temporary variables to track Console attributes like size, position and color
            int cursorTop = 0;
            int cursorLeft = 0;
            int windowWidth = Console.WindowWidth;
            int windowHeight = Console.WindowHeight;
            ConsoleColor foregroundColor = Console.ForegroundColor;
            ConsoleColor backgroundColor = Console.BackgroundColor;
            
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

            for (int y = 0; y < Height; y++)
            {
                // See if this is one of the rows we should skip in Interlaced mode
                if (Interlaced && ((_oddRows && y % 2 == 0) || (!_oddRows && y % 2 != 0)))
                    continue;

                for (int x = 0; x < Width; x++)
                {
                    if (_pixels[y][x] == _previous[y][x])
                        continue;

                    if (x >= windowWidth)
                        continue;

                    if (y >= windowHeight)
                        continue;

                    if (cursorLeft != x)
                    {
                        try
                        {
                            Console.CursorLeft = x;
                            formRender.CaretPosition = formRender.CaretPosition.GetPositionAtOffset(x);
                        }
                        catch (ArgumentOutOfRangeException)
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
                            Console.CursorTop = y;
                            formRender.CaretPosition = formRender.CaretPosition.GetPositionAtOffset(formRender.Document.ContentStart.GetOffsetToPosition(formRender.CaretPosition) + (windowWidth * (y - cursorTop)));
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            return Render();
                        }

                        cursorTop = y;
                    }

                    if (_pixels[y][x].Character != ' ' && _pixels[y][x].Foreground != foregroundColor)
                    {
                        Console.ForegroundColor = _pixels[y][x].Foreground;
                        formRender.Foreground = new SolidColorBrush(MapConsoleColor(_pixels[y][x].Foreground));
                        foregroundColor = _pixels[y][x].Foreground;
                    }

                    if (_pixels[y][x].Background != backgroundColor)
                    {
                        Console.BackgroundColor = _pixels[y][x].Background;
                        formRender.Background = new SolidColorBrush(MapConsoleColor(_pixels[y][x].Background));
                        backgroundColor = _pixels[y][x].Background;
                        backgroundOperations++;
                    }

                    Console.Write(_pixels[y][x].Character);
                    formRender.AppendText(_pixels[y][x].Character.ToString());
                    cursorLeft++;

                    _previous[y][x] = _pixels[y][x];

                    // After writing the last character on the bottom right, reposition the cursor to prevent
                    // an unintended newline, which may shift the screen downwards, causing jitter
                    if (cursorLeft == windowWidth && cursorTop == windowHeight - 1)
                    {
                        Console.CursorLeft = 0;
                        Console.CursorTop = 0;
                        formRender.CaretPosition = formRender.Document.ContentStart;
                        cursorLeft = 0;
                        cursorTop = 0;
                    }
                }
            }

            ConsoleRendererModule.canvas.Clear();

            if (_drawBorder)
            {
                ConsoleRendererModule.canvas.CreateBorder();
            }

            // Swap whether we render odd or even rows next frame
            _oddRows = !_oddRows;

            return this;
        }
        public ConsoleCanvas Resize(int width, int height)
        {
            Width = width;
            Height = height;
            _pixels = new List<List<Pixel>>();
            _previous = new List<List<Pixel>>();

            for (int y = 0; y < Height; y++)
            {
                var row = new List<Pixel>();
                var previousRow = new List<Pixel>();
                for (int x = 0; x < Width; x++)
                {
                    var pixel = new Pixel
                    {
                        Character = _emptyCharacter,
                        Foreground = DefaultForegroundColor,
                        Background = DefaultBackgroundColor
                    };

                    row.Add(pixel);
                    previousRow.Add(pixel);
                }

                _pixels.Add(row);
                _previous.Add(previousRow);
            }

            return this;
        }
        public ConsoleCanvas Set(int x, int y, char character = _defaultCharacter)
        {
            return Set(x, y, character, DefaultForegroundColor);
        }
        public ConsoleCanvas Set(int x, int y, ConsoleColor color)
        {
            return Set(x, y, _defaultCharacter, color);
        }
        public ConsoleCanvas Set(int x, int y, char character, ConsoleColor color)
        {
            return Set(x, y, character, color, DefaultBackgroundColor);
        }
        public ConsoleCanvas Set(int x, int y, char character, ConsoleColor foreground, ConsoleColor background)
        {
            return Set(x, y, new Pixel
            {
                Character = character,
                Foreground = foreground,
                Background = background,
            });
        }
        public ConsoleCanvas Set(int x, int y, Pixel pixel)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
                _pixels[y][x] = pixel;

            return this;
        }
        public ConsoleCanvas Set(int x, int y, Pixel[] pixels)
        {
            for (int t = 0; t < pixels.Length; t++)
            {
                Set(x + t, y, pixels[t]);
            }

            return this;
        }
        public ConsoleCanvas Set(int x, int y, List<Pixel> pixels)
        {
            for (int t = 0; t < pixels.Count; t++)
            {
                Set(x + t, y, pixels[t]);
            }

            return this;
        }
        public ConsoleCanvas Text(int x, int y, string text, bool centered = false, ConsoleColor? foreground = null, ConsoleColor? background = null)
        {
            // If the text should be centered, deduct half the text length from the x coordinate
            int startX = centered ? x - (int)Math.Floor(text.Length / 2d) : x;

            for (int t = 0; t < text.Length && t < Width; t++)
            {
                Set(startX + t, y, new Pixel
                {
                    Character = text[t],
                    Foreground = foreground ?? DefaultForegroundColor,
                    Background = background ?? DefaultBackgroundColor
                });
            }

            return this;
        }
        public Pixel Get(int x, int y, bool backBuffer = true)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
            {
                throw new IndexOutOfRangeException($"The coordinates {x},{y} need to be positive and less than {Width} and {Height}");
            }

            return backBuffer ? _previous[y][x] : _pixels[y][x];
        }
        private void ClearPixelCache()
        {
            var defaultPixel = new Pixel
            {
                Background = DefaultBackgroundColor,
                Foreground = DefaultForegroundColor,
                Character = '\u00A0'
            };

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    _previous[y][x] = defaultPixel;
        }
        private static System.Windows.Media.Color MapConsoleColor(ConsoleColor c)
        {
            // Map ConsoleColor to System.Drawing.Color
            return c switch
            {
                ConsoleColor.Black => System.Windows.Media.Color.FromArgb(System.Drawing.Color.Black.A, System.Drawing.Color.Black.R, System.Drawing.Color.Black.G, System.Drawing.Color.Black.B),
                ConsoleColor.DarkBlue => System.Windows.Media.Color.FromArgb(System.Drawing.Color.DarkBlue.A, System.Drawing.Color.DarkBlue.R, System.Drawing.Color.DarkBlue.G, System.Drawing.Color.DarkBlue.B),
                ConsoleColor.DarkGreen => System.Windows.Media.Color.FromArgb(System.Drawing.Color.DarkGreen.A, System.Drawing.Color.DarkGreen.R, System.Drawing.Color.DarkGreen.G, System.Drawing.Color.DarkGreen.B),
                ConsoleColor.DarkCyan => System.Windows.Media.Color.FromArgb(System.Drawing.Color.DarkCyan.A, System.Drawing.Color.DarkCyan.R, System.Drawing.Color.DarkCyan.G, System.Drawing.Color.DarkCyan.B),
                ConsoleColor.DarkRed => System.Windows.Media.Color.FromArgb(System.Drawing.Color.DarkRed.A, System.Drawing.Color.DarkRed.R, System.Drawing.Color.DarkRed.G, System.Drawing.Color.DarkRed.B),
                ConsoleColor.DarkMagenta => System.Windows.Media.Color.FromArgb(System.Drawing.Color.DarkMagenta.A, System.Drawing.Color.DarkMagenta.R, System.Drawing.Color.DarkMagenta.G, System.Drawing.Color.DarkMagenta.B),
                ConsoleColor.DarkYellow => System.Windows.Media.Color.FromArgb(System.Drawing.Color.Olive.A, System.Drawing.Color.Olive.R, System.Drawing.Color.Olive.G, System.Drawing.Color.Olive.B),
                ConsoleColor.Gray => System.Windows.Media.Color.FromArgb(System.Drawing.Color.Gray.A, System.Drawing.Color.Gray.R, System.Drawing.Color.Gray.G, System.Drawing.Color.Gray.B),
                ConsoleColor.DarkGray => System.Windows.Media.Color.FromArgb(System.Drawing.Color.DimGray.A, System.Drawing.Color.DimGray.R, System.Drawing.Color.DimGray.G, System.Drawing.Color.DimGray.B),
                ConsoleColor.Blue => System.Windows.Media.Color.FromArgb(System.Drawing.Color.Blue.A, System.Drawing.Color.Blue.R, System.Drawing.Color.Blue.G, System.Drawing.Color.Blue.B),
                ConsoleColor.Green => System.Windows.Media.Color.FromArgb(System.Drawing.Color.Lime.A, System.Drawing.Color.Lime.R, System.Drawing.Color.Lime.G, System.Drawing.Color.Lime.B),
                ConsoleColor.Cyan => System.Windows.Media.Color.FromArgb(System.Drawing.Color.Cyan.A, System.Drawing.Color.Cyan.R, System.Drawing.Color.Cyan.G, System.Drawing.Color.Cyan.B),
                ConsoleColor.Red => System.Windows.Media.Color.FromArgb(System.Drawing.Color.Red.A, System.Drawing.Color.Red.R, System.Drawing.Color.Red.G, System.Drawing.Color.Red.B),
                ConsoleColor.Magenta => System.Windows.Media.Color.FromArgb(System.Drawing.Color.Magenta.A, System.Drawing.Color.Magenta.R, System.Drawing.Color.Magenta.G, System.Drawing.Color.Magenta.B),
                ConsoleColor.Yellow => System.Windows.Media.Color.FromArgb(System.Drawing.Color.Yellow.A, System.Drawing.Color.Yellow.R, System.Drawing.Color.Yellow.G, System.Drawing.Color.Yellow.B),
                ConsoleColor.White => System.Windows.Media.Color.FromArgb(System.Drawing.Color.White.A, System.Drawing.Color.White.R, System.Drawing.Color.White.G, System.Drawing.Color.White.B),
                _ => System.Windows.Media.Color.FromArgb(System.Drawing.Color.White.A, System.Drawing.Color.White.R, System.Drawing.Color.White.G, System.Drawing.Color.White.B)
            };
        }
    }
}


namespace OrionEngine
{
    public struct Pixel
    {
        public char Character;
        public ConsoleColor Foreground;
        public ConsoleColor Background;

        public Pixel(char Character = '*', ConsoleColor Foreground = ConsoleColor.White, ConsoleColor Background = ConsoleColor.Black)
        {
            this.Character = Character;
            this.Background = Background;
            this.Foreground = Foreground;
        }
        public static bool operator ==(Pixel p1, Pixel p2)
        {
            return p1.Character == p2.Character &&
                p1.Foreground == p2.Foreground &&
                p1.Background == p2.Background;
        }

        public static bool operator !=(Pixel p1, Pixel p2)
        {
            return p1.Character != p2.Character ||
                p1.Foreground != p2.Foreground ||
                p1.Background != p2.Background;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null)
                return false;

            if (obj is Pixel pixel)
                return pixel == this;

            return false;
        }
    }
}
