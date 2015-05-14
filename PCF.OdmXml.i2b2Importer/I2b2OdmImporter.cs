using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

//License?
namespace PCF.OdmXml.i2b2Importer
{
    public static class IConstants
    {
        //From https://github.com/CTMM-TraIT/trait_odm_to_i2b2/blob/452a1950b94d3a779eb66aaf1ad7ef34976c628c/src/main/java/com/recomdata/i2b2/IConstants.java
        public const int C_HLEVEL_1 = 1;
        public const int C_HLEVEL_2 = 2;
        public const int C_HLEVEL_3 = 3;
        public const int C_HLEVEL_4 = 4;
        public const int C_HLEVEL_5 = 5;
        public const int C_HLEVEL_6 = 6;
        public const string C_SYNONYM_CD = "N";
        public const string C_VISUALATTRIBUTES_FOLDER = "FA";
        public const string C_VISUALATTRIBUTES_LEAF = "LA";
        public const string C_FACTTABLECOLUMN = "concept_cd";
        public const string C_TABLENAME = "concept_dimension";
        public const string C_COLUMNNAME = "concept_path";
        public const string C_COLUMNDATATYPE = "T";
        public const string C_OPERATOR = "LIKE";
    }

    //https://github.com/CTMM-TraIT/trait_odm_to_i2b2/blob/452a1950b94d3a779eb66aaf1ad7ef34976c628c/src/main/java/com/recomdata/i2b2/util/ODMUtil.java
    public static class ODMUtil
    {
        public static ODMcomplexTypeDefinitionStudy getStudy(ODM odm, String studyOID)
        {
            foreach (ODMcomplexTypeDefinitionStudy study in odm.Study)
            {
                if (study.OID.Equals(studyOID))
                {
                    return study;
                }
            }

            return null;
        }

        /**
         * Resolve StudyEventDef from StudyEventRef
         *
         * @throws JAXBException
         */

        public static ODMcomplexTypeDefinitionStudyEventDef getStudyEvent(ODMcomplexTypeDefinitionStudy study, String studyEventOID)
        {
            ODMcomplexTypeDefinitionMetaDataVersion version = study.MetaDataVersion.First();

            if (version.StudyEventDef != null && version.StudyEventDef.Count > 0)
            {
                foreach (ODMcomplexTypeDefinitionStudyEventDef studyEventDef in version.StudyEventDef)
                {
                    if (studyEventDef.OID.Equals(studyEventOID))
                    {
                        return studyEventDef;
                    }
                }
            }

            return null;
        }

        /**
         * Resolve FormDef from FormRef
         *
         * @throws JAXBException
         */

        public static ODMcomplexTypeDefinitionFormDef getForm(ODMcomplexTypeDefinitionStudy study, String formOID)
        {
            ODMcomplexTypeDefinitionMetaDataVersion version = study.MetaDataVersion.First();

            if ((version.FormDef != null) && (version.FormDef.Count > 0))
            {
                foreach (ODMcomplexTypeDefinitionFormDef formDef in version.FormDef)
                {
                    if (formDef.OID.Equals(formOID))
                    {
                        return formDef;
                    }
                }
            }

            return null;
        }

        /**
         * Resolve ItemGroupDef from ItemGroupRef
         *
         * @throws JAXBException
         */

        public static ODMcomplexTypeDefinitionItemGroupDef getItemGroup(ODMcomplexTypeDefinitionStudy study, String itemGroupOID)
        {
            ODMcomplexTypeDefinitionMetaDataVersion version = study.MetaDataVersion.First();

            if ((version.ItemGroupDef != null) && (version.ItemGroupDef.Count > 0))
            {
                foreach (ODMcomplexTypeDefinitionItemGroupDef itemGroupDef in version.ItemGroupDef)
                {
                    if (itemGroupDef.OID.Equals(itemGroupOID))
                    {
                        return itemGroupDef;
                    }
                }
            }

            return null;
        }

        /**
         * Resolve ItemDef from ItemRef
         *
         * @throws JAXBException
         */

        public static ODMcomplexTypeDefinitionItemDef getItem(ODMcomplexTypeDefinitionStudy study, String itemOID)
        {
            ODMcomplexTypeDefinitionMetaDataVersion version = study.MetaDataVersion.First();

            if ((version.ItemDef != null) && (version.ItemDef.Count > 0))
            {
                foreach (ODMcomplexTypeDefinitionItemDef itemDef in version.ItemDef)
                {
                    if (itemDef.OID.Equals(itemOID))
                    {
                        return itemDef;
                    }
                }
            }

            return null;
        }

