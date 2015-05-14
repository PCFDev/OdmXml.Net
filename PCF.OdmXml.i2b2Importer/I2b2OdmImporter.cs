using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using PCF.OdmXml.i2b2Importer.DTO;
using PCF.OdmXml.i2b2Importer.Helpers;
using PCF.OdmXml.i2b2Importer.Interfaces;

//License?
namespace PCF.OdmXml.i2b2Importer
{
    /// <summary>
    /// An ODMXML importer for i2b2
    /// </summary>
    public class I2b2OdmImporter : IOdmImporter
    {
        /// <summary>
        /// Takes a populated ODM model and inserts the data into an i2b2 database
        /// </summary>
        /// <param name="odm">Fully populated ODMXML model</param>
        /// <returns></returns>
        public async Task ImportAsync(ODM odm, IDictionary<string, string> settings)
        {
            //TODO implment this funcion based on the code in the harvard implementation
            //here is a link to a copy of the java file: https://github.com/CTMM-TraIT/trait_odm_to_i2b2/blob/master/src/main/java/com/recomdata/i2b2/I2B2ODMStudyHandler.java
            throw new NotImplementedException();
        }

        /**
         * The log for this class.
         */
        //private static final Logger log = LoggerFactory.getLogger(I2B2ODMStudyHandler.class);

        // initialize ODM object
        private ODM odm = null;

        private I2B2StudyInfo studyInfo = new I2B2StudyInfo();
        private I2B2ClinicalDataInfo clinicalDataInfo = new I2B2ClinicalDataInfo();

        private IStudyDao studyDao = null;//TODO: Entity framework
        private IClinicalDataDao clinicalDataDao = null;//TODO: Entity framework

        private DateTime currentDate;// = null;
        private HashAlgorithm messageDigest = null;
        private StringBuilder conceptBuffer = new StringBuilder("STUDY|");
        private MetaDataXML mdx = new MetaDataXML();

        /**
         * Constructor to set ODM object
         *
         * @param odm the entire ODM tree.
         * @throws SQLException
         * @throws NoSuchAlgorithmException
         */

        public I2b2OdmImporter(ODM odm)
        {
            this.odm = odm;

            studyDao = new StudyDao();//TODO: Entity framework
            clinicalDataDao = new ClinicalDataDao();//TODO: Entity framework

            studyInfo.SourceSystemCd = odm.SourceSystem;
            clinicalDataInfo.SourcesystemCd = odm.SourceSystem;

            currentDate = DateTime.UtcNow;//Assuming we want UTC date for now.
            messageDigest = MD5.Create();
        }

        /**
         * set up i2b2 metadata level 1 (Study) info into STUDY
         *
         * @throws SQLException
         * @throws JAXBException
         */

