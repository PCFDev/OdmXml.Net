using System;
using System.Collections.Generic;
using PCF.OdmXml.i2b2Importer.DTO;

namespace PCF.OdmXml.i2b2Importer.Interfaces
{
    public interface IClinicalDataDao
    {
        void CleanupClinicalData(IEnumerable<ODMcomplexTypeDefinitionStudy> odmStudies, string sourceSystem);

        void InsertObservations(IEnumerable<I2B2ClinicalDataInfo> clinicalDatas);
    }
}
