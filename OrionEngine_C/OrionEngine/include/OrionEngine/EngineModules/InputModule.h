// Converted from OrionEngine/EngineModules/InputModule.cs.
//
// The C# module P/Invoked user32!GetAsyncKeyState, which is Windows only. This
// port keeps that path on Windows and adds a POSIX backend built on termios raw
// mode; see InputModule.cpp for the behavioural differences that implies.
#pragma once

#include "OrionEngine/Keycode.h"

namespace OrionEngine
{
    namespace EngineModules
    {
        class InputModule
        {
        public:
            /// <summary>Call once per frame to poll keyboard state.</summary>
            static void Update();

            static bool GetKey(Keycode key);
            static bool GetKeyDown(Keycode key);
            static bool GetKeyUp(Keycode key);
            static bool AnyKeyDown();

            /// <summary>
            /// Restores the terminal mode claimed by the POSIX backend. No-op on
            /// Windows. Invoked by EngineCore::Shutdown.
            /// </summary>
            static void Shutdown();
        };
    }
}
