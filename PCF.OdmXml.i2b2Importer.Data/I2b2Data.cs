namespace PCF.OdmXml.i2b2Importer.Data
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class I2b2Data : DbContext
    {
        public I2b2Data()
            : base("name=I2b2Data")
        {
        }

        public virtual DbSet<CONCEPT_DIMENSION> CONCEPT_DIMENSION { get; set; }
        public virtual DbSet<OBSERVATION_FACT> OBSERVATION_FACT { get; set; }
        public virtual DbSet<ONTOLOGY> I2B2 { get; set; }
        public virtual DbSet<TABLE_ACCESS> TABLE_ACCESS { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            modelBuilder.Configurations.Add(new OntologyMap());
            modelBuilder.Configurations.Add(new ConceptDimensionMap());
            modelBuilder.Configurations.Add(new ObservationFactMap());
            modelBuilder.Configurations.Add(new TableAccessMap());

        }
    }
}
