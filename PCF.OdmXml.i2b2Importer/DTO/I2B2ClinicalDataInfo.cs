using System;
using System.Text;

namespace PCF.OdmXml.i2b2Importer.DTO
{
    public class I2B2ClinicalDataInfo
    {
        public I2B2ClinicalDataInfo()
        {
            ProviderId = "@";
            ModifierCd = "@";
        }

        //observation_fact
        //TODO: Better names
        public string ConceptCd { get; set; }
        public decimal? ConfidenceNum { get; set; }//non nullable?//BigDecimal
        public DateTime? DownloadDate { get; set; }//non nullable?
        public int EncounterNum { get; set; }
        public DateTime? EndDate { get; set; }//non nullable?
        public DateTime? ImportDate { get; set; }//non nullable?
        public int InstanceNum { get; set; }
        public string LocationCd { get; set; }
        public string ModifierCd { get; set; }
        public decimal? NvalNum { get; set; }//non nullable?//BigDecimal
        public string ObservationBlob { get; set; }
        public string PatientNum { get; set; }
        public string ProviderId { get; set; }
        public decimal? QuantityNum { get; set; }//non nullable?//BigDecimal
        public string SourcesystemCd { get; set; }
        public DateTime? StartDate { get; set; }
        public string TvalChar { get; set; }
        public string UnitsCd { get; set; }
        public DateTime? UpdateDate { get; set; }//non nullable?
        public int UploadId { get; set; }
        public string ValTypeCd { get; set; }
        public string ValueFlagCd { get; set; }

        public override string ToString()
        {
            return new StringBuilder("I2B2ClinicalDataInfo [")
                .Append("patientNum=").Append(PatientNum)
                .Append(", encounterNum=").Append(EncounterNum)
                .Append(", instanceNum=").Append(InstanceNum)
                .Append(", conceptCd=").Append(ConceptCd)
                .Append(", modifierCd=").Append(ModifierCd)
                .Append(", startDate=").Append(StartDate)
                .Append(", endDate=").Append(EndDate)
                .Append(", valueFlagCd=").Append(ValueFlagCd)
                .Append(", valTypeCd=").Append(ValTypeCd)
                .Append(", tvalChar=").Append(TvalChar)
                .Append(", nvalNum=").Append(NvalNum)
                .Append(", quantityNum=").Append(QuantityNum)
                .Append(", unitsCd=").Append(UnitsCd)
                .Append(", sourcesystemCd=").Append(SourcesystemCd)
                .Append("]").ToString();
        }
    }
}
