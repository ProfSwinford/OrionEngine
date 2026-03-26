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
        public void Print(Object? message = null, char end = '\n') 
        {
            EngineUtils.Print(message.ToString() + end);
        }
        public void WaitForInput() 
        {
            EngineUtils.WaitForInput();
        }
        #endregion

        internal Action Awake;
        internal Action OnEnable;
        internal Action Start;
        internal Action FixedUpdate;
        internal Action Update;
        internal Action LateUpdate;
        internal Action OnDisable;
        internal Action OnDestroy;

        internal bool InvokedAwake = false;
        internal bool InvokedStart = false;
        internal bool IsEnabled = true;
        public bool Enabled { get => IsEnabled; set => BehaviourControlModule.SetEnabled(this, value); }

        public OrionBehaviour(bool enabled = true)
        {
            BehaviourControlModule.RegisterBehaviour(this);
            CheckLifecycleMethods();
        }

        private void CheckLifecycleMethods()
        {
            var type = GetType();

            // We will use reflection to check if the derived class has implemented any of the lifecycle methods.
            // If so, we will subscribe them to the appropriate events in the EngineControlModule.
            var awakeMethod = type.GetMethod("Awake");
            if (awakeMethod != null)
            {
                Awake = (Action)Delegate.CreateDelegate(
                    typeof(Action), this, awakeMethod);
                //BehaviourControlModule.SubscribeUpdate(cachedAwake); // All of these "subscribe" functions have been depreciated
            }
            var OnEnableMethod = type.GetMethod("OnEnable");
            if (OnEnableMethod != null)
            {
                OnEnable = (Action)Delegate.CreateDelegate(
                    typeof(Action), this, OnEnableMethod);
                //BehaviourControlModule.SubscribeUpdate(cachedOnEnable);
            }
            var StartMethod = type.GetMethod("Start");
            if (StartMethod != null)
            {
                Start = (Action)Delegate.CreateDelegate(
                    typeof(Action), this, StartMethod);
                //BehaviourControlModule.SubscribeUpdate(cachedStart);
            }
            var FixedUpdateMethod = type.GetMethod("FixedUpdate");
            if (FixedUpdateMethod != null)
            {
                FixedUpdate = (Action)Delegate.CreateDelegate(
                    typeof(Action), this, FixedUpdateMethod);
                //BehaviourControlModule.SubscribeUpdate(cachedFixedUpdate);
            }
            var updateMethod = type.GetMethod("Update");
            if (updateMethod != null)
            {
                Update = (Action)Delegate.CreateDelegate(
                    typeof(Action), this, updateMethod);
                //BehaviourControlModule.SubscribeUpdate(cachedUpdate);
            }
            var lateUpdateMethod = type.GetMethod("LateUpdate");
            if (lateUpdateMethod != null)
            {
                LateUpdate = (Action)Delegate.CreateDelegate(
                    typeof(Action), this, lateUpdateMethod);
                //BehaviourControlModule.SubscribeLateUpdate(cachedLateUpdate);
            }
            var OnDisableMethod = type.GetMethod("OnDisable");
            if (OnDisableMethod != null)
            {
                OnDisable = (Action)Delegate.CreateDelegate(
                    typeof(Action), this, OnDisableMethod);
                //BehaviourControlModule.SubscribeLateUpdate(cachedOnDisable);
            }
            var OnDestroyMethod = type.GetMethod("OnDestroy");
            if (OnDestroyMethod != null)
            {
                OnDestroy = (Action)Delegate.CreateDelegate(
                    typeof(Action), this, OnDestroyMethod);
                //BehaviourControlModule.SubscribeLateUpdate(cachedOnDestroy);
            }
        }

        #region Unused/Old Code
        //private void UnregisterLifecycleMethods()
        //{
        //    if (Update != null)
        //        BehaviourControlModule.UnsubscribeUpdate(Update);

        //    if (LateUpdate != null)
        //        BehaviourControlModule.UnsubscribeLateUpdate(LateUpdate);
        //}
        #endregion
    }
}
