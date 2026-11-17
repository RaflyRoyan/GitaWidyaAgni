using System.ComponentModel.DataAnnotations;
using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Transaksi.EVoting;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.EVoting
{
    /// <summary>
    /// E-Voting: pemilihan (Draft → Pendaftaran → Dibuka → Ditutup → Final), kandidat, daftar pemilih, hasil, kotak suara, dan log audit.
    /// Pilihan suara terenkripsi dan tidak terhubung ke pemilih; hasil baru terlihat setelah Finalisasi.
    /// </summary>
    public class EVotingVM
    {
        public class Index
        {
        }

        public class List
        {
            public List<Row> Rows { get; set; } = Election.GetDataList().Select(x => new Row
            {
                EncDecrKey = x.EncDecrKey,
                Title = x.Title,
                PeriodName = x.PeriodName,
                StartAt = x.StartAt,
                EndAt = x.EndAt,
                CandidateCount = x.CandidateCount,
                VoterCount = x.VoterCount,
                VotedCount = x.VotedCount,
                Status = (int)x.Status,
                StatusText = EnumHelper.Text(x.Status)
            }).OrderByDescending(x => x.StartAt).ToList();
        }

        public class Row
        {
            public string EncDecrKey { get; set; } = "";
            public string Title { get; set; } = "";
            public string? PeriodName { get; set; }
            public DateTime StartAt { get; set; }
            public DateTime EndAt { get; set; }
            public int CandidateCount { get; set; }
            public int VoterCount { get; set; }
            public int VotedCount { get; set; }
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
        }

        public class Add
        {
            public NewRow Row { get; set; } = new()
            {
                PeriodId = Opsi.PeriodeAktifId(),
                StartAt = DateTime.Today.AddDays(7).AddHours(8),
                EndAt = DateTime.Today.AddDays(7).AddHours(17)
            };
            public List<GwaVM.OpsiItem> PeriodeOpsi { get; set; } = Opsi.Periode(true);
            public bool JadwalTerkunci { get; set; }
            public bool BolehPerpanjang { get; set; }
        }

        public class Edit : Add
        {
            public string StatusText { get; set; } = "";
            public Edit(string encKey)
            {
                var obj = LibExtensions.Load<Election>(encKey, "Pemilihan");
                if (obj.Status == ElectionStatus.Finalized)
                {
                    throw new ArgumentException("Pemilihan yang sudah final tidak bisa diubah.");
                }
                StatusText = EnumHelper.Text(obj.Status);
                JadwalTerkunci = obj.Status != ElectionStatus.Draft && obj.Status != ElectionStatus.Registration;
                BolehPerpanjang = obj.Status == ElectionStatus.Open;
                Row = new NewRow { PeriodId = obj.PeriodId, Title = obj.Title, Description = obj.Description, StartAt = obj.StartAt, EndAt = obj.EndAt };
            }
        }

        public class NewRow
        {
            [Display(Name = "Periode")]
            public int? PeriodId { get; set; }

            [Required(ErrorMessage = "Judul harus diisi")]
            [StringLength(200, ErrorMessage = "Judul maksimal 200 karakter")]
            [Display(Name = "Judul Pemilihan")]
            public string? Title { get; set; }

            [Display(Name = "Deskripsi")]
            public string? Description { get; set; }

            [Required(ErrorMessage = "Waktu mulai harus diisi")]
            [Display(Name = "Mulai Pemungutan")]
            public DateTime? StartAt { get; set; }

            [Required(ErrorMessage = "Waktu selesai harus diisi")]
            [Display(Name = "Selesai Pemungutan")]
            public DateTime? EndAt { get; set; }
        }

        // Kelola pemilihan (modal bertab).
        public class Kelola
        {
            public string Key { get; set; } = "";
            public string Judul { get; set; } = "";
            public ElectionStatus Status { get; set; }
            public bool BisaUbahKandidat => Status == ElectionStatus.Draft || Status == ElectionStatus.Registration;
            public bool BisaUbahPemilih => BisaUbahKandidat || Status == ElectionStatus.Open;
            public int JumlahSuara { get; set; }
            public List<ElectionCandidate> Kandidat { get; set; }
            public List<ElectionEligibleVoter> Pemilih { get; set; }
            public List<ElectionResult> Hasil { get; set; }
            public List<ElectionVote> KotakSuara { get; set; }
            public List<ElectionAuditEvent> Audit { get; set; }
            public List<GwaVM.OpsiItem> AnggotaOpsi { get; set; } = Opsi.Anggota();
            public List<GwaVM.OpsiItem> TipeAnggotaOpsi { get; set; } = GwaVM.Opsi<MemberType>();

            public Kelola(string encKey)
            {
                var obj = LibExtensions.Load<Election>(encKey, "Pemilihan");
                Key = obj.EncDecrKey;
                Status = obj.Status;
                Judul = obj.Title + " · " + EnumHelper.Text(obj.Status) + " · " + obj.StartAt.ToString("dd MMM yyyy HH:mm") + " – " + obj.EndAt.ToString("dd MMM yyyy HH:mm");
                Kandidat = ElectionCandidate.GetDataList(obj.Id).OrderBy(x => x.CandidateNumber ?? int.MaxValue).ThenBy(x => x.MemberName).ToList();
                Pemilih = ElectionEligibleVoter.GetDataList(obj.Id).OrderBy(x => x.MemberName).ToList();
                Hasil = obj.Status == ElectionStatus.Finalized ? ElectionResult.GetDataList(obj.Id) : new List<ElectionResult>();
                JumlahSuara = ElectionVote.GetCount(obj.Id);
                KotakSuara = ElectionVote.GetDataList(obj.Id);
                Audit = ElectionAuditEvent.GetDataList(obj.Id).OrderByDescending(x => x.InsDate).ToList();
            }
        }

        public class KandidatInput
        {
            public string Key { get; set; } = "";
            public string? CandidateKey { get; set; }
            public int MemberId { get; set; }
            public int? CandidateNumber { get; set; }
            public string? Vision { get; set; }
            public string? Mission { get; set; }
        }

        public class PemilihInput
        {
            public string Key { get; set; } = "";
            public List<int>? MemberIds { get; set; }
            public List<int>? MemberTypes { get; set; }
        }

        public class Method
        {
            public static Election Load(string? key) => LibExtensions.Load<Election>(key, "Pemilihan");
            public static ElectionCandidate LoadKandidat(string? key) => LibExtensions.Load<ElectionCandidate>(key, "Kandidat");
            public static ElectionEligibleVoter LoadPemilih(string? key) => LibExtensions.Load<ElectionEligibleVoter>(key, "Pemilih");

            public static void Simpan(NewRow input, string user)
                => Election.Save(new Election.NewRow
                {
                    PeriodId = input.PeriodId,
                    Title = input.Title ?? "",
                    Description = input.Description,
                    StartAt = input.StartAt ?? DateTime.MinValue,
                    EndAt = input.EndAt ?? DateTime.MinValue
                }, user).Ok();

            public static void Update(string key, NewRow input, string user)
            {
                var obj = Load(key);
                obj.PeriodId = input.PeriodId;
                obj.Title = input.Title ?? "";
                obj.Description = input.Description;
                obj.StartAt = input.StartAt ?? obj.StartAt;
                obj.EndAt = input.EndAt ?? obj.EndAt;
                obj.Update(user).Ok();
            }

            public static void SimpanKandidat(KandidatInput input, byte[]? foto, bool hapusFoto, string user)
            {
                if (string.IsNullOrEmpty(input.CandidateKey))
                {
                    var election = Load(input.Key);
                    ElectionCandidate.Register(new ElectionCandidate.NewRow
                    {
                        ElectionId = election.Id,
                        MemberId = input.MemberId,
                        CandidateNumber = input.CandidateNumber,
                        Vision = input.Vision,
                        Mission = input.Mission,
                        Photo = foto
                    }, user).Ok();
                    return;
                }
                var c = LoadKandidat(input.CandidateKey);
                c.CandidateNumber = input.CandidateNumber;
                c.Vision = input.Vision;
                c.Mission = input.Mission;
                c.Update(user).Ok();
                if (foto != null || hapusFoto)
                {
                    c.UpdatePhoto(foto, user).Ok();
                }
            }

            public static int TambahPemilih(PemilihInput input, string user)
            {
                var election = Load(input.Key);
                if (input.MemberTypes != null)
                {
                    return ElectionEligibleVoter.Generate(election.Id, input.MemberTypes.Count == 0 ? null : input.MemberTypes.Select(x => (MemberType)x).ToList(), user).Ok();
                }
                return ElectionEligibleVoter.Add(election.Id, input.MemberIds ?? new List<int>(), user).Ok();
            }
        }
    }
}
