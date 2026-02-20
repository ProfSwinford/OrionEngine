// based on the Unity Engine Execution Order: https://docs.unity3d.com/6000.3/Documentation/Manual/execution-order.html

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using static OrionEngine.EngineUtils;
using static OrionEngine.EngineStatistics;

namespace OrionEngine
{
    //This class will be responsible for managing the overall state of the engine, as well as any necessary initialization and cleanup tasks.
    public class EngineCore
    {
        
        int loadingDotCount = 5;

        #region Singleton
        private static readonly EngineCore instance = new EngineCore();
        public static EngineCore OrionEngine => instance;
        static EngineCore() { }
        private EngineCore()
        {
            new ColoredText("Welcome to Orion Engine!", ConsoleColor.Cyan).Write('\n');
            Debug.LogMessage(new ColoredText("Starting Engine Systems:", ConsoleColor.Cyan));
            Debug.Log("Engine Initializing Logs");
            Debug.LogWarning("Engine Initializing Warnings");
            Debug.LogError("Engine Initializing Errors");
            
            Console.CursorVisible = false;

            BehaviourControlModule.InitializeBehaviourControlModule();
            EngineFrameCycle.InitializeEngineFrameCycle();
        }
        #endregion

        public static bool EngineCoreInit() => OrionEngine != null;
    }
    public static class EngineStatistics
    {
        private static int frameCount = 0;
        public static int totalFrames {get => frameCount;}
        public static void IncrementFrameCount() => frameCount++;
        public static int currentFrame => totalFrames + 1;
        public static bool lockFrameRate = true;
        public static float targetFrameRate = 60;
    }

