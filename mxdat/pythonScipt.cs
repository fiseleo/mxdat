using System;
using System.Diagnostics;
using mxdat;
namespace mxdat
{
    public class pythonScipt
    {
        public static void pythonSciptMain(string[] args)
        {
            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string sourceFilePath = Path.Combine(rootDirectory, "Excel.zip");
            string targetDirectoryPath = Path.Combine(rootDirectory, "mxdatpy", "extracted");
            string targetFilePath = Path.Combine(targetDirectoryPath, "Excel.zip");

            
            if (!Directory.Exists(targetDirectoryPath))
            {
                Directory.CreateDirectory(targetDirectoryPath);
            }
            else
            {
                Console.WriteLine("Directory already exists");
            }
            

            if (File.Exists(sourceFilePath))
            {
                File.Copy(sourceFilePath, targetFilePath, true);
                Console.WriteLine("Excel.zip copied successfully");
            }
            else
            {
                Console.WriteLine("File does not exist");
                return;
            }

            // Use the system's Python interpreter
            string pythonInterpreter = "python";
            string pythonScriptRelativePath = Path.Combine(rootDirectory,"mxdatpy", "extract_tables.py");
            string fullPathToPythonScript = Path.GetFullPath(Path.Combine(rootDirectory, pythonScriptRelativePath));
            Console.WriteLine($"Python Interpreter Path: {pythonInterpreter}");
            Console.WriteLine($"Python Script Path: {fullPathToPythonScript}");

            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = pythonInterpreter;
            start.Arguments = fullPathToPythonScript;
            start.WorkingDirectory = rootDirectory;
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;

            try
            {
                using (Process process = Process.Start(start))
                {
                    using (StreamReader outputReader = process.StandardOutput)
                    using (StreamReader errorReader = process.StandardError)
                    {
                        string result = outputReader.ReadToEnd();
                        string error = errorReader.ReadToEnd();
                        if (!string.IsNullOrEmpty(error))
                        {
                            Console.WriteLine($"Error: {error}");
                        }
                        else
                        {
                            Console.WriteLine(result);
                        }
                    }
                }

                Updatalist.UpdatalistMain(args);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
