// Converted from the `public static class EngineStatistics` in OrionEngine/EngineCore.cs.
// The C# file exposed this through `global using static OrionEngine.EngineStatistics`;
// C++ has no such thing, so members are qualified as EngineStatistics::member.
#pragma once

namespace OrionEngine
{
    class EngineStatistics
    {
    public:
        static int totalFrames();
        static void IncrementFrameCount();
        static int currentFrame();

        inline static bool lockFrameRate = false;
        inline static float targetFrameRate = 60;
        inline static bool disableConsoleOutput = false;

    private:
        inline static int frameCount = 0;
    };
}
