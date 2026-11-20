# Panduan Bendahara

Peran **BENDAHARA** membuka: Keuangan (Transaksi, Anggaran, Pengajuan Dana, Iuran, Hutang), Pengadaan, Merchandise, dan Sponsorship.

Kembali ke [daftar panduan](README.md).

## Prinsip

- Transaksi berstatus **Draft** belum dihitung di saldo. **Posting** membuat transaksi dihitung; **Batalkan** (void) membatalkannya tanpa menghapus jejak.
- Hanya transaksi Draft yang bisa diubah atau dihapus.
- Transaksi yang dibuat oleh dokumen lain (iuran, pengajuan dana, pengadaan, penjualan, peminjaman, perawatan, hutang) hanya bisa dibatalkan dari dokumen asalnya.
- Semua laporan keuangan hanya menghitung transaksi **Posted**.

## Persiapan awal periode

1. **Keuangan › Transaksi**, tab **Akun Kas**: daftarkan kas tunai, rekening bank, dan e-wallet beserta saldo awal. Saldo awal terkunci setelah akun punya transaksi.
2. Tab **Kategori**: kategori pemasukan dan pengeluaran.
3. Tab **Saldo Periode**: pilih periode, lalu **Buka** saldo. Saldo awal diambil dari saldo akhir periode sebelumnya.

## Transaksi harian

- **Tambah Transaksi**: jenis (masuk / keluar), akun, kategori, tanggal, jumlah, keterangan, dan item anggaran bila ada. Simpan sebagai Draft, lalu **Posting**.
- Lampirkan bukti (nota, kuitansi) di tiap transaksi.
- Akhir periode: di tab **Saldo Periode**, klik **Tutup saldo**. Penutupan ditolak selama masih ada transaksi Draft, dan transaksi periode itu terkunci setelahnya. **Buka lagi** hanya bisa selama periode berikutnya belum punya saldo.

## Laporan keuangan

**Keuangan › Transaksi › Cetak Laporan Keuangan** membuka pilihan laporan:

| Laporan | Isi | Pilihan |
|---|---|---|
| Buku kas | Per akun: saldo awal, setiap transaksi dengan saldo berjalan, saldo akhir | Rentang tanggal, akun (atau semua) |
| Arus kas | Pemasukan dan pengeluaran per kategori, surplus / defisit, tabel per bulan | Rentang tanggal, akun |
| Realisasi anggaran | RAB vs realisasi per item, sisa dan persentase, pengeluaran di luar anggaran | Anggaran |
| Rekap iuran | Per periode tagihan dan per anggota, termasuk tunggakan | Pengaturan iuran, section |

Klik **Cetak** untuk membuka penampil laporan, atau **Unduh Excel** untuk langsung mengunduh berkas `.xlsx`. Laporan ditandatangani Bendahara dan Ketua Umum periode aktif. Rentang tanggal maksimal 3 tahun.

## Anggaran (RAB)

**Keuangan › Anggaran › Tambah Anggaran**: pilih periode, lalu tambahkan item (jumlah × harga satuan). Total dihitung otomatis. **Ajukan** untuk disetujui pimpinan; anggaran yang ditolak bisa diperbaiki dan diajukan lagi. Realisasi per item terlihat di daftar item dan di laporan realisasi.

## Pengajuan dana

**Keuangan › Pengajuan Dana › Buat Pengajuan**: panitia atau divisi mengajukan dana untuk kegiatan atau item anggaran. Setelah disetujui sesuai alur, **Cairkan** (membuat transaksi keluar). Pemohon mencatat realisasi dan bukti, lalu mengajukan pertanggungjawaban. Saat **diselesaikan**, sisa dana dicatat sebagai pemasukan atau kelebihan pengeluaran sebagai pengeluaran.

## Iuran

1. Tab **Pengaturan Iuran** (**Tambah Iuran**): nama, nominal, frekuensi (bulanan, semester, tahunan, sekali), dan kategori pemasukan.
2. Tab **Tagihan** (**Buat Tagihan**): pilih pengaturan iuran, periode tagihan, dan jatuh tempo. Tagihan dibuat untuk semua anggota aktif yang belum punya tagihan periode itu, dan anggota mendapat notifikasi.
3. **Catat bayar**: pilih akun penerima. Transaksi pemasukan dibuat otomatis. **Batal bayar** bila salah catat.
4. **Bebaskan** tagihan untuk anggota yang dibebaskan dari iuran (**Batal bebas** untuk mengembalikan).

## Hutang

**Keuangan › Hutang › Catat Hutang**: pemberi pinjaman, jumlah, dan jatuh tempo. Pinjaman tunai bisa langsung dicatat sebagai pemasukan. **Catat pembayaran** cicilan (membuat pengeluaran); status menjadi Sebagian lalu Lunas. **Hapuskan** untuk hutang yang diputihkan.

## Pengadaan

**Pengadaan › Buat Pengadaan**: daftar barang, penawaran vendor, dan item anggaran. Setelah disetujui, **Beli / bayar** (bayar langsung atau melalui pengajuan dana), lalu catat penerimaan barang. Barang berjenis aset otomatis masuk ke Inventaris.

## Merchandise

**Merchandise › Tambah Produk**: harga, varian, stok, atau pre-order. **Catat Pesanan** untuk pembeli langsung; pesanan dari Toko anggota dan toko publik (**/toko**) masuk otomatis. **Catat pembayaran**, lalu tandai **Siap diambil** dan **Selesai**. Stok berkurang otomatis saat pesanan dibuat dan kembali bila dibatalkan.

## Sponsorship

**Kemitraan › Sponsorship**: **Tambah Paket** (harga dan benefit), **Catat Sponsorship** untuk mitra, **Catat dana masuk** untuk setiap pembayaran, centang benefit yang sudah dipenuhi (dengan bukti), dan unggah MoU.
