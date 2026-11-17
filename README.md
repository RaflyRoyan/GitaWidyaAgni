# GitaWidyaAgni

Sistem manajemen **Marching Band Gita Widya Agni (MB GWA) UPN "Veteran" Jawa Timur**. Repo ini berisi library backend .NET 8 untuk satu organisasi (anggota dan kepengurusan, program dan kegiatan, presensi, keuangan, persuratan, e-voting, pengadaan, sponsorship, inventaris, dan lain-lain), aplikasi web ASP.NET Core MVC, dan seeder data contoh.

Organisasi adalah level tertinggi. Tidak ada tenant, kampus, kategori UKM, atau pegawai.

> **Status: dalam pengembangan.**
> - Library sudah lengkap: layer service ada untuk semua tabel master dan semua 88 tabel transaksi.
> - Semua modul sudah dijalankan ke database lewat `GWASeeder`, dan seluruh 131 tabel berhasil terisi.
> - Web sudah punya fondasi (auth, hak akses, menu dinamis), landing page, login, dan kerangka dashboard bertema GWA.
> - Halaman pengelolaan per modul belum dibuat. Ada 37 menu sidebar yang tautannya masih 404; daftarnya ada di `CLAUDE.md` bagian "Web pages to build".

## Struktur solution

```
GitaWidyaAgni.sln
├── GWAShared/            Helper tanpa dependency
│   ├── Enums/            Satu enum per file untuk setiap kolom berkode (namespace GWAShared.Enums)
│   ├── Extensions/       MappingData, enkripsi string, enum, decimal, value
│   ├── Models/           EnumItem, SelectListItemCustom
│   ├── Results/          Response<T>, ResponseFactory, ResponseMessages, ExceptionMessage
│   └── Utilities/        Crypto, File, Session, Validate
├── GWALib/               Akses data dan logika bisnis (mereferensikan GWAShared)
│   ├── EF/               Entity dan ModelContext hasil scaffold database-first (Pomelo MySQL)
│   ├── General/          Koneksi, penanganan error, utilitas (id dari m_sequence, gambar, validasi)
│   └── Lib/              Service per tabel: Master/<Modul>/ dan Transaksi/<Modul>/
├── GitaWidyaAgniWeb/     Aplikasi web ASP.NET Core 8 MVC (template Velzon + DevExtreme)
│   ├── Controllers/      Landing (publik), Login, Home (dashboard), Brand (logo)
│   ├── Infrastructure/   AccessFilter, AccessService, BaseController, ClaimsExtensions
│   ├── Views/            Landing, Login, Home, Shared (layout publik, login, dashboard)
│   └── wwwroot/          assets/ (Velzon + salinan *.gwa.min.css), css/landing.css, login.css, gwa-theme.css, gwa-sidebar.css
└── GWASeeder/            Console untuk mengisi semua tabel dengan data contoh MB GWA
```

Struktur dan konvensinya mengikuti project ClubSphere (ClubLib / ClubShared).

## Prasyarat

- .NET SDK 8.0 atau yang lebih baru.
- MySQL 8.0.
- DevExpress 23.2. `NuGet.config` mengambil paket `DevExtreme.AspNet.*` dari folder lokal `C:\Program Files\DevExpress 23.2\DevExtreme\System\DevExtreme\Bin\AspNetCore`, jadi restore akan gagal di mesin yang belum memasang DevExpress 23.2.

## Build

```
dotnet build GitaWidyaAgni.sln
```

Belum ada project test.

## Menjalankan web

1. Buat database dari skrip `database/gitawidyaagni_create.sql` (lihat bagian Database dan `database/README.md`).
2. Isi connection string asli di `GitaWidyaAgniWeb/appsettings.Development.json`. File ini di-ignore git, sehingga password tidak ikut ter-commit. Isinya:
   ```json
   { "ConnectionStrings": { "Default": "server=127.0.0.1;port=3306;database=gitawidyaagni;user=root;password=..." } }
   ```
   `appsettings.json` yang ter-commit hanya berisi placeholder. Di server, isi lewat env `ConnectionStrings__Default`.
3. Buat admin pertama dengan user-secrets. Admin hanya dibuat selama tabel `m_user` masih kosong:
   ```
   dotnet user-secrets set "Bootstrap:AdminUsername" "admin" --project GitaWidyaAgniWeb
   dotnet user-secrets set "Bootstrap:AdminPassword" "<password kuat>" --project GitaWidyaAgniWeb
   ```
   Hasilnya role `SUPERADMIN`, user admin, dan relasi `m_user_role`. Setelah login pertama, hapus kedua secret itu.
