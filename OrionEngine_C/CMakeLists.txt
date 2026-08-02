cmake_minimum_required(VERSION 3.20)

project(OrionEngine_C
    VERSION 1.0.0
    DESCRIPTION "OrionEngine Education Edition - C++ conversion"
    LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

# Single output directory so the executable and library land together, mirroring
# the bin\Debug / bin\Release layout of the C# projects.
set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin")
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin")
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin")

# Emitted for clangd / IDE tooling.
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

if(NOT CMAKE_BUILD_TYPE AND NOT CMAKE_CONFIGURATION_TYPES)
    set(CMAKE_BUILD_TYPE Debug CACHE STRING "Build type" FORCE)
endif()

if(MSVC)
    add_compile_options(/W4 /permissive- /utf-8)
    add_compile_definitions(_CRT_SECURE_NO_WARNINGS NOMINMAX WIN32_LEAN_AND_MEAN)
else()
    add_compile_options(-Wall -Wextra -Wpedantic)
endif()

# The C# projects defined DEBUG in their Debug configuration; ORION_DEBUG is the
# equivalent switch that turns on OrionEngine::Debug logging.
add_compile_definitions($<$<CONFIG:Debug>:ORION_DEBUG>)

# Make OrionIDE the start-up project when generating a Visual Studio solution.
set_property(DIRECTORY PROPERTY VS_STARTUP_PROJECT OrionIDE)

add_subdirectory(OrionEngine)
add_subdirectory(OrionIDE)
