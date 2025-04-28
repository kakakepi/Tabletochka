using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace T4bJl3T04K4
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("username", TypeName = "varchar(50)")]
        public string Username { get; set; }

        [Column("first_name", TypeName = "varchar(50)")]
        public string FirstName { get; set; }

        [Column("last_name", TypeName = "varchar(50)")]
        public string LastName { get; set; }

        [Required]
        [Column("password_hash", TypeName = "varchar(255)")]
        public string PasswordHash { get; set; }

        [Required]
        [Column("salt", TypeName = "varchar(100)")]
        public string Salt { get; set; }

        [Column("picture", TypeName = "varchar")]
        public string Picture { get; set; }

        [Column("admin")]
        public bool Admin { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<LoginHistory> LoginHistories { get; set; } = new List<LoginHistory>();
        public ICollection<SearchHistory> SearchHistories { get; set; } = new List<SearchHistory>();
    }
}