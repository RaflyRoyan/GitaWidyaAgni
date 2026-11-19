# Database

MySQL 8, database `gitawidyaagni`, charset `utf8mb4` / collation `utf8mb4_0900_ai_ci`.

| Berkas | Untuk |
|---|---|
| `gitawidyaagni_create.sql` | Membuat database baru lengkap (135 tabel, semua foreign key dan COMMENT kolom). |
| `migrations/YYYY-MM-DD_<nama>.sql` | Perubahan skema setelah create script dibuat. Jalankan berurutan pada database yang sudah ada. |

## Setup database baru

```
mysql -u <user> -p < database/gitawidyaagni_create.sql
```

Lalu:

1. Isi connection string di `GitaWidyaAgniWeb/appsettings.Development.json` (berkas ini di-ignore git; jangan taruh password di `appsettings.json`).
2. Isi data contoh dan akun admin: `dotnet run --project GWASeeder` (lihat README utama untuk akun dan opsi).
   Database produksi tanpa data contoh: jalankan web dengan `Bootstrap:AdminUsername` / `Bootstrap:AdminPassword`, lalu `dotnet run --project GWASeeder -- --sync-akses` untuk menu dan hak akses.

Create script sudah memuat semua migrasi sampai tanggal di kepala berkasnya; migrasi tidak perlu dijalankan lagi setelahnya.

## Database yang sudah ada

Jalankan berkas di `migrations/` yang lebih baru dari skema database tersebut, urut tanggal:

```
mysql -u <user> -p gitawidyaagni < database/migrations/2026-11-17_lpj.sql
mysql -u <user> -p gitawidyaagni < database/migrations/2026-11-18_section.sql
mysql -u <user> -p gitawidyaagni < database/migrations/2026-11-18_asset_qr_token.sql
mysql -u <user> -p gitawidyaagni < database/migrations/2026-11-18_activity_absence.sql
mysql -u <user> -p gitawidyaagni < database/migrations/2026-11-19_attendance_mode.sql
mysql -u <user> -p gitawidyaagni < database/migrations/2026-11-19_attendance_reminder.sql
```

## Aturan perubahan skema

- Tulis perubahan sebagai berkas migrasi baru di `migrations/`, lalu buat ulang create script dari database pengembangan:
  `mysqldump --no-data --skip-add-drop-table --routines --triggers --events --set-gtid-purged=OFF --skip-comments --no-tablespaces gitawidyaagni`
  dan pasang kembali kepala berkas (CREATE DATABASE + USE).
- Perbarui juga entity EF di `GWALib/EF/` (scaffold ulang atau tulis tangan) dan enum di `GWAShared/Enums/` sesuai COMMENT kolom.
- Tidak ada AUTO_INCREMENT: id dari `m_sequence` lewat `SequenceUtility`, kecuali `t_election_vote.id` (acak).
