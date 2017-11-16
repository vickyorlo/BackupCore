using System;
using System.Collections.Generic;
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
            BackupActionList.Add(new BackupAction(@"D:\THINGS\Projects", @"D:\Projects", BackupMode.FileCompareBackup));

            foreach (var backupAction in BackupActionList)
            {
                BackupFiles(backupAction);
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