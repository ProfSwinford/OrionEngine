#include "OrionEngine/EngineStatistics.h"

namespace OrionEngine
{
    int EngineStatistics::totalFrames()
    {
        return frameCount;
    }

    void EngineStatistics::IncrementFrameCount()
    {
        frameCount++;
    }

    int EngineStatistics::currentFrame()
    {
        return totalFrames() + 1;
    }
}
