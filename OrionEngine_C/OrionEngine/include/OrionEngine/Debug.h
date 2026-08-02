// Converted from the `#if DEBUG public static class Debug` block in OrionEngine/EngineCore.cs.
#pragma once

#include <string>

#include "OrionEngine/EngineUtils.h"
#include "OrionEngine/Object.h"

namespace OrionEngine
{
    /// <summary>
    /// Provides a set of methods and properties for debugging applications. This class offers functionality to help
    /// diagnose issues and monitor application behavior during development.
    /// </summary>
    /// <remarks>
    /// The C# class sat inside `#if DEBUG`, which meant release builds could not
    /// compile the call sites in EngineCore. The C++ port always declares the class
    /// and instead defaults ENABLE_DEBUG_LOGS to true only when ORION_DEBUG is
    /// defined (the CMake Debug configuration), so both configurations build.
    /// </remarks>
    class Debug
    {
    public:
#if defined(ORION_DEBUG)
        inline static bool ENABLE_DEBUG_LOGS = true;
#else
        inline static bool ENABLE_DEBUG_LOGS = false;
#endif

        enum class LogType
        {
            LOG,
            WARNING,
            ERROR_,  // `ERROR` is a macro in several Windows headers.
            MESSAGE,
            NONE
        };

        static void Log(const Object& message);
        static void LogWarning(const Object& message);
        static void LogError(const Object& message);
        static void LogMessage(const Object& message);
        static void LogMessage(const ColoredText& message);

        static void DebugOut(LogType type, const std::string& message, bool TimeStamp = true, char end = '\n');
    };
}
