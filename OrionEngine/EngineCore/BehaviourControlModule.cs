// based on the Unity Engine Execution Order: https://docs.unity3d.com/6000.3/Documentation/Manual/execution-order.html

using System;

namespace OrionEngine
{
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
}
