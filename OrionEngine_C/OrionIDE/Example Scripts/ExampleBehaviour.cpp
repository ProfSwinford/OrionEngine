#include "ExampleBehaviour.h"

void ExampleBehaviour::Awake()
{
    Print("ExampleBehaviour Awake");
}

void ExampleBehaviour::OnEnable()
{
    Print("ExampleBehaviour OnEnable");
}

void ExampleBehaviour::Start()
{
    Print("ExampleBehaviour Start");
}

void ExampleBehaviour::FixedUpdate()
{
    if (count < 1)
    {
        Print("ExampleBehaviour FixedUpdate");
    }
}

void ExampleBehaviour::Update()
{
    // count++;
    // EngineUtils::ConsoleWriteLoadingDots(count, 5);

    if (Input::IsAnyKeyDown())
    {
    }
}

void ExampleBehaviour::LateUpdate()
{
    if (count < 1)
    {
        Print("ExampleBehaviour LateUpdate");
    }
}

void ExampleBehaviour::OnDisable()
{
    Print("ExampleBehaviour OnDisable");
}

void ExampleBehaviour::OnDestroy()
{
    Print("ExampleBehaviour OnDestroy");
}
