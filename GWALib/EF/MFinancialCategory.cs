using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("m_financial_category")]
[Index("CategoryType", "Name", Name = "uq_fincat_name", IsUnique = true)]
public partial class MFinancialCategory
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 1=INCOME, 2=EXPENSE
    /// </summary>
    [Column("category_type")]
    public int CategoryType { get; set; }

    /// <summary>
    /// 0=Tidak Aktif, 1=Aktif
    /// </summary>
    [Column("is_active")]
    public int IsActive { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [InverseProperty("Category")]
    public virtual ICollection<MDuesSetting> MDuesSettings { get; set; } = new List<MDuesSetting>();

    [InverseProperty("Category")]
    public virtual ICollection<TBudgetItem> TBudgetItems { get; set; } = new List<TBudgetItem>();

    [InverseProperty("Category")]
    public virtual ICollection<TFinancialTransaction> TFinancialTransactions { get; set; } = new List<TFinancialTransaction>();
}
