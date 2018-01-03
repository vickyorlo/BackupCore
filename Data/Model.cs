using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Numerics;
using System.ComponentModel.DataAnnotations;

namespace BackupCore
{
    public class FileContext : DbContext
    {
        private string name = "Backup";
        public FileContext(string name)
        {
            this.name = name;
        }
        public DbSet<ProcessedFile> Files { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=" + name + ".db");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProcessedFile>()
                .HasKey(c => new { c.FilePath, c.BackupPath });
        }
    }

    /// <summary>
    /// A database entry for the file that has been processed by a backup action.
    /// TODO: Might need more fields for more advanced operation.
    /// </summary>
    public class ProcessedFile
    {
        public string FilePath { get; set; }
        public string BackupPath { get; set; }
        public string FileName { get; set; }
        public System.DateTime DateModified { get; set; }
        public byte[] FileHash { get; set; }
    }
}