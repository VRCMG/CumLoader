#include "Exports.h"
#include "CumLoader.h"
#include "Il2Cpp.h"
#include "Mono.h"
#include "HookManager.h"
#include "Logger.h"
#include "AssertionManager.h"

void Log(MonoString* namesection, MonoString* txt) { Logger::Log(Mono::mono_string_to_utf8(namesection), Mono::mono_string_to_utf8(txt)); }
void LogColor(MonoString* namesection, MonoString* txt, ConsoleColor color) { Logger::Log(Mono::mono_string_to_utf8(namesection), Mono::mono_string_to_utf8(txt), color); }
void LogWarning(MonoString* namesection, MonoString* txt) { Logger::LogWarning(Mono::mono_string_to_utf8(namesection), Mono::mono_string_to_utf8(txt)); }
void LogError(MonoString* namesection, MonoString* txt) { Logger::LogError(Mono::mono_string_to_utf8(namesection), Mono::mono_string_to_utf8(txt)); }
void LogCumError(MonoString* namesection, MonoString* txt) { Logger::LogCumError(Mono::mono_string_to_utf8(namesection), Mono::mono_string_to_utf8(txt)); }
void LogCumCompatibility(CumLoader_Base::CumCompatibility comp) { Logger::LogCumCompatibility(comp); }
bool IsIl2CppGame() { return CumLoader::IsGameIl2Cpp; }
bool IsDebugMode() { return CumLoader::DebugMode; }
bool IsConsoleEnabled() { return Console::Enabled; }
bool ShouldShowGameLogs() { return Console::ShouldShowGameLogs; }
MonoString* GetGameDirectory() { return Mono::mono_string_new(Mono::Domain, CumLoader::GamePath); }
MonoString* GetGameDataDirectory() { return Mono::mono_string_new(Mono::Domain, CumLoader::DataPath); }
void Hook(Il2CppMethod* target, void* detour) { HookManager::Hook(target, detour); }
void Unhook(Il2CppMethod* target, void* detour) { HookManager::Unhook(target, detour); }
bool IsOldMono() { return Mono::IsOldMono; }
MonoString* GetCompanyName() { return Mono::mono_string_new(Mono::Domain, ((CumLoader::CompanyName == NULL) ? "UNKNOWN" : CumLoader::CompanyName)); }
MonoString* GetProductName() { return Mono::mono_string_new(Mono::Domain, ((CumLoader::ProductName == NULL) ? "UNKNOWN" : CumLoader::ProductName)); }
MonoString* GetAssemblyDirectory() { return Mono::mono_string_new(Mono::Domain, Mono::AssemblyPath); }
MonoString* GetMonoConfigDirectory() { return Mono::mono_string_new(Mono::Domain, Mono::ConfigPath); }
MonoString* GetExePath() { return Mono::mono_string_new(Mono::Domain, CumLoader::ExePath); }
bool IsQuitFix() { return CumLoader::QuitFix; }
CumLoader::LoadMode GetLoadMode_CumPlugins() { return CumLoader::LoadMode_CumPlugins; }
CumLoader::LoadMode GetLoadMode_CumMods() { return CumLoader::LoadMode_CumMods; }
bool AG_Force_Regenerate() { return CumLoader::AG_Force_Regenerate; }
MonoString* AG_Force_Version_Unhollower() { if (CumLoader::ForceUnhollowerVersion != NULL) return Mono::mono_string_new(Mono::Domain, CumLoader::ForceUnhollowerVersion); return NULL; }
void SetTitleForConsole(MonoString* txt) { Console::SetTitle(Mono::mono_string_to_utf8(txt)); }
void ThrowInternalError(MonoString* txt) { AssertionManager::ThrowInternalError(Mono::mono_string_to_utf8(txt)); }
HANDLE GetConsoleOutputHandle() { return Console::OutputHandle; }

void Exports::AddInternalCalls()
{
	Mono::mono_add_internal_call("CumLoader.Imports::IsIl2CppGame", IsIl2CppGame);
	Mono::mono_add_internal_call("CumLoader.Imports::IsDebugMode", IsDebugMode);
	Mono::mono_add_internal_call("CumLoader.Imports::GetGameDirectory", GetGameDirectory);
	Mono::mono_add_internal_call("CumLoader.Imports::GetGameDataDirectory", GetGameDataDirectory);
	Mono::mono_add_internal_call("CumLoader.Imports::GetAssemblyDirectory", GetAssemblyDirectory);
	Mono::mono_add_internal_call("CumLoader.Imports::GetMonoConfigDirectory", GetMonoConfigDirectory);
	Mono::mono_add_internal_call("CumLoader.Imports::Hook", Hook);
	Mono::mono_add_internal_call("CumLoader.Imports::Unhook", Unhook);
	Mono::mono_add_internal_call("CumLoader.Imports::GetCompanyName", GetCompanyName);
	Mono::mono_add_internal_call("CumLoader.Imports::GetProductName", GetProductName);
	Mono::mono_add_internal_call("CumLoader.Imports::GetExePath", GetExePath);

	Mono::mono_add_internal_call("CumLoader.CumLoaderBase::IsOldMono", IsOldMono);
	Mono::mono_add_internal_call("CumLoader.CumLoaderBase::IsQuitFix", IsQuitFix);
	Mono::mono_add_internal_call("CumLoader.CumLoaderBase::UNLOAD", CumLoader::UNLOAD);

	Mono::mono_add_internal_call("CumLoader.CumHandler::GetLoadMode_CumPlugins", GetLoadMode_CumPlugins);
	Mono::mono_add_internal_call("CumLoader.CumHandler::GetLoadMode_CumMods", GetLoadMode_CumMods);

	Mono::mono_add_internal_call("CumLoader.CumConsole::Allocate", Console::Create);
	Mono::mono_add_internal_call("CumLoader.CumConsole::SetTitle", SetTitleForConsole);
	Mono::mono_add_internal_call("CumLoader.CumConsole::SetColor", Console::SetColor);
	Mono::mono_add_internal_call("CumLoader.CumConsole::IsConsoleEnabled", IsConsoleEnabled);

	Mono::mono_add_internal_call("CumLoader.CumLogger::Native_Log", Log);
	Mono::mono_add_internal_call("CumLoader.CumLogger::Native_LogColor", LogColor);
	Mono::mono_add_internal_call("CumLoader.CumLogger::Native_LogWarning", LogWarning);
	Mono::mono_add_internal_call("CumLoader.CumLogger::Native_LogError", LogError);
	Mono::mono_add_internal_call("CumLoader.CumLogger::Native_LogCumError", LogCumError);
	Mono::mono_add_internal_call("CumLoader.CumLogger::Native_LogCumCompatibility", LogCumCompatibility);
	Mono::mono_add_internal_call("CumLoader.CumLogger::Native_ThrowInternalError", ThrowInternalError);
	Mono::mono_add_internal_call("CumLoader.CumLogger::Native_GetConsoleOutputHandle", GetConsoleOutputHandle);

	Mono::mono_add_internal_call("CumLoader.AssemblyGenerator::Force_Regenerate", AG_Force_Regenerate);
	Mono::mono_add_internal_call("CumLoader.AssemblyGenerator::Force_Version_Unhollower", AG_Force_Version_Unhollower);
}