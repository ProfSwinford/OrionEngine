// based on the Unity Engine Execution Order: https://docs.unity3d.com/6000.3/Documentation/Manual/execution-order.html

global using static OrionEngine.EngineModules.InputModule;
global using static OrionEngine.EngineUtils;
global using static OrionEngine.EngineStatistics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using OrionEngine.EngineModules.Rendering;

namespace OrionEngine
{
    //This class will be responsible for managing the overall state of the engine, as well as any necessary initialization and cleanup tasks.
    public class EngineCore
    {
        #region Core Singleton
        private static readonly EngineCore instance = new EngineCore();
        public static EngineCore OrionEngine => instance;
        static EngineCore() { }
        private EngineCore()
        {
            InitializeEngine();
        }
        public static bool EngineCoreInit() => OrionEngine != null;
        #endregion

        private static void InitializeEngine()
        {
            //Debug logs for testing the Debug class and ColoredText struct
            //new ColoredText("Welcome to Orion Engine!", ConsoleColor.Cyan).Write('\n');
            //Debug.LogMessage(new ColoredText("Starting Engine Systems:", ConsoleColor.Cyan));
            //Debug.Log("Engine Initializing Logs");
            //Debug.LogWarning("Engine Initializing Warnings");
            //Debug.LogError("Engine Initializing Errors");

            //ConsoleRendererModule renderModule = new ConsoleRendererModule();

            Console.CursorVisible = false;
            Debug.Log("Initializing Core Engine Modules...");
            if (!InitEngineModules())
            {
                throw new Exception("Failed to initialize Engine Core, check log output for details.");
            }
            Debug.Log("Engine Initialized Successfully, press any key to contine...");
            WaitForInput();

            FrameStateModule.RunEngineCycle();

        }

        private static bool InitEngineModules()
        {
            if (!BehaviourControlModule.InitializeBehaviourControlModule())
            {
                throw new Exception("Failed to initialize Behaviour Control Module.");
            }
            Debug.Log("Initializing Engine Frame Cycle...");
            if (!FrameStateModule.InitializeEngineFrameCycle())
            {
                throw new Exception("Failed to initialize Engine Frame Cycle.");
            }
            return true;
        }
    }
    public static class EngineStatistics
    {
        private static int frameCount = 0;
        public static int totalFrames {get => frameCount;}
        public static void IncrementFrameCount() => frameCount++;
        public static int currentFrame => totalFrames + 1;
        public static bool lockFrameRate = true;
        public static float targetFrameRate = 60;
        public static bool disableConsoleOutput = false;
    }

    /// <summary>
    /// Various utility functions that can be used throughout the engine.
    /// </summary>
    public static class EngineUtils
    {
#nullable enable
        public static void Print(object? message = null, char end = char.MinValue)
        {
            if (disableConsoleOutput)
                return;

            if (message == null)
            {
                message = "\n";
            }

            Console.Write((message??"").ToString()+end);
        }

        public static void WaitForInput()
        {
            Console.ReadKey();
        }
#nullable disable

        public static void ConsoleCurrentLineClear()
        {
            int line = Console.CursorTop;
            // Protect against very small window widths
            int width = Math.Max(1, Console.WindowWidth);
            // move back to start of line
            Console.SetCursorPosition(0, line);
            //set all line values to space for the width of the console
            Console.Write(new string(' ', width));
            // move back to start of line again
            Console.SetCursorPosition(0, line);
        }
        public static void ConsoleWriteLoadingDots(int count, int dotCount)
        {
            ConsoleCurrentLineClear();
            Console.Write(String.Concat(Enumerable.Repeat(".", count % dotCount+1)));
        }

        /// <summary>
        /// Represents a segment of text associated with a specific console color for display purposes.
        /// </summary>
        /// <remarks>Use the ColoredText struct to encapsulate a string and its intended foreground color
        /// when writing to the console. This type can be used to simplify colored output scenarios, such as logging or
        /// highlighting messages in command-line applications.</remarks>
        public struct ColoredText
        {
            //Color references: https://learn.microsoft.com/en-us/dotnet/api/system.consolecolor?view=net-10.0
            public string Text { get; }
            public ConsoleColor Color { get; }
            public ColoredText(string text, ConsoleColor color)
            {
                Text = text ?? String.Empty;
                Color = color;
            }
            // Helper to write directly to the console
            public void Write(char end = char.MinValue)
            {
                var prev = Console.ForegroundColor; //Store current console text color
                Console.ForegroundColor = Color; //Set console text color to the specified color
                Print(Text+end);
                Console.ForegroundColor = prev; //restore previous console text color
            }
            public override string ToString() => Text;
        }
    }
    /// <summary>
    /// Provides a set of methods and properties for debugging applications. This class offers functionality to help
    /// diagnose issues and monitor application behavior during development.
    /// </summary>
    /// <remarks>The members of the Debug class are typically used only in debug builds and are omitted from
    /// release builds by default. Use this class to write information to the debugger, assert conditions, and perform
    /// other diagnostic tasks during development.</remarks>

#if DEBUG
    public static class Debug 
    {
        public static bool ENABLE_DEBUG_LOGS = true;

        public enum LogType
        {
            LOG,
            WARNING,
            ERROR,
            MESSAGE,
            NONE
        }

        internal static void DebugOut(LogType type, string message, bool TimeStamp = true, char end = '\n')
        {
            if (!ENABLE_DEBUG_LOGS || disableConsoleOutput)
                return;

            var prev = Console.ForegroundColor; //Store current console text color

            Print($"{(TimeStamp ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ") : "")}[");
            switch (type) 
            {
                case LogType.LOG:
                    new ColoredText("LOG", ConsoleColor.Green).Write();
                    break;
                case LogType.WARNING:
                    new ColoredText("WRN", ConsoleColor.Yellow).Write();
                    break;
                case LogType.ERROR:
                    new ColoredText("ERR", ConsoleColor.Red).Write();
                    break;
                case LogType.MESSAGE:
                default:
                    Print("MSG");
                    break;
            }

            Print($"] |\t{message}", end);
            Console.ForegroundColor = prev; //restore previous console text color

        }
        public static void Log(Object message)
        {
            DebugOut(LogType.LOG, message.ToString());
        }
        public static void LogWarning(Object message)
        {
            DebugOut(LogType.WARNING, message.ToString());
        }
        public static void LogError(Object message)
        {
            DebugOut(LogType.ERROR, message.ToString());
        }
        public static void LogMessage(Object message)
        {
            DebugOut(LogType.MESSAGE, message.ToString());
        }
        public static void LogMessage(ColoredText message)
        {
            DebugOut(LogType.MESSAGE, "", end:char.MinValue);
            message.Write();
            Print();
        }
    }

#endif
}
