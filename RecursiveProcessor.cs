using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace BackupCore
{
    public static class RecursiveFileFinder
    {
        /// <summary>
        /// Takes a file system path and attemps to find all the files contained within and deeper in.
        /// Throws Exception when the given path is not valid.
        /// TODO: Throw custom exception?
        /// </summary>
        /// <param name="path">The path to deep-scan for files.</param>
        /// <returns>A list of paths to files found within the path</returns>
        public static List<string> ProcessPath(string path)
        {
            List<string> FoundFiles = new List<string>();
            if (File.Exists(path))
            {
                FoundFiles.Add(path);
            }
            else if (Directory.Exists(path))
            {
                ProcessDirectory(path, FoundFiles);
            }
            else
            {
                throw new Exception("Not a valid path");
            }
            return FoundFiles;
        }

        /// <summary>
        /// Scans all the entries in a directory, adding them to the list of found files if they're a file
        /// or sending them for further scanning if they're a directory.
        /// TODO: Asynchronize? Could probably scan multiple directories at once. Might end up having to add lists together. Possibly less nice in regard to memory.
        /// </summary>
        /// <param name="targetDirectory">A path to the directory to scan for files and/or directories</param>
        /// <param name="foundFiles">A list to put the found files in.</param>
        private static void ProcessDirectory(string targetDirectory, List<string> foundFiles)
        {
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
            {
                foundFiles.Add(fileName);
            }

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                ProcessDirectory(subdirectory, foundFiles);
        }
    }
}
