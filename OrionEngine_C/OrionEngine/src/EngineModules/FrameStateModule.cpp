#include "OrionEngine/EngineModules/FrameStateModule.h"

#include "OrionEngine/Debug.h"
#include "OrionEngine/EngineModules/BehaviourControlModule.h"
#include "OrionEngine/EngineModules/InputModule.h"
#include "OrionEngine/EngineModules/Rendering/ConsoleRendererModule.h"
#include "OrionEngine/EngineStatistics.h"

namespace OrionEngine
{
    namespace EngineModules
    {
        namespace
        {
            bool engineStarted = false;
            FrameState curFrameState = static_cast<FrameState>(0);
            bool programRunning = true;  // don't change
            bool _moduleConstructed = false;
        }

        FrameState& operator++(FrameState& state)
        {
            state = static_cast<FrameState>(static_cast<int>(state) + 1);
            return state;
        }

        FrameStateModule& FrameStateModule::FrameCycle()
        {
            static FrameStateModule frameCycle;
            _moduleConstructed = true;
            return frameCycle;
        }

        bool FrameStateModule::InitializeEngineFrameCycle()
        {
            // See BehaviourControlModule: the C# null check becomes a construction flag.
            int attempts = 0;

        EFCINIT:
            FrameCycle();
            if (_moduleConstructed)
            {
                Debug::Log("Engine Frame Cycle Initialized Successfully.");
                return true;
            }
            else if (attempts < 3)
            {
                Debug::LogError("Failed to EFC Module. Retrying");
                attempts++;
                goto EFCINIT;
            }
            else
            {
                Debug::LogError("Failed to Initialize EFC after 3 attempts. Aborting.");
                return false;
            }
        }

        void FrameStateModule::RunEngineCycle()
        {
            engineStarted = true;
            while (engineStarted)
            {
                switch (curFrameState)
                {
                    case FrameState::OnInputEvents:  // Moved to before Awake to allow for input state storage before OrionBehaviour functions are called.
                        InputModule::Update();
                        break;
                    case FrameState::Awake:
                    case FrameState::OnEnable:
                        if (!programRunning)
                        {
                            break;
                        }

                        BehaviourControlModule::ProcessNewObjects();
                        break;
                    case FrameState::Start:
                        if (!programRunning)
                        {
                            break;
                        }

                        BehaviourControlModule::ProcessStart();
                        break;
                    case FrameState::FixedUpdate:
                        if (!programRunning)
                        {
                            break;
                        }

                        BehaviourControlModule::ProcessFixedUpdate();
                        break;
                    case FrameState::Update:
                        // EngineCore::EngineUpdate();

                        if (!programRunning)
                        {
                            break;
                        }

                        BehaviourControlModule::ProcessUpdate();
                        break;
                    case FrameState::LateUpdate:
                        if (!programRunning)
                        {
                            break;
                        }

                        BehaviourControlModule::ProcessLateUpdate();
                        break;
                    case FrameState::OnRenderImage:
                        if (!programRunning)
                        {
                            break;
                        }

                        // canvas.Render();
                        Rendering::ConsoleRendererModule::canvas().Render();
                        break;
                    case FrameState::OnApplicationQuit:
                        break;
                    case FrameState::OnDisable:
                        break;
                    case FrameState::OnDestroy:
                        ResetCycle();
                        if (!programRunning)
                        {
                            break;
                        }
                        EngineStatistics::IncrementFrameCount();
                        break;
                    default:
                        break;
                }
                // Debug::Log("Current Engine Cycle: " + std::to_string((int)curFrameState));
                // 1000ms / 60fps = 16.67ms per frame, so we sleep for that amount of time to maintain a consistent frame rate.
                // We can adjust this based on the target frame rate set in the ECM Timing Settings.
                // std::this_thread::sleep_for(std::chrono::milliseconds(1000 / (EngineStatistics::lockFrameRate ? (int)EngineStatistics::targetFrameRate : 1000))); //Sleep to maintain target frame rate
                IncrementCycle();
            }
        }

        void FrameStateModule::ChangeCycle(FrameState newCycle)
        {
            curFrameState = newCycle;
        }

        void FrameStateModule::IncrementCycle()
        {
            ++curFrameState;
        }

        void FrameStateModule::ResetCycle()
        {
            curFrameState = static_cast<FrameState>(0);
        }

        void FrameStateModule::StopEngine()
        {
            engineStarted = false;
        }

        void FrameStateModule::ToggleProgramRunning()
        {
            programRunning = !programRunning;
        }

        bool FrameStateModule::EngineStarted()
        {
            return engineStarted;
        }
    }
}
