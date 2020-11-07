#include <Windows.h>
#include "CumLoader.h"
#include "CumLoader_Base.h"

BOOL WINAPI DllMain(HINSTANCE hinstDLL, DWORD fdwReason, LPVOID lpvReserved)
{
	CumLoader::thisdll = hinstDLL;
	if (fdwReason == DLL_PROCESS_ATTACH)
	{
#ifndef DEBUG
		DisableThreadLibraryCalls(CumLoader::thisdll);
#endif
		CumLoader::Main();
	}
	else if (fdwReason == DLL_PROCESS_DETACH)
	{
		CumLoader::UNLOAD();
		FreeLibrary(CumLoader::thisdll);
	}
	return TRUE;
}