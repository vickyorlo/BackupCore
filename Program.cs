using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CommandLine;

namespace BackupCore
{
    public class Program
    {
        public static bool Verbose;


        public static int Main(string[] args)
        {

            List<BackupAction> BackupActionList = new List<BackupAction>();
            ConfigBuilder configBuilder = new ConfigBuilder();
            bool archive = false;

            if (args.Count() == 0)
            {
                Console.Error.Write("No arguments given. See --help for usage examples.");
                return 1;
            }
            try
            {
                var result = Parser.Default.ParseArguments<Options>(args);
                result.WithParsed((options) => (BackupActionList, Verbose, archive) = configBuilder.LoadConfig(options));
                if (BackupActionList.Count > 0)
                {
                    foreach (var backupAction in BackupActionList)
                    {
                        backupAction.Start();
                    }

                    if (archive) ArchiveFiles(BackupActionList);
                    return 0;
                }
                else return 1;
            }
            catch (ArgumentException ex)
            {
                Console.Error.Write(ex.Message);
                return -1;
            }
        }

        static void ArchiveFiles(IList<BackupAction> actionList)
        {
            // Prepare the process to run
            ProcessStartInfo start = new ProcessStartInfo();
            // Enter in the command line arguments, everything you would enter after the executable name itself
            start.Arguments = "a -t7z " + "-p" + actionList[0].ArchivePassword + " " + actionList[0].ActionName.Replace(" ", "-") + ".7z  ";
            foreach (var action in actionList)
            {
                start.Arguments += action.DestinationPath + " ";
            }
            // Enter the executable to run, including the complete path
            start.FileName = ".\\7z.exe";
            // Do you want to show a console window?
            start.WindowStyle = ProcessWindowStyle.Normal;
            int exitCode;


            // Run the external process & wait for it to finish
            using (Process proc = Process.Start(start))
            {
                proc.WaitForExit();

                // Retrieve the app's exit code
                exitCode = proc.ExitCode;
            }
        }
    }
}