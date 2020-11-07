#include <Windows.h>
#include <string>
#include "CumLoader_Base.h"
#include "CumLoader.h"
#include "AssertionManager.h"
#include "Logger.h"
#include "HookManager.h"
#include "Il2Cpp.h"

bool CumLoader_Base::HasInitialized = false;
MonoMethod* CumLoader_Base::startup = NULL;

void CumLoader_Base::Initialize()
{
	AssertionManager::Start("CumLoader_Base.cpp", "CumLoader_Base::Initialize");
	if (Mono::Domain != NULL)
	{
		std::string modhandlerpath = std::string(CumLoader::GamePath) + "\\CumLoader\\CumLoader.ModHandler.dll";
		MonoAssembly* assembly = Mono::mono_domain_assembly_open(Mono::Domain, modhandlerpath.c_str());
		AssertionManager::Decide(assembly, "CumLoader.ModHandler.dll");
		if (assembly != NULL)
		{
			MonoImage* image = Mono::mono_assembly_get_image(assembly);
			AssertionManager::Decide(assembly, "Image");
			if (image != NULL)
			{
				MonoClass* klass = Mono::mono_class_from_name(image, "CumLoader", "CumLoaderBase");
				AssertionManager::Decide(assembly, "CumLoader.CumLoaderBase");
				if (klass != NULL)
				{
					MonoMethod* initialize = Mono::mono_class_get_method_from_name(klass, "Initialize", NULL);
					AssertionManager::Decide(initialize, "Initialize");
					if (initialize != NULL)
					{
						MonoObject* exceptionObject = NULL;
						Mono::mono_runtime_invoke(initialize, NULL, NULL, &exceptionObject);
						if ((exceptionObject != NULL) && CumLoader::DebugMode)
							Mono::LogExceptionMessage(exceptionObject);
						else
						{
							startup = Mono::mono_class_get_method_from_name(klass, "Startup", NULL);
							AssertionManager::Decide(startup, "Startup");
							if (CumLoader::IsGameIl2Cpp)
								HookManager::Hook(&(LPVOID&)Il2Cpp::il2cpp_runtime_invoke, HookManager::Hooked_runtime_invoke);
							else
								HookManager::Hook(&(LPVOID&)Mono::mono_runtime_invoke, HookManager::Hooked_runtime_invoke);
							HasInitialized = true;
						}
					}
				}
			}
		}
	}
}

void CumLoader_Base::Startup()
{
	if (startup != NULL)
	{
		MonoObject* exceptionObject = NULL;
		Mono::mono_runtime_invoke(startup, NULL, NULL, &exceptionObject);
		if ((exceptionObject != NULL) && CumLoader::DebugMode)
			Mono::LogExceptionMessage(exceptionObject);
	}
}