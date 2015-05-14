using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCF.OdmXml.i2b2Importer
{

    /// <summary>
    /// An interface for ODMXML importers
    /// </summary>
    public interface IOdmImporter
    {
        /// <summary>
        /// Takes a populated ODM model and inserts the data into an i2b2 database
        /// </summary>
        /// <param name="odm">Fully populated ODMXML model</param>
        /// <returns></returns>
        Task ImportAsync(ODM odm, IDictionary<string, string> settings);
    }
}
