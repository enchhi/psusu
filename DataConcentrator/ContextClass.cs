using System.Data.Entity;

namespace DataConcentrator
{
    // EF6 Code First kontekst. Tacno 3 tabele: Tags (TPH), Alarms, ActivatedAlarms.
    public class ContextClass : DbContext
    {
        // Singleton
        private static ContextClass instance;

        public static ContextClass Instance
        {
            get { return instance ?? (instance = new ContextClass()); }
        }

        // Koristi connection string "ContextClass" iz App.config (LocalDB).
        public ContextClass() : base("name=ContextClass") { }

        public DbSet<Tag> Tags { get; set; }
        public DbSet<Alarm> Alarms { get; set; }
        public DbSet<ActivatedAlarm> ActivatedAlarms { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // TPH: sve vrste tagova u jednoj tabeli "Tags".
            // Registrujemo naslednike da bi ih EF ukljucio u hijerarhiju (Discriminator kolona).
            modelBuilder.Entity<Tag>().ToTable("Tags");
            modelBuilder.Entity<AnalogInput>();
            modelBuilder.Entity<AnalogOutput>();
            modelBuilder.Entity<DigitalInput>();
            modelBuilder.Entity<DigitalOutput>();

            base.OnModelCreating(modelBuilder);
        }
    }
}
