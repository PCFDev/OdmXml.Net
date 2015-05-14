using System;
using System.Linq;

namespace PCF.OdmXml.i2b2Importer.Helpers
{
    //https://github.com/CTMM-TraIT/trait_odm_to_i2b2/blob/452a1950b94d3a779eb66aaf1ad7ef34976c628c/src/main/java/com/recomdata/i2b2/util/ODMUtil.java
    public static class Utilities
    {
        /// <summary>
        /// Resolve CodListDef from CodeListRef
        /// </summary>
        /// <param name="study"></param>
        /// <param name="codeListOID"></param>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <returns></returns>
        public static ODMcomplexTypeDefinitionCodeList GetCodeList(ODMcomplexTypeDefinitionStudy study, string codeListOID)
        {
            var version = study.MetaDataVersion.First();//FirstOrDefault()?
            if (version.CodeList == null)
                return null;

            foreach (var codeListDef in version.CodeList)
            {
                if (codeListDef.OID.Equals(codeListOID))
                    return codeListDef;
            }
            return null;
        }

        public static ODMcomplexTypeDefinitionCodeListItem GetCodeListItem(ODMcomplexTypeDefinitionCodeList codeList, string codedValue)
        {
            foreach (var codeListItem in codeList.Items.Where(_ => _ is ODMcomplexTypeDefinitionCodeListItem).Select(_ => _ as ODMcomplexTypeDefinitionCodeListItem).ToList())//.getCodeListItem()
            {
                if (codeListItem.CodedValue.Equals(codedValue))
                    return codeListItem;
            }
            return null;
        }

        public static string[] GetCodeListValues(ODMcomplexTypeDefinitionCodeList codeList, string lang)
        {
            var codeListItems = codeList.Items.Where(_ => _ is ODMcomplexTypeDefinitionCodeListItem).Select(_ => _ as ODMcomplexTypeDefinitionCodeListItem).ToList();//.getCodeListItem();
            string[] codeListValues = new string[codeListItems.Count];

            for (int i = 0; i < codeListValues.Length; i++)
            {
                var codeListItem = codeListItems[i];

                codeListValues[i] = GetTranslatedValue(codeListItem, lang);
            }

            return codeListValues;
        }

        /// <summary>
        /// Resolve FormDef from FormRef
        /// </summary>
        /// <param name="study"></param>
        /// <param name="formOID"></param>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <returns></returns>
        public static ODMcomplexTypeDefinitionFormDef GetForm(ODMcomplexTypeDefinitionStudy study, string formOID)
        {
            var version = study.MetaDataVersion.First();//FirstOrDefault()?
            if (version.FormDef == null)
                return null;

            foreach (var formDef in version.FormDef)
            {
                if (formDef.OID.Equals(formOID))
                    return formDef;
            }
            return null;
        }

        /// <summary>
        /// Resolve ItemDef from ItemRef
        /// </summary>
        /// <param name="study"></param>
        /// <param name="itemOID"></param>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <returns></returns>
        public static ODMcomplexTypeDefinitionItemDef GetItem(ODMcomplexTypeDefinitionStudy study, string itemOID)
        {
            var version = study.MetaDataVersion.First();//FirstOrDefault()?
            if (version.ItemDef == null)
                return null;

            foreach (var itemDef in version.ItemDef)
            {
                if (itemDef.OID.Equals(itemOID))
                    return itemDef;
            }
            return null;
        }

        /// <summary>
        /// Resolve ItemGroupDef from ItemGroupRef
        /// </summary>
        /// <param name="study"></param>
        /// <param name="itemGroupOID"></param>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <returns></returns>
        public static ODMcomplexTypeDefinitionItemGroupDef GetItemGroup(ODMcomplexTypeDefinitionStudy study, string itemGroupOID)
        {
            var version = study.MetaDataVersion.First();//FirstOrDefault()?
            if (version.ItemGroupDef == null)
                return null;

            foreach (var itemGroupDef in version.ItemGroupDef)
            {
                if (itemGroupDef.OID.Equals(itemGroupOID))
                    return itemGroupDef;
            }
            return null;
        }

        public static ODMcomplexTypeDefinitionStudy GetStudy(ODM odm, string studyOID)
        {
            foreach (var study in odm.Study)
            {
                if (study.OID.Equals(studyOID))
                    return study;
            }
            return null;
        }

        /// <summary>
        /// Resolve StudyEventDef from StudyEventRef
        /// </summary>
        /// <param name="study"></param>
        /// <param name="studyEventOID"></param>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <returns></returns>
        public static ODMcomplexTypeDefinitionStudyEventDef GetStudyEvent(ODMcomplexTypeDefinitionStudy study, string studyEventOID)
        {
            var version = study.MetaDataVersion.First();//FirstOrDefault()?
            if (version.StudyEventDef == null)
                return null;

            foreach (var studyEventDef in version.StudyEventDef)
            {
                if (studyEventDef.OID.Equals(studyEventOID))
                    return studyEventDef;
            }
            return null;
        }

        /// <summary>
        /// Look for a translated value for the given item. Returns the language specific value, or the first value if the translated value could not be found.
        /// </summary>
        /// <param name="codeListItem"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public static string GetTranslatedValue(ODMcomplexTypeDefinitionCodeListItem codeListItem, string lang)
        {
            var translatedValue = default(string);

            foreach (var translatedText in codeListItem.Decode.TranslatedText)
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
                translatedValue = codeListItem.Decode.TranslatedText.First().Value;//FirstOrDefault()?
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
    }
}
