# GitaWidyaAgni

Sistem manajemen **Marching Band Gita Widya Agni (MB GWA) UPN "Veteran" Jawa Timur**. Repo ini berisi:

- **GWALib / GWAShared**: library backend .NET 8 untuk satu organisasi (anggota dan kepengurusan, program dan kegiatan, presensi, keuangan, persuratan, e-voting, pengadaan, sponsorship, inventaris, merchandise, prestasi, formulir, informasi).
- **GitaWidyaAgniWeb**: aplikasi web ASP.NET Core 8 MVC. Berisi situs publik, dashboard pengurus untuk semua modul, Area Anggota, laporan DevExpress, dan job terjadwal.
- **GWASeeder**: console untuk mengisi database dengan data contoh dan katalog hak akses.
- **database/**: skrip SQL pembuatan database dan migrasi skema.

Organisasi adalah level tertinggi. Tidak ada tenant, kampus, kategori UKM, atau pegawai.

> **Status (19 November 2026)**
> - Library lengkap: setiap tabel master dan transaksi punya service class.
> - Web lengkap: landing page, login, 37 halaman pengelolaan, menu LPJ, 11 halaman Area Anggota, dan halaman publik (rekrutmen, verifikasi sertifikat, toko, detail aset dari QR).
> - Presensi tanpa QR (jadwal otomatis, check-in kamera + lokasi, pengingat, pantau langsung, rekap, poin otomatis) dan invoice / kuitansi peminjaman alat sudah dites.
> - Halaman Area Anggota selain Presensi baru dites dengan akun admin; tampilan untuk anggota asli masih perlu dicoba.

## Daftar isi

1. [Perangkat yang dipakai](#perangkat-yang-dipakai)
2. [Setup project dari nol](#setup-project-dari-nol)
3. [Struktur solution](#struktur-solution)
4. [Database](#database)
5. [Konfigurasi](#konfigurasi)
6. [Data contoh (GWASeeder)](#data-contoh-gwaseeder)
7. [Panduan per menu](#panduan-per-menu) (panduan pengguna per peran: [docs/panduan](docs/panduan/README.md))
8. [Laporan (DevExpress Reports)](#laporan-devexpress-reports)
9. [Job terjadwal dan pengiriman pesan](#job-terjadwal-dan-pengiriman-pesan)
10. [Autentikasi dan hak akses](#autentikasi-dan-hak-akses)
11. [Tampilan](#tampilan)
12. [Panduan pengembangan](#panduan-pengembangan)
13. [Deploy ke server](#deploy-ke-server)
14. [Troubleshooting](#troubleshooting)
15. [Catatan keamanan](#catatan-keamanan)

## Perangkat yang dipakai

| Perangkat | Versi | Untuk apa |
|---|---|---|
| **Visual Studio 2022** (disarankan) | 17.8 atau lebih baru, Community cukup | IDE utama. Saat instalasi, centang workload **ASP.NET and web development**. Solution (`GitaWidyaAgni.sln`) dibuat dengan Visual Studio 17. |
| .NET SDK | 8.0 | Ikut terpasang bersama workload di atas. Cek dengan `dotnet --version`. |
| **DevExpress** | **23.2.5** (installer "DevExpress Universal / ASP.NET & Reporting") | Paket DevExtreme (grid, form, chart) dan DevExpress Reporting (laporan PDF) diambil dari folder hasil instalasi, bukan dari nuget.org. Tanpa instalasi ini, project tidak bisa di-restore. Versi trial boleh untuk pengembangan. |
| **MySQL Server** | 8.0 (lewat MySQL Installer) | Database. Pilih juga **MySQL Workbench** kalau ingin melihat tabel lewat GUI. |
| Git | terbaru | Clone dan commit. |
| Browser | Chrome / Edge terbaru | Presensi memakai kamera dan GPS, yang hanya jalan lewat **HTTPS**. |

Alternatif IDE: **VS Code** dengan ekstensi *C# Dev Kit*, atau **JetBrains Rider**. Semua langkah di bawah juga bisa dijalankan lewat terminal (`dotnet build`, `dotnet run`).

Folder paket DevExpress yang dibaca `NuGet.config`:

- `C:\Program Files\DevExpress 23.2\DevExtreme\System\DevExtreme\Bin\AspNetCore` (DevExtreme.AspNet.*)
- `C:\Program Files\DevExpress 23.2\Components\System\Components\Packages` (DevExpress.AspNetCore.Reporting)

Kalau DevExpress dipasang di folder lain, ubah kedua path di `NuGet.config`.

**Lisensi DevExpress:** instalasi trial membuat PDF laporan berisi tulisan *"This document was created using the trial version of DevExpress components"*. Build dengan instalasi berlisensi supaya tulisan itu hilang.

## Setup project dari nol

Langkah dari komputer kosong sampai bisa login. Perintah terminal ditulis untuk **PowerShell** atau **Git Bash** di Windows.

### 1. Pasang perangkat

1. Pasang **Visual Studio 2022** dengan workload **ASP.NET and web development**.
2. Pasang **DevExpress 23.2.5**. Pada pilihan komponen, centang minimal **ASP.NET Core** (DevExtreme) dan **Reporting**. Biarkan folder instalasi bawaan (`C:\Program Files\DevExpress 23.2`).
3. Pasang **MySQL 8.0** lewat MySQL Installer (Server, opsional Workbench). Catat password user `root`.
4. Pasang **Git**.
5. Percayai sertifikat HTTPS lokal (sekali per komputer), supaya browser tidak menolak `https://localhost`:
   ```
   dotnet dev-certs https --trust
   ```

### 2. Clone repo

```
git clone https://github.com/RaflyRoyan/GitaWidyaAgni.git
cd GitaWidyaAgni
```

### 3. Buat database

Perintah ini membuat database `gitawidyaagni` beserta semua tabelnya (struktur saja, tanpa data):

```
mysql -u root -p < database/gitawidyaagni_create.sql
```

- Kalau `mysql` belum ada di PATH, pakai path lengkapnya: `"C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe"`.
- PowerShell tidak mendukung `<`. Jalankan dari **Command Prompt** / **Git Bash**, atau masuk ke `mysql` lalu ketik `source database/gitawidyaagni_create.sql;`.
- Bisa juga lewat MySQL Workbench: **File › Open SQL Script**, pilih file itu, lalu klik ikon petir (Execute).

### 4. Buka solution di Visual Studio

1. Buka `GitaWidyaAgni.sln` (double-click atau **File › Open › Project/Solution**).
2. Visual Studio otomatis melakukan *restore* paket. Kalau gagal mencari `DevExtreme.AspNet.*`, berarti DevExpress belum terpasang atau path di `NuGet.config` salah.
3. Di **Solution Explorer**, klik kanan **GitaWidyaAgniWeb › Set as Startup Project**.
4. Di toolbar, pilih profil **https** (bukan IIS Express) pada tombol Run.

### 5. Isi connection string

Buat file baru `GitaWidyaAgniWeb/appsettings.Development.json`. File ini di-ignore git, jadi setiap orang membuatnya sendiri:

```json
{
  "ConnectionStrings": {
    "Default": "server=127.0.0.1;port=3306;database=gitawidyaagni;user=root;password=PASSWORD_MYSQL_KAMU"
  }
}
```

### 6. Buat kunci e-voting (user-secret)

Kunci ini mengenkripsi suara pemilihan. Simpan sebagai user-secret (sekali per komputer), jangan di file yang di-commit.

- **Lewat Visual Studio:** klik kanan **GitaWidyaAgniWeb › Manage User Secrets**, lalu isi:
  ```json
  {
    "EVoting:Key": "<kunci base64 32 byte>"
  }
  ```
- **Lewat terminal:**
  ```powershell
  $key = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
  dotnet user-secrets set "EVoting:Key" $key --project GitaWidyaAgniWeb
  ```

Untuk mendapatkan kunci acak tanpa terminal, jalankan seeder (langkah 7) tanpa `--evoting-key`; seeder mencetak kunci yang dibuatnya.

### 7. Isi data

Pilih salah satu:

- **Pengembangan, dengan data contoh** (disarankan untuk tim):
  ```
  dotnet run --project GWASeeder -- --evoting-key <kunci dari langkah 6> --admin-password <password admin>
  ```
  Dari Visual Studio: klik kanan **GWASeeder › Properties › Debug › Open debug launch profiles UI**, isi *Command line arguments* `--evoting-key ... --admin-password ...`, lalu klik kanan **GWASeeder › Debug › Start New Instance**.

  Seeder mengisi profil MB GWA, menu, role, 24 akun contoh, dan data semua modul. Daftar akun ada di [Akun contoh](#akun-contoh).
- **Produksi, tanpa data contoh**:
  1. Isi admin awal sebagai user-secret: `Bootstrap:AdminUsername` dan `Bootstrap:AdminPassword`.
  2. Jalankan web sekali; admin hanya dibuat selama tabel `m_user` masih kosong.
  3. Isi menu, controller, dan role: `dotnet run --project GWASeeder -- --sync-akses`.
  4. Setelah login pertama, hapus kedua secret `Bootstrap:*`.

### 8. Jalankan web

- **Visual Studio:** tekan **F5** (dengan debugger) atau **Ctrl+F5** (tanpa debugger). Browser terbuka di `https://localhost:7061`.
- **Terminal:** `dotnet run --project GitaWidyaAgniWeb --launch-profile https`.

Halaman pertama adalah landing page; tombol **Login** ada di navbar. Login dengan `admin` dan password dari langkah 7.

### 9. Lengkapi data awal

Setelah login sebagai admin:

1. **Organisasi › Profil Organisasi**: nama, kode, logo, visi, misi, pembina.
2. **Pengaturan Sistem › Site Setting**: `whatsapp`, `alamat`, `instagram`, `kota`, dan seterusnya (lihat [Konfigurasi](#konfigurasi)).
3. **Organisasi › Periode**: pastikan ada satu periode berstatus Active.
4. Isi data master lain di [Data master yang perlu diisi](#data-master-yang-perlu-diisi-sebelum-dipakai).

### 10. Setiap kali `git pull`

1. Lihat apakah ada berkas baru di `database/migrations/`. Jalankan berkas yang belum pernah dijalankan ke database lokal, urut tanggal (lihat [Database](#database)).
2. Kalau ada halaman baru, jalankan `dotnet run --project GWASeeder -- --sync-akses`.
3. Hentikan web yang sedang berjalan sebelum build ulang (kalau tidak, build gagal `MSB3027`).

## Struktur solution

```
GitaWidyaAgni.sln
├── GWAShared/                 Helper tanpa dependency
│   ├── Enums/                 Satu enum per file untuk setiap kolom berkode (namespace GWAShared.Enums)
│   ├── Extensions/            MappingData, enkripsi string (encKey), enum, decimal, value
│   ├── Models/                EnumItem, SelectListItemCustom
│   ├── Results/               Response<T>, ResponseFactory, ResponseMessages, ExceptionMessage
│   └── Utilities/             Crypto, File, Session, Validate
├── GWALib/                    Akses data dan logika bisnis (mereferensikan GWAShared)
│   ├── EF/                    Entity + ModelContext (database-first, Pomelo MySQL)
│   ├── General/               DBClass, ServiceHandler, SequenceUtility, ImageUtility, RequestInfo
│   └── Lib/
│       ├── Master/<Modul>/    Service tabel master (m_*)
│       └── Transaksi/<Modul>/ Service tabel transaksi (t_*)
├── GitaWidyaAgniWeb/          ASP.NET Core 8 MVC (template Velzon + DevExtreme + DevExpress Reports)
│   ├── Controllers/<Area>/    Satu area per header menu sidebar (Akses, Keuangan, Inventaris, Lpj, AreaAnggota, ...)
│   ├── Models/<Area>/         View model <Nama>VM (Index, List, Row, Add, Edit, NewRow, Method) dan penyusun data laporan (*Cetak)
│   ├── Views/<Area>/<Ctrl>/   Index, _List, _Add, _Edit, _Form
│   ├── Reports/               Laporan DevExpress lewat kode (LPJ, persuratan, sertifikat, label QR, peminjaman, rekap presensi)
│   ├── Infrastructure/        BaseController, AccessFilter, AccessService, ActivityLogFilter, AsetLabel, Jobs/
│   └── wwwroot/               assets/ (Velzon), css/, js/ (AppUI site.js, grid-export.js, gwa-map-picker.js), Scripts/<Area>/, lib/dxreporting-23.2.5/
├── GWASeeder/                 Console data contoh + katalog hak akses (Seed.*.cs)
└── database/
    ├── gitawidyaagni_create.sql   Skema lengkap
    ├── migrations/                Perubahan skema per tanggal
    └── README.md                  Cara pakai skrip database
```

Struktur dan konvensinya mengikuti project ClubSphere (ClubLib / ClubShared). Panduan teknis yang lebih rinci untuk pengembang ada di `CLAUDE.md`.

## Database

### Skrip

| Berkas | Untuk |
|---|---|
| [`database/gitawidyaagni_create.sql`](database/gitawidyaagni_create.sql) | Membuat database baru lengkap: 136 tabel, semua relasi dan `COMMENT` kolom. Isinya hanya struktur, tanpa data. Sudah memuat semua migrasi sampai tanggal di kepala berkas. |
| [`database/migrations/`](database/migrations/) | Perubahan skema setelah create script dibuat, satu berkas per perubahan (`YYYY-MM-DD_<nama>.sql`). Hanya untuk database lama. |
| [`database/README.md`](database/README.md) | Cara pakai dan aturan menambah migrasi. |

- **Database baru:** jalankan create script saja.
- **Database lama:** jalankan migrasi yang belum pernah dijalankan, urut tanggal:
  ```
  mysql -u root -p gitawidyaagni < database/migrations/2026-11-17_lpj.sql
  mysql -u root -p gitawidyaagni < database/migrations/2026-11-18_section.sql
  mysql -u root -p gitawidyaagni < database/migrations/2026-11-18_asset_qr_token.sql
  mysql -u root -p gitawidyaagni < database/migrations/2026-11-18_activity_absence.sql
  mysql -u root -p gitawidyaagni < database/migrations/2026-11-19_attendance_mode.sql
  mysql -u root -p gitawidyaagni < database/migrations/2026-11-19_attendance_reminder.sql
  mysql -u root -p gitawidyaagni < database/migrations/2026-11-20_member_card.sql
  ```

Riwayat migrasi:

| Tanggal | Berkas | Isi |
|---|---|---|
| 2026-11-17 | `2026-11-17_lpj.sql` | Tabel `t_lpj` (LPJ kegiatan / kepengurusan); kolom `t_program.division_id` (bidang) dan `t_program.term` (jangka pendek / panjang) |
| 2026-11-18 | `2026-11-18_section.sql` | Tabel `m_section` (section musik) dan `t_section_member` (riwayat section anggota); kolom `t_activity.section_id` (audiens 6 = Section) dan `t_management_assignment.section_id` (Section Leader) |
| 2026-11-18 | `2026-11-18_asset_qr_token.sql` | Kolom `m_asset_unit.qr_token`: token acak isi QR label, menggantikan kode unit di URL `/aset/...`. Label lama harus dicetak ulang |
| 2026-11-18 | `2026-11-18_activity_absence.sql` | Tabel `t_activity_absence_request`: anggota otomatis ikut kegiatan audiensnya (mis. latihan rutin) dan mengajukan tidak ikut satu pertemuan atau seluruh kegiatan, disetujui pengurus |
| 2026-11-19 | `2026-11-19_attendance_mode.sql` | Kolom `attendance_mode` pada `t_activity_schedule` dan `t_attendance_session` (1 = Tanpa QR, 2 = Wajib scan QR). Data lama otomatis mode QR |
| 2026-11-19 | `2026-11-19_attendance_reminder.sql` | Kolom `reminder_sent_at` dan `checkout_reminder_sent_at` pada `t_attendance_session` (pengingat presensi tidak terkirim dua kali) |
| 2026-11-20 | `2026-11-20_member_card.sql` | Kolom `card_token` (unik) dan `card_issued_at` pada `m_member` untuk QR Kartu Tanda Anggota (KTA) |

### Konvensi skema

- Prefix `m_*` untuk data master dan `t_*` untuk data transaksi.
- **Tidak ada `AUTO_INCREMENT`.** Id diambil dari tabel `m_sequence` lewat `SequenceUtility.GetNextId`; baris `m_sequence` yang belum ada dibuat otomatis dari `MAX(id)`. Pengecualian: `t_election_vote.id` dibuat acak.
- Arti angka pada kolom status/tipe ditulis di `COMMENT` kolom (`1=..., 2=...`), dan setiap kolom berkode punya enum di `GWAShared/Enums`.
- File (foto, dokumen, PDF) disimpan sebagai blob di tabelnya masing-masing dan tidak ikut dimuat di daftar.
- Hampir semua tabel punya kolom audit `ins_date`, `ins_by`, `upd_date`, `upd_by`.
- ERD (131 tabel, sebelum `t_lpj`): https://claude.ai/artifact/MvmojvM5z8kKz7SEwtjdJV

### Grup tabel

| Grup | Contoh tabel |
|---|---|
| Inti & akses | `m_organization`, `m_user`, `m_role`, `m_user_role`, `m_controller`, `m_controller_action`, `m_header_menu`, `m_sub_menu`, `m_role_menu`, `m_sequence` |
| Sistem | `m_site_setting`, `m_document_number_format`, `m_approval_flow(_step)`, `m_message_template`, `t_message_outbox`, `t_notification`, `t_activity_log`, `t_scheduled_job_log`, `t_user_token` |
| Anggota | `m_member`, `m_section`, `t_section_member`, `m_division`, `m_position`, `m_management_period`, `t_management_assignment`, `t_recruitment(_applicant)`, `t_period_handover(_item)`, `m_alumni_profile` |
| Program & kegiatan | `t_program`, `t_program_task`, `t_timeline_plan`, `t_activity`, `t_activity_participant`, `t_activity_absence_request`, `t_activity_report`, `t_gallery(_photo)`, `t_lpj` |
| Tim pelaksana & presensi | `t_committee_*`, `t_activity_rundown`, `t_activity_checklist`, `t_activity_schedule`, `t_attendance_session`, `t_attendance_record`, `t_meeting_minutes` |
| Keuangan | `m_financial_account`, `m_financial_category`, `t_financial_transaction`, `t_budget(_item)`, `t_funding_request`, `m_dues_setting`, `t_dues_bill`, `t_debt`, `t_period_balance` |
| Mitra, sponsorship, pengadaan | `m_external_party(_contact)`, `m_sponsorship_package`, `t_sponsorship`, `t_sponsorship_mou`, `t_procurement_*` |
| Merchandise & inventaris | `m_product(_variant)`, `t_product_stock_movement`, `t_sales_order(_item)`, `m_asset`, `m_asset_unit`, `t_borrow_request`, `t_borrow_item(_unit)`, `t_asset_inspection`, `t_maintenance_log` |
| Surat, prestasi, formulir, e-voting, informasi | `t_letter`, `t_document`, `t_competition`, `t_achievement`, `t_certificate`, `t_member_point`, `t_form*`, `t_aspiration`, `t_election*`, `t_announcement`, `m_banner` |

### Mengubah skema

1. Tulis berkas migrasi baru di `database/migrations/` lalu jalankan ke database pengembangan.
2. Perbarui entity EF di `GWALib/EF/`, lewat scaffold ulang Pomelo atau ditulis tangan. Scaffold menimpa file EF; biarkan `OnConfiguring` tetap dikomentari.
3. Tambah atau ubah enum di `GWAShared/Enums/` sesuai `COMMENT` kolom.
4. Perbarui `database/gitawidyaagni_create.sql` (perintah `mysqldump` ada di `database/README.md`) dan tambahkan baris di tabel riwayat migrasi di atas.

## Konfigurasi

### appsettings / user-secrets / environment

Nilai rahasia (password, token, kunci) **tidak boleh** masuk `appsettings.json`. Simpan di `appsettings.Development.json` (di-ignore git), di user-secrets (Visual Studio: **Manage User Secrets**; terminal: `dotnet user-secrets set "<key>" "<nilai>" --project GitaWidyaAgniWeb`), atau di environment variable server (titik dua jadi `__`, misalnya `ConnectionStrings__Default`).

| Key | Wajib | Fungsi |
|---|---|---|
| `ConnectionStrings:Default` | Ya | Connection string MySQL. Web gagal start kalau kosong. |
| `EVoting:Key` | Untuk e-voting | Base64 dari 32 byte acak. Tanpa kunci ini, membuka pemungutan suara, memilih, dan finalisasi ditolak. **Jangan diganti** sebelum semua pemilihan yang memakainya difinalisasi. Seeder dan web harus memakai kunci yang sama. |
| `Bootstrap:AdminUsername` / `Bootstrap:AdminPassword` | Sekali | Membuat role SUPERADMIN + user admin selama `m_user` kosong. Hapus setelah login pertama. |
| `App:PublicUrl` | Produksi | URL publik aplikasi, misalnya `https://gwa.example.org`. Dipakai sebagai isi QR label aset dan QR sertifikat. Kalau kosong, dipakai host request; label yang dicetak dari `localhost` tidak bisa dibuka dari HP. |
| `Jobs:Enabled` | - | Bawaan `true`. Pada server dengan beberapa instance, aktifkan di satu instance saja. Saat menguji dengan data asli, set `false` supaya sesi presensi tidak ditutup otomatis. |
| `Jobs:Username` | - | User yang menjalankan job. Kosong = user SUPERADMIN aktif pertama. |
| `Jobs:NotificationRetentionDays` / `ActivityLogRetentionDays` / `TokenRetentionDays` / `JobLogRetentionDays` / `SessionDaysAhead` | - | Bawaan 90 / 365 / 30 / 90 / 7. |
| `Smtp:Host`, `Port`, `EnableSsl`, `Username`, `Password`, `From`, `FromName` | Untuk email | Pengiriman email dari antrian `t_message_outbox`. |
| `WhatsApp:Url`, `WhatsApp:Token` | Untuk WhatsApp | Gateway HTTP gaya Fonnte (POST form `target` + `message`, token di header `Authorization`), misalnya `https://api.fonnte.com/send`. |

Saluran email atau WhatsApp yang belum dikonfigurasi tidak dikirim, dan pesannya tetap berstatus Menunggu sampai dikonfigurasi.

### Site setting (dari menu Pengaturan Sistem)

| Key `m_site_setting` | Dipakai di |
|---|---|
| `hero_title`, `hero_subtitle` | Hero landing page |
| `alamat`, `instagram`, `youtube`, `tiktok` | Kontak dan footer landing page, kop surat laporan |
| `whatsapp` | Tombol WhatsApp: landing, konfirmasi pesanan toko, halaman detail aset |
| `kota` | Kota pada tanda tangan laporan (bawaan Surabaya) |

### Template pesan (dari menu Pengaturan Sistem)

| Kode | Saluran | Dipakai untuk | Placeholder |
|---|---|---|---|
| `RESET_PASSWORD`, `VERIFY_EMAIL` | Email | Lupa password, verifikasi email | `{name} {link} {app}` |
| `DUES_BILL` | Email / WhatsApp | Tagihan iuran | `{name} {period} {amount} {due}` |
| `BORROW_DUE` | WhatsApp | Peminjaman jatuh tempo | `{name} {number} {due}` |
| `ATTENDANCE_REMINDER` | WhatsApp | Pengingat presensi dibuka | `{name} {session} {time} {place}` |
| `ATTENDANCE_CHECKOUT` | WhatsApp | Pengingat check-out | `{name} {session} {time} {place}` |
| `BIRTHDAY` | WhatsApp | Ucapan ulang tahun | `{name} {age} {org}` |
| `ALUMNI_INVITE` | WhatsApp | Undangan alumni (mis. reuni) | `{name} {title} {message} {link}` |

Template WhatsApp presensi bersifat opsional: tanpa template, pengingat tetap dikirim sebagai notifikasi in-app.

### Data master yang perlu diisi sebelum dipakai

- **Profil organisasi**: nama, kode (untuk `{org}` di nomor dokumen), logo, visi, misi, dan nama pembina (tanda tangan LPJ kepengurusan).
- **Periode kepengurusan** yang berstatus Active, beserta section, divisi dan jabatan. Setiap anggota wajib punya section (Brass, Battery, Front Ensemble, Colour Guard); divisi (Humas, Logistik, Danus, ...) hanya untuk anggota pilihan Ketua Umum. Penanda tangan laporan dicari dari jabatan "Ketua Umum", "Sekretaris", dan "Bendahara".
- **Format nomor dokumen** dan **alur persetujuan** untuk `FUNDING_REQUEST`, `PROCUREMENT`, dan `LETTER`. Tanpa alur aktif, pengajuan dana dan pengadaan tidak bisa diajukan.
- **Akun keuangan** dan **kategori**, termasuk kategori untuk iuran, sewa alat, denda, penjualan, dan sponsorship. Akun Bank / E-Wallet aktif tampil sebagai rekening transfer di invoice peminjaman.
- **Aturan poin** (Prestasi › Aturan Poin) untuk Hadir Kegiatan, Panitia, Pemateri, Pengurus, dan Prestasi.

## Data contoh (GWASeeder)

```
dotnet run --project GWASeeder                    # database kosong: isi data contoh
dotnet run --project GWASeeder -- --reset         # KOSONGKAN semua tabel lalu isi ulang (ketik RESET; --yes melewati konfirmasi)
dotnet run --project GWASeeder -- --sync-akses    # hanya sinkron controller/aksi, menu, dan role di database yang sudah berisi
```

Opsi tambahan: `--evoting-key <base64>` dan `--admin-password <password>`.

- Mengisi 134 dari 136 tabel lewat service GWALib, jadi semua validasi ikut berjalan. `t_lpj` (LPJ dibuat dari web) dan `t_document_signature` (terisi saat laporan dicetak) tidak diisi. Datanya meliputi:
  - controller + aksi, menu sidebar (termasuk **Area Anggota**), role, dan hak akses;
  - profil MB GWA;
  - dua periode kepengurusan, dengan serah terima dari periode lama ke periode baru;
  - 24 anggota dengan jabatan berbeda (lihat tabel akun);
  - kegiatan, presensi, keuangan, sponsorship, pengadaan, merchandise, inventaris, surat, prestasi, formulir, e-voting, berita, dan banner.
- User `admin` (SUPERADMIN) dibuat otomatis bila `m_user` kosong. Password diambil dari `--admin-password`, env `GWA_ADMIN_PASSWORD`, atau bawaan `admin12345`. Ganti setelah login pertama.
- Tanpa `--reset`, seeder menolak berjalan bila `m_organization` sudah berisi.
- `--sync-akses` memakai katalog di `GWASeeder/Seed.Akses.cs`. Jalankan setiap kali menambah halaman web baru. Aksi, menu role, dan detail menu milik role katalog disamakan persis dengan katalog, jadi perubahan manual pada role itu ikut tertimpa. Role lain dibiarkan.
- Connection string diambil dari env `GWA_DB`, lalu `GitaWidyaAgniWeb/appsettings.Development.json`, lalu `appsettings.json`.
- Kunci e-voting diambil dari `--evoting-key`, env `GWA_EVOTING_KEY`, atau `EVoting:Key`. Kalau tidak ada, kunci dibuat acak dan dicetak di akhir. Simpan kunci itu ke `EVoting:Key` web.

### Akun contoh

Password semua akun anggota: `gwa12345`. Akun ini hanya untuk pengembangan.

| Username | Nama | Jabatan (periode aktif) | Role |
|---|---|---|---|
| `admin` | Administrator | - | SUPERADMIN |
| `ketua` | Rizky Pratama Putra | Ketua Umum | KETUA, ANGGOTA |
| `wakil` | Nabila Ayu Lestari | Wakil Ketua Umum | WAKIL, ANGGOTA |
| `sekretaris` | Dimas Arya Saputra | Sekretaris | SEKRETARIS, ANGGOTA |
| `bendahara` | Salsabila Putri Ramadhani | Bendahara | BENDAHARA, ANGGOTA |
| `leader.brass` | Aditya Wicaksono | Section Leader Brass | SECTION_LEADER, ANGGOTA |
| `leader.battery` | Bagas Setiawan | Section Leader Battery | SECTION_LEADER, ANGGOTA |
| `leader.guard` | Citra Maharani | Section Leader Colour Guard | SECTION_LEADER, ANGGOTA |
| `leader.front` | Dewi Kartika Sari | Section Leader Front Ensemble | SECTION_LEADER, ANGGOTA |
| `kadiv.logistik` | Eko Prasetyo | Kepala Divisi Logistik | KADIV, ANGGOTA |
| `kadiv.humas` | Farah Aulia | Kepala Divisi Humas & Media | KADIV, ANGGOTA |
| `ketua.lama` | Fajar Nugroho | Anggota (Ketua Umum periode lalu) | ANGGOTA |
| `anggota01` … `anggota11` | 11 anggota biasa | Anggota (punya section; beberapa juga di divisi) | ANGGOTA |
| `alumni01`, `alumni02` | 2 alumni | Alumni | - |

Hak akses per role:

| Role | Menu pengelolaan |
|---|---|
| SUPERADMIN | Semua, termasuk Akses (User, Role, Menu) dan Pengaturan Sistem |
| KETUA, WAKIL | Semua modul operasional dan LPJ |
| SEKRETARIS | Anggota, Alumni, Rekrutmen, Kegiatan, LPJ, Presensi, Surat, Dokumen, Sertifikat, Formulir, Aspirasi, Berita |
| BENDAHARA | Transaksi, Anggaran, Pengajuan Dana, Iuran, Hutang, Pengadaan, Merchandise, Sponsorship |
| KADIV | Program Kerja, Kegiatan, Tim Pelaksana, Presensi, Galeri, Aset, Peminjaman, Prestasi, Berita |
| SECTION_LEADER | Section, Kegiatan, Presensi, Galeri, Aset, Peminjaman, Prestasi |
| ANGGOTA | Hanya Area Anggota (semua role juga mendapat menu ini) |

Siapa yang boleh menyetujui dokumen tetap ditentukan alur persetujuan. Pada data contoh, pengajuan dana dan pengadaan disetujui Bendahara lalu Ketua Umum, dan surat keluar disetujui Sekretaris lalu Ketua Umum.

## Panduan per menu

Bagian ini menjelaskan **untuk apa** setiap menu, siapa yang biasanya memakainya, dan alur kerjanya. URL dashboard berbentuk `/<Area>/<Controller>`. Semua grid punya pencarian, filter, dan **export Excel + PDF**; tombol **Tampilkan Legenda** menjelaskan arti setiap tombol aksi di grid.

Panduan langkah demi langkah untuk pengguna akhir, per peran (anggota, ketua / wakil, sekretaris, bendahara, kadiv / section leader, administrator), ada di [docs/panduan](docs/panduan/README.md).

### Halaman publik (tanpa login)

| URL | Untuk apa |
|---|---|
| `/` | Wajah organisasi untuk umum: hero dengan banner, tentang MB GWA, seksi musik, agenda, prestasi, galeri, berita, ajakan open recruitment, dan kontak. Isinya diambil dari menu Profil Organisasi, Banner, Kegiatan (publik), Prestasi (publik), Galeri, dan Berita. |
| `/Landing/Berita`, `/berita/{slug}` | Membaca berita dan pengumuman publik. |
| `/rekrutmen/{id?}` | Calon anggota mendaftar open recruitment tanpa akun, termasuk menjawab pertanyaan tambahan dari formulir. |
| `/sertifikat/verifikasi?kode=` | Siapa saja bisa mengecek keaslian sertifikat dari kode / QR yang tercetak di sertifikat. |
| `/toko` | Pembeli umum memesan merchandise (nama + WhatsApp), konfirmasi lewat WhatsApp, dan melacak pesanan dengan nomor pesanan + nomor WhatsApp. |
| `/aset/{token}` | Hasil scan label QR alat: detail unit, kondisi, status, lokasi, dan foto. Harga, tarif, dan data peminjam tidak ditampilkan. |

### Dashboard

Halaman pertama setelah login. Menampilkan sapaan dan periode aktif, kartu ringkas (jumlah anggota, kegiatan mendatang, saldo kas, iuran belum lunas), grafik arus kas 6 bulan dan anggota per section, agenda terdekat, daftar **Perlu tindakan** (pengajuan yang menunggu persetujuan Anda, izin kegiatan, disposisi, iuran sendiri), 5 poin tertinggi, dan pintasan menu. Bagian yang tampil menyesuaikan hak akses. Bila ada anggota yang berulang tahun dalam 7 hari ke depan, muncul kartu **Ulang tahun** (hari ini ditandai).

Akun yang terhubung ke data anggota juga melihat **Ringkasan saya** di atas: kehadiran periode aktif dan bulan ini (merah + peringatan bila di bawah 75%), poin dan peringkat periode aktif, iuran belum dibayar dan jatuh tempo terdekat, kepanitiaan mendatang, jumlah prestasi / sertifikat / peminjaman berjalan, serta jadwal presensi 7 hari ke depan dengan tombol Check-in / Check-out. Setiap kartu menuju halaman Area Anggota terkait.

### Akses (SUPERADMIN)

| Menu | Untuk apa |
|---|---|
| **User** | Membuat akun login, menghubungkan akun ke data anggota, memberi role (boleh terikat periode kepengurusan), mengaktifkan / menonaktifkan akun, dan reset password. |
| **Role** | Membuat role (KETUA, BENDAHARA, ...) dan mencentang aksi serta menu yang boleh dibuka role itu. |
| **Menu** | Mengatur controller, daftar aksinya, header menu sidebar, dan sub menu. Biasanya cukup lewat `--sync-akses`; menu ini untuk penyesuaian manual. |

### Pengaturan Sistem (SUPERADMIN)

Satu halaman dengan beberapa tab untuk mengatur perilaku aplikasi:

- **Site Setting**: teks landing page, kontak, kota tanda tangan (tabel di [Konfigurasi](#site-setting-dari-menu-pengaturan-sistem)).
- **Format Nomor**: pola nomor dokumen per jenis (surat, pengajuan dana, pengadaan, sertifikat, peminjaman, ...).
- **Alur Persetujuan**: urutan pemberi persetujuan per jenis dokumen (per role, per jabatan, atau user tertentu).
- **Template Pesan**: isi email / WhatsApp / in-app (tabel [Template pesan](#template-pesan-dari-menu-pengaturan-sistem)).
- **Sequence**: nomor id terakhir per tabel (hanya bisa dinaikkan).
- **Log Job**, **Antrian Pesan** (kirim ulang yang gagal), dan **Log Aktivitas** (siapa mengubah apa, kapan, dari IP mana).

### Organisasi

| Menu | Untuk apa |
|---|---|
| **Profil Organisasi** | Identitas MB GWA: nama, kode, logo, deskripsi, visi, misi, kontak, data pembina + SK, dan foto organisasi. Dipakai di landing page, kop surat, dan tanda tangan laporan. |
| **Periode** | Periode kepengurusan (Draft → Active → Closed) dan **serah terima** ke periode berikutnya: daftar barang / dokumen / MoU yang diserahkan, berkas LPJ, diajukan lalu disetujui. Persetujuan menutup periode lama dan mengaktifkan periode baru. |
| **Section** | Section musik (Brass, Battery, Front Ensemble, Colour Guard) dan **riwayat section** tiap anggota. Anggota bisa dipindah section kapan saja; riwayatnya tersimpan. |
| **Divisi** | Divisi kepanitiaan organisasi (Humas, Logistik, Danus, ...) dan anggota pilihan Ketua Umum di tiap divisi. |
| **Jabatan** | Daftar jabatan pengurus (Ketua Umum, Sekretaris, Bendahara, Kepala Divisi, Section Leader, ...), termasuk penanda jabatan inti. |
| **Kepengurusan** | Menugaskan anggota ke jabatan pada periode (bisa memimpin divisi atau section) dan mengakhiri penugasan. Data ini dipakai untuk penanda tangan laporan dan hak menyetujui. |

### Anggota

| Menu | Untuk apa |
|---|---|
| **Data Anggota** | Buku induk anggota: NIA, NIM, fakultas, kontak, foto, section, jenis (anggota / pengurus), status (aktif / alumni / keluar), dan akun login. Tombol riwayat section ada di sini. Kolom **Lama Bergabung** dihitung dari tanggal bergabung. **Cetak KTA** (per anggota, seukuran kartu depan-belakang) dan **Cetak KTA Massal** (A4, semua anggota aktif atau satu section) membuat Kartu Tanda Anggota dengan QR verifikasi; **Ganti QR KTA** dipakai bila kartu hilang (QR lama langsung tidak berlaku). |
| **Alumni** | Profil alumni (pekerjaan, kontak, kesediaan dihubungi) untuk anggota berstatus Alumni. **Undang Alumni** mengirim undangan (mis. reuni) ke semua alumni atau satu angkatan, bisa ditautkan ke kegiatan: notifikasi aplikasi untuk alumni yang punya akun, WhatsApp (template `ALUMNI_INVITE`) hanya untuk yang bersedia dihubungi. |
| **Rekrutmen** | Membuka pendaftaran anggota baru (Draft → Open → Closed), kuota, formulir tambahan, lalu menyeleksi pelamar. Pelamar yang diterima otomatis menjadi data anggota (wajib pilih section). |

### Program Kerja

Rencana kerja periode: program per **bidang** (divisi) dan **jangka** (pendek / panjang), status (Planned → Ongoing → Done / Cancelled), tugas per program (Belum → Proses → Selesai), dan **timeline** bulanan yang bisa dihubungkan ke kegiatan. Data ini menjadi bahan evaluasi program di LPJ kepengurusan.

### Kegiatan

| Menu | Untuk apa |
|---|---|
| **Daftar Kegiatan** | Semua kegiatan: latihan, rapat, event, lomba, piket. Atur jenis, **audiens** (semua anggota, pengurus, divisi, section, peserta terdaftar, tim pelaksana), waktu, lokasi + koordinat, dan publik / disorot di landing. Di sini juga peserta, kolaborator (mitra), laporan kegiatan (diajukan → disetujui), dan **pengajuan tidak ikut** dari anggota (setujui / tolak). |
| **Tim Pelaksana** | Kepanitiaan per kegiatan: sie (seksi), anggota panitia dan perannya (Ketua Pelaksana, Bendahara, Koordinator Sie, Staf), open recruitment panitia, rundown acara, checklist persiapan (bisa dari template), dan daftar rombongan perjalanan beserta surat izin. |
| **Presensi** | Mengatur kehadiran anggota. Lihat penjelasan lengkap di bawah tabel. |
| **Galeri** | Album foto kegiatan (Draft → Published → Archived) dan foto sampul. Album publik tampil di landing page dan menjadi lampiran dokumentasi LPJ. |

**Presensi** terdiri dari empat tab:

- **Jadwal Rutin**: jadwal mingguan (mis. latihan Selasa & Jumat 18.30–21.00). Isi hari, jam, toleransi terlambat, **mode presensi**, dan **titik lokasi + radius** lewat peta (klik peta, tombol *Lokasi saya*, atau *Lokasi kegiatan*). Begitu disimpan, sesi 7 hari ke depan langsung dibuat, lalu job menambah sesi berikutnya. Mengubah atau menonaktifkan jadwal ikut menyesuaikan sesi yang belum dipakai.
- **Sesi**: satu pertemuan yang diabsen. Dibuat otomatis dari jadwal rutin, otomatis untuk kegiatan non-rutin yang punya koordinat (satu sesi dari jam mulai sampai selesai), atau manual.
  - **Mode Tanpa QR**: anggota langsung check-in dari Area Anggota › Presensi.
  - **Mode Wajib Scan QR** (cocok untuk rapat): anggota harus memindai QR yang ditampilkan pengurus (**Tampilkan QR**, bisa layar penuh / proyektor).
  - Sesi **dibuka otomatis 30 menit sebelum mulai** dan **ditutup otomatis setelah selesai**; yang tidak hadir dicatat Alpa (atau Izin bila izinnya disetujui), dan yang hadir langsung mendapat **poin Hadir Kegiatan**. Tombol Buka / Tutup manual tetap ada.
  - **Pantau langsung**: daftar siapa yang sudah / belum hadir, jam masuk dan keluar, jarak dari titik (merah bila di luar radius), dan foto selfie; diperbarui tiap 15 detik.
  - **Rekap & presensi manual**: mencatat Izin / Sakit / Alpa / Hadir oleh pengurus atau mengoreksi Alpa otomatis (poin ikut disesuaikan).
  - **Cetak Rekap Kehadiran**: laporan per anggota untuk rentang tanggal, kegiatan, dan section tertentu.
- **Notulen**: notulen rapat untuk sesi yang sudah dibuka / ditutup (agenda, pembahasan, keputusan, tindak lanjut, lampiran), Draft → Final.
- **Statistik**: grafik keaktifan dari sesi yang sudah ditutup. Saring 3 / 6 / 12 bulan terakhir, section, atau satu anggota. Isinya ringkasan (persen hadir, hadir, izin / sakit, alpa), grafik tren per bulan (batang bertumpuk + garis % hadir dengan batas 75%), grafik % hadir per section, dan **daftar anggota dengan kehadiran di bawah 75%** (minimal 3 catatan). Klik nama anggota untuk melihat tren anggota itu.

### LPJ

Menyusun Laporan Pertanggungjawaban **kegiatan** atau **kepengurusan** secara otomatis dari data aplikasi, lalu mencetak / memfinalkan PDF-nya. Lihat [Laporan](#lpj-menu-lpj).

### Keuangan

| Menu | Untuk apa |
|---|---|
| **Transaksi** | Buku kas: pemasukan dan pengeluaran per akun (kas, bank, e-wallet) dan kategori, lampiran bukti, Draft → Posted → Void. Tab **Saldo Periode** membuka dan menutup saldo per akun per periode (setelah ditutup, transaksi periode itu terkunci). Transaksi dari modul lain (iuran, peminjaman, penjualan, sponsorship, pengadaan) muncul di sini otomatis. |
| **Anggaran** | RAB per periode: item anggaran (qty × harga satuan), diajukan lalu disetujui, dan realisasi tiap item dari transaksi. |
| **Pengajuan Dana** | Anggota / panitia mengajukan dana untuk kegiatan (mengacu ke item RAB), disetujui sesuai alur, dicairkan (otomatis membuat transaksi pengeluaran), lalu dipertanggungjawabkan dengan realisasi dan bukti; sisa dana dikembalikan atau kekurangan dibayarkan. |
| **Iuran** | Mengatur jenis iuran (bulanan / semester / tahunan / sekali), membuat tagihan untuk anggota aktif, mencatat pembayaran (otomatis jadi pemasukan), membebaskan tagihan. Anggota melihat tagihannya di Area Anggota. |
| **Hutang** | Mencatat hutang organisasi (termasuk pinjaman kas), pembayaran cicilan, lampiran, dan penghapusan. |

### Inventaris

| Menu | Untuk apa |
|---|---|
| **Aset** | Daftar alat musik dan barang: kategori, jumlah, kondisi, lokasi, harga, tarif sewa & denda per hari. Aset **per unit** (mis. 12 snare) punya kode unit dan **label QR** yang bisa dicetak. Juga foto, mutasi masuk / keluar, inspeksi kondisi, dan jadwal perawatan (biaya perawatan bisa langsung jadi pengeluaran). |
| **Peminjaman** | Peminjaman alat oleh anggota, organisasi / instansi, atau perorangan luar: pengajuan → disetujui → **serahkan barang** (scan QR unit) → pengembalian (scan per unit atau per jumlah, catat kondisi / hilang). Menghitung sewa dan denda keterlambatan, mencatat pembayaran, membebaskan denda, serta **cetak invoice** dan **kuitansi** (per pembayaran atau gabungan). |

### Pengadaan

Pembelian barang organisasi: usulan dengan item dan perkiraan harga (peringatan bila melebihi sisa anggaran), penawaran vendor, persetujuan sesuai alur, pembelian (dibayar langsung atau lewat pengajuan dana), lalu penerimaan barang. Barang aset yang diterima bisa langsung tercatat sebagai aset inventaris.

### Merchandise

Danus / penjualan merchandise: produk dan varian (ukuran, warna), stok (masuk, penyesuaian, rusak), pre-order dengan batas waktu, pesanan dari anggota dan dari toko publik `/toko`, pembayaran (otomatis jadi pemasukan), status siap diambil → selesai, dan pembatalan (stok dikembalikan).

### Kemitraan

| Menu | Untuk apa |
|---|---|
| **Mitra** | Daftar pihak luar (sponsor, vendor, kampus, komunitas, media) dan kontak personnya; mitra bisa diblacklist. |
| **Sponsorship** | Paket sponsor beserta benefitnya, kerja sama sponsorship (dana atau barang), checklist pemenuhan benefit dengan bukti, penerimaan dana (jadi pemasukan), dan MoU (nomor otomatis, dokumen). |

### Persuratan

| Menu | Untuk apa |
|---|---|
| **Surat** | Template surat, surat keluar (draft → persetujuan → nomor otomatis → dikirim → arsip) dan surat masuk, disposisi ke anggota, serta cetakan surat berkop, lembar disposisi, dan buku agenda. |
| **Arsip Dokumen** | Menyimpan dokumen penting (SK, AD/ART, proposal, ...) per kategori dengan hak lihat, dan mencetak daftar arsip. |

### Prestasi

| Menu | Untuk apa |
|---|---|
| **Lomba & Prestasi** | Lomba yang diikuti (pendaftaran, peserta, materi latihan), prestasi yang diraih (otomatis memberi poin), **aturan poin** untuk setiap sumber (hadir kegiatan, panitia, pemateri, pengurus, prestasi), poin manual, dan **leaderboard**. |
| **Sertifikat** | Menerbitkan sertifikat peserta / panitia / pemateri / pengurus / prestasi dengan nomor dan kode verifikasi, mencetaknya (QR verifikasi), dan mencabut / memulihkan. |

### Formulir

| Menu | Untuk apa |
|---|---|
| **Formulir** | Membuat formulir dan survei (evaluasi kegiatan, pendaftaran rekrutmen, survei umum) dengan berbagai jenis pertanyaan, membuka / menutup, dan melihat ringkasan jawaban (jumlah per pilihan, rata-rata skala). |
| **Aspirasi** | Menampung aspirasi / saran anggota (boleh anonim) dan menanggapinya (Baru → Diproses → Selesai / Ditolak). |

### E-Voting

Pemilihan ketua umum atau lainnya secara rahasia: kandidat (pendaftaran dan verifikasi), daftar pemilih, membuka pemungutan, menutup, lalu **finalisasi** hasil (suara dienkripsi dan baru dibuka saat finalisasi). Pemilih mendapat tanda terima untuk mengecek bahwa suaranya tercatat. Semua tindakan tercatat di log audit.

### Informasi

| Menu | Untuk apa |
|---|---|
| **Berita** | Berita dan pengumuman (publik atau khusus anggota), sampul, Draft → Published → Archived. Berita baru mengirim notifikasi ke anggota. |
| **Banner** | Gambar slide di hero landing page, dengan tombol, tautan, urutan, dan jadwal tayang. |

### Area Anggota (`/AreaAnggota/...`, untuk semua anggota yang login)

| Menu | Untuk apa |
|---|---|
| **Profil Saya** | Mengubah kontak dan foto, melihat riwayat section, jabatan dan divisi, lama bergabung, mengganti password. **Rapor** per periode (lihat / unduh PDF). **Lihat KTA / Unduh PDF**: Kartu Tanda Anggota digital (depan-belakang) dengan QR; siapa pun yang memindai QR melihat halaman verifikasi `/kta/...` berisi status keanggotaan terkini. |
| **Presensi** | Absensi gaya aplikasi lapangan: kartu sesi yang sedang berlangsung / berikutnya, tab **Hari Ini / Mendatang / Riwayat**, dan tombol **Check-In / Check-Out**. Check-in membuka kamera langsung untuk selfie (bisa ganti kamera / foto ulang, atau pilih dari galeri bila kamera tidak tersedia), mengambil lokasi otomatis beserta jarak ke titik, lalu simpan. Sesi mode QR meminta scan QR dulu. Check-in bisa sejak 30 menit sebelum mulai. Berhalangan? Tekan **Ajukan izin** di kartu sesi (hari ini atau mendatang), isi alasan, lalu tunggu persetujuan pengurus di Daftar Kegiatan; status izin (menunggu / disetujui / ditolak) tampil di kartu dan pengajuan yang masih menunggu bisa ditarik. Di samping ada grafik kehadiran 6 bulan terakhir beserta peringatan bila di bawah 75%. |
| **Kegiatan Saya** | Kegiatan yang otomatis diikuti (mis. latihan rutin) dan **ajukan tidak ikut** satu pertemuan atau seluruh kegiatan; daftar / batal kegiatan dengan pendaftaran; melamar tim pelaksana dan melihat tugas panitia. |
| **Iuran Saya** | Melihat tagihan iuran sendiri dan statusnya. |
| **Peminjaman Alat** | Mengajukan dan membatalkan peminjaman alat sebagai anggota. |
| **Toko Merchandise** | Katalog, memesan sebagai anggota, dan membatalkan pesanan sendiri. |
| **Formulir & Aspirasi** | Mengisi formulir yang sedang dibuka dan mengirim aspirasi. |
| **E-Voting** | Memilih, menerima tanda terima satu kali, mengecek tanda terima, dan melihat hasil. |
| **Prestasi & Poin** | Prestasi, poin dan peringkat, lomba yang diikuti, serta mengunduh sertifikat sendiri. |
| **Disposisi** | Disposisi surat yang ditujukan ke saya dan menandainya selesai. |
| **Direktori Alumni** | Daftar alumni (cari nama, kota, bidang, perusahaan; saring angkatan). WhatsApp, email, dan LinkedIn hanya tampil untuk alumni yang bersedia dihubungi. Alumni yang login bisa mengisi **Profil alumni saya** (tahun lulus, pekerjaan, perusahaan, kota, LinkedIn, bersedia dihubungi). |
| **Notifikasi** | Semua notifikasi (persetujuan, tagihan, pengingat presensi, ...); lonceng di header menampilkan jumlah yang belum dibaca. |

User yang belum terhubung ke data anggota melihat pesan "bukan anggota" di halaman ini.

### Layanan lintas modul

- **Notifikasi in-app** otomatis untuk: pengajuan dan keputusan persetujuan (surat, pengajuan dana, pengadaan), disposisi, tagihan iuran, pemilihan dibuka, hasil lamaran panitia, pesanan siap diambil, keputusan dan keterlambatan peminjaman, sertifikat terbit, tanggapan aspirasi, penugasan pengurus, berita baru, keputusan izin kegiatan, serta **pengingat presensi** (check-in dibuka dan jelang check-out).
- **Log aktivitas**: setiap simpan, ubah, dan hapus yang berhasil tercatat (modul, aksi, data, IP, user agent). Lihat di Pengaturan Sistem.
- **Penomoran dokumen** otomatis sesuai format di Pengaturan Sistem, dengan placeholder `{seq} {type} {org} {month_roman} {month} {year} {yy}`.

## Laporan (DevExpress Reports)

Laporan cetak memakai DevExpress XtraReport + Web Document Viewer, bukan cetak dari browser. Viewer terbuka di modal layar penuh; dari viewer bisa cetak, atau export ke PDF, Word (DOCX), dan Excel. Untuk PDF langsung tanpa viewer, tambahkan `pdf=1` pada URL cetakan.

### LPJ (menu LPJ)

1. Klik **Buat LPJ**, lalu pilih **LPJ Kegiatan** (kegiatan induk; sub-kegiatan ikut dilaporkan) atau **LPJ Kepengurusan** (periode).
2. Isi bagian naratif. Bagian yang dikosongkan diisi awal otomatis dari profil organisasi dan data kegiatan. Baris yang diawali `- ` dicetak sebagai poin.
3. **Pratinjau** membuat dokumen dari data terkini.
4. **Finalisasi** menyimpan PDF dan mengunci LPJ. Untuk LPJ kepengurusan, PDF juga dipasang ke serah terima dari periode itu bila serah terimanya masih Draft / Ditolak.
5. **Unduh PDF final** mengambil PDF yang tersimpan. **Buka lagi** mengembalikan LPJ ke Draft.

Isi otomatis:

| LPJ Kegiatan | LPJ Kepengurusan |
|---|---|
| Sampul (judul, logo, institusi, tahun) | Sampul |
| Pendahuluan, latar belakang, macam, maksud & tujuan, bentuk kegiatan (narasi) | Pendahuluan, visi & misi, tujuan (narasi) |
| Waktu & tempat dari kegiatan, sub-kegiatan, dan jadwal rutin | Evaluasi program kerja per **bidang** dan **jangka** (Terealisasi / Tidak / Berjalan) |
| Hasil kegiatan dari prestasi lomba terkait | Daftar kegiatan periode (Terlaksana / Tidak) |
| Tanda tangan Ketua Pelaksana, Bendahara, mengetahui Ketua Umum | Tanda tangan Ketua Umum, Sekretaris, mengetahui Pembina |
| Lampiran dokumentasi (galeri kegiatan, maks. 12 foto) | Lampiran daftar inventaris per kategori dengan jumlah per kondisi |
| Lampiran realisasi anggaran: pemasukan; pengeluaran per kategori dengan **rincian RAB (qty × harga), anggaran, realisasi, selisih**; rekap sisa / kekurangan | Lampiran susunan pengurus (foto, jabatan, nama, NIA, fakultas/jurusan; tanpa alamat dan telepon) dan dokumentasi (maks. 24 foto) |

Supaya angka LPJ kegiatan lengkap:

- Catat transaksi dengan **kegiatan** terisi, atau lewat sponsorship / pengajuan dana yang terkait kegiatan itu. Hanya transaksi berstatus Posted yang dihitung.
- Isi **mata anggaran (item RAB)** pada transaksi atau pengajuan dana. Rincian RAB hanya tampil untuk item yang dipakai kegiatan itu.
- Tetapkan Ketua Pelaksana dan Bendahara di Tim Pelaksana. Unggah foto ke Galeri dengan kegiatan terisi.

### Persuratan

Semua cetakan Persuratan memakai kop surat: logo, nama organisasi, universitas, alamat (`alamat`), dan kontak dari profil organisasi.

| Cetakan | Tombol | Isi |
|---|---|---|
| Surat keluar | Baris surat keluar › **Cetak surat** | Tempat dan tanggal, Nomor / Lampiran / Perihal, tujuan, isi surat (HTML dari template), dan tanda tangan pengesah sesuai urutan persetujuan (gambar tanda tangan bila diunggah). Surat yang belum disetujui diberi tanda air **DRAFT**, dan yang ditolak **DITOLAK**. |
| Lembar disposisi | Baris surat masuk (atau surat keluar yang sudah disetujui) › **Lembar disposisi** | Data surat, tabel disposisi (dari, kepada, instruksi, batas waktu, status, paraf) ditambah baris kosong untuk ditulis tangan, serta kotak catatan |
| Agenda surat masuk / keluar | Toolbar tab Surat Masuk / Surat Keluar › **Cetak Agenda** (pilih rentang tanggal) | Buku agenda landscape. Surat masuk diurutkan menurut tanggal diterima dan memuat tujuan disposisinya. |
| Pratinjau template | Tab Template › **Pratinjau di kop surat** | Isi template di atas kop, dengan tanda air **CONTOH** |
| Daftar arsip dokumen | Arsip Dokumen › **Cetak Daftar Arsip** | Semua dokumen per kategori: judul, keterangan, berkas, ukuran, akses, tanggal unggah |

### Sertifikat

A4 landscape dengan bingkai emas, logo, nomor, nama penerima dan NIA, kalimat peran sesuai jenis sertifikat, tempat dan tanggal, tanda tangan (Ketua Pelaksana + Ketua Umum untuk kegiatan; Ketua Umum + Pembina untuk lainnya), dan QR verifikasi. Sertifikat yang dicabut diberi tanda air **DICABUT**. Admin mencetak dari **Sertifikat › Cetak**, anggota mengunduh sertifikatnya sendiri dari Area Anggota › Prestasi & Poin.

### Peminjaman alat (invoice dan kuitansi)

| Cetakan | Tombol | Isi |
|---|---|---|
| Invoice | Baris peminjaman › **Cetak invoice** (kecuali Ditolak / Dibatalkan) | Data peminjam, waktu pinjam dan kembali, skema biaya, jaminan; tabel sewa per barang (jumlah × hari × tarif, kode unit) dan denda; ringkasan total, pembebasan denda, setiap pembayaran, dan sisa tagihan; rekening transfer (akun Bank / E-Wallet aktif) selama masih ada sisa; tanda tangan Bendahara; tanda air **LUNAS / DITOLAK / DIBATALKAN**. |
| Kuitansi | Baris peminjaman › **Cetak kuitansi** (gabungan semua pembayaran) atau tombol kuitansi per pembayaran di modal Detail | Nomor transaksi, "telah terima dari", jumlah uang dan **terbilang**, keperluan, kotak nominal, sisa tagihan, tanda tangan penyetor dan Bendahara. |

### Rekap kehadiran

**Kegiatan › Presensi › Cetak Rekap Kehadiran**: pilih rentang tanggal, kegiatan, dan section. Isinya ringkasan (jumlah sesi ditutup, jumlah anggota, rata-rata hadir) dan tabel per anggota: jumlah sesi, hadir (berapa yang terlambat), izin / sakit, alpa, dan persentase hadir. Baris dengan kehadiran di bawah 75% diberi latar. Ditandatangani Sekretaris yang sedang menjabat.

### Laporan keuangan

**Keuangan › Transaksi › Cetak Laporan Keuangan** membuka pilihan jenis laporan. Semua laporan hanya menghitung transaksi berstatus Diposting dan ditandatangani Bendahara serta Ketua Umum periode aktif.

| Laporan | Pilihan | Isi |
|---|---|---|
| Buku kas (landscape) | Rentang tanggal, akun (atau semua) | Per akun: saldo awal (saldo awal akun + transaksi sebelum tanggal mulai), setiap transaksi dengan saldo berjalan, jumlah dan saldo akhir; ringkasan per akun bila lebih dari satu |
| Arus kas | Rentang tanggal, akun (atau semua) | Pemasukan dan pengeluaran per kategori, ringkasan saldo awal / surplus-defisit / saldo akhir, dan tabel per bulan (bulan defisit diberi latar) |
| Realisasi anggaran (landscape) | Anggaran | Per item RAB: kategori, uraian + program, volume × harga, RAB, realisasi, sisa, persentase (melebihi RAB diberi latar); lalu pengeluaran periode itu yang tidak memakai item anggaran |
| Rekap iuran (landscape) | Pengaturan iuran, section | Per periode tagihan (tagihan, lunas, dibebaskan, belum, terkumpul, tunggakan) dan per anggota (tunggakan beserta periode yang belum dibayar, urut tunggakan terbesar) |

Tombol **Unduh Excel** di modal yang sama mengunduh laporan terpilih sebagai `.xlsx` (`CetakLaporan?...&xlsx=1`). Semua laporan DevExpress menerima `?xlsx=1` (dan `?pdf=1`) untuk unduh langsung.

### Rapor anggota

Rapor per periode kepengurusan: identitas dan foto, kehadiran (ringkasan + per bulan dari sesi yang sudah ditutup), poin per sumber dan peringkat, jabatan dan divisi, kepanitiaan, prestasi, dan sertifikat pada periode itu. Ditandatangani Sekretaris dan Ketua Umum periode rapor.

- Pengurus: **Data Anggota** › baris anggota › **Rapor anggota**, pilih periode.
- Anggota: **Area Anggota › Profil Saya** › pilih periode › **Lihat Rapor** / **Unduh PDF**.

### Laporan inventaris

**Inventaris › Aset › Cetak Laporan Inventaris** (landscape): pilih kategori (atau semua) dan rentang tanggal riwayat perawatan (bawaan awal periode aktif s.d. hari ini). Isinya ringkasan (jenis aset, jumlah barang / unit, kondisi, nilai perolehan, perawatan), daftar aset yang belum dihapuskan per kategori (kode, nama, jumlah, baik, rusak ringan / berat, hilang, lalu keterangan: dipinjam, perawatan, lokasi, perolehan), dan riwayat perawatan (tanggal, aset / unit, pekerjaan, vendor, biaya, status). Aset per unit dihitung dari unitnya; aset cukup jumlah memakai kondisi asetnya. Ditandatangani Sekretaris dan Ketua Umum periode aktif.

### Rekap poin keaktifan

**Prestasi › Lomba & Prestasi › tab Poin › Cetak Rekap Poin** (landscape): pilih periode (bawaan aktif) dan section. Tabel per anggota: peringkat (poin sama = peringkat sama, dihitung di antara semua anggota periode itu), nama + NIA + section saat ini, poin hadir, panitia, pengurus, prestasi, lainnya (termasuk pemateri), dan total; tiga besar diberi latar. Ditandatangani Sekretaris dan Ketua Umum periode itu.

### Label QR inventaris (Aset › Unit & label QR)

- Label A4, 4 kolom, ukuran 46 × 53 mm. Isinya logo, nama organisasi, QR, kode unit, nama aset, dan nomor seri.
- Isi QR adalah URL publik `{App:PublicUrl}/aset/{token}` dengan token acak 32 karakter per unit (`m_asset_unit.qr_token`), jadi URL tidak bisa ditebak dari kode inventaris. Scan dengan kamera HP membuka detail alat tanpa login; kode unit biasa di URL ditolak (404). Scanner internal (checkout, pengembalian, inspeksi, perawatan) menerima URL itu, tokennya, atau kode unit yang diketik. Label yang dicetak sebelum 2026-11-18 harus dicetak ulang.
- Isi `App:PublicUrl` dengan domain produksi **sebelum** mencetak label sungguhan.

### Tanda tangan elektronik (QR)

Di laporan yang memakai QR, tempat tanda tangan basah setiap penanda tangan yang namanya terisi diganti QR dengan tulisan "Ditandatangani secara elektronik". QR berisi `{App:PublicUrl}/ttd/{token}`. Saat dipindai, halaman publik (tanpa login) menampilkan status sah, nama, jabatan, NIA, tanggal tanda tangan, jenis dan judul dokumen, keterangan (nomor surat / filter), serta kapan dan oleh siapa dokumen pertama kali diterbitkan. Token yang tidak dikenal menampilkan peringatan (404).

| Pakai QR | Tetap tanda tangan basah |
|---|---|
| Laporan keuangan, rapor anggota, rekap poin, rekap kehadiran, laporan inventaris, invoice dan kuitansi peminjaman (Bendahara), surat keluar yang sudah disetujui | LPJ, surat yang masih Draft / Ditolak, penyetor di kuitansi (pihak luar), jabatan yang belum ada pemegangnya |

Token disimpan di `t_document_signature` (migrasi `2026-11-20_document_signature.sql`). Mencetak ulang dokumen yang sama (judul, keterangan, penanda tangan, tanggal) memakai token yang sama dan hanya menambah jumlah cetak. Isi `App:PublicUrl` dengan domain produksi sebelum mencetak dokumen resmi.

## Job terjadwal dan pengiriman pesan

Web menjalankan job lewat `ScheduledJobService` (hosted service, dicek tiap menit). Setiap run dicatat di `t_scheduled_job_log` dan bisa dilihat di Pengaturan Sistem. Jadwal berikutnya dihitung dari run terakhir yang tercatat, jadi restart tidak mengulang job harian.

| Job | Interval | Isi |
|---|---|---|
| `SEND_OUTBOX` | 1 menit | Kirim `t_message_outbox` yang jatuh tempo (email SMTP, WhatsApp via gateway). Gagal 5 kali = Gagal, bisa diulang dari Pengaturan. |
| `SYNC_ATTENDANCE_SESSIONS` | 1 menit | Presensi: kirim pengingat (check-in dibuka / jelang check-out), buka sesi 30 menit sebelum mulai, tutup sesi yang sudah selesai (Alpa + poin hadir). Dicatat di log hanya bila ada perubahan. |
| `BIRTHDAY_GREETINGS` | 1 jam (mulai 07.00) | Ucapan ulang tahun in-app + WhatsApp ke anggota aktif dan alumni yang berulang tahun hari ini; yang sudah diberi ucapan hari ini dilewati |
| `CLOSE_EXPIRED_ELECTION` | 5 menit | `Election.CloseExpired()` |
| `MARK_BORROW_OVERDUE` | 30 menit | `BorrowRequest.MarkOverdue` (anggota peminjam mendapat notifikasi) |
| `GENERATE_ATTENDANCE_SESSIONS` | 6 jam | Sesi presensi dari jadwal rutin, hari ini s.d. `Jobs:SessionDaysAhead` hari ke depan (tanggal yang jamnya sudah lewat dilewati) |
| `PURGE_OLD_DATA` | harian | Notifikasi dibaca > 90 hari, log aktivitas > 365 hari, token > 30 hari, log job > 90 hari |

Konfigurasi ada di [Konfigurasi](#konfigurasi). Method library yang dipanggil job juga punya versi `...Async` bila ingin dijalankan dari scheduler lain.

> **Perhatian:** saat job presensi pertama kali jalan, semua sesi lama yang masih Terjadwal tetapi jamnya sudah lewat ikut ditutup dan anggotanya dicatat Alpa. Batalkan dulu sesi-sesi itu bila tidak diinginkan, atau jalankan web dengan `Jobs:Enabled = false` saat menguji.

## Autentikasi dan hak akses

- **Login:** cookie authentication (`gwa.auth`, berlaku 8 jam dan diperpanjang selama aktif). Claim `NameIdentifier` berisi `m_user.id`, dan nilai inilah yang diteruskan ke parameter `user` di method GWALib. Login, gagal login, dan logout dicatat di log aktivitas.
- **Filter global**, berurutan: wajib login → `AccessFilter` → validasi token antiforgery.
- **Aturan `AccessFilter`:**
  - `[AllowAnonymous]`: terbuka untuk siapa saja.
  - `[LoginOnly]`: cukup login.
  - Aksi yang terdaftar di `m_controller_action` dengan `need_auth = 1`: harus ada di `m_role_action` milik salah satu role user.
  - **Aksi yang belum terdaftar: hanya SUPERADMIN.** Setiap halaman baru wajib didaftarkan di katalog `GWASeeder/Seed.Akses.cs`, lalu jalankan `--sync-akses`.
  - SUPERADMIN melewati semua pengecekan.
- **Role berlaku** kalau role aktif dan `m_user_role`-nya tidak terikat periode, atau terikat periode yang sedang Active.
- **Sidebar** dibangun dari `m_header_menu`, `m_sub_menu`, dan `m_role_menu` sesuai role user. Cache menu dan akses 5 menit; perubahan role, menu, atau user langsung membersihkan cache.

Kosakata aksi per controller:

| Aksi | Untuk |
|---|---|
| `Index` | Membuka halaman dan semua data baca (daftar, detail, unduh file, pratinjau laporan) |
| `Create` / `Edit` / `Delete` | Data utama maupun data anak di halaman itu |
| `Submit` | Mengajukan dokumen ke alur persetujuan |
| `Approve` | Menyetujui, menolak, atau meminta revisi |
| `Process` | Perubahan status operasional (buka/tutup, publikasi, checkout, pengembalian, finalisasi LPJ, dan sejenisnya) |
| `Payment` | Menerima, mencatat, atau membatalkan pembayaran yang membuat transaksi keuangan |

## Tampilan

- **Situs publik dan login:** tema terang yang hangat, senada dengan warna logo: gading, krem, emas `#e8b52a`, merah api `#c8202a`, hijau pita. Tidak ada section gelap dan tidak ada mode gelap. Font Playfair Display + Inter. Stylesheet di `wwwroot/css/landing.css` dan `login.css`.
- **Dashboard:** Velzon dengan palet GWA lewat salinan `assets/css/*.gwa.min.css` (primary bronze `#9a6b00`, secondary merah api, success hijau pita, warning emas, latar gading). File asli Velzon tidak diubah; kalau Velzon diperbarui, buat ulang salinannya dengan pemetaan warna yang sama.
- **Sidebar dan header** mengikuti model SurabayaTaxWeb (`wwwroot/css/gwa-sidebar.css`): sidebar gradasi merah api ke oranye, menu aktif berupa pill dengan garis emas, panel melayang saat sidebar diciutkan, dan dropdown user di header.
- **Widget DevExtreme** memakai tema `dx.material.blue.light.compact` + `css/gwa-devextreme.css` (header grid bronze, baris zebra gading).
- **Presensi anggota** memakai `wwwroot/css/presensi-saya.css` (pola absensi aplikasi lapangan). Peta pemilih lokasi memakai Leaflet (`assets/libs/leaflet`) dengan tile OpenStreetMap.
- **Logo** diambil dari profil organisasi lewat `/Brand/Logo`; kalau belum ada, dipakai `assets/images/gwa-logo.png`.

## Panduan pengembangan

### Build

- **Visual Studio:** **Build › Build Solution** (Ctrl+Shift+B).
- **Terminal:**
  ```
  dotnet build GitaWidyaAgni.sln
  ```

Belum ada project test. Sekitar 18 warning build berasal dari helper yang disalin dari ClubShared.

### Menguji dengan beberapa akun

Login dengan akun berbeda di jendela browser yang berbeda (mis. jendela biasa untuk `admin`, jendela InPrivate / Incognito untuk `anggota01`). Kamera dan GPS presensi hanya jalan lewat `https://`; di HP dalam jaringan yang sama, pakai alamat HTTPS yang bisa diakses HP atau server uji.

### Konfigurasi library saat startup

Aplikasi yang memakai library wajib memanggil dua method berikut sekali saat startup. `GitaWidyaAgniWeb/Program.cs` sudah melakukannya.

```csharp
DBClass.Configure(connectionString);   // connection string MySQL
ElectionVote.ConfigureKey(base64Key);  // kunci e-voting dari secret, bukan dari database
```

### Menambah halaman pengelolaan

Ikuti pola yang sudah ada (contoh paling sederhana: `Informasi/Banner`; detail lengkap di `CLAUDE.md`, bagian "Management page pattern"):

1. `Controllers/<Area>/<Nama>Controller.cs`: `[Area("<Area>")]`, mewarisi `BaseController`. Aksi baca diberi `[AccessAs(Acc.Index)]`, aksi tulis `[AccessAs(Acc.Create/Edit/...)]`.
2. `Models/<Area>/<Nama>VM.cs` dengan kelas `Index`, `List`, `Row`, `Add`, `Edit`, `NewRow` (DataAnnotations), dan `Method` (semua panggilan GWALib).
3. `Views/<Area>/<Nama>/`: `Index.cshtml`, `_List.cshtml` (grid `ListGrid`), `_Add`, `_Edit`, `_Form` (dxForm dalam modal).
4. `wwwroot/Scripts/<Area>/<Nama>.js`: modul `window.<Nama> = (function () { ... })();` yang memakai helper `AppUI` di `wwwroot/js/site.js`.
5. Daftarkan controller dan menu di `GWASeeder/Seed.Akses.cs`, lalu jalankan `dotnet run --project GWASeeder -- --sync-akses`.

### Menambah laporan

1. Buat kelas laporan lewat kode di `GitaWidyaAgniWeb/Reports/` (tanpa `.repx`). Untuk dokumen, turunkan dari `DocumentReport`, yang sudah menyediakan blok kop surat, judul, paragraf, tabel, isi HTML, tanda tangan, tanda air, dan `Terbilang`. Identitas organisasi diambil dari `ReportBrand.Load(webRoot)`.
2. Susun datanya di `Models/<Area>/<Nama>Cetak.cs`.
3. Aksi controller cukup memanggil `return Laporan(() => report);`. Hasilnya halaman viewer bersama; dengan `?pdf=1`, hasilnya PDF langsung.
4. Di JS, buka viewernya dengan `AppUI.openReport(judul, url)`. Tombol tambahan di toolbar bisa ditambahkan lewat `ToolbarModel.Extra`.

Satuan bawaan laporan adalah 1/100 inci (A4 = 827 × 1169).

### Konvensi kode

- Service GWALib memakai pola `<Nama>.cs` + `<Nama>.Services.cs`, `NewRow`, `GetDataList` / `Save` / `Update` / `Delete`, dibungkus `ServiceHandler.Handle`. `ArgumentException` adalah pesan untuk pengguna.
- Pesan untuk pengguna ditulis dalam bahasa Indonesia sebagai konstanta di `GWAShared.Results.ExceptionMessage`.
- Hasil service berupa `Response<T>` dari `ResponseFactory`. Di web, `.Ok()` mengambil data atau melempar pesan errornya.
- `<summary>` setiap enum mencantumkan semua `tabel.kolom` yang memakainya, dan setiap nilainya diberi `[Description]` berbahasa Indonesia.
- `EncryptString<T>()` / `DecryptString<T>()` memakai kunci yang bergantung pada nama kelas dan tanggal hari ini, jadi encKey hanya berlaku pada hari yang sama. Nama kelas maksimal 24 karakter. Kirim encKey sebagai query string (`?key=`), bukan segmen path, karena encKey bisa berisi `/`.
- Jangan menulis ulang file repo lewat PowerShell `Get-Content` / `Set-Content`, karena karakter UTF-8 bisa rusak. Pakai editor biasa.
- Commit hanya perubahan yang sudah dibuild. Jangan commit `appsettings.Development.json` atau secret.

## Deploy ke server

1. Buat database dari `database/gitawidyaagni_create.sql`. Untuk server yang sudah ada, jalankan migrasi yang belum diterapkan.
2. Publish: `dotnet publish GitaWidyaAgniWeb -c Release -o ./publish` (atau Visual Studio: klik kanan GitaWidyaAgniWeb › **Publish**). Build di mesin yang punya DevExpress 23.2 berlisensi.
3. Isi environment variable server: `ConnectionStrings__Default`, `EVoting__Key`, `App__PublicUrl`, `Smtp__*`, `WhatsApp__*`, dan `Jobs__Enabled` (`false` di instance tambahan).
4. Jalankan di belakang HTTPS. Cookie login `Secure`, serta kamera dan lokasi untuk presensi, hanya bekerja di HTTPS.
5. Isi menu dan hak akses dengan `dotnet run --project GWASeeder -- --sync-akses` (connection string dari env `GWA_DB`), lalu buat admin pertama lewat `Bootstrap:*`.
6. Isi template pesan WhatsApp (`ATTENDANCE_REMINDER`, `ATTENDANCE_CHECKOUT`, `BIRTHDAY`, `ALUMNI_INVITE`, dan lainnya) di Pengaturan Sistem bila ingin pengingat lewat WhatsApp.

## Troubleshooting

| Gejala | Penyebab / solusi |
|---|---|
| Restore gagal mencari `DevExtreme.AspNet.*` / `DevExpress.AspNetCore.Reporting` | DevExpress 23.2 belum terpasang, atau terpasang di folder lain. Sesuaikan path di `NuGet.config`. |
| Build gagal `MSB3027 ... locked by "GitaWidyaAgniWeb"` | Web masih berjalan. Hentikan prosesnya dulu (Stop di Visual Studio, atau tutup terminal `dotnet run`). |
| Browser menolak `https://localhost` | Jalankan `dotnet dev-certs https --trust` lalu buka ulang browser. |
| Web gagal start: connection string kosong | `appsettings.Development.json` belum dibuat, atau env `ConnectionStrings__Default` belum diisi. |
| `Unknown column ... in 'field list'` | Database belum menjalankan migrasi terbaru. Jalankan berkas di `database/migrations/` yang belum diterapkan. |
| `mysql ... < file.sql` gagal di PowerShell | PowerShell tidak mendukung `<`. Pakai Command Prompt / Git Bash, atau `source file.sql;` di dalam `mysql`. |
| Menu baru tidak muncul / halaman ditolak | Controller belum ada di katalog, atau `--sync-akses` belum dijalankan. Login ulang, atau tunggu 5 menit sampai cache menu habis. |
| Check-in ditolak "Lokasi Anda ... m dari titik presensi" | Anggota di luar radius sesi. Mendekat ke lokasi, atau perbesar radius di sesi / jadwal. |
| Check-in ditolak "Sesi ini wajib scan QR" | Sesi bermode Wajib Scan QR. Scan QR dari pengurus, atau ubah mode sesi menjadi Tanpa QR. |
| Kamera / lokasi tidak muncul di presensi | Halaman dibuka lewat `http://`, atau izin kamera / lokasi browser ditolak. Buka lewat HTTPS dan izinkan. |
| E-voting ditolak "Kunci enkripsi e-voting belum dikonfigurasi" | `EVoting:Key` kosong. Pakai kunci yang sama dengan saat pemilihan dibuka. |
| Label QR membuka `localhost` di HP | `App:PublicUrl` belum diisi saat label dicetak. Isi lalu cetak ulang label. |
| PDF laporan bertuliskan "trial version of DevExpress" | Build memakai DevExpress trial. Build ulang dengan lisensi. |
| Pesan email / WhatsApp tetap Menunggu | `Smtp:*` atau `WhatsApp:*` belum diisi, atau `Jobs:Enabled` = false. |
| "Form edit sudah kedaluwarsa" | Kunci edit tersimpan sekali pakai, dan encKey berganti tiap hari. Buka ulang form edit. |
| Tes halaman login lewat curl gagal | Cookie `Secure` tidak dikirim lewat `http://`. Jalankan web di `https://localhost:<port>` dan pakai `curl -k` dengan cookie jar. |
| Login ditolak "Verifikasi gagal atau kedaluwarsa" | Captcha belum diselesaikan, atau tokennya sudah dipakai / lewat 10 menit. Seret ikon musik yang sama ke kotak lagi. Bila web dijalankan di beberapa server, aktifkan sticky session karena jawaban captcha disimpan di memori server. |

## Catatan keamanan

- Jangan commit password, token, atau kunci. `appsettings.Development.json` sudah di-ignore; untuk nilai lain pakai user-secrets atau environment variable.
- `GWALib/EF/ModelContext.cs` masih memuat baris `OnConfiguring` lama yang dikomentari, dengan password root database. Baris itu sudah ada di riwayat git. **Ganti password root MySQL tersebut** dan hapus barisnya.
- Akun dan password contoh dari GWASeeder hanya untuk pengembangan. Jangan jalankan seeder data contoh di database produksi.
- Kunci `EVoting:Key` membuka isi suara pemilihan yang belum difinalisasi. Simpan hanya di secret server.
- Foto selfie dan lokasi presensi adalah data pribadi anggota; hanya role yang punya akses menu Presensi yang bisa melihatnya.
