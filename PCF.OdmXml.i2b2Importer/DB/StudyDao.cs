using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using EntityFramework.Extensions;
using PCF.OdmXml.i2b2Importer.Data;
using PCF.OdmXml.i2b2Importer.DTO;
using PCF.OdmXml.i2b2Importer.Interfaces;

namespace PCF.OdmXml.i2b2Importer.DB
{
    //TODO: Entity framework
    public class StudyDao : IStudyDao
    {
        public void CleanStudies(string projectId, string sourceSystem)
        {
            var cPath = "\\STUDY\\" + sourceSystem + ":" + projectId + "\\";//%
            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
            using (var context = new I2b2DbContext())
            {
                var studies = context.Studies;
                var currentDate = DateTime.UtcNow;

                studies.Where(_ => _.C_FULLNAME.StartsWith(cPath)).Delete();

                context.SaveChanges();
                scope.Complete();
            }
        }

        //TODO: Batch processing
        public void InsertStudies(IEnumerable<I2B2StudyInfo> studyInfos)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
            using (var context = new I2b2DbContext())
            {
                var studies = context.Studies;
                var currentDate = DateTime.UtcNow;

                foreach (var studyInfo in studyInfos)
                {
                    var study = studies.Create();

                    study.C_BASECODE = studyInfo.Cbasecode;
                    study.C_COLUMNDATATYPE = studyInfo.CcolumnDatatype;
                    study.C_COLUMNNAME = studyInfo.Ccolumnname;
                    study.C_COMMENT = studyInfo.Ccomment;
                    study.C_DIMCODE = studyInfo.Cdimcode;
                    study.C_FACTTABLECOLUMN = studyInfo.CfactTableColumn;
                    study.C_FULLNAME = studyInfo.Cfullname;
                    study.C_HLEVEL = studyInfo.Chlevel;
                    study.C_METADATAXML = studyInfo.Cmetadataxml;
                    study.C_NAME = studyInfo.Cname;
                    study.C_OPERATOR = studyInfo.Coperator;
                    study.C_SYNONYM_CD = studyInfo.CsynonmCd;
                    study.C_TABLENAME = studyInfo.Ctablename;
                    study.C_TOOLTIP = studyInfo.Ctooltip;
                    study.C_TOTALNUM = studyInfo.CtotalNum;
                    study.C_VISUALATTRIBUTES = studyInfo.CvisualAttributes;
                    study.DOWNLOAD_DATE = studyInfo.DownloadDate;
                    study.IMPORT_DATE = studyInfo.ImportDate;
                    study.M_APPLIED_PATH = studyInfo.MappliedPath;
                    study.SOURCESYSTEM_CD = studyInfo.SourceSystemCd;
                    study.UPDATE_DATE = studyInfo.UpdateDate ?? currentDate;//???
                    study.VALUETYPE_CD = studyInfo.Valuetype;

                    studies.Add(study);
                }

                context.SaveChanges();
                scope.Complete();
            }
        }

        public void SetupStudies()
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
            using (var context = new I2b2DbContext())
            {
                var studies = context.Studies;
                var currentDate = DateTime.UtcNow;

                //FirstOrDefault?
                if (!studies.Any(_ => _.C_HLEVEL == 0 && _.C_FULLNAME == "\\STUDY\\"))
                {
                    var study = studies.Create();
                    study.C_BASECODE = null;
                    study.C_COLUMNDATATYPE = "T";
                    study.C_COLUMNNAME = "concept_path";
                    study.C_COMMENT = null;
                    study.C_DIMCODE = "\\STUDY\\";
                    study.C_FACTTABLECOLUMN = "concept_cd";
                    study.C_FULLNAME = "\\STUDY\\";
                    study.C_HLEVEL = 0;
                    study.C_METADATAXML = null;
                    study.C_NAME = "Study";
                    study.C_OPERATOR = "LIKE";
                    study.C_SYNONYM_CD = "N";
                    study.C_TABLENAME = "concept_dimension";
                    study.C_TOOLTIP = "STUDY";
                    study.C_TOTALNUM = 0;
                    study.C_VISUALATTRIBUTES = "FA";
                    study.DOWNLOAD_DATE = currentDate;
                    study.IMPORT_DATE = currentDate;
                    study.M_APPLIED_PATH = "@";
                    study.SOURCESYSTEM_CD = null;
                    study.UPDATE_DATE = currentDate;
                    study.VALUETYPE_CD = null;
                    studies.Add(study);
                }

                context.SaveChanges();
                scope.Complete();
            }
        }
    }
}