        /**
         * Resolve CodListDef from CodeListRef
         *
         * @throws JAXBException
         */

        public static ODMcomplexTypeDefinitionCodeList getCodeList(ODMcomplexTypeDefinitionStudy study, String codeListOID)
        {
            ODMcomplexTypeDefinitionMetaDataVersion version = study.MetaDataVersion.First();

            if ((version.CodeList != null) && (version.CodeList.Count > 0))
            {
                foreach (ODMcomplexTypeDefinitionCodeList codeListDef in version.CodeList)
                {
                    if (codeListDef.OID.Equals(codeListOID))
                    {
                        return codeListDef;
                    }
                }
            }

            return null;
        }

        public static String[] getCodeListValues(ODMcomplexTypeDefinitionCodeList codeList, String lang)
        {
            List<ODMcomplexTypeDefinitionCodeListItem> codeListItems = codeList.Items.Where(_ => _ is ODMcomplexTypeDefinitionCodeListItem).Select(_ => _ as ODMcomplexTypeDefinitionCodeListItem).ToList();//.getCodeListItem();
            String[] codeListValues = new String[codeListItems.Count];

            for (int i = 0; i < codeListValues.Length; i++)
            {
                ODMcomplexTypeDefinitionCodeListItem codeListItem = codeListItems[i];

                codeListValues[i] = GetTranslatedValue(codeListItem, lang);
            }

            return codeListValues;
        }

        /**
         * Look for a translated value for the given item. Returns the language specific value, or the
         * first value if the translated value could not be found.
         */

        public static string GetTranslatedValue(ODMcomplexTypeDefinitionCodeListItem codeListItem, string lang)
        {
            var translatedValue = default(string);

            foreach (ODMcomplexTypeDefinitionTranslatedText translatedText in codeListItem.Decode.TranslatedText)
            {
                // TODO: the language attribute is not always available for OpenClinica data.
                if (translatedText.lang != null && translatedText.lang.Equals("en"))
                {
                    translatedValue = translatedText.Value;
                    break;
                }
            }

            if (translatedValue == null)
            {
                // take first value if we can't find an english translation
                translatedValue = codeListItem.Decode.TranslatedText.First().Value;
            }

            return translatedValue;
        }

        public static bool IsNumeric(DataType type)
        {
            switch (type)
            {
                case DataType.integer:
                case DataType.@float:
                case DataType.@double:
                    return true;
                default:
                    return false;
            }
        }

        public static ODMcomplexTypeDefinitionCodeListItem getCodeListItem(ODMcomplexTypeDefinitionCodeList codeList, String codedValue)
        {
            foreach (ODMcomplexTypeDefinitionCodeListItem codeListItem in codeList.Items.Where(_ => _ is ODMcomplexTypeDefinitionCodeListItem).Select(_ => _ as ODMcomplexTypeDefinitionCodeListItem).ToList())//.getCodeListItem()
            {
                if (codeListItem.CodedValue.Equals(codedValue))
                {
                    return codeListItem;
                }
            }

            return null;
        }
    }

    public class ByteArrayBulder
    {
        private List<byte> Bytes = new List<byte>();

        public void Update(params byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException("bytes");
            Bytes.AddRange(bytes);
        }

        public byte[] GetBytes()
        {
            return Bytes.ToArray();
        }
    }

    //TODO: Entity framework
    public interface IStudyDao
    {
        void insertMetadata(I2B2StudyInfo studyInfo);

        void preSetupI2B2Study(string projectID, string sourceSystem);

        void executeBatch();
    }

    //TODO: Entity framework
    public class StudyDao : IStudyDao
    {
        public void insertMetadata(I2B2StudyInfo studyInfo)
        {
            throw new NotImplementedException();
        }

        public void preSetupI2B2Study(string projectID, string sourceSystem)
        {
            throw new NotImplementedException();
        }

        public void executeBatch()
        {
            throw new NotImplementedException();
        }
    }

    //TODO: Entity framework
    public interface IClinicalDataDao
    {
        void cleanupClinicalData(string projectID, string sourceSystem);

        void insertObservation(I2B2ClinicalDataInfo clinicalDataInfo);

        void executeBatch();
    }

    //TODO: Entity framework
    public class ClinicalDataDao : IClinicalDataDao
    {
        public void cleanupClinicalData(string projectID, string sourceSystem)
        {
            throw new NotImplementedException();
        }

        public void insertObservation(I2B2ClinicalDataInfo clinicalDataInfo)
        {
            throw new NotImplementedException();
        }

