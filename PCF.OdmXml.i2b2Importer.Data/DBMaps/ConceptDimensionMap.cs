using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCF.OdmXml.i2b2Importer.Data
{
    public class ConceptDimensionMap : EntityTypeConfiguration<ConceptDimension>
    {
        public ConceptDimensionMap()
        {

            this.ToTable("CONCEPT_DIMENSION");

            this.Property(e => e.CONCEPT_PATH)
                .HasColumnName("CONCEPT_PATH")
                .IsUnicode(false);

            this.Property(e => e.CONCEPT_CD)
                .HasColumnName("CONCEPT_CD")
                .IsUnicode(false);

            this.Property(e => e.NAME_CHAR)
                .HasColumnName("NAME_CHAR")
                .IsUnicode(false);

            this.Property(e => e.CONCEPT_BLOB)
                .HasColumnName("CONCEPT_BLOB")
                .IsUnicode(false);

            this.Property(e => e.SOURCESYSTEM_CD)
                .HasColumnName("SOURCESYSTEM_CD")
                .IsUnicode(false);
        }
    }
}
