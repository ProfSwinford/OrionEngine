#include "OrionEngine/EngineCore.h"

#include <cstdlib>
#include <stdexcept>

#include "OrionEngine/Console.h"
#include "OrionEngine/EngineModules/BehaviourControlModule.h"
#include "OrionEngine/EngineModules/FrameStateModule.h"
#include "OrionEngine/EngineModules/InputModule.h"
#include "OrionEngine/EngineModules/Rendering/ConsoleRendererModule.h"
#include "OrionEngine/Object.h"

namespace OrionEngine
{
    namespace
    {
        bool g_engineConstructed = false;

        void RestoreConsoleAtExit()
        {
            EngineModules::InputModule::Shutdown();
            Console::Shutdown();
        }
    }

    EngineCore& EngineCore::OrionEngine()
    {
        static EngineCore instance;
        g_engineConstructed = true;
        return instance;
    }

    EngineCore::EngineCore()
    {
        InitializeEngine();
    }

    bool EngineCore::EngineCoreInit()
    {
        OrionEngine();
        return g_engineConstructed;
    }

    void EngineCore::InitializeEngine()
    {
        // --- Debug Testing ---
        // Debug logs for testing the Debug class and ColoredText struct
        // ColoredText("Welcome to Orion Engine!", ConsoleColor::Cyan).Write('\n');
        // Debug::LogMessage(ColoredText("Starting Engine Systems:", ConsoleColor::Cyan));
        // Debug::Log("Engine Initializing Logs");
        // Debug::LogWarning("Engine Initializing Warnings");
        // Debug::LogError("Engine Initializing Errors");

        // Make sure the terminal is handed back in a usable state even when a script
        // exits the process outright (the C# PongExample calls Environment.Exit).
        std::atexit(&RestoreConsoleAtExit);

        EngineModules::Rendering::ConsoleRendererModule renderModule;
        (void)renderModule;

        Console::SetCursorVisible(false);
        Debug::Log("Initializing Core Engine Modules...");
        if (!InitEngineModules())
        {
            throw std::runtime_error("Failed to initialize Engine Core, check log output for details.");
        }
        Debug::Log("Engine Initialized Successfully, press any key to contine...");
        EngineModules::FrameStateModule::RunEngineCycle();

        // Reached once StopEngine() breaks the cycle.
        Shutdown();
    }

    void EngineCore::EngineUpdate()
    {
        // WaitForInput();
        // FrameStateModule::ToggleProgramRunning();
    }

    bool EngineCore::InitEngineModules()
    {
        if (!EngineModules::BehaviourControlModule::InitializeBehaviourControlModule())
        {
            throw std::runtime_error("Failed to initialize Behaviour Control Module.");
        }
        Debug::Log("Initializing Engine Frame Cycle...");
        if (!EngineModules::FrameStateModule::InitializeEngineFrameCycle())
        {
            throw std::runtime_error("Failed to initialize Engine Frame Cycle.");
        }
        return true;
    }

    void EngineCore::Shutdown()
    {
        EngineModules::BehaviourControlModule::DestroyAllBehaviours();
        GameObject::DestroyAllGameObjects();
        RestoreConsoleAtExit();
    }
}