4. Isi kunci e-voting:
   ```
   dotnet user-secrets set "EVoting:Key" "<base64 32 byte>" --project GitaWidyaAgniWeb
   ```
5. Jalankan web:
   ```
   dotnet run --project GitaWidyaAgniWeb
   ```
   Halaman pertama adalah landing page. Tombol **Login** di navbar membuka halaman login.

Hentikan web sebelum build ulang dari IDE. Proses web yang masih berjalan mengunci `GWALib.dll` / `GWAShared.dll`, sehingga muncul error `MSB3027 ... The file is locked by: "GitaWidyaAgniWeb"`.

## Data contoh (GWASeeder)

```
dotnet run --project GWASeeder                    # database kosong → isi data contoh
dotnet run --project GWASeeder -- --reset         # KOSONGKAN semua tabel lalu isi ulang (ketik RESET; --yes melewati)
dotnet run --project GWASeeder -- --sync-akses    # hanya sinkron controller/aksi, menu, role di database yang sudah berisi
```

Opsi tambahan: `--evoting-key <base64>` dan `--admin-password <password>`.

- Mengisi semua 131 tabel lewat service GWALib, sehingga semua validasi ikut berjalan. Datanya meliputi:
  - controller + aksi, menu sidebar (termasuk **Area Anggota**), role, dan hak akses;
  - profil MB GWA;
  - dua periode kepengurusan, dengan serah terima periode lama ke periode baru;
  - 24 anggota dengan jabatan berbeda (lihat tabel akun);
  - kegiatan, presensi, keuangan, sponsorship, pengadaan, merchandise, inventaris, surat, prestasi, formulir, e-voting, berita, dan banner.
- User `admin` (SUPERADMIN) dibuat otomatis bila `m_user` kosong. Password dari `--admin-password`, env `GWA_ADMIN_PASSWORD`, atau bawaan `admin12345`. Ganti setelah login pertama.
- Tanpa `--reset`, seeder menolak jalan bila `m_organization` sudah berisi.
- `--sync-akses` memakai katalog di `GWASeeder/Seed.Akses.cs`. Jalankan setelah menambah halaman web baru. Aksi, menu role, dan detail menu milik role katalog disamakan persis dengan katalog, jadi perubahan manual pada role itu ikut tertimpa. Role lain dibiarkan.
- Connection string diambil dari env `GWA_DB`. Kalau tidak ada, dari `GitaWidyaAgniWeb/appsettings.Development.json`, lalu `appsettings.json`.
- Kunci e-voting diambil dari argumen `--evoting-key`, env `GWA_EVOTING_KEY`, atau `EVoting:Key`. Kalau tidak ada, kunci dibuat acak dan dicetak di akhir. Web harus memakai kunci yang sama supaya pemilihan bisa difinalisasi.

### Akun contoh

Password semua akun anggota: `gwa12345`.

| Username | Nama | Jabatan (periode aktif) | Role |
|---|---|---|---|
| `admin` | Administrator | - | SUPERADMIN |
| `ketua` | Rizky Pratama Putra | Ketua Umum | KETUA, ANGGOTA |
| `wakil` | Nabila Ayu Lestari | Wakil Ketua Umum | WAKIL, ANGGOTA |
| `sekretaris` | Dimas Arya Saputra | Sekretaris | SEKRETARIS, ANGGOTA |
| `bendahara` | Salsabila Putri Ramadhani | Bendahara | BENDAHARA, ANGGOTA |
| `kadiv.brass` | Aditya Wicaksono | Kepala Divisi Brass | KADIV, ANGGOTA |
| `kadiv.battery` | Bagas Setiawan | Kepala Divisi Battery | KADIV, ANGGOTA |
| `kadiv.guard` | Citra Maharani | Kepala Divisi Colour Guard | KADIV, ANGGOTA |
| `kadiv.front` | Dewi Kartika Sari | Kepala Divisi Front Ensemble | KADIV, ANGGOTA |
| `kadiv.logistik` | Eko Prasetyo | Kepala Divisi Logistik | KADIV, ANGGOTA |
| `kadiv.humas` | Farah Aulia | Kepala Divisi Humas & Media | KADIV, ANGGOTA |
| `ketua.lama` | Fajar Nugroho | Anggota (Ketua Umum periode lalu) | ANGGOTA |
| `anggota01` … `anggota11` | 11 anggota biasa | Anggota divisi musik | ANGGOTA |
| `alumni01`, `alumni02` | 2 alumni | Alumni | - |

