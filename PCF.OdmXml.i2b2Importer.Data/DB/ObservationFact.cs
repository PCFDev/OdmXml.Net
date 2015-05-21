namespace PCF.OdmXml.i2b2Importer.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ObservationFact
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ENCOUNTER_NUM { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PATIENT_NUM { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(50)]
        public string CONCEPT_CD { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(50)]
        public string PROVIDER_ID { get; set; }

        [Key]
        [Column(Order = 4)]
        public DateTime START_DATE { get; set; }

        [Key]
        [Column(Order = 5)]
        [StringLength(100)]
        public string MODIFIER_CD { get; set; }

        [Key]
        [Column(Order = 6)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int INSTANCE_NUM { get; set; }

        [StringLength(50)]
        public string VALTYPE_CD { get; set; }

        [StringLength(255)]
        public string TVAL_CHAR { get; set; }

        public decimal? NVAL_NUM { get; set; }

        [StringLength(50)]
        public string VALUEFLAG_CD { get; set; }

        public decimal? QUANTITY_NUM { get; set; }

        [StringLength(50)]
        public string UNITS_CD { get; set; }

        public DateTime? END_DATE { get; set; }

        [StringLength(50)]
        public string LOCATION_CD { get; set; }

        [Column(TypeName = "text")]
        public string OBSERVATION_BLOB { get; set; }

        public decimal? CONFIDENCE_NUM { get; set; }

        public DateTime? UPDATE_DATE { get; set; }

        public DateTime? DOWNLOAD_DATE { get; set; }

        public DateTime? IMPORT_DATE { get; set; }

        [StringLength(50)]
        public string SOURCESYSTEM_CD { get; set; }

        public int? UPLOAD_ID { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TEXT_SEARCH_INDEX { get; set; }
    }
}
