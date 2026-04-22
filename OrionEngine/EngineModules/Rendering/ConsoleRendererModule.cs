// Derrived from https://github.com/NinovanderMark/ConsoleRenderer
// https://github.com/NinovanderMark/ConsoleRenderer/tree/main/ConsoleRenderer.Examples/Programs
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


namespace OrionEngine.EngineModules.Rendering
{
    public class ConsoleRendererModule
    {
        private static ConsoleCanvas _canvas = new ConsoleCanvas(Console.WindowWidth, Console.WindowHeight);
        public static ConsoleCanvas canvas = _canvas;

        public ConsoleRendererModule() 
        {
            EngineStatistics.disableConsoleOutput = false;

            //canvas = new ConsoleCanvas(Console.WindowWidth, Console.WindowHeight);
            //canvas.CreateBorder();
            //canvas.Render();
        }
    }

    public class ConsoleCanvas
    {
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

        // Add this event
        public event Action? OnRender;

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
            : this(Console.WindowWidth, Console.WindowHeight, interlaced, autoResize)
        {
        }
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
            //ConsoleRendererModule.canvas.Clear();

            //if (_drawBorder)
            //{
            //    ConsoleRendererModule.canvas.CreateBorder();
            //}




            Console.CursorTop = 0;
            Console.CursorLeft = 0;

            // Raise the event after rendering
            OnRender?.Invoke();

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
                        foregroundColor = _pixels[y][x].Foreground;
                    }

                    if (_pixels[y][x].Background != backgroundColor)
                    {
                        Console.BackgroundColor = _pixels[y][x].Background;
                        backgroundColor = _pixels[y][x].Background;
                        backgroundOperations++;
                    }

                    Console.Write(_pixels[y][x].Character);
                    cursorLeft++;

                    _previous[y][x] = _pixels[y][x];

                    // After writing the last character on the bottom right, reposition the cursor to prevent
                    // an unintended newline, which may shift the screen downwards, causing jitter
                    if (cursorLeft == windowWidth && cursorTop == windowHeight - 1)
                    {
                        Console.CursorLeft = 0;
                        Console.CursorTop = 0;
                        cursorLeft = 0;
                        cursorTop = 0;
                    }
                }
            }
            ConsoleRendererModule.canvas.Clear();
            ConsoleRendererModule.canvas.CreateBorder();
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
    }
    
    public struct Pixel
    {
        public char Character;
        public ConsoleColor Foreground;
        public ConsoleColor Background;

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

    //public class Pong
    //{
    //    //private readonly double _framerate;

    //    private int _x;
    //    private int _y;
    //    private int _xVel;
    //    private int _yVel;

    //    //DateTime _previousFrame;

    //    public Pong()
    //    {
    //        _xVel = 1;
    //        _yVel = 1;

    //        //_previousFrame = DateTime.Now;
    //        //_framerate = 1000d / framesPerSecond;

    //        ConsoleRendererModule.canvas.OnRender += this.Tick;
    //    }

    //    public void Tick()
    //    {
    //        //ConsoleRendererModule.canvas.Clear();
    //        //ConsoleRendererModule.canvas.CreateBorder();

    //        //var currentTime = DateTime.Now;
    //        //if ((currentTime - _previousFrame).TotalMilliseconds >= _framerate)
    //        //{
    //            _x += _xVel;
    //            _y += _yVel;

    //            if (_x < 1)
    //            {
    //                _x = 1;
    //                _xVel = 1;
    //            }
    //            else if (_x + 1 >= ConsoleRendererModule.canvas.Width)
    //            {
    //                _x = ConsoleRendererModule.canvas.Width - 2;
    //                _xVel = -1;
    //            }

    //            if (_y < 1)
    //            {
    //                _y = 1;
    //                _yVel = 1;
    //            }
    //            else if (_y + 1 >= ConsoleRendererModule.canvas.Height)
    //            {
    //                _y = ConsoleRendererModule.canvas.Height - 2;
    //                _yVel = -1;
    //            }

    //            //_previousFrame = currentTime;
    //        //}

    //        ConsoleRendererModule.canvas.Set(_x, _y, ConsoleColor.Blue);
    //    }
    //}
}
