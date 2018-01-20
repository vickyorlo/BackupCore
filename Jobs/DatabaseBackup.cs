using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace BackupCore
{
    class DatabaseBackup : IBackup
    {
        private BackupAction action;
        private List<ProcessedFile> deletedFiles;
        /// <summary>
        /// Starts a backup action, using an internal database to speed up file comparation.
        /// Good for cases of relatively slow IO speeds.
        /// </summary>
        /// <param name="action">The action containing the source, destination and files to backup</param>
        public void Start(BackupAction backupAction)
        {
            action = backupAction;
            Console.WriteLine("Proceeding with a database based comparison backup job");
            Console.WriteLine("Difference mechanism: Write-time based");
            Console.WriteLine("Backing up " + action.SourcePath + " to " + action.DestinationPath);

            using (var db = new FileContext(action.ActionName))
            {
                db.Database.EnsureCreated();
                deletedFiles = new List<ProcessedFile>(db.Files.Where((f) => f.FilePath.Contains(action.SourcePath)));
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
                        deletedFiles.Remove(cataloguedFile);
                        ReplaceCataloguedFile(db, cataloguedFile);
                    };
                }
                foreach (var file in deletedFiles)
                {
                    PushNewCopy(null, file.BackupPath);
                    db.Files.Remove(file);
                }


                var count = db.SaveChanges();

                if (Program.Verbose)
                {
                    Console.WriteLine("{0} records saved to database", count);

                    Console.WriteLine();
                    Console.WriteLine("All files in database:");
                    foreach (var file in db.Files)
                    {
                        Console.WriteLine(" - {0}", file.FileName);
                    }
                }
            }
        }

        /// <summary>
        /// Given a file already catalogued in the database, updates the backed up file when it is required.
        /// </summary>
        /// <param name="db">The database containing catalogues files</param>
        /// <param name="currentFile">The catalogued file to update</param>
        private void ReplaceCataloguedFile(FileContext db, ProcessedFile currentFile)
        {
            if (action.Comparator.WasFileUpdated(currentFile))
            {
                if (File.Exists(currentFile.BackupPath))
                {
                    PushNewCopy(currentFile.FilePath, currentFile.BackupPath);
                    action.Comparator.UpdateEntry(db, currentFile);
                    Console.WriteLine("Replaced file " + currentFile.FileName);
                }
                else
                {
                    throw new Exception("Warning: Found a file where there should be none!");
                }
            }
            else
            {
                if (Program.Verbose) Console.WriteLine("File doesn't need replacing " + currentFile.FileName);
            }
        }

        private void PushNewCopy(string from, string to, int copies = 0)
        {
            if (++copies < action.BackupCopies)
            {
                string copyPath = action.DestinationPath + "/.copyMinus" + copies + "//" + to.Replace(action.DestinationPath, "");

                if (!Directory.Exists(Path.GetDirectoryName(copyPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(copyPath)); //ensure the directory to backup to exists
                }

                PushNewCopy(to, copyPath, copies);
            }
            if (from == null)
            {
                Console.WriteLine("Deleted file " + to);
                File.Delete(to);
            }
            else File.Copy(from, to, true);
        }

        /// <summary>
        /// Given a path to a file, adds it to the file database and backs it up to the target path.
        /// </summary>
        /// <param name="db">The database containing catalogues files</param>
        /// <param name="targetPath">The path to copy the file to</param>
        /// <param name="file">The file to back up</param>
        private void CatalogueAndCopyNewFile(FileContext db, string targetPath, string file)
        {

            if (!Directory.Exists(targetPath.Replace(Path.GetFileName(file), "")))
            {
                Directory.CreateDirectory(targetPath.Replace(Path.GetFileName(file), "")); //ensure the directory to backup to exists
            }

            File.Copy(file, targetPath, false);
            db.Files.Add(new ProcessedFile { FileName = Path.GetFileName(file), FilePath = file, BackupPath = targetPath, DateModified = File.GetLastWriteTime(file), FileHash = HashTools.HashFile(file) });

            Console.WriteLine("Added new file " + Path.GetFileName(file));
        }

    }
}