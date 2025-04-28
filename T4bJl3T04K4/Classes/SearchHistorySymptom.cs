using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace T4bJl3T04K4
{
    [Table("search_history_symptoms")]
    public class SearchHistorySymptom
    {
        [Key]
        [Column("search_history_id")]
        public Guid SearchHistoryId { get; set; }

        [Key]
        [Column("symptom_id")]
        public Guid SymptomId { get; set; }
        [ForeignKey("SearchHistoryId")]
        public SearchHistory SearchHistory { get; set; }

        [ForeignKey("SymptomId")]
        public Symptom Symptom { get; set; }
    }
}
