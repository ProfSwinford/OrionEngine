// Umbrella header for the engine.
//
// The C# projects pulled the engine in with `using OrionEngine;` plus the
// `global using` directives declared in EngineCore.cs and OrionBehaviour.cs.
// Including this single header is the C++ equivalent: user scripts need only
//
//     #include "OrionEngine/OrionEngine.h"
//     using namespace OrionEngine;
//
#pragma once

#include "OrionEngine/Console.h"
#include "OrionEngine/Debug.h"
#include "OrionEngine/EngineCore.h"
#include "OrionEngine/EngineModules/BehaviourControlModule.h"
#include "OrionEngine/EngineModules/FrameStateModule.h"
#include "OrionEngine/EngineModules/InputModule.h"
#include "OrionEngine/EngineModules/Physics/ConsolePhysicsModule.h"
#include "OrionEngine/EngineModules/Rendering/ConsoleRendererModule.h"
#include "OrionEngine/EngineStatistics.h"
#include "OrionEngine/EngineUtils.h"
#include "OrionEngine/Keycode.h"
#include "OrionEngine/Object.h"
#include "OrionEngine/OrionBehaviour.h"
#include "OrionEngine/Pixel.h"
#include "OrionEngine/Vector2.h"

namespace OrionEngine
{
    /// <summary>
    /// Equivalent of `global using Input = OrionEngine.EngineModules.InputModule;`
    /// from OrionBehaviour.cs. Note that OrionBehaviour also declares a nested
    /// `Input` wrapper, which takes precedence inside behaviour classes - same as
    /// in the C# original.
    /// </summary>
    using InputModule = EngineModules::InputModule;

    using ConsoleCanvas = EngineModules::Rendering::ConsoleCanvas;
    using ConsoleRendererModule = EngineModules::Rendering::ConsoleRendererModule;
    using FrameStateModule = EngineModules::FrameStateModule;
    using BehaviourControlModule = EngineModules::BehaviourControlModule;
}
