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
    /// antrian pesan, log job, log aktivitas, sequence id.
    /// </summary>
    public class PengaturanVM
    {
        // ================================================================ Pengaturan situs
        public class FormSetting
        {
            public bool IsEdit { get; set; }
            public string SettingGroup { get; set; } = "";
            public string SettingKey { get; set; } = "";
            public string? SettingValue { get; set; }
            public string? Description { get; set; }
            public bool HasImage { get; set; }
            public bool HapusGambar { get; set; }
            public string? ImageUrl { get; set; }
        }
        public class EditSetting : FormSetting
        {
            public EditSetting(string encKey, string imageUrl)
            {
                var obj = LibExtensions.Load<SiteSetting>(encKey, "Pengaturan");
                IsEdit = true;
                SettingGroup = obj.SettingGroup;
                SettingKey = obj.SettingKey;
                SettingValue = obj.SettingValue;
                Description = obj.Description;
                HasImage = obj.HasImage;
                ImageUrl = obj.HasImage ? imageUrl : null;
            }
        }

        // ================================================================ Format nomor
        public class FormFormat
        {
            public bool IsEdit { get; set; }
            public string DocumentType { get; set; } = "";
            public string NumberFormat { get; set; } = "";
            public int SeqPadding { get; set; } = 3;
            public int ResetPeriod { get; set; } = (int)NumberResetPeriod.Tahunan;
            public string? Description { get; set; }
            public int IsActive { get; set; } = (int)ActiveStatus.Aktif;
        }
        public class EditFormat : FormFormat
        {
            public EditFormat(string encKey)
            {
                var obj = LibExtensions.Load<DocumentNumberFormat>(encKey, "Format nomor");
                IsEdit = true;
                DocumentType = obj.DocumentType;
                NumberFormat = obj.NumberFormat;
                SeqPadding = obj.SeqPadding;
                ResetPeriod = (int)obj.ResetPeriod;
                Description = obj.Description;
                IsActive = (int)obj.IsActive;
            }
        }

        // ================================================================ Alur persetujuan
        public class FormFlow
        {
            public bool IsEdit { get; set; }
            public string DocumentType { get; set; } = "";
            public string Name { get; set; } = "";
            public string? Description { get; set; }
            public int IsActive { get; set; } = (int)ActiveStatus.TidakAktif;
        }
        public class EditFlow : FormFlow
        {
            public EditFlow(string encKey)
            {
                var obj = LibExtensions.Load<ApprovalFlow>(encKey, "Alur persetujuan");
                IsEdit = true;
                DocumentType = obj.DocumentType;
                Name = obj.Name;
                Description = obj.Description;
                IsActive = (int)obj.IsActive;
            }
        }
        public class FormStep
        {
            public bool IsEdit { get; set; }
            public string FlowKey { get; set; } = "";
            public string NamaAlur { get; set; } = "";
            public int StepLevel { get; set; }
            public string StepName { get; set; } = "";
            public int ApproverType { get; set; } = (int)GWAShared.Enums.ApproverType.Role;
            public int? RoleId { get; set; }
            public int? PositionId { get; set; }
            public int? UserId { get; set; }
            public int AllowRevision { get; set; } = (int)RevisionPermission.Boleh;
            public List<GwaVM.OpsiItem> RoleList { get; set; } = new();
            public List<GwaVM.OpsiItem> PositionList { get; set; } = new();
            public List<GwaVM.OpsiItem> UserList { get; set; } = new();

            protected void IsiOpsi()
            {
                RoleList = Role.GetDataList(FilterAktif.Aktif).Select(x => new GwaVM.OpsiItem(x.Id, $"{x.Name} ({x.Code})")).ToList();
                PositionList = Position.GetDataList(FilterAktif.Aktif).Select(x => new GwaVM.OpsiItem(x.Id, x.Name)).ToList();
                UserList = UserAccount.GetDataList(FilterAktif.Aktif).OrderBy(x => x.Username).Select(x => new GwaVM.OpsiItem(x.Id, x.Username)).ToList();
            }
        }
        public class AddStep : FormStep
        {
            public AddStep(string flowKey)
            {
                var flow = LibExtensions.Load<ApprovalFlow>(flowKey, "Alur persetujuan");
                FlowKey = flow.EncDecrKey;
                NamaAlur = flow.Name;
                IsiOpsi();
            }
        }
        public class EditStep : FormStep
        {
            public EditStep(string encKey)
            {
                var obj = LibExtensions.Load<ApprovalFlowStep>(encKey, "Langkah persetujuan");
                IsEdit = true;
                NamaAlur = obj.Flow.Name;
                StepLevel = obj.StepLevel;
                StepName = obj.StepName;
                ApproverType = (int)obj.ApproverType;
                RoleId = obj.Role?.Id;
                PositionId = obj.Position?.Id;
                UserId = obj.User?.Id;
                AllowRevision = (int)obj.AllowRevision;
                IsiOpsi();
            }
        }

        // ================================================================ Template pesan
        public class FormTemplate
        {
            public bool IsEdit { get; set; }
            public string TemplateCode { get; set; } = "";
            public int Channel { get; set; } = (int)NotificationChannel.Email;
            public string? Subject { get; set; }
            public string Body { get; set; } = "";
            public string? Description { get; set; }
            public int IsActive { get; set; } = (int)ActiveStatus.Aktif;
        }
        public class EditTemplate : FormTemplate
        {
            public EditTemplate(string encKey)
            {
                var obj = LibExtensions.Load<MessageTemplate>(encKey, "Template");
                IsEdit = true;
                TemplateCode = obj.TemplateCode;
                Channel = (int)obj.Channel;
                Subject = obj.Subject;
                Body = obj.Body;
                Description = obj.Description;
                IsActive = (int)obj.IsActive;
            }
        }

        // ================================================================ Sequence
        public class FormSequence
        {
            public string TableName { get; set; } = "";
            public int LastId { get; set; }

            public FormSequence() { }
            public FormSequence(string encKey)
            {
                var obj = LibExtensions.Load<Sequence>(encKey, "Sequence");
                TableName = obj.TableName;
                LastId = obj.LastId;
            }
        }

        // ================================================================ Detail baca saja
        public class DetailOutbox
        {
            public MessageOutbox Data { get; }
            public DetailOutbox(string encKey) { Data = LibExtensions.Load<MessageOutbox>(encKey, "Pesan"); }
        }
        public class DetailLog
        {
            public ActivityLog Data { get; }
            public DetailLog(string encKey) { Data = LibExtensions.Load<ActivityLog>(encKey, "Log"); }
        }

        public class ViewModel
        {
            public class SettingRow
            {
                public string EncDecrKey { get; set; } = "";
                public int Id { get; set; }
                public string SettingGroup { get; set; } = "";
                public string SettingKey { get; set; } = "";
                public string? SettingValue { get; set; }
                public string? Description { get; set; }
                public bool HasImage { get; set; }
            }
            public class FormatRow
            {
                public string EncDecrKey { get; set; } = "";
                public int Id { get; set; }
                public string DocumentType { get; set; } = "";
                public string NumberFormat { get; set; } = "";
                public string Contoh { get; set; } = "";
                public string Reset { get; set; } = "";
                public int IsActive { get; set; }
            }
            public class FlowRow
            {
                public string EncDecrKey { get; set; } = "";
                public int Id { get; set; }
                public string DocumentType { get; set; } = "";
                public string Name { get; set; } = "";
                public string? Description { get; set; }
                public int JumlahLangkah { get; set; }
                public int IsActive { get; set; }
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
            public class TemplateRow
            {
                public string EncDecrKey { get; set; } = "";
                public int Id { get; set; }
                public string TemplateCode { get; set; } = "";
                public string Kanal { get; set; } = "";
                public string? Subject { get; set; }
                public string Body { get; set; } = "";
                public int IsActive { get; set; }
            }
            public class OutboxRow
            {
                public string EncDecrKey { get; set; } = "";
                public int Id { get; set; }
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
            public class LogRow
            {
                public string EncDecrKey { get; set; } = "";
                public int Id { get; set; }
                public DateTime Waktu { get; set; }
                public string? Username { get; set; }
                public string Module { get; set; } = "";
                public string Action { get; set; } = "";
                public string? Data { get; set; }
                public string? IpAddress { get; set; }
            }
            public class SequenceRow
            {
                public string EncDecrKey { get; set; } = "";
                public string TableName { get; set; } = "";
                public int LastId { get; set; }
            }
        }

        public class Method
        {
            // ---------- Situs
            public static IEnumerable<ViewModel.SettingRow> GetSettings() => SiteSetting.GetDataList().Select(x => new ViewModel.SettingRow
            {
                EncDecrKey = x.EncDecrKey, Id = x.Id, SettingGroup = x.SettingGroup, SettingKey = x.SettingKey,
                SettingValue = x.SettingValue, Description = x.Description, HasImage = x.HasImage
            });
            public static void SimpanSetting(FormSetting input, byte[]? image, string user)
            {
                SiteSetting.Save(new SiteSetting.NewRow
                {
                    SettingGroup = input.SettingGroup, SettingKey = input.SettingKey, SettingValue = input.SettingValue,
                    Description = input.Description, SettingImage = image
                }, user).Ok();
            }
            public static void UpdateSetting(string? encKey, FormSetting input, byte[]? image, string user)
            {
                var obj = LibExtensions.Load<SiteSetting>(encKey);
                obj.SettingGroup = input.SettingGroup;
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
            public static IEnumerable<ViewModel.FormatRow> GetFormats() => DocumentNumberFormat.GetDataList(FilterAktif.Semua).Select(x =>
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
                return new ViewModel.FormatRow
                {
                    EncDecrKey = x.EncDecrKey, Id = x.Id, DocumentType = x.DocumentType, NumberFormat = x.NumberFormat,
                    Contoh = contoh, Reset = EnumHelper.Text(x.ResetPeriod), IsActive = (int)x.IsActive
                };
            });
            public static void SimpanFormat(FormFormat input, string user)
            {
                DocumentNumberFormat.Save(new DocumentNumberFormat.NewRow
                {
                    DocumentType = input.DocumentType, NumberFormat = input.NumberFormat, SeqPadding = input.SeqPadding,
                    ResetPeriod = (NumberResetPeriod)input.ResetPeriod, Description = input.Description
                }, user).Ok();
            }
            public static void UpdateFormat(string? encKey, FormFormat input, string user)
            {
                var obj = LibExtensions.Load<DocumentNumberFormat>(encKey);
                obj.NumberFormat = input.NumberFormat;
                obj.SeqPadding = input.SeqPadding;
                obj.ResetPeriod = (NumberResetPeriod)input.ResetPeriod;
                obj.Description = input.Description;
                obj.IsActive = (ActiveStatus)input.IsActive;
                obj.Update(user).Ok();
            }
            public static void HapusFormat(string? encKey, string user) => LibExtensions.Load<DocumentNumberFormat>(encKey).Delete(user).Ok();

            // ---------- Alur persetujuan
            public static IEnumerable<ViewModel.FlowRow> GetFlows() => ApprovalFlow.GetDataList(FilterAktif.Semua).Select(x =>
            {
                var steps = x.GetSteps(true).OrderBy(s => s.StepLevel).Select(s => new ViewModel.StepRow
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
                return new ViewModel.FlowRow
                {
                    EncDecrKey = x.EncDecrKey, Id = x.Id, DocumentType = x.DocumentType, Name = x.Name, Description = x.Description,
                    JumlahLangkah = steps.Count, IsActive = (int)x.IsActive, Steps = steps
                };
            });
            public static void SimpanFlow(FormFlow input, string user)
            {
                ApprovalFlow.Save(new ApprovalFlow.NewRow
                {
                    DocumentType = input.DocumentType, Name = input.Name, Description = input.Description, IsActive = (ActiveStatus)input.IsActive
                }, user).Ok();
            }
            public static void UpdateFlow(string? encKey, FormFlow input, string user)
            {
                var obj = LibExtensions.Load<ApprovalFlow>(encKey);
                obj.Name = input.Name;
                obj.Description = input.Description;
                obj.IsActive = (ActiveStatus)input.IsActive;
                obj.Update(user).Ok();
            }
            public static void HapusFlow(string? encKey, string user) => LibExtensions.Load<ApprovalFlow>(encKey).Delete(user).Ok();

            public static void SimpanStep(string? flowKey, FormStep input, string user)
            {
                var flow = LibExtensions.Load<ApprovalFlow>(flowKey);
                ApprovalFlowStep.Save(new ApprovalFlowStep.NewRow
                {
                    FlowId = flow.Id, StepLevel = input.StepLevel, StepName = input.StepName, ApproverType = (ApproverType)input.ApproverType,
                    RoleId = input.RoleId, PositionId = input.PositionId, UserId = input.UserId, AllowRevision = (RevisionPermission)input.AllowRevision
                }, user).Ok();
            }
            public static void UpdateStep(string? encKey, FormStep input, string user)
            {
                var obj = LibExtensions.Load<ApprovalFlowStep>(encKey);
                obj.StepLevel = input.StepLevel;
                obj.StepName = input.StepName;
                obj.ApproverType = (ApproverType)input.ApproverType;
                obj.Role = input.RoleId > 0 ? new Role { Id = input.RoleId.Value } : null;
                obj.Position = input.PositionId > 0 ? new Position { Id = input.PositionId.Value } : null;
                obj.User = input.UserId > 0 ? new UserAccount { Id = input.UserId.Value } : null;
                obj.AllowRevision = (RevisionPermission)input.AllowRevision;
                obj.Update(user).Ok();
            }
            public static void HapusStep(string? encKey, string user) => LibExtensions.Load<ApprovalFlowStep>(encKey).Delete(user).Ok();

            // ---------- Template pesan
            public static IEnumerable<ViewModel.TemplateRow> GetTemplates() => MessageTemplate.GetDataList(FilterAktif.Semua).Select(x => new ViewModel.TemplateRow
            {
                EncDecrKey = x.EncDecrKey, Id = x.Id, TemplateCode = x.TemplateCode, Kanal = EnumHelper.Text(x.Channel),
                Subject = x.Subject, Body = x.Body, IsActive = (int)x.IsActive
            });
            public static void SimpanTemplate(FormTemplate input, string user)
            {
                MessageTemplate.Save(new MessageTemplate.NewRow
                {
                    TemplateCode = input.TemplateCode, Channel = (NotificationChannel)input.Channel, Subject = input.Subject,
                    Body = input.Body, Description = input.Description
                }, user).Ok();
            }
            public static void UpdateTemplate(string? encKey, FormTemplate input, string user)
            {
                var obj = LibExtensions.Load<MessageTemplate>(encKey);
                obj.Subject = input.Subject;
                obj.Body = input.Body;
                obj.Description = input.Description;
                obj.IsActive = (ActiveStatus)input.IsActive;
                obj.Update(user).Ok();
            }
            public static void HapusTemplate(string? encKey, string user) => LibExtensions.Load<MessageTemplate>(encKey).Delete(user).Ok();

            // ---------- Antrian pesan
            public static IEnumerable<ViewModel.OutboxRow> GetOutbox() => MessageOutbox.GetDataList(startdata: 0, jmldata: 1000).Select(x => new ViewModel.OutboxRow
            {
                EncDecrKey = x.EncDecrKey, Id = x.Id, ScheduledAt = x.ScheduledAt, Kanal = x.ChannelNama, Recipient = x.Recipient,
                Ringkasan = x.Subject ?? (x.Body.Length > 80 ? x.Body[..80] + "…" : x.Body),
                Status = (int)x.Status, StatusText = x.StatusNama, AttemptCount = x.AttemptCount, LastError = x.LastError, SentAt = x.SentAt
            });
            public static void Retry(string? encKey, string user) => LibExtensions.Load<MessageOutbox>(encKey).Retry(user).Ok();
            public static void BatalPesan(string? encKey, string user) => LibExtensions.Load<MessageOutbox>(encKey).Cancel(user).Ok();

            // ---------- Log
            public static IEnumerable<ViewModel.JobRow> GetJobs() => ScheduledJobLog.GetDataList(startdata: 0, jmldata: 1000).Select(x => new ViewModel.JobRow
            {
                Id = x.Id, JobName = x.JobName, StartedAt = x.StartedAt, FinishedAt = x.FinishedAt, Status = x.StatusNama,
                ProcessedCount = x.ProcessedCount, Message = x.Message
            });
            public static IEnumerable<ViewModel.LogRow> GetLogs() => ActivityLog.GetDataList(startdata: 0, jmldata: 2000).Select(x => new ViewModel.LogRow
            {
                EncDecrKey = x.EncDecrKey, Id = x.Id, Waktu = x.InsDate, Username = x.Username, Module = x.Module, Action = x.Action,
                Data = x.EntityName == null ? null : x.EntityName + (x.EntityId != null ? " #" + x.EntityId : ""), IpAddress = x.IpAddress
            });

            // ---------- Sequence
            public static IEnumerable<ViewModel.SequenceRow> GetSequences() => Sequence.GetDataList().OrderBy(x => x.TableName)
                .Select(x => new ViewModel.SequenceRow { EncDecrKey = x.EncDecrKey, TableName = x.TableName, LastId = x.LastId });
            public static void UpdateSequence(string? encKey, FormSequence input, string user)
            {
                var obj = LibExtensions.Load<Sequence>(encKey);
                obj.LastId = input.LastId;
                obj.Update(user).Ok();
            }
        }
    }
}
