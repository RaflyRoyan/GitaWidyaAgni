using System.ComponentModel.DataAnnotations;
using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Master.Anggota;
using GWALib.Lib.Transaksi.Anggota;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Organisasi
{
    /// <summary>Organisasi › Divisi: master divisi / tim (m_division) dan anggotanya (t_division_member).</summary>
    public class DivisiVM
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
            public string Name { get; set; } = "";
            public string Jenis { get; set; } = "";
            public string? Description { get; set; }
            public int SortOrder { get; set; }
            public int JumlahAnggota { get; set; }
            public bool Aktif { get; set; }
        }

        public class Add
        {
            public NewRow Row { get; set; } = new();
            public List<GwaVM.OpsiItem> JenisList { get; set; } = GwaVM.Opsi<DivisionType>();
            public List<GwaVM.OpsiItem> AktifList { get; set; } = GwaVM.OpsiAktif;
        }

        public class Edit : Add
        {
            public Edit(string encKey)
            {
                var obj = LibExtensions.Load<Division>(encKey, "Divisi");
                Row = new NewRow { Name = obj.Name, DivisionType = (int)obj.DivisionType, Description = obj.Description, SortOrder = obj.SortOrder, IsActive = (int)obj.IsActive };
            }
        }

        public class NewRow
        {
            [Required(ErrorMessage = "Nama divisi harus diisi")]
            [StringLength(100, ErrorMessage = "Nama maksimal 100 karakter")]
            [Display(Name = "Nama Divisi")]
            public string? Name { get; set; }

            [Display(Name = "Jenis")]
            public int DivisionType { get; set; } = (int)GWAShared.Enums.DivisionType.Divisi;

            [StringLength(255, ErrorMessage = "Keterangan maksimal 255 karakter")]
            [Display(Name = "Keterangan")]
            public string? Description { get; set; }

            [Range(0, 9999, ErrorMessage = "Urutan 0 - 9999")]
            [Display(Name = "Urutan Tampil")]
            public int SortOrder { get; set; }

            [Display(Name = "Status")]
            public int IsActive { get; set; } = (int)ActiveStatus.Aktif;
        }

        // ---------- Anggota divisi (modal _Anggota + grid _AnggotaList)
        public class AnggotaIndex
        {
            public string Key { get; set; } = "";
            public string Nama { get; set; } = "";
            public AnggotaInput Input { get; set; } = new();
            public List<GwaVM.OpsiItem> AnggotaOpsi { get; set; } = Opsi.Anggota();
            public List<GwaVM.OpsiItem> PeriodeOpsi { get; set; } = Opsi.Periode(true);
            public AnggotaList List { get; set; }

            public AnggotaIndex(string encKey)
            {
                var obj = LibExtensions.Load<Division>(encKey, "Divisi");
                Key = obj.EncDecrKey;
                Nama = obj.Name;
                Input.PeriodId = Opsi.PeriodeAktifId();
                List = new AnggotaList(obj.Id);
            }
        }

        public class AnggotaList
        {
            public List<AnggotaRow> Rows { get; set; }
            public AnggotaList(int divisionId)
            {
                Rows = DivisionMember.GetDataList(FilterAktif.Semua, divisionId).Select(x => new AnggotaRow
                {
                    EncDecrKey = x.EncDecrKey,
                    Nama = x.MemberName,
                    Peran = x.MemberRole,
                    Periode = x.PeriodName,
                    JoinedAt = Opsi.KeDateTime(x.JoinedAt),
                    LeftAt = Opsi.KeDateTime(x.LeftAt),
                    Aktif = x.IsActive == ActiveStatus.Aktif
                }).OrderByDescending(x => x.Aktif).ThenBy(x => x.Nama).ToList();
            }
            public AnggotaList(string encKey) : this(LibExtensions.Load<Division>(encKey, "Divisi").Id)
            {
            }
        }

        public class AnggotaRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Nama { get; set; } = "";
            public string Peran { get; set; } = "";
            public string? Periode { get; set; }
            public DateTime? JoinedAt { get; set; }
            public DateTime? LeftAt { get; set; }
            public bool Aktif { get; set; }
        }

        public class AnggotaInput
        {
            public string Key { get; set; } = "";

            [Required(ErrorMessage = "Pilih anggota")]
            [Display(Name = "Anggota")]
            public int? MemberId { get; set; }

            [Display(Name = "Periode")]
            public int? PeriodId { get; set; }

            [StringLength(50, ErrorMessage = "Peran maksimal 50 karakter")]
            [Display(Name = "Peran")]
            public string? MemberRole { get; set; } = DivisionMember.DEFAULT_ROLE;

            [Display(Name = "Tanggal Bergabung")]
            public DateTime? JoinedAt { get; set; }
        }

        public class Method
        {
            public static IEnumerable<Row> GetDataList()
            {
                var members = DivisionMember.GetDataList(FilterAktif.Aktif)
                    .GroupBy(x => x.DivisionId).ToDictionary(g => g.Key, g => g.Count());
                return Division.GetDataList(FilterAktif.Semua).Select(x => new Row
                {
                    EncDecrKey = x.EncDecrKey,
                    Name = x.Name,
                    Jenis = EnumHelper.Text(x.DivisionType),
                    Description = x.Description,
                    SortOrder = x.SortOrder,
                    JumlahAnggota = members.TryGetValue(x.Id, out var n) ? n : 0,
                    Aktif = x.IsActive == ActiveStatus.Aktif
                }).OrderBy(x => x.SortOrder).ThenBy(x => x.Name);
            }

            public static void Simpan(NewRow input, string user)
            {
                Division.Save(new Division.NewRow
                {
                    Name = input.Name ?? "",
                    DivisionType = (DivisionType)input.DivisionType,
                    Description = input.Description,
                    SortOrder = input.SortOrder
                }, user).Ok();
            }

            public static void Update(string encKey, NewRow input, string user)
            {
                var obj = LibExtensions.Load<Division>(encKey, "Divisi");
                obj.Name = input.Name ?? "";
                obj.DivisionType = (DivisionType)input.DivisionType;
                obj.Description = input.Description;
                obj.SortOrder = input.SortOrder;
                obj.IsActive = (ActiveStatus)input.IsActive;
                obj.Update(user).Ok();
            }

            public static void Nonaktifkan(string? encKey, string user)
            {
                LibExtensions.Load<Division>(encKey, "Divisi").Delete(user).Ok();
            }

            public static void TambahAnggota(AnggotaInput input, string user)
            {
                var division = LibExtensions.Load<Division>(input.Key, "Divisi");
                DivisionMember.Save(new DivisionMember.NewRow
                {
                    DivisionId = division.Id,
                    MemberId = input.MemberId ?? 0,
                    PeriodId = input.PeriodId,
                    MemberRole = string.IsNullOrWhiteSpace(input.MemberRole) ? DivisionMember.DEFAULT_ROLE : input.MemberRole.Trim().ToUpperInvariant(),
                    JoinedAt = Opsi.Tanggal(input.JoinedAt)
                }, user).Ok();
            }

            public static void KeluarkanAnggota(string? encKey, string user)
            {
                LibExtensions.Load<DivisionMember>(encKey, "Anggota divisi").Delete(user).Ok();
            }
        }
    }
}
