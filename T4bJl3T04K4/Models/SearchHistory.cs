using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace T4bJl3T04K4
{
    [Table("search_history")]
    public class SearchHistory
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [ForeignKey("User")]
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("search_date")]
        public DateTime SearchDate { get; set; } = DateTime.UtcNow;

        public virtual User User { get; set; }

        public virtual ICollection<SearchHistorySymptom> SearchHistorySymptoms { get; set; } = new List<SearchHistorySymptom>();
    }
}
