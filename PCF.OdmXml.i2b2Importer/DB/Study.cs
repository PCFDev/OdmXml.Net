using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PCF.OdmXml.i2b2Importer.Interfaces;

namespace PCF.OdmXml.i2b2Importer.DB
{
    public class Study : IStudy
    {
        public int HLevel { get; set; }
        public string FullName { get; set; }
        public string Name { get; set; }
        public string SynonymCD { get; set; }
        public string VisualAttributes { get; set; }
        public int TotalNumber { get; set; }
        public string BaseCode { get; set; }
        public string MetaDataXML { get; set; }
        public string FactTableColumn { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string ColumnDataType { get; set; }
        public string Operator { get; set; }
        public string DimCode { get; set; }
        public string Comment { get; set; }
        public string Tooltip { get; set; }
        public string AppliedPath { get; set; }
        public DateTime UpdateDate { get; set; }
        public DateTime DownloadDate { get; set; }
        public DateTime ImportDate { get; set; }
        public string SourceSystemCD { get; set; }
        public string ValueTypeCD { get; set; }

        public static void BuildModel(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IStudy>()
                .Property(_ => _.HLevel)
                .HasColumnName("C_HLEVEL");

            modelBuilder.Entity<IStudy>()
                .Property(_ => _.FullName)
                .HasColumnName("C_FULLNAME");

            modelBuilder.Entity<IStudy>()
                .Property(_ => _.Name)
                .HasColumnName("C_NAME");

            modelBuilder.Entity<IStudy>()
                .Property(_ => _.SynonymCD )
                .HasColumnName("C_SYNONYM_CD");

            modelBuilder.Entity<IStudy>()
                .Property(_ => _.VisualAttributes)
                .HasColumnName("C_VISUALATTRIBUTES");

            modelBuilder.Entity<IStudy>()
                .Property(_ => _.TotalNumber)
                .HasColumnName("C_TOTALNUM");

            modelBuilder.Entity<IStudy>()
                .Property(_ => _.BaseCode)
                .HasColumnName("C_BASECODE");

            modelBuilder.Entity<IStudy>()
                .Property(_ => _.MetaDataXML)
                .HasColumnName("C_METADATAXML");

            modelBuilder.Entity<IStudy>()
                .Property(_ => _.FactTableColumn)
                .HasColumnName("C_FACTTABLECOLUMN");

            modelBuilder.Entity<IStudy>()
                .Property(_ => _.TableName)
                .HasColumnName("C_TABLENAME");

            modelBuilder.Entity<IStudy>()
                .Property(_ => _.ColumnName)
                .HasColumnName("C_COLUMNNAME");

            modelBuilder.Entity<IStudy>()
                .Property(_ => _.ColumnDataType)
                .HasColumnName("C_COLUMNDATATYPE");

            modelBuilder.Entity<IStudy>()
                .Property(_ => _.Operator)
                .HasColumnName("C_OPERATOR");

            modelBuilder.Entity<IStudy>()
                .Property(_ => _.DimCode)
                .HasColumnName("C_DIMCODE");

            modelBuilder.Entity<IStudy>()
                .Property(_ => _.Comment)
                .HasColumnName("C_COMMENT");

            modelBuilder.Entity<IStudy>()
                .Property(_ => _.Tooltip)
                .HasColumnName("C_TOOLTIP");

            modelBuilder.Entity<IStudy>()
                .Property(_ => _.AppliedPath)
                .HasColumnName("M_APPLIED_PATH");

            modelBuilder.Entity<IStudy>()
                .Property(_ => _.UpdateDate)
                .HasColumnName("UPDATE_DATE");

            modelBuilder.Entity<IStudy>()
                .Property(_ => _.DownloadDate)
                .HasColumnName("DOWNLOAD_DATE");

            modelBuilder.Entity<IStudy>()
                .Property(_ => _.ImportDate)
                .HasColumnName("IMPORT_DATE");

            modelBuilder.Entity<IStudy>()
                .Property(_ => _.SourceSystemCD)
                .HasColumnName("SOURCESYSTEM_CD");

            modelBuilder.Entity<IStudy>()
                .Property(_ => _.ValueTypeCD)
                .HasColumnName("VALUETYPE_CD");
        }
    }
}
