using System.ComponentModel.DataAnnotations;
using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Transaksi.Anggota;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Organisasi
{
    /// <summary>
    /// Organisasi › Kepengurusan: penugasan anggota ke jabatan per periode (t_management_assignment).
    /// Jabatan inti hanya satu pemegang berjalan per (periode, jabatan, divisi). Penugasan yang sudah diakhiri terkunci.
    /// </summary>
    public class KepengurusanVM
    {
        public class Index
        {
        }

        public class List
        {
            public List<Row> Rows { get; set; } = Method.GetDataList().ToList();
        }

        public class Row
        {
            public string EncDecrKey { get; set; } = "";
            public string Periode { get; set; } = "";
            public string Anggota { get; set; } = "";
            public string Jabatan { get; set; } = "";
            public bool Inti { get; set; }
            public string? Divisi { get; set; }
            public string? SkNumber { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public bool Berjalan { get; set; }
            // Untuk inactiveRowPrepared: penugasan lampau ditandai.
            public bool Aktif => Berjalan;
        }

        public class Add
        {
            public NewRow Row { get; set; } = new();
            public List<GwaVM.OpsiItem> PeriodeOpsi { get; set; } = Opsi.Periode(true);
            public List<GwaVM.OpsiItem> AnggotaOpsi { get; set; } = Opsi.Anggota();
            public List<GwaVM.OpsiItem> JabatanOpsi { get; set; } = Opsi.Jabatan();
            public List<GwaVM.OpsiItem> DivisiOpsi { get; set; } = Opsi.Divisi();

            public Add()
            {
                Row.PeriodId = Opsi.PeriodeAktifId();
                Row.StartDate = DateTime.Today;
            }
        }

        /// <summary>Edit: periode, anggota, dan jabatan tetap; yang bisa diubah divisi, SK, dan tanggal.</summary>
        public class Edit : Add
        {
            public string Anggota { get; set; } = "";
            public string Jabatan { get; set; } = "";
            public string Periode { get; set; } = "";

            public Edit(string encKey)
            {
                var obj = LibExtensions.Load<ManagementAssignment>(encKey, "Penugasan");
                if (obj.IsCurrent != AssignmentPeriod.Berjalan)
                {
                    throw new ArgumentException("Penugasan yang sudah diakhiri tidak bisa diubah.");
                }
                Anggota = obj.MemberName;
                Jabatan = obj.PositionName;
                Periode = obj.PeriodName;
                Row = new NewRow
                {
                    PeriodId = obj.PeriodId,
                    MemberId = obj.MemberId,
                    PositionId = obj.PositionId,
                    DivisionId = obj.DivisionId,
                    SkNumber = obj.SkNumber,
                    StartDate = Opsi.KeDateTime(obj.StartDate),
                    EndDate = Opsi.KeDateTime(obj.EndDate)
                };
            }
        }

        public class NewRow
        {
            [Required(ErrorMessage = "Pilih periode")]
            [Display(Name = "Periode")]
            public int? PeriodId { get; set; }

            [Required(ErrorMessage = "Pilih anggota")]
            [Display(Name = "Anggota")]
            public int? MemberId { get; set; }

            [Required(ErrorMessage = "Pilih jabatan")]
            [Display(Name = "Jabatan")]
            public int? PositionId { get; set; }

            [Display(Name = "Divisi yang Dipimpin")]
            public int? DivisionId { get; set; }

            [StringLength(100, ErrorMessage = "Nomor SK maksimal 100 karakter")]
            [Display(Name = "Nomor SK")]
            public string? SkNumber { get; set; }

            [Required(ErrorMessage = "Tanggal mulai harus diisi")]
            [Display(Name = "Tanggal Mulai")]
            public DateTime? StartDate { get; set; }

            [Display(Name = "Tanggal Selesai (rencana)")]
            public DateTime? EndDate { get; set; }
        }

        public class AkhiriInput
        {
            public string Key { get; set; } = "";
            public DateTime? EndDate { get; set; }
        }

        public class Method
        {
            public static IEnumerable<Row> GetDataList()
            {
                return ManagementAssignment.GetDataList().Select(x => new Row
                {
                    EncDecrKey = x.EncDecrKey,
                    Periode = x.PeriodName,
                    Anggota = x.MemberName,
                    Jabatan = x.PositionName,
                    Inti = x.PositionLevel == PositionLevel.Inti,
                    Divisi = x.DivisionName,
                    SkNumber = x.SkNumber,
                    StartDate = x.StartDate.ToDateTime(TimeOnly.MinValue),
                    EndDate = Opsi.KeDateTime(x.EndDate),
                    Berjalan = x.IsCurrent == AssignmentPeriod.Berjalan
                }).OrderByDescending(x => x.Berjalan).ThenByDescending(x => x.StartDate).ThenBy(x => x.Anggota);
            }

            public static void Simpan(NewRow input, string user)
            {
                ManagementAssignment.Save(new ManagementAssignment.NewRow
                {
                    PeriodId = input.PeriodId ?? 0,
                    MemberId = input.MemberId ?? 0,
                    PositionId = input.PositionId ?? 0,
                    DivisionId = input.DivisionId,
                    SkNumber = input.SkNumber,
                    StartDate = Opsi.TanggalWajib(input.StartDate, "Tanggal mulai"),
                    EndDate = Opsi.Tanggal(input.EndDate)
                }, user).Ok();
            }

            public static void Update(string encKey, NewRow input, string user)
            {
                var obj = LibExtensions.Load<ManagementAssignment>(encKey, "Penugasan");
                obj.DivisionId = input.DivisionId;
                obj.SkNumber = input.SkNumber;
                obj.StartDate = Opsi.TanggalWajib(input.StartDate, "Tanggal mulai");
                obj.EndDate = Opsi.Tanggal(input.EndDate);
                obj.Update(user).Ok();
            }

            public static void Akhiri(AkhiriInput input, string user)
            {
                LibExtensions.Load<ManagementAssignment>(input.Key, "Penugasan").End(Opsi.Tanggal(input.EndDate), user).Ok();
            }

            public static void Hapus(string? encKey, string user)
            {
                LibExtensions.Load<ManagementAssignment>(encKey, "Penugasan").Delete(user).Ok();
            }
        }
    }
}
