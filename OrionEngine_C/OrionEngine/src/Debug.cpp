#include "OrionEngine/Debug.h"

#include <chrono>
#include <ctime>

#include "OrionEngine/Console.h"
#include "OrionEngine/EngineStatistics.h"

namespace OrionEngine
{
    namespace
    {
        /// <summary>Equivalent of DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ").</summary>
        std::string TimeStampNow()
        {
            const auto now = std::chrono::system_clock::now();
            const std::time_t time = std::chrono::system_clock::to_time_t(now);

            std::tm local{};
#if defined(_WIN32)
            localtime_s(&local, &time);
#else
            localtime_r(&time, &local);
#endif

            char buffer[32] = {};
            std::strftime(buffer, sizeof(buffer), "%Y-%m-%d %H:%M:%S ", &local);
            return std::string(buffer);
        }
    }

    void Debug::DebugOut(LogType type, const std::string& message, bool TimeStamp, char end)
    {
        if (!ENABLE_DEBUG_LOGS || EngineStatistics::disableConsoleOutput)
        {
            return;
        }

        const ConsoleColor prev = Console::ForegroundColor();  // Store current console text color

        EngineUtils::Print((TimeStamp ? TimeStampNow() : std::string()) + "[");
        switch (type)
        {
            case LogType::LOG:
                ColoredText("LOG", ConsoleColor::Green).Write();
                break;
            case LogType::WARNING:
                ColoredText("WRN", ConsoleColor::Yellow).Write();
                break;
            case LogType::ERROR_:
                ColoredText("ERR", ConsoleColor::Red).Write();
                break;
            case LogType::MESSAGE:
            default:
                EngineUtils::Print("MSG");
                break;
        }

        EngineUtils::Print("] |\t" + message, end);
        Console::SetForegroundColor(prev);  // Restore previous console text color
    }

    void Debug::Log(const Object& message)
    {
        DebugOut(LogType::LOG, message.ToString());
    }

    void Debug::LogWarning(const Object& message)
    {
        DebugOut(LogType::WARNING, message.ToString());
    }

    void Debug::LogError(const Object& message)
    {
        DebugOut(LogType::ERROR_, message.ToString());
    }

    void Debug::LogMessage(const Object& message)
    {
        DebugOut(LogType::MESSAGE, message.ToString());
    }

    void Debug::LogMessage(const ColoredText& message)
    {
        DebugOut(LogType::MESSAGE, "", true, '\0');
        message.Write();
        EngineUtils::Print();
    }
}
