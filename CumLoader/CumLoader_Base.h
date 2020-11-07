#pragma once
#include "Mono.h"

class CumLoader_Base
{
public:
	enum CumCompatibility
	{
		UNIVERSAL,
		COMPATIBLE,
		NOATTRIBUTE,
		INCOMPATIBLE
	};

	static bool HasInitialized;
	static MonoMethod* startup;
	
	static void Initialize();
	static void Startup();
};