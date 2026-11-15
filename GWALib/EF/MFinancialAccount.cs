using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("m_financial_account")]
public partial class MFinancialAccount
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 1=CASH, 2=BANK, 3=E_WALLET
    /// </summary>
    [Column("account_type")]
    public int AccountType { get; set; }

    [Column("bank_name")]
    [StringLength(100)]
    public string? BankName { get; set; }

    [Column("account_number")]
    [StringLength(50)]
    public string? AccountNumber { get; set; }

    [Column("account_holder")]
    [StringLength(150)]
    public string? AccountHolder { get; set; }

    [Column("opening_balance")]
    [Precision(15, 2)]
    public decimal OpeningBalance { get; set; }

    /// <summary>
    /// 0=Tidak, 1=Ya
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

    [InverseProperty("Account")]
    public virtual ICollection<TFinancialTransaction> TFinancialTransactions { get; set; } = new List<TFinancialTransaction>();

    [InverseProperty("Account")]
    public virtual ICollection<TPeriodBalance> TPeriodBalances { get; set; } = new List<TPeriodBalance>();
}
