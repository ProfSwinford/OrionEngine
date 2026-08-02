# OrionIDE - console executable, equivalent of the OrionIDE.csproj Exe project.

add_executable(OrionIDE
    src/Program.cpp
    src/IDEProgram.cpp
    src/ConsoleRenderer.cpp

    "Assets/Scripts/CharacterController.cpp"
    "Assets/Scripts/CharacterController.h"
    "Assets/Scripts/Sprite Data/SpriteData.cpp"
    "Assets/Scripts/Sprite Data/SpriteData.h"

    "Example Scripts/DX11QuadExample.cpp"
    "Example Scripts/ExampleBehaviour.cpp"
    "Example Scripts/ExampleBehaviour.h"
    "Example Scripts/PongExample.cpp"
    "Example Scripts/PongExample.h"
)

target_link_libraries(OrionIDE PRIVATE OrionEngine::OrionEngine)

# Scripts include each other by path relative to the project root, e.g.
# #include "Example Scripts/PongExample.h".
target_include_directories(OrionIDE PRIVATE "${CMAKE_CURRENT_SOURCE_DIR}")

source_group(TREE "${CMAKE_CURRENT_SOURCE_DIR}" FILES
    $<TARGET_PROPERTY:OrionIDE,SOURCES>)

# Class templates and assets are content, not build inputs; copy them next to the
# executable so the layout matches the C# output folder.
add_custom_command(TARGET OrionIDE POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy_directory
        "${CMAKE_CURRENT_SOURCE_DIR}/ClassTemplates"
        "$<TARGET_FILE_DIR:OrionIDE>/ClassTemplates"
    COMMENT "Copying OrionIDE class templates")
