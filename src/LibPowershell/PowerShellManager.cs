using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;
using System.Threading.Tasks;

namespace Bau.Libraries.LibPowerShell
{
	/// <summary>
	///		Instancia de ejecución de un script de PowerShell
	/// </summary>
	/// <remarks>https://keithbabinec.com/2020/02/15/how-to-run-powershell-core-scripts-from-net-core-applications/</remarks>
    public class PowerShellManager
    {
		// Eventos públicos
		public event EventHandler EndExecute;
		// Variables globales
		private PowerShellResult _result;

		public PowerShellManager(PowerShellContext context)
		{
			Context = context;
		}

		/// <summary>
		///		Ejecuta un script de PowerShell
		/// </summary>
		public async Task<PowerShellResult> ProcessAsync(CancellationToken cancellationToken)
		{
			// Genera un resultado
			_result = new();
			// Ejecuta el script
			try
			{
				using (PowerShell instance = PowerShell.Create())
				{
					PSDataCollection<PSObject> outputItems;

						// Si se ha solicitado ejecutar en un espacio de ejecución, se crea
						if (Context.UseRunSpace)
							instance.RunspacePool = CreateRunspace(Context.MinRunSpaces, Context.MaxRunSpace, Context.Modules);
						// Añade el script a PowerShell
						instance.AddScript(Context.Script);
						// Añade los parámetros de entrada
						if (Context.InputParameters is not null)
							instance.AddParameters(Context.InputParameters);
						// En caso que los estemos ejecutando en un espacio separado, añadimos el tratamiento de los eventos
						//if (Context.UseRunSpace)
						//{
							instance.Streams.Error.DataAdded += (sender, args) => TreatStreamInfo(sender, args);
							instance.Streams.Warning.DataAdded += (sender, args) => TreatStreamInfo(sender, args);
							instance.Streams.Information.DataAdded += (sender, args) => TreatStreamInfo(sender, args);
						//}
						// Llama a la ejecución de PowerShell
						outputItems = await instance.InvokeAsync();
						// Guarda los valores de salida
						foreach (PSObject outputItem in outputItems)
							_result.OutputObjects.Add(outputItem.BaseObject);
						//// Guarda los errores
						//if (instance.Streams.Error.Count > 0)
						//	foreach (ErrorRecord error in instance.Streams.Error)
						//		_result.AddLog(PowerShellLog.LogType.Error, error.ToString(), error.Exception);
				}
			}
			catch (Exception exception)
			{
				_result.AddLog(PowerShellLog.LogType.Error, exception.Message, exception);
			}
			// Llama al evento de fin de proceso
			EndExecute?.Invoke(this, EventArgs.Empty);
			// Devuelve el resultado
			return _result;
		}

		/// <summary>
		///		Trata la información enviada en los eventos de PowerShell
		/// </summary>
		private void TreatStreamInfo(object sender, DataAddedEventArgs args)
		{
			switch (sender)
			{
				case PSDataCollection<InformationalRecord> records:
						_result.AddLog(PowerShellLog.LogType.Info, records[args.Index].Message);
					break;
				case PSDataCollection<WarningRecord> records:
						_result.AddLog(PowerShellLog.LogType.Warning, records[args.Index].Message);
					break;
				case PSDataCollection<ErrorRecord> records:
						_result.AddLog(PowerShellLog.LogType.Error, records[args.Index].ToString(), records[args.Index].Exception);
					break;
			}
		}

		/// <summary>
		///		Inicializa el pool del espacio de ejecución
		/// </summary>
		private RunspacePool CreateRunspace(int minRunspaces, int maxRunspaces, List<string> modulesToLoad)
		{
			RunspacePool pool = RunspaceFactory.CreateRunspacePool(CreateSession(modulesToLoad));

				// Asigna los datos del pool
				pool.SetMinRunspaces(minRunspaces);
				pool.SetMaxRunspaces(maxRunspaces);
				// Configura las opciones del pool para la utilización de hilos. Se pueden reutilizar los hilos o eliminarlos dependiendo del escenario
				pool.ThreadOptions = PSThreadOptions.UseNewThread;
				// Abre el pool, esto lo arrancará inicializándolo al número mínimo de espacios de ejecución
				pool.Open();
				// Devuelve el pool creado
				return pool;
		}

		/// <summary>
		///		Crea el estado de la sesión
		/// </summary>
		private InitialSessionState CreateSession(List<string> modules)
		{
			InitialSessionState sessionState = InitialSessionState.CreateDefault();

				// Crea el estado de la sesión predeterminada. El estado de la sesión se puede utilizar para configurar la política de ejecución
				// restricciones del lenguaje y otras opciones y para cargar los módulos especificados por nombre
				sessionState.ExecutionPolicy = Microsoft.PowerShell.ExecutionPolicy.Unrestricted;
				// Importa los módulos
				if (modules is not null)
					foreach (string moduleName in modules)
						sessionState.ImportPSModule(moduleName);
				// Devuelve la sesión
				return sessionState;
		}

		/// <summary>
		///		Contexto de ejecución
		/// </summary>
		public PowerShellContext Context { get; }
    }
}