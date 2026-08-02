# OrionEngine_C

OrionEngine Education Edition — a direct C++ conversion of the C# `OrionEngine`
project.

Every type, module, field and lifecycle stage from the C# original is carried
over, including the commented-out placeholders. The engine still renders to the
terminal, still drives behaviours through the same Unity-style execution order,
and still ships the same Pong / character-controller examples — but it now builds
with CMake on Windows, Linux and macOS instead of targeting .NET Framework 4.8.

## Building

Requires CMake 3.20+ and a C++20 compiler (MSVC 19.29+, GCC 10+, Clang 12+).

```sh
cmake --preset debug
cmake --build --preset debug
./build/debug/bin/OrionIDE          # Windows: build\debug\bin\Debug\OrionIDE.exe
```

Without presets:

```sh
cmake -S . -B build -DCMAKE_BUILD_TYPE=Debug
cmake --build build -j
./build/bin/OrionIDE
```

For a Visual Studio solution (`build/vs2022/OrionEngine_C.sln`, with `OrionIDE`
preselected as the start-up project):

```sh
cmake --preset vs2022
cmake --build --preset vs2022-debug
```

`Debug` builds define `ORION_DEBUG`, which is what enables
`OrionEngine::Debug` logging — the equivalent of the C# `DEBUG` constant.

**Controls:** `WASD` moves the character, `Space` freezes the Pong object,
`Escape` quits. Run it in a real terminal; output is a live-updating canvas.

## Layout

```
OrionEngine_C/
├── CMakeLists.txt              root project
├── CMakePresets.json
├── OrionEngine/                static library  (was OrionEngine.csproj)
│   ├── include/OrionEngine/    public headers
│   └── src/                    implementation
└── OrionIDE/                   console executable (was OrionIDE.csproj)
    ├── src/                    entry point + disabled placeholders
    ├── Assets/Scripts/         user scripts
    ├── Example Scripts/
    └── ClassTemplates/
```

### File mapping

| C# source | C++ conversion |
| --- | --- |
| `OrionEngine/EngineCore.cs` | `EngineCore.{h,cpp}`, `EngineUtils.{h,cpp}`, `EngineStatistics.{h,cpp}`, `Debug.{h,cpp}` |
| `OrionEngine/Object.cs` | `Object.{h,cpp}`, `Vector2.{h,cpp}` |
| `OrionEngine/OrionBehaviour.cs` | `OrionBehaviour.{h,cpp}` |
| `OrionEngine/EngineModules/InputModule.cs` | `EngineModules/InputModule.{h,cpp}`, `Keycode.h` |
| `OrionEngine/EngineModules/FrameStateModule.cs` | `EngineModules/FrameStateModule.{h,cpp}` |
| `OrionEngine/EngineModules/BehaviourControlModule.cs` | `EngineModules/BehaviourControlModule.{h,cpp}` |
| `OrionEngine/EngineModules/Rendering/ConsoleRendererModule.cs` | `EngineModules/Rendering/ConsoleRendererModule.{h,cpp}`, `Pixel.{h,cpp}` |
| `OrionEngine/EngineModules/Physics/ConsolePhysicsModule.cs` | `EngineModules/Physics/ConsolePhysicsModule.{h,cpp}` |
| `OrionIDE/Program.cs` | `OrionIDE/src/Program.cpp` |
| `OrionIDE/IDEProgram.cs`, `ConsoleRenderer.cs`, `Example Scripts/DX11QuadExample.cs` | placeholder `.cpp` files (all three are fully commented out in C#) |
| `OrionIDE/**/*.cs` scripts | matching `.h` / `.cpp` pairs |
| `OrionIDE/ClassTemplates/NewBehaviour.txt` | `NewBehaviour.h.txt` + `NewBehaviour.cpp.txt` |
| `*.csproj`, `packages.config`, `app.config`, `AssemblyInfo.cs`, `Orion.slnx` | `CMakeLists.txt`, `CMakePresets.json` |

## What could not convert directly

**Reflection → virtual methods.** `OrionBehaviour.CheckLifecycleMethods()` used
`Type.GetMethod` + `Delegate.CreateDelegate` to discover whether a script
implemented `Awake`, `Update`, and so on. C++ has no reflection, so the eight
lifecycle stages are `virtual` methods with empty defaults; overriding one opts
in exactly as declaring it did in C#. Scripts change from
`public void Update()` to `void Update() override`.

**`System.Console` → `OrionEngine::Console`.** Cursor addressing, colors and
window metrics have no standard C++ equivalent. `Console.{h,cpp}` reimplements
the subset the engine used on top of the Win32 console API, and on POSIX with
ANSI escape sequences plus `ioctl(TIOCGWINSZ)`. `ConsoleColor` keeps the .NET
numeric values. The POSIX backend batches a whole frame into one `write()`.

**`GetAsyncKeyState` → per-platform input.** The C# module P/Invoked
`user32.dll`, which is Windows-only. Windows keeps that path verbatim (the
virtual-key table is unchanged). POSIX puts the terminal in raw mode via
`termios` and decodes bytes and escape sequences. A terminal reports keystrokes
rather than key up/down edges, so a key counts as held for 350 ms after its most
recent byte — terminal auto-repeat refreshes that window while it is physically
down. `GetKeyDown` / `GetKeyUp` edges still work; a genuinely held key may show a
short gap before auto-repeat starts. Tune `kKeyHoldDuration` in
`InputModule.cpp` if that feels wrong for your terminal.

**Garbage collection → explicit ownership.** Behaviours and GameObjects are
still created with `new` and never deleted by user code, matching C# reference
semantics. `BehaviourControlModule` owns every behaviour from construction and
`GameObject` keeps a registry of every instance; `EngineCore::Shutdown()` frees
both. GameObjects and behaviours must therefore be heap allocated. Because
`PongExample` exits through `std::exit(0)` (the C# `Environment.Exit(0)`), the
console and terminal restore is also registered with `std::atexit`.

**`OrionEngine.Object` → `std::variant`.** The C# class boxed an arbitrary
`object` payload and inherited from `System.Object`. C++ has no universal base
class, so the payload is a `std::variant` and the same `TypeTag`, `As<T>`,
`TryAs<T>` and conversion operators sit on top of it. `TryAs<T>` takes an out
parameter instead of `out T?`.

**`Pixel[,]` → `PixelGrid`.** C++ has no rectangular array type;
`using PixelGrid = std::vector<std::vector<Pixel>>` indexed `[y][x]` keeps the
nested brace-initializer sprite literals working unchanged.

**`char` → `char32_t`.** `Pixel.Character` was a UTF-16 `char`. It is now
`OrionChar` (`char32_t`) so the box-drawing border characters survive on every
platform; UTF-8 encoding happens at the console boundary. ASCII literals such as
`Pixel('o')` still compile as-is.

**`global using` → umbrella header.** `global using static OrionEngine.EngineUtils`
and friends become `#include "OrionEngine/OrionEngine.h"` plus
`using namespace OrionEngine;`. Members are qualified (`EngineUtils::Print`,
`EngineStatistics::disableConsoleOutput`).

**`event Action? OnRender` → callback list.** Exposed as `AddOnRender()` /
`ClearOnRender()` over a `std::vector<std::function<void()>>`.

**`#if DEBUG` around `Debug`.** In C# the whole class was compiled out of
release builds, which would have broken every call site in `EngineCore`. The C++
class is always declared; `ENABLE_DEBUG_LOGS` just defaults to `false` unless
`ORION_DEBUG` is defined, so both configurations build.

**Singletons.** `private static readonly X instance = new X()` becomes a
function-local static (`EngineCore::OrionEngine()`, `FrameStateModule::FrameCycle()`,
`BehaviourControlModule::BCM()`, `ConsoleRendererModule::canvas()`). The C# retry
loops checked those singletons against `null`, which a reference cannot be, so
they now check a construction flag — the loop shape is kept for parity.

**Properties → methods.** `canvas.Width` is `canvas().Width()`,
`behaviour.Enabled = false` is `behaviour.Enabled(false)`, and
`gameObject.transform.position` is `gameObject->transform()->position`.

## Behaviour deliberately preserved

These are quirks of the C# original that were carried over rather than fixed, so
the two engines behave the same:

- `BehaviourControlModule::ProcessNewObjects` bails out of the whole queue (C#
  `return`, not `continue`) on the first disabled behaviour, leaving the queue to
  be retried next frame.
- `ConsoleCanvas::Render` recurses on an out-of-range cursor write.
- The two-argument `ConsoleCanvas(bool, bool)` constructor forwards its arguments
  one slot to the left, so `interlaced` lands on `drawBorder`.
- `EngineUtils::WaitForInput` never pumps the input module inside its loop.
- The frame cycle runs uncapped: the frame-rate sleep is commented out in both
  versions, and `EngineStatistics::lockFrameRate` / `targetFrameRate` are still
  unused.

Two changes were needed for memory safety rather than parity: the
`Process*` methods iterate over a snapshot (in C#, mutating a collection
mid-`foreach` throws; in C++ it is undefined behaviour), and `ProcessNewObjects`
/ `ProcessStart` consume only the batch they started with, so behaviours created
inside `Awake` or `Start` roll into the next frame instead of being dropped.