    /// <summary>
    /// Various utility functions that can be used throughout the engine.
    /// </summary>
    public static class EngineUtils
    {
#nullable enable
        public static void Print(object? message = null, char end = char.MinValue)
        {
            if (message == null)
            {
                message = "\n";
            }

            Console.Write((message??"").ToString()+end);
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
    /// <summary> Engine Control Module
    /// This class will be responsible for managing the execution of the engine cycle, as well as any other necessary
    /// tasks related to the engine's operation. It will also be responsible for managing the various modules that make
    /// up the engine, such as the rendering module, physics module, etc.
    /// </summary>
    public class BehaviourControlModule
    {
        #region BCM Singleton
        private static BehaviourControlModule controlModule = new BehaviourControlModule();
        public static BehaviourControlModule BCM => controlModule;
        static BehaviourControlModule() { }
        private BehaviourControlModule() { }
        #endregion
        public static bool InitializeBehaviourControlModule()
        {
            int attempts = 0;
            Debug.Log("Initializing Behaviour Control Module...");
        BCMINIT:
            if (BCM != null)
            {
                Debug.Log("Behaviour Control Module Initialized Successfully.");
                return true;
            }
            else if(attempts < 3)
            {
                Debug.LogError("Failed to Initialize Behaviour Control Module. Retrying");
                attempts++;
                goto BCMINIT;
            }
            else
            {
                Debug.LogError("Failed to Initialize Behaviour Control Module after 3 attempts. Aborting.");
                return false;
            }
        }

        #region Internal Update Functions
        internal static event Action UpdateEvent;
        internal static void SubscribeUpdate(Action action) => UpdateEvent += action;
        internal static void UnsubscribeUpdate(Action action) => UpdateEvent -= action;
        internal static void InvokeUpdate() => UpdateEvent?.Invoke();
        #endregion
        #region Internal LateUpdate Functions
        internal static event Action LateUpdateEvent;
        internal static void SubscribeLateUpdate(Action action) => LateUpdateEvent += action;
        internal static void UnsubscribeLateUpdate(Action action) => LateUpdateEvent -= action;
        internal static void InvokeLateUpdate() => LateUpdateEvent?.Invoke();
        #endregion

    }
    public class EngineFrameCycle
    {
        // This class will be responsible for managing the execution of the engine cycle.
        private static EngineFrameCycle frameCycle = new EngineFrameCycle();
        public static EngineFrameCycle FrameCycle => frameCycle;
        internal static bool engineStarted = false;
        static EngineCycle currentCycle = 0;
        public static bool InitializeEngineFrameCycle()
        {
        EFCINIT:
            int attempts = 0;
            Debug.Log("Initializing EFC...");
            if (FrameCycle != null)
            {
                Debug.Log("Engine Frame Cycle Initialized Successfully.");
                return true;
            }
            else if (attempts < 3)
            {
                Debug.LogError("Failed to EFC Module. Retrying");
                attempts++;
                goto EFCINIT;
            }
            else
            {
                Debug.LogError("Failed to Initialize EFC after 3 attempts. Aborting.");
                return false;
            }
        }
        static EngineFrameCycle() { }
        private EngineFrameCycle()
        {
            StartEngine();
        }
        
        public static void EngineLoop()
        {
            engineStarted = true;
            while (engineStarted)
            {
                switch (currentCycle)
                {
                    case EngineCycle.Awake:
                        break;
                    case EngineCycle.OnEnable:
                        break;
                    case EngineCycle.Start:
                        break;
                    case EngineCycle.FixedUpdate:
                        break;
                    case EngineCycle.OnInputEvents:
                        break;
                    case EngineCycle.Update:
                        BehaviourControlModule.InvokeUpdate();
                        break;
                    case EngineCycle.LateUpdate:
                        break;
                    case EngineCycle.OnRenderImage:
                        break;
                    case EngineCycle.OnApplicationQuit:
                        break;
                    case EngineCycle.OnDisable:
                        break;
                    case EngineCycle.OnDestroy:
                        IncrementFrameCount();
                        break;
                }
                //Debug.Log($"Current Engine Cycle: {currentCycle}");
                System.Threading.Thread.Sleep(1000 / (lockFrameRate ? (int)targetFrameRate : 1000)); //Sleep to maintain target frame rate 
                IncrementCycle();
            }
        }
        public static void ChangeCycle(EngineCycle newCycle) => currentCycle = newCycle;
        public static void IncrementCycle() { IncrementFrameCount(); currentCycle++; }
        public static void StartEngine() => engineStarted = true;
        public static void StopEngine() => engineStarted = false;
    }

    /// <summary>
    /// Behavious class that other scrips will inherit from, allowing them to be attached to game objects and have their own update loops, etc.
    /// From this class, we will subscribe to the engine's update loop allowing us to have a more flexible and modular approach to game development,
    /// as well as allowing us to easily create and manage game objects and their behaviors.
    /// </summary>
    public abstract class OrionBehaviour
    {
        #region OrionBehaviour Utils
        public void Print(Object message) 
        {
            EngineUtils.Print(message.ToString());
        }
        #endregion

        private Action cachedAwake;
        private Action cachedOnEnable;
        private Action cachedStart;
        private Action cachedFixedUpdate;
        private Action cachedUpdate;
        private Action cachedLateUpdate;
        private Action OnDisable;
        private Action OnDestroy;

        internal void InternalOnEnable()
        {
            RegisterLifecycleMethods();
        }
        internal void InternalOnDisable()
        {
            UnregisterLifecycleMethods();
        }
        internal void InternalOnDestroy()
        {
            InternalOnDestroy();
        }

        private void RegisterLifecycleMethods()
        {
            var type = GetType();

            // We will use reflection to check if the derived class has implemented any of the lifecycle methods.
            // If so, we will subscribe them to the appropriate events in the EngineControlModule.
            var updateMethod = type.GetMethod("Update");
            if (updateMethod != null)
            {
                cachedUpdate = (Action)Delegate.CreateDelegate(
                    typeof(Action), this, updateMethod);
                BehaviourControlModule.SubscribeUpdate(cachedUpdate);
            }

            var lateUpdateMethod = type.GetMethod("LateUpdate");
            if (lateUpdateMethod != null)
            {
                cachedLateUpdate = (Action)Delegate.CreateDelegate(
                    typeof(Action), this, lateUpdateMethod);
                BehaviourControlModule.SubscribeLateUpdate(cachedLateUpdate);
            }
        }

        private void UnregisterLifecycleMethods()
        {
            if (cachedUpdate != null)
                BehaviourControlModule.UnsubscribeUpdate(cachedUpdate);

            if (cachedLateUpdate != null)
                BehaviourControlModule.UnsubscribeLateUpdate(cachedLateUpdate);
        }
    }
}
