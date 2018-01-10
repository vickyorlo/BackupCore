using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using IniParser;
using IniParser.Model;
using CommandLine;

namespace BackupCore
{
    public class Program
    {
        static List<BackupAction> BackupActionList = new List<BackupAction>();
        public static bool Verbose;

        public static int Main(string[] args)
        {
            if (args.Count() == 0)
            {
                Console.Error.Write("No arguments given. See --help for usage examples.");
                return 1;
            }
            try
            {
                var result = Parser.Default.ParseArguments<Options>(args);
                result.WithParsed((options) => AddBackupAction(options));
                if (BackupActionList.Count > 0)
                {
                    foreach (var backupAction in BackupActionList)
                    {
                        BackupFiles(backupAction);
                    }

                    if (BackupActionList[0].Archive) ArchiveFiles(BackupActionList);
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

        private static void AddBackupAction(Options options)
        {
            Program.Verbose = options.Verbose;
            if (options.Configuration != null)
            {
                ReadConfigurationFromFile(options);
            }
            else
            {
                ReadConfigurationFromCommandLine(options);
            }
        }

        private static void ReadConfigurationFromCommandLine(Options options)
        {
            CompareMethod comparator;
            switch (options.ComparisonMethod)
            {
                case "bydate": comparator = CompareMethod.WriteTimeComparator; break;
                case "byhash": comparator = CompareMethod.HashComparator; break;
                default: throw new ArgumentException("Invalid value in 'compare' setting!");
            }

            if (options.Outputs.Count() == 1)
            {
                for (int i = 0; i < options.Inputs.Count(); i++)
                {
                    BackupActionList.Add(new BackupAction("backup", options.Inputs[i], options.Outputs[0] + "/" + Path.GetFileName(options.Inputs[i]) + "/", BackupMode.FileCompareBackup, comparator, (int)options.History, options.Archive, options.Password));
                }
            }
            else if (options.Inputs.Count() == options.Outputs.Count())
            {
                for (int i = 0; i < options.Inputs.Count(); i++)
                {
                    BackupActionList.Add(new BackupAction("backup", options.Inputs[i], options.Outputs[i], BackupMode.FileCompareBackup, comparator, (int)options.History, options.Archive, options.Password));
                }
            }
            else
            {
                throw new ArgumentException("The number of destinations must be equal to 1 or the number of sources!");
            }
        }

        private static void ReadConfigurationFromFile(Options options)
        {
            IniData data;
            try
            {
                var parser = new FileIniDataParser();
                data = parser.ReadFile(options.Configuration);
            }
            catch (Exception e)
            {
                Console.Error.Write(e);
                throw new ArgumentException("Configuration file does not exist or invalid format! /n Check the example ");
            }
            var flags = data["Flags"];
            var files = data["Files"];
            string actionName = flags["profile"];
            BackupMode mode;
            switch (flags["mode"])
            {
                case "database": mode = BackupMode.DatabaseCompareBackup; break;
                case "simple": mode = BackupMode.FileCompareBackup; break;
                default: throw new ArgumentException("Invalid value in 'mode' setting!");
            }
            CompareMethod comparator;
            switch (flags["compare"])
            {
                case "bydate": comparator = CompareMethod.WriteTimeComparator; break;
                case "byhash": comparator = CompareMethod.HashComparator; break;
                default: throw new ArgumentException("Invalid value in 'compare' setting!");
            }

            int copies = Convert.ToInt32(flags["history"]);
            bool archive = (flags["archive"] == "yes" || flags["archive"] == "true") ? true : false;
            string password = flags["password"];

            string[] sources = files["sources"].Replace("\"", "").Split(",");
            string[] destination = files["destinations"].Replace("\"", "").Split(",");

            if (destination.Length == 1)
            {
                foreach (var source in sources)
                {
                    BackupActionList.Add(new BackupAction(actionName, source, destination[0], mode, comparator, copies, archive, password));
                }
            }
            else if (sources.Length == destination.Length)
            {
                for (int i = 0; i < sources.Length; i++)
                {
                    BackupActionList.Add(new BackupAction(actionName, sources[i], destination[i], mode, comparator, copies, archive, password));
                }
            }
            else
            {
                throw new ArgumentException("The number of destinations must be equal to 1 or the number of sources!");
            }
        }

        /// <summary>
        /// Processes a given backup job, copying files and processing metadata to speed up the process.
        /// </summary>
        /// <param name="action">The BackupAction to process</param>
        static void BackupFiles(BackupAction action)
        {
            switch (action.Mode)
            {
                case BackupMode.DatabaseCompareBackup:
                    DatabaseBackup.Start(action);
                    break;

                case BackupMode.FileCompareBackup:
                    FileBackup.Start(action);
                    break;

                default:
                    break;
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