Hak akses per role:

| Role | Menu pengelolaan |
|---|---|
| KETUA, WAKIL | Semua modul operasional (tanpa Akses & Pengaturan Sistem, yang khusus SUPERADMIN) |
| SEKRETARIS | Anggota, Alumni, Rekrutmen, Kegiatan, Presensi, Surat, Dokumen, Sertifikat, Formulir, Aspirasi, Berita |
| BENDAHARA | Transaksi, Anggaran, Pengajuan Dana, Iuran, Hutang, Pengadaan, Merchandise, Sponsorship |
| KADIV | Program Kerja, Kegiatan, Tim Pelaksana, Presensi, Galeri, Aset, Peminjaman, Prestasi, Berita |
| ANGGOTA | Hanya Area Anggota (semua role juga mendapat menu ini) |

Siapa yang boleh menyetujui dokumen tetap ditentukan alur persetujuan: pengajuan dana dan pengadaan oleh Bendahara lalu Ketua Umum, dan surat keluar oleh Sekretaris lalu Ketua Umum.

### Kosakata aksi web

Setiap controller pengelolaan punya aksi `Index`, `Create`, `Edit`, dan `Delete`, ditambah sebagian dari aksi berikut sesuai modulnya:

| Aksi | Untuk |
|---|---|
| `Index` | Membuka halaman dan semua data baca (daftar, detail, unduh file) |
| `Create` / `Edit` / `Delete` | Data utama maupun data anak di halaman itu |
| `Submit` | Mengajukan dokumen ke alur persetujuan |
| `Approve` | Menyetujui, menolak, atau meminta revisi |
| `Process` | Perubahan status operasional (buka/tutup, publikasi, checkout, pengembalian, dan sejenisnya) |
| `Payment` | Menerima, mencatat, atau membatalkan pembayaran yang membuat transaksi keuangan |

Controller Area Anggota (`ProfilSaya`, `PresensiSaya`, `KegiatanSaya`, `IuranSaya`, `PeminjamanSaya`, `Toko`, `FormulirSaya`, `Pemilihan`, `PrestasiSaya`, `DisposisiSaya`, `Notifikasi`) cukup login.

## Konfigurasi library saat startup

Aplikasi yang memakai library ini wajib memanggil dua method berikut sekali saat startup. `GitaWidyaAgniWeb/Program.cs` sudah melakukannya.

```csharp
// Connection string MySQL.
DBClass.Configure(connectionString);

// Kunci e-voting: base64 dari 32 byte acak, dari secret aplikasi (bukan database).
// Tanpa kunci ini, membuka pemungutan suara, memilih, dan finalisasi akan ditolak.
// Kunci tidak boleh diganti sebelum semua pemilihan yang memakainya difinalisasi.
ElectionVote.ConfigureKey(base64Key);
```

Membuat kunci e-voting, misalnya dengan PowerShell:

```powershell
[Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
```

`OnConfiguring` di `GWALib/EF/ModelContext.cs` sengaja dibiarkan dikomentari supaya connection string tidak ikut ter-commit. Scaffold ulang akan menimpa file EF.

## Autentikasi & hak akses (web)

- **Login:** memakai cookie authentication (`gwa.auth`, berlaku 8 jam dan diperpanjang selama aktif). Claim `NameIdentifier` berisi `m_user.id`, dan nilai inilah yang diteruskan ke parameter `user` di method GWALib. Password diverifikasi `UserAccount.Login` memakai `PasswordHasher`.
- **Filter global:** semua halaman wajib login, lalu `AccessFilter` mengecek hak akses, lalu token antiforgery divalidasi.
- **Aturan `AccessFilter`:**
  - `[AllowAnonymous]`: terbuka untuk siapa saja.
  - `[LoginOnly]`: cukup login.
  - Aksi yang terdaftar di `m_controller_action` dengan `need_auth = 1`: harus ada di `m_role_action` milik salah satu role user.
  - **Aksi yang belum terdaftar: hanya SUPERADMIN.** Setiap halaman baru wajib didaftarkan di master Controller Action.
