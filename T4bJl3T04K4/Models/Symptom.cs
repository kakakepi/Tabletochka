using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace T4bJl3T04K4
{
    [Table("symptoms")]
    public class Symptom
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        [Column("name", TypeName = "varchar(100)")]
        public string Name { get; set; }

        public virtual ICollection<DiseaseSymptom> DiseaseSymptoms { get; set; } = new List<DiseaseSymptom>();

        public virtual ICollection<SearchHistorySymptom> SearchHistorySymptoms { get; set; } = new List<SearchHistorySymptom>();
    }
}
