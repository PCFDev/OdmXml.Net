using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PCF.OdmXml.i2b2Importer.Interfaces;

namespace PCF.OdmXml.i2b2Importer.DB
{
    public class ObservationFact : IObservationFact
    {
        public int EncounterNumber { get; set; }
        public string REDCapSubjectId { get; set; }
        public string ConceptCD { get; set; }
        public string ProviderId { get; set; }
        public DateTime StartDate { get; set; }
        public string ModifierCD { get; set; }
        public string ValueTypeCD { get; set; }
        public string TValueCharacter { get; set; }
        public decimal TValueNumber { get; set; }
        public int InstanceNumber { get; set; }
        public string ValueFlagCD { get; set; }
        public decimal QuantityNumber { get; set; }
        public string UnitsCD { get; set; }
        public DateTime EndDate { get; set; }
        public string LocationCD { get; set; }
        public string ObservationBlob { get; set; }
        public decimal ConfidenceNumber { get; set; }
        public DateTime UpdateDate { get; set; }
        public DateTime DownloadDate { get; set; }
        public DateTime ImportDate { get; set; }
        public string SourceSystemCD { get; set; }
        public int UploadId { get; set; }

        public static void BuildModel(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IObservationFact>()
                .Property(_ => _.EncounterNumber)
                .HasColumnName("Encounter_Num");

            modelBuilder.Entity<IObservationFact>()
                .Property(_ => _.REDCapSubjectId)
                .HasColumnName("REDCap_Subject_ID");

            modelBuilder.Entity<IObservationFact>()
                .Property(_ => _.ConceptCD)
                .HasColumnName("Concept_Cd");

            modelBuilder.Entity<IObservationFact>()
                .Property(_ => _.ProviderId)
                .HasColumnName("Provider_Id");

            modelBuilder.Entity<IObservationFact>()
                .Property(_ => _.StartDate)
                .HasColumnName("Start_Date");

            modelBuilder.Entity<IObservationFact>()
                .Property(_ => _.ModifierCD)
                .HasColumnName("Modifier_Cd");

            modelBuilder.Entity<IObservationFact>()
                .Property(_ => _.ValueTypeCD)
                .HasColumnName("ValType_Cd");

            modelBuilder.Entity<IObservationFact>()
                .Property(_ => _.TValueCharacter)
                .HasColumnName("TVal_Char");

            modelBuilder.Entity<IObservationFact>()
                .Property(_ => _.TValueNumber)
                .HasColumnName("NVal_Num");

            modelBuilder.Entity<IObservationFact>()
                .Property(_ => _.InstanceNumber)
                .HasColumnName("INSTANCE_NUM");

            modelBuilder.Entity<IObservationFact>()
                .Property(_ => _.ValueFlagCD)
                .HasColumnName("ValueFlag_Cd");

            modelBuilder.Entity<IObservationFact>()
                .Property(_ => _.QuantityNumber)
                .HasColumnName("Quantity_Num");

            modelBuilder.Entity<IObservationFact>()
                .Property(_ => _.UnitsCD)
                .HasColumnName("Units_Cd");

            modelBuilder.Entity<IObservationFact>()
                .Property(_ => _.EndDate)
                .HasColumnName("End_Date");

            modelBuilder.Entity<IObservationFact>()
                .Property(_ => _.LocationCD)
                .HasColumnName("Location_Cd");

            modelBuilder.Entity<IObservationFact>()
                .Property(_ => _.ObservationBlob)
                .HasColumnName("Observation_Blob");

            modelBuilder.Entity<IObservationFact>()
                .Property(_ => _.ConfidenceNumber)
                .HasColumnName("Confidence_Num");

            modelBuilder.Entity<IObservationFact>()
                .Property(_ => _.UpdateDate)
                .HasColumnName("UPDATE_DATE");

            modelBuilder.Entity<IObservationFact>()
                .Property(_ => _.DownloadDate)
                .HasColumnName("DOWNLOAD_DATE");

            modelBuilder.Entity<IObservationFact>()
                .Property(_ => _.ImportDate)
                .HasColumnName("IMPORT_DATE");

            modelBuilder.Entity<IObservationFact>()
                .Property(_ => _.SourceSystemCD)
                .HasColumnName("SOURCESYSTEM_CD");

            modelBuilder.Entity<IObservationFact>()
                .Property(_ => _.UploadId)
                .HasColumnName("UPLOAD_ID");
        }
    }
}
