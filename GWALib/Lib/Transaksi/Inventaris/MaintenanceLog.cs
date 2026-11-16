using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Inventaris
{
    /// <summary>
    /// Perawatan aset (t_maintenance_log). Aset per unit wajib menyebut unit. Alur status: Scheduled → InProgress → Done (Scheduled juga boleh langsung Done).
    /// Start: unit harus Tersedia lalu menjadi Perawatan; aset cukup jumlah tidak boleh ada yang dipinjam lalu aset menjadi Perawatan.
    /// Finish mengembalikan unit / aset ke Tersedia, opsional memperbarui kondisi, dan bila akun + kategori diisi mencatat biaya sebagai
    /// transaksi Expense Posted (TransactionId; dibatalkan hanya lewat perawatan). Ubah saat Scheduled / InProgress; hapus hanya Scheduled.
    /// </summary>
    public partial class MaintenanceLog : BaseLib
    {
        public class NewRow
        {
            public int AssetId { get; set; }
            public int? AssetUnitId { get; set; }
            public DateOnly MaintenanceDate { get; set; }
            public string Description { get; set; } = null!;
            public string? Vendor { get; set; }
            public decimal Cost { get; set; }
        }

        public int Id { get; set; }
        public int AssetId { get; private set; }
        public string AssetName { get; private set; } = null!;
        public int? AssetUnitId { get; private set; }
        public string? UnitCode { get; private set; }
        public DateOnly MaintenanceDate { get; set; }
        public string Description { get; set; } = null!;
        public string? Vendor { get; set; }
        public decimal Cost { get; set; }
        // Hanya-baca: diubah lewat Start / Finish.
        public MaintenanceStatus Status { get; private set; }
        public DateTime? FinishedAt { get; private set; }
        public int? TransactionId { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private static IQueryable<EF.TMaintenanceLog> _Querys
        {
            get
            {
                return DBClass.GetContext().TMaintenanceLogs.Include(x => x.Asset).Include(x => x.AssetUnit).AsQueryable();
            }
        }
        public MaintenanceLog()
        {
        }
        public MaintenanceLog(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<MaintenanceLog>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal MaintenanceLog(EF.TMaintenanceLog row)
        {
            SetData(row);
        }
        private void SetData(EF.TMaintenanceLog row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<MaintenanceLog>();
            AssetId = row.AssetId;
            AssetName = row.Asset?.Name ?? string.Empty;
            AssetUnitId = row.AssetUnitId;
            UnitCode = row.AssetUnit?.UnitCode;
            MaintenanceDate = row.MaintenanceDate;
            Description = row.Description;
            Vendor = row.Vendor;
            Cost = row.Cost;
            Status = (MaintenanceStatus)row.Status;
            FinishedAt = row.FinishedAt;
            TransactionId = row.TransactionId;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
