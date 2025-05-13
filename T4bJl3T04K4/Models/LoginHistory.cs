using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace T4bJl3T04K4
{
    [Table("login_history")]
    public class LoginHistory
    {
        [Key]
        [Column("login_id")]
        public Guid LoginId { get; set; } = Guid.NewGuid();

        [ForeignKey("User")]
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("login_time")]
        public DateTime LoginTime { get; set; } = DateTime.UtcNow;

        [Column("is_successful")]
        public bool IsSuccessful { get; set; }
        public User User { get; set; }
    }
}