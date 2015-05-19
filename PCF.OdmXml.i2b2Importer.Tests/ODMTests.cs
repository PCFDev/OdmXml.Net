using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PCF.OdmXml.i2b2Importer.Tests
{
    [TestClass]
    public class ODMTests
    {
        [TestMethod]
        public void Load_Study_FromFile()
        {
            ODMcomplexTypeDefinitionStudy study;
            Exception exception;
            //Assert.IsTrue(

            ODMcomplexTypeDefinitionStudy.Load(@"Samples\Example-study.xml", out study, out exception);

            //);
            Assert.IsNotNull(study);
            Assert.IsNull(exception);
        }

        [TestMethod]
        public void Load_MetadataVersion_FromFile()
        {
            ODMcomplexTypeDefinitionMetaDataVersion item;
            Exception exception;

            var success = ODMcomplexTypeDefinitionMetaDataVersion.Load(@"Samples\Example-metadataversion.xml", out item, out exception);

            Assert.IsTrue(success);

            Assert.IsNotNull(item);
            Assert.IsNull(exception);
        }
    }
}
