using System;
using System.IO;
using System.Collections.Generic;

namespace BackupCore
{
    class FileBackup : IBackup
    {
        private BackupAction action;
        /// <summary>
        /// Starts a backup action, using a file-to-file comparison to look for changes.
        /// Safer than database backups, allows for modification of jobs at the user's risk. However, can be bottlenecked by IO speeds.
        /// </summary>
        /// <param name="action">The action containing the source, destination and files to backup</param>
        public void Start(BackupAction backupAction)
        {
            action = backupAction;
            Console.WriteLine("Proceeding with a simple file cross-comparison backup job");
            Console.WriteLine("Difference mechanism: Write-time based");
            Console.WriteLine("Backing up " + action.SourcePath + " to " + action.DestinationPath);

            List<string> deletedFiles = RecursiveFileFinder.ProcessPath(action.DestinationPath, false);
            List<string> relativeFiles = new List<string>();
            for (int i = 0; i < action.FilesToCopy.Count; i++)
            {
                relativeFiles.Add(action.FilesToCopy[i].Replace(action.SourcePath + "\\", ""));
            }
            deletedFiles.RemoveAll((s) => relativeFiles.Contains(s.Replace(action.DestinationPath, "")));
            deletedFiles.RemoveAll((s) => s.Contains(".copyMinus"));


            foreach (var file in action.FilesToCopy)
            {
                string targetPath = action.DestinationPath + (file.Replace(action.SourcePath, ""));
                if (File.Exists(targetPath))
                {

                    if (action.Comparator.WasFileUpdated(file, targetPath)) // file changed
                    {
                        PushNewCopy(file, targetPath);
                        Console.WriteLine("Replaced file " + Path.GetFileName(targetPath));
                    }
                    else
                    {
                        if (Program.Verbose) Console.WriteLine("File doesn't need replacing " + Path.GetFileName(targetPath));
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
            foreach (var file in deletedFiles)
            {
                PushNewCopy(null, file);
            }
            Console.WriteLine("Done!");
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
    }
}