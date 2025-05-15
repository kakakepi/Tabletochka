using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace T4bJl3T04K4
{
    [Table("diseases_symptoms")]
    public class DiseaseSymptom
    {
        [Key]
        [Column("diseases_id")]
        public Guid DiseaseId { get; set; }

        [Key]
        [Column("symptoms_id")]
        public Guid SymptomId { get; set; }

        [ForeignKey("DiseaseId")]
        public virtual Disease Disease { get; set; }

        [ForeignKey("SymptomId")]
        public virtual Symptom Symptom { get; set; }
    }
}
