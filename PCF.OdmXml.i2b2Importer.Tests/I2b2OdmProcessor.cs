using System;
using PCF.OdmXml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PCF.OdmXml.i2b2Importer.Tests
{
    [TestClass]
    public class I2b2OdmProcessor
    {
        [TestMethod]
        public void TestMethod1()
        {
            ODM odm;
            Exception exception;
            Assert.IsTrue(ODM.Load(@"Samples\odm130.xml", out odm, out exception));
            Assert.IsNotNull(odm);
            Assert.IsNull(exception);


        }
    }
}
