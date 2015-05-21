using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PCF.OdmXml.i2b2Importer.DB;
using PCF.OdmXml.i2b2Importer.DTO;
using PCF.OdmXml.i2b2Importer.Helpers;

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

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Constructor to set ODM object
        /// </summary>
        /// <param name="odm">The entire ODM tree.</param>
        public I2b2OdmProcessor(ODM odm, IDictionary<string, string> settings)//settings?
        {
            ODM = odm;
            ODM.SourceSystem = ODM.SourceSystem ?? String.Empty;
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
            clinicalDataDao.CleanupClinicalData(ODM.Study, ODM.SourceSystem);

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
                var clinicalDatas = new List<I2B2ClinicalDataInfo>();

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
                                    if (itemData.Value == null)
                                        continue;

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
                                            conceptCd = Utilities.GenerateConceptCode(ODM.SourceSystem, study.OID, studyEventData.StudyEventOID, formData.FormOID, itemData.ItemOID, itemValue);

                                            clinicalDataInfo.TvalChar = Utilities.GetTranslatedValue(codeListItem, "en");
                                        }
                                    }
                                    else if (Utilities.IsNumeric(item.DataType))
                                    {
                                        conceptCd = Utilities.GenerateConceptCode(ODM.SourceSystem, study.OID, studyEventData.StudyEventOID, formData.FormOID, itemData.ItemOID, null);

                                        clinicalDataInfo.ValTypeCd = Constants.VALUE_TYPE_NUMBER;
                                        clinicalDataInfo.TvalChar = "E";//TODO: Magic
                                        clinicalDataInfo.NvalNum = String.IsNullOrWhiteSpace(itemValue) ? default(decimal?) : Decimal.Parse(itemValue);// TryParse? BigDecimal == Decimal? not sure these are equivolent, but it may be close enough for our purposes.
                                    }
                                    else
                                    {
                                        conceptCd = Utilities.GenerateConceptCode(ODM.SourceSystem, study.OID, studyEventData.StudyEventOID, formData.FormOID, itemData.ItemOID, null);

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

                                    clinicalDatas.Add(clinicalDataInfo);
                                }
                            }
                        }
                    }
                }

                clinicalDataDao.InsertObservations(clinicalDatas);

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

                studyDao.PreSetupI2B2Study(study.OID, ODM.SourceSystem);//Batch? We would need to step through twice.

                Debug.WriteLine("Inserting study metadata into i2b2");
                var timer = Stopwatch.StartNew();

                var studyInfos = GetStudies(study);
                studyDao.InsertMetadata(studyInfos);

                timer.Stop();
                Debug.WriteLine("Completed loading study metadata into i2b2 in " + timer.ElapsedMilliseconds + " ms");
            }
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
        private IEnumerable<I2B2StudyInfo> GetCodeListItems(ODMcomplexTypeDefinitionStudy study,
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
            var studyInfo = new I2B2StudyInfo(Constants.C_HLEVEL_5,
                                              codeListItemPath,
                                              Utilities.GetTranslatedDescription(itemDef.Description, "en", itemDef.Name) + ": " + value,
                                              codeListItemPath,
                                              codeListItemToolTip,
                                              Constants.C_VISUALATTRIBUTES_LEAF)
                                              { SourceSystemCd = ODM.SourceSystem };

            studyInfo.Cbasecode = Utilities.GenerateConceptCode(ODM.SourceSystem, study.OID, studyEventDef.OID, formDef.OID, itemDef.OID, codedValue);
            studyInfo.Cmetadataxml = null;

            Debug.WriteLine("Inserting study metadata record: " + studyInfo);

            yield return studyInfo;
        }

        /// <summary>
        /// Set up i2b2 metadata level 2 (Event) info into STUDY
        /// </summary>
        /// <param name="study"></param>
        /// <param name="studyEventDef"></param>
        /// <param name="studyPath"></param>
        /// <param name="studyToolTip"></param>
        private IEnumerable<I2B2StudyInfo> GetEvents(ODMcomplexTypeDefinitionStudy study,
                                                     ODMcomplexTypeDefinitionStudyEventDef studyEventDef,
                                                     string studyPath,
                                                     string studyToolTip)
        {
            var eventPath = studyPath + studyEventDef.OID + "\\";
            var eventToolTip = studyToolTip + "\\" + studyEventDef.OID;

            // set c_hlevel 2 data (StudyEvent)
            var studyInfo = new I2B2StudyInfo(Constants.C_HLEVEL_2,
                                              eventPath,
                                              studyEventDef.Name,
                                              eventPath,
                                              eventToolTip,
                                              Constants.C_VISUALATTRIBUTES_FOLDER)
                                              { SourceSystemCd = ODM.SourceSystem };

            Debug.WriteLine("Inserting study metadata record: " + studyInfo);

            // insert level 2 data
            yield return studyInfo;

            foreach (var formRef in studyEventDef.FormRef)
            {
                var formDef = Utilities.GetForm(study, formRef.FormOID);
                var forms = GetForms(study, studyEventDef, formDef, eventPath, eventToolTip);

                foreach (var form in forms)
                {
                    yield return form;
                }
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
        private IEnumerable<I2B2StudyInfo> GetForms(ODMcomplexTypeDefinitionStudy study,
                                                    ODMcomplexTypeDefinitionStudyEventDef studyEventDef,
                                                    ODMcomplexTypeDefinitionFormDef formDef,
                                                    string eventPath,
                                                    string eventToolTip)
        {
            var formPath = eventPath + formDef.OID + "\\";
            var formToolTip = eventToolTip + "\\" + formDef.OID;

            // set c_hlevel 3 data (Form)
            var studyInfo = new I2B2StudyInfo(Constants.C_HLEVEL_3,
                                              formPath,
                                              Utilities.GetTranslatedDescription(formDef.Description, "en", formDef.Name),
                                              formPath,
                                              formToolTip,
                                              Constants.C_VISUALATTRIBUTES_FOLDER)
                                              { SourceSystemCd = ODM.SourceSystem };

            Debug.WriteLine("Inserting study metadata record: " + studyInfo);

            // insert level 3 data
            yield return studyInfo;

            foreach (var itemGroupRef in formDef.ItemGroupRef)
            {
                var itemGroupDef = Utilities.GetItemGroup(study, itemGroupRef.ItemGroupOID);
                if (itemGroupDef.ItemRef != null)
                {
                    foreach (var itemRef in itemGroupDef.ItemRef)
                    {
                        var itemDef = Utilities.GetItem(study, itemRef.ItemOID);
                        var items = GetItems(study, studyEventDef, formDef, itemDef, formPath, formToolTip);

                        foreach (var item in items)
                        {
                            yield return item;
                        }
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
        private IEnumerable<I2B2StudyInfo> GetItems(ODMcomplexTypeDefinitionStudy study,
                                                    ODMcomplexTypeDefinitionStudyEventDef studyEventDef,
                                                    ODMcomplexTypeDefinitionFormDef formDef,
                                                    ODMcomplexTypeDefinitionItemDef itemDef,
                                                    string formPath,
                                                    string formToolTip)
        {
            var itemPath = formPath + itemDef.OID + "\\";
            var itemToolTip = formToolTip + "\\" + itemDef.OID;
            // It is a leaf node
            var visualAttributes = itemDef.CodeListRef == null
                                 ? Constants.C_VISUALATTRIBUTES_LEAF
                                 : Constants.C_VISUALATTRIBUTES_FOLDER;

            // set c_hlevel 4 data (Items)
            var studyInfo = new I2B2StudyInfo(Constants.C_HLEVEL_4,
                                              itemPath,
                                              Utilities.GetTranslatedDescription(itemDef.Description, "en", itemDef.Name),
                                              itemPath,
                                              itemToolTip,
                                              visualAttributes)
                                              { SourceSystemCd = ODM.SourceSystem };

            studyInfo.Cbasecode = Utilities.GenerateConceptCode(ODM.SourceSystem, study.OID, studyEventDef.OID, formDef.OID, itemDef.OID, null);
            studyInfo.Cmetadataxml = MetaDataXML.CreateMetadataXml(study, itemDef);

            Debug.WriteLine("Inserting study metadata record: " + studyInfo);

            // insert level 4 data
            yield return studyInfo;

            if (itemDef.CodeListRef != null)
            {
                var codeList = Utilities.GetCodeList(study, itemDef.CodeListRef.CodeListOID);
                if (codeList != null)
                {
                    foreach (var codeListItem in codeList.Items.Where(_ => _ is ODMcomplexTypeDefinitionCodeListItem).Select(_ => _ as ODMcomplexTypeDefinitionCodeListItem).ToList())//getCodeListItem()
                    {
                        var codeListItems = GetCodeListItems(study, studyEventDef, formDef, itemDef, codeListItem, itemPath, itemToolTip);

                        foreach (var item in codeListItems)
                        {
                            yield return item;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Set up i2b2 metadata level 1 (Study) info into STUDY
        /// </summary>
        /// <param name="study"></param>
        private IEnumerable<I2B2StudyInfo> GetStudies(ODMcomplexTypeDefinitionStudy study)
        {
            // Need to include source system in path to avoid conflicts between servers
            var studyKey = ODM.SourceSystem + ":" + study.OID;
            var studyPath = "\\STUDY\\" + studyKey + "\\";
            var studyToolTip = "STUDY\\" + studyKey;

            // set c_hlevel 1 data (Study)
            var studyInfo = new I2B2StudyInfo(Constants.C_HLEVEL_1,
                                              studyPath,
                                              study.GlobalVariables.StudyName.Value,
                                              studyPath,
                                              studyToolTip,
                                              Constants.C_VISUALATTRIBUTES_FOLDER)
                                              { SourceSystemCd = ODM.SourceSystem };

            Debug.WriteLine("Inserting study metadata record: " + studyInfo);

            // insert level 1 data
            yield return studyInfo;
            //yield return studyInfo);

            // save child events
            var version = study.MetaDataVersion.First();//FirstOrDefault()?
            if (version.Protocol.StudyEventRef != null)
            {
                foreach (var studyEventRef in version.Protocol.StudyEventRef)
                {
                    var studyEventDef = Utilities.GetStudyEvent(study, studyEventRef.StudyEventOID);
                    var events = GetEvents(study, studyEventDef, studyPath, studyToolTip);

                    foreach (var @event in events)
                    {
                        yield return @event;
                    }
                }
            }
        }

        #endregion Private Methods
    }
}
