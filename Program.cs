using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

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
            DbName = "files";
            BackupActionList.Add(new BackupAction(@"D:\THINGS\Projects\BackupCore", @"D:\zfagg", BackupMode.DatabaseCompareBackup, copies: 2));

            foreach (var backupAction in BackupActionList)
            {
                BackupFiles(backupAction);
            }


            // Prepare the process to run
            ProcessStartInfo start = new ProcessStartInfo();
            // Enter in the command line arguments, everything you would enter after the executable name itself
            start.Arguments = "a -t7z files.7z \"" + BackupActionList[0].DestinationPath + "\"";
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
    }
}