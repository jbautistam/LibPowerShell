using System;

namespace Bau.Libraries.LibPowerShell.EventArguments
{
	/// <summary>
	///		Argumento del evento de log
	/// </summary>
	public class PowerShellLogEventArgs : EventArgs
	{
		public PowerShellLogEventArgs(PowerShellLog log)
		{
			Log = log;
		}

		/// <summary>
		///		Elemento de log
		/// </summary>
		public PowerShellLog Log { get; }
	}
}
