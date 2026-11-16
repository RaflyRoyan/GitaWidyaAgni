# GitaWidyaAgni

Library backend .NET 8 untuk sistem manajemen **satu organisasi**: anggota dan kepengurusan, program dan kegiatan, presensi, keuangan, persuratan, e-voting, pengadaan, sponsorship, inventaris, dan lain-lain.

Organisasi adalah level tertinggi. Tidak ada tenant, kampus, kategori UKM, atau pegawai.

> Status: dalam pengembangan. Layer service sudah ada untuk semua tabel master (`GWALib/Lib/Master`) dan semua 88 tabel transaksi (`GWALib/Lib/Transaksi`). Modul sampai Presensi sudah diuji ke database. Modul Keuangan sampai Information baru lolos build dan belum diuji ke database.

## Struktur solution

```
GitaWidyaAgni.sln
├── GWAShared/        Helper tanpa dependency
│   ├── Enums/        Satu enum per file untuk setiap kolom berkode (namespace GWAShared.Enums)
│   ├── Extensions/   MappingData, enkripsi string, enum, decimal, value
│   ├── Models/       EnumItem, SelectListItemCustom
│   ├── Results/      Response<T>, ResponseFactory, ResponseMessages, ExceptionMessage
│   └── Utilities/    Crypto, File, Session, Validate
└── GWALib/           Akses data dan logika bisnis (mereferensikan GWAShared)
    ├── EF/           Entity dan ModelContext hasil scaffold database-first (Pomelo MySQL)
    ├── General/      Koneksi, penanganan error, utilitas (id dari m_sequence, gambar, validasi)
    └── Lib/          Service per tabel: Master/<Modul>/ dan Transaksi/<Modul>/
```

Struktur dan konvensinya mengikuti project ClubSphere (ClubLib / ClubShared).

## Prasyarat

- .NET SDK 8.0 atau yang lebih baru
- MySQL 8.0

## Build

```
dotnet build GitaWidyaAgni.sln
```

Belum ada project test.

## Konfigurasi saat startup

Aplikasi yang memakai library ini wajib memanggil dua method berikut sekali saat startup:

```csharp
// Connection string MySQL; jangan di-commit, ambil dari konfigurasi / secret.
DBClass.Configure(connectionString);

// Kunci e-voting: base64 dari 32 byte acak, dari secret aplikasi (bukan database).
// Tanpa kunci ini, membuka pemungutan suara, memilih dan finalisasi akan ditolak.
// Kunci tidak boleh diganti sebelum semua pemilihan yang memakainya difinalisasi.
ElectionVote.ConfigureKey(base64Key);
```

Membuat kunci e-voting, misalnya dengan PowerShell:

```powershell
[Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
```

`OnConfiguring` di `GWALib/EF/ModelContext.cs` sengaja dibiarkan dikomentari supaya connection string tidak ikut ter-commit. Scaffold ulang akan menimpa file EF.

## Job terjadwal

Method berikut disediakan untuk dipanggil job atau scheduler aplikasi. Pakai `ScheduledJobLog` (`Start` / `Finish` / `Fail`) untuk mencatat setiap run.

| Method | Fungsi |
|---|---|
| `Election.CloseExpired()` | Menutup pemilihan yang sudah lewat waktu selesai |
| `BorrowRequest.MarkOverdue(user)` | Menandai peminjaman yang lewat tanggal kembali |
| `ActivitySchedule.GenerateSessions(from, to, activityId, user)` | Membuat sesi presensi dari jadwal rutin |
| `Notification.Purge(olderThanDays)` | Menghapus notifikasi yang sudah dibaca |
| `ActivityLog.Purge(before)` | Menghapus log aktivitas lama |
| `UserToken.Purge(before, user)` | Menghapus token lama |
| `ScheduledJobLog.Purge(startedBefore, user)` | Menghapus log job lama |

Setiap method di atas juga punya versi `...Async`.

## Database

- Skrip pembuatan database: `gitawidyaagni_create.sql` (MySQL 8.0, database `gitawidyaagni`, collation `utf8mb4_0900_ai_ci`). Skrip ini berisi 131 tabel dan 264 relasi.
- ERD: https://claude.ai/artifact/MvmojvM5z8kKz7SEwtjdJV
- Konvensi:
  - Prefix `m_*` untuk data master dan `t_*` untuk data transaksi.
  - Tidak ada `AUTO_INCREMENT`. ID diambil dari `m_sequence`, kecuali `t_election_vote.id` yang dibuat acak.
  - File disimpan sebagai blob.
  - Arti angka pada kolom status/tipe ditulis di `COMMENT` kolom, dan setiap kolom berkode punya enum di `GWAShared/Enums`.
  - Hampir semua tabel punya kolom audit `ins_date`, `ins_by`, `upd_date`, `upd_by`.

## Modul

| Modul | Cakupan |
|---|---|
| Profil Organisasi | Profil, visi misi, kontak, logo, foto, data pembina |
| Pengguna & Hak Akses | User, role per periode, hak aksi, menu, token |
| Notifikasi, Penomoran & Persetujuan | Template pesan, antrian email/WhatsApp, format nomor dokumen, alur persetujuan |
| Anggota & Kepengurusan | Anggota, divisi, periode, jabatan, rekrutmen, serah terima, alumni |
| Program & Kegiatan | Program kerja, timeline, kegiatan, kolaborasi, peserta, laporan, galeri |
| Tim Pelaksana | Sie, panitia, open recruitment, rundown, checklist, manifest rombongan |
| Jadwal & Presensi | Jadwal rutin, sesi presensi (QR + lokasi + foto), notulen rapat |
| Keuangan | Akun kas/bank, transaksi, anggaran, pengajuan dana, iuran, hutang, saldo periode |
| Mitra & Pihak Luar | Brand, organisasi, vendor, beserta kontak PIC |
| Sponsorship | Paket, sponsorship, benefit, MoU |
| Pengadaan Barang | Usulan pembelian, penawaran vendor, persetujuan, penerimaan barang |
| Danus & Merchandise | Produk, varian, stok, pesanan dan pre-order |
| Inventaris | Aset, unit berkode QR, peminjaman, sewa dan denda, inspeksi, perawatan |
| Persuratan & Dokumen | Template surat, surat masuk/keluar, persetujuan, disposisi, arsip |
| E-Voting | Pemilihan dengan suara anonim terenkripsi |
| Prestasi & Sertifikat | Lomba, materi, peserta/tim, prestasi, sertifikat berkode verifikasi, poin keaktifan |
| Formulir & Aspirasi | Formulir dan survei (evaluasi kegiatan, pertanyaan rekrutmen), aspirasi/pengaduan |
| Landing & Informasi | Berita, banner, pengaturan situs, notifikasi, log aktivitas |

## Konvensi kode

- Pesan untuk pengguna ditulis dalam bahasa Indonesia sebagai konstanta di `GWAShared.Results.ExceptionMessage`.
- Hasil service dikembalikan sebagai `Response<T>` yang dibuat lewat `ResponseFactory`.
- `<summary>` setiap enum mencantumkan semua `tabel.kolom` yang memakainya, dan setiap nilainya diberi `[Description]` berbahasa Indonesia.
- `StringExtensions.EncryptString<T>()` / `DecryptString<T>()` memakai kunci yang bergantung pada tanggal hari ini, sehingga id terenkripsi hanya bisa didekripsi pada hari yang sama.