        public void executeBatch()
        {
            throw new NotImplementedException();
        }
    }

    //https://github.com/CTMM-TraIT/trait_odm_to_i2b2/blob/edbc360643d64a51ca13ce4c0c57e282c04ccb2d/src/main/java/com/recomdata/i2b2/MetaDataXML.java
    public class MetaDataXML
    {
        /**
         * The element name used for the enumeration values.
         */
        private const String ENUM_VALUES_ELEMENT_NAME = "EnumValues";

        /**
         * The value used for units that are not available.
         */
        private const String NOT_AVAILABLE_VALUE = "N/A";

        /**
         * Create Enum type metadata xml for items such as Sex, Race, etc.
         *
         * @param itemOID    the OID of the item.
         * @param itemName   the name of the item.
         * @param enumValues the enumeration values to add.
         * @return the metadata xml (as a string).
         */

        public String getEnumMetadataXML(string itemOID, string itemName, string[] enumValues)
        {
            throw new NotImplementedException();
        }

        /**
         * Create Integer type metadata xml.
         *
         * @param itemOID  the OID of the item.
         * @param itemName the name of the item.
         * @return the metadata xml (as a string).
         */

        public String getIntegerMetadataXML(string itemOID, string itemName)
        {
            throw new NotImplementedException();
        }

        /**
         * Create Float type metadata xml.
         *
         * @param itemOID  the OID of the item.
         * @param itemName the name of the item.
         * @return the metadata xml (as a string).
         */

        public String getFloatMetadataXML(string itemOID, string itemName)
        {
            throw new NotImplementedException();
        }

        /**
         * Create String type metadata xml.
         *
         * @param itemOID  the OID of the item.
         * @param itemName the name of the item.
         * @return the metadata xml (as a string).
         */

        public String getStringMetadataXML(string itemOID, string itemName)
        {
            throw new NotImplementedException();
        }

        /**
         * Create metadata xml using the specified values (which can be altered if required).
         *
         * @param testId   the value for the TestID element.
         * @param testName the value for the TestName element.
         * @param dataType the value for the DataType element.
         * @return the metadata xml (as a string).
         */

        private XElement createBaseMetadata(string testId, string testName, string dataType)
        {
            throw new NotImplementedException();
        }

        /**
         * Add the simple elements for the metadat xml to the root element.
         *
         * @param root     the root element for the xml document.
         * @param testId   the value for the TestID element.
         * @param testName the value for the TestName element.
         * @param dataType the value for the DataType element.
         */

        private void addSimpleElements(XElement root, string testId, string testName, string dataType)
        {
            throw new NotImplementedException();
        }

        /**
         * Convert an xml root element into a compact formatted xml string.
         *
         * @param rootElement the root element to convert.
         * @return the compact formatted xml string.
         */

        private string toString(XElement rootElement)
        {
            throw new NotImplementedException();
        }
    }

    public class I2B2StudyInfo
    {
        public I2B2StudyInfo()
        {
            MappliedPath = "@";
        }

        //TODO: Better names
        public int Chlevel { get; set; }
        public string Cfullname { get; set; }
        public string Cname { get; set; }
        public string CsynonmCd { get; set; }
        public string CvisualAttributes { get; set; }
        public int CtotalNum { get; set; }
        public string Cbasecode { get; set; }
        public string Cmetadataxml { get; set; }
        public string CfactTableColumn { get; set; }
        public string Ctablename { get; set; }
        public string Ccolumnname { get; set; }
        public string CcolumnDatatype { get; set; }
        public string Coperator { get; set; }
        public string Cdimcode { get; set; }
        public string Ccomment { get; set; }
        public string Ctooltip { get; set; }
        public string MappliedPath { get; set; }
        public DateTime? UpdateDate { get; set; }//non nullable?
        public DateTime? DownloadDate { get; set; }//non nullable?
        public DateTime? ImportDate { get; set; }//non nullable?
        public string SourceSystemCd { get; set; }
        public string Valuetype { get; set; }

        public override string ToString()
        {
            return "I2B2StudyInfo [cbasecode=" + Cbasecode + ", cdimcode=" + Cdimcode + ", chlevel=" + Chlevel + ", cname=" + Cname + "]";
        }
    }

    public class I2B2ClinicalDataInfo
    {
        public I2B2ClinicalDataInfo()
        {
            ProviderId = "@";
            ModifierCd = "@";
        }

