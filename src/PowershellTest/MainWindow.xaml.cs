using System;
using System.Windows;

using Bau.Libraries.LibPowerShellManager;

namespace PowerShellTest
{
	/// <summary>
	///		Ventana principal de la aplicación
	/// </summary>
	public partial class MainWindow : Window
	{
		// Variables privadas
		PowerShellManager _manager = new PowerShellManager();
		private bool _processing;

		public MainWindow()
		{
			InitializeComponent();
		}

		/// <summary>
		///		Ejecuta el script de powerShell
		/// </summary>
		private void ExecuteScript()
		{
			if (_processing)
				MessageBox.Show("Ya se está ejecutando un script");
			else if (string.IsNullOrEmpty(txtEditor.Text))
				MessageBox.Show("Introduzca el texto del script");
			else
			{	
				// Indica que está en ejecución
				_processing = true;
				// Carga el script
				_manager.LoadScript(txtEditor.Text);
				// y lo ejecuta
				_manager.Execute(EndExecute);
			}
		}

		/// <summary>
		///		Trata el final de la ejecución del script
		/// </summary>
		private void EndExecute()
		{
			// Indica que ha finalizado la ejecución
			_processing = false;
			// Log y mensaje al usuario
			Dispatcher.Invoke(new Action(() => LogResults()), null);
			MessageBox.Show("Final de la ejecución");
		}

		/// <summary>
		///		Log de los resultados
		/// </summary>
		private void LogResults()
		{
			// Cabecera
			Log($"{DateTime.Now:HH:mm:ss}");
			// Objetos de salida
			if (_manager.OutputItems.Count > 0)
				foreach (object item in _manager.OutputItems)
				{
					Log($"Objeto {_manager.OutputItems.IndexOf(item)}");
					if (item != null)
						Log(item.ToString());
				}
			else
				Log("Sin objetos de salida");
			// Errores
			if (_manager.Errors.Count > 0)
			{
				Log("Errores");
				foreach (string error in _manager.Errors)
					Log($"\t{error}");
			}
			else
				Log("Sin errores");
			// Final
			Log(new string('-', 80));
			Log(Environment.NewLine);
		}

		/// <summary>
		///		Añade una cadena al log
		/// </summary>
		private void Log(string log)
		{
			txtLog.AppendText(log);
			txtLog.AppendText(Environment.NewLine);
		}

		/// <summary>
		///		Abre un archivo
		/// </summary>
		private void OpenFile()
		{
			string fileName = OpenDialogLoad(null, "Archivos PowerShell (*.ps)|*.ps|Todos los archivos (*.*)|*.*");

				if (!string.IsNullOrEmpty(fileName) && System.IO.File.Exists(fileName))
					txtEditor.Text = LoadTextFile(fileName);
		}

		/// <summary>
		///		Graba un archivo de texto
		/// </summary>
		private void SaveFile()
		{
			if (!string.IsNullOrWhiteSpace(txtEditor.Text))
			{ 
				string fileName = OpenDialogSave(null, "Archivos PowerShell (*.ps)|*.ps|Todos los archivos (*.*)|*.*");

				if (!string.IsNullOrEmpty(fileName))
				{
					// Graba el archivo
					SaveTextFile(fileName, txtEditor.Text);
					// Mensaje al usuario
					MessageBox.Show("Archivo grabado");
				}
			}
		}

		/// <summary>
		///		Abre el cuadro de diálogo de carga de archivos
		/// </summary>
		private string OpenDialogLoad(string defaultPath, string filter, string defaultFileName = null, string defaultExtension = null)
		{
			Microsoft.Win32.OpenFileDialog file = new Microsoft.Win32.OpenFileDialog();

				// Asigna las propiedades
				file.InitialDirectory = defaultPath;
				file.FileName = defaultFileName;
				file.DefaultExt = defaultExtension;
				file.Filter = filter;
				// Muestra el cuadro de diálogo
				if (file.ShowDialog() ?? false)
					return file.FileName;
				else
					return null;
		}

		/// <summary>
		///		Abre el cuadro de diálogo de grabación de archivos
		/// </summary>
		private string OpenDialogSave(string defaultPath, string filter, string defaultFileName = null, string defaultExtension = null)
		{
			Microsoft.Win32.SaveFileDialog file = new Microsoft.Win32.SaveFileDialog();

				// Asigna las propiedades
				file.InitialDirectory = defaultPath;
				file.FileName = defaultFileName;
				file.DefaultExt = defaultExtension;
				file.Filter = filter;
				// Muestra el cuadro de diálogo
				if (file.ShowDialog() ?? false)
					return file.FileName;
				else
					return null;
		}

		/// <summary>
		///		Carga un archivo de texto
		/// </summary>
		private string LoadTextFile(string fileName)
		{
			System.Text.StringBuilder content = new System.Text.StringBuilder();

				// Carga el archivo
				using (System.IO.StreamReader file = new System.IO.StreamReader(fileName, System.Text.Encoding.UTF8))
				{ 
					string data;

						// Lee los datos
						while ((data = file.ReadLine()) != null)
						{ 
							// Le añade un salto de línea si es necesario
							if (content.Length > 0)
								content.Append("\n");
							// Añade la línea leída
							content.Append(data);
						}
						// Cierra el stream
						file.Close();
				}
				// Devuelve el contenido
				return content.ToString();
		}

		/// <summary>
		/// 	Graba una cadena en un archivo de texto
		/// </summary>
		public static void SaveTextFile(string fileName, string text)
		{	
			using (System.IO.StreamWriter file = new System.IO.StreamWriter(fileName, false, System.Text.Encoding.UTF8))
			{ 
				// Escribe la cadena
				file.Write(text);
				// Cierra el stream
				file.Close();
			}
		}		

		private void cmdProcess_Click(object sender, RoutedEventArgs e)
		{
			ExecuteScript();
		}

		private void cmdOpen_Click(object sender, RoutedEventArgs e)
		{
			OpenFile();
		}

		private void cmdSave_Click(object sender, RoutedEventArgs e)
		{
			SaveFile();
		}
	}
}
