using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public virtual SearchHistory SearchHistory { get; set; }

        [ForeignKey("SymptomId")]
        public virtual Symptom Symptom { get; set; }
    }
}
