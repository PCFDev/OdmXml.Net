using System;
using PCF.OdmXml.i2b2Importer.Interfaces;

namespace PCF.OdmXml.i2b2Importer.DTO
{
    //TODO: Entity framework
    public class StudyDao : IStudyDao
    {
        public void ExecuteBatch()
        {
            throw new NotImplementedException();
        }

        public void InsertMetadata(I2B2StudyInfo studyInfo)
        {
            throw new NotImplementedException();
        }

        public void PreSetupI2B2Study(string projectId, string sourceSystem)
        {
            throw new NotImplementedException();
        }
    }
}
