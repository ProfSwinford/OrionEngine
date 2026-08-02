#include "OrionEngine/EngineModules/BehaviourControlModule.h"

#include <algorithm>

#include "OrionEngine/Debug.h"
#include "OrionEngine/OrionBehaviour.h"

namespace OrionEngine
{
    namespace EngineModules
    {
        namespace
        {
            std::vector<OrionBehaviour*> _newObjects;
            std::vector<OrionBehaviour*> _startQueue;
            std::vector<OrionBehaviour*> _activeObjects;
            std::vector<OrionBehaviour*> _disabledObjects;
            std::vector<OrionBehaviour*> _destroyedObjects;

            /// <summary>Owns every registered behaviour, standing in for the CLR heap.</summary>
            std::vector<OrionBehaviour*> _allBehaviours;

            bool _moduleConstructed = false;

            void Remove(std::vector<OrionBehaviour*>& list, OrionBehaviour* behaviour)
            {
                list.erase(std::remove(list.begin(), list.end(), behaviour), list.end());
            }
        }

        BehaviourControlModule& BehaviourControlModule::BCM()
        {
            static BehaviourControlModule controlModule;
            _moduleConstructed = true;
            return controlModule;
        }

        bool BehaviourControlModule::InitializeBehaviourControlModule()
        {
            // The C# retry loop tested the singleton against null. A function local
            // static reference can never be null, so construction is confirmed with
            // a flag instead; the retry shape is kept for parity.
            int attempts = 0;
        BCMINIT:
            BCM();
            if (_moduleConstructed)
            {
                Debug::Log("Behaviour Control Module Initialized Successfully.");
                return true;
            }
            else if (attempts < 3)
            {
                Debug::LogError("Failed to Initialize Behaviour Control Module. Retrying");
                attempts++;
                goto BCMINIT;
            }
            else
            {
                Debug::LogError("Failed to Initialize Behaviour Control Module after 3 attempts. Aborting.");
                return false;
            }
        }

        void BehaviourControlModule::RegisterBehaviour(OrionBehaviour* obj)
        {
            _newObjects.push_back(obj);
            _allBehaviours.push_back(obj);
        }

        void BehaviourControlModule::ProcessNewObjects()
        {
            // The C# foreach would throw if a callback registered another behaviour
            // mid-iteration. Here the batch size is captured up front and only that
            // prefix is consumed, so behaviours created during Awake/OnEnable simply
            // roll into the next frame.
            const size_t batch = _newObjects.size();

            for (size_t index = 0; index < batch; index++)
            {
                OrionBehaviour* const b = _newObjects[index];

                if (!b->IsEnabled)
                {
                    // Matches the C# `return`: the queue is left intact and retried
                    // on the following frame.
                    return;
                }

                if (!b->InvokedAwake)
                {
                    b->Awake();
                    b->InvokedAwake = true;
                }

                if (b->IsEnabled)
                {
                    b->OnEnable();
                    _startQueue.push_back(b);
                }
            }

            _newObjects.erase(_newObjects.begin(), _newObjects.begin() + static_cast<long>(batch));
        }

        void BehaviourControlModule::ProcessStart()
        {
            const size_t batch = _startQueue.size();

            for (size_t index = 0; index < batch; index++)
            {
                OrionBehaviour* const b = _startQueue[index];

                if (!b->InvokedStart)
                {
                    b->Start();
                    b->InvokedStart = true;
                    _activeObjects.push_back(b);
                }
            }

            _startQueue.erase(_startQueue.begin(), _startQueue.begin() + static_cast<long>(batch));
        }

        void BehaviourControlModule::ProcessFixedUpdate()
        {
            // Snapshot so a callback that enables, disables or destroys a behaviour
            // cannot invalidate the iteration.
            const std::vector<OrionBehaviour*> active = _activeObjects;
            for (OrionBehaviour* const b : active)
            {
                b->FixedUpdate();
            }
        }

        void BehaviourControlModule::ProcessUpdate()
        {
            const std::vector<OrionBehaviour*> active = _activeObjects;
            for (OrionBehaviour* const b : active)
            {
                b->Update();
            }
        }

        void BehaviourControlModule::ProcessLateUpdate()
        {
            const std::vector<OrionBehaviour*> active = _activeObjects;
            for (OrionBehaviour* const b : active)
            {
                b->LateUpdate();
            }
        }

        void BehaviourControlModule::SetEnabled(OrionBehaviour* b, bool enabled)
        {
            if (b->IsEnabled == enabled)
            {
                return;
            }

            b->IsEnabled = enabled;

            if (enabled)
            {
                b->OnEnable();
                _startQueue.push_back(b);
                _activeObjects.push_back(b);
            }
            else
            {
                b->OnDisable();
                Remove(_activeObjects, b);
            }
        }

        void BehaviourControlModule::Destroy(OrionBehaviour* b)
        {
            if (b->IsEnabled)
            {
                b->OnDisable();
            }

            b->OnDestroy();

            Remove(_activeObjects, b);
        }

        void BehaviourControlModule::DestroyAllBehaviours()
        {
            std::vector<OrionBehaviour*> behaviours;
            behaviours.swap(_allBehaviours);

            _newObjects.clear();
            _startQueue.clear();
            _activeObjects.clear();
            _disabledObjects.clear();
            _destroyedObjects.clear();

            for (OrionBehaviour* const behaviour : behaviours)
            {
                delete behaviour;
            }
        }
    }
}
