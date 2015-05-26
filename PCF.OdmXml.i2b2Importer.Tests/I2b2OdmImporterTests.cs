using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PCF.OdmXml.i2b2Importer.Tests
{
    [TestClass]
    public class I2b2OdmImporterTests
    {
        [TestMethod]
        public async Task ImportAsync()
        {
            ODM odm;
            Exception exception;
            Assert.IsTrue(ODM.Load(@"Samples\ODM_REDCap_Example.xml", out odm, out exception));
            Assert.IsNotNull(odm);
            Assert.IsNull(exception);

            await new I2b2OdmImporter().ImportAsync(odm, null);
        }
    }
}
