using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCF.OdmXml.i2b2Importer.Data
{
    public class ObservationFactMap : EntityTypeConfiguration<OBSERVATION_FACT>
    {
        public ObservationFactMap()
        {
            this.ToTable("OBSERVATION_FACT");

            this.Property(e => e.CONCEPT_CD)
                .IsUnicode(false);

            this.Property(e => e.PROVIDER_ID)
                .IsUnicode(false);

            this.Property(e => e.MODIFIER_CD)
                .IsUnicode(false);

            this.Property(e => e.VALTYPE_CD)
                .IsUnicode(false);

            this.Property(e => e.TVAL_CHAR)
                .IsUnicode(false);

            this.Property(e => e.NVAL_NUM)
                .HasPrecision(18, 5);

            this.Property(e => e.VALUEFLAG_CD)
                .IsUnicode(false);

            this.Property(e => e.QUANTITY_NUM)
                .HasPrecision(18, 5);

            this.Property(e => e.UNITS_CD)
                .IsUnicode(false);

            this.Property(e => e.LOCATION_CD)
                .IsUnicode(false);

            this.Property(e => e.OBSERVATION_BLOB)
                .IsUnicode(false);

            this.Property(e => e.CONFIDENCE_NUM)
                .HasPrecision(18, 5);

            this.Property(e => e.SOURCESYSTEM_CD)
                .IsUnicode(false);

        }
    }
}
