using System;
using System.Collections.Generic;
using PCF.OdmXml.i2b2Importer.DTO;

namespace PCF.OdmXml.i2b2Importer.Interfaces
{
    public interface IStudyDao
    {
        void InsertMetadata(IEnumerable<I2B2StudyInfo> studyInfo);

        void PreSetupI2B2Study(string projectId, string sourceSystem);
    }
}