- **Sidebar:** menu dibangun dari `m_header_menu`, `m_sub_menu`, dan `m_role_menu` sesuai role user. Role hanya berlaku kalau `m_user_role` tidak terikat periode, atau terikat periode yang sedang Active.

## Tampilan publik

- **Landing page (`/`):**
  - Tema terang yang hangat dan senada dengan warna logo: gading, krem, emas `#e8b52a`, merah api `#c8202a`, dan hijau pita. Tidak ada section gelap.
  - Font Playfair Display + Inter.
  - Isi diambil dari data (profil organisasi, banner, kegiatan publik, prestasi, galeri, berita, rekrutmen yang sedang buka, site setting).
  - Tersedia juga halaman daftar berita `/Landing/Berita` dan detail berita `/berita/{slug}`.
- **Login:** layar dibagi dua dengan warna yang sama seperti landing. Ada panel foto organisasi, logo, dan kutipan di kiri, form di kanan. Form punya tombol tampil/sembunyikan password, peringatan Caps Lock, dan status loading saat dikirim.
- **Stylesheet:** `wwwroot/css/landing.css` (token warna dan komponen) dan `wwwroot/css/login.css`. Logo diambil dari profil organisasi lewat `/Brand/Logo`; kalau profil belum punya logo, dipakai `assets/images/gwa-logo.png`.

## Tampilan dashboard

- **Warna:** template Velzon memakai palet GWA lewat salinan `assets/css/bootstrap.gwa.min.css`, `app.gwa.min.css`, dan `custom.gwa.min.css`. File asli Velzon tidak diubah. Penggantian warnanya:

  | Warna Velzon | Jadi |
  |---|---|
  | primary navy | **bronze emas `#9a6b00`** |
  | secondary | **merah api `#c8202a`** |
  | success | **hijau pita `#1f7a45`** |
  | warning | **emas `#e8b52a`** |
  | latar | **gading `#f8f4ec`** |

  Kalau template Velzon diperbarui, buat ulang salinan ini dengan pemetaan yang sama.
- **Sidebar dan header** mengikuti model SurabayaTaxWeb, dengan gaya di `wwwroot/css/gwa-sidebar.css`:
  - Sidebar bergradasi merah api ke oranye bakar dengan teks putih.
  - Bagian atas sidebar berisi logo besar dan judul "GITA WIDYA AGNI / BACK OFFICE"; bagian bawahnya ada ilustrasi logo yang samar.
  - Menu aktif tampil sebagai pill putih transparan dengan garis emas.
  - Saat sidebar diciutkan, submenu muncul sebagai panel melayang berjudul.
  - Header berisi judul halaman, tautan ke situs publik, tombol fullscreen, dan dropdown user (avatar inisial emas, nama anggota, role aktif, logout).
- **Detail lain** ada di `wwwroot/css/gwa-theme.css`: font Inter + Playfair Display, kartu, blok sambutan, loader, dan aksen plugin.
- **Halaman pengelolaan** mengikuti struktur BEGateSecuritySystemWeb (detail lengkap di CLAUDE.md, "Management page pattern"):
  - Satu area per header sidebar, misalnya `[Area("Akses")]` dengan URL `/Akses/User`. View tetap di `Views/<Area>/<Controller>/`.
  - Setiap halaman terdiri atas `Index` (kartu + toolbar Tambah / Tampilkan Legenda), `_List` (grid), modal `_Add` / `_Edit` yang memakai `_Form` bersama, dan modul JS `wwwroot/Scripts/<Area>/<Controller>.js`.
  - Semua grid memakai `ListGrid("Nama")`: filter, header filter, pager 10/20/50, dan **export Excel + PDF** (`wwwroot/js/grid-export.js`). Header grid dan PDF berwarna bronze.
  - Judul halaman dan breadcrumb dibuat otomatis oleh `ViewComponents/PageTitle`. Helper JavaScript ada di `AppUI` (`wwwroot/js/site.js`).

## Halaman yang belum dibuat

Menu sidebar dan data Controller Action sudah ada di database (dari `GWASeeder`), tetapi controller web-nya belum dibuat. Urutan pengerjaan yang disarankan:

