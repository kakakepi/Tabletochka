using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace T4bJl3T04K4
{
    [Table("symptoms")]
    public class Symptom
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("name_en", TypeName = "varchar(100)")]
        public string NameEn { get; set; }

        [Column("name_ru", TypeName = "varchar(100)")]
        public string NameRu { get; set; }
        [Column("system")]
        public string System { get; set; }


        public ICollection<DiseaseSymptom> DiseaseSymptoms { get; set; } = new List<DiseaseSymptom>();
        public ICollection<SearchHistorySymptom> SearchHistorySymptoms { get; set; } = new List<SearchHistorySymptom>();
    }

}