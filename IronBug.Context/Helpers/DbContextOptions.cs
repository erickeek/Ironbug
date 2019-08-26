namespace IronBug.Context.Helpers
{
    public class DbContextOptions
    {
        public DbContextOptions()
        {
            CommitCount = 1000;
            AutoDetectChangesEnabled = false;
            ValidateOnSaveEnabled = false;
        }

        public int CommitCount { get; set; }
        public bool AutoDetectChangesEnabled { get; set; }
        public bool ValidateOnSaveEnabled { get; set; }
    }
}