using System;

namespace PCF.OdmXml.i2b2Importer.DTO
{
    public class I2B2StudyInfo
    {
        private I2B2StudyInfo()
        {
            var currentDate = DateTime.UtcNow;

            MappliedPath = "@";
            UpdateDate = currentDate;
            DownloadDate = currentDate;
            ImportDate = currentDate;
        }

        public I2B2StudyInfo(int hlevel, string fullname, string name, string dimcode, string tooltip, string visualAttributes)
            : this()
        {
            Cdimcode = dimcode;
            Cfullname = fullname;
            Chlevel = hlevel;
            Cname = name;
            Ctooltip = tooltip;
            CvisualAttributes = visualAttributes;

            //private setters?
            CcolumnDatatype = Constants.C_COLUMNDATATYPE;
            Ccolumnname = Constants.C_COLUMNNAME;
            CfactTableColumn = Constants.C_FACTTABLECOLUMN;
            Coperator = Constants.C_OPERATOR;
            CsynonmCd = Constants.C_SYNONYM_CD;
            Ctablename = Constants.C_TABLENAME;
        }

        //TODO: Better names
        public string Cbasecode { get; set; }
        public string CcolumnDatatype { get; set; }
        public string Ccolumnname { get; set; }
        public string Ccomment { get; set; }
        public string Cdimcode { get; set; }
        public string CfactTableColumn { get; set; }
        public string Cfullname { get; set; }
        public int Chlevel { get; set; }
        public string Cmetadataxml { get; set; }
        public string Cname { get; set; }
        public string Coperator { get; set; }
        public string CsynonmCd { get; set; }
        public string Ctablename { get; set; }
        public string Ctooltip { get; set; }
        public int CtotalNum { get; set; }
        public string CvisualAttributes { get; set; }
        public DateTime? DownloadDate { get; set; }//non nullable?
        public DateTime? ImportDate { get; set; }//non nullable?
        public string MappliedPath { get; set; }
        public string SourceSystemCd { get; set; }
        public DateTime? UpdateDate { get; set; }//non nullable?
        public string Valuetype { get; set; }

        public override string ToString()
        {
            return "I2B2StudyInfo [cbasecode=" + Cbasecode + ", cdimcode=" + Cdimcode + ", chlevel=" + Chlevel + ", cname=" + Cname + "]";
        }
    }
}
