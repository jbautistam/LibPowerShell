using System;
using System.Collections.Generic;
using System.Linq;

namespace Bau.Libraries.LibPowerShell
{
	/// <summary>
	///		Resultado de ejecución de un script de Powershell
	/// </summary>
	public class PowerShellResult
	{
		/// <summary>
		///		Añade un elemento de log (y lo devuelve por si lo queremos utilizar por ejemplo para lanzarlo en un evento)
		/// </summary>
		internal PowerShellLog AddLog(PowerShellLog.LogType type, string message, Exception exception = null)
		{
			PowerShellLog log = new PowerShellLog(type, message, exception);

				// Añade el log a la colección de salida
				Log.Add(log);
				// y devuelve el objeto por si lo queremos utilizar
				return log;
		}

		/// <summary>
		///		Objetos de salida
		/// </summary>
		public List<object> OutputObjects { get; } = new();

		/// <summary>
		///		Log de información
		/// </summary>
		public List<PowerShellLog> Log { get; } = new();

		/// <summary>
		///		Indica si ha habido algún error de ejecución
		/// </summary>
		public bool WithError
		{
			get { return Log is not null && Log.FirstOrDefault(item => item.Type == PowerShellLog.LogType.Error) is not null; }
		}
	}
}