1. Akses: User, Role, Menu.
2. Pengaturan Sistem.
3. Organisasi: Profil, Periode, Divisi, Jabatan, Kepengurusan.
4. Anggota: Data Anggota, Alumni, Rekrutmen.
5. Program Kerja.
6. Kegiatan: Daftar Kegiatan, Tim Pelaksana, Presensi, Galeri.
7. Keuangan: Transaksi, Anggaran, Pengajuan Dana, Iuran, Hutang.
8. Inventaris: Aset, Peminjaman.
9. Pengadaan dan Merchandise.
10. Kemitraan: Mitra, Sponsorship.
11. Persuratan: Surat, Arsip Dokumen.
12. Prestasi: Lomba & Prestasi, Sertifikat.
13. Formulir: Formulir, Aspirasi.
14. E-Voting.
15. Informasi: Berita, Banner.

Setelah itu menyusul halaman self-service anggota, seperti profil, scan presensi, iuran saya, e-voting, dan notifikasi. Rincian controller dan kelas GWALib per menu ada di `CLAUDE.md` bagian "Web pages to build".

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

Aplikasi web menjalankan semuanya lewat `ScheduledJobService` (hosted service, dicek tiap menit). Jadwal dan konfigurasinya:

| Job | Interval | Isi |
|---|---|---|
| `SEND_OUTBOX` | 1 menit | Kirim `t_message_outbox` yang jatuh tempo (email SMTP, WhatsApp via gateway) |
| `CLOSE_EXPIRED_ELECTION` | 5 menit | `Election.CloseExpired()` |
| `MARK_BORROW_OVERDUE` | 30 menit | `BorrowRequest.MarkOverdue` (anggota peminjam mendapat notifikasi) |
| `GENERATE_ATTENDANCE_SESSIONS` | 6 jam | Sesi presensi hari ini s.d. 7 hari ke depan |
| `PURGE_OLD_DATA` | harian | Notifikasi dibaca > 90 hari, log aktivitas > 365 hari, token > 30 hari, log job > 90 hari |

- `Jobs:Enabled` (bawaan `true`) dan `Jobs:Username` (kosong = user SUPERADMIN aktif pertama). Bila web berjalan di beberapa instance, aktifkan job di satu instance saja.
- Email: `Smtp:Host`, `Smtp:Port`, `Smtp:EnableSsl`, `Smtp:Username`, `Smtp:Password`, `Smtp:From`, `Smtp:FromName`.
- WhatsApp: `WhatsApp:Url` (mis. `https://api.fonnte.com/send`) dan `WhatsApp:Token`.
- Simpan password dan token di user-secrets, contohnya `dotnet user-secrets set "Smtp:Password" "..." --project GitaWidyaAgniWeb`. Saluran yang belum diisi tidak dikirim, dan pesannya tetap berstatus Menunggu.

## Database

- Skrip pembuatan database: [`database/gitawidyaagni_create.sql`](database/gitawidyaagni_create.sql) (MySQL 8.0, database `gitawidyaagni`, collation `utf8mb4_0900_ai_ci`), berisi 132 tabel dan 267 relasi. Perubahan skema berikutnya ada di `database/migrations/`; cara pakai di [`database/README.md`](database/README.md).
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
| E-Voting | Pemilihan dengan suara anonim terenkripsi (AES-256-GCM), tanda terima pemilih |
| Prestasi & Sertifikat | Lomba, materi, peserta/tim, prestasi, sertifikat berkode verifikasi, poin keaktifan |
| Formulir & Aspirasi | Formulir dan survei (evaluasi kegiatan, pertanyaan rekrutmen), aspirasi/pengaduan |
| Landing & Informasi | Berita, banner, pengaturan situs, notifikasi, log aktivitas |

## Konvensi kode

- Pesan untuk pengguna ditulis dalam bahasa Indonesia sebagai konstanta di `GWAShared.Results.ExceptionMessage`.
- Hasil service dikembalikan sebagai `Response<T>` yang dibuat lewat `ResponseFactory`.
- `<summary>` setiap enum mencantumkan semua `tabel.kolom` yang memakainya, dan setiap nilainya diberi `[Description]` berbahasa Indonesia.
- `StringExtensions.EncryptString<T>()` / `DecryptString<T>()` memakai kunci yang bergantung pada tanggal hari ini, sehingga id terenkripsi hanya bisa didekripsi pada hari yang sama. Kalau encKey dipakai di URL, kirim sebagai query string (`?key=`), bukan segmen path, karena encKey bisa berisi `/`.
- Controller web untuk halaman setelah login mewarisi `BaseController` (`LibUser` untuk parameter `user` GWALib, `LibResult` untuk respons JSON).
