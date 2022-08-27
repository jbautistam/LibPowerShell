using System;
using System.Collections.Generic;

namespace Bau.Libraries.LibPowerShell
{
	/// <summary>
	///		Contexto de ejecución para el script de PowerShell
	/// </summary>
	public class PowerShellContext
	{
		/// <summary>
		///		Script a ejecutar
		/// </summary>
		public string Script { get; set; }

		/// <summary>
		///		Parámetros de entrada
		/// </summary>
		public Dictionary<string, object> InputParameters { get; set; }

		/// <summary>
		///		Módulos necesarios para ejecutar el script
		/// </summary>
		public List<string> Modules { get; } = new();

		/// <summary>
		///		Indica si se utiliza un pool de espacios de ejecución
		/// </summary>
		public bool UseRunSpace { get; set; }

		/// <summary>
		///		Número mínimo de espacios de ejecución
		/// </summary>
		public int MinRunSpaces { get; set; } = 1;

		/// <summary>
		///		Número máximo de espacios de ejecución
		/// </summary>
		public int MaxRunSpace { get; set; } = 1;
	}
}
