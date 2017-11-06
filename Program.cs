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
            BackupActionList.Add(new BackupAction(@"D:\THINGS\Projects", @"D:\Projects"));

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
                Directory.CreateDirectory(targetPath.Replace(Path.GetFileName(file), ""));
            }

            File.Copy(file, targetPath, false);

            db.Files.Add(new ProcessedFile { FileName = Path.GetFileName(file), FilePath = file, BackupPath = targetPath, DateModified = File.GetLastWriteTime(file) });

            Console.WriteLine("Added new file " + Path.GetFileName(file));
        }
    }
}