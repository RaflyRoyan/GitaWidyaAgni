using GWALib.Lib.Master.Anggota;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models
{
    /// <summary>
    /// Pilihan SelectBox yang dipakai banyak halaman (periode, anggota, divisi, jabatan).
    /// Value = id baris; dipakai sebagai PeriodId / MemberId / DivisionId / PositionId di form.
    /// </summary>
    public static class Opsi
    {
        public static List<GwaVM.OpsiItem> Periode(bool tanpaClosed = false)
        {
            return ManagementPeriod.GetDataList()
                .Where(x => !tanpaClosed || x.Status != ManagementPeriodStatus.Closed)
                .OrderByDescending(x => x.StartDate)
                .Select(x => new GwaVM.OpsiItem(x.Id, x.Name + (x.Status == ManagementPeriodStatus.Active ? " (aktif)" : "")))
                .ToList();
        }

        public static int? PeriodeAktifId() => ManagementPeriod.GetActive()?.Id;

        public static List<GwaVM.OpsiItem> Anggota(MemberStatus? status = MemberStatus.Active)
        {
            return Member.GetDataList(status)
                .OrderBy(x => x.Fullname)
                .Select(x => new GwaVM.OpsiItem(x.Id, x.Fullname + (string.IsNullOrEmpty(x.MemberNumber) ? "" : " · " + x.MemberNumber)))
                .ToList();
        }

        public static List<GwaVM.OpsiItem> Divisi()
        {
            return Division.GetDataList(FilterAktif.Aktif)
                .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
                .Select(x => new GwaVM.OpsiItem(x.Id, x.Name))
                .ToList();
        }

        public static List<GwaVM.OpsiItem> Section()
        {
            return GWALib.Lib.Master.Anggota.Section.GetDataList(FilterAktif.Aktif)
                .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
                .Select(x => new GwaVM.OpsiItem(x.Id, x.Name))
                .ToList();
        }

        public static List<GwaVM.OpsiItem> Jabatan()
        {
            return Position.GetDataList(FilterAktif.Aktif)
                .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
                .Select(x => new GwaVM.OpsiItem(x.Id, x.Name))
                .ToList();
        }

        public static List<GwaVM.OpsiItem> Program(int? periodId = null, bool hanyaTerbuka = false)
        {
            return GWALib.Lib.Transaksi.Program.WorkProgram.GetDataList(periodId)
                .Where(x => !hanyaTerbuka || x.Status == ProgramStatus.Planned || x.Status == ProgramStatus.Ongoing)
                .OrderByDescending(x => x.PeriodId).ThenBy(x => x.Title)
                .Select(x => new GwaVM.OpsiItem(x.Id, x.Title + " · " + x.PeriodName))
                .ToList();
        }

        // Koordinat kegiatan (id → [lat, lng]) untuk peta pemilih titik presensi.
        public static Dictionary<int, decimal[]> KoordinatKegiatan()
        {
            return GWALib.Lib.Transaksi.Program.Activity.GetDataList()
                .Where(x => x.Latitude != null && x.Longitude != null)
                .ToDictionary(x => x.Id, x => new[] { x.Latitude!.Value, x.Longitude!.Value });
        }

        public static List<GwaVM.OpsiItem> Kegiatan(bool hanyaTerbuka = false)
        {
            return GWALib.Lib.Transaksi.Program.Activity.GetDataList()
                .Where(x => !hanyaTerbuka || x.Status == ProgramStatus.Planned || x.Status == ProgramStatus.Ongoing)
                .OrderByDescending(x => x.StartAt)
                .Select(x => new GwaVM.OpsiItem(x.Id, x.Title + " · " + x.StartAt.ToString("dd MMM yyyy")))
                .ToList();
        }

        public static List<GwaVM.OpsiItem> Mitra()
        {
            return GWALib.Lib.Master.Mitra.ExternalParty.GetDataList(ExternalPartyStatus.Aktif)
                .OrderBy(x => x.Name)
                .Select(x => new GwaVM.OpsiItem(x.Id, x.Name))
                .ToList();
        }

        public static List<GwaVM.OpsiItem> Lomba()
        {
            return GWALib.Lib.Transaksi.Prestasi.Competition.GetDataList()
                .OrderByDescending(x => x.Id)
                .Select(x => new GwaVM.OpsiItem(x.Id, x.Name))
                .ToList();
        }

        public static List<GwaVM.OpsiItem> Akun()
        {
            return GWALib.Lib.Master.Keuangan.FinancialAccount.GetDataList(FilterAktif.Aktif)
                .OrderBy(x => x.Name)
                .Select(x => new GwaVM.OpsiItem(x.Id, x.Name))
                .ToList();
        }

        // Kategori aktif; Text diberi akhiran jenis bila tanpa filter.
        public static List<GwaVM.OpsiItem> Kategori(CashFlowType? type = null)
        {
            return GWALib.Lib.Master.Keuangan.FinancialCategory.GetDataList(FilterAktif.Aktif, type)
                .OrderBy(x => x.CategoryType).ThenBy(x => x.Name)
                .Select(x => new GwaVM.OpsiItem(x.Id, type.HasValue ? x.Name : x.Name + (x.CategoryType == CashFlowType.Income ? " (masuk)" : " (keluar)")))
                .ToList();
        }

        // Mata anggaran dari anggaran yang sudah Disetujui.
        public static List<GwaVM.OpsiItem> MataAnggaran()
        {
            return GWALib.Lib.Transaksi.Keuangan.Budget.GetDataList(status: BudgetStatus.Approved)
                .SelectMany(b => b.GetItems(true).Select(i => new GwaVM.OpsiItem(i.Id, i.Name + " · " + b.Name)))
                .ToList();
        }

        public static DateOnly? Tanggal(DateTime? value) => value.HasValue ? DateOnly.FromDateTime(value.Value) : null;
        public static DateOnly TanggalWajib(DateTime? value, string nama)
            => value.HasValue ? DateOnly.FromDateTime(value.Value) : throw new ArgumentException(nama + " harus diisi");
        public static DateTime? KeDateTime(DateOnly? value) => value?.ToDateTime(TimeOnly.MinValue);
    }
}
