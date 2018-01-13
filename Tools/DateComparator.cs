namespace BackupCore
{
    class DateComparator : IComparator
    {
        public bool WasFileUpdated(string path1, string path2)
        {
            return System.IO.File.GetLastWriteTime(path1) > System.IO.File.GetLastWriteTime(path2);

        }

        public bool WasFileUpdated(ProcessedFile currentFile)
        {
            return (System.IO.File.GetLastWriteTime(currentFile.BackupPath) > currentFile.DateModified);
        }
    }
}