        private void SaveStudy(ODMcomplexTypeDefinitionStudy study)
        {
            // Need to include source system in path to avoid conflicts between servers
            var studyKey = odm.SourceSystem + ":" + study.OID;
            var studyPath = "\\" + "STUDY" + "\\" + studyKey + "\\";
            var studyToolTip = "STUDY" + "\\" + studyKey;

            // set c_hlevel 1 data (Study)
            studyInfo.Chlevel = Constants.C_HLEVEL_1;
            studyInfo.Cfullname = studyPath;
            studyInfo.Cname = study.GlobalVariables.StudyName.Value;
            studyInfo.CsynonmCd = Constants.C_SYNONYM_CD;
            studyInfo.CvisualAttributes = Constants.C_VISUALATTRIBUTES_FOLDER;
            studyInfo.CfactTableColumn = Constants.C_FACTTABLECOLUMN;
            studyInfo.Ctablename = Constants.C_TABLENAME;
            studyInfo.Ccolumnname = Constants.C_COLUMNNAME;
            studyInfo.CcolumnDatatype = Constants.C_COLUMNDATATYPE;
            studyInfo.Coperator = Constants.C_OPERATOR;
            studyInfo.SourceSystemCd = odm.SourceSystem;
            studyInfo.UpdateDate = currentDate;
            studyInfo.DownloadDate = currentDate;
            studyInfo.ImportDate = currentDate;
            studyInfo.Cdimcode = studyPath;
            studyInfo.Ctooltip = studyToolTip;

            LogStudyInfo();

            // insert level 1 data
            studyDao.InsertMetadata(studyInfo);

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

        /**
         * set up i2b2 metadata level 2 (Event) info into STUDY
         *
         * @throws SQLException
         * @throws JAXBException
         */

        private void SaveEvent(ODMcomplexTypeDefinitionStudy study,
                               ODMcomplexTypeDefinitionStudyEventDef studyEventDef,
                               string studyPath,
                               string studyToolTip)
        {
            var eventPath = studyPath + studyEventDef.OID + "\\";
            var eventToolTip = studyToolTip + "\\" + studyEventDef.OID;

            // set c_hlevel 2 data (StudyEvent)
            studyInfo.Chlevel = Constants.C_HLEVEL_2;
            studyInfo.Cfullname = eventPath;
            studyInfo.Cname = studyEventDef.Name;
            studyInfo.Cdimcode = eventPath;
            studyInfo.Ctooltip = eventToolTip;
            studyInfo.CvisualAttributes = Constants.C_VISUALATTRIBUTES_FOLDER;

            LogStudyInfo();

            // insert level 2 data
            studyDao.InsertMetadata(studyInfo);

            foreach (var formRef in studyEventDef.FormRef)
            {
                var formDef = Utilities.GetForm(study, formRef.FormOID);

                SaveForm(study, studyEventDef, formDef, eventPath, eventToolTip);
            }
        }

        /**
         * set up i2b2 metadata level 3 (Form) info into STUDY
         *
         * @throws SQLException
         * @throws JAXBException
         */

        private void SaveForm(ODMcomplexTypeDefinitionStudy study,
                              ODMcomplexTypeDefinitionStudyEventDef studyEventDef,
                              ODMcomplexTypeDefinitionFormDef formDef,
                              string eventPath,
                              string eventToolTip)
        {
            var formPath = eventPath + formDef.OID + "\\";
            var formToolTip = eventToolTip + "\\" + formDef.OID;

            // set c_hlevel 3 data (Form)
            studyInfo.Chlevel = Constants.C_HLEVEL_3;
            studyInfo.Cfullname = formPath;
            studyInfo.Cname = GetTranslatedDescription(formDef.Description, "en", formDef.Name);
            studyInfo.Cdimcode = formPath;
            studyInfo.Ctooltip = formToolTip;
            studyInfo.CvisualAttributes = Constants.C_VISUALATTRIBUTES_FOLDER;

            LogStudyInfo();

            // insert level 3 data
            studyDao.InsertMetadata(studyInfo);

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

        /**
         * set up i2b2 metadata level 4 (Item) info into STUDY and CONCEPT_DIMENSION
         *
         * @throws SQLException
         * @throws JAXBException
         */

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
            studyInfo.Chlevel = Constants.C_HLEVEL_4;
            studyInfo.Cfullname = itemPath;
            studyInfo.Cname = GetTranslatedDescription(itemDef.Description, "en", itemDef.Name);
            studyInfo.Cbasecode = GenerateConceptCode(study.OID, studyEventDef.OID, formDef.OID, itemDef.OID, null);
            studyInfo.Cdimcode = itemPath;
            studyInfo.Ctooltip = itemToolTip;
            studyInfo.Cmetadataxml = CreateMetadataXml(study, itemDef);

            // It is a leaf node
            studyInfo.CvisualAttributes = itemDef.CodeListRef == null
                                        ? Constants.C_VISUALATTRIBUTES_LEAF
                                        : Constants.C_VISUALATTRIBUTES_FOLDER;
            LogStudyInfo();

            // insert level 4 data
            studyDao.InsertMetadata(studyInfo);

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

        /**
         * @throws JAXBException
         */

        private string CreateMetadataXml(ODMcomplexTypeDefinitionStudy study, ODMcomplexTypeDefinitionItemDef itemDef)
        {
            var metadataXml = default(string);

            switch (itemDef.DataType)
            {
                case DataType.integer:
                    metadataXml = mdx.GetIntegerMetadataXML(itemDef.OID, itemDef.Name);
                    break;
                case DataType.@float:
                case DataType.@double:
                    metadataXml = mdx.GetFloatMetadataXML(itemDef.OID, itemDef.Name);
                    break;
                case DataType.text:
                case DataType.@string:
                    if (itemDef.CodeListRef == null)
                        metadataXml = mdx.GetStringMetadataXML(itemDef.OID, itemDef.Name);
                    else
                    {
                        var codeList = Utilities.GetCodeList(study, itemDef.CodeListRef.CodeListOID);
                        var codeListValues = Utilities.GetCodeListValues(codeList, "en");
                        metadataXml = mdx.GetEnumMetadataXML(itemDef.OID, itemDef.Name, codeListValues);
                    }
                    break;
                case DataType.boolean:
                    break;
                case DataType.date:
                case DataType.time:
                case DataType.datetime:
                    metadataXml = mdx.GetStringMetadataXML(itemDef.OID, itemDef.Name);
                    break;
                default:
                    break;
            }

            return metadataXml;
        }

        /**
         * set up i2b2 metadata level 5 (TranslatedText) info into STUDY
         *
         * @throws SQLException
         */

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
            studyInfo.Chlevel = Constants.C_HLEVEL_5;
            studyInfo.Cfullname = codeListItemPath;
            studyInfo.Cname = GetTranslatedDescription(itemDef.Description, "en", itemDef.Name) + ": " + value;
            studyInfo.Cbasecode = GenerateConceptCode(study.OID, studyEventDef.OID, formDef.OID, itemDef.OID, codedValue);
            studyInfo.Cdimcode = codeListItemPath;
            studyInfo.Ctooltip = codeListItemToolTip;
            studyInfo.Cmetadataxml = null;
            studyInfo.CvisualAttributes = Constants.C_VISUALATTRIBUTES_LEAF;

            LogStudyInfo();

            studyDao.InsertMetadata(studyInfo);
        }

        /**
         * method to parse ODM and save data into i2b2
         *
         * @throws SQLException
         * @throws JAXBException
         * @throws ParseException
         */

        public void ProcessODM()
        {
            //log.info("Start to parse ODM xml and save to i2b2");
            Debug.WriteLine("Start to parse ODM xml and save to i2b2");

            // build the call
            ProcessODMStudy();
            ProcessODMClinicalData();
        }

        /*
         * This method takes ODM XML io.File obj as input and parsed by JAXB API and
         * then traverses through ODM tree object and save data into i2b2 metadata
         * database in i2b2 data format.
         *
         * @throws SQLException
         * @throws JAXBException
         */

        public void ProcessODMStudy()
        {
            /*
             * Need to traverse through the study definition to: 1) Lookup all
             * definition values in tree nodes. 2) Set node values into i2b2 bean
             * info and ready for populating into i2b2 database.
             */
            foreach (var study in odm.Study)
            {
                //log.info("Processing study metadata for study " + study.getGlobalVariables().getStudyName().getValue() + "(OID " + study.OID + ")");
                Debug.WriteLine("Processing study metadata for study " + study.GlobalVariables.StudyName.Value + "(OID " + study.OID + ")");
                //log.info("Deleting old study metadata and data");
                Debug.WriteLine("Deleting old study metadata and data");

                studyDao.PreSetupI2B2Study(study.OID, odm.SourceSystem);

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
            studyDao.ExecuteBatch();
        }

        /*
         * This method takes ODM XML io.File obj as input and parsed by JAXB API and
         * then traversal through ODM tree object and save clinical data into i2b2
         * demo database ini2b2 data format. Keep method public in case of only want
         * to parse demodata.
         *
         * @throws JAXBException
         * @throws ParseException
         * @throws SQLException
         */

        public void ProcessODMClinicalData()
        {
            //log.info("Parse and save ODM clinical data into i2b2...");
            Debug.WriteLine("Parse and save ODM clinical data into i2b2...");

            // traverse through the clinical data to:
            // 1) Lookup the concept path from odm study metadata.
            // 2) Set patient and clinical information into observation fact.
            if (odm.ClinicalData == null || odm.ClinicalData.Count == 0)
            {
                //log.info("ODM does not contain clinical data");
                Debug.WriteLine("ODM does not contain clinical data");
                return;
            }

            foreach (var study in odm.Study)
            {
                clinicalDataDao.CleanupClinicalData(study.OID, odm.SourceSystem);
            }

            foreach (var clinicalData in odm.ClinicalData)
            {
                if (clinicalData.SubjectData == null)
                    continue;

                //log.info("Save Clinical data for study OID " + clinicalData.getStudyOID() + " into i2b2...");
                Debug.WriteLine("Save Clinical data for study OID " + clinicalData.StudyOID + " into i2b2...");
                var timer = Stopwatch.StartNew();

                var study = Utilities.GetStudy(odm, clinicalData.StudyOID);
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
                clinicalDataDao.ExecuteBatch();

                timer.Stop();
                //log.info("Completed Clinical data to i2b2 for study OID " + clinicalData.getStudyOID() + " in " + (endTime - startTime) + " ms");
                Debug.WriteLine("Completed Clinical data to i2b2 for study OID " + clinicalData.StudyOID + " in " + timer.ElapsedMilliseconds + " ms");
            }
        }

        private void LogStudyInfo()
        {
            //if (log.isDebugEnabled()) {
            //    log.debug("Inserting study metadata record: " + studyInfo);
            //}
            Debug.WriteLine("Inserting study metadata record: " + studyInfo);
        }

        /**
         * @throws JAXBException
         * @throws ParseException
         * @throws SQLException
         */

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
                clinicalDataInfo.ValTypeCd = "T";//TODO: Magic
                clinicalDataInfo.NvalNum = null;

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

                    clinicalDataInfo.TvalChar = Utilities.GetTranslatedValue(codeListItem, "en");
                }
            }
            else if (Utilities.IsNumeric(item.DataType))
            {
                conceptCd = GenerateConceptCode(study.OID, studyEventData.StudyEventOID, formData.FormOID, itemData.ItemOID, null);

                clinicalDataInfo.ValTypeCd = "N";//TODO: Magic
                clinicalDataInfo.TvalChar = "E";//TODO: Magic
                clinicalDataInfo.NvalNum = String.IsNullOrWhiteSpace(itemValue) ? default(decimal?) : Decimal.Parse(itemValue);// TryParse? BigDecimal == Decimal? not sure these are equivolent, but it may be close enough for our purposes.
            }
            else
            {
                conceptCd = GenerateConceptCode(study.OID, studyEventData.StudyEventOID, formData.FormOID, itemData.ItemOID, null);

                clinicalDataInfo.ValTypeCd = "T";//TODO: Magic
                clinicalDataInfo.TvalChar = itemValue;
                clinicalDataInfo.NvalNum = null;
            }

