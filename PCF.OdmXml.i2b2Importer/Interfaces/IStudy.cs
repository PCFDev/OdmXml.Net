using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCF.OdmXml.i2b2Importer.Interfaces
{
    public interface IStudy
    {
        int HLevel { get; set; }
        string FullName { get; set; }
        string Name { get; set; }
        string SynonymCD { get; set; }
        string VisualAttributes { get; set; }
        int TotalNumber { get; set; }
        string BaseCode { get; set; }
        string MetaDataXML { get; set; }
        string FactTableColumn { get; set; }
        string TableName { get; set; }
        string ColumnName { get; set; }
        string ColumnDataType { get; set; }
        string Operator { get; set; }
        string DimCode { get; set; }
        string Comment { get; set; }
        string Tooltip { get; set; }
        string AppliedPath { get; set; }
        DateTime UpdateDate { get; set; }
        DateTime DownloadDate { get; set; }
        DateTime ImportDate { get; set; }
        string SourceSystemCD { get; set; }
        string ValueTypeCD { get; set; }
    }
}
