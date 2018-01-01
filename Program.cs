﻿using System;
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
        public static string DbName;
        public static string SourcePath;
        public static string DestinationPath;
        static List<BackupAction> BackupActionList = new List<BackupAction>();

        public static void Main(string[] args)
        {
            try
            {
                var result = Parser.Default.ParseArguments<Options>(args);
                result.WithParsed((options) => AddBackupAction(options));

                foreach (var backupAction in BackupActionList)
                {
                    BackupFiles(backupAction);
                    if (backupAction.Archive)
                    {
                        ArchiveFiles(backupAction);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.Write(ex.Message);
            }


        }

        private static void AddBackupAction(Options options)
        {
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
                foreach (var source in options.Inputs)
                {
                    BackupActionList.Add(new BackupAction("backup", options.Inputs[0], options.Outputs[0], BackupMode.FileCompareBackup, comparator, (int)options.History, options.Archive, options.Password));
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
            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile(options.Configuration);
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

        static void ArchiveFiles(BackupAction action)
        {
            // Prepare the process to run
            ProcessStartInfo start = new ProcessStartInfo();
            // Enter in the command line arguments, everything you would enter after the executable name itself
            start.Arguments = "a -t7z " + action.ActionName.Replace(" ", "-") + ".7z \"" + action.DestinationPath + "\"";
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