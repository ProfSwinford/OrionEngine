# OrionEngine - static library, equivalent of the OrionEngine.csproj class library.

add_library(OrionEngine STATIC
    src/Console.cpp
    src/Debug.cpp
    src/EngineCore.cpp
    src/EngineStatistics.cpp
    src/EngineUtils.cpp
    src/Object.cpp
    src/OrionBehaviour.cpp
    src/Pixel.cpp
    src/Vector2.cpp
    src/EngineModules/BehaviourControlModule.cpp
    src/EngineModules/FrameStateModule.cpp
    src/EngineModules/InputModule.cpp
    src/EngineModules/Physics/ConsolePhysicsModule.cpp
    src/EngineModules/Rendering/ConsoleRendererModule.cpp

    include/OrionEngine/Console.h
    include/OrionEngine/Debug.h
    include/OrionEngine/EngineCore.h
    include/OrionEngine/EngineStatistics.h
    include/OrionEngine/EngineUtils.h
    include/OrionEngine/Keycode.h
    include/OrionEngine/Object.h
    include/OrionEngine/OrionBehaviour.h
    include/OrionEngine/OrionEngine.h
    include/OrionEngine/Pixel.h
    include/OrionEngine/Vector2.h
    include/OrionEngine/EngineModules/BehaviourControlModule.h
    include/OrionEngine/EngineModules/FrameStateModule.h
    include/OrionEngine/EngineModules/InputModule.h
    include/OrionEngine/EngineModules/Physics/ConsolePhysicsModule.h
    include/OrionEngine/EngineModules/Rendering/ConsoleRendererModule.h
)

add_library(OrionEngine::OrionEngine ALIAS OrionEngine)

target_include_directories(OrionEngine PUBLIC
    $<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}/include>
    $<INSTALL_INTERFACE:include>)

target_compile_features(OrionEngine PUBLIC cxx_std_20)

if(WIN32)
    # GetAsyncKeyState lives in user32, exactly as the C# [DllImport("user32.dll")].
    target_link_libraries(OrionEngine PRIVATE user32)
endif()

# Group the headers alongside their sources in Visual Studio / Xcode.
source_group(TREE "${CMAKE_CURRENT_SOURCE_DIR}" FILES
    $<TARGET_PROPERTY:OrionEngine,SOURCES>)
