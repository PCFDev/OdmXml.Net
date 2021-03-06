namespace PCF.OdmXml.i2b2Importer.Data
{
    using System;
    using System.Data.Entity;

    public partial class I2b2DbContext : DbContext
    {
        public I2b2DbContext()
            : this(true)
        {
        }

        public I2b2DbContext(bool isReadWrite)
            : base("name=I2b2Data")
        {
            if (isReadWrite)
            {
                this.Configuration.ProxyCreationEnabled = true;
                this.Configuration.ValidateOnSaveEnabled = true;
            }
            else
            {
                this.Configuration.ProxyCreationEnabled = true; // ugh
                this.Configuration.AutoDetectChangesEnabled = false;
                this.Configuration.LazyLoadingEnabled = true; // ugh
                this.Configuration.ValidateOnSaveEnabled = false;   // no saving here!
            }
        }

        public virtual DbSet<ConceptDimension> ConceptDimensions { get; set; }
        public virtual DbSet<ObservationFact> ObservationFacts { get; set; }
        public virtual DbSet<Study> Studies { get; set; }
        public virtual DbSet<TableAccess> TableAccess { get; set; }//Not needed?

        //TODO: Move attribute to fluent in maps?
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new StudyMap());
            modelBuilder.Configurations.Add(new ConceptDimensionMap());
            modelBuilder.Configurations.Add(new ObservationFactMap());
            modelBuilder.Configurations.Add(new TableAccessMap());
        }
    }
}
