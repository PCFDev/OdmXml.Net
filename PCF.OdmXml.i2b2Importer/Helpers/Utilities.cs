using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PCF.OdmXml.i2b2Importer.Helpers
{
    //https://github.com/CTMM-TraIT/trait_odm_to_i2b2/blob/452a1950b94d3a779eb66aaf1ad7ef34976c628c/src/main/java/com/recomdata/i2b2/util/ODMUtil.java
    public static class Utilities
    {
        /// <summary>
        /// Create concept code with all OIDs and make the total length less than 50 and unique.
        /// </summary>
        /// <param name="studyOID"></param>
        /// <param name="studyEventOID"></param>
        /// <param name="formOID"></param>
        /// <param name="itemOID"></param>
        /// <param name="value"></param>
        /// <returns>The unique concept code.</returns>
        public static string GenerateConceptCode(string sourceSystem, string studyOID, string studyEventOID, string formOID, string itemOID, string value)
        {
            //TODO: Move to utilities? what about logging?
            //What is this actually used for?
            var concept = new StringBuilder("STUDY|")
                .Append(studyOID)
                .Append("|");

            //I don't think we want quite use StringBuilder here because the pipes are byte cast chars, not Unicode literals. md5("\x00\x7C") vs md5("\x7C")
            var message = new ByteArrayBulder()
                .Append(Encoding.UTF8.GetBytes(sourceSystem))
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
            Debug.WriteLine(new StringBuilder("Concept code ")
                .Append(conceptCode)
                .Append(" generated for studyOID=").Append(studyOID)
                .Append(", studyEventOID=").Append(studyEventOID)
                .Append(", formOID=").Append(formOID)
                .Append(", itemOID=").Append(itemOID)
                .Append(", value=").Append(value).ToString());

            return conceptCode;
        }

        /// <summary>
        /// Resolve CodListDef from CodeListRef
        /// </summary>
        /// <param name="study"></param>
        /// <param name="codeListOID"></param>
        /// <returns></returns>
        public static ODMcomplexTypeDefinitionCodeList GetCodeList(ODMcomplexTypeDefinitionStudy study, string codeListOID)
        {
            var version = study.MetaDataVersion.FirstOrDefault();
            if (version == null || version.CodeList == null)
                return null;
            return version.CodeList.FirstOrDefault(_ => _.OID == codeListOID);
        }

        public static ODMcomplexTypeDefinitionCodeListItem GetCodeListItem(ODMcomplexTypeDefinitionCodeList codeList, string codedValue)
        {
            return codeList.Items
                .Where(_ => _ is ODMcomplexTypeDefinitionCodeListItem)
                .Select(_ => _ as ODMcomplexTypeDefinitionCodeListItem)
                .FirstOrDefault(_ => _.CodedValue == codedValue);
        }

        public static IEnumerable<string> GetCodeListValues(ODMcomplexTypeDefinitionCodeList codeList, string lang)
        {
            return codeList.Items
                .Where(_ => _ is ODMcomplexTypeDefinitionCodeListItem)
                .Select(_ => _ as ODMcomplexTypeDefinitionCodeListItem)
                .Select(_ => GetTranslatedValue(_, lang))
                .ToList();//TODO: Do we want to just return the enumerable? is the data immutable?
        }

        /// <summary>
        /// Resolve FormDef from FormRef
        /// </summary>
        /// <param name="study"></param>
        /// <param name="formOID"></param>
        /// <returns></returns>
        public static ODMcomplexTypeDefinitionFormDef GetForm(ODMcomplexTypeDefinitionStudy study, string formOID)
        {
            var version = study.MetaDataVersion.FirstOrDefault();
            if (version == null || version.FormDef == null)
                return null;
            return version.FormDef.FirstOrDefault(_ => _.OID == formOID);
        }

        /// <summary>
        /// Resolve ItemDef from ItemRef
        /// </summary>
        /// <param name="study"></param>
        /// <param name="itemOID"></param>
        /// <returns></returns>
        public static ODMcomplexTypeDefinitionItemDef GetItem(ODMcomplexTypeDefinitionStudy study, string itemOID)
        {
            var version = study.MetaDataVersion.FirstOrDefault();
            if (version == null || version.ItemDef == null)
                return null;
            return version.ItemDef.FirstOrDefault(_ => _.OID == itemOID);
        }

        /// <summary>
        /// Resolve ItemGroupDef from ItemGroupRef
        /// </summary>
        /// <param name="study"></param>
        /// <param name="itemGroupOID"></param>
        /// <returns></returns>
        public static ODMcomplexTypeDefinitionItemGroupDef GetItemGroup(ODMcomplexTypeDefinitionStudy study, string itemGroupOID)
        {
            var version = study.MetaDataVersion.FirstOrDefault();
            if (version == null || version.ItemGroupDef == null)
                return null;
            return version.ItemGroupDef.FirstOrDefault(_ => _.OID == itemGroupOID);
        }

        public static ODMcomplexTypeDefinitionStudy GetStudy(ODM odm, string studyOID)
        {
            return odm.Study.FirstOrDefault(_ => _.OID == studyOID);
        }

        /// <summary>
        /// Resolve StudyEventDef from StudyEventRef
        /// </summary>
        /// <param name="study"></param>
        /// <param name="studyEventOID"></param>
        /// <returns></returns>
        public static ODMcomplexTypeDefinitionStudyEventDef GetStudyEvent(ODMcomplexTypeDefinitionStudy study, string studyEventOID)
        {
            var version = study.MetaDataVersion.FirstOrDefault();
            if (version == null || version.StudyEventDef == null)
                return null;
            return version.StudyEventDef.FirstOrDefault(_ => _.OID == studyEventOID);
        }

        public static string GetTranslatedDescription(ODMcomplexTypeDefinitionDescription description, string lang = "en", string defaultValue = default(string))
        {
            if (description == null)
                return defaultValue;
            return description.TranslatedText.Where(_ => _.lang == lang).Select(_ => _.Value).FirstOrDefault();
        }

        /// <summary>
        /// Look for a translated value for the given item. Returns the language specific value, or the first value if the translated value could not be found.
        /// </summary>
        /// <param name="codeListItem"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public static string GetTranslatedValue(ODMcomplexTypeDefinitionCodeListItem codeListItem, string lang = "en")
        {
            if (codeListItem.Decode == null)
                return default(string);
            var translatedText = codeListItem.Decode.TranslatedText;
            // TODO: the language attribute is not always available for OpenClinica data.
            return translatedText.Where(_ => _.lang == lang).Select(_ => _.Value).FirstOrDefault()
                ?? translatedText.First().Value;//FirstOrDefault? //Take first value if we can't find an english translation.
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
    }
}
