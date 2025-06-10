#pragma once

#include "ColorImageEngineServer.h" // Incluye el archivo con la clase nativa
using namespace System;

namespace CIESWrapper {
	public ref class ColorImageEngineWrapper
	{
	private:
		CIES::ColorImageEngineServer* engine; // Puntero a la clase nativa

	public:
		// Constructor que inicializa el objeto nativo
		ColorImageEngineWrapper(CIES::DataSourceInterface &dsInterface)
		{
			engine = new CIES::ColorImageEngineServer(dsInterface);
		}

		// Destructor para liberar memoria
		~ColorImageEngineWrapper()
		{
			delete engine;
		}

		// Método público de C++/CLI que llama al método nativo
		int StartImageEngine()
		{
			return engine->StartImageEngine();
		}

		int StopImageEngine()
		{
			return engine->StopImageEngine();
		}

		bool IsRunning()
		{
			return engine->IsRuning();
		}

		// Puedes agregar más métodos de esta manera
	};
}
