using System;
using System.Collections.Generic;
using System.Threading.Tasks;

//License?
namespace PCF.OdmXml.i2b2Importer
{
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
            var processor = new I2b2OdmProcessor(odm, settings);

            try
            {
                await Task.Run(() => processor.ProcessODM());
            }
            catch (Exception ex)
            {
                //log
                throw;
            }
        }
    }
}
