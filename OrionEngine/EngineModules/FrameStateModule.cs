using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OrionEngine.EngineStatistics;

namespace OrionEngine
{
    public class FrameStateModule
    {
        #region Singleton
        // This class will be responsible for managing the execution of the engine cycle.
        private static FrameStateModule frameCycle = new FrameStateModule();
        public static FrameStateModule FrameCycle => frameCycle;
        public static bool InitializeEngineFrameCycle()
        {
            int attempts = 0;

        EFCINIT:
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
        static FrameStateModule() { }
        private FrameStateModule() { }
        #endregion

        internal static bool engineStarted = false;
        static FrameState currentCycle = 0;

        public static void RunEngineCycle()
        {
            engineStarted = true;
            while (engineStarted)
            {
                switch (currentCycle)
                {
                    case FrameState.Awake:
                        break;
                    case FrameState.OnEnable:
                        break;
                    case FrameState.Start:
                        break;
                    case FrameState.FixedUpdate:
                        break;
                    case FrameState.OnInputEvents:
                        break;
                    case FrameState.Update:
                        BehaviourControlModule.InvokeUpdate();
                        break;
                    case FrameState.LateUpdate:
                        break;
                    case FrameState.OnRenderImage:
                        break;
                    case FrameState.OnApplicationQuit:
                        break;
                    case FrameState.OnDisable:
                        break;
                    case FrameState.OnDestroy:
                        IncrementFrameCount();
                        ResetCycle();
                        break;
                }
                //Debug.Log($"Current Engine Cycle: {currentCycle}");
                System.Threading.Thread.Sleep(1000 / (lockFrameRate ? (int)targetFrameRate : 1000)); //Sleep to maintain target frame rate 
                IncrementCycle();
            }
        }
        public static void ChangeCycle(FrameState newCycle) => currentCycle = newCycle;
        public static void IncrementCycle() => currentCycle++;
        public static void ResetCycle() => currentCycle = 0;
        public static void StopEngine() => engineStarted = false;
    }
    public enum FrameState
    {
        // Comment out unnecessary options to limit our options for the time being. We'll add more as we need them.

        Awake, // OrionBehaviour - Awake is called when the script instance is being loaded. This happens before any Start functions and also just after a prefab is instantiated. This is used to initialize any variables or game state before the game starts. Awake is called only once during the lifetime of the script instance. Awake is called even if the script component is disabled, allowing you to initialize data that may be used by other scripts during their Start phase.
        OnEnable, // OrionBehaviour - OnEnable is called when the behaviour becomes enabled and active. This can happen when the behaviour is first created, or when it is re-enabled after being disabled. OnEnable is called every time the behaviour is enabled, so it can be called multiple times during the lifetime of the script instance. OnEnable is called after Awake and before Start, allowing you to set up any necessary state or references before the game starts.
        //Reset,
        Start, // OrionBehaviour - Start is called before the first frame update only if the script instance is enabled. This is used to initialize any variables or game state before the game starts. Start is called only once during the lifetime of the script instance, and it is called after all Awake functions have been called. Start is called only if the script component is enabled, allowing you to set up any necessary state or references before the game starts.
        FixedUpdate, // OrionBehaviour - FixedUpdate is called every fixed framerate frame, and is used for physics updates. This is called at a consistent rate, independent of the frame rate, making it ideal for handling physics calculations. FixedUpdate is called every fixed frame-rate frame, and the interval between calls will be modifiable in the ECM Timing Settings. FixedUpdate is called only if the script component is enabled, allowing you to perform physics calculations at a consistent rate.
        //InternalPhysicsUpdate,
        //InternalAnimationUpdate,
        //OnTriggerEnter,
        //OnTriggerStay,
        //OnTriggerExit,
        //OnCollisionEnter,
        //OnCollisionStay,
        //OnCollisionExit,
        //YieldWaitForFixedUpdate,
        //AwaitableFixedUpdateAsync,
        OnInputEvents, // OrionEngine - OnInputEvents is called every frame, and is used for handling input events. This is called immediately before Update functions, in preparation for storing input states to use during the Update state.
        Update, // OrionBehaviour - Update is called once per frame, and is used for regular game calculations such as moving non-physics objects, simple timers, etc. This is called every frame, and the interval between calls will vary based on the frame rate. Update is called only if the script component is enabled, allowing you to perform regular updates at a variable rate.
        //YieldNull,
        //YieldWaitForSeconds,
        //YieldWWW,
        //YieldStartCoroutine,
        //AsyncTaskContinuation,
        //AwaitableNextFrameAsync,
        //LateInternalAnimationUpdate,
        LateUpdate, // OrionBehaviour - LateUpdate is called once per frame, after all Update functions have been called. This is used to perform any calculations that need to happen after all Update functions have been called, such as following a target with a camera. LateUpdate is called every frame, and the interval between calls will vary based on the frame rate. LateUpdate is called only if the script component is enabled, allowing you to perform updates that need to happen after all Update functions have been called.
        //OnPreCull,
        //OnBecameInvisible,
        //OnWillRenderObject,
        //OnPreRender,
        //OnRenderObject,
        //OnPostRender,
        OnRenderImage, // OrionEngine - OnRenderImage is called after all rendering is complete to render image. This is used to perform any post-processing effects on the rendered image, such as bloom, color grading, etc.
        //OnDrawGizmos,
        //YieldWaitForEndOfFrame,
        //AwaitableEndOfFrameAsync,
        //OnApplicationPause,
        OnApplicationQuit, // OrionEngine - OnApplicationQuit is called when the application is quitting. This is used to perform any cleanup tasks before the application exits, such as saving game data, releasing resources, etc. 
        OnDisable, // OrionBehaviour - OnDisable is called when the behaviour becomes disabled or inactive. This can happen when the host object is destroyed, or when the specific behavious is disabled. Since it is called every time the behaviour becomes disabled, so it can be called multiple times during the lifetime of the script instance. OnDisable is called after all Update functions have been called, allowing you to perform any necessary cleanup tasks before the behaviour is disabled.
        OnDestroy // OrionBehaviour - OnDestroy is called when the behaviour will be destroyed. This is used to perform any cleanup tasks before the object is destroyed, such as saving game data, releasing resources, etc. OnDestroy is called only once during the lifetime of the script instance, and it is called after all Update functions have been called, allowing you to perform any necessary cleanup tasks before the object is destroyed.
    }
}
