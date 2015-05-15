using System;

namespace PCF.OdmXml.i2b2Importer
{
    public static class Constants
    {
        //From https://github.com/CTMM-TraIT/trait_odm_to_i2b2/blob/452a1950b94d3a779eb66aaf1ad7ef34976c628c/src/main/java/com/recomdata/i2b2/IConstants.java
        //TODO: How many of these do we actually need?
        public const int C_HLEVEL_1 = 1;
        public const int C_HLEVEL_2 = 2;
        public const int C_HLEVEL_3 = 3;
        public const int C_HLEVEL_4 = 4;
        public const int C_HLEVEL_5 = 5;
        public const int C_HLEVEL_6 = 6;
        
        public const string C_COLUMNDATATYPE = "T";
        public const string C_COLUMNNAME = "concept_path";
        public const string C_FACTTABLECOLUMN = "concept_cd";
        public const string C_OPERATOR = "LIKE";
        public const string C_SYNONYM_CD = "N";
        public const string C_TABLENAME = "concept_dimension";
        public const string C_VISUALATTRIBUTES_FOLDER = "FA";
        public const string C_VISUALATTRIBUTES_LEAF = "LA";

        public const string VALUE_TYPE_TEXT = "T";
        public const string VALUE_TYPE_NUMBER = "N";

        //ODM1-3-2-Final.htm#t_datetime
        //YYYY-MM-DDThh:mm:ss(.n+)?(((+|-)hh:mm)|Z)?
        //https://msdn.microsoft.com/en-us/library/az4se3k1%28v=vs.110%29.aspx#Roundtrip
        public const string DATETIME_FORMAT = "O";
    }
}
