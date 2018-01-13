namespace BackupCore
{
    interface IBackup
    {
        void Start(BackupAction action);
    }
}