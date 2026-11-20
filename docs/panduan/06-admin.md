# Panduan Administrator (SUPERADMIN)

SUPERADMIN membuka semua halaman, termasuk **Akses** dan **Pengaturan Sistem** yang tidak tersedia untuk peran lain. Panduan teknis instalasi dan konfigurasi server ada di [README proyek](../../README.md).

Kembali ke [daftar panduan](README.md).

## Pengguna dan peran

**Akses › User**

- **Tambah User**: username dan password awal. Tautkan user ke data anggota di **Anggota › Data Anggota** agar Area Anggota dan Ringkasan saya berfungsi.
- **Role user**: tambahkan peran (KETUA, WAKIL, SEKRETARIS, BENDAHARA, KADIV, SECTION_LEADER, ANGGOTA). Peran bisa diikat ke satu periode; peran berperiode hanya berlaku selama periode itu Aktif. **Cabut role** untuk mencabutnya.
- **Reset password** untuk user yang lupa password.
- **Nonaktifkan user** agar tidak bisa login.

**Akses › Role**: **Hak akses** mengatur aksi (Index, Create, Edit, Delete, Submit, Approve, Process, Payment) dan menu per peran. Perubahan langsung berlaku tanpa login ulang.

**Akses › Menu**: struktur sidebar (header menu, sub menu, controller, dan daftar aksi). Halaman baru harus didaftarkan di sini; aksi yang tidak terdaftar hanya bisa dibuka SUPERADMIN.

Pengurus berganti setiap periode. Setelah serah terima, cabut atau sesuaikan peran pengurus lama dan beri peran ke pengurus baru.

## Pengaturan Sistem

| Tab | Kegunaan |
|---|---|
| Pengaturan Situs | Alamat, kota tanda tangan, WhatsApp, media sosial, teks halaman depan |
| Format Nomor | Pola nomor dokumen per jenis (surat, transaksi, pengadaan, sertifikat, …), mis. `{seq}/{type}/{org}/{month_roman}/{year}` |
| Alur Persetujuan | Langkah persetujuan per jenis dokumen (surat, pengajuan dana, pengadaan); penyetuju per peran, jabatan, atau user tertentu |
| Template Pesan | Teks email / WhatsApp (pengingat presensi, ulang tahun, undangan alumni, reset password, …) |
| Antrian Pesan | Pesan email / WhatsApp yang menunggu, terkirim, atau gagal; **Kirim ulang** atau **Batalkan pesan** |
| Log Job | Riwayat tugas terjadwal (kirim pesan, buka / tutup sesi presensi, ucapan ulang tahun, pembersihan data) |
| Log Aktivitas | Jejak setiap perubahan data: siapa, kapan, modul, aksi, dan nilai baru |
| Sequence ID | Nomor id terakhir per tabel (hanya untuk perbaikan data) |

**Organisasi › Profil Organisasi**: nama, kode, logo, visi-misi, kontak, dan pembina. Logo dan nama dipakai di kop semua laporan, KTA, dan sertifikat. Klik **Simpan Profil** setelah mengubah.

## Pemeriksaan rutin

- **Antrian Pesan**: pesan Gagal biasanya karena nomor WhatsApp salah atau gateway belum dikonfigurasi.
- **Log Job**: pastikan job berjalan. Hanya satu server yang boleh menjalankan job (`Jobs:Enabled`).
- **Log Aktivitas**: telusuri perubahan bila ada data yang dipertanyakan.
- Sebelum mencetak label QR atau KTA untuk dipakai, pastikan alamat publik aplikasi (`App:PublicUrl`) sudah benar. QR yang tercetak memuat alamat ini.
