// based on the Unity Engine Execution Order: https://docs.unity3d.com/6000.3/Documentation/Manual/execution-order.html

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OrionEngine.EngineUtils;

namespace OrionEngine
{
    //This class will be responsible for managing the overall state of the engine, as well as any necessary initialization and cleanup tasks.
    public class EngineCore
    {
        int lockedFrameRate = 60;
        #region Singleton
        private static readonly EngineCore instance = new EngineCore();
        public static EngineCore OrionEngine => instance;
        static EngineCore() { }
        private EngineCore()
        {
            int frame = 0;
            Debug.Log("Engine Initializng");
            Console.CursorVisible = false;
            while (true)
            {
                frame++;

                ConsoleWriteLoadingDots(frame%6);

                System.Threading.Thread.Sleep(20*(1000/lockedFrameRate));
            }
        }
        #endregion
        public static bool EngineCoreInit() => OrionEngine != null;
    }

    /// <summary>
    /// Various utility functions that can be used throughout the engine, such as logging, debugging, etc.
    /// </summary>
    public static class EngineUtils
    {
        public static void Print(Object message)
        {
            Console.Write(message.ToString());
        }
        public static void Println(Object message)
        {
            Print(message.ToString()+"\n");
        }
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
        public static void ConsoleWriteLoadingDots(int count)
        {
            ConsoleCurrentLineClear();
            Console.Write(String.Concat(Enumerable.Repeat(".", count % 6)));
        }
    }
    /// <summary>
    /// Provides a set of methods and properties for debugging applications. This class offers functionality to help
    /// diagnose issues and monitor application behavior during development.
    /// </summary>
    /// <remarks>The members of the Debug class are typically used only in debug builds and are omitted from
    /// release builds by default. Use this class to write information to the debugger, assert conditions, and perform
    /// other diagnostic tasks during development.</remarks>
    public static class Debug 
    {
        internal static void TimeStamp() 
        {
            Print(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss |\t"));
        }

        public static void Log(Object message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            TimeStamp();
            Println(message.ToString());
            Console.ResetColor();

        }
        public static void LogWarning(Object message)
        {
                Console.ForegroundColor = ConsoleColor.Yellow;
                TimeStamp();
                Println(message.ToString());
                Console.ResetColor();
        }
        public static void LogError(Object message)
        {
                Console.ForegroundColor = ConsoleColor.Red;
                TimeStamp();
                Println(message.ToString());
                Console.ResetColor();
        }
    }

    /// <summary> Engine Control Module
    /// This class will be responsible for managing the execution of the engine cycle, as well as any other necessary
    /// tasks related to the engine's operation. It will also be responsible for managing the various modules that make
    /// up the engine, such as the rendering module, physics module, etc.
    /// </summary>
    class EngineControlModule
    {
        internal void ModuleSelfCheckIn()
        {

        }
    }

    /// <summary>
    /// Behavious class that other scrips will inherit from, allowing them to be attached to game objects and have their own update loops, etc.
    /// From this class, we will subscribe to the engine's update loop allowing us to have a more flexible and modular approach to game development,
    /// as well as allowing us to easily create and manage game objects and their behaviors.
    /// </summary>
    public class OrionBehaviour
    { 
        
    }

}
