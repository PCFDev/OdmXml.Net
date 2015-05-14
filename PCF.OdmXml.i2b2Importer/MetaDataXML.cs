using System;
using System.Xml.Linq;

namespace PCF.OdmXml.i2b2Importer
{
    //TODO: Refactor
    //https://github.com/CTMM-TraIT/trait_odm_to_i2b2/blob/edbc360643d64a51ca13ce4c0c57e282c04ccb2d/src/main/java/com/recomdata/i2b2/MetaDataXML.java
    public class MetaDataXML
    {
        /// <summary>
        /// The element name used for the enumeration values.
        /// </summary>
        private const string ENUM_VALUES_ELEMENT_NAME = "EnumValues";
        /// <summary>
        /// The value used for units that are not available.
        /// </summary>
        private const string NOT_AVAILABLE_VALUE = "N/A";

        /// <summary>
        /// Create Enum type metadata xml for items such as Sex, Race, etc.
        /// </summary>
        /// <param name="itemOID">The OID of the item.</param>
        /// <param name="itemName">The name of the item.</param>
        /// <param name="enumValues">The enumeration values to add.</param>
        /// <returns>The metadata xml as a <see cref="System.String" />.</returns>
        public string GetEnumMetadataXML(string itemOID, string itemName, string[] enumValues)
        {
            var root = CreateBaseMetadata(itemOID, itemName, "Enum");

            var enumValuesElement = root.Element(ENUM_VALUES_ELEMENT_NAME);
            foreach (string enumValue in enumValues) {
                enumValuesElement.Add(new XElement("Val", enumValue));
            }

            return ToString(root);
        }

        /// <summary>
        /// Create Integer type metadata xml.
        /// </summary>
        /// <param name="itemOID">The OID of the item.</param>
        /// <param name="itemName">The name of the item.</param>
        /// <returns>The metadata xml as a <see cref="System.String" />.</returns>
        public string GetIntegerMetadataXML(string itemOID, string itemName)
        {
            return ToString(CreateBaseMetadata(itemOID, itemName, "Integer"));
        }

        /// <summary>
        /// Create Float type metadata xml.
        /// </summary>
        /// <param name="itemOID">The OID of the item.</param>
        /// <param name="itemName">The name of the item.</param>
        /// <returns>The metadata xml as a <see cref="System.String" />.</returns>
        public string GetFloatMetadataXML(string itemOID, string itemName)
        {
            return ToString(CreateBaseMetadata(itemOID, itemName, "Float"));
        }

        /// <summary>
        /// Create string type metadata xml.
        /// </summary>
        /// <param name="itemOID">The OID of the item.</param>
        /// <param name="itemName">The name of the item.</param>
        /// <returns>The metadata xml as a <see cref="System.String" />.</returns>
        public string GetStringMetadataXML(string itemOID, string itemName)
        {
            return ToString(CreateBaseMetadata(itemOID, itemName, "String"));
        }

        /// <summary>
        /// Create metadata xml using the specified values (which can be altered if required).
        /// </summary>
        /// <param name="testId">The value for the TestID element.</param>
        /// <param name="testName">The value for the TestName element.</param>
        /// <param name="dataType">The value for the DataType element.</param>
        /// <returns>The metadata xml as a <see cref="System.Xml.Linq.XElement" />.</returns>
        private XElement CreateBaseMetadata(string testId, string testName, string dataType)
        {
            var root = new XElement("ValueMetadata");

            AddSimpleElements(root, testId, testName, dataType);

            // Add CommentsDeterminingExclusion element with sub element.
            root.Add(new XElement("CommentsDeterminingExclusion", new XElement("Com")));

            // Add UnitValues element with sub elements.
            var unitValue = new XElement("UnitValues");
            unitValue.Add(new XElement("NormalUnits", NOT_AVAILABLE_VALUE));
            unitValue.Add(new XElement("EqualUnits", NOT_AVAILABLE_VALUE));
            unitValue.Add(new XElement("ExcludingUnits"));
            var convertUnit = new XElement("ConvertingUnits");
            convertUnit.Add(new XElement("Units"));
            convertUnit.Add(new XElement("MultiplyingFactor"));
            unitValue.Add(convertUnit);
            root.Add(unitValue);

            // Add Analysis element with sub elements.
            var analysis = new XElement("Analysis");
            analysis.Add(new XElement("Enums"));
            analysis.Add(new XElement("Counts"));
            analysis.Add(new XElement("New"));
            root.Add(analysis);

            return root;
        }

        /// <summary>
        /// Add the simple elements for the metadat xml to the root element.
        /// </summary>
        /// <param name="root">The root element for the xml document.</param>
        /// <param name="testId">The value for the TestID element.</param>
        /// <param name="testName">The value for the TestName element.</param>
        /// <param name="dataType"The value for the DataType element.></param>
        private void AddSimpleElements(XElement root, string testId, string testName, string dataType)
        {
            var creationDateTime = DateTime.UtcNow.ToString(Constants.DATETIME_FORMAT);

            // Creating children for the root element.
            root.Add(new XElement("Version", "3.02"));
            root.Add(new XElement("CreationDateTime", creationDateTime));
            root.Add(new XElement("TestID", testId));
            root.Add(new XElement("TestName", testName));
            root.Add(new XElement("DataType", dataType));
            root.Add(new XElement("CodeType", "GRP"));
            root.Add(new XElement("Loinc", "1"));
            root.Add(new XElement("Flagstouse"));
            root.Add(new XElement("Oktousevalues", "N"));
            root.Add(new XElement("MaxStringLength"));
            root.Add(new XElement("LowofLowValue"));
            root.Add(new XElement("HighofLowValue"));
            root.Add(new XElement("LowofHighValue"));
            root.Add(new XElement("HighofHighValue"));
            root.Add(new XElement("LowofToxicValue"));
            root.Add(new XElement("HighofToxicValue"));
            root.Add(new XElement(ENUM_VALUES_ELEMENT_NAME));
        }

        /// <summary>
        /// Convert an xml root element into a compact formatted xml string.
        /// </summary>
        /// <param name="rootElement">The root element to convert.</param>
        /// <returns>The compact formatted xml <see cref="System.String" />.</returns>
        private string ToString(XElement rootElement)
        {
            //Save option?
            //return rootElement.ToString(SaveOptions.None);
            return new XDocument(rootElement).ToString(SaveOptions.None);
        }
    }
}
