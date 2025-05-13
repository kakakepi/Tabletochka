using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace T4bJl3T04K4
{
    [Table("diseases")]
    public class Disease
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("name", TypeName = "varchar(50)")]
        public string Name { get; set; }

        [Column("description", TypeName = "varchar(100)")]
        public string Description { get; set; }
        public ICollection<DiseaseSymptom> DiseaseSymptoms { get; set; } = new List<DiseaseSymptom>();
    }
}