using System;
using System.Linq;
using System.Transactions;
using EntityFramework.Extensions;
using PCF.OdmXml.i2b2Importer.Data;
using PCF.OdmXml.i2b2Importer.DTO;
using PCF.OdmXml.i2b2Importer.Interfaces;

namespace PCF.OdmXml.i2b2Importer.DB
{
    //TODO: Entity framework
    public class ClinicalDataDao : IClinicalDataDao
    {
        public void CleanupClinicalData(string projectId, string sourceSystem)
        {
            var conceptPattern = "STUDY|" + projectId + "|";//%
            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
            using (var context = new I2b2DbContext())
            {
                var studies = context.I2B2;
                var observations = context.OBSERVATION_FACT;
                var concepts = context.CONCEPT_DIMENSION;

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

                context.SaveChanges();
                scope.Complete();
            }
        }

        public void ExecuteBatch()
        {
            //throw new NotImplementedException();
        }

        //TODO: Batch processing
        public void InsertObservation(I2B2ClinicalDataInfo clinicalDataInfo)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
            using (var context = new I2b2DbContext())
            {
                var observations = context.OBSERVATION_FACT;
                var currentDate = DateTime.UtcNow;
                var observation = observations.Create();

                observation.CONCEPT_CD = clinicalDataInfo.ConceptCd;
                observation.CONFIDENCE_NUM = clinicalDataInfo.ConfidenceNum;
                observation.DOWNLOAD_DATE = clinicalDataInfo.DownloadDate;
                observation.ENCOUNTER_NUM = clinicalDataInfo.EncounterNum;
                observation.END_DATE = clinicalDataInfo.EndDate;
                observation.IMPORT_DATE = clinicalDataInfo.ImportDate;
                observation.INSTANCE_NUM = clinicalDataInfo.InstanceNum;
                observation.LOCATION_CD = clinicalDataInfo.LocationCd;
                observation.MODIFIER_CD = clinicalDataInfo.ModifierCd;
                observation.NVAL_NUM = clinicalDataInfo.NvalNum;
                observation.OBSERVATION_BLOB = clinicalDataInfo.ObservationBlob;
                observation.PATIENT_NUM = int.Parse(clinicalDataInfo.PatientNum);
                observation.QUANTITY_NUM = clinicalDataInfo.QuantityNum;
                observation.SOURCESYSTEM_CD = clinicalDataInfo.SourcesystemCd;
                observation.START_DATE = clinicalDataInfo.StartDate ?? currentDate;//???
                observation.TVAL_CHAR = clinicalDataInfo.TvalChar;
                observation.UNITS_CD = clinicalDataInfo.UnitsCd;
                observation.UPDATE_DATE = clinicalDataInfo.UpdateDate;
                observation.UPLOAD_ID = clinicalDataInfo.UploadId;
                observation.VALTYPE_CD = clinicalDataInfo.ValTypeCd;
                observation.VALUEFLAG_CD = clinicalDataInfo.ValueFlagCd;

                observations.Add(observation);
                context.SaveChanges();
                scope.Complete();
            }

            //if (Boolean.getBoolean("batch.disabled"))
            //{
            //    insertObservationStatement.execute();
            //}
            //else
            //{
            //    insertObservationStatement.addBatch();

            //    if (++observationBatchCount > BATCH_SIZE)
            //    {
            //        executeBatch();
            //    }
            //}
        }
    }
}
