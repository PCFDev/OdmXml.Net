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
        /// <param name="settings"></param>
        /// <returns></returns>
        public async Task ImportAsync(ODM odm, IDictionary<string, string> settings)
        {
            try
            {
                var processor = new I2b2OdmProcessor(odm, settings);
                await Task.Run(() => processor.ProcessODM());
            }
            catch (Exception ex)
            {
                //log
                throw;
            }
        }

        /// <summary>
        /// Takes a populated ODM model and inserts the data into an i2b2 database
        /// </summary>
        /// <param name="odm">Fully populated ODMXML model</param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public void Import(ODM odm, IDictionary<string, string> settings)
        {

            try
            {
                var processor = new I2b2OdmProcessor(odm, settings);
                processor.ProcessODM();
            }
            catch (Exception ex)
            {
                //log
                throw;
            }
        }

    }
}
