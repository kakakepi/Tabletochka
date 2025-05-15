using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace T4bJl3T04K4
{
    [Table("diseases")]
    public class Disease
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        [Column("name", TypeName = "varchar(50)")]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("description", TypeName = "varchar(100)")]
        public string Description { get; set; }

        public virtual ICollection<DiseaseSymptom> DiseaseSymptoms { get; set; } = new List<DiseaseSymptom>();
    }
}
