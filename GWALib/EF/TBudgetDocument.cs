using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_budget_document")]
[Index("BudgetId", Name = "fk_budgetdoc_budget")]
public partial class TBudgetDocument
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("budget_id")]
    public int BudgetId { get; set; }

    [Column("file_name")]
    [StringLength(255)]
    public string FileName { get; set; } = null!;

    [Column("file_type")]
    [StringLength(100)]
    public string? FileType { get; set; }

    [Column("file_data", TypeName = "mediumblob")]
    public byte[] FileData { get; set; } = null!;

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("BudgetId")]
    [InverseProperty("TBudgetDocuments")]
    public virtual TBudget Budget { get; set; } = null!;
}
