using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;
using System.Threading.Tasks;

namespace Bau.LibrariesLibPowershell
{
	/// <summary>
	///     Manager de ejecución de scripts de powershell actuando como host
	/// </summary>
	public class PowerShellHostedManager
	{
		/// <summary>
		///		Inicializa el pool del espacio de ejecución
		/// </summary>
		public void InitializeRunspaces(int minRunspaces, int maxRunspaces, string[] modulesToLoad)
		{
			// create the default session state.
			// session state can be used to set things like execution policy, language constraints, etc.
			// optionally load any modules (by name) that were supplied.

			InitialSessionState defaultSessionState = InitialSessionState.CreateDefault();

				// Crea el estado de la sesión predeterminada. El estado de la sesión se puede utlizar para configurar la política de ejecución
				// restricciones del lenguaje y otras opciones y para cargar los módulos especificados por nombre
				defaultSessionState.ExecutionPolicy = Microsoft.PowerShell.ExecutionPolicy.Unrestricted;
				// Importa los módulos
				foreach (string moduleName in modulesToLoad)
					defaultSessionState.ImportPSModule(moduleName);
				// 
			// use the runspace factory to create a pool of runspaces
			// with a minimum and maximum number of runspaces to maintain.

			RsPool = RunspaceFactory.CreateRunspacePool(defaultSessionState);
			RsPool.SetMinRunspaces(minRunspaces);
			RsPool.SetMaxRunspaces(maxRunspaces);

			// set the pool options for thread use.
			// we can throw away or re-use the threads depending on the usage scenario.

			RsPool.ThreadOptions = PSThreadOptions.UseNewThread;

			// open the pool. 
			// this will start by initializing the minimum number of runspaces.

			RsPool.Open();
		}

		/// <summary>
		/// Runs a PowerShell script with parameters and prints the resulting pipeline objects to the console output. 
		/// </summary>
		public async Task RunScript(string scriptContents, Dictionary<string, object> scriptParameters)
		{
			if (RsPool == null)
			{
				throw new ApplicationException("Runspace Pool must be initialized before calling RunScript().");
			}

			// create a new hosted PowerShell instance using a custom runspace.
			// wrap in a using statement to ensure resources are cleaned up.

			using (PowerShell ps = PowerShell.Create())
			{
				// use the runspace pool.
				ps.RunspacePool = RsPool;

				// specify the script code to run.
				ps.AddScript(scriptContents);

				// specify the parameters to pass into the script.
				ps.AddParameters(scriptParameters);

				// subscribe to events from some of the streams
				ps.Streams.Error.DataAdded += Error_DataAdded;
				ps.Streams.Warning.DataAdded += Warning_DataAdded;
				ps.Streams.Information.DataAdded += Information_DataAdded;

				// execute the script and await the result.
				var pipelineObjects = await ps.InvokeAsync().ConfigureAwait(false);

				// print the resulting pipeline objects to the console.
				Console.WriteLine("----- Pipeline Output below this point -----");
				foreach (PSObject item in pipelineObjects)
				{
					Console.WriteLine(item.BaseObject.ToString());
				}
			}
		}

		/// <summary>
		/// Handles data-added events for the information stream.
		/// </summary>
		/// <remarks>
		/// Note: Write-Host and Write-Information messages will end up in the information stream.
		/// </remarks>
		private void Information_DataAdded(object sender, DataAddedEventArgs e)
		{
			if (sender is PSDataCollection<InformationRecord> streamObjectsReceived)
			{
				InformationRecord currentStreamRecord = streamObjectsReceived[e.Index];

				Console.WriteLine($"InfoStreamEvent: {currentStreamRecord.MessageData}");
			}
		}

		/// <summary>
		/// Handles data-added events for the warning stream.
		/// </summary>
		private void Warning_DataAdded(object sender, DataAddedEventArgs e)
		{
			var streamObjectsReceived = sender as PSDataCollection<WarningRecord>;
			var currentStreamRecord = streamObjectsReceived[e.Index];

			Console.WriteLine($"WarningStreamEvent: {currentStreamRecord.Message}");
		}

		/// <summary>
		/// Handles data-added events for the error stream.
		/// </summary>
		/// <remarks>
		/// Note: Uncaught terminating errors will stop the pipeline completely.
		/// Non-terminating errors will be written to this stream and execution will continue.
		/// </remarks>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Error_DataAdded(object sender, DataAddedEventArgs e)
		{
			var streamObjectsReceived = sender as PSDataCollection<ErrorRecord>;
			var currentStreamRecord = streamObjectsReceived[e.Index];

			Console.WriteLine($"ErrorStreamEvent: {currentStreamRecord.Exception}");
		}

		/// <summary>
		///		Pool de ejecución de PowerShell
		/// </summary>
		private RunspacePool RsPool { get; set; }
	}
}