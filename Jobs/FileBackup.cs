using System;
using System.IO;

namespace BackupCore
{
    static class FileBackup
    {
        /// <summary>
        /// Starts a backup action, using a file-to-file comparison to look for changes.
        /// Safer than database backups, allows for modification of jobs at the user's risk. However, can be bottlenecked by IO speeds.
        /// </summary>
        /// <param name="action">The action containing the source, destination and files to backup</param>
        static void Start(BackupAction action)
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
    }
}