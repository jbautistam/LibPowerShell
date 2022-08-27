using System;
using System.Windows;
using System.Threading;
using System.Threading.Tasks;

using Bau.Libraries.LibPowerShell;

namespace PowerShellTest
{
	/// <summary>
	///		Ventana principal de la aplicación
	/// </summary>
	public partial class MainWindow : Window
	{
		// Variables privadas
		private bool _processing;

		public MainWindow()
		{
			InitializeComponent();
		}

		/// <summary>
		///		Ejecuta el script de powerShell
		/// </summary>
		private async Task ExecuteScriptAsync()
		{
			if (_processing)
				MessageBox.Show("There is another script executing");
			else if (string.IsNullOrEmpty(txtEditor.Text))
				MessageBox.Show("Enter the script text");
			else
			{	
				PowerShellManager manager = new PowerShellManager(new PowerShellContext
																			{
																				Script = txtEditor.Text,
																				InputParameters = null,
																				UseRunSpace = chkHost.IsChecked ?? false
																			}
																  );

					// Indica que está en ejecución
					_processing = true;
					// Lo ejecuta y muestra el resultado
					LogResults(await manager.ProcessAsync(CancellationToken.None));
					// e indica que ya ha terminado
					_processing = false;
			}
		}

		/// <summary>
		///		Log de los resultados
		/// </summary>
		private void LogResults(PowerShellResult psResult)
		{
			// Cabecera
			Log($"{DateTime.Now:HH:mm:ss}");
			// Objetos de salida
			if (psResult.OutputObjects.Count > 0)
				foreach (object item in psResult.OutputObjects)
				{
					Log($"Output object: {psResult.OutputObjects.IndexOf(item)}");
					if (item != null)
						Log(item.ToString());
				}
			else
				Log("Without ouptut objects");
			// Log
			if (psResult.Log.Count > 0)
			{
				Log("Log");
				foreach (PowerShellLog log in psResult.Log)
					Log($"\t[{log.Type.ToString()}] {log.Message}");
			}
			else
				Log("No errors");
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
			string fileName = OpenDialogLoad(null, "PowerShell files (*.ps)|*.ps|All files (*.*)|*.*");

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
				string fileName = OpenDialogSave(null, "PowerShell files (*.ps)|*.ps|All files (*.*)|*.*");

					if (!string.IsNullOrEmpty(fileName))
					{
						// Graba el archivo
						SaveTextFile(fileName, txtEditor.Text);
						// Mensaje al usuario
						MessageBox.Show("File saved");
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

		private async void cmdProcess_Click(object sender, RoutedEventArgs e)
		{
			await ExecuteScriptAsync();
		}

		private void cmdOpen_Click(object sender, RoutedEventArgs e)
		{
			OpenFile();
		}

		private void cmdSave_Click(object sender, RoutedEventArgs e)
		{
			SaveFile();
		}

		private void cmdCleanLog_Click(object sender, RoutedEventArgs e)
		{
			txtLog.Text = string.Empty;
		}
	}
}