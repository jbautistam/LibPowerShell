using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace Bau.Libraries.LibPowerShellManager
{
	/// <summary>
	///		Instancia de ejecución de un script de PowerShell
	/// </summary>
    internal class PowerShellInstance
    {
		// Eventos públicos
		public event EventHandler EndExecute;

		internal PowerShellInstance(string script, Dictionary<string, object> parameters)
		{
			Script = script;
			InputParameters = parameters;
		}

		/// <summary>
		///		Ejecuta un script de PowerShell
		/// </summary>
		internal void Process()
		{
			// Ejecuta el script
			try
			{
				using (PowerShell instance = PowerShell.Create())
				{
					Collection<PSObject> outputItems;

						// Añade el script a PowerShell
						instance.AddScript(Script);
						// Añade los parámetros de entrada
						if (InputParameters != null)
							foreach (KeyValuePair<string, object> parameter in InputParameters)
								instance.AddParameter(parameter.Key, parameter.Value);
						// Llama a la ejecución de PowerShell
						outputItems = instance.Invoke();
						// Guarda los valores de salida
						foreach (PSObject outputItem in outputItems)
							OutputObjects.Add(outputItem.BaseObject);
						// Guarda los errores
						if (instance.Streams.Error.Count > 0)
							foreach (ErrorRecord error in instance.Streams.Error)
								Errors.Add(error.ToString());
				}
			}
			catch (Exception exception)
			{
				Errors.Add(exception.Message);
			}
			// Llama al evento de fin de proceso
			EndExecute?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		///		Script de powerShell
		/// </summary>
		private string Script { get; }

		/// <summary>
		///		Parámetros de entrada
		/// </summary>
		private Dictionary<string, object> InputParameters { get; }

		/// <summary>
		///		Objetos de salida
		/// </summary>
		internal List<object> OutputObjects { get; } = new List<object>();

		/// <summary>
		///		Errores
		/// </summary>
		internal List<string> Errors { get; } = new List<string>();
    }
}
