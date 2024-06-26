using System;
using System.Diagnostics;
using System.IO;

namespace mxdat
{
    public class pythonScipt
    {
        public static void pythonSciptMain(string[] args)
        {
            string currentDirectory = Environment.CurrentDirectory;
            string sourceFilePath = Path.Combine(currentDirectory, "Excel.zip");
            string targetDirectoryPath = Path.Combine(currentDirectory, @"mxdatpy\extracted");
            string targetFilePath = Path.Combine(targetDirectoryPath, "Excel.zip");



            Directory.CreateDirectory(targetDirectoryPath);
            if (File.Exists(sourceFilePath))
            {
                File.Copy(sourceFilePath, targetFilePath, true);
            }
            else
            {
                Console.WriteLine("File does not exist");
            }

            string pythonInterpreterRelativePath = @"mxdatpy\python.exe"; 
            string pythonScriptRelativePath = @"mxdatpy\extract_tables.py";
            string fullPathToPythonInterpreter = Path.GetFullPath(Path.Combine(currentDirectory, pythonInterpreterRelativePath));
            string fullPathToPythonScript = Path.GetFullPath(Path.Combine(currentDirectory, pythonScriptRelativePath));
            Console.WriteLine($"Python Interpreter Path: {fullPathToPythonInterpreter}");
            Console.WriteLine($"Python Script Path: {fullPathToPythonScript}");

            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = fullPathToPythonInterpreter;
            start.Arguments = fullPathToPythonScript;
            start.WorkingDirectory = Path.GetDirectoryName(fullPathToPythonInterpreter);
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;

            try
            {
                using (Process process = Process.Start(start))
                {
                    using (StreamReader reader = process.StandardOutput)
                    {
                        string result = reader.ReadToEnd();
                        Console.WriteLine(result);
                    }
                }
                
                Getlist.GetlistMain(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

        }

    }
}