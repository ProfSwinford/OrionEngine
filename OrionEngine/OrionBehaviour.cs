// based on the Unity Engine Execution Order: https://docs.unity3d.com/6000.3/Documentation/Manual/execution-order.html

using System;

namespace OrionEngine
{
    /// <summary>
    /// Behavious class that other scrips will inherit from, allowing them to be attached to game objects and have their own update loops, etc.
    /// From this class, we will subscribe to the engine's update loop allowing us to have a more flexible and modular approach to game development,
    /// as well as allowing us to easily create and manage game objects and their behaviors.
    /// </summary>
    public abstract class OrionBehaviour
    {
        #region OrionBehaviour Utils
        public void Print(Object? message = null) 
        {
            EngineUtils.Print(message.ToString());
        }
        public void WaitForInput() 
        {
            EngineUtils.WaitForInput();
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

        public OrionBehaviour()
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
