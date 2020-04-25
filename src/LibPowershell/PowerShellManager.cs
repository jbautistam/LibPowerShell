using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bau.Libraries.LibPowerShellManager
{
	/// <summary>
	///		Manager de PowerShell
	/// </summary>
    public class PowerShellManager
    {
		// Eventos públicos
		public event EventHandler EndExecute;

		/// <summary>
		///		Carga un script en texto
		/// </summary>
		public void LoadScript(string script)
		{
			// Inicializa el script
			Script = script;
			// Limpia los datos del manager
			InputParameters.Clear();
			OutputItems.Clear();
			Errors.Clear();
		}

		/// <summary>
		///		Añade un parámetro de entrada al script
		/// </summary>
		public void AddParameter(string key, object value)
		{
			InputParameters.Add(key, value);
		}

		/// <summary>
		///		Ejecuta el script
		/// </summary>
		public void Execute(Action endCallback = null)
		{
			Task task;
			PowerShellInstance processor = new PowerShellInstance(Script, InputParameters);

				// Limpia los datos de salida
				OutputItems.Clear();
				Errors.Clear();
				// Asigna el manejador de eventos
				processor.EndExecute += (sender, args) => TreatEndScript(processor, endCallback);
				// Crea la tarea para la ejecución en otro hilo
				task = new Task(() => processor.Process());
				// Arranca la tarea 
				try
				{
					task.Start();
				}
				catch (Exception exception)
				{
					Errors.Add($"Error when execute script {exception.Message}");
					endCallback?.Invoke();
					EndExecute?.Invoke(this, EventArgs.Empty);
				}
		}

		/// <summary>
		///		Trata el fin de la ejecución del proceso
		/// </summary>
		private void TreatEndScript(PowerShellInstance processor, Action endCallBack)
		{
			// Recoge los datos de salida
			foreach (object item in processor.OutputObjects)
				OutputItems.Add(item);
			// Recoge los errores
			foreach (string error in processor.Errors)
				Errors.Add(error);
			// Llama a la acción indicada por el creador y lanza el evento de fin
			endCallBack?.Invoke();
			EndExecute?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		///		Script
		/// </summary>
		public string Script { get; set; }

		/// <summary>
		///		Parametros de entrada
		/// </summary>
		internal Dictionary<string, object> InputParameters { get; } = new Dictionary<string, object>();

		/// <summary>
		///		Datos de salida
		/// </summary>
		public List<object> OutputItems { get; } = new List<object>();

		/// <summary>
		///		Errores
		/// </summary>
		public List<string> Errors { get; } = new List<string>();
    }
}
