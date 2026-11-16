using System.ComponentModel.DataAnnotations;
using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Master.Akses;
using GWALib.Lib.Master.Anggota;
using GWALib.Lib.Master.Approval;
using GWALib.Lib.Master.Information;
using GWALib.Lib.Transaksi.Approval;
using GWALib.Lib.Transaksi.Information;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Pengaturan
{
    /// <summary>
    /// Pengaturan Sistem: pengaturan situs, format nomor dokumen, alur persetujuan + langkah, template pesan,
    /// antrian pesan, log job, log aktivitas, sequence id. Satu tab = satu grid _List&lt;X&gt; + modal _Add&lt;X&gt; / _Edit&lt;X&gt;.
    /// </summary>
    public class PengaturanVM
    {
        public static readonly string[] JenisAlur = { "FUNDING_REQUEST", "PROCUREMENT", "LETTER" };

        public class Index
        {
        }

        // ================================================================ Pengaturan situs
        public class ListSetting
        {
            public List<SettingRow> Rows { get; set; } = Method.GetSettings().ToList();
        }
        public class SettingRow
        {
            public string EncDecrKey { get; set; } = "";
            public string SettingGroup { get; set; } = "";
            public string SettingKey { get; set; } = "";
            public string? SettingValue { get; set; }
            public string? Description { get; set; }
            public bool HasImage { get; set; }
        }

        public class AddSetting
        {
            public NewSetting Row { get; set; } = new();
        }
        public class EditSetting : AddSetting
        {
            public bool HasImage { get; set; }
            public string? ImageUrl { get; set; }

            public EditSetting(string encKey, string imageUrl)
            {
                var obj = LibExtensions.Load<SiteSetting>(encKey, "Pengaturan");
                Row = new NewSetting { SettingGroup = obj.SettingGroup, SettingKey = obj.SettingKey, SettingValue = obj.SettingValue, Description = obj.Description };
                HasImage = obj.HasImage;
                ImageUrl = obj.HasImage ? imageUrl : null;
            }
        }
        /// <summary>Form pengaturan situs (dikirim FormData karena ada gambar). Kunci tidak bisa diubah setelah disimpan.</summary>
        public class NewSetting
        {
            [Required(ErrorMessage = "Grup harus diisi")]
            [StringLength(50, ErrorMessage = "Grup maksimal 50 karakter")]
            [Display(Name = "Grup")]
            public string? SettingGroup { get; set; }

            [Required(ErrorMessage = "Kunci harus diisi")]
            [StringLength(100, ErrorMessage = "Kunci maksimal 100 karakter")]
            [Display(Name = "Kunci")]
            public string? SettingKey { get; set; }

            [Display(Name = "Nilai")]
            public string? SettingValue { get; set; }

            [StringLength(200, ErrorMessage = "Keterangan maksimal 200 karakter")]
            [Display(Name = "Keterangan")]
            public string? Description { get; set; }

            [Display(Name = "Hapus gambar saat ini")]
            public bool HapusGambar { get; set; }
        }

        // ================================================================ Format nomor
        public class ListFormat
        {
            public List<FormatRow> Rows { get; set; } = Method.GetFormats().ToList();
        }
        public class FormatRow
        {
            public string EncDecrKey { get; set; } = "";
            public string DocumentType { get; set; } = "";
            public string NumberFormat { get; set; } = "";
            public string Contoh { get; set; } = "";
            public string Reset { get; set; } = "";
            public bool Aktif { get; set; }
        }

        public class AddFormat
        {
            public NewFormat Row { get; set; } = new();
            public List<GwaVM.OpsiItem> ResetList { get; set; } = GwaVM.Opsi<NumberResetPeriod>();
            public List<GwaVM.OpsiItem> AktifList { get; set; } = GwaVM.OpsiAktif;
        }
        public class EditFormat : AddFormat
        {
            public string Contoh { get; set; } = "";

            public EditFormat(string encKey)
            {
                var obj = LibExtensions.Load<DocumentNumberFormat>(encKey, "Format nomor");
                Row = new NewFormat
                {
                    DocumentType = obj.DocumentType, NumberFormat = obj.NumberFormat, SeqPadding = obj.SeqPadding,
                    ResetPeriod = (int)obj.ResetPeriod, Description = obj.Description, IsActive = (int)obj.IsActive
                };
                try
                {
                    Contoh = obj.GetPreview();
                }
                catch (Exception ex)
                {
                    Contoh = "(" + ex.Message + ")";
                }
            }
        }
        /// <summary>Form format nomor. Jenis dokumen tidak bisa diubah; status hanya di Edit.</summary>
        public class NewFormat
        {
            [Required(ErrorMessage = "Jenis dokumen harus diisi")]
            [StringLength(50, ErrorMessage = "Jenis dokumen maksimal 50 karakter")]
            [Display(Name = "Jenis dokumen")]
            public string? DocumentType { get; set; }

            [Required(ErrorMessage = "Format harus diisi")]
            [StringLength(100, ErrorMessage = "Format maksimal 100 karakter")]
            [RegularExpression(@".*\{seq\}.*", ErrorMessage = "Format wajib memuat {{seq}}")]
            [Display(Name = "Format")]
            public string? NumberFormat { get; set; }

            [Range(1, 10, ErrorMessage = "Digit nomor urut 1 sampai 10")]
            [Display(Name = "Digit nomor urut")]
            public int SeqPadding { get; set; } = 3;

            [Display(Name = "Nomor urut direset")]
            public int ResetPeriod { get; set; } = (int)NumberResetPeriod.Tahunan;

            [StringLength(200, ErrorMessage = "Keterangan maksimal 200 karakter")]
            [Display(Name = "Keterangan")]
            public string? Description { get; set; }

            [Display(Name = "Status")]
            public int IsActive { get; set; } = (int)ActiveStatus.Aktif;
        }

        // ================================================================ Alur persetujuan + langkah
        public class ListFlow
        {
            public List<FlowRow> Rows { get; set; } = Method.GetFlows().ToList();
        }
        public class FlowRow
        {
            public string EncDecrKey { get; set; } = "";
            public string DocumentType { get; set; } = "";
            public string Name { get; set; } = "";
            public string? Description { get; set; }
            public int JumlahLangkah { get; set; }
            public bool Aktif { get; set; }
            public List<StepRow> Steps { get; set; } = new();
        }
        public class StepRow
        {
            public string EncDecrKey { get; set; } = "";
            public int StepLevel { get; set; }
            public string StepName { get; set; } = "";
            public string Jenis { get; set; } = "";
            public string? Penyetuju { get; set; }
            public string Revisi { get; set; } = "";
        }

        public class AddFlow
        {
            public NewFlow Row { get; set; } = new();
            public string[] JenisList { get; set; } = JenisAlur;
            public List<GwaVM.OpsiItem> AktifList { get; set; } = GwaVM.OpsiAktif;
        }
        public class EditFlow : AddFlow
        {
            public int JumlahLangkah { get; set; }

            public EditFlow(string encKey)
            {
                var obj = LibExtensions.Load<ApprovalFlow>(encKey, "Alur persetujuan");
                Row = new NewFlow { DocumentType = obj.DocumentType, Name = obj.Name, Description = obj.Description, IsActive = (int)obj.IsActive };
                JumlahLangkah = obj.GetSteps(true).Count();
            }
        }
        /// <summary>Form alur persetujuan. Jenis dokumen tidak bisa diubah. Alur baru boleh langsung aktif bila sudah punya langkah.</summary>
        public class NewFlow
        {
            [Required(ErrorMessage = "Jenis dokumen harus diisi")]
            [StringLength(50, ErrorMessage = "Jenis dokumen maksimal 50 karakter")]
            [Display(Name = "Jenis dokumen")]
            public string? DocumentType { get; set; }

            [Required(ErrorMessage = "Nama alur harus diisi")]
            [StringLength(100, ErrorMessage = "Nama alur maksimal 100 karakter")]
            [Display(Name = "Nama alur")]
            public string? Name { get; set; }

            [StringLength(200, ErrorMessage = "Keterangan maksimal 200 karakter")]
            [Display(Name = "Keterangan")]
            public string? Description { get; set; }

            [Display(Name = "Status")]
            public int IsActive { get; set; } = (int)ActiveStatus.TidakAktif;
        }

        public class AddStep
        {
            public string NamaAlur { get; set; } = "";
            public NewStep Row { get; set; } = new();
            public List<GwaVM.OpsiItem> JenisList { get; set; } = GwaVM.Opsi<ApproverType>();
            public List<GwaVM.OpsiItem> RevisiList { get; set; } = GwaVM.Opsi<RevisionPermission>();
            public List<GwaVM.OpsiItem> RoleList { get; set; } = new();
            public List<GwaVM.OpsiItem> PositionList { get; set; } = new();
            public List<GwaVM.OpsiItem> UserList { get; set; } = new();

            protected AddStep()
            {
                RoleList = Role.GetDataList(FilterAktif.Aktif).Select(x => new GwaVM.OpsiItem(x.Id, $"{x.Name} ({x.Code})")).ToList();
                PositionList = Position.GetDataList(FilterAktif.Aktif).Select(x => new GwaVM.OpsiItem(x.Id, x.Name)).ToList();
                UserList = UserAccount.GetDataList(FilterAktif.Aktif).OrderBy(x => x.Username).Select(x => new GwaVM.OpsiItem(x.Id, x.Username)).ToList();
            }
            public AddStep(string flowKey) : this()
            {
                NamaAlur = LibExtensions.Load<ApprovalFlow>(flowKey, "Alur persetujuan").Name;
            }
        }
        public class EditStep : AddStep
        {
            public EditStep(string encKey) : base()
            {
                var obj = LibExtensions.Load<ApprovalFlowStep>(encKey, "Langkah persetujuan");
                NamaAlur = obj.Flow.Name;
                Row = new NewStep
                {
                    StepLevel = obj.StepLevel, StepName = obj.StepName, ApproverType = (int)obj.ApproverType,
                    RoleId = obj.Role?.Id, PositionId = obj.Position?.Id, UserId = obj.User?.Id, AllowRevision = (int)obj.AllowRevision
                };
            }
        }
        /// <summary>Form langkah. Penyetuju diisi sesuai jenisnya (1 Role, 2 Jabatan, 3 User tertentu); kolom lain dikosongkan.</summary>
        public class NewStep
        {
            [Range(0, 99, ErrorMessage = "Level 0 sampai 99")]
            [Display(Name = "Level")]
            public int StepLevel { get; set; }

            [Required(ErrorMessage = "Nama langkah harus diisi")]
            [StringLength(100, ErrorMessage = "Nama langkah maksimal 100 karakter")]
            [Display(Name = "Nama langkah")]
            public string? StepName { get; set; }

            [Display(Name = "Penyetuju berdasarkan")]
            public int ApproverType { get; set; } = (int)GWAShared.Enums.ApproverType.Role;

            [Display(Name = "Role")]
            public int? RoleId { get; set; }

            [Display(Name = "Jabatan")]
            public int? PositionId { get; set; }

            [Display(Name = "User")]
            public int? UserId { get; set; }

            [Display(Name = "Boleh minta revisi")]
            public int AllowRevision { get; set; } = (int)RevisionPermission.Boleh;
        }

        // ================================================================ Template pesan
        public class ListTemplate
        {
            public List<TemplateRow> Rows { get; set; } = Method.GetTemplates().ToList();
        }
        public class TemplateRow
        {
            public string EncDecrKey { get; set; } = "";
            public string TemplateCode { get; set; } = "";
            public string Kanal { get; set; } = "";
            public string? Subject { get; set; }
            public string Body { get; set; } = "";
            public bool Aktif { get; set; }
        }

        public class AddTemplate
        {
            public NewTemplate Row { get; set; } = new();
            public List<GwaVM.OpsiItem> KanalList { get; set; } = GwaVM.Opsi<NotificationChannel>();
            public List<GwaVM.OpsiItem> AktifList { get; set; } = GwaVM.OpsiAktif;
        }
        public class EditTemplate : AddTemplate
        {
            public EditTemplate(string encKey)
            {
                var obj = LibExtensions.Load<MessageTemplate>(encKey, "Template");
                Row = new NewTemplate
                {
                    TemplateCode = obj.TemplateCode, Channel = (int)obj.Channel, Subject = obj.Subject, Body = obj.Body,
                    Description = obj.Description, IsActive = (int)obj.IsActive
                };
            }
        }
        /// <summary>Form template pesan. Kode dan kanal tidak bisa diubah; status hanya di Edit.</summary>
        public class NewTemplate
        {
            [Required(ErrorMessage = "Kode template harus diisi")]
            [StringLength(50, ErrorMessage = "Kode template maksimal 50 karakter")]
            [Display(Name = "Kode template")]
            public string? TemplateCode { get; set; }

            [Display(Name = "Kanal")]
            public int Channel { get; set; } = (int)NotificationChannel.Email;

            [StringLength(200, ErrorMessage = "Subjek maksimal 200 karakter")]
            [Display(Name = "Subjek")]
            public string? Subject { get; set; }

            [Required(ErrorMessage = "Isi template harus diisi")]
            [Display(Name = "Isi")]
            public string? Body { get; set; }

            [StringLength(200, ErrorMessage = "Keterangan maksimal 200 karakter")]
            [Display(Name = "Keterangan")]
            public string? Description { get; set; }

            [Display(Name = "Status")]
            public int IsActive { get; set; } = (int)ActiveStatus.Aktif;
        }

        // ================================================================ Antrian pesan, log (baca saja)
        public class ListOutbox
        {
            public List<OutboxRow> Rows { get; set; } = Method.GetOutbox().ToList();
        }
        public class OutboxRow
        {
            public string EncDecrKey { get; set; } = "";
            public DateTime ScheduledAt { get; set; }
            public string Kanal { get; set; } = "";
            public string Recipient { get; set; } = "";
            public string Ringkasan { get; set; } = "";
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
            public int AttemptCount { get; set; }
            public string? LastError { get; set; }
            public DateTime? SentAt { get; set; }
        }
        public class DetailOutbox
        {
            public MessageOutbox Data { get; }
            public DetailOutbox(string encKey) { Data = LibExtensions.Load<MessageOutbox>(encKey, "Pesan"); }
        }

        public class ListJob
        {
            public List<JobRow> Rows { get; set; } = Method.GetJobs().ToList();
        }
        public class JobRow
        {
            public int Id { get; set; }
            public string JobName { get; set; } = "";
            public DateTime StartedAt { get; set; }
            public DateTime? FinishedAt { get; set; }
            public string Status { get; set; } = "";
            public int ProcessedCount { get; set; }
            public string? Message { get; set; }
        }

        public class ListLog
        {
            public List<LogRow> Rows { get; set; } = Method.GetLogs().ToList();
        }
        public class LogRow
        {
            public string EncDecrKey { get; set; } = "";
            public DateTime Waktu { get; set; }
            public string? Username { get; set; }
            public string Module { get; set; } = "";
            public string Action { get; set; } = "";
            public string? Data { get; set; }
            public string? IpAddress { get; set; }
        }
        public class DetailLog
        {
            public ActivityLog Data { get; }
            public DetailLog(string encKey) { Data = LibExtensions.Load<ActivityLog>(encKey, "Log"); }
        }

        // ================================================================ Sequence
        public class ListSequence
        {
            public List<SequenceRow> Rows { get; set; } = Method.GetSequences().ToList();
        }
        public class SequenceRow
        {
            public string EncDecrKey { get; set; } = "";
            public string TableName { get; set; } = "";
            public int LastId { get; set; }
        }
        public class EditSequence
        {
            public string TableName { get; set; } = "";
            public NewSequence Row { get; set; } = new();

            public EditSequence(string encKey)
            {
                var obj = LibExtensions.Load<Sequence>(encKey, "Sequence");
                TableName = obj.TableName;
                Row = new NewSequence { LastId = obj.LastId };
            }
        }
        public class NewSequence
        {
            [Range(0, int.MaxValue, ErrorMessage = "Id terakhir tidak boleh negatif")]
            [Display(Name = "Id terakhir")]
            public int LastId { get; set; }
        }

        public class Method
        {
            // ---------- Situs
            public static IEnumerable<SettingRow> GetSettings() => SiteSetting.GetDataList().Select(x => new SettingRow
            {
                EncDecrKey = x.EncDecrKey, SettingGroup = x.SettingGroup, SettingKey = x.SettingKey,
                SettingValue = x.SettingValue, Description = x.Description, HasImage = x.HasImage
            }).OrderBy(x => x.SettingGroup).ThenBy(x => x.SettingKey);
            public static void SimpanSetting(NewSetting input, byte[]? image, string user)
            {
                SiteSetting.Save(new SiteSetting.NewRow
                {
                    SettingGroup = (input.SettingGroup ?? "").Trim().ToUpperInvariant(), SettingKey = input.SettingKey ?? "", SettingValue = input.SettingValue,
                    Description = input.Description, SettingImage = image
                }, user).Ok();
            }
            public static void UpdateSetting(string encKey, NewSetting input, byte[]? image, string user)
            {
                var obj = LibExtensions.Load<SiteSetting>(encKey);
                obj.SettingGroup = (input.SettingGroup ?? "").Trim().ToUpperInvariant();
                obj.SettingValue = input.SettingValue;
                obj.Description = input.Description;
                obj.Update(user).Ok();
                if (image != null || input.HapusGambar)
                {
                    obj.UpdateImage(image, user).Ok();
                }
            }
            public static void HapusSetting(string? encKey, string user) => LibExtensions.Load<SiteSetting>(encKey).Delete(user).Ok();

            // ---------- Format nomor
            public static IEnumerable<FormatRow> GetFormats() => DocumentNumberFormat.GetDataList(FilterAktif.Semua).Select(x =>
            {
                string contoh;
                try
                {
                    contoh = x.GetPreview();
                }
                catch (Exception ex)
                {
                    contoh = "(" + ex.Message + ")";
                }
                return new FormatRow
                {
                    EncDecrKey = x.EncDecrKey, DocumentType = x.DocumentType, NumberFormat = x.NumberFormat,
                    Contoh = contoh, Reset = EnumHelper.Text(x.ResetPeriod), Aktif = x.IsActive == ActiveStatus.Aktif
                };
            }).OrderBy(x => x.DocumentType);
            public static void SimpanFormat(NewFormat input, string user)
            {
                DocumentNumberFormat.Save(new DocumentNumberFormat.NewRow
                {
                    DocumentType = input.DocumentType ?? "", NumberFormat = input.NumberFormat ?? "", SeqPadding = input.SeqPadding,
                    ResetPeriod = (NumberResetPeriod)input.ResetPeriod, Description = input.Description
                }, user).Ok();
            }
            public static void UpdateFormat(string encKey, NewFormat input, string user)
            {
                var obj = LibExtensions.Load<DocumentNumberFormat>(encKey);
                obj.NumberFormat = input.NumberFormat ?? "";
                obj.SeqPadding = input.SeqPadding;
                obj.ResetPeriod = (NumberResetPeriod)input.ResetPeriod;
                obj.Description = input.Description;
                obj.IsActive = (ActiveStatus)input.IsActive;
                obj.Update(user).Ok();
            }
            public static void HapusFormat(string? encKey, string user) => LibExtensions.Load<DocumentNumberFormat>(encKey).Delete(user).Ok();

            // ---------- Alur persetujuan
            public static IEnumerable<FlowRow> GetFlows() => ApprovalFlow.GetDataList(FilterAktif.Semua).Select(x =>
            {
                var steps = x.GetSteps(true).OrderBy(s => s.StepLevel).Select(s => new StepRow
                {
                    EncDecrKey = s.EncDecrKey,
                    StepLevel = s.StepLevel,
                    StepName = s.StepName,
                    Jenis = EnumHelper.Text(s.ApproverType),
                    Penyetuju = s.ApproverType switch
                    {
                        ApproverType.Role => s.Role?.Name,
                        ApproverType.Jabatan => s.Position?.Name,
                        _ => s.User?.Username
                    },
                    Revisi = EnumHelper.Text(s.AllowRevision)
                }).ToList();
                return new FlowRow
                {
                    EncDecrKey = x.EncDecrKey, DocumentType = x.DocumentType, Name = x.Name, Description = x.Description,
                    JumlahLangkah = steps.Count, Aktif = x.IsActive == ActiveStatus.Aktif, Steps = steps
                };
            }).OrderBy(x => x.DocumentType);
            public static void SimpanFlow(NewFlow input, string user)
            {
                ApprovalFlow.Save(new ApprovalFlow.NewRow
                {
                    DocumentType = input.DocumentType ?? "", Name = input.Name ?? "", Description = input.Description, IsActive = (ActiveStatus)input.IsActive
                }, user).Ok();
            }
            public static void UpdateFlow(string encKey, NewFlow input, string user)
            {
                var obj = LibExtensions.Load<ApprovalFlow>(encKey);
                obj.Name = input.Name ?? "";
                obj.Description = input.Description;
                obj.IsActive = (ActiveStatus)input.IsActive;
                obj.Update(user).Ok();
            }
            public static void HapusFlow(string? encKey, string user) => LibExtensions.Load<ApprovalFlow>(encKey).Delete(user).Ok();

            // Penyetuju yang tidak sesuai jenis dikosongkan.
            private static void Rapikan(NewStep input)
            {
                var jenis = (ApproverType)input.ApproverType;
                if (jenis != ApproverType.Role) input.RoleId = null;
                if (jenis != ApproverType.Jabatan) input.PositionId = null;
                if (jenis != ApproverType.UserTertentu) input.UserId = null;
                if ((input.RoleId ?? input.PositionId ?? input.UserId) == null)
                {
                    throw new ArgumentException("Pilih penyetuju.");
                }
            }
            public static void SimpanStep(string flowKey, NewStep input, string user)
            {
                Rapikan(input);
                var flow = LibExtensions.Load<ApprovalFlow>(flowKey);
                ApprovalFlowStep.Save(new ApprovalFlowStep.NewRow
                {
                    FlowId = flow.Id, StepLevel = input.StepLevel, StepName = input.StepName ?? "", ApproverType = (ApproverType)input.ApproverType,
                    RoleId = input.RoleId, PositionId = input.PositionId, UserId = input.UserId, AllowRevision = (RevisionPermission)input.AllowRevision
                }, user).Ok();
            }
            public static void UpdateStep(string encKey, NewStep input, string user)
            {
                Rapikan(input);
                var obj = LibExtensions.Load<ApprovalFlowStep>(encKey);
                obj.StepLevel = input.StepLevel;
                obj.StepName = input.StepName ?? "";
                obj.ApproverType = (ApproverType)input.ApproverType;
                obj.Role = input.RoleId > 0 ? new Role { Id = input.RoleId.Value } : null;
                obj.Position = input.PositionId > 0 ? new Position { Id = input.PositionId.Value } : null;
                obj.User = input.UserId > 0 ? new UserAccount { Id = input.UserId.Value } : null;
                obj.AllowRevision = (RevisionPermission)input.AllowRevision;
                obj.Update(user).Ok();
            }
            public static void HapusStep(string? encKey, string user) => LibExtensions.Load<ApprovalFlowStep>(encKey).Delete(user).Ok();

            // ---------- Template pesan
            public static IEnumerable<TemplateRow> GetTemplates() => MessageTemplate.GetDataList(FilterAktif.Semua).Select(x => new TemplateRow
            {
                EncDecrKey = x.EncDecrKey, TemplateCode = x.TemplateCode, Kanal = EnumHelper.Text(x.Channel),
                Subject = x.Subject, Body = x.Body, Aktif = x.IsActive == ActiveStatus.Aktif
            }).OrderBy(x => x.TemplateCode);
            public static void SimpanTemplate(NewTemplate input, string user)
            {
                MessageTemplate.Save(new MessageTemplate.NewRow
                {
                    TemplateCode = input.TemplateCode ?? "", Channel = (NotificationChannel)input.Channel, Subject = input.Subject,
                    Body = input.Body ?? "", Description = input.Description
                }, user).Ok();
            }
            public static void UpdateTemplate(string encKey, NewTemplate input, string user)
            {
                var obj = LibExtensions.Load<MessageTemplate>(encKey);
                obj.Subject = input.Subject;
                obj.Body = input.Body ?? "";
                obj.Description = input.Description;
                obj.IsActive = (ActiveStatus)input.IsActive;
                obj.Update(user).Ok();
            }
            public static void HapusTemplate(string? encKey, string user) => LibExtensions.Load<MessageTemplate>(encKey).Delete(user).Ok();

            // ---------- Antrian pesan
            public static IEnumerable<OutboxRow> GetOutbox() => MessageOutbox.GetDataList(startdata: 0, jmldata: 1000).Select(x => new OutboxRow
            {
                EncDecrKey = x.EncDecrKey, ScheduledAt = x.ScheduledAt, Kanal = x.ChannelNama, Recipient = x.Recipient,
                Ringkasan = x.Subject ?? (x.Body.Length > 80 ? x.Body[..80] + "…" : x.Body),
                Status = (int)x.Status, StatusText = x.StatusNama, AttemptCount = x.AttemptCount, LastError = x.LastError, SentAt = x.SentAt
            }).OrderByDescending(x => x.ScheduledAt);
            public static void Retry(string? encKey, string user) => LibExtensions.Load<MessageOutbox>(encKey).Retry(user).Ok();
            public static void BatalPesan(string? encKey, string user) => LibExtensions.Load<MessageOutbox>(encKey).Cancel(user).Ok();

            // ---------- Log
            public static IEnumerable<JobRow> GetJobs() => ScheduledJobLog.GetDataList(startdata: 0, jmldata: 1000).Select(x => new JobRow
            {
                Id = x.Id, JobName = x.JobName, StartedAt = x.StartedAt, FinishedAt = x.FinishedAt, Status = x.StatusNama,
                ProcessedCount = x.ProcessedCount, Message = x.Message
            }).OrderByDescending(x => x.StartedAt);
            public static IEnumerable<LogRow> GetLogs() => ActivityLog.GetDataList(startdata: 0, jmldata: 2000).Select(x => new LogRow
            {
                EncDecrKey = x.EncDecrKey, Waktu = x.InsDate, Username = x.Username, Module = x.Module, Action = x.Action,
                Data = x.EntityName == null ? null : x.EntityName + (x.EntityId != null ? " #" + x.EntityId : ""), IpAddress = x.IpAddress
            }).OrderByDescending(x => x.Waktu);

            // ---------- Sequence
            public static IEnumerable<SequenceRow> GetSequences() => Sequence.GetDataList().OrderBy(x => x.TableName)
                .Select(x => new SequenceRow { EncDecrKey = x.EncDecrKey, TableName = x.TableName, LastId = x.LastId });
            public static void UpdateSequence(string encKey, NewSequence input, string user)
            {
                var obj = LibExtensions.Load<Sequence>(encKey);
                obj.LastId = input.LastId;
                obj.Update(user).Ok();
            }
        }
    }
}
