// Converted from OrionIDE/Example Scripts/PongExample.cs.
#pragma once

#include "OrionEngine/OrionEngine.h"

using namespace OrionEngine;

class PongExample : public OrionBehaviour
{
public:
    void Awake() override;
    void Update() override;

    void ModifiedPong();
    void OriginalPong();

    void LateUpdate() override;

private:
    GameObject2D* pongObj = nullptr;
    int _x = 0;
    int _y = 0;
    int _xVel = 0;
    int _yVel = 0;

    int color = 0;
};
