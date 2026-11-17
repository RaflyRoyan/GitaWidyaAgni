using System.ComponentModel.DataAnnotations;
using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Master.Akses;
using GWALib.Lib.Master.Anggota;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Anggota
{
    /// <summary>
    /// Anggota › Data Anggota (m_member + m_member_photo). NIM, nomor anggota, dan akun user unik.
    /// Hapus = nonaktifkan (status Tidak Aktif + tanggal keluar).
    /// </summary>
    public class AnggotaVM
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
            public string Fullname { get; set; } = "";
            public string? MemberNumber { get; set; }
            public string? Nim { get; set; }
            public string Kategori { get; set; } = "";
            public string? Prodi { get; set; }
            public short? Angkatan { get; set; }
            public string Tipe { get; set; } = "";
            public string StatusText { get; set; } = "";
            public string? Username { get; set; }
            public string? Phone { get; set; }
            public string? Email { get; set; }
            public DateTime? JoinedAt { get; set; }
            public int JumlahFoto { get; set; }
            public int? FotoId { get; set; }
            public bool Aktif { get; set; }
        }

        public class Add
        {
            public NewRow Row { get; set; } = new();
            public List<GwaVM.OpsiItem> TipeList { get; set; } = GwaVM.Opsi<MemberType>();
            public List<GwaVM.OpsiItem> KategoriList { get; set; } = GwaVM.Opsi<MemberCategory>();
            public List<GwaVM.OpsiItem> GenderList { get; set; } = GwaVM.Opsi<Gender>();
            public List<GwaVM.OpsiItem> StatusList { get; set; } = GwaVM.Opsi<MemberStatus>();
            public List<GwaVM.OpsiItem> UserList { get; set; }

            public Add() : this(null)
            {
                Row.JoinedAt = DateTime.Today;
            }

            protected Add(int? userIdSendiri)
            {
                // Akun user aktif yang belum dipakai anggota lain (+ akun anggota ini sendiri saat Edit).
                var dipakai = Member.GetDataList().Where(x => x.User != null && x.User.Id != userIdSendiri).Select(x => x.User!.Id).ToHashSet();
                UserList = UserAccount.GetDataList(FilterAktif.Aktif)
                    .Where(x => !dipakai.Contains(x.Id))
                    .OrderBy(x => x.Username)
                    .Select(x => new GwaVM.OpsiItem(x.Id, x.Username))
                    .ToList();
            }
        }

        public class Edit : Add
        {
            public string StatusText { get; set; } = "";

            public Edit(string encKey) : this(LibExtensions.Load<Member>(encKey, "Anggota"))
            {
            }

            private Edit(Member obj) : base(obj.User?.Id)
            {
                StatusText = EnumHelper.Text(obj.Status);
                Row = new NewRow
                {
                    UserId = obj.User?.Id,
                    MemberNumber = obj.MemberNumber,
                    MemberType = (int)obj.MemberType,
                    JoinedAt = Opsi.KeDateTime(obj.JoinedAt),
                    LeftAt = Opsi.KeDateTime(obj.LeftAt),
                    MemberCategory = (int)obj.MemberCategory,
                    Nim = obj.Nim,
                    Fullname = obj.Fullname,
                    Gender = obj.Gender.HasValue ? (int)obj.Gender.Value : null,
                    BirthDate = Opsi.KeDateTime(obj.BirthDate),
                    Fakultas = obj.Fakultas,
                    ProgramStudi = obj.ProgramStudi,
                    Angkatan = obj.Angkatan,
                    Institution = obj.Institution,
                    Phone = obj.Phone,
                    Email = obj.Email,
                    Address = obj.Address,
                    Status = (int)obj.Status
                };
            }
        }

        public class NewRow
        {
            [Required(ErrorMessage = "Nama lengkap harus diisi")]
            [StringLength(150, ErrorMessage = "Nama maksimal 150 karakter")]
            [Display(Name = "Nama Lengkap")]
            public string? Fullname { get; set; }

            [StringLength(30, ErrorMessage = "Nomor anggota maksimal 30 karakter")]
            [Display(Name = "Nomor Anggota")]
            public string? MemberNumber { get; set; }

            [Display(Name = "Tipe")]
            public int MemberType { get; set; } = (int)GWAShared.Enums.MemberType.Anggota;

            [Display(Name = "Kategori")]
            public int MemberCategory { get; set; } = (int)GWAShared.Enums.MemberCategory.Mahasiswa;

            [StringLength(20, ErrorMessage = "NIM maksimal 20 karakter")]
            [Display(Name = "NIM")]
            public string? Nim { get; set; }

            [Display(Name = "Jenis Kelamin")]
            public int? Gender { get; set; }

            [Display(Name = "Tanggal Lahir")]
            public DateTime? BirthDate { get; set; }

            [StringLength(100, ErrorMessage = "Fakultas maksimal 100 karakter")]
            [Display(Name = "Fakultas")]
            public string? Fakultas { get; set; }

            [StringLength(100, ErrorMessage = "Program studi maksimal 100 karakter")]
            [Display(Name = "Program Studi")]
            public string? ProgramStudi { get; set; }

            [Range(1990, 2100, ErrorMessage = "Angkatan tidak valid")]
            [Display(Name = "Angkatan")]
            public short? Angkatan { get; set; }

            [StringLength(150, ErrorMessage = "Instansi maksimal 150 karakter")]
            [Display(Name = "Instansi (anggota umum)")]
            public string? Institution { get; set; }

            [StringLength(30, ErrorMessage = "Telepon maksimal 30 karakter")]
            [Display(Name = "Telepon / WhatsApp")]
            public string? Phone { get; set; }

            [EmailAddress(ErrorMessage = "Format email tidak valid")]
            [StringLength(100, ErrorMessage = "Email maksimal 100 karakter")]
            [Display(Name = "Email")]
            public string? Email { get; set; }

            [StringLength(255, ErrorMessage = "Alamat maksimal 255 karakter")]
            [Display(Name = "Alamat")]
            public string? Address { get; set; }

            [Display(Name = "Tanggal Bergabung")]
            public DateTime? JoinedAt { get; set; }

            [Display(Name = "Tanggal Keluar")]
            public DateTime? LeftAt { get; set; }

            [Display(Name = "Akun Login")]
            public int? UserId { get; set; }

            [Display(Name = "Status")]
            public int Status { get; set; } = (int)MemberStatus.Active;
        }

        // ---------- Foto anggota (modal _Foto)
        public class Foto
        {
            public string Key { get; set; } = "";
            public string Nama { get; set; } = "";
            public List<MemberPhoto.Info> Items { get; set; }

            public Foto(string encKey)
            {
                var obj = LibExtensions.Load<Member>(encKey, "Anggota");
                Key = obj.EncDecrKey;
                Nama = obj.Fullname;
                Items = MemberPhoto.GetInfoList(obj.Id).OrderBy(x => x.Id).ToList();
            }
        }

        public class FotoInput
        {
            public string Key { get; set; } = "";
            public int Id { get; set; }
        }

        public class Method
        {
            public static IEnumerable<Row> GetDataList()
            {
                var users = UserAccount.GetDataList(FilterAktif.Semua).ToDictionary(x => x.Id, x => x.Username);
                var photos = MemberPhoto.GetInfoList().GroupBy(x => x.MemberId).ToDictionary(g => g.Key, g => g.OrderBy(x => x.Id).ToList());
                return Member.GetDataList().Select(x => new Row
                {
                    EncDecrKey = x.EncDecrKey,
                    Fullname = x.Fullname,
                    MemberNumber = x.MemberNumber,
                    Nim = x.Nim,
                    Kategori = EnumHelper.Text(x.MemberCategory),
                    Prodi = x.ProgramStudi ?? x.Institution,
                    Angkatan = x.Angkatan,
                    Tipe = EnumHelper.Text(x.MemberType),
                    StatusText = EnumHelper.Text(x.Status),
                    Username = x.User != null && users.TryGetValue(x.User.Id, out var u) ? u : null,
                    Phone = x.Phone,
                    Email = x.Email,
                    JoinedAt = Opsi.KeDateTime(x.JoinedAt),
                    JumlahFoto = photos.TryGetValue(x.Id, out var p) ? p.Count : 0,
                    FotoId = photos.TryGetValue(x.Id, out var p2) ? p2[0].Id : null,
                    Aktif = x.Status == MemberStatus.Active
                }).OrderBy(x => x.Fullname);
            }

            private static Member.NewRow ToNewRow(NewRow input) => new()
            {
                UserId = input.UserId,
                MemberNumber = input.MemberNumber,
                MemberType = (MemberType)input.MemberType,
                JoinedAt = Opsi.Tanggal(input.JoinedAt),
                MemberCategory = (MemberCategory)input.MemberCategory,
                Nim = input.Nim,
                Fullname = input.Fullname ?? "",
                Gender = input.Gender.HasValue ? (Gender)input.Gender.Value : null,
                BirthDate = Opsi.Tanggal(input.BirthDate),
                Fakultas = input.Fakultas,
                ProgramStudi = input.ProgramStudi,
                Angkatan = input.Angkatan,
                Institution = input.Institution,
                Phone = input.Phone,
                Email = input.Email,
                Address = input.Address
            };

            public static void Simpan(NewRow input, string user)
            {
                Member.Save(ToNewRow(input), user).Ok();
            }

            public static void Update(string encKey, NewRow input, string user)
            {
                var obj = LibExtensions.Load<Member>(encKey, "Anggota");
                var n = ToNewRow(input);
                obj.User = n.UserId.HasValue ? new UserAccount { Id = n.UserId.Value } : null;
                obj.MemberNumber = n.MemberNumber;
                obj.MemberType = n.MemberType;
                obj.JoinedAt = n.JoinedAt;
                obj.LeftAt = Opsi.Tanggal(input.LeftAt);
                obj.MemberCategory = n.MemberCategory;
                obj.Nim = n.Nim;
                obj.Fullname = n.Fullname;
                obj.Gender = n.Gender;
                obj.BirthDate = n.BirthDate;
                obj.Fakultas = n.Fakultas;
                obj.ProgramStudi = n.ProgramStudi;
                obj.Angkatan = n.Angkatan;
                obj.Institution = n.Institution;
                obj.Phone = n.Phone;
                obj.Email = n.Email;
                obj.Address = n.Address;
                obj.Status = (MemberStatus)input.Status;
                obj.Update(user).Ok();
            }

            public static void Nonaktifkan(string? encKey, string user)
            {
                LibExtensions.Load<Member>(encKey, "Anggota").Delete(user).Ok();
            }

            public static void TambahFoto(string? encKey, byte[]? data, string user)
            {
                var obj = LibExtensions.Load<Member>(encKey, "Anggota");
                if (data == null)
                {
                    throw new ArgumentException("Pilih foto terlebih dahulu.");
                }
                MemberPhoto.Save(new MemberPhoto.NewRow { MemberId = obj.Id, Data = data }, user).Ok();
            }

            public static MemberPhoto GetFoto(string? encKey, int id)
            {
                var obj = LibExtensions.Load<Member>(encKey, "Anggota");
                return MemberPhoto.GetDataList(obj.Id).FirstOrDefault(x => x.Id == id) ?? throw new ArgumentException("Foto tidak ditemukan.");
            }

            public static void HapusFoto(FotoInput input, string user)
            {
                GetFoto(input.Key, input.Id).Delete(user).Ok();
            }
        }
    }
}
