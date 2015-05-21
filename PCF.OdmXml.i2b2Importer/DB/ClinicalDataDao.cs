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
    public class ClinicalDataDao : IClinicalDataDao
    {
        public void CleanupClinicalData(IEnumerable<ODMcomplexTypeDefinitionStudy> odmStudies, string sourceSystem)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
            using (var context = new I2b2DbContext())
            {
                var studies = context.Studies;
                var observations = context.ObservationFacts;
                var concepts = context.ConceptDimensions;

                foreach (var study in odmStudies)
                {
                    var projectId = study.OID;
                    var conceptPattern = "STUDY|" + projectId + "|";//%

                    observations.Where(_ => _.CONCEPT_CD.StartsWith(conceptPattern) && _.SOURCESYSTEM_CD == sourceSystem).Delete();
                    concepts.Where(_ => _.CONCEPT_CD.StartsWith(conceptPattern) && _.SOURCESYSTEM_CD == sourceSystem).Delete();

                    //INSERT INTO
                    //    Concept_Dimension (concept_path, concept_cd, name_char, update_date, download_date, import_date, sourcesystem_cd)
                    //SELECT
                    //    C_DIMCODE, C_BASECODE, C_NAME, UPDATE_DATE, DOWNLOAD_DATE, IMPORT_DATE, SOURCESYSTEM_CD
                    //FROM
                    //    STUDY
                    //WHERE
                    //    C_BASECODE LIKE <conceptPattern>

                    var newConcepts = studies.Where(_ => _.C_BASECODE.StartsWith(conceptPattern));
                    //TODO: Bulk insert?
                    //Gross
                    foreach (var newConcept in newConcepts)
                    {
                        var concept = concepts.Create();

                        concept.CONCEPT_PATH = newConcept.C_DIMCODE;
                        concept.CONCEPT_CD = newConcept.C_BASECODE;
                        concept.NAME_CHAR = newConcept.C_NAME;
                        concept.UPDATE_DATE = newConcept.UPDATE_DATE;
                        concept.DOWNLOAD_DATE = newConcept.DOWNLOAD_DATE;
                        concept.IMPORT_DATE = newConcept.IMPORT_DATE;
                        concept.SOURCESYSTEM_CD = newConcept.SOURCESYSTEM_CD;

                        concepts.Add(concept);
                    }
                }

                context.SaveChanges();
                scope.Complete();
            }
        }

        public void InsertObservations(IEnumerable<I2B2ClinicalDataInfo> clinicalDatas)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
            using (var context = new I2b2DbContext())
            {
                var observations = context.ObservationFacts;
                var currentDate = DateTime.UtcNow;

                //Bulk insert performance, memory usage? http://stackoverflow.com/a/5942176
                //Batch sizing?
                foreach (var clinicalData in clinicalDatas)
                {
                    var observation = observations.Create();

                    observation.CONCEPT_CD = clinicalData.ConceptCd;
                    observation.CONFIDENCE_NUM = clinicalData.ConfidenceNum;
                    observation.DOWNLOAD_DATE = clinicalData.DownloadDate;
                    observation.ENCOUNTER_NUM = clinicalData.EncounterNum;
                    observation.END_DATE = clinicalData.EndDate;
                    observation.IMPORT_DATE = clinicalData.ImportDate;
                    observation.INSTANCE_NUM = clinicalData.InstanceNum;
                    observation.LOCATION_CD = clinicalData.LocationCd;
                    observation.MODIFIER_CD = clinicalData.ModifierCd;
                    observation.NVAL_NUM = clinicalData.NvalNum;
                    observation.OBSERVATION_BLOB = clinicalData.ObservationBlob;
                    observation.PATIENT_NUM = int.Parse(clinicalData.PatientNum);
                    observation.QUANTITY_NUM = clinicalData.QuantityNum;
                    observation.SOURCESYSTEM_CD = clinicalData.SourcesystemCd;
                    observation.START_DATE = clinicalData.StartDate ?? currentDate;//???
                    observation.TVAL_CHAR = clinicalData.TvalChar;
                    observation.UNITS_CD = clinicalData.UnitsCd;
                    observation.UPDATE_DATE = clinicalData.UpdateDate;
                    observation.UPLOAD_ID = clinicalData.UploadId;
                    observation.VALTYPE_CD = clinicalData.ValTypeCd;
                    observation.VALUEFLAG_CD = clinicalData.ValueFlagCd;

                    observations.Add(observation);
                }

                context.SaveChanges();
                scope.Complete();
            }
        }
    }
}
