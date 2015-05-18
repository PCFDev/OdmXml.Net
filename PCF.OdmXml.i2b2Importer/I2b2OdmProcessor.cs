using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using PCF.OdmXml.i2b2Importer.DB;
using PCF.OdmXml.i2b2Importer.DTO;
using PCF.OdmXml.i2b2Importer.Helpers;
using PCF.OdmXml.i2b2Importer.Interfaces;

//private static Logger log = LoggerFactory.getLogger(I2B2ODMStudyHandler.class);
namespace PCF.OdmXml.i2b2Importer
{
    /// <summary>
    /// An ODMXML importer for i2b2
    /// </summary>
    public class I2b2OdmProcessor
    {
        #region Properties

        //Assuming we want UTC date for now.
        private DateTime CurrentDate = DateTime.UtcNow;

        private ODM ODM { get; set; }

        private IClinicalDataDao ClinicalDataDao { get; set; }
        private I2B2ClinicalDataInfo ClinicalDataInfo { get; set; }
        private IStudyDao StudyDao { get; set; }
        private I2B2StudyInfo StudyInfo { get; set; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Constructor to set ODM object
        /// </summary>
        /// <param name="odm">The entire ODM tree.</param>
        public I2b2OdmProcessor(ODM odm, IDictionary<string, string> settings)//settings?
        {
            ODM = odm;

            ClinicalDataDao = new ClinicalDataDao();//TODO: Entity framework
            ClinicalDataInfo = new I2B2ClinicalDataInfo { SourcesystemCd = odm.SourceSystem };
            StudyDao = new StudyDao();//TODO: Entity framework
            StudyInfo = new I2B2StudyInfo { SourceSystemCd = odm.SourceSystem };
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// method to parse ODM and save data into i2b2
        /// </summary>
        public void ProcessODM()
        {
            //log.info("Start to parse ODM xml and save to i2b2");
            Debug.WriteLine("Start to parse ODM xml and save to i2b2");

            // build the call
            ProcessODMStudy();
            ProcessODMClinicalData();
        }

        /// <summary>
        /// This method takes ODM XML io.File obj as input and parsed by JAXB API and
        /// then traversal through ODM tree object and save clinical data into i2b2
        /// demo database ini2b2 data format. Keep method public in case of only want
        /// to parse demodata.
        /// </summary>
        public void ProcessODMClinicalData()
        {
            //log.info("Parse and save ODM clinical data into i2b2...");
            Debug.WriteLine("Parse and save ODM clinical data into i2b2...");

            // traverse through the clinical data to:
            // 1) Lookup the concept path from odm study metadata.
            // 2) Set patient and clinical information into observation fact.
            if (ODM.ClinicalData == null || ODM.ClinicalData.Count == 0)
            {
                //log.info("ODM does not contain clinical data");
                Debug.WriteLine("ODM does not contain clinical data");
                return;
            }

            foreach (var study in ODM.Study)
            {
                ClinicalDataDao.CleanupClinicalData(study.OID, ODM.SourceSystem);
            }

            foreach (var clinicalData in ODM.ClinicalData)
            {
                if (clinicalData.SubjectData == null)
                    continue;

                //log.info("Save Clinical data for study OID " + clinicalData.getStudyOID() + " into i2b2...");
                Debug.WriteLine("Save Clinical data for study OID " + clinicalData.StudyOID + " into i2b2...");
                var timer = Stopwatch.StartNew();

                var study = Utilities.GetStudy(ODM, clinicalData.StudyOID);
                if (study == null)
                {
                    //log.error("ODM does not contain study metadata for study OID " + clinicalData.getStudyOID());
                    Debug.WriteLine("ODM does not contain study metadata for study OID " + clinicalData.StudyOID);

                    continue;
                }

                /*
                 * Generate a unique encounter number per subject per study to ensure that
                 * observation fact primary key is not violated.
                 */
                var encounterNum = 0;
                //5 nested loops, gross.
                foreach (var subjectData in clinicalData.SubjectData)
                {
                    if (subjectData.StudyEventData == null)
                        continue;

                    encounterNum++;

                    foreach (var studyEventData in subjectData.StudyEventData)
                    {
                        if (studyEventData.FormData == null)
                            continue;

                        foreach (var formData in studyEventData.FormData)
                        {
                            if (formData.ItemGroupData == null)
                                continue;

                            foreach (var itemGroupData in formData.ItemGroupData)
                            {
                                if (itemGroupData.Items == null)
                                    continue;

                                foreach (var itemData in itemGroupData.Items.Where(_ => _ is ODMcomplexTypeDefinitionItemData).Select(_ => _ as ODMcomplexTypeDefinitionItemData).ToList())//getItemDataGroup()
                                {
                                    if (itemData.Value != null)
                                        SaveItemData(study, subjectData, studyEventData, formData, itemData, encounterNum);
                                }
                            }
                        }
                    }
                }

                /*
                 * Flush any remaining batched up observations;
                 */
                ClinicalDataDao.ExecuteBatch();

                timer.Stop();
                //log.info("Completed Clinical data to i2b2 for study OID " + clinicalData.getStudyOID() + " in " + (endTime - startTime) + " ms");
                Debug.WriteLine("Completed Clinical data to i2b2 for study OID " + clinicalData.StudyOID + " in " + timer.ElapsedMilliseconds + " ms");
            }
        }

        /// <summary>
        /// This method takes ODM XML io.File obj as input and parsed by JAXB API and
        /// then traverses through ODM tree object and save data into i2b2 metadata
        /// database in i2b2 data format.
        /// </summary>
        public void ProcessODMStudy()
        {
            /*
             * Need to traverse through the study definition to: 1) Lookup all
             * definition values in tree nodes. 2) Set node values into i2b2 bean
             * info and ready for populating into i2b2 database.
             */
            foreach (var study in ODM.Study)
            {
                //log.info("Processing study metadata for study " + study.getGlobalVariables().getStudyName().getValue() + "(OID " + study.OID + ")");
                Debug.WriteLine("Processing study metadata for study " + study.GlobalVariables.StudyName.Value + "(OID " + study.OID + ")");
                //log.info("Deleting old study metadata and data");
                Debug.WriteLine("Deleting old study metadata and data");

                StudyDao.PreSetupI2B2Study(study.OID, ODM.SourceSystem);

                //log.info("Inserting study metadata into i2b2");
                Debug.WriteLine("Inserting study metadata into i2b2");
                var timer = Stopwatch.StartNew();

                SaveStudy(study);

                timer.Stop();
                //log.info("Completed loading study metadata into i2b2 in " + (endTime - startTime) + " ms");
                Debug.WriteLine("Completed loading study metadata into i2b2 in " + timer.ElapsedMilliseconds + " ms");
            }

            /*
             * Flush any remaining batched up records.
             */
            StudyDao.ExecuteBatch();
        }

        #endregion Public Methods

        #region Private Methods

        private string CreateMetadataXml(ODMcomplexTypeDefinitionStudy study, ODMcomplexTypeDefinitionItemDef itemDef)
        {
            var metadataXml = default(string);

            switch (itemDef.DataType)
            {
                case DataType.integer:
                    metadataXml = MetaDataXML.GetIntegerMetadataXML(itemDef.OID, itemDef.Name);
                    break;
                case DataType.@float:
                case DataType.@double:
                    metadataXml = MetaDataXML.GetFloatMetadataXML(itemDef.OID, itemDef.Name);
                    break;
                case DataType.text:
                case DataType.@string:
                    if (itemDef.CodeListRef == null)
                        metadataXml = MetaDataXML.GetStringMetadataXML(itemDef.OID, itemDef.Name);
                    else
                    {
                        var codeList = Utilities.GetCodeList(study, itemDef.CodeListRef.CodeListOID);
                        var codeListValues = Utilities.GetCodeListValues(codeList, "en");
                        metadataXml = MetaDataXML.GetEnumMetadataXML(itemDef.OID, itemDef.Name, codeListValues);
                    }
                    break;
                case DataType.boolean:
                    break;
                case DataType.date:
                case DataType.time:
                case DataType.datetime:
                    metadataXml = MetaDataXML.GetStringMetadataXML(itemDef.OID, itemDef.Name);
                    break;
                default:
                    break;
            }

            return metadataXml;
        }

        /// <summary>
        /// Create concept code with all OIDs and make the total length less than 50 and unique.
        /// </summary>
        /// <param name="studyOID"></param>
        /// <param name="studyEventOID"></param>
        /// <param name="formOID"></param>
        /// <param name="itemOID"></param>
        /// <param name="value"></param>
        /// <returns>The unique concept code.</returns>
        private string GenerateConceptCode(string studyOID, string studyEventOID, string formOID, string itemOID, string value)
        {
            var concept = new StringBuilder("STUDY|")
                .Append(studyOID)
                .Append("|");

            //I don't think we want quite use StringBuilder here becuase the pipes are byte cast chars, not Unicode literals. md5("\x00\x7C") vs md5("\x7C")
            var message = new ByteArrayBulder()
                .Append(Encoding.UTF8.GetBytes(ODM.SourceSystem ?? String.Empty))
                .Append((byte)'|')
                .Append(Encoding.UTF8.GetBytes(studyEventOID))
                .Append((byte)'|')
                .Append(Encoding.UTF8.GetBytes(formOID))
                .Append((byte)'|')
                .Append(Encoding.UTF8.GetBytes(itemOID));

            if (value != null)
                message.Append((byte)'|').Append(Encoding.UTF8.GetBytes(value));

            using (var md5 = MD5.Create())
            {
                var digest = md5.ComputeHash(message.GetBytes());
                var hex = BitConverter.ToString(digest).Replace("-", "").ToLowerInvariant();
                concept.Append(hex);
            }

            var conceptCode = concept.ToString();
            //if (log.isDebugEnabled()) {
            //    log.debug(new StringBuffer("Concept code ").append(conceptCode)
            //            .append(" generated for studyOID=").append(studyOID)
            //            .append(", studyEventOID=").append(studyEventOID)
            //            .append(", formOID=").append(formOID)
            //            .append(", itemOID=").append(itemOID)
            //            .append(", value=").append(value).toString());
            //}
            Debug.WriteLine(new StringBuilder("Concept code ")
                .Append(conceptCode)
                .Append(" generated for studyOID=").Append(studyOID)
                .Append(", studyEventOID=").Append(studyEventOID)
                .Append(", formOID=").Append(formOID)
                .Append(", itemOID=").Append(itemOID)
                .Append(", value=").Append(value).ToString());

            return conceptCode;
        }

        private string GetTranslatedDescription(ODMcomplexTypeDefinitionDescription description, string lang, string defaultValue)
        {
            if (description != null)
            {
                foreach (var translatedText in description.TranslatedText)
                {
                    if (translatedText.lang.Equals(lang))
                        return translatedText.Value;
                }
            }

            return defaultValue;
        }

        private void LogStudyInfo()
        {
            //if (log.isDebugEnabled()) {
            //    log.debug("Inserting study metadata record: " + studyInfo);
            //}
            Debug.WriteLine("Inserting study metadata record: " + StudyInfo);
        }

        /// <summary>
        /// Set up i2b2 metadata level 5 (TranslatedText) info into STUDY
        /// </summary>
        /// <param name="study"></param>
        /// <param name="studyEventDef"></param>
        /// <param name="formDef"></param>
        /// <param name="itemDef"></param>
        /// <param name="codeListItem"></param>
        /// <param name="itemPath"></param>
        /// <param name="itemToolTip"></param>
        private void SaveCodeListItem(ODMcomplexTypeDefinitionStudy study,
                                      ODMcomplexTypeDefinitionStudyEventDef studyEventDef,
                                      ODMcomplexTypeDefinitionFormDef formDef,
                                      ODMcomplexTypeDefinitionItemDef itemDef,
                                      ODMcomplexTypeDefinitionCodeListItem codeListItem,
                                      string itemPath,
                                      string itemToolTip)
        {
            var value = Utilities.GetTranslatedValue(codeListItem, "en");
            var codedValue = codeListItem.CodedValue;
            var codeListItemPath = itemPath + codedValue + "\\";
            var codeListItemToolTip = itemToolTip + "\\" + value;

            // set c_hlevel 5 data (TranslatedText)
            StudyInfo.Chlevel = Constants.C_HLEVEL_5;
            StudyInfo.Cfullname = codeListItemPath;
            StudyInfo.Cname = GetTranslatedDescription(itemDef.Description, "en", itemDef.Name) + ": " + value;
            StudyInfo.Cbasecode = GenerateConceptCode(study.OID, studyEventDef.OID, formDef.OID, itemDef.OID, codedValue);
            StudyInfo.Cdimcode = codeListItemPath;
            StudyInfo.Ctooltip = codeListItemToolTip;
            StudyInfo.Cmetadataxml = null;
            StudyInfo.CvisualAttributes = Constants.C_VISUALATTRIBUTES_LEAF;

            LogStudyInfo();

            StudyDao.InsertMetadata(StudyInfo);
        }

        /// <summary>
        /// Set up i2b2 metadata level 2 (Event) info into STUDY
        /// </summary>
        /// <param name="study"></param>
        /// <param name="studyEventDef"></param>
        /// <param name="studyPath"></param>
        /// <param name="studyToolTip"></param>
        private void SaveEvent(ODMcomplexTypeDefinitionStudy study,
                               ODMcomplexTypeDefinitionStudyEventDef studyEventDef,
                               string studyPath,
                               string studyToolTip)
        {
            var eventPath = studyPath + studyEventDef.OID + "\\";
            var eventToolTip = studyToolTip + "\\" + studyEventDef.OID;

            // set c_hlevel 2 data (StudyEvent)
            StudyInfo.Chlevel = Constants.C_HLEVEL_2;
            StudyInfo.Cfullname = eventPath;
            StudyInfo.Cname = studyEventDef.Name;
            StudyInfo.Cdimcode = eventPath;
            StudyInfo.Ctooltip = eventToolTip;
            StudyInfo.CvisualAttributes = Constants.C_VISUALATTRIBUTES_FOLDER;

            LogStudyInfo();

            // insert level 2 data
            StudyDao.InsertMetadata(StudyInfo);

            foreach (var formRef in studyEventDef.FormRef)
            {
                var formDef = Utilities.GetForm(study, formRef.FormOID);

                SaveForm(study, studyEventDef, formDef, eventPath, eventToolTip);
            }
        }

        /// <summary>
        /// Set up i2b2 metadata level 3 (Form) info into STUDY
        /// </summary>
        /// <param name="study"></param>
        /// <param name="studyEventDef"></param>
        /// <param name="formDef"></param>
        /// <param name="eventPath"></param>
        /// <param name="eventToolTip"></param>
        private void SaveForm(ODMcomplexTypeDefinitionStudy study,
                              ODMcomplexTypeDefinitionStudyEventDef studyEventDef,
                              ODMcomplexTypeDefinitionFormDef formDef,
                              string eventPath,
                              string eventToolTip)
        {
            var formPath = eventPath + formDef.OID + "\\";
            var formToolTip = eventToolTip + "\\" + formDef.OID;

            // set c_hlevel 3 data (Form)
            StudyInfo.Chlevel = Constants.C_HLEVEL_3;
            StudyInfo.Cfullname = formPath;
            StudyInfo.Cname = GetTranslatedDescription(formDef.Description, "en", formDef.Name);
            StudyInfo.Cdimcode = formPath;
            StudyInfo.Ctooltip = formToolTip;
            StudyInfo.CvisualAttributes = Constants.C_VISUALATTRIBUTES_FOLDER;

            LogStudyInfo();

            // insert level 3 data
            StudyDao.InsertMetadata(StudyInfo);

            foreach (var itemGroupRef in formDef.ItemGroupRef)
            {
                var itemGroupDef = Utilities.GetItemGroup(study, itemGroupRef.ItemGroupOID);
                if (itemGroupDef.ItemRef != null)
                {
                    foreach (var itemRef in itemGroupDef.ItemRef)
                    {
                        var itemDef = Utilities.GetItem(study, itemRef.ItemOID);

                        SaveItem(study, studyEventDef, formDef, itemDef, formPath, formToolTip);
                    }
                }
            }
        }

        /// <summary>
        /// Set up i2b2 metadata level 4 (Item) info into STUDY and CONCEPT_DIMENSION
        /// </summary>
        /// <param name="study"></param>
        /// <param name="studyEventDef"></param>
        /// <param name="formDef"></param>
        /// <param name="itemDef"></param>
        /// <param name="formPath"></param>
        /// <param name="formToolTip"></param>
        private void SaveItem(ODMcomplexTypeDefinitionStudy study,
                              ODMcomplexTypeDefinitionStudyEventDef studyEventDef,
                              ODMcomplexTypeDefinitionFormDef formDef,
                              ODMcomplexTypeDefinitionItemDef itemDef,
                              string formPath,
                              string formToolTip)
        {
            var itemPath = formPath + itemDef.OID + "\\";
            var itemToolTip = formToolTip + "\\" + itemDef.OID;

            // set c_hlevel 4 data (Items)
            StudyInfo.Chlevel = Constants.C_HLEVEL_4;
            StudyInfo.Cfullname = itemPath;
            StudyInfo.Cname = GetTranslatedDescription(itemDef.Description, "en", itemDef.Name);
            StudyInfo.Cbasecode = GenerateConceptCode(study.OID, studyEventDef.OID, formDef.OID, itemDef.OID, null);
            StudyInfo.Cdimcode = itemPath;
            StudyInfo.Ctooltip = itemToolTip;
            StudyInfo.Cmetadataxml = CreateMetadataXml(study, itemDef);

            // It is a leaf node
            StudyInfo.CvisualAttributes = itemDef.CodeListRef == null
                                        ? Constants.C_VISUALATTRIBUTES_LEAF
                                        : Constants.C_VISUALATTRIBUTES_FOLDER;
            LogStudyInfo();

            // insert level 4 data
            StudyDao.InsertMetadata(StudyInfo);

            if (itemDef.CodeListRef != null)
            {
                var codeList = Utilities.GetCodeList(study, itemDef.CodeListRef.CodeListOID);
                if (codeList != null)
                {
                    foreach (var codeListItem in codeList.Items.Where(_ => _ is ODMcomplexTypeDefinitionCodeListItem).Select(_ => _ as ODMcomplexTypeDefinitionCodeListItem).ToList())//getCodeListItem()
                    {
                        // save
                        // level 5
                        SaveCodeListItem(study, studyEventDef, formDef, itemDef, codeListItem, itemPath, itemToolTip);
                    }
                }
            }
        }

        private void SaveItemData(ODMcomplexTypeDefinitionStudy study,
                                  ODMcomplexTypeDefinitionSubjectData subjectData,
                                  ODMcomplexTypeDefinitionStudyEventData studyEventData,
                                  ODMcomplexTypeDefinitionFormData formData,
                                  ODMcomplexTypeDefinitionItemData itemData,
                                  int encounterNum)
        {
            var itemValue = itemData.Value;
            var item = Utilities.GetItem(study, itemData.ItemOID);
            var conceptCd = default(string);

            if (item.CodeListRef != null)
            {
                ClinicalDataInfo.ValTypeCd = Constants.VALUE_TYPE_TEXT;
                ClinicalDataInfo.NvalNum = null;

                var codeList = Utilities.GetCodeList(study, item.CodeListRef.CodeListOID);
                var codeListItem = Utilities.GetCodeListItem(codeList, itemValue);

                if (codeListItem == null)
                {
                    //log.error("Code list item for coded value: " + itemValue + " not found in code list: " + codeList.getOID());
                    Debug.WriteLine("Code list item for coded value: " + itemValue + " not found in code list: " + codeList.OID);
                    return;
                }
                else
                {
                    /*
                     * Need to include the item value in the concept code, since there is a different code for each code list item.
                     */
                    conceptCd = GenerateConceptCode(study.OID, studyEventData.StudyEventOID, formData.FormOID, itemData.ItemOID, itemValue);

                    ClinicalDataInfo.TvalChar = Utilities.GetTranslatedValue(codeListItem, "en");
                }
            }
            else if (Utilities.IsNumeric(item.DataType))
            {
                conceptCd = GenerateConceptCode(study.OID, studyEventData.StudyEventOID, formData.FormOID, itemData.ItemOID, null);

                ClinicalDataInfo.ValTypeCd = Constants.VALUE_TYPE_NUMBER;
                ClinicalDataInfo.TvalChar = "E";//TODO: Magic
                ClinicalDataInfo.NvalNum = String.IsNullOrWhiteSpace(itemValue) ? default(decimal?) : Decimal.Parse(itemValue);// TryParse? BigDecimal == Decimal? not sure these are equivolent, but it may be close enough for our purposes.
            }
            else
            {
                conceptCd = GenerateConceptCode(study.OID, studyEventData.StudyEventOID, formData.FormOID, itemData.ItemOID, null);

                ClinicalDataInfo.ValTypeCd = Constants.VALUE_TYPE_TEXT;
                ClinicalDataInfo.TvalChar = itemValue;
                ClinicalDataInfo.NvalNum = null;
            }

            ClinicalDataInfo.ConceptCd = conceptCd;
            ClinicalDataInfo.EncounterNum = encounterNum;
            ClinicalDataInfo.PatientNum = subjectData.SubjectKey;
            ClinicalDataInfo.UpdateDate = CurrentDate;
            ClinicalDataInfo.DownloadDate = CurrentDate;
            ClinicalDataInfo.ImportDate = CurrentDate;
            ClinicalDataInfo.StartDate = CurrentDate;
            ClinicalDataInfo.EndDate = CurrentDate;

            //log.debug("Inserting clinical data: " + clinicalDataInfo);
            Debug.WriteLine("Inserting clinical data: " + ClinicalDataInfo);

            // save observation
            // into i2b2

            try
            {
                //log.info("clinicalDataInfo: " + clinicalDataInfo);
                Debug.WriteLine("clinicalDataInfo: " + ClinicalDataInfo);
                ClinicalDataDao.InsertObservation(ClinicalDataInfo);
            }
            catch (Exception ex)//TODO: Entity framework exception (was SQLException)
            {
                var exError = "Error inserting observation_fact record."
                            + " study: " + study.OID
                            + " item: " + itemData.ItemOID;
                //log.error(exError, ex);
                Debug.WriteLine(exError);
            }
        }

        /// <summary>
        /// Set up i2b2 metadata level 1 (Study) info into STUDY
        /// </summary>
        /// <param name="study"></param>
        private void SaveStudy(ODMcomplexTypeDefinitionStudy study)
        {
            // Need to include source system in path to avoid conflicts between servers
            var studyKey = ODM.SourceSystem + ":" + study.OID;
            var studyPath = "\\STUDY\\" + studyKey + "\\";
            var studyToolTip = "STUDY\\" + studyKey;

            // set c_hlevel 1 data (Study)
            StudyInfo.Chlevel = Constants.C_HLEVEL_1;
            StudyInfo.Cfullname = studyPath;
            StudyInfo.Cname = study.GlobalVariables.StudyName.Value;
            StudyInfo.CsynonmCd = Constants.C_SYNONYM_CD;
            StudyInfo.CvisualAttributes = Constants.C_VISUALATTRIBUTES_FOLDER;
            StudyInfo.CfactTableColumn = Constants.C_FACTTABLECOLUMN;
            StudyInfo.Ctablename = Constants.C_TABLENAME;
            StudyInfo.Ccolumnname = Constants.C_COLUMNNAME;
            StudyInfo.CcolumnDatatype = Constants.C_COLUMNDATATYPE;
            StudyInfo.Coperator = Constants.C_OPERATOR;
            StudyInfo.SourceSystemCd = ODM.SourceSystem;
            StudyInfo.UpdateDate = CurrentDate;
            StudyInfo.DownloadDate = CurrentDate;
            StudyInfo.ImportDate = CurrentDate;
            StudyInfo.Cdimcode = studyPath;
            StudyInfo.Ctooltip = studyToolTip;

            LogStudyInfo();

            // insert level 1 data
            StudyDao.InsertMetadata(StudyInfo);

            // save child events
            var version = study.MetaDataVersion.First();//FirstOrDefault()?
            if (version.Protocol.StudyEventRef != null)
            {
                foreach (var studyEventRef in version.Protocol.StudyEventRef)
                {
                    var studyEventDef = Utilities.GetStudyEvent(study, studyEventRef.StudyEventOID);

                    SaveEvent(study, studyEventDef, studyPath, studyToolTip);
                }
            }
        }

        #endregion Private Methods
    }
}
