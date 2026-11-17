using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Master.Inventaris;
using GWALib.Lib.Transaksi.Inventaris;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Inventaris
{
    public partial class AsetVM
    {
        public class Method
        {
            public static Asset Load(string? key) => LibExtensions.Load<Asset>(key, "Aset");

            // Kode unit (QR) → id unit milik aset; kosong = null.
            private static int? UnitId(int assetId, string? code)
            {
                if (string.IsNullOrWhiteSpace(code))
                {
                    return null;
                }
                var unit = AssetUnit.GetByCode(AsetLabel.KodeUnit(code)) ?? throw new ArgumentException("Kode unit " + code + " tidak ditemukan.");
                if (unit.AssetId != assetId)
                {
                    throw new ArgumentException("Kode unit " + code + " bukan milik aset yang dipilih.");
                }
                return unit.Id;
            }

            // ---------- Aset
            public static void SimpanAset(NewAset input, string user)
                => Asset.Save(new Asset.NewRow
                {
                    CategoryId = input.CategoryId ?? 0,
                    AssetCode = (input.AssetCode ?? "").Trim().ToUpperInvariant(),
                    Name = input.Name ?? "",
                    Brand = input.Brand,
                    Specification = input.Specification,
                    Quantity = input.Quantity,
                    Unit = string.IsNullOrWhiteSpace(input.Unit) ? "unit" : input.Unit,
                    TrackUnit = (UnitTracking)input.TrackUnit,
                    UnitPrefix = string.IsNullOrWhiteSpace(input.UnitPrefix) ? null : input.UnitPrefix.Trim().ToUpperInvariant(),
                    AcquisitionDate = Opsi.Tanggal(input.AcquisitionDate),
                    AcquisitionCost = input.AcquisitionCost,
                    AcquisitionSource = (AcquisitionSource)input.AcquisitionSource,
                    Location = input.Location,
                    AssetCondition = (ItemCondition)input.AssetCondition,
                    RentalFeePerDay = input.RentalFeePerDay,
                    LateFeePerDay = input.LateFeePerDay
                }, user).Ok();

            public static void UpdateAset(string key, NewAset input, string user)
            {
                var obj = Load(key);
                obj.Category = new AssetCategory { Id = input.CategoryId ?? 0 };
                obj.AssetCode = (input.AssetCode ?? "").Trim().ToUpperInvariant();
                obj.Name = input.Name ?? "";
                obj.Brand = input.Brand;
                obj.Specification = input.Specification;
                obj.Unit = string.IsNullOrWhiteSpace(input.Unit) ? "unit" : input.Unit;
                obj.AcquisitionDate = Opsi.Tanggal(input.AcquisitionDate);
                obj.AcquisitionCost = input.AcquisitionCost;
                obj.AcquisitionSource = (AcquisitionSource)input.AcquisitionSource;
                obj.Location = input.Location;
                obj.AssetCondition = (ItemCondition)input.AssetCondition;
                obj.RentalFeePerDay = input.RentalFeePerDay;
                obj.LateFeePerDay = input.LateFeePerDay;
                obj.Update(user).Ok();
            }

            // ---------- Unit
            public static void TambahUnit(UnitInput input, string user)
            {
                var obj = Load(input.Key);
                AssetUnit.Save(new AssetUnit.NewRow { AssetId = obj.Id, SerialNumber = input.SerialNumber, UnitCondition = (ItemCondition)input.UnitCondition, Notes = input.Notes }, user).Ok();
            }

            public static void HapusUnit(string? key, string user) => LibExtensions.Load<AssetUnit>(key, "Unit").Delete(user).Ok();

            // Data label QR unit terpilih (harus unit aset ini); setiap unit ditandai label tercetak.
            public static List<Reports.LabelQrUnitReport.Row> SiapkanLabel(LabelInput input, string baseUrl, string user)
            {
                var aset = Load(input.AsetKey);
                var keys = (input.Keys ?? new()).Distinct().ToList();
                if (keys.Count == 0)
                {
                    throw new ArgumentException("Centang unit yang labelnya akan dicetak.");
                }
                var rows = new List<Reports.LabelQrUnitReport.Row>();
                foreach (var key in keys)
                {
                    var unit = LibExtensions.Load<AssetUnit>(key, "Unit");
                    if (unit.AssetId != aset.Id)
                    {
                        throw new ArgumentException("Unit bukan milik aset " + aset.Name + ".");
                    }
                    unit.MarkLabelPrinted(user).Ok();
                    rows.Add(new Reports.LabelQrUnitReport.Row { UnitCode = unit.UnitCode, Url = AsetLabel.PublicUrl(baseUrl, unit.UnitCode), AssetName = aset.Name, SerialNumber = unit.SerialNumber });
                }
                return rows.OrderBy(x => x.UnitCode).ToList();
            }

            // ---------- Foto
            public static void TambahFoto(string? key, byte[]? data, string user)
            {
                var obj = Load(key);
                if (data == null)
                {
                    throw new ArgumentException("Pilih foto terlebih dahulu.");
                }
                AssetPhoto.Save(new AssetPhoto.NewRow { AssetId = obj.Id, Data = data }, user).Ok();
            }

            public static AssetPhoto GetFoto(string? key, int seq)
                => AssetPhoto.GetDataList(Load(key).Id).FirstOrDefault(x => x.SeqPhoto == seq) ?? throw new ArgumentException("Foto tidak ditemukan.");

            // ---------- Mutasi manual
            public static void SimpanMutasi(MutasiInput input, string user)
            {
                var obj = Load(input.Key);
                AssetMovement.Save(new AssetMovement.NewRow { AssetId = obj.Id, MovementType = (AssetMovementType)input.MovementType, Quantity = input.Quantity, Notes = input.Notes }, user).Ok();
            }

            // ---------- Kategori
            public static void SimpanKategori(NewKategori input, string user)
                => AssetCategory.Save(new AssetCategory.NewRow { Name = input.Name ?? "", Description = input.Description }, user).Ok();

            public static void UpdateKategori(string key, NewKategori input, string user)
            {
                var obj = LibExtensions.Load<AssetCategory>(key, "Kategori aset");
                obj.Name = input.Name ?? "";
                obj.Description = input.Description;
                obj.IsActive = (ActiveStatus)input.IsActive;
                obj.Update(user).Ok();
            }

            public static void HapusKategori(string? key, string user) => LibExtensions.Load<AssetCategory>(key, "Kategori aset").Delete(user).Ok();

            // ---------- Perawatan
            public static MaintenanceLog LoadPerawatan(string? key) => LibExtensions.Load<MaintenanceLog>(key, "Perawatan");

            public static void SimpanPerawatan(NewPerawatan input, string user)
            {
                var assetId = input.AssetId ?? 0;
                MaintenanceLog.Save(new MaintenanceLog.NewRow
                {
                    AssetId = assetId,
                    AssetUnitId = UnitId(assetId, input.UnitCode),
                    MaintenanceDate = Opsi.TanggalWajib(input.MaintenanceDate, "Tanggal"),
                    Description = input.Description ?? "",
                    Vendor = input.Vendor,
                    Cost = input.Cost
                }, user).Ok();
            }

            public static void UpdatePerawatan(string key, NewPerawatan input, string user)
            {
                var obj = LoadPerawatan(key);
                obj.MaintenanceDate = Opsi.TanggalWajib(input.MaintenanceDate, "Tanggal");
                obj.Description = input.Description ?? "";
                obj.Vendor = input.Vendor;
                obj.Cost = input.Cost;
                obj.Update(user).Ok();
            }

            public static void SelesaiPerawatan(SelesaiInput input, string user)
                => LoadPerawatan(input.Key).Finish(input.Condition.HasValue ? (ItemCondition)input.Condition.Value : null, input.AccountId, input.CategoryId, user).Ok();

            // ---------- Inspeksi
            public static void SimpanInspeksi(NewInspeksi input, byte[]? foto, string user)
            {
                var assetId = input.AssetId ?? 0;
                AssetInspection.Save(new AssetInspection.NewRow
                {
                    AssetId = assetId,
                    AssetUnitId = UnitId(assetId, input.UnitCode),
                    ItemCondition = (ItemCondition)input.ItemCondition,
                    InspectedAt = input.InspectedAt,
                    Notes = input.Notes,
                    Photo = foto
                }, user).Ok();
            }

            public static AssetInspection LoadInspeksi(string? key) => LibExtensions.Load<AssetInspection>(key, "Inspeksi");
        }
    }
}
