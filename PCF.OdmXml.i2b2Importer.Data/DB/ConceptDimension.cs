namespace PCF.OdmXml.i2b2Importer.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ConceptDimension
    {
        [Key]
        [StringLength(700)]
        public string CONCEPT_PATH { get; set; }

        [StringLength(50)]
        public string CONCEPT_CD { get; set; }

        [StringLength(2000)]
        public string NAME_CHAR { get; set; }

        [Column(TypeName = "text")]
        public string CONCEPT_BLOB { get; set; }

        public DateTime? UPDATE_DATE { get; set; }

        public DateTime? DOWNLOAD_DATE { get; set; }

        public DateTime? IMPORT_DATE { get; set; }

        [StringLength(50)]
        public string SOURCESYSTEM_CD { get; set; }

        public int? UPLOAD_ID { get; set; }
    }
}
