namespace BackupCore
{
    interface Comparator
    {
        bool AreFilesDifferent(string path1, string path2);
    }
}