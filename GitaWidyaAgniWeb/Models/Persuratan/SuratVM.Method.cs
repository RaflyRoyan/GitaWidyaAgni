using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Master.Surat;
using GWALib.Lib.Transaksi.Surat;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Persuratan
{
    public partial class SuratVM
    {
        public class Method
        {
            public static Letter Load(string? key) => LibExtensions.Load<Letter>(key, "Surat");

            public static void SimpanSurat(NewSurat input, byte[]? file, IFormFile? meta, string user)
                => Letter.Save(new Letter.NewRow
                {
                    Direction = (LetterDirection)input.Direction,
                    PeriodId = input.PeriodId,
                    TemplateId = input.TemplateId,
                    LetterType = input.LetterType,
                    LetterNumber = input.LetterNumber,
                    Subject = input.Subject ?? "",
                    LetterDate = Opsi.Tanggal(input.LetterDate),
                    ReceivedDate = Opsi.Tanggal(input.ReceivedDate),
                    Sender = input.Sender,
                    Recipient = input.Recipient,
                    Summary = input.Summary,
                    Body = input.Body,
                    FileData = file,
                    FileName = meta?.FileName,
                    ContentType = meta?.ContentType
                }, user).Ok();

            public static void UpdateSurat(string key, NewSurat input, byte[]? file, IFormFile? meta, bool hapusFile, string user)
            {
                var obj = Load(key);
                obj.PeriodId = input.PeriodId;
                obj.TemplateId = input.TemplateId;
                obj.LetterType = input.LetterType ?? obj.LetterType;
                obj.LetterNumber = input.LetterNumber;
                obj.Subject = input.Subject ?? "";
                obj.LetterDate = Opsi.Tanggal(input.LetterDate);
                obj.ReceivedDate = Opsi.Tanggal(input.ReceivedDate);
                obj.Sender = input.Sender;
                obj.Recipient = input.Recipient;
                obj.Summary = input.Summary;
                obj.Body = input.Body;
                obj.Update(user).Ok();
                if (file != null)
                {
                    obj.UpdateFile(file, meta!.FileName, meta.ContentType, user).Ok();
                }
                else if (hapusFile)
                {
                    obj.UpdateFile(null, null, null, user).Ok();
                }
            }

            public static void TambahDisposisi(DisposisiInput input, string user)
            {
                var obj = Load(input.Key);
                LetterDisposition.Save(new LetterDisposition.NewRow { LetterId = obj.Id, ToMemberId = input.ToMemberId, Instruction = input.Instruction ?? "", DueDate = Opsi.Tanggal(input.DueDate) }, user).Ok();
            }

            public static LetterDisposition LoadDisposisi(string? key) => LibExtensions.Load<LetterDisposition>(key, "Disposisi");

            public static void SimpanTemplate(NewTemplate input, byte[]? file, string? fileName, string user)
                => LetterTemplate.Save(new LetterTemplate.NewRow { Name = input.Name ?? "", LetterType = input.LetterType ?? "", Body = input.Body, FileData = file, FileName = fileName }, user).Ok();

            public static void UpdateTemplate(string key, NewTemplate input, byte[]? file, string? fileName, bool hapusFile, string user)
            {
                var obj = LoadTemplate(key);
                obj.Name = input.Name ?? "";
                obj.LetterType = input.LetterType ?? "";
                obj.Body = input.Body;
                obj.IsActive = (ActiveStatus)input.IsActive;
                obj.Update(user).Ok();
                if (file != null)
                {
                    obj.UpdateFile(file, fileName, user).Ok();
                }
                else if (hapusFile)
                {
                    obj.UpdateFile(null, null, user).Ok();
                }
            }

            public static LetterTemplate LoadTemplate(string? key) => LibExtensions.Load<LetterTemplate>(key, "Template surat");
        }
    }
}
