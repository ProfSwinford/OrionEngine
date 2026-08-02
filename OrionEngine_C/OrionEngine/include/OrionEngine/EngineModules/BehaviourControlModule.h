// Converted from OrionEngine/EngineModules/BehaviourControlModule.cs.
// based on the Unity Engine Execution Order: https://docs.unity3d.com/6000.3/Documentation/Manual/execution-order.html
// convert to ECS (Entity Component System) eventually?
#pragma once

#include <vector>

namespace OrionEngine
{
    class OrionBehaviour;

    namespace EngineModules
    {
        /// <summary> Engine Control Module
        /// This class will be responsible for managing the execution of the engine cycle, as well as any other necessary
        /// tasks related to the engine's operation. It will also be responsible for managing the various modules that make
        /// up the engine, such as the rendering module, physics module, etc.
        /// </summary>
        class BehaviourControlModule
        {
        public:
            static BehaviourControlModule& BCM();
            static bool InitializeBehaviourControlModule();

            static void RegisterBehaviour(OrionBehaviour* obj);
            static void ProcessNewObjects();
            static void ProcessStart();
            static void ProcessFixedUpdate();
            static void ProcessUpdate();
            static void ProcessLateUpdate();
            static void SetEnabled(OrionBehaviour* b, bool enabled);
            static void Destroy(OrionBehaviour* b);

            /// <summary>
            /// Frees every registered behaviour. Replaces the garbage collector the
            /// C# implementation relied on; called by EngineCore::Shutdown.
            /// </summary>
            static void DestroyAllBehaviours();

            BehaviourControlModule(const BehaviourControlModule&) = delete;
            BehaviourControlModule& operator=(const BehaviourControlModule&) = delete;

        private:
            BehaviourControlModule() = default;
        };
    }
}
