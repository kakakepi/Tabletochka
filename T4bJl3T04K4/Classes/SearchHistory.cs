using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace T4bJl3T04K4
{
    [Table("search_history")]
    public class SearchHistory
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [ForeignKey("User")]
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("search_date")]
        public DateTime SearchDate { get; set; } = DateTime.UtcNow;
        public User User { get; set; }
        public ICollection<SearchHistorySymptom> SearchHistorySymptoms { get; set; } = new List<SearchHistorySymptom>();
    }
}