// Converted from OrionIDE/Program.cs.
#include "OrionEngine/OrionEngine.h"

#include "Assets/Scripts/CharacterController.h"
#include "Example Scripts/PongExample.h"

using namespace OrionEngine;

namespace OrionIDE
{
    class Program
    {
    public:
        static int Main(int /*argc*/, char** /*argv*/)
        {
            // Behaviours register themselves with the engine on construction and are
            // owned by BehaviourControlModule, so `new` without a matching `delete`
            // mirrors the C# `new PongExample()` exactly.
            new PongExample();

            new CharacterController();

            EngineCore::EngineCoreInit();

            return 0;
        }
    };
}

int main(int argc, char** argv)
{
    return OrionIDE::Program::Main(argc, argv);
}
