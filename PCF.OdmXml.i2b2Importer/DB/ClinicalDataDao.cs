using System;
using PCF.OdmXml.i2b2Importer.DTO;
using PCF.OdmXml.i2b2Importer.Interfaces;

namespace PCF.OdmXml.i2b2Importer.DB
{
    //TODO: Entity framework
    public class ClinicalDataDao : IClinicalDataDao
    {
        public void CleanupClinicalData(string projectId, string sourceSystem)
        {
            //throw new NotImplementedException();
        }

        public void ExecuteBatch()
        {
            //throw new NotImplementedException();
        }

        public void InsertObservation(I2B2ClinicalDataInfo clinicalDataInfo)
        {
            //throw new NotImplementedException();
        }
    }
}
