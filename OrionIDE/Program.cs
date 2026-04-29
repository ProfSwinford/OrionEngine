using OrionEngine;

namespace OrionIDE;

internal class Program
{
    static void Main(string[] args)
    {
        PongExample p = new PongExample();

        CharacterController c = new CharacterController();

        EngineCore.EngineCoreInit();

    }
}