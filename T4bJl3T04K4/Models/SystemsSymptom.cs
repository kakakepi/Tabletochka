using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("systems_symptoms")]
public class SystemsSymptom
{
    [Key]
    public Guid Id { get; set; }

    [Column("system_name")]
    public string SystemName { get; set; }

    [Column("symptom_id")]
    public Guid SymptomId { get; set; }
}
