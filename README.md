# GitaWidyaAgni

Sistem manajemen **Marching Band Gita Widya Agni (MB GWA) UPN "Veteran" Jawa Timur**. Repo ini berisi:

- **GWALib / GWAShared**: library backend .NET 8 untuk satu organisasi (anggota dan kepengurusan, program dan kegiatan, presensi, keuangan, persuratan, e-voting, pengadaan, sponsorship, inventaris, merchandise, prestasi, formulir, informasi).
- **GitaWidyaAgniWeb**: aplikasi web ASP.NET Core 8 MVC. Berisi situs publik, dashboard pengurus untuk semua modul, Area Anggota, laporan DevExpress (LPJ dan label QR), serta job terjadwal.
- **GWASeeder**: console untuk mengisi database dengan data contoh dan katalog hak akses.
- **database/**: skrip SQL pembuatan database dan migrasi skema.

Organisasi adalah level tertinggi. Tidak ada tenant, kampus, kategori UKM, atau pegawai.

> **Status (17 November 2026)**
> - Library lengkap: setiap tabel master dan transaksi punya service class.
> - Web lengkap: landing page, login, 37 halaman pengelolaan, menu LPJ, 11 halaman Area Anggota, dan halaman publik (rekrutmen, verifikasi sertifikat, toko, detail aset dari QR).
> - Semua halaman admin sudah dites sebagai admin. Halaman Area Anggota baru dites dengan akun admin, yang tidak terhubung ke data anggota; tampilan untuk anggota asli masih perlu dicoba dengan akun anggota.

## Daftar isi

1. [Mulai cepat (setup untuk tim)](#mulai-cepat-setup-untuk-tim)
2. [Prasyarat](#prasyarat)
3. [Struktur solution](#struktur-solution)
4. [Database](#database)
5. [Konfigurasi](#konfigurasi)
6. [Data contoh (GWASeeder)](#data-contoh-gwaseeder)
7. [Fitur dan halaman](#fitur-dan-halaman)
8. [Laporan (DevExpress Reports)](#laporan-devexpress-reports)
9. [Job terjadwal dan pengiriman pesan](#job-terjadwal-dan-pengiriman-pesan)
10. [Autentikasi dan hak akses](#autentikasi-dan-hak-akses)
11. [Tampilan](#tampilan)
12. [Panduan pengembangan](#panduan-pengembangan)
13. [Deploy ke server](#deploy-ke-server)
14. [Troubleshooting](#troubleshooting)
15. [Catatan keamanan](#catatan-keamanan)

## Mulai cepat (setup untuk tim)

Langkah dari nol sampai bisa login. Perintah ditulis untuk PowerShell atau Git Bash di Windows.

1. **Pasang prasyarat**: .NET 8 SDK, MySQL 8.0, dan DevExpress 23.2 (lihat [Prasyarat](#prasyarat)).
2. **Clone repo**:
   ```
   git clone https://github.com/RaflyRoyan/GitaWidyaAgni.git
   cd GitaWidyaAgni
   ```
3. **Buat database** dari skrip di repo. Perintah ini membuat database `gitawidyaagni` beserta 135 tabelnya:
   ```
   mysql -u root -p < database/gitawidyaagni_create.sql
   ```
   Kalau `mysql` belum ada di PATH, pakai path lengkapnya, misalnya `"C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe"`.
4. **Isi connection string** di file baru `GitaWidyaAgniWeb/appsettings.Development.json`. File ini di-ignore git, jadi setiap orang membuatnya sendiri:
   ```json
   {
     "ConnectionStrings": {
       "Default": "server=127.0.0.1;port=3306;database=gitawidyaagni;user=root;password=PASSWORD_MYSQL_KAMU"
     }
   }
   ```
5. **Buat kunci e-voting**, lalu simpan sebagai user-secret (sekali per mesin):
   ```powershell
   $key = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
   dotnet user-secrets set "EVoting:Key" $key --project GitaWidyaAgniWeb
   ```
6. **Isi data**. Pilih salah satu:
   - **Pengembangan, dengan data contoh**:
     ```
     dotnet run --project GWASeeder -- --evoting-key <kunci dari langkah 5> --admin-password <password admin>
     ```
     Data yang diisi: profil MB GWA, menu, role, 24 akun contoh, dan data semua modul. Daftar akun ada di [Akun contoh](#akun-contoh).
   - **Produksi, tanpa data contoh**:
     1. Isi admin awal:
        ```
        dotnet user-secrets set "Bootstrap:AdminUsername" "admin" --project GitaWidyaAgniWeb
        dotnet user-secrets set "Bootstrap:AdminPassword" "<password kuat>" --project GitaWidyaAgniWeb
        ```
     2. Jalankan web sekali; admin hanya dibuat selama tabel `m_user` masih kosong.
     3. Isi menu, controller, dan role: `dotnet run --project GWASeeder -- --sync-akses`.
     4. Setelah login pertama, hapus kedua secret `Bootstrap:*`.
7. **Jalankan web**:
   ```
   dotnet run --project GitaWidyaAgniWeb
   ```
   Buka URL yang tercetak di konsol (profil `https`). Halaman pertama adalah landing page; tombol **Login** ada di navbar.
8. **Isi profil organisasi** lewat menu **Organisasi › Profil Organisasi**: nama, logo, visi, misi, dan pembina. Lalu isi **Pengaturan Sistem › Site Setting**: `whatsapp`, `alamat`, `instagram`, `kota`, dan seterusnya (lihat [Konfigurasi](#konfigurasi)).

## Prasyarat

| Komponen | Versi | Catatan |
|---|---|---|
| .NET SDK | 8.0+ | |
| MySQL | 8.0 | Collation `utf8mb4_0900_ai_ci` |
| DevExpress | 23.2.5 | `NuGet.config` mengambil paket DevExtreme dan DevExpress Reporting dari folder lokal hasil instalasi DevExpress 23.2. Tanpa instalasi itu, restore gagal. |
| Browser | Chrome / Edge terbaru | Scan QR presensi butuh kamera dan izin lokasi (HTTPS) |

Sumber paket di `NuGet.config`:

- `C:\Program Files\DevExpress 23.2\DevExtreme\System\DevExtreme\Bin\AspNetCore` (DevExtreme.AspNet.*)
- `C:\Program Files\DevExpress 23.2\Components\System\Components\Packages` (DevExpress.AspNetCore.Reporting)

**Lisensi DevExpress:** instalasi trial membuat PDF hasil laporan berisi tulisan *"This document was created using the trial version of DevExpress components"*. Build dengan instalasi berlisensi supaya tulisan itu hilang.

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
│   ├── Models/<Area>/         View model <Nama>VM (Index, List, Row, Add, Edit, NewRow, Method)
│   ├── Views/<Area>/<Ctrl>/   Index, _List, _Add, _Edit, _Form
│   ├── Reports/               Laporan DevExpress lewat kode: LpjReport, LabelQrUnitReport
│   ├── Infrastructure/        BaseController, AccessFilter, AccessService, ActivityLogFilter, AsetLabel, Jobs/
│   └── wwwroot/               assets/ (Velzon), css/, js/ (AppUI site.js, grid-export.js), Scripts/<Area>/, lib/dxreporting-23.2.5/
├── GWASeeder/                 Console data contoh + katalog hak akses (Seed.*.cs)
└── database/
    ├── gitawidyaagni_create.sql   Skema lengkap (135 tabel)
    ├── migrations/                Perubahan skema per tanggal
    └── README.md                  Cara pakai skrip database
```

Struktur dan konvensinya mengikuti project ClubSphere (ClubLib / ClubShared). Panduan teknis yang lebih rinci untuk pengembang ada di `CLAUDE.md`.

## Database

### Skrip

| Berkas | Untuk |
|---|---|
| [`database/gitawidyaagni_create.sql`](database/gitawidyaagni_create.sql) | Membuat database baru lengkap: 135 tabel, semua relasi dan `COMMENT` kolom. Isinya hanya struktur, tanpa data. Sudah memuat semua migrasi sampai tanggal di kepala berkas. |
| [`database/migrations/`](database/migrations/) | Perubahan skema setelah create script dibuat, satu berkas per perubahan (`YYYY-MM-DD_<nama>.sql`). Hanya untuk database lama. |
| [`database/README.md`](database/README.md) | Cara pakai dan aturan menambah migrasi. |

- **Database baru:** jalankan create script saja.
- **Database lama:** jalankan migrasi yang tanggalnya lebih baru dari skema database itu, urut tanggal:
  ```
  mysql -u root -p gitawidyaagni < database/migrations/2026-11-17_lpj.sql
  mysql -u root -p gitawidyaagni < database/migrations/2026-11-18_section.sql
  mysql -u root -p gitawidyaagni < database/migrations/2026-11-18_asset_qr_token.sql
  mysql -u root -p gitawidyaagni < database/migrations/2026-11-18_activity_absence.sql
  ```

Riwayat migrasi:

| Tanggal | Berkas | Isi |
|---|---|---|
| 2026-11-17 | `2026-11-17_lpj.sql` | Tabel `t_lpj` (LPJ kegiatan / kepengurusan); kolom `t_program.division_id` (bidang) dan `t_program.term` (jangka pendek / panjang) |
| 2026-11-18 | `2026-11-18_section.sql` | Tabel `m_section` (section musik) dan `t_section_member` (riwayat section anggota); kolom `t_activity.section_id` (audiens 6 = Section) dan `t_management_assignment.section_id` (Section Leader) |
| 2026-11-18 | `2026-11-18_asset_qr_token.sql` | Kolom `m_asset_unit.qr_token`: token acak isi QR label, menggantikan kode unit di URL `/aset/...`. Label lama harus dicetak ulang |
| 2026-11-18 | `2026-11-18_activity_absence.sql` | Tabel `t_activity_absence_request`: anggota otomatis ikut kegiatan audiensnya (mis. latihan rutin) dan mengajukan tidak ikut satu pertemuan atau seluruh kegiatan, disetujui pengurus |

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
| Program & kegiatan | `t_program`, `t_program_task`, `t_timeline_plan`, `t_activity`, `t_activity_participant`, `t_activity_report`, `t_gallery(_photo)`, `t_lpj` |
| Tim pelaksana & presensi | `t_committee_*`, `t_activity_rundown`, `t_activity_checklist`, `t_activity_schedule`, `t_attendance_session`, `t_attendance_record`, `t_meeting_minutes` |
| Keuangan | `m_financial_account`, `m_financial_category`, `t_financial_transaction`, `t_budget(_item)`, `t_funding_request`, `m_dues_setting`, `t_dues_bill`, `t_debt`, `t_period_balance` |
| Mitra, sponsorship, pengadaan | `m_external_party(_contact)`, `m_sponsorship_package`, `t_sponsorship`, `t_sponsorship_mou`, `t_procurement_*` |
| Merchandise & inventaris | `m_product(_variant)`, `t_product_stock_movement`, `t_sales_order(_item)`, `m_asset`, `m_asset_unit`, `t_borrow_request`, `t_borrow_item(_unit)`, `t_asset_inspection`, `t_maintenance_log` |
| Surat, prestasi, formulir, e-voting, informasi | `t_letter`, `t_document`, `t_competition`, `t_achievement`, `t_certificate`, `t_member_point`, `t_form*`, `t_aspiration`, `t_election*`, `t_announcement`, `m_banner` |

### Mengubah skema

1. Tulis berkas migrasi baru di `database/migrations/` lalu jalankan ke database pengembangan.
2. Perbarui entity EF di `GWALib/EF/`, lewat scaffold ulang Pomelo atau ditulis tangan. Scaffold menimpa file EF; biarkan `OnConfiguring` tetap dikomentari.
3. Tambah atau ubah enum di `GWAShared/Enums/` sesuai `COMMENT` kolom.
4. Buat ulang `database/gitawidyaagni_create.sql` (perintah `mysqldump` ada di `database/README.md`) dan tambahkan baris di tabel riwayat migrasi di atas.

## Konfigurasi

### appsettings / user-secrets / environment

Nilai rahasia (password, token, kunci) **tidak boleh** masuk `appsettings.json`. Simpan di `appsettings.Development.json` (di-ignore git), di user-secrets (`dotnet user-secrets set "<key>" "<nilai>" --project GitaWidyaAgniWeb`), atau di environment variable server (titik dua jadi `__`, misalnya `ConnectionStrings__Default`).

| Key | Wajib | Fungsi |
|---|---|---|
| `ConnectionStrings:Default` | Ya | Connection string MySQL. Web gagal start kalau kosong. |
| `EVoting:Key` | Untuk e-voting | Base64 dari 32 byte acak. Tanpa kunci ini, membuka pemungutan suara, memilih, dan finalisasi ditolak. **Jangan diganti** sebelum semua pemilihan yang memakainya difinalisasi. Seeder dan web harus memakai kunci yang sama. |
| `Bootstrap:AdminUsername` / `Bootstrap:AdminPassword` | Sekali | Membuat role SUPERADMIN + user admin selama `m_user` kosong. Hapus setelah login pertama. |
| `App:PublicUrl` | Produksi | URL publik aplikasi, misalnya `https://gwa.example.org`. Dipakai sebagai isi QR label aset. Kalau kosong, dipakai host request; label yang dicetak dari `localhost` tidak bisa dibuka dari HP. |
| `Jobs:Enabled` | - | Bawaan `true`. Pada server dengan beberapa instance, aktifkan di satu instance saja. |
| `Jobs:Username` | - | User yang menjalankan job. Kosong = user SUPERADMIN aktif pertama. |
| `Jobs:NotificationRetentionDays` / `ActivityLogRetentionDays` / `TokenRetentionDays` / `JobLogRetentionDays` / `SessionDaysAhead` | - | Bawaan 90 / 365 / 30 / 90 / 7. |
| `Smtp:Host`, `Port`, `EnableSsl`, `Username`, `Password`, `From`, `FromName` | Untuk email | Pengiriman email dari antrian `t_message_outbox`. |
| `WhatsApp:Url`, `WhatsApp:Token` | Untuk WhatsApp | Gateway HTTP gaya Fonnte (POST form `target` + `message`, token di header `Authorization`), misalnya `https://api.fonnte.com/send`. |

Saluran email atau WhatsApp yang belum dikonfigurasi tidak dikirim, dan pesannya tetap berstatus Menunggu sampai dikonfigurasi.

### Site setting (dari menu Pengaturan Sistem)

| Key `m_site_setting` | Dipakai di |
|---|---|
| `hero_title`, `hero_subtitle` | Hero landing page |
| `alamat`, `instagram`, `youtube`, `tiktok` | Kontak dan footer landing page |
| `whatsapp` | Tombol WhatsApp: landing, konfirmasi pesanan toko, halaman detail aset |
| `kota` | Kota pada tanda tangan LPJ (bawaan Surabaya) |

### Data master yang perlu diisi sebelum dipakai

- **Profil organisasi**: nama, kode (untuk `{org}` di nomor dokumen), logo, visi, misi, dan nama pembina (tanda tangan LPJ kepengurusan).
- **Periode kepengurusan** yang berstatus Active, beserta section, divisi dan jabatan. Setiap anggota wajib punya section (Brass, Battery, Front Ensemble, Colour Guard); divisi (Humas, Logistik, Danus, ...) hanya untuk anggota pilihan Ketua Umum. Penanda tangan LPJ dicari dari jabatan "Ketua Umum" dan "Sekretaris".
- **Format nomor dokumen** dan **alur persetujuan** untuk `FUNDING_REQUEST`, `PROCUREMENT`, dan `LETTER`. Tanpa alur aktif, pengajuan dana dan pengadaan tidak bisa diajukan.
- **Akun keuangan** dan **kategori**, termasuk kategori untuk iuran, sewa alat, denda, penjualan, dan sponsorship.

## Data contoh (GWASeeder)

```
dotnet run --project GWASeeder                    # database kosong: isi data contoh
dotnet run --project GWASeeder -- --reset         # KOSONGKAN semua tabel lalu isi ulang (ketik RESET; --yes melewati konfirmasi)
dotnet run --project GWASeeder -- --sync-akses    # hanya sinkron controller/aksi, menu, dan role di database yang sudah berisi
```

Opsi tambahan: `--evoting-key <base64>` dan `--admin-password <password>`.

- Mengisi 134 dari 135 tabel lewat service GWALib, jadi semua validasi ikut berjalan. Hanya `t_lpj` yang tidak diisi; LPJ dibuat dari web. Datanya meliputi:
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
| ANGGOTA | Hanya Area Anggota (semua role juga mendapat menu ini) |

Siapa yang boleh menyetujui dokumen tetap ditentukan alur persetujuan. Pada data contoh, pengajuan dana dan pengadaan disetujui Bendahara lalu Ketua Umum, dan surat keluar disetujui Sekretaris lalu Ketua Umum.

## Fitur dan halaman

### Halaman publik (tanpa login)

| URL | Isi |
|---|---|
| `/` | Landing page: hero dengan banner, tentang, seksi musik, agenda, prestasi, galeri, berita, rekrutmen yang sedang buka, kontak |
| `/Landing/Berita`, `/berita/{slug}` | Daftar dan detail berita publik |
| `/rekrutmen/{id?}` | Pendaftaran open recruitment, termasuk pertanyaan tambahan dari formulir |
| `/sertifikat/verifikasi?kode=` | Cek keaslian sertifikat dari kode verifikasi |
| `/toko` | Pesan merchandise tanpa login (nama + WhatsApp), konfirmasi lewat WhatsApp, dan lacak pesanan dengan nomor pesanan + nomor WhatsApp |
| `/aset/{token}` | Hasil scan label QR alat (token acak per unit, bukan kode unit): detail unit, kondisi, status, lokasi, dan foto. Harga, tarif, dan data peminjam tidak ditampilkan. |

### Dashboard pengurus

Menu sidebar dan area URL-nya (`/<Area>/<Controller>`):

| Menu | Controller | Isi |
|---|---|---|
| Akses › User / Role / Menu | `User`, `Role`, `Menu` | Akun, role per periode, hak aksi, menu sidebar, reset password |
| Pengaturan Sistem | `Pengaturan` | Site setting, format nomor, alur persetujuan, template pesan, sequence, log job, antrian pesan, log aktivitas |
| Organisasi › Profil / Periode / Section / Divisi / Jabatan / Kepengurusan | `Organisasi`, `Periode`, `Section`, `Divisi`, `Jabatan`, `Kepengurusan` | Profil + logo, periode dan serah terima (dengan berkas LPJ), section musik dan riwayat section anggota (pindah kapan saja), divisi dan anggota pilihannya, jabatan, penugasan pengurus (termasuk Section Leader) |
| Anggota › Data Anggota / Alumni / Rekrutmen | `Anggota`, `Alumni`, `Rekrutmen` | Data anggota dan foto, profil alumni, rekrutmen dan seleksi pelamar |
| Program Kerja | `ProgramKerja` | Program kerja (dengan **bidang** dan **jangka**), tugas, timeline |
| Kegiatan › Daftar / Tim Pelaksana / Presensi / Galeri | `Kegiatan`, `TimPelaksana`, `Presensi`, `Galeri` | Kegiatan, peserta, kolaborator, laporan; panitia, rundown, checklist; jadwal rutin, sesi QR, notulen; galeri foto |
| **LPJ** | `Lpj` | Laporan pertanggungjawaban kegiatan dan kepengurusan (lihat [Laporan](#laporan-devexpress-reports)) |
| Keuangan › Transaksi / Anggaran / Pengajuan Dana / Iuran / Hutang | `Transaksi`, `Anggaran`, `PengajuanDana`, `Iuran`, `Hutang` | Transaksi + saldo periode, RAB, pengajuan dana + realisasi, tagihan iuran, hutang |
| Inventaris › Aset / Peminjaman | `Aset`, `Peminjaman` | Aset, unit ber-QR + cetak label, foto, mutasi, inspeksi, perawatan; peminjaman dengan checkout dan pengembalian lewat scan QR, sewa, denda |
| Pengadaan | `Pengadaan` | Usulan, penawaran, persetujuan, pembelian, penerimaan (bisa langsung jadi aset) |
| Merchandise | `Merchandise` | Produk, varian, stok, pesanan (termasuk pesanan tamu dari `/toko`), pembayaran |
| Kemitraan › Mitra / Sponsorship | `Mitra`, `Sponsorship` | Pihak luar + kontak; paket, sponsorship, benefit, MoU |
| Persuratan › Surat / Arsip Dokumen | `Surat`, `Dokumen` | Template surat, surat masuk/keluar + persetujuan + disposisi, arsip dokumen |
| Prestasi › Lomba & Prestasi / Sertifikat | `Prestasi`, `Sertifikat` | Lomba, materi, peserta, prestasi, aturan poin, leaderboard; sertifikat |
| Formulir › Formulir / Aspirasi | `Formulir`, `Aspirasi` | Pembuat formulir, ringkasan jawaban; aspirasi anggota |
| E-Voting | `EVoting` | Pemilihan, kandidat, pemilih, hasil, log audit |
| Informasi › Berita / Banner | `Berita`, `Banner` | Berita/pengumuman, banner landing page |

Semua grid punya pencarian, filter, dan **export Excel + PDF**.

### Area Anggota (`/AreaAnggota/...`, cukup login)

| Menu | Isi |
|---|---|
| Profil Saya | Ubah kontak dan foto, lihat riwayat section, jabatan dan divisi, ganti password |
| Presensi | Scan QR sesi (atau ketik kode), selfie, lokasi, check-in / check-out, riwayat |
| Kegiatan Saya | Kegiatan untuk anggota (mis. latihan rutin) otomatis diikuti; ajukan tidak ikut satu pertemuan atau seluruh kegiatan (disetujui pengurus). Kegiatan dengan pendaftaran terbuka: daftar / batal. Melamar tim pelaksana, tugas panitia |
| Iuran Saya | Tagihan iuran sendiri |
| Peminjaman Alat | Mengajukan dan membatalkan peminjaman |
| Toko Merchandise | Katalog dan pesanan sebagai anggota |
| Formulir & Aspirasi | Mengisi formulir, mengirim aspirasi |
| E-Voting | Memilih, menerima tanda terima, cek tanda terima, hasil |
| Prestasi & Poin | Prestasi, poin dan peringkat, lomba, sertifikat |
| Disposisi | Disposisi surat untuk saya |
| Notifikasi | Daftar notifikasi; lonceng di header menampilkan jumlah yang belum dibaca |

User yang belum terhubung ke data anggota melihat pesan "bukan anggota" di halaman ini.

### Layanan lintas modul

- **Notifikasi in-app** otomatis untuk: pengajuan dan keputusan persetujuan (surat, pengajuan dana, pengadaan), disposisi, tagihan iuran, pemilihan dibuka, hasil lamaran panitia, pesanan siap diambil, keputusan dan keterlambatan peminjaman, sertifikat terbit, tanggapan aspirasi, penugasan pengurus, dan berita baru.
- **Log aktivitas**: setiap simpan, ubah, dan hapus yang berhasil tercatat (modul, aksi, data, IP, user agent). Lihat di Pengaturan Sistem.
- **Penomoran dokumen** otomatis sesuai format di Pengaturan Sistem, dengan placeholder `{seq} {type} {org} {month_roman} {month} {year} {yy}`.

## Laporan (DevExpress Reports)

Laporan cetak memakai DevExpress XtraReport + Web Document Viewer, bukan cetak dari browser. Viewer terbuka di modal layar penuh; dari viewer bisa cetak, atau export ke PDF, Word (DOCX), dan Excel.

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

Untuk mendapatkan PDF langsung tanpa viewer, tambahkan `pdf=1` pada URL cetakan. Contoh: `/Persuratan/Surat/Agenda?arah=1&pdf=1`.

### Label QR inventaris (Aset › Unit & label QR)

- Label A4, 4 kolom, ukuran 46 × 53 mm. Isinya logo, nama organisasi, QR, kode unit, nama aset, dan nomor seri.
- Isi QR adalah URL publik `{App:PublicUrl}/aset/{token}` dengan token acak 32 karakter per unit (`m_asset_unit.qr_token`), jadi URL tidak bisa ditebak dari kode inventaris. Scan dengan kamera HP membuka detail alat tanpa login; kode unit biasa di URL ditolak (404). Scanner internal (checkout, pengembalian, inspeksi, perawatan) menerima URL itu, tokennya, atau kode unit yang diketik. Label yang dicetak sebelum 2026-11-18 harus dicetak ulang.
- Isi `App:PublicUrl` dengan domain produksi **sebelum** mencetak label sungguhan.

## Job terjadwal dan pengiriman pesan

Web menjalankan job lewat `ScheduledJobService` (hosted service, dicek tiap menit). Setiap run dicatat di `t_scheduled_job_log` dan bisa dilihat di Pengaturan Sistem. Jadwal berikutnya dihitung dari run terakhir yang tercatat, jadi restart tidak mengulang job harian.

| Job | Interval | Isi |
|---|---|---|
| `SEND_OUTBOX` | 1 menit | Kirim `t_message_outbox` yang jatuh tempo (email SMTP, WhatsApp via gateway). Gagal 5 kali = Gagal, bisa diulang dari Pengaturan. |
| `CLOSE_EXPIRED_ELECTION` | 5 menit | `Election.CloseExpired()` |
| `MARK_BORROW_OVERDUE` | 30 menit | `BorrowRequest.MarkOverdue` (anggota peminjam mendapat notifikasi) |
| `GENERATE_ATTENDANCE_SESSIONS` | 6 jam | Sesi presensi dari jadwal rutin, hari ini s.d. `Jobs:SessionDaysAhead` hari ke depan |
| `PURGE_OLD_DATA` | harian | Notifikasi dibaca > 90 hari, log aktivitas > 365 hari, token > 30 hari, log job > 90 hari |

Konfigurasi ada di [Konfigurasi](#konfigurasi). Method library yang dipanggil job (`Election.CloseExpired`, `BorrowRequest.MarkOverdue`, `ActivitySchedule.GenerateSessions`, `Notification.Purge`, `ActivityLog.Purge`, `UserToken.Purge`, `ScheduledJobLog.Purge`) juga punya versi `...Async` bila ingin dijalankan dari scheduler lain.

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
- **Logo** diambil dari profil organisasi lewat `/Brand/Logo`; kalau belum ada, dipakai `assets/images/gwa-logo.png`.

## Panduan pengembangan

### Build

```
dotnet build GitaWidyaAgni.sln
```

Belum ada project test. Sekitar 18 warning build berasal dari helper yang disalin dari ClubShared.

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

1. Buat kelas laporan lewat kode di `GitaWidyaAgniWeb/Reports/` (tanpa `.repx`). Untuk dokumen, turunkan dari `DocumentReport`, yang sudah menyediakan blok kop surat, judul, paragraf, tabel, isi HTML, tanda tangan, dan tanda air. Identitas organisasi diambil dari `ReportBrand.Load(webRoot)`.
2. Aksi controller cukup memanggil `return Laporan(() => report);`. Hasilnya halaman viewer bersama; dengan `?pdf=1`, hasilnya PDF langsung.
3. Di JS, buka viewernya dengan `AppUI.openReport(judul, url)`. Tombol tambahan di toolbar bisa ditambahkan lewat `ToolbarModel.Extra`.

Satuan bawaan laporan adalah 1/100 inci (A4 = 827 × 1169).

### Konvensi kode

- Service GWALib memakai pola `<Nama>.cs` + `<Nama>.Services.cs`, `NewRow`, `GetDataList` / `Save` / `Update` / `Delete`, dibungkus `ServiceHandler.Handle`. `ArgumentException` adalah pesan untuk pengguna.
- Pesan untuk pengguna ditulis dalam bahasa Indonesia sebagai konstanta di `GWAShared.Results.ExceptionMessage`.
- Hasil service berupa `Response<T>` dari `ResponseFactory`. Di web, `.Ok()` mengambil data atau melempar pesan errornya.
- `<summary>` setiap enum mencantumkan semua `tabel.kolom` yang memakainya, dan setiap nilainya diberi `[Description]` berbahasa Indonesia.
- `EncryptString<T>()` / `DecryptString<T>()` memakai kunci yang bergantung pada nama kelas dan tanggal hari ini, jadi encKey hanya berlaku pada hari yang sama. Nama kelas maksimal 24 karakter. Kirim encKey sebagai query string (`?key=`), bukan segmen path, karena encKey bisa berisi `/`.
- Jangan menulis ulang file repo lewat PowerShell `Get-Content` / `Set-Content`, karena karakter UTF-8 bisa rusak. Pakai editor biasa.

## Deploy ke server

1. Buat database dari `database/gitawidyaagni_create.sql`. Untuk server yang sudah ada, jalankan migrasi yang belum diterapkan.
2. Publish: `dotnet publish GitaWidyaAgniWeb -c Release -o ./publish`. Build di mesin yang punya DevExpress 23.2 berlisensi.
3. Isi environment variable server: `ConnectionStrings__Default`, `EVoting__Key`, `App__PublicUrl`, `Smtp__*`, `WhatsApp__*`, dan `Jobs__Enabled` (`false` di instance tambahan).
4. Jalankan di belakang HTTPS. Cookie login `Secure`, serta kamera dan lokasi untuk presensi, hanya bekerja di HTTPS.
5. Isi menu dan hak akses dengan `dotnet run --project GWASeeder -- --sync-akses` (connection string dari env `GWA_DB`), lalu buat admin pertama lewat `Bootstrap:*`.

## Troubleshooting

| Gejala | Penyebab / solusi |
|---|---|
| Restore gagal mencari `DevExtreme.AspNet.*` / `DevExpress.AspNetCore.Reporting` | DevExpress 23.2 belum terpasang, atau terpasang di folder lain. Sesuaikan path di `NuGet.config`. |
| Build gagal `MSB3027 ... locked by "GitaWidyaAgniWeb"` | Web masih berjalan. Hentikan prosesnya dulu. |
| Web gagal start: connection string kosong | `appsettings.Development.json` belum dibuat, atau env `ConnectionStrings__Default` belum diisi. |
| Menu baru tidak muncul / halaman ditolak | Controller belum ada di katalog, atau `--sync-akses` belum dijalankan. Login ulang, atau tunggu 5 menit sampai cache menu habis. |
| E-voting ditolak "Kunci enkripsi e-voting belum dikonfigurasi" | `EVoting:Key` kosong. Pakai kunci yang sama dengan saat pemilihan dibuka. |
| Label QR membuka `localhost` di HP | `App:PublicUrl` belum diisi saat label dicetak. Isi lalu cetak ulang label. |
| PDF laporan bertuliskan "trial version of DevExpress" | Build memakai DevExpress trial. Build ulang dengan lisensi. |
| Pesan email / WhatsApp tetap Menunggu | `Smtp:*` atau `WhatsApp:*` belum diisi, atau `Jobs:Enabled` = false. |
| "Form edit sudah kedaluwarsa" | Kunci edit tersimpan sekali pakai, dan encKey berganti tiap hari. Buka ulang form edit. |
| Tes halaman login lewat curl gagal | Cookie `Secure` tidak dikirim lewat `http://`. Jalankan web di `https://localhost:<port>` dan pakai `curl -k` dengan cookie jar. |

## Catatan keamanan

- Jangan commit password, token, atau kunci. `appsettings.Development.json` sudah di-ignore; untuk nilai lain pakai user-secrets atau environment variable.
- `GWALib/EF/ModelContext.cs` masih memuat baris `OnConfiguring` lama yang dikomentari, dengan password root database. Baris itu sudah ada di riwayat git. **Ganti password root MySQL tersebut** dan hapus barisnya.
- Akun dan password contoh dari GWASeeder hanya untuk pengembangan. Jangan jalankan seeder data contoh di database produksi.
- Kunci `EVoting:Key` membuka isi suara pemilihan yang belum difinalisasi. Simpan hanya di secret server.
