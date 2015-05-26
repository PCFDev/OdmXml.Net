using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PCF.OdmXml.i2b2Importer.Tests
{
    [TestClass]
    public class I2b2OdmProcessorTests
    {
        [TestMethod]
        public void ProcessODM()
        {
            ODM odm;
            Exception exception;
            Assert.IsTrue(ODM.Load(@"Samples\ODM_REDCap_Example.xml", out odm, out exception));
            Assert.IsNotNull(odm);
            Assert.IsNull(exception);

            new I2b2OdmProcessor(odm, null).ProcessODM();//This just calls the other two, should we even bother?
        }

        [TestMethod]
        public void ProcessODMClinicalData()
        {
            ODM odm;
            Exception exception;
            Assert.IsTrue(ODM.Load(@"Samples\ODM_REDCap_Example.xml", out odm, out exception));
            Assert.IsNotNull(odm);
            Assert.IsNull(exception);

            new I2b2OdmProcessor(odm, null).ProcessODMClinicalData();
        }

        [TestMethod]
        public void ProcessODMStudy()
        {
            ODM odm;
            Exception exception;
            Assert.IsTrue(ODM.Load(@"Samples\ODM_REDCap_Example.xml", out odm, out exception));
            Assert.IsNotNull(odm);
            Assert.IsNull(exception);

            new I2b2OdmProcessor(odm, null).ProcessODMStudy();
        }
    }
}
