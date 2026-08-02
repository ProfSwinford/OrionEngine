// Converted from OrionEngine/EngineCore.cs.
// based on the Unity Engine Execution Order: https://docs.unity3d.com/6000.3/Documentation/Manual/execution-order.html
//
// The C# file opened with:
//     global using static OrionEngine.EngineUtils;
//     global using static OrionEngine.EngineStatistics;
// C++ has no global using, so the equivalent headers are included here and the
// members are reached through their class names (EngineUtils::Print, ...).
#pragma once

#include "OrionEngine/Debug.h"
#include "OrionEngine/EngineStatistics.h"
#include "OrionEngine/EngineUtils.h"

namespace OrionEngine
{
    /// <summary>
    /// This class will be responsible for managing the overall state of the engine, as well as any necessary
    /// initialization and cleanup tasks.
    /// </summary>
    class EngineCore
    {
    public:
        static EngineCore& OrionEngine();

        /// <summary>
        /// Constructs the engine singleton, which runs the frame cycle to
        /// completion before returning - exactly as in the C# original, where
        /// touching the static field triggered the private constructor.
        /// </summary>
        static bool EngineCoreInit();

        /// <summary>
        /// Releases behaviours, GameObjects and the terminal state. Not present in
        /// the C# version, where the GC and the CLR handled both; also wired up
        /// through std::atexit so `Environment.Exit`-style teardown still restores
        /// the console.
        /// </summary>
        static void Shutdown();

        EngineCore(const EngineCore&) = delete;
        EngineCore& operator=(const EngineCore&) = delete;

    private:
        EngineCore();

        static void InitializeEngine();
        static void EngineUpdate();
        static bool InitEngineModules();
    };
}
