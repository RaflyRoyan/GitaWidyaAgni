using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_budget_item")]
[Index("BudgetId", Name = "fk_budgetitem_budget")]
[Index("CategoryId", Name = "fk_budgetitem_category")]
[Index("ProgramId", Name = "fk_budgetitem_program")]
public partial class TBudgetItem
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("budget_id")]
    public int BudgetId { get; set; }

    [Column("category_id")]
    public int CategoryId { get; set; }

    [Column("program_id")]
    public int? ProgramId { get; set; }

    [Column("name")]
    [StringLength(200)]
    public string Name { get; set; } = null!;

    [Column("quantity")]
    [Precision(12, 2)]
    public decimal Quantity { get; set; }

    [Column("unit")]
    [StringLength(30)]
    public string? Unit { get; set; }

    [Column("unit_price")]
    [Precision(15, 2)]
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// quantity x unit_price
    /// </summary>
    [Column("amount")]
    [Precision(15, 2)]
    public decimal Amount { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("BudgetId")]
    [InverseProperty("TBudgetItems")]
    public virtual TBudget Budget { get; set; } = null!;

    [ForeignKey("CategoryId")]
    [InverseProperty("TBudgetItems")]
    public virtual MFinancialCategory Category { get; set; } = null!;

    [ForeignKey("ProgramId")]
    [InverseProperty("TBudgetItems")]
    public virtual TProgram? Program { get; set; }

    [InverseProperty("BudgetItem")]
    public virtual ICollection<TFinancialTransaction> TFinancialTransactions { get; set; } = new List<TFinancialTransaction>();

    [InverseProperty("BudgetItem")]
    public virtual ICollection<TFundingRequest> TFundingRequests { get; set; } = new List<TFundingRequest>();

    [InverseProperty("BudgetItem")]
    public virtual ICollection<TProcurementRequest> TProcurementRequests { get; set; } = new List<TProcurementRequest>();
}
