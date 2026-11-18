using System.ComponentModel.DataAnnotations;
using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Master.Anggota;
using GWALib.Lib.Transaksi.Anggota;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Organisasi
{
    /// <summary>
    /// Organisasi › Section: master section musik (m_section) dan riwayat section anggota (t_section_member).
    /// Semua anggota punya satu section aktif; pindah section menutup section lama dan membuka yang baru.
    /// Modal riwayat per anggota (RiwayatAnggota) juga dibuka dari halaman Data Anggota.
    /// </summary>
    public class SectionVM
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
            public string? Description { get; set; }
            public int SortOrder { get; set; }
            public int JumlahAnggota { get; set; }
            public bool Aktif { get; set; }
        }

        public class Add
        {
            public NewRow Row { get; set; } = new();
            public List<GwaVM.OpsiItem> AktifList { get; set; } = GwaVM.OpsiAktif;
        }

        public class Edit : Add
        {
            public Edit(string encKey)
            {
                var obj = LibExtensions.Load<Section>(encKey, "Section");
                Row = new NewRow { Name = obj.Name, Description = obj.Description, SortOrder = obj.SortOrder, IsActive = (int)obj.IsActive };
            }
        }

        public class NewRow
        {
            [Required(ErrorMessage = "Nama section harus diisi")]
            [StringLength(100, ErrorMessage = "Nama maksimal 100 karakter")]
            [Display(Name = "Nama Section")]
            public string? Name { get; set; }

            [StringLength(255, ErrorMessage = "Keterangan maksimal 255 karakter")]
            [Display(Name = "Keterangan")]
            public string? Description { get; set; }

            [Range(0, 9999, ErrorMessage = "Urutan 0 - 9999")]
            [Display(Name = "Urutan Tampil")]
            public int SortOrder { get; set; }

            [Display(Name = "Status")]
            public int IsActive { get; set; } = (int)ActiveStatus.Aktif;
        }

        // ---------- Anggota section (modal _Anggota + grid _AnggotaList): riwayat semua anggota yang pernah di section ini.
        public class AnggotaIndex
        {
            public string Key { get; set; } = "";
            public string Nama { get; set; } = "";
            public PindahInput Input { get; set; } = new() { StartDate = DateTime.Today };
            public List<GwaVM.OpsiItem> AnggotaOpsi { get; set; } = Opsi.Anggota();
            public RiwayatList List { get; set; }

            public AnggotaIndex(string encKey)
            {
                var obj = LibExtensions.Load<Section>(encKey, "Section");
                Key = obj.EncDecrKey;
                Nama = obj.Name;
                Input.SectionKey = obj.EncDecrKey;
                List = RiwayatList.OfSection(obj.Id);
            }
        }

        // ---------- Riwayat section satu anggota (modal _Riwayat + grid _RiwayatList)
        public class RiwayatIndex
        {
            public string MemberKey { get; set; } = "";
            public string Nama { get; set; } = "";
            public string? SectionSaatIni { get; set; }
            public PindahInput Input { get; set; } = new() { StartDate = DateTime.Today };
            public List<GwaVM.OpsiItem> SectionOpsi { get; set; } = Opsi.Section();
            public bool BisaPindah { get; set; }
            public RiwayatList List { get; set; }

            public RiwayatIndex(string memberKey)
            {
                var member = LibExtensions.Load<Member>(memberKey, "Anggota");
                MemberKey = member.EncDecrKey;
                Nama = member.Fullname;
                SectionSaatIni = member.CurrentSectionName;
                BisaPindah = member.Status == MemberStatus.Active;
                Input.MemberKey = member.EncDecrKey;
                List = RiwayatList.OfMember(member.Id);
            }
        }

        public class RiwayatList
        {
            /// <summary>true = grid per section (kolom anggota); false = grid per anggota (kolom section).</summary>
            public bool PerSection { get; set; }
            public List<RiwayatRow> Rows { get; set; } = new();

            public static RiwayatList OfSection(int sectionId) => new()
            {
                PerSection = true,
                Rows = SectionMember.GetDataList(sectionId: sectionId).Select(ToRow)
                    .OrderByDescending(x => x.Aktif).ThenBy(x => x.Anggota).ThenByDescending(x => x.StartDate).ToList()
            };

            public static RiwayatList OfMember(int memberId) => new()
            {
                PerSection = false,
                Rows = SectionMember.GetDataList(memberId: memberId).Select(ToRow).OrderByDescending(x => x.StartDate).ToList()
            };

            private static RiwayatRow ToRow(SectionMember x) => new()
            {
                EncDecrKey = x.EncDecrKey,
                Anggota = x.MemberName,
                NoAnggota = x.MemberNumber,
                Section = x.SectionName,
                StartDate = Opsi.KeDateTime(x.StartDate),
                EndDate = Opsi.KeDateTime(x.EndDate),
                Note = x.Note,
                Aktif = x.IsCurrent
            };
        }

        public class RiwayatRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Anggota { get; set; } = "";
            public string? NoAnggota { get; set; }
            public string Section { get; set; } = "";
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public string? Note { get; set; }
            /// <summary>Section aktif saat ini (EndDate kosong).</summary>
            public bool Aktif { get; set; }
        }

        /// <summary>Pindah section. Dari modal section: SectionKey + MemberId. Dari modal anggota: MemberKey + SectionId.</summary>
        public class PindahInput
        {
            public string? SectionKey { get; set; }
            public string? MemberKey { get; set; }

            [Display(Name = "Anggota")]
            public int? MemberId { get; set; }

            [Display(Name = "Pindah ke Section")]
            public int? SectionId { get; set; }

            [Required(ErrorMessage = "Tanggal mulai harus diisi")]
            [Display(Name = "Mulai Tanggal")]
            public DateTime? StartDate { get; set; }

            [StringLength(255, ErrorMessage = "Catatan maksimal 255 karakter")]
            [Display(Name = "Catatan / Event")]
            public string? Note { get; set; }
        }

        public class Method
        {
            public static IEnumerable<Row> GetDataList()
            {
                var counts = Section.GetMemberCounts();
                return Section.GetDataList(FilterAktif.Semua).Select(x => new Row
                {
                    EncDecrKey = x.EncDecrKey,
                    Name = x.Name,
                    Description = x.Description,
                    SortOrder = x.SortOrder,
                    JumlahAnggota = counts.TryGetValue(x.Id, out var n) ? n : 0,
                    Aktif = x.IsActive == ActiveStatus.Aktif
                }).OrderBy(x => x.SortOrder).ThenBy(x => x.Name);
            }

            public static void Simpan(NewRow input, string user)
            {
                Section.Save(new Section.NewRow { Name = input.Name ?? "", Description = input.Description, SortOrder = input.SortOrder }, user).Ok();
            }

            public static void Update(string encKey, NewRow input, string user)
            {
                var obj = LibExtensions.Load<Section>(encKey, "Section");
                obj.Name = input.Name ?? "";
                obj.Description = input.Description;
                obj.SortOrder = input.SortOrder;
                obj.IsActive = (ActiveStatus)input.IsActive;
                obj.Update(user).Ok();
            }

            public static void Nonaktifkan(string? encKey, string user)
            {
                LibExtensions.Load<Section>(encKey, "Section").Delete(user).Ok();
            }

            public static void Pindah(PindahInput input, string user)
            {
                var sectionId = !string.IsNullOrEmpty(input.SectionKey) ? LibExtensions.Load<Section>(input.SectionKey, "Section").Id : input.SectionId ?? 0;
                var memberId = !string.IsNullOrEmpty(input.MemberKey) ? LibExtensions.Load<Member>(input.MemberKey, "Anggota").Id : input.MemberId ?? 0;
                if (memberId <= 0)
                {
                    throw new ArgumentException("Pilih anggota.");
                }
                if (sectionId <= 0)
                {
                    throw new ArgumentException("Pilih section tujuan.");
                }
                SectionMember.Move(new SectionMember.NewRow
                {
                    MemberId = memberId,
                    SectionId = sectionId,
                    StartDate = Opsi.TanggalWajib(input.StartDate, "Mulai Tanggal"),
                    Note = input.Note
                }, user).Ok();
            }

            public static void HapusRiwayat(string? encKey, string user)
            {
                LibExtensions.Load<SectionMember>(encKey, "Riwayat section").Delete(user).Ok();
            }
        }
    }
}