        //observation_fact
        //TODO: Better names
        public int EncounterNum { get; set; }
        public string PatientNum { get; set; }
        public string ConceptCd { get; set; }
        public string ProviderId { get; set; }
        public DateTime? StartDate { get; set; }//non nullable?
        public string ModifierCd { get; set; }
        public string ValTypeCd { get; set; }
        public string TvalChar { get; set; }
        public decimal? NvalNum { get; set; }//non nullable?//BigDecimal
        public int InstanceNum { get; set; }
        public string ValueFlagCd { get; set; }
        public decimal? QuantityNum { get; set; }//non nullable?//BigDecimal
        public string UnitsCd { get; set; }
        public DateTime? EndDate { get; set; }//non nullable?
        public string LocationCd { get; set; }
        public string ObservationBlob { get; set; }
        public decimal? ConfidenceNum { get; set; }//non nullable?//BigDecimal
        public DateTime? UpdateDate { get; set; }//non nullable?
        public DateTime? DownloadDate { get; set; }//non nullable?
        public DateTime? ImportDate { get; set; }//non nullable?
        public string SourcesystemCd { get; set; }
        public int UploadId { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("I2B2ClinicalDataInfo [patientNum=");
            builder.Append(PatientNum);
            builder.Append(", encounterNum=");
            builder.Append(EncounterNum);
            builder.Append(", instanceNum=");
            builder.Append(InstanceNum);
            builder.Append(", conceptCd=");
            builder.Append(ConceptCd);
            builder.Append(", modifierCd=");
            builder.Append(ModifierCd);
            builder.Append(", startDate=");
            builder.Append(StartDate);
            builder.Append(", endDate=");
            builder.Append(EndDate);
            builder.Append(", valueFlagCd=");
            builder.Append(ValueFlagCd);
            builder.Append(", valTypeCd=");
            builder.Append(ValTypeCd);
            builder.Append(", tvalChar=");
            builder.Append(TvalChar);
            builder.Append(", nvalNum=");
            builder.Append(NvalNum);
            builder.Append(", quantityNum=");
            builder.Append(QuantityNum);
            builder.Append(", unitsCd=");
            builder.Append(UnitsCd);
            builder.Append(", sourcesystemCd=");
            builder.Append(SourcesystemCd);
            builder.Append("]");
            return builder.ToString();
        }
    }

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

        private void saveStudy(ODMcomplexTypeDefinitionStudy study)
        {
            // Need to include source system in path to avoid conflicts between servers
            String studyKey = odm.SourceSystem + ":" + study.OID;

            String studyPath = "\\" + "STUDY" + "\\" + studyKey + "\\";
            String studyToolTip = "STUDY" + "\\" + studyKey;

            // set c_hlevel 1 data (Study)
            studyInfo.Chlevel = IConstants.C_HLEVEL_1;
            studyInfo.Cfullname = studyPath;
            studyInfo.Cname = study.GlobalVariables.StudyName.Value;
            studyInfo.CsynonmCd = IConstants.C_SYNONYM_CD;
            studyInfo.CvisualAttributes = IConstants.C_VISUALATTRIBUTES_FOLDER;
            studyInfo.CfactTableColumn = IConstants.C_FACTTABLECOLUMN;
            studyInfo.Ctablename = IConstants.C_TABLENAME;
            studyInfo.Ccolumnname = IConstants.C_COLUMNNAME;
            studyInfo.CcolumnDatatype = IConstants.C_COLUMNDATATYPE;
            studyInfo.Coperator = IConstants.C_OPERATOR;
            studyInfo.SourceSystemCd = odm.SourceSystem;
            studyInfo.UpdateDate = currentDate;
            studyInfo.DownloadDate = currentDate;
            studyInfo.ImportDate = currentDate;
            studyInfo.Cdimcode = studyPath;
            studyInfo.Ctooltip = studyToolTip;

            logStudyInfo();

            // insert level 1 data
            studyDao.insertMetadata(studyInfo);

            // save child events
            ODMcomplexTypeDefinitionMetaDataVersion version = study.MetaDataVersion.First();

            if (version.Protocol.StudyEventRef != null)
            {
                foreach (ODMcomplexTypeDefinitionStudyEventRef studyEventRef in version.Protocol.StudyEventRef)
                {
                    ODMcomplexTypeDefinitionStudyEventDef studyEventDef =
                            ODMUtil.getStudyEvent(study, studyEventRef.StudyEventOID);

                    saveEvent(study, studyEventDef, studyPath, studyToolTip);
                }
            }
        }

        /**
         * set up i2b2 metadata level 2 (Event) info into STUDY
         *
         * @throws SQLException
         * @throws JAXBException
         */