            clinicalDataInfo.ConceptCd = conceptCd;
            clinicalDataInfo.EncounterNum = encounterNum;
            clinicalDataInfo.PatientNum = subjectData.SubjectKey;
            clinicalDataInfo.UpdateDate = currentDate;
            clinicalDataInfo.DownloadDate = currentDate;
            clinicalDataInfo.ImportDate = currentDate;
            clinicalDataInfo.StartDate = currentDate;
            clinicalDataInfo.EndDate = currentDate;

            //log.debug("Inserting clinical data: " + clinicalDataInfo);
            Debug.WriteLine("Inserting clinical data: " + clinicalDataInfo);

            // save observation
            // into i2b2

            try
            {
                //log.info("clinicalDataInfo: " + clinicalDataInfo);
                Debug.WriteLine("clinicalDataInfo: " + clinicalDataInfo);
                clinicalDataDao.InsertObservation(clinicalDataInfo);
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
            conceptBuffer.Length = 6;
            conceptBuffer.Append(studyOID).Append("|");

            //I don't think we want quite use StringBuilder here becuase the pipes are byte cast chars, not Unicode literals. md5("\x00\x7C") vs md5("\x7C")
            var message = new ByteArrayBulder()
                .Append(Encoding.UTF8.GetBytes(odm.SourceSystem))
                .Append((byte)'|')
                .Append(Encoding.UTF8.GetBytes(studyEventOID))
                .Append((byte)'|')
                .Append(Encoding.UTF8.GetBytes(formOID))
                .Append((byte)'|')
                .Append(Encoding.UTF8.GetBytes(itemOID));

            if (value != null)
                message.Append((byte)'|').Append(Encoding.UTF8.GetBytes(value));

            var digest = messageDigest.ComputeHash(message.GetBytes());
            foreach (var digestByte in digest)
            {
                //Case sensitive?
                conceptBuffer.Append(digestByte.ToString("X2"));//& 0xFF returns exactly the same byte, not sure why they did that.
            }

            var conceptCode = conceptBuffer.ToString();
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
    }
}
