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

        [Column("name_en", TypeName = "varchar(50)")]
        public string NameEn { get; set; }

        [Column("name_ru", TypeName = "varchar(50)")]
        public string NameRu { get; set; }

        [Column("description_en", TypeName = "varchar(100)")]
        public string DescriptionEn { get; set; }

        [Column("description_ru", TypeName = "varchar(100)")]
        public string DescriptionRu { get; set; }

        public ICollection<DiseaseSymptom> DiseaseSymptoms { get; set; } = new List<DiseaseSymptom>();
    }

}