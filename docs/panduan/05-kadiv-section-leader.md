# Panduan Kepala Divisi & Section Leader

| Peran | Menu |
|---|---|
| **KADIV** (Kepala Divisi) | Program Kerja, Kegiatan (Daftar Kegiatan, Tim Pelaksana, Presensi, Galeri), Inventaris (Aset, Peminjaman), Prestasi, Berita |
| **SECTION_LEADER** | Section, Kegiatan (Daftar Kegiatan, Presensi, Galeri), Inventaris (Aset, Peminjaman), Prestasi |

Kembali ke [daftar panduan](README.md).

## Section (Section Leader)

**Organisasi › Section**: lihat anggota section Anda. **Riwayat section** memindahkan anggota ke section lain mulai tanggal tertentu (mis. pindah dari Brass ke Battery untuk satu event); riwayat lama tetap tercatat.

Latihan sectional dibuat sebagai kegiatan dengan sasaran **Section**. Hanya anggota section itu (pada tanggal sesi) yang diharapkan hadir.

## Program kerja (Kadiv)

**Program Kerja**

- **Tambah Program**: nama, bidang (divisi), jangka (pendek / panjang), dan rencana. **Mulai program**, lalu **Selesaikan program** atau **Batalkan program**.
- **Tambah Tugas**: tugas per program dengan penanggung jawab dan tenggat. **Mulai dikerjakan**, lalu **Tandai selesai** (bisa **Buka lagi**).
- **Tambah Rencana** (timeline): rencana bulanan; **Tautkan kegiatan** saat kegiatannya sudah dibuat.

Evaluasi program per bidang dan jangka masuk otomatis ke LPJ kepengurusan.

## Kegiatan dan tim pelaksana

- **Kegiatan › Daftar Kegiatan › Tambah Kegiatan**: lihat [Sekretaris](03-sekretaris.md#kegiatan-dan-presensi). Setujui atau tolak **pengajuan tidak ikut** anggota dari aksi baris kegiatan.
- **Kegiatan › Tim Pelaksana** (Kadiv), per kegiatan:
  - **Tambah Seksi** (sie acara, konsumsi, dll.) dan **Tambah Panitia**. Ketua Pelaksana, Wakil, Sekretaris, dan Bendahara panitia tidak memakai seksi; Koordinator Sie dan Staf wajib punya seksi.
  - **Rekrutmen panitia**: buka pendaftaran, lalu **Terima** (masuk sebagai Staf di seksi pilihan) atau **Tolak** pelamar.
  - **Tambah Acara** (rundown), checklist persiapan (bisa dibuat dari template dengan tenggat relatif terhadap tanggal kegiatan), dan **Tambah Anggota Rombongan** (untuk perjalanan, dengan surat izin).
- **Kegiatan › Presensi**: buat jadwal latihan dan sesi, pantau kehadiran, dan cetak rekap. Lihat [Sekretaris](03-sekretaris.md#kegiatan-dan-presensi).
- **Kegiatan › Galeri › Tambah Galeri**: unggah foto, pilih sampul, lalu **Terbitkan** agar tampil di halaman depan.

## Inventaris

**Inventaris › Aset**

- **Tambah Aset**: kategori, nama, merek, jumlah, satuan, lokasi, perolehan, dan tarif sewa / denda. Pilih **Per Unit** untuk alat yang perlu dilacak satu per satu (trumpet, snare); setiap unit mendapat kode (mis. `TRP001`) dan QR.
- **Unit & label QR**: tambah atau hapus unit, lalu cetak label QR (A4, 4 kolom). Pindai label dengan HP untuk membuka detail unit tanpa login.
- **Mutasi stok** (aset cukup jumlah): masuk, keluar, atau penyesuaian.
- Tab Perawatan (**Jadwalkan Perawatan**): **Mulai** (alat tidak bisa dipinjam), lalu **Selesai** dengan kondisi akhir dan biaya.
- Tab Inspeksi (**Catat Inspeksi**): perbarui kondisi alat beserta foto.
- **Cetak Laporan Inventaris**: pilih kategori (atau semua) dan rentang tanggal riwayat perawatan. Isinya ringkasan kondisi; daftar aset per kategori (jumlah, baik, rusak ringan / berat, hilang, dipinjam, perawatan, lokasi, nilai perolehan); dan riwayat perawatan dengan biayanya. Ekspor ke PDF atau Excel dari toolbar penampil.

**Inventaris › Peminjaman**

1. **Catat Peminjaman** (atau permohonan anggota dari Area Anggota): peminjam (anggota, instansi, atau perorangan), alat, jumlah, tanggal, dan skema biaya.
2. **Setujui** atau **Tolak**.
3. **Serahkan barang**: pindai kode unit untuk alat per unit.
4. Saat kembali, buka **Detail**: catat jumlah kembali dan kondisinya, atau pindai unit di **Kembalikan Unit**. Denda keterlambatan dihitung otomatis; **Bebaskan denda** bila perlu.
5. **Catat pembayaran**, lalu **Cetak invoice** atau **Cetak kuitansi**.

Peminjaman yang lewat tanggal kembali otomatis ditandai Terlambat, dan peminjam anggota mendapat notifikasi.

## Prestasi dan poin

**Prestasi › Lomba & Prestasi**

- **Tambah Lomba**, daftarkan **Peserta**, dan ubah status lomba sesuai tahapnya.
- **Catat Prestasi**: anggota atau tim, tingkat, dan berkas sertifikat / foto. Poin prestasi diberikan otomatis sesuai aturan poin.
- **Tambah Materi**: materi latihan atau lomba (berkas, tautan, video).
- Tab **Poin**:
  - **Generate dari Kegiatan**: poin hadir dan panitia untuk satu kegiatan.
  - **Generate Poin Pengurus**: poin jabatan untuk satu periode.
  - **Tambah Poin Manual**: untuk pemateri atau kontribusi lain.
  - **Peringkat**: papan peringkat per periode.
  - **Cetak Rekap Poin**: pilih periode (bawaan periode aktif) dan section (atau semua). Hasilnya tabel per anggota dengan peringkat dan poin per sumber (hadir, panitia, pengurus, prestasi, lainnya), lalu total. Peringkat dihitung di antara semua anggota periode itu, meski dicetak per section.
- Tab **Aturan**: nilai poin per sumber, jenis kegiatan, dan tingkat prestasi.
