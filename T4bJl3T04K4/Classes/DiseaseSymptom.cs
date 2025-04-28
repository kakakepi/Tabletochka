using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

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
        public Disease Disease { get; set; }

        [ForeignKey("SymptomId")]
        public Symptom Symptom { get; set; }
    }
}