// Converted from OrionEngine/OrionBehaviour.cs.
// based on the Unity Engine Execution Order: https://docs.unity3d.com/6000.3/Documentation/Manual/execution-order.html
#pragma once

#include "OrionEngine/EngineModules/InputModule.h"
#include "OrionEngine/EngineModules/Rendering/ConsoleRendererModule.h"
#include "OrionEngine/EngineUtils.h"
#include "OrionEngine/Keycode.h"
#include "OrionEngine/Object.h"

namespace OrionEngine
{
    namespace EngineModules
    {
        class BehaviourControlModule;
    }

    /// <summary>
    /// Behaviour class that other scripts will inherit from, allowing them to be attached to game objects and have their own update loops, etc.
    /// From this class, we will subscribe to the engine's update loop allowing us to have a more flexible and modular approach to game development,
    /// as well as allowing us to easily create and manage game objects and their behaviors.
    /// </summary>
    /// <remarks>
    /// The C# version discovered Awake/Start/Update/... by reflecting over the
    /// derived type and binding whatever it found to a delegate. C++ has no
    /// reflection, so the lifecycle callbacks are virtual methods with empty
    /// defaults instead: overriding one opts into it exactly as declaring the
    /// method did in C#. Behaviours must be heap allocated (`new MyBehaviour()`),
    /// matching the C# reference semantics; BehaviourControlModule owns them from
    /// construction and frees them at engine shutdown.
    /// </remarks>
    class OrionBehaviour
    {
    public:
        explicit OrionBehaviour(bool enabled = true);
        virtual ~OrionBehaviour();

        OrionBehaviour(const OrionBehaviour&) = delete;
        OrionBehaviour& operator=(const OrionBehaviour&) = delete;

        // --- Lifecycle callbacks (override to opt in) ---
        virtual void Awake() {}
        virtual void OnEnable() {}
        virtual void Start() {}
        virtual void FixedUpdate() {}
        virtual void Update() {}
        virtual void LateUpdate() {}
        virtual void OnDisable() {}
        virtual void OnDestroy() {}

        // --- OrionBehaviour Utils ---
        void Print(const Object& message = Object(), char end = '\n');
        void WaitForInput();

        bool Enabled() const { return IsEnabled; }
        void Enabled(bool value);

        struct Camera
        {
            static EngineModules::Rendering::ConsoleCanvas& canvas();
        };

        /// <summary>Input Module Wrapper.</summary>
        struct Input
        {
            static bool GetKeyDown(Keycode key) { return EngineModules::InputModule::GetKeyDown(key); }
            static bool GetKey(Keycode key) { return EngineModules::InputModule::GetKey(key); }
            static bool GetKeyUp(Keycode key) { return EngineModules::InputModule::GetKeyUp(key); }
            static bool IsAnyKeyDown() { return EngineModules::InputModule::AnyKeyDown(); }
        };

        /// <summary>
        /// Temporarily internal in the C# original until coroutines are implemented,
        /// then it can be made public so users may toggle the engine from scripts.
        /// </summary>
        struct Engine
        {
            static void ToggleEngine();
        };

    private:
        friend class EngineModules::BehaviourControlModule;

        bool InvokedAwake = false;
        bool InvokedStart = false;
        bool IsEnabled = true;
    };
}
