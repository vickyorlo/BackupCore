using System;
using System.IO;

namespace BackupCore
{
    static class DatabaseBackup
    {
        /// <summary>
        /// Starts a backup action, using an internal database to speed up file comparation.
        /// Good for cases of relatively slow IO speeds.
        /// </summary>
        /// <param name="action">The action containing the source, destination and files to backup</param>
        public static void Start(BackupAction action)
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
                        CatalogueAndCopyNewFile(db, targetPath, file, action.Comparator);
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
            if () //then our file got updated
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

        private static bool IsFileEqualToDB(ProcessedFile currentFile, CompareMethod method)
        {
            switch (method)
            {
                case CompareMethod.WriteTimeComparator:
                    {
                        return (File.GetLastWriteTime(currentFile.FilePath) > currentFile.DateModified);
                    }
                case CompareMethod.HashComparator:
                    {
                        return (HashTools.CompareHashes(HashTools.HashFile(currentFile.FilePath), currentFile.FileHash));
                    }
            }
            return true;
        }

        /// <summary>
        /// Given a path to a file, adds it to the file database and backs it up to the target path.
        /// </summary>
        /// <param name="db">The database containing catalogues files</param>
        /// <param name="targetPath">The path to copy the file to</param>
        /// <param name="file">The file to back up</param>
        private static void CatalogueAndCopyNewFile(FileContext db, string targetPath, string file, CompareMethod comparator)
        {

            if (!Directory.Exists(targetPath.Replace(Path.GetFileName(file), "")))
            {
                Directory.CreateDirectory(targetPath.Replace(Path.GetFileName(file), "")); //ensure the directory to backup to exists
            }

            File.Copy(file, targetPath, false);

            switch (comparator)
            {
                case CompareMethod.WriteTimeComparator:
                    {
                        db.Files.Add(new ProcessedFile { FileName = Path.GetFileName(file), FilePath = file, BackupPath = targetPath, DateModified = File.GetLastWriteTime(file) });
                        break;
                    }
                case CompareMethod.HashComparator:
                    {
                        db.Files.Add(new ProcessedFile { FileName = Path.GetFileName(file), FilePath = file, BackupPath = targetPath, FileHash = HashTools.HashFile(file) });
                        break;
                    }

            }

            Console.WriteLine("Added new file " + Path.GetFileName(file));
        }

    }
}