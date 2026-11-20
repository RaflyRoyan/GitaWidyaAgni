# Panduan Pengguna — Sistem MB Gita Widya Agni

Panduan ini menjelaskan cara memakai aplikasi web MB Gita Widya Agni sesuai peran masing-masing. Mulai dari bagian **Dasar** di bawah, lalu buka panduan peran Anda.

| Peran | Panduan |
|---|---|
| Anggota, pengurus, dan alumni (semua akun) | [Area Anggota](01-anggota.md) |
| Ketua Umum dan Wakil Ketua Umum | [Ketua & Wakil](02-ketua-wakil.md) |
| Sekretaris | [Sekretaris](03-sekretaris.md) |
| Bendahara | [Bendahara](04-bendahara.md) |
| Kepala Divisi dan Section Leader | [Kadiv & Section Leader](05-kadiv-section-leader.md) |
| Administrator sistem (SUPERADMIN) | [Administrator](06-admin.md) |

Satu akun bisa punya lebih dari satu peran. Misalnya Bendahara juga anggota, jadi ia memakai Area Anggota dan menu Keuangan sekaligus. Menu di sidebar hanya menampilkan halaman yang boleh dibuka oleh peran Anda.

## Dasar

### Masuk dan keluar

1. Buka alamat aplikasi, lalu klik **Login** di kanan atas halaman depan.
2. Isi username dan password, lalu klik **Masuk**. Tombol mata menampilkan password; peringatan muncul bila Caps Lock menyala.
3. Untuk keluar, klik nama Anda di kanan atas, lalu **Logout**.

Sesi login berlaku 8 jam dan diperpanjang selama Anda aktif. Lupa password? Minta sekretaris atau administrator mereset password Anda. Ganti password sendiri di **Area Anggota › Profil Saya › Ganti Password**.

### Dashboard

Halaman pertama setelah login. Isinya menyesuaikan peran:

- **Ringkasan saya** (akun yang terhubung ke data anggota): kehadiran periode ini dan bulan ini, poin keaktifan dan peringkat, iuran yang belum dibayar, kepanitiaan mendatang, serta jadwal presensi 7 hari ke depan dengan tombol Check-in. Kartu kehadiran menjadi merah dan muncul peringatan bila kehadiran di bawah 75%. Setiap kartu bisa diklik untuk membuka halaman detailnya.
- **Angka organisasi**: jumlah anggota, kegiatan mendatang, saldo kas dan iuran (hanya peran keuangan), aset.
- **Grafik**: arus kas 6 bulan dan anggota per section.
- **Agenda mendatang**, **Perlu tindakan** (persetujuan yang menunggu Anda, disposisi dan iuran Anda), **Poin keaktifan** 5 besar, **ulang tahun** 7 hari ke depan, dan **pintasan** menu.

### Pola halaman pengelolaan

Hampir semua halaman pengelolaan memakai pola yang sama:

- **Tombol tambah** di kiri atas (mis. "Tambah Anggota") membuka formulir di jendela modal. Kolom bertanda wajib divalidasi sebelum disimpan.
- **Tampilkan Legenda** menjelaskan arti tombol kecil di kolom aksi tiap baris. Warna tombol: kuning = ubah, merah = hapus / nonaktifkan, hijau = data turunan atau akses, biru / abu = lihat.
- **Tabel** bisa dicari (kotak pencarian), difilter per kolom, diurutkan (klik judul kolom), dan diekspor ke **Excel** atau **PDF** dari tombol di kanan atas tabel.
- Baris berlatar merah muda = data tidak aktif.
- Pesan hijau = berhasil; pesan merah menjelaskan kenapa aksi ditolak (mis. data sudah dikunci).

### Laporan cetak

Laporan resmi (surat, LPJ, KTA, sertifikat, laporan keuangan, rapor, rekap) dibuka di **penampil laporan** layar penuh. Dari toolbar penampil Anda bisa mencetak, berpindah halaman, dan mengekspor ke PDF, Excel, atau format lain. Beberapa tombol juga menyediakan **Unduh PDF** atau **Unduh Excel** langsung.

Laporan memakai kop surat dari Profil Organisasi dan menampilkan penanda tangan dari data Kepengurusan. Jika nama penanda tangan kosong, berarti jabatan itu belum diisi di **Organisasi › Kepengurusan**.

**Tanda tangan elektronik (QR).** Laporan keuangan, rapor, rekap poin, rekap kehadiran, laporan inventaris, invoice / kuitansi peminjaman, dan surat keluar yang sudah disetujui memakai QR di tempat tanda tangan, dengan tulisan "Ditandatangani secara elektronik". Siapa pun yang memindai QR melihat halaman verifikasi: nama dan jabatan penanda tangan, judul dan tanggal dokumen, serta kapan dokumen diterbitkan. Cocokkan isinya dengan dokumen; bila berbeda, dokumen sudah diubah. **LPJ** dan dokumen yang belum disetujui tetap memakai tanda tangan basah.

### Notifikasi

Lonceng di kanan atas menampilkan jumlah notifikasi yang belum dibaca. Notifikasi dikirim untuk persetujuan, disposisi, tagihan iuran, pengingat presensi, keputusan izin, sertifikat baru, dan lain-lain. Atur saluran (aplikasi, email, WhatsApp) melalui administrator.

### Halaman publik

Tanpa login, siapa pun bisa membuka halaman depan (profil, agenda, prestasi, berita), **/rekrutmen** (daftar anggota baru), **/toko** (pesan merchandise), **/sertifikat/verifikasi** (cek keaslian sertifikat), serta halaman hasil pindai QR KTA (**/kta/…**) dan label aset (**/aset/…**).
