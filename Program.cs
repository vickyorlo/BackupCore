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
                    DatabaseCompareBackup(action);
                    break;

                case BackupMode.FileCompareBackup:
                    FileCompareBackup(action);
                    break;

                default:
                    break;
            }

        }

        static void FileCompareBackup(BackupAction action)
        {
            Console.WriteLine("Proceeding with a simple file cross-comparison backup job");
            Console.WriteLine("Difference mechanism: Write-time based");
            Console.WriteLine("Backing up " + action.SourcePath + " to " + action.DestinationPath);
            foreach (var file in action.FilesToCopy)
            {
                string targetPath = action.DestinationPath + (file.Replace(action.SourcePath, ""));
                if (File.Exists(targetPath))
                {
                    if (File.GetLastWriteTime(file) > File.GetLastWriteTime(targetPath)) // file changed
                    {
                        File.Copy(file, targetPath, true);
                        Console.WriteLine("Replaced file " + Path.GetFileName(targetPath));
                    }
                    else
                    {
                        Console.WriteLine("File doesn't need replacing " + Path.GetFileName(targetPath));
                    }
                }
                else
                {
                    if (!Directory.Exists(targetPath.Replace(Path.GetFileName(file), "")))
                    {
                        Directory.CreateDirectory(targetPath.Replace(Path.GetFileName(file), "")); //ensure the directory to backup to exists
                    }
                    File.Copy(file, targetPath, false);
                    Console.WriteLine("Added new file " + Path.GetFileName(targetPath));
                }
            }
        }

        static void DatabaseCompareBackup(BackupAction action)
        {
            Console.WriteLine("Proceeding with a database based comparison backup job");
            Console.WriteLine("Difference mechanism: Write-time based");
            Console.WriteLine("Backing up " + action.SourcePath + " to " + action.DestinationPath);

            using (var db = new FileContext())
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                ProcessedFile cataloguedFile;
                foreach (var file in action.FilesToCopy)
                {
                    string targetPath = action.DestinationPath + (file.Replace(action.SourcePath, ""));
                    if ((cataloguedFile = db.Files.Find(file, targetPath)) == null)
                    {
                        CatalogueAndCopyNewFile(db, targetPath, file);
                    }
                    else
                    {
                        ReplaceCataloguedFile(db, cataloguedFile);
                    };
                }
                var count = db.SaveChanges();
                Console.WriteLine("{0} records saved to database", count);

                Console.WriteLine();
                Console.WriteLine("All files in database:");
                foreach (var file in db.Files)
                {
                    Console.WriteLine(" - {0}", file.FileName);
                }
            }
        }

        /// <summary>
        /// Given a file already catalogued in the database, updates the backed up file when it is required.
        /// </summary>
        /// <param name="db">The database containing catalogues files</param>
        /// <param name="currentFile">The catalogued file to update</param>
        private static void ReplaceCataloguedFile(FileContext db, ProcessedFile currentFile)
        {
            if (File.GetLastWriteTime(currentFile.FilePath) > currentFile.DateModified) //then our file got updated
            {
                if (File.Exists(currentFile.BackupPath))
                {
                    File.Copy(currentFile.FilePath, currentFile.BackupPath, true);
                    currentFile.DateModified = File.GetLastWriteTime(currentFile.FilePath);
                    Console.WriteLine("Replaced file " + currentFile.FileName);
                }
                else
                {
                    throw new Exception("what");
                }
            }
            else
            {
                Console.WriteLine("File doesn't need replacing " + currentFile.FileName);
            }
        }

        /// <summary>
        /// Given a path to a file, adds it to the file database and backs it up to the target path.
        /// </summary>
        /// <param name="db">The database containing catalogues files</param>
        /// <param name="targetPath">The path to copy the file to</param>
        /// <param name="file">The file to back up</param>
        private static void CatalogueAndCopyNewFile(FileContext db, string targetPath, string file)
        {

            if (!Directory.Exists(targetPath.Replace(Path.GetFileName(file), "")))
            {
                Directory.CreateDirectory(targetPath.Replace(Path.GetFileName(file), "")); //ensure the directory to backup to exists
            }

            File.Copy(file, targetPath, false);

            db.Files.Add(new ProcessedFile { FileName = Path.GetFileName(file), FilePath = file, BackupPath = targetPath, DateModified = File.GetLastWriteTime(file) });

            Console.WriteLine("Added new file " + Path.GetFileName(file));
        }
    }
}