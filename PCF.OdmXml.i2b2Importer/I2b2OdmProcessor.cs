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

            StudyInfo = new I2B2StudyInfo { SourceSystemCd = odm.SourceSystem };
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// method to parse ODM and save data into i2b2
        /// </summary>
        public void ProcessODM()
        {
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
            Debug.WriteLine("Parse and save ODM clinical data into i2b2...");

            // traverse through the clinical data to:
            // 1) Lookup the concept path from odm study metadata.
            // 2) Set patient and clinical information into observation fact.
            if (ODM.ClinicalData == null || ODM.ClinicalData.Count == 0)
            {
                Debug.WriteLine("ODM does not contain clinical data");
                return;
            }

            var clinicalDataDao = new ClinicalDataDao();

            foreach (var study in ODM.Study)
            {
                clinicalDataDao.CleanupClinicalData(study.OID, ODM.SourceSystem);
            }

            foreach (var clinicalData in ODM.ClinicalData)
            {
                if (clinicalData.SubjectData == null)
                    continue;

                Debug.WriteLine("Save Clinical data for study OID " + clinicalData.StudyOID + " into i2b2...");
                var timer = Stopwatch.StartNew();

                var study = Utilities.GetStudy(ODM, clinicalData.StudyOID);
                if (study == null)
                {
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
                                        SaveItemData(ref clinicalDataDao, study, subjectData, studyEventData, formData, itemData, encounterNum);
                                }
                            }
                        }
                    }
                }

                /*
                 * Flush any remaining batched up observations;
                 */
                clinicalDataDao.ExecuteBatch();

                timer.Stop();
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
            var studyDao = new StudyDao();

            /*
             * Need to traverse through the study definition to: 1) Lookup all
             * definition values in tree nodes. 2) Set node values into i2b2 bean
             * info and ready for populating into i2b2 database.
             */
            foreach (var study in ODM.Study)
            {
                Debug.WriteLine("Processing study metadata for study " + study.GlobalVariables.StudyName.Value + "(OID " + study.OID + ")");
                Debug.WriteLine("Deleting old study metadata and data");

                studyDao.PreSetupI2B2Study(study.OID, ODM.SourceSystem);

                Debug.WriteLine("Inserting study metadata into i2b2");
                var timer = Stopwatch.StartNew();

                SaveStudy(ref studyDao, study);

                timer.Stop();
                Debug.WriteLine("Completed loading study metadata into i2b2 in " + timer.ElapsedMilliseconds + " ms");
            }

            /*
             * Flush any remaining batched up records.
             */
            studyDao.ExecuteBatch();
        }

        #endregion Public Methods

        #region Private Methods

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
        private void SaveCodeListItem(ref StudyDao studyDao, 
                                      ODMcomplexTypeDefinitionStudy study,
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
            StudyInfo.Cname = Utilities.GetTranslatedDescription(itemDef.Description, "en", itemDef.Name) + ": " + value;
            StudyInfo.Cbasecode = Utilities.GenerateConceptCode(ODM.SourceSystem ?? String.Empty, study.OID, studyEventDef.OID, formDef.OID, itemDef.OID, codedValue);
            StudyInfo.Cdimcode = codeListItemPath;
            StudyInfo.Ctooltip = codeListItemToolTip;
            StudyInfo.Cmetadataxml = null;
            StudyInfo.CvisualAttributes = Constants.C_VISUALATTRIBUTES_LEAF;

            Debug.WriteLine("Inserting study metadata record: " + StudyInfo);

            studyDao.InsertMetadata(StudyInfo);
        }

        /// <summary>
        /// Set up i2b2 metadata level 2 (Event) info into STUDY
        /// </summary>
        /// <param name="study"></param>
        /// <param name="studyEventDef"></param>
        /// <param name="studyPath"></param>
        /// <param name="studyToolTip"></param>
        private void SaveEvent(ref StudyDao studyDao,
                               ODMcomplexTypeDefinitionStudy study,
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

            Debug.WriteLine("Inserting study metadata record: " + StudyInfo);

            // insert level 2 data
            studyDao.InsertMetadata(StudyInfo);

            foreach (var formRef in studyEventDef.FormRef)
            {
                var formDef = Utilities.GetForm(study, formRef.FormOID);

                SaveForm(ref studyDao, study, studyEventDef, formDef, eventPath, eventToolTip);
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
        private void SaveForm(ref StudyDao studyDao,
                              ODMcomplexTypeDefinitionStudy study,
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
            StudyInfo.Cname = Utilities.GetTranslatedDescription(formDef.Description, "en", formDef.Name);
            StudyInfo.Cdimcode = formPath;
            StudyInfo.Ctooltip = formToolTip;
            StudyInfo.CvisualAttributes = Constants.C_VISUALATTRIBUTES_FOLDER;

            Debug.WriteLine("Inserting study metadata record: " + StudyInfo);

            // insert level 3 data
            studyDao.InsertMetadata(StudyInfo);

            foreach (var itemGroupRef in formDef.ItemGroupRef)
            {
                var itemGroupDef = Utilities.GetItemGroup(study, itemGroupRef.ItemGroupOID);
                if (itemGroupDef.ItemRef != null)
                {
                    foreach (var itemRef in itemGroupDef.ItemRef)
                    {
                        var itemDef = Utilities.GetItem(study, itemRef.ItemOID);

                        SaveItem(ref studyDao, study, studyEventDef, formDef, itemDef, formPath, formToolTip);
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
        private void SaveItem(ref StudyDao studyDao,
                              ODMcomplexTypeDefinitionStudy study,
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
            StudyInfo.Cname = Utilities.GetTranslatedDescription(itemDef.Description, "en", itemDef.Name);
            StudyInfo.Cbasecode = Utilities.GenerateConceptCode(ODM.SourceSystem ?? String.Empty, study.OID, studyEventDef.OID, formDef.OID, itemDef.OID, null);
            StudyInfo.Cdimcode = itemPath;
            StudyInfo.Ctooltip = itemToolTip;
            StudyInfo.Cmetadataxml = MetaDataXML.CreateMetadataXml(study, itemDef);

            // It is a leaf node
            StudyInfo.CvisualAttributes = itemDef.CodeListRef == null
                                        ? Constants.C_VISUALATTRIBUTES_LEAF
                                        : Constants.C_VISUALATTRIBUTES_FOLDER;
            Debug.WriteLine("Inserting study metadata record: " + StudyInfo);

            // insert level 4 data
            studyDao.InsertMetadata(StudyInfo);

            if (itemDef.CodeListRef != null)
            {
                var codeList = Utilities.GetCodeList(study, itemDef.CodeListRef.CodeListOID);
                if (codeList != null)
                {
                    foreach (var codeListItem in codeList.Items.Where(_ => _ is ODMcomplexTypeDefinitionCodeListItem).Select(_ => _ as ODMcomplexTypeDefinitionCodeListItem).ToList())//getCodeListItem()
                    {
                        // save
                        // level 5
                        SaveCodeListItem(ref studyDao, study, studyEventDef, formDef, itemDef, codeListItem, itemPath, itemToolTip);
                    }
                }
            }
        }

        private void SaveItemData(ref ClinicalDataDao clinicalDataDao,
                                  ODMcomplexTypeDefinitionStudy study,
                                  ODMcomplexTypeDefinitionSubjectData subjectData,
                                  ODMcomplexTypeDefinitionStudyEventData studyEventData,
                                  ODMcomplexTypeDefinitionFormData formData,
                                  ODMcomplexTypeDefinitionItemData itemData,
                                  int encounterNum)
        {
            var itemValue = itemData.Value;
            var item = Utilities.GetItem(study, itemData.ItemOID);
            var conceptCd = default(string);

            var clinicalDataInfo = new I2B2ClinicalDataInfo { SourcesystemCd = ODM.SourceSystem };

            if (item.CodeListRef != null)
            {
                clinicalDataInfo.ValTypeCd = Constants.VALUE_TYPE_TEXT;
                clinicalDataInfo.NvalNum = null;

                var codeList = Utilities.GetCodeList(study, item.CodeListRef.CodeListOID);
                var codeListItem = Utilities.GetCodeListItem(codeList, itemValue);

                if (codeListItem == null)
                {
                    Debug.WriteLine("Code list item for coded value: " + itemValue + " not found in code list: " + codeList.OID);
                    return;
                }
                else
                {
                    /*
                     * Need to include the item value in the concept code, since there is a different code for each code list item.
                     */
                    conceptCd = Utilities.GenerateConceptCode(ODM.SourceSystem ?? String.Empty, study.OID, studyEventData.StudyEventOID, formData.FormOID, itemData.ItemOID, itemValue);

                    clinicalDataInfo.TvalChar = Utilities.GetTranslatedValue(codeListItem, "en");
                }
            }
            else if (Utilities.IsNumeric(item.DataType))
            {
                conceptCd = Utilities.GenerateConceptCode(ODM.SourceSystem ?? String.Empty, study.OID, studyEventData.StudyEventOID, formData.FormOID, itemData.ItemOID, null);

                clinicalDataInfo.ValTypeCd = Constants.VALUE_TYPE_NUMBER;
                clinicalDataInfo.TvalChar = "E";//TODO: Magic
                clinicalDataInfo.NvalNum = String.IsNullOrWhiteSpace(itemValue) ? default(decimal?) : Decimal.Parse(itemValue);// TryParse? BigDecimal == Decimal? not sure these are equivolent, but it may be close enough for our purposes.
            }
            else
            {
                conceptCd = Utilities.GenerateConceptCode(ODM.SourceSystem ?? String.Empty, study.OID, studyEventData.StudyEventOID, formData.FormOID, itemData.ItemOID, null);

                clinicalDataInfo.ValTypeCd = Constants.VALUE_TYPE_TEXT;
                clinicalDataInfo.TvalChar = itemValue;
                clinicalDataInfo.NvalNum = null;
            }

            clinicalDataInfo.ConceptCd = conceptCd;
            clinicalDataInfo.EncounterNum = encounterNum;
            clinicalDataInfo.PatientNum = subjectData.SubjectKey;
            clinicalDataInfo.UpdateDate = CurrentDate;
            clinicalDataInfo.DownloadDate = CurrentDate;
            clinicalDataInfo.ImportDate = CurrentDate;
            clinicalDataInfo.StartDate = CurrentDate;
            clinicalDataInfo.EndDate = CurrentDate;

            Debug.WriteLine("Inserting clinical data: " + clinicalDataInfo);

            // save observation
            // into i2b2

            try
            {
                Debug.WriteLine("clinicalDataInfo: " + clinicalDataInfo);
                clinicalDataDao.InsertObservation(clinicalDataInfo);
            }
            catch (Exception ex)//TODO: Entity framework exception (was SQLException)
            {
                var exError = "Error inserting observation_fact record."
                            + " study: " + study.OID
                            + " item: " + itemData.ItemOID;
                Debug.WriteLine(exError);
            }
        }

        /// <summary>
        /// Set up i2b2 metadata level 1 (Study) info into STUDY
        /// </summary>
        /// <param name="study"></param>
        private void SaveStudy(ref StudyDao studyDao, ODMcomplexTypeDefinitionStudy study)
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

            Debug.WriteLine("Inserting study metadata record: " + StudyInfo);

            // insert level 1 data
            studyDao.InsertMetadata(StudyInfo);

            // save child events
            var version = study.MetaDataVersion.First();//FirstOrDefault()?
            if (version.Protocol.StudyEventRef != null)
            {
                foreach (var studyEventRef in version.Protocol.StudyEventRef)
                {
                    var studyEventDef = Utilities.GetStudyEvent(study, studyEventRef.StudyEventOID);

                    SaveEvent(ref studyDao, study, studyEventDef, studyPath, studyToolTip);
                }
            }
        }

        #endregion Private Methods
    }
}
