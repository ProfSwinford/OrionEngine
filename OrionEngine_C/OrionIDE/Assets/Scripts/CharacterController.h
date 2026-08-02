// Converted from OrionIDE/Assets/Scripts/CharacterController.cs.
#pragma once

#include "OrionEngine/OrionEngine.h"

using namespace OrionEngine;

class CharacterController : public OrionBehaviour
{
public:
    void Start() override;
    void Update() override;

private:
    GameObject2D* character = nullptr;

    int floor = 0;
};
