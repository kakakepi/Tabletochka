using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace T4bJl3T04K4
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        [Column("username", TypeName = "varchar(50)")]
        public string Username { get; set; }

        [MaxLength(50)]
        [Column("first_name", TypeName = "varchar(50)")]
        public string FirstName { get; set; }

        [MaxLength(50)]
        [Column("last_name", TypeName = "varchar(50)")]
        public string LastName { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("password_hash", TypeName = "varchar(255)")]
        public string PasswordHash { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("salt", TypeName = "varchar(100)")]
        public string Salt { get; set; }

        [Column("picture", TypeName = "varchar(255)")]
        public string? Picture { get; set; }

        [Column("admin")]
        public bool Admin { get; set; } = false;

        [Column("gender")]
        public bool Gender { get; set; }

        [Column("date_of_birth")]
        public DateTime? DateOfBirth { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<LoginHistory> LoginHistories { get; set; } = new List<LoginHistory>();

        public virtual ICollection<SearchHistory> SearchHistories { get; set; } = new List<SearchHistory>();
    }
}
