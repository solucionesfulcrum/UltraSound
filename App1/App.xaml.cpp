//
// App.xaml.cpp
// Implementación de la clase App.
//

#include "pch.h"
#include "MainPage.xaml.h"

using namespace App1;

using namespace Platform;
using namespace Windows::ApplicationModel;
using namespace Windows::ApplicationModel::Activation;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::UI::Xaml::Controls::Primitives;
using namespace Windows::UI::Xaml::Data;
using namespace Windows::UI::Xaml::Input;
using namespace Windows::UI::Xaml::Interop;
using namespace Windows::UI::Xaml::Media;
using namespace Windows::UI::Xaml::Navigation;

// La plantilla Aplicación vacía está documentada en http://go.microsoft.com/fwlink/?LinkId=234227

/// <summary>
/// Inicializa el objeto de aplicación Singleton.  Esta es la primera línea de código creado
/// ejecutado y, como tal, es el equivalente lógico de main() o WinMain().
/// </summary>
App::App()
{
	InitializeComponent();
	Suspending += ref new SuspendingEventHandler(this, &App::OnSuspending);
}

/// <summary>
/// Se invoca cuando la aplicación la inicia normalmente el usuario final.	Se usarán otros puntos
/// se usará por ejemplo cuando la aplicación se inicie para abrir un archivo específico.
/// </summary>
/// <param name="e">Información detallada acerca de la solicitud y el proceso de inicio.</param>
void App::OnLaunched(Windows::ApplicationModel::Activation::LaunchActivatedEventArgs^ e)
{

#if _DEBUG
		// Mostrar información de generación de perfiles gráfica durante la depuración.
		if (IsDebuggerPresent())
		{
			// Mostrar los contadores de velocidad de marcos actual
			 DebugSettings->EnableFrameRateCounter = true;
		}
#endif

	auto rootFrame = dynamic_cast<Frame^>(Window::Current->Content);

	// No repetir la inicialización de la aplicación si la ventana tiene contenido todavía,
	// solo asegurarse de que la ventana está activa.
	if (rootFrame == nullptr)
	{
		// Crear un marco para que actúe como contexto de navegación y asociarlo con
		// una clave SuspensionManager
		rootFrame = ref new Frame();

		// Establecer el idioma predeterminado
		rootFrame->Language = Windows::Globalization::ApplicationLanguages::Languages->GetAt(0);

		rootFrame->NavigationFailed += ref new Windows::UI::Xaml::Navigation::NavigationFailedEventHandler(this, &App::OnNavigationFailed);

		if (e->PreviousExecutionState == ApplicationExecutionState::Terminated)
		{
			// TODO: Restaurar el estado de sesión guardado solo si es apropiado, programando
			// los pasos de inicio finales una vez completada la restauración

		}

		if (rootFrame->Content == nullptr)
		{
			// Cuando no se restaura la pila de navegación para navegar a la primera página,
			// configurar la nueva página al pasar la información requerida como parámetro
			// parámetro
			rootFrame->Navigate(TypeName(MainPage::typeid), e->Arguments);
		}
		// Poner el marco en la ventana actual.
		Window::Current->Content = rootFrame;
		// Asegurarse de que la ventana actual está activa.
		Window::Current->Activate();
	}
	else
	{
		if (rootFrame->Content == nullptr)
		{
			// Cuando no se restaura la pila de navegación para navegar a la primera página,
			// configurar la nueva página al pasar la información requerida como parámetro
			// parámetro
			rootFrame->Navigate(TypeName(MainPage::typeid), e->Arguments);
		}
		// Asegurarse de que la ventana actual está activa.
		Window::Current->Activate();
	}
}

/// <summary>
/// Se invoca al suspender la ejecución de la aplicación.	El estado de la aplicación se guarda
/// sin saber si la aplicación se terminará o se reanudará con el contenido
/// de la memoria aún intacto.
/// </summary>
/// <param name="sender">Origen de la solicitud de suspensión.</param>
/// <param name="e">Detalles sobre la solicitud de suspensión.</param>
void App::OnSuspending(Object^ sender, SuspendingEventArgs^ e)
{
	(void) sender;	// Parámetro sin usar
	(void) e;	// Parámetro sin usar

	//TODO: Guardar el estado de la aplicación y detener toda actividad en segundo plano
}

/// <summary>
/// Se invoca cuando se produce un error en la navegación a una página determinada
/// </summary>
/// <param name="sender">Marco que produjo el error de navegación</param>
/// <param name="e">Detalles sobre el error de navegación</param>
void App::OnNavigationFailed(Platform::Object ^sender, Windows::UI::Xaml::Navigation::NavigationFailedEventArgs ^e)
{
	throw ref new FailureException("Failed to load Page " + e->SourcePageType.Name);
}