        private void saveEvent(ODMcomplexTypeDefinitionStudy study,
                               ODMcomplexTypeDefinitionStudyEventDef studyEventDef,
                               String studyPath, String studyToolTip)
        {
            String eventPath = studyPath + studyEventDef.OID + "\\";
            String eventToolTip = studyToolTip + "\\" + studyEventDef.OID;

            // set c_hlevel 2 data (StudyEvent)
            studyInfo.Chlevel = IConstants.C_HLEVEL_2;
            studyInfo.Cfullname = eventPath;
            studyInfo.Cname = studyEventDef.Name;
            studyInfo.Cdimcode = eventPath;
            studyInfo.Ctooltip = eventToolTip;
            studyInfo.CvisualAttributes = IConstants.C_VISUALATTRIBUTES_FOLDER;

            logStudyInfo();

            // insert level 2 data
            studyDao.insertMetadata(studyInfo);

            foreach (ODMcomplexTypeDefinitionFormRef formRef in studyEventDef.FormRef)
            {
                ODMcomplexTypeDefinitionFormDef formDef = ODMUtil.getForm(study, formRef.FormOID);

                saveForm(study, studyEventDef, formDef, eventPath, eventToolTip);
            }
        }

        /**
         * set up i2b2 metadata level 3 (Form) info into STUDY
         *
         * @throws SQLException
         * @throws JAXBException
         */

