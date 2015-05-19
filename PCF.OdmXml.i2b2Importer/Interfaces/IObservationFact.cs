using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCF.OdmXml.i2b2Importer.Interfaces
{
    public interface IObservationFact
    {
        int EncounterNumber { get; set; }
        string REDCapSubjectId { get; set; }
        string ConceptCD { get; set; }
        string ProviderId { get; set; }
        DateTime StartDate { get; set; }
        string ModifierCD { get; set; }
        string ValueTypeCD { get; set; }
        string TValueCharacter { get; set; }
        decimal TValueNumber { get; set; }
        int InstanceNumber { get; set; }
        string ValueFlagCD { get; set; }
        decimal QuantityNumber { get; set; }
        string UnitsCD { get; set; }
        DateTime EndDate { get; set; }
        string LocationCD { get; set; }
        string ObservationBlob { get; set; }
        decimal ConfidenceNumber { get; set; }
        DateTime UpdateDate { get; set; }
        DateTime DownloadDate { get; set; }
        DateTime ImportDate { get; set; }
        string SourceSystemCD { get; set; }
        int UploadId { get; set; }
    }
}
