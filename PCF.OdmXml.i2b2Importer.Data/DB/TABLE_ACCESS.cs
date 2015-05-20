namespace PCF.OdmXml.i2b2Importer.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TABLE_ACCESS
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(50)]
        public string C_TABLE_CD { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(50)]
        public string C_TABLE_NAME { get; set; }

        [StringLength(1)]
        public string C_PROTECTED_ACCESS { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int C_HLEVEL { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(700)]
        public string C_FULLNAME { get; set; }

        [Key]
        [Column(Order = 4)]
        [StringLength(2000)]
        public string C_NAME { get; set; }

        [Key]
        [Column(Order = 5)]
        [StringLength(1)]
        public string C_SYNONYM_CD { get; set; }

        [Key]
        [Column(Order = 6)]
        [StringLength(3)]
        public string C_VISUALATTRIBUTES { get; set; }

        public int? C_TOTALNUM { get; set; }

        [StringLength(50)]
        public string C_BASECODE { get; set; }

        [Column(TypeName = "text")]
        public string C_METADATAXML { get; set; }

        [Key]
        [Column(Order = 7)]
        [StringLength(50)]
        public string C_FACTTABLECOLUMN { get; set; }

        [Key]
        [Column(Order = 8)]
        [StringLength(50)]
        public string C_DIMTABLENAME { get; set; }

        [Key]
        [Column(Order = 9)]
        [StringLength(50)]
        public string C_COLUMNNAME { get; set; }

        [Key]
        [Column(Order = 10)]
        [StringLength(50)]
        public string C_COLUMNDATATYPE { get; set; }

        [Key]
        [Column(Order = 11)]
        [StringLength(10)]
        public string C_OPERATOR { get; set; }

        [Key]
        [Column(Order = 12)]
        [StringLength(700)]
        public string C_DIMCODE { get; set; }

        [Column(TypeName = "text")]
        public string C_COMMENT { get; set; }

        [StringLength(900)]
        public string C_TOOLTIP { get; set; }

        public DateTime? C_ENTRY_DATE { get; set; }

        public DateTime? C_CHANGE_DATE { get; set; }

        [StringLength(1)]
        public string C_STATUS_CD { get; set; }

        [StringLength(50)]
        public string VALUETYPE_CD { get; set; }
    }
}
