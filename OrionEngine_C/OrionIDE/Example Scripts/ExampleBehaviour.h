// Converted from OrionIDE/Example Scripts/ExampleBehaviour.cs.
#pragma once

#include "OrionEngine/OrionEngine.h"

using namespace OrionEngine;

class ExampleBehaviour : public OrionBehaviour
{
public:
    void Awake() override;
    void OnEnable() override;
    void Start() override;
    void FixedUpdate() override;
    void Update() override;
    void LateUpdate() override;
    void OnDisable() override;
    void OnDestroy() override;

private:
    int count = 0;
};
