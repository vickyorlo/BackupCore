namespace BackupCore
{
    class HashComparator : IComparator
    {
        public void UpdateEntry(FileContext db, ProcessedFile currentFile)
        {
            currentFile.FileHash = HashTools.HashFile(currentFile.FilePath);
            db.Files.Attach(currentFile);
            db.Entry(currentFile).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        }

        public bool WasFileUpdated(string path1, string path2)
        {
            return !HashTools.CompareHashes(HashTools.HashFile(path1), HashTools.HashFile(path2));
        }

        public bool WasFileUpdated(ProcessedFile currentFile)
        {
            return !HashTools.CompareHashes(HashTools.HashFile(currentFile.FilePath), currentFile.FileHash);
        }
    }
}