using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCF.OdmXml.i2b2Importer.Data
{
    public class OntologyMap : EntityTypeConfiguration<ONTOLOGY>
    {
        public OntologyMap()
        {
            this.ToTable("STUDY");

            this.Property(e => e.C_FULLNAME)
                .IsUnicode(false);

            this.Property(e => e.C_NAME)
                .IsUnicode(false);

            this.Property(e => e.C_SYNONYM_CD)
                .IsFixedLength()
                .IsUnicode(false);

            this.Property(e => e.C_VISUALATTRIBUTES)
                .IsFixedLength()
                .IsUnicode(false);

            this.Property(e => e.C_BASECODE)
                .IsUnicode(false);

            this.Property(e => e.C_METADATAXML)
                .IsUnicode(false);

            this.Property(e => e.C_FACTTABLECOLUMN)
                .IsUnicode(false);

            this.Property(e => e.C_TABLENAME)
                .IsUnicode(false);

            this.Property(e => e.C_COLUMNNAME)
                .IsUnicode(false);

            this.Property(e => e.C_COLUMNDATATYPE)
                .IsUnicode(false);

            this.Property(e => e.C_OPERATOR)
                .IsUnicode(false);

            this.Property(e => e.C_DIMCODE)
                .IsUnicode(false);

            this.Property(e => e.C_COMMENT)
                .IsUnicode(false);

            this.Property(e => e.C_TOOLTIP)
                .IsUnicode(false);

            this.Property(e => e.M_APPLIED_PATH)
                .IsUnicode(false);

            this.Property(e => e.SOURCESYSTEM_CD)
                .IsUnicode(false);

            this.Property(e => e.VALUETYPE_CD)
                .IsUnicode(false);

            this.Property(e => e.M_EXCLUSION_CD)
                .IsUnicode(false);

            this.Property(e => e.C_PATH)
                .IsUnicode(false);

            this.Property(e => e.C_SYMBOL)
                .IsUnicode(false);
        }
    }
}
