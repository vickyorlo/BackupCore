using System;
using System.Collections.Generic;

namespace BackupCore
{
    /// <summary>
    /// A class containing all the data needed for a singular backup job.
    /// TODO: More fields for more advanced operation. Functionality switches.
    /// </summary>
    class BackupAction
    {
        public string SourcePath;
        public string DestinationPath;
        public List<string> FilesToCopy;
        public BackupMode Mode;
        public CompareMethod Comparator;

        public BackupAction(string source, string destination, BackupMode bmode = BackupMode.DatabaseCompareBackup)
        {
            SourcePath = source;
            DestinationPath = destination;
            FilesToCopy = RecursiveFileFinder.ProcessPath(source);
            Mode = bmode;
        }
    }

    enum BackupMode
    {
        DatabaseCompareBackup,
        FileCompareBackup
    };

    enum CompareMethod
    {
        HashComparator,
        WriteTimeComparator
    }
}