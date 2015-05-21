using System;
using PCF.OdmXml.i2b2Importer.DTO;

namespace PCF.OdmXml.i2b2Importer.Interfaces
{
    public interface IClinicalDataDao
    {
        void CleanupClinicalData(string projectId, string sourceSystem);
        void InsertObservation(I2B2ClinicalDataInfo clinicalDataInfo);
    }
}
