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
        public string ActionName { get; }
        public string SourcePath { get; }
        public string DestinationPath { get; }
        public List<string> FilesToCopy { get; }
        public BackupMode Mode { get; }
        public CompareMethod Comparator { get; }
        public int BackupCopies { get; }
        public bool Archive { get; }
        public string ArchivePassword { get; }

        public BackupAction(string name, string source, string destination, BackupMode bmode = BackupMode.DatabaseCompareBackup, CompareMethod comparator = CompareMethod.WriteTimeComparator,
            int copies = 1, bool archive = false, string pass = "")
        {
            ActionName = name;
            SourcePath = source;
            DestinationPath = destination;
            FilesToCopy = RecursiveFileFinder.ProcessPath(source, true);
            Mode = bmode;
            Comparator = comparator;
            BackupCopies = copies;
            Archive = archive;
            ArchivePassword = pass;
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