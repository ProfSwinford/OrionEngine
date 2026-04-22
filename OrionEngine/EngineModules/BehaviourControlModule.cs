// based on the Unity Engine Execution Order: https://docs.unity3d.com/6000.3/Documentation/Manual/execution-order.html
// convert to ECS (Entity Component System) eventually? 
using System;
using System.Collections.Generic;

namespace OrionEngine.EngineModules
{
    /// <summary> Engine Control Module
    /// This class will be responsible for managing the execution of the engine cycle, as well as any other necessary
    /// tasks related to the engine's operation. It will also be responsible for managing the various modules that make
    /// up the engine, such as the rendering module, physics module, etc.
    /// </summary>
    public class BehaviourControlModule
    {
        private static List<OrionBehaviour> _newObjects = new();
        private static List<OrionBehaviour> _startQueue = new();
        private static List<OrionBehaviour> _activeObjects = new();
        private static List<OrionBehaviour> _disabledObjects = new();
        private static List<OrionBehaviour> _destroyedObjects = new();

        #region BCM Singleton
        private static BehaviourControlModule controlModule = new BehaviourControlModule();
        public static BehaviourControlModule BCM => controlModule;
        static BehaviourControlModule() { }
        private BehaviourControlModule() { }
        #endregion

        public static bool InitializeBehaviourControlModule()
        {
            int attempts = 0;
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
        internal static void RegisterBehaviour(OrionBehaviour obj)
        {
            _newObjects.Add(obj);
        }
        static internal void ProcessNewObjects()
        {
            foreach (var b in _newObjects)
            {
                if (!b.IsEnabled)
                {
                    return;
                }

                if (!b.InvokedAwake)
                {
                    b.Awake?.Invoke();
                    b.InvokedAwake = true;
                }

                if (b.IsEnabled)
                {
                    b.OnEnable?.Invoke();
                    _startQueue.Add(b);
                }
            }

            _newObjects.Clear();
        }
        static internal void ProcessStart()
        {
            foreach (var b in _startQueue)
            {
                if (!b.InvokedStart)
                {
                    b.Start?.Invoke();
                    b.InvokedStart = true;
                    _activeObjects.Add(b);
                }
            }

            _startQueue.Clear();
        }
        internal static void ProcessFixedUpdate()
        {
            foreach (var b in _activeObjects)
                b.FixedUpdate?.Invoke();
        }
        internal static void ProcessUpdate()
        {
            foreach (var b in _activeObjects)
                b.Update?.Invoke();
        }
        internal static void ProcessLateUpdate()
        {
            foreach (var b in _activeObjects)
                b.LateUpdate?.Invoke();
        }
        internal static void SetEnabled(OrionBehaviour b, bool enabled)
        {
            if (b.IsEnabled == enabled) return;

            b.IsEnabled = enabled;

            if (enabled)
            {
                b.OnEnable?.Invoke();
                _startQueue.Add(b);
                _activeObjects.Add(b);
            }
            else
            {
                b.OnDisable?.Invoke();
                _activeObjects.Remove(b);
            }
        }
        internal static void Destroy(OrionBehaviour b)
        {
            if (b.IsEnabled)
            {
                b.OnDisable?.Invoke();
            }

            b.OnDestroy?.Invoke();

            _activeObjects.Remove(b);
        }
        
        ///
        ///
        /// Dead Zone
        /// 
        ///
        
        #region Unused/Old Code
        //#region Internal Awake Functions
        //internal static event Action AwakeEvent;
        //internal static void SubscribeAwake(Action action) => AwakeEvent += action;
        //internal static void UnsubscribeAwake(Action action) => AwakeEvent -= action;
        //internal static void InvokeAwake()
        //{
        //    AwakeEvent?.Invoke();
        //}
        //#endregion
        //#region Internal OnEnable Functions
        //internal static event Action OnEnableEvent;
        //internal static void SubscribeOnEnable(Action action) => OnEnableEvent += action;
        //internal static void UnsubscribeOnEnable(Action action) => OnEnableEvent -= action;
        //internal static void InvokeOnEnable() => OnEnableEvent?.Invoke();
        //#endregion
        //#region Internal Start Functions
        //internal static event Action StartEvent;
        //internal static void SubscribeStart(Action action) => StartEvent += action;
        //internal static void UnsubscribeStart(Action action) => StartEvent -= action;
        //internal static void InvokeStartUpdate() => StartEvent?.Invoke();

        //#endregion
        //#region Internal Fixed Update Functions
        //internal static event Action FixedUpdateEvent;
        //internal static void SubscribeFixedUpdate(Action action) => FixedUpdateEvent += action;
        //internal static void UnsubscribeFixedUpdate(Action action) => FixedUpdateEvent -= action;
        //internal static void InvokeFixedUpdate() => FixedUpdateEvent?.Invoke();
        //#endregion
        //#region Internal Update Functions
        //internal static event Action UpdateEvent;
        //internal static void SubscribeUpdate(Action action) => UpdateEvent += action;
        //internal static void UnsubscribeUpdate(Action action) => UpdateEvent -= action;
        //internal static void InvokeUpdate() => UpdateEvent?.Invoke();
        //#endregion
        //#region Internal LateUpdate Functions
        //internal static event Action LateUpdateEvent;
        //internal static void SubscribeLateUpdate(Action action) => LateUpdateEvent += action;
        //internal static void UnsubscribeLateUpdate(Action action) => LateUpdateEvent -= action;
        //internal static void InvokeLateUpdate() => LateUpdateEvent?.Invoke();
        //#endregion
        //#region Internal OnDisable Functions
        //internal static event Action OnDisableEvent;
        //internal static void SubscribeOnDisable(Action action) => OnDisableEvent += action;
        //internal static void UnsubscribeOnDisable(Action action) => OnDisableEvent -= action;
        //internal static void InvokeOnDisable() => OnDisableEvent?.Invoke();
        //#endregion
        //#region Internal OnDestroy Functions
        //internal static event Action OnDestroyEvent;
        //internal static void SubscribeOnDestroy(Action action) => OnDestroyEvent += action;
        //internal static void UnsubscribeOnDestroy(Action action) => OnDestroyEvent -= action;
        //internal static void InvokeOnDestroy() => OnDestroyEvent?.Invoke();
        //#endregion
        #endregion
    }
}