        private void saveForm(ODMcomplexTypeDefinitionStudy study,
                              ODMcomplexTypeDefinitionStudyEventDef studyEventDef,
                              ODMcomplexTypeDefinitionFormDef formDef, String eventPath,
                              String eventToolTip)
        {
            String formPath = eventPath + formDef.OID + "\\";
            String formToolTip = eventToolTip + "\\" + formDef.OID;

            // set c_hlevel 3 data (Form)
            studyInfo.Chlevel = IConstants.C_HLEVEL_3;
            studyInfo.Cfullname = formPath;
            studyInfo.Cname = getTranslatedDescription(formDef.Description, "en", formDef.Name);
            studyInfo.Cdimcode = formPath;
            studyInfo.Ctooltip = formToolTip;
            studyInfo.CvisualAttributes = IConstants.C_VISUALATTRIBUTES_FOLDER;

            logStudyInfo();

            // insert level 3 data
            studyDao.insertMetadata(studyInfo);

            foreach (ODMcomplexTypeDefinitionItemGroupRef itemGroupRef in formDef.ItemGroupRef)
            {
                ODMcomplexTypeDefinitionItemGroupDef itemGroupDef =
                        ODMUtil.getItemGroup(study, itemGroupRef.ItemGroupOID);

                if (itemGroupDef.ItemRef != null)
                {
                    foreach (ODMcomplexTypeDefinitionItemRef itemRef in itemGroupDef.ItemRef)
                    {
                        ODMcomplexTypeDefinitionItemDef itemDef = ODMUtil.getItem(study, itemRef.ItemOID);

                        saveItem(study, studyEventDef, formDef, itemDef, formPath, formToolTip);
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

        private void saveItem(ODMcomplexTypeDefinitionStudy study,
                              ODMcomplexTypeDefinitionStudyEventDef studyEventDef,
                              ODMcomplexTypeDefinitionFormDef formDef,
                              ODMcomplexTypeDefinitionItemDef itemDef, String formPath,
                              String formToolTip)
        {
            String itemPath = formPath + itemDef.OID + "\\";
            String itemToolTip = formToolTip + "\\" + itemDef.OID;

            // set c_hlevel 4 data (Items)
            studyInfo.Chlevel = IConstants.C_HLEVEL_4;
            studyInfo.Cfullname = itemPath;
            studyInfo.Cname = getTranslatedDescription(itemDef.Description, "en", itemDef.Name);
            studyInfo.Cbasecode = generateConceptCode(study.OID, studyEventDef.OID, formDef.OID, itemDef.OID, null);
            studyInfo.Cdimcode = itemPath;
            studyInfo.Ctooltip = itemToolTip;
            studyInfo.Cmetadataxml = createMetadataXml(study, itemDef);

            // It is a leaf node
            if (itemDef.CodeListRef == null)
            {
                studyInfo.CvisualAttributes = IConstants.C_VISUALATTRIBUTES_LEAF;
            }
            else
            {
                studyInfo.CvisualAttributes = IConstants.C_VISUALATTRIBUTES_FOLDER;
            }

            logStudyInfo();

            // insert level 4 data
            studyDao.insertMetadata(studyInfo);

            if (itemDef.CodeListRef != null)
            {
                ODMcomplexTypeDefinitionCodeList codeList = ODMUtil.getCodeList(study, itemDef.CodeListRef.CodeListOID);

                if (codeList != null)
                {
                    foreach (ODMcomplexTypeDefinitionCodeListItem codeListItem in codeList.Items.Where(_ => _ is ODMcomplexTypeDefinitionCodeListItem).Select(_ => _ as ODMcomplexTypeDefinitionCodeListItem).ToList())//getCodeListItem()
                    {
                        // save
                        // level 5
                        saveCodeListItem(study, studyEventDef, formDef, itemDef, codeListItem, itemPath, itemToolTip);
                    }
                }
            }
        }

        private String getTranslatedDescription(
                ODMcomplexTypeDefinitionDescription description, String lang, String defaultValue)
        {
            if (description != null)
            {
                foreach (ODMcomplexTypeDefinitionTranslatedText translatedText in description.TranslatedText)
                {
                    if (translatedText.lang.Equals(lang))
                    {
                        return translatedText.Value;
                    }
                }
            }

            return defaultValue;
        }

        /**
         * @throws JAXBException
         */

        private String createMetadataXml(ODMcomplexTypeDefinitionStudy study,
                                         ODMcomplexTypeDefinitionItemDef itemDef)
        {
            String metadataXml = null;

            switch (itemDef.DataType)
            {
                case DataType.integer:
                    metadataXml = mdx.getIntegerMetadataXML(itemDef.OID, itemDef.Name);
                    break;

                case DataType.@float:
                case DataType.@double:
                    metadataXml = mdx.getFloatMetadataXML(itemDef.OID, itemDef.Name);
                    break;

                case DataType.text:
                case DataType.@string:
                    if (itemDef.CodeListRef == null)
                    {
                        metadataXml = mdx.getStringMetadataXML(itemDef.OID, itemDef.Name);
                    }
                    else
                    {
                        ODMcomplexTypeDefinitionCodeList codeList =
                                ODMUtil.getCodeList(study, itemDef.CodeListRef.CodeListOID);
                        String[] codeListValues = ODMUtil.getCodeListValues(codeList, "en");

                        metadataXml = mdx.getEnumMetadataXML(itemDef.OID, itemDef.Name, codeListValues);
                    }
                    break;

                case DataType.boolean:

                    break;

                case DataType.date:
                case DataType.time:
                case DataType.datetime:
                    metadataXml = mdx.getStringMetadataXML(itemDef.OID, itemDef.Name);
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

        private void saveCodeListItem(ODMcomplexTypeDefinitionStudy study,
                                      ODMcomplexTypeDefinitionStudyEventDef studyEventDef,
                                      ODMcomplexTypeDefinitionFormDef formDef,
                                      ODMcomplexTypeDefinitionItemDef itemDef,
                                      ODMcomplexTypeDefinitionCodeListItem codeListItem, String itemPath,
                                      String itemToolTip)
        {
            String value = ODMUtil.GetTranslatedValue(codeListItem, "en");
            String codedValue = codeListItem.CodedValue;
            String codeListItemPath = itemPath + codedValue + "\\";
            String codeListItemToolTip = itemToolTip + "\\" + value;

            // set c_hlevel 5 data (TranslatedText)
            studyInfo.Chlevel = IConstants.C_HLEVEL_5;
            studyInfo.Cfullname = codeListItemPath;
            studyInfo.Cname = getTranslatedDescription(itemDef.Description, "en", itemDef.Name) + ": " + value;
            studyInfo.Cbasecode = generateConceptCode(study.OID, studyEventDef.OID, formDef.OID, itemDef.OID, codedValue);
            studyInfo.Cdimcode = codeListItemPath;
            studyInfo.Ctooltip = codeListItemToolTip;
            studyInfo.Cmetadataxml = null;
            studyInfo.CvisualAttributes = IConstants.C_VISUALATTRIBUTES_LEAF;

            logStudyInfo();

            studyDao.insertMetadata(studyInfo);
        }

        /**
         * method to parse ODM and save data into i2b2
         *
         * @throws SQLException
         * @throws JAXBException
         * @throws ParseException
         */

        public void processODM()
        {
            //log.info("Start to parse ODM xml and save to i2b2");
            Debug.WriteLine("Start to parse ODM xml and save to i2b2");

            // build the call
            processODMStudy();
            processODMClinicalData();
        }

        /*
         * This method takes ODM XML io.File obj as input and parsed by JAXB API and
         * then traverses through ODM tree object and save data into i2b2 metadata
         * database in i2b2 data format.
         *
         * @throws SQLException
         * @throws JAXBException
         */

        public void processODMStudy()
        {
            /*
             * Need to traverse through the study definition to: 1) Lookup all
             * definition values in tree nodes. 2) Set node values into i2b2 bean
             * info and ready for populating into i2b2 database.
             */
            foreach (ODMcomplexTypeDefinitionStudy study in odm.Study)
            {
                //log.info("Processing study metadata for study " + study.getGlobalVariables().getStudyName().getValue() + "(OID " + study.OID + ")");
                Debug.WriteLine("Processing study metadata for study " + study.GlobalVariables.StudyName.Value + "(OID " + study.OID + ")");
                //log.info("Deleting old study metadata and data");
                Debug.WriteLine("Deleting old study metadata and data");

                studyDao.preSetupI2B2Study(study.OID, odm.SourceSystem);

                //log.info("Inserting study metadata into i2b2");
                Debug.WriteLine("Inserting study metadata into i2b2");
                var timer = Stopwatch.StartNew();

                saveStudy(study);

                timer.Stop();
                //log.info("Completed loading study metadata into i2b2 in " + (endTime - startTime) + " ms");
                Debug.WriteLine("Completed loading study metadata into i2b2 in " + timer.ElapsedMilliseconds + " ms");
            }

            /*
             * Flush any remaining batched up records.
             */
            studyDao.executeBatch();
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

        public void processODMClinicalData()
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

            foreach (ODMcomplexTypeDefinitionStudy study in odm.Study)
            {
                clinicalDataDao.cleanupClinicalData(study.OID, odm.SourceSystem);
            }

            foreach (ODMcomplexTypeDefinitionClinicalData clinicalData in odm.ClinicalData)
            {
                if (clinicalData.SubjectData == null)
                {
                    continue;
                }

                //log.info("Save Clinical data for study OID " + clinicalData.getStudyOID() + " into i2b2...");
                Debug.WriteLine("Save Clinical data for study OID " + clinicalData.StudyOID + " into i2b2...");
                var timer = Stopwatch.StartNew();

                ODMcomplexTypeDefinitionStudy study = ODMUtil.getStudy(odm, clinicalData.StudyOID);
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
                int encounterNum = 0;

                foreach (ODMcomplexTypeDefinitionSubjectData subjectData in clinicalData.SubjectData)
                {
                    if (subjectData.StudyEventData == null)
                    {
                        continue;
                    }

                    encounterNum++;

                    foreach (ODMcomplexTypeDefinitionStudyEventData studyEventData in subjectData.StudyEventData)
                    {
                        if (studyEventData.FormData == null)
                        {
                            continue;
                        }

                        foreach (ODMcomplexTypeDefinitionFormData formData in studyEventData.FormData)
                        {
                            if (formData.ItemGroupData == null)
                            {
                                continue;
                            }

                            foreach (ODMcomplexTypeDefinitionItemGroupData itemGroupData in formData.ItemGroupData)
                            {
                                if (itemGroupData.Items == null)
                                {
                                    continue;
                                }

                                foreach (ODMcomplexTypeDefinitionItemData itemData in itemGroupData.Items.Where(_ => _ is ODMcomplexTypeDefinitionItemData).Select(_ => _ as ODMcomplexTypeDefinitionItemData).ToList())
                                {//getItemDataGroup()
                                    if (itemData.Value != null)
                                    {
                                        saveItemData(study, subjectData, studyEventData, formData, itemData, encounterNum);
                                    }
                                }
                            }
                        }
                    }
                }

                /*
                 * Flush any remaining batched up observations;
                 */
                clinicalDataDao.executeBatch();

                timer.Stop();
                //log.info("Completed Clinical data to i2b2 for study OID " + clinicalData.getStudyOID() + " in " + (endTime - startTime) + " ms");
                Debug.WriteLine("Completed Clinical data to i2b2 for study OID " + clinicalData.StudyOID + " in " + timer.ElapsedMilliseconds + " ms");
            }
        }

        private void logStudyInfo()
        {
            //if (log.isDebugEnabled()) {
            //    log.debug("Inserting study metadata record: " + studyInfo);
            Debug.WriteLine("Inserting study metadata record: " + studyInfo);
            //}
        }

        /**
         * @throws JAXBException
         * @throws ParseException
         * @throws SQLException
         */

        private void saveItemData(
                ODMcomplexTypeDefinitionStudy study,
                ODMcomplexTypeDefinitionSubjectData subjectData,
                ODMcomplexTypeDefinitionStudyEventData studyEventData,
                ODMcomplexTypeDefinitionFormData formData,
                ODMcomplexTypeDefinitionItemData itemData,
                int encounterNum)
        {
            String itemValue = itemData.Value;
            ODMcomplexTypeDefinitionItemDef item = ODMUtil.getItem(study, itemData.ItemOID);

            String conceptCd;

            if (item.CodeListRef != null)
            {
                clinicalDataInfo.ValTypeCd = "T";
                clinicalDataInfo.NvalNum = null;

                ODMcomplexTypeDefinitionCodeList codeList = ODMUtil.getCodeList(study, item.CodeListRef.CodeListOID);
                ODMcomplexTypeDefinitionCodeListItem codeListItem = ODMUtil.getCodeListItem(codeList, itemValue);

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
                    conceptCd = generateConceptCode(
                            study.OID,
                            studyEventData.StudyEventOID,
                            formData.FormOID,
                            itemData.ItemOID,
                            itemValue);
                    clinicalDataInfo.TvalChar = ODMUtil.GetTranslatedValue(codeListItem, "en");
                }
            }
            else if (ODMUtil.IsNumeric(item.DataType))
            {
                conceptCd = generateConceptCode(
                        study.OID,
                        studyEventData.StudyEventOID,
                        formData.FormOID,
                        itemData.ItemOID,
                        null);

                clinicalDataInfo.ValTypeCd = "N";
                clinicalDataInfo.TvalChar = "E";
                clinicalDataInfo.NvalNum = String.IsNullOrWhiteSpace(itemValue) ? default(decimal?) : Decimal.Parse(itemValue);// TryParse? BigDecimal == Decimal? not sure these are equivolent, but it may be close enough for our purposes.
            }
            else
            {
                conceptCd = generateConceptCode(
                        study.OID,
                        studyEventData.StudyEventOID,
                        formData.FormOID,
                        itemData.ItemOID,
                        null);

                clinicalDataInfo.ValTypeCd = "T";
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
                clinicalDataDao.insertObservation(clinicalDataInfo);
            }
            catch (Exception e)//TODO: Entity framework exception (was SQLException)
            {
                String sError = "Error inserting observation_fact record.";
                sError += " study: " + study.OID;
                sError += " item: " + itemData.ItemOID;
                //log.error(sError, e);
                Debug.WriteLine(sError);
            }
        }

        /**
         * Create concept code with all OIDs and make the total length less than 50
         * and unique
         *
         * @return the unique concept code.
         */

        private String generateConceptCode(String studyOID, String studyEventOID,
                                           String formOID, String itemOID, String value)
        {
            conceptBuffer.Length = 6;
            conceptBuffer.Append(studyOID).Append("|");

            var message = new ByteArrayBulder();//I don't think we want quite use StringBuilder here becuase the pipes are byte cast chars, not Unicode literals. md5("\x00\x7C") vs md5("\x7C")

            message.Update(Encoding.UTF8.GetBytes(odm.SourceSystem));
            message.Update((byte)'|');
            message.Update(Encoding.UTF8.GetBytes(studyEventOID));
            message.Update((byte)'|');
            message.Update(Encoding.UTF8.GetBytes(formOID));
            message.Update((byte)'|');
            message.Update(Encoding.UTF8.GetBytes(itemOID));

            if (value != null)
            {
                message.Update((byte)'|');
                message.Update(Encoding.UTF8.GetBytes(value));
            }

            var digest = messageDigest.ComputeHash(message.GetBytes());
            foreach (byte digestByte in digest)
            {
                conceptBuffer.Append(digestByte.ToString("X2"));//& 0xFF returns exactly the same byte, not sure why they did that.
            }

            String conceptCode = conceptBuffer.ToString();
            //if (log.isDebugEnabled()) {
            //    log.debug(new StringBuffer("Concept code ").append(conceptCode)
            //            .append(" generated for studyOID=").append(studyOID)
            //            .append(", studyEventOID=").append(studyEventOID)
            //            .append(", formOID=").append(formOID)
            //            .append(", itemOID=").append(itemOID)
            //           .append(", value=").append(value).toString());
            Debug.WriteLine(new StringBuilder("Concept code ").Append(conceptCode)
                    .Append(" generated for studyOID=").Append(studyOID)
                    .Append(", studyEventOID=").Append(studyEventOID)
                    .Append(", formOID=").Append(formOID)
                    .Append(", itemOID=").Append(itemOID)
                    .Append(", value=").Append(value).ToString());
            //}

            return conceptCode;
        }
    }
}
