using System;

namespace Bau.Libraries.LibPowerShell
{
	/// <summary>
	///		Log de salida de powerShell
	/// </summary>
	public class PowerShellLog
	{
		/// <summary>
		///		Tipo de log
		/// </summary>
		public enum LogType
		{
			/// <summary>Informativo</summary>
			Info,
			/// <summary>Advertencia</summary>
			Warning,
			/// <summary>Error</summary>
			Error
		}

		public PowerShellLog(LogType type, string message, Exception exception = null)
		{
			Type = type;
			Message = message;
			Exception = exception;
		}

		/// <summary>
		///		Tipo de log
		/// </summary>
		public LogType Type { get; }

		/// <summary>
		///		Fecha de generación
		/// </summary>
		public DateTime Date { get; } = DateTime.UtcNow;

		/// <summary>
		///		Mensaje de log
		/// </summary>
		public string Message { get; }

		/// <summary>
		///		Excepción
		/// </summary>
		public Exception Exception { get; }
	}
}
