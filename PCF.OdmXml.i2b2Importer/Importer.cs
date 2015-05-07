using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCF.OdmXml.i2b2Importer
{
    public class Importer
    {
        public async Task Import(string filePath)
        {
            var odm = ODM.Load(filePath);

            

        }
    }
}
