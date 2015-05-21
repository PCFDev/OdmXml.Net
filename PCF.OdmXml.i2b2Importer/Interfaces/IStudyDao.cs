using System;
using System.Collections.Generic;
using PCF.OdmXml.i2b2Importer.DTO;

namespace PCF.OdmXml.i2b2Importer.Interfaces
{
    public interface IStudyDao
    {
        void CleanStudies(string projectId, string sourceSystem);

        void InsertStudies(IEnumerable<I2B2StudyInfo> studyInfo);

        void SetupStudies();
    }
}
