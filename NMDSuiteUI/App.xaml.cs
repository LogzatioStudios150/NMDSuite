using NMDSuite;
using static NMDBase.NMDUtil;
using System.IO;
using System;
using System.Windows;
using System.Xml.Linq;
using NMDBase;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using DiscordRPC.Helper;
using System.Text;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Windows.Input;

namespace NMDSuiteUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private bool openOnExit;
        
        public static string? nmd_path;

        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();
        public App()
        {
            InitializeComponent();
            // Get the command line arguments
            string[] args = Environment.GetCommandLineArgs();

            // Check if there are any arguments
            if (args.Length > 1)
            {
                if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    Splash.keepeye = true;
                }
                Splash.fileToParse = args[1];
            }
            if (args.Length > 2)
            {
      
                AllocConsole();
                Console.Title = "NMDSuite: Batch process NMD files";
                string name = null;
                string fileToParse = null;
                string[] fileList = null;
                string? exportDirectory = ".\\converted";
                bool keepEye = false;
                bool log = false;

                // Parse the arguments for supported flags
                for (int i = 1; i < args.Length; i++)
                {

                    if (args[i] == "--name" && i + 1 < args.Length)
                    {
                        name = args[i + 1];
                    }

                    else if (args[i] == "/parsefiles" && i + 1 < args.Length)
                    {
                        string path = args[i + 1];
                        string[] path_list = path.Split(',');
                        if (Directory.Exists(path_list[0]))
                        {
                            // The input argument is a directory
                            fileList = Directory.GetFiles(path);
                            foreach(string file in fileList)
                            {
                               
                            }
                            Console.WriteLine($"* Batch converting NMD files from directory: {path} *\n");
                            Thread.Sleep(2000);

                        }
                        else if (File.Exists(path_list[0]))
                        {
                            // The input argument is a file
                            fileList = path_list;
                            Console.WriteLine("* Batch converting NMD files from specified list *\n");
                            Thread.Sleep(2000);
                        }
                    }
                    else if (args[i] == "--exportdir" && i + 1 < args.Length)
                    {
                        
                        if (args[i + 1] != "")
                        {
                            
                            exportDirectory = args[i + 1];

                        }
                       
                        
                        Thread.Sleep(2000);

                    }
                    else if (args[i] == "--keepeye")
                    {
                        
                        keepEye = true;
                        

                    }
                    else if (args[i] == "--log")
                    {
                        log = true;
                       

                    }
                    else if (args[i] == "--openexportdir")
                    {
                        
                        openOnExit = true;
                        

                    }
                }
                // Batch process NMD files
                if (!Directory.Exists(exportDirectory))
                {
                    Directory.CreateDirectory(exportDirectory);
                }
                string log_path = Path.Combine(exportDirectory, $"NMD_info_{DateTime.Now.Month}.{DateTime.Now.Day}.{DateTime.Now.Year}_{DateTime.Now.Hour}.{DateTime.Now.Minute}.{DateTime.Now.Second}.log");
                StreamWriter streamWriter = null;

                if (log)
                {
                    if (File.Exists(log_path))
                    {
                        File.Delete(log_path);
                    }
                    streamWriter = new StreamWriter(log_path, true);
                   
                }

                if (fileList != null)
                {
                    Console.WriteLine($"* Exporting converted files to: {exportDirectory} *\n");
                    Thread.Sleep(1500);
                    if (keepEye)
                    {
                        Console.WriteLine("* Keeping eye bones. *\n");
                        Thread.Sleep(2000);
                    }
                    if (openOnExit)
                    {
                        Console.WriteLine("* Opening export directory when finished. *\n");
                        Thread.Sleep(2000);
                    }
                    if (log)
                    {
                        Console.WriteLine("* Logging NMD info to file. *\n");
                        Thread.Sleep(2000);
                    }
                    Console.WriteLine($"* Converting {fileList.Length} file(s)... *\n");
                    
                    foreach (string file in fileList)
                    {
                        try
                        {
                            string exportStatus = null;
                            if (File.Exists(file)) 
                            {
                                if (Path.GetExtension(file) != ".nmd")
                                {
                                    Console.WriteLine($"* Skipping {Path.GetFileName(file)} because it isn't a NMD file. *\n");
                                    continue;
                                }
                                if (log)
                                {
                                    Console.SetOut(streamWriter);
                                }
                                Console.WriteLine($"{Path.GetFileName(file)}:");
                                Thread.Sleep(1000);
                               
                                
                                var bones = ParseNMD(file, keepEye,new(),new());
                                if (bones.Bones != null)
                                {
                                    exportStatus = ExportNMD(bones.Bones, Path.Combine(exportDirectory, Path.GetFileName(file)));
                                }

                                Console.WriteLine();
                                Console.WriteLine(exportStatus);
                                Console.WriteLine();
                                Thread.Sleep(2000);
                            }

                            
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Exception: {ex.Message}");
                        }
                    }
                    
                    Console.SetOut(Console.Out);
                    if (keepEye)

                    {
                        Console.Out.WriteLine("\nBatch conversion operation completed successfully!\nEye bones were retained.");
                    }
                    else
                    {
                        Console.Out.WriteLine("\nBatch conversion operation completed successfully!");
                    }
                    if (log)
                    {
                        Console.SetOut(Console.Out);
                        
                        streamWriter.Dispose();
                    }

                    if (openOnExit)
                    {
                        Process.Start("explorer.exe", Path.GetFullPath(exportDirectory));
                    }
                    
                    Thread.Sleep(2000);

                    Current.Dispatcher.Invoke(() =>
                    {
                        Current.Shutdown();
                    });
                }
            }


            // Pass the parsed arguments to main window

            //mainWindow.Show();

        
            else
            {
                // If there are no arguments, create and show the main window
                
            }

        }
    }

}
