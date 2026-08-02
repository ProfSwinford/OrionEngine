#include "OrionEngine/OrionBehaviour.h"

#include "OrionEngine/EngineModules/BehaviourControlModule.h"
#include "OrionEngine/EngineModules/FrameStateModule.h"

namespace OrionEngine
{
    OrionBehaviour::OrionBehaviour(bool enabled)
        : IsEnabled(enabled)
    {
        EngineModules::BehaviourControlModule::RegisterBehaviour(this);

        // The C# constructor called CheckLifecycleMethods() here to reflect over the
        // derived type. Virtual dispatch replaces that step entirely.
    }

    OrionBehaviour::~OrionBehaviour() = default;

    void OrionBehaviour::Print(const Object& message, char end)
    {
        EngineUtils::Print(message.ToString() + std::string(1, end));
    }

    void OrionBehaviour::WaitForInput()
    {
        EngineUtils::WaitForInput();
    }

    void OrionBehaviour::Enabled(bool value)
    {
        EngineModules::BehaviourControlModule::SetEnabled(this, value);
    }

    EngineModules::Rendering::ConsoleCanvas& OrionBehaviour::Camera::canvas()
    {
        return EngineModules::Rendering::ConsoleRendererModule::canvas();
    }

    void OrionBehaviour::Engine::ToggleEngine()
    {
        EngineModules::FrameStateModule::ToggleProgramRunning();
    }
}
