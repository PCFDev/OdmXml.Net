using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PCF.OdmXml.i2b2Importer.Data;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;
using System.Data.Entity.Infrastructure;

namespace PCF.OdmXml.i2b2Importer.Tests
{
    [TestClass]
    public class EFTests
    {
        [TestMethod]
        public void Observation_Fact_Insert_One_Record()
        {

            using (var context = new I2b2DbContext())
            {
                var observations = context.ObservationFacts;
                var currentDate = DateTime.UtcNow;


                var observation = observations.Create();

                observation.ENCOUNTER_NUM = 1;
                observation.PATIENT_NUM = 1;
                observation.CONCEPT_CD = UnicodeEncoding.Default.GetString(MD5.Create().ComputeHash(UnicodeEncoding.Default.GetBytes("test this hash")));
                observation.PROVIDER_ID = "@"; //HACK where is the provider?
                observation.START_DATE = currentDate - TimeSpan.FromDays(3);//???
                observation.MODIFIER_CD = "@";
                observation.INSTANCE_NUM = 1;


                observation.VALTYPE_CD = "N";
                observation.TVAL_CHAR = "E";
                observation.NVAL_NUM = decimal.Parse("53.9");

                observation.VALUEFLAG_CD = "";
                observation.QUANTITY_NUM = null;
                observation.UNITS_CD = "";

                observation.END_DATE = currentDate;
                observation.LOCATION_CD = "@";
               
                observation.OBSERVATION_BLOB = string.Empty;

                observation.CONFIDENCE_NUM = null;

                
                observation.UPDATE_DATE = DateTime.Now;
                observation.DOWNLOAD_DATE = DateTime.Now;                               
                observation.IMPORT_DATE =  currentDate;                             
                
                observation.SOURCESYSTEM_CD = "TEST";
                               
                observation.UPLOAD_ID = 0;
                
                
                observations.Add(observation);

                try
                {

                    var numOfChanges = context.SaveChanges();

                    Assert.AreEqual(1, numOfChanges);

                }
                catch  (DbUpdateConcurrencyException dbEx)
                {
                    Assert.Fail(dbEx.Message);

                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error Saving Observations: " + ex.Message);
                    Debugger.Break();
                    Assert.Fail(ex.Message);
                }
            }
        }
    }}
