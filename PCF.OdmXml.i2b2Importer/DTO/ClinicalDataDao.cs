using System;
using PCF.OdmXml.i2b2Importer.Interfaces;

namespace PCF.OdmXml.i2b2Importer.DTO
{
    //TODO: Entity framework
    public class ClinicalDataDao : IClinicalDataDao
    {
        public void CleanupClinicalData(string projectId, string sourceSystem)
        {
            throw new NotImplementedException();
        }

        public void ExecuteBatch()
        {
            throw new NotImplementedException();
        }

        public void InsertObservation(I2B2ClinicalDataInfo clinicalDataInfo)
        {
            throw new NotImplementedException();
        }
    }
}
