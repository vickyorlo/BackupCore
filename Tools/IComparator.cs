namespace BackupCore
{
    interface IComparator
    {
        bool WasFileUpdated(string path1, string path2);
        bool WasFileUpdated(ProcessedFile currentFile);
        void UpdateEntry(FileContext db, ProcessedFile currentFile);
    }
}