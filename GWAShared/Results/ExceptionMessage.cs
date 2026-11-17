using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWAShared.Results
{
    public class ExceptionMessage
    {
        public const string DATA_NOT_FOUND = "Data Tidak Ditemukan dalam Sistem";
        public const string DATA_X_NOT_FOUND = "Data {0} Tidak Ditemukan dalam Sistem";
        public const string DATA_INVALID = "Data Tidak Valid";
        public const string DATA_DUPLICATE = "Data dengan {0} ini sudah pernah ada";

        public const string MODEL_STRING_LENGHT_NOTVALID = "Jumlah Karakter \"{0}\" harus kurang dari {1}";
        public const string FIELD_REQUIRED = "{0} Harus diisi";
        public const string FIELD_INVALID = "{0} tidak valid";
        public const string FIELD_JSON_INVALID = "{0} harus berupa JSON yang valid";
        public const string OBJECT_NOT_ACTIVE = "{0} sudah tidak aktif dalam sistem";
        public const string DATE_RANGE_INVALID = "Tanggal selesai tidak boleh sebelum tanggal mulai";
        public const string STATUS_ACTION_INVALID = "Tidak dapat {0} karena status {1}";

        public const string CLASS_CONSTRUCTOR_NOT_FOUND = "Constructor \"internal {0}({1} row)\" tidak ditemukan dalam class {2}";
        public const string CLASS_CONSTRUCTOR2_NOT_FOUND = "Constructor \"public {0}({1} encryptedKey)\" tidak ditemukan dalam class {2}";

        public const string FILE_TOO_LARGE = "Ukuran {0} maksimal {1} MB";
        public const string FILE_TYPE_INVALID = "{0} harus berupa file {1}";
        public const string IMAGE_INVALID = "File bukan gambar yang valid (gunakan JPG, PNG atau WEBP)";
        public const string IMAGE_PROCESS_FAILED = "Foto gagal diproses";

        // Menu
        public const string CONTROLLER_NAMA_REQUIRED = "Nama Controller harus diisi";
        public const string CONTROLLER_DUPLICATE = "Controller sudah terdaftar dalam sistem";
        public const string CONTROLLER_NOT_ACTIVE = "Controller sudah tidak aktif dalam sistem";
        public const string CONTROLLER_NOT_FOUND = "Controller tidak ditemukan dalam sistem";

        public const string CONTROLLER_ACTION_REQUIRED = "Controller Action harus ada minimal 1";
        public const string CONTROLLER_ACTION_INDEX_REQUIRED = "Daftar Action harus memuat Index Action controller ({0})";
        public const string CONTROLLER_ACTION_INDEX_LOCKED = "Index Action controller tidak dapat diubah namanya atau dinonaktifkan";
        public const string CONTROLLER_ACTION_DUPLICATE = "Controller Action tidak boleh ada yang sama dalam 1 controller";
        public const string CONTROLLER_ACTION_NAMA_REQUIRED = "Nama Controller Action harus diisi";
        public const string CONTROLLER_ACTION_NOT_FOUND = "Controller Action tidak ditemukan dalam sistem";

        public const string HEADERMENU_NAMA_REQUIRED = "Nama Header Menu harus diisi";
        public const string HEADERMENU_NOT_ACTIVE = "Header Menu sudah tidak aktif dalam sistem";
        public const string HEADERMENU_NOT_FOUND = "Header Menu tidak ditemukan dalam sistem";
        public const string HEADERMENU_SEQ_INVALID = "Urutan menu harus lebih besar dari 0";
        public const string HEADERMENU_DETAIL_DUPLICATE = "Sub Menu tidak boleh ada yang sama dalam 1 header menu";
        public const string HEADERMENU_DETAIL_NOT_FOUND = "Header Menu Detail tidak ditemukan dalam sistem";
        public const string HEADERMENU_DETAIL_NO_DEL_ROLE_MENU = "Tidak dapat menghapus Sub Menu yang dipakai pada Role Menu";

        public const string SUBMENU_NAMA_REQUIRED = "Nama Sub Menu harus diisi";
        public const string SUBMENU_NOT_ACTIVE = "Sub Menu sudah tidak aktif dalam sistem";
        public const string SUBMENU_NOT_FOUND = "Sub Menu tidak ditemukan dalam sistem";

        // Pengguna & Hak Akses
        public const string SEQUENCE_TABLE_INVALID = "Tabel \"{0}\" tidak ada dalam database";
        public const string SEQUENCE_LAST_ID_INVALID = "Last Id tidak boleh kurang dari 0";
        public const string SEQUENCE_LAST_ID_BELOW_MAX = "Last Id tabel {0} tidak boleh lebih kecil dari Id terbesar yang sudah ada ({1})";
        public const string TOKEN_INVALID = "Token tidak valid atau sudah kedaluwarsa";
        public const string ROLE_MENU_SUBMENU_NOT_IN_HEADER = "Sub Menu tidak terdaftar pada Header Menu yang dipilih";

        // Profil Organisasi
        public const string ORGANIZATION_CODE_INVALID = "Kode organisasi hanya boleh huruf dan angka tanpa spasi";
        public const string ORGANIZATION_NOT_SET = "Profil organisasi belum diisi";
        public const string ORGANIZATION_EXISTS = "Profil organisasi sudah ada; ubah profil yang sudah ada";
        public const string ADVISOR_FILE_NAME_REQUIRED = "Nama file SK harus diisi bila file SK diunggah";

        // Anggota & Kepengurusan
        public const string PERIOD_DATE_INVALID = "Tanggal selesai periode tidak boleh sebelum tanggal mulai";
        public const string PERIOD_ACTIVE_EXISTS = "Sudah ada periode kepengurusan aktif";
        public const string PERIOD_CLOSED = "Periode kepengurusan sudah ditutup";
        public const string PERIOD_DELETE_NOT_DRAFT = "Hanya periode kepengurusan berstatus Draft yang dapat dihapus";
        public const string PERIOD_IN_USE = "Periode kepengurusan sudah dipakai data lain sehingga tidak dapat dihapus";
        public const string USER_MEMBER_USED = "Akun user ini sudah dipakai oleh anggota lain";
        public const string MEMBER_LEFT_DATE_INVALID = "Tanggal keluar tidak boleh sebelum tanggal bergabung";
        public const string MEMBER_NOT_ALUMNI = "Profil alumni hanya untuk anggota berstatus Alumni";
        public const string POSITION_HOLDER_EXISTS = "Jabatan {0} pada periode ini sudah dipegang anggota lain";
        public const string ASSIGNMENT_HAS_CERTIFICATE = "Penugasan sudah dipakai sertifikat; akhiri penugasan alih-alih menghapus";
        public const string RECRUITMENT_NOT_OPEN = "Pendaftaran rekrutmen sedang tidak dibuka";
        public const string RECRUITMENT_CLOSE_DATE_PASSED = "Tanggal tutup rekrutmen sudah lewat; perbarui tanggal tutup sebelum membuka";
        public const string RECRUITMENT_IN_USE = "Rekrutmen sudah punya pendaftar atau formulir sehingga tidak dapat dihapus";
        public const string RECRUITMENT_QUOTA_BELOW_ACCEPTED = "Kuota tidak boleh lebih kecil dari jumlah pendaftar yang sudah diterima";
        public const string RECRUITMENT_QUOTA_FULL = "Kuota rekrutmen sudah penuh";
        public const string RECRUITMENT_STUDENT_ONLY = "Rekrutmen ini hanya untuk mahasiswa";
        public const string RECRUITMENT_ALREADY_APPLIED = "NIM / email ini sudah terdaftar pada rekrutmen ini";
        public const string HANDOVER_SAME_PERIOD = "Periode asal dan tujuan serah terima tidak boleh sama";
        public const string HANDOVER_EXISTS = "Periode asal ini sudah punya berita acara serah terima";

        // Program & Kegiatan
        public const string PROGRAM_PREVIOUS_SAME_PERIOD = "Program sebelumnya harus berasal dari periode lain";
        public const string FEATURED_NOT_PUBLIC = "Hanya data publik yang bisa disorot di landing page";
        public const string ACTIVITY_PARENT_INVALID = "Kegiatan induk tidak valid (maksimal satu tingkat dan tidak boleh kegiatan itu sendiri)";
        public const string ACTIVITY_COMMITTEE_NEEDS_PARENT = "Audiens Tim Pelaksana membutuhkan kegiatan induk";
        public const string ACTIVITY_IN_USE = "Kegiatan sudah punya data turunan sehingga tidak dapat dihapus; batalkan kegiatan alih-alih menghapus";
        public const string REPORT_SOURCE_REQUIRED = "Laporan harus terkait kegiatan atau program kerja";
        public const string GALLERY_EMPTY = "Galeri yang terbit harus memiliki minimal satu foto";
        public const string PROGRAM_IN_USE = "Program kerja sudah dipakai data lain sehingga tidak dapat dihapus; batalkan program alih-alih menghapus";
        public const string LPJ_ACTIVITY_EXISTS = "LPJ untuk kegiatan ini sudah ada";
        public const string LPJ_PERIOD_EXISTS = "LPJ untuk periode ini sudah ada";
        public const string LPJ_SUB_ACTIVITY = "LPJ dibuat untuk kegiatan induk; sub-kegiatan ikut dilaporkan di dalamnya";
        public const string LPJ_DOCUMENT_REQUIRED = "Dokumen PDF LPJ final harus ada";

        // Tim Pelaksana
        public const string COMMITTEE_SECTION_MISMATCH = "Sie bukan milik kegiatan ini";
        public const string COMMITTEE_SECTION_IN_USE = "Sie sudah dipakai anggota, rundown, checklist atau lamaran sehingga tidak dapat dihapus";
        public const string COMMITTEE_ROLE_SECTION_REQUIRED = "Koordinator sie dan staf wajib memiliki sie";
        public const string COMMITTEE_ROLE_TAKEN = "Peran {0} sudah diisi anggota lain";
        public const string COMMITTEE_ALREADY_MEMBER = "Anggota ini sudah menjadi tim pelaksana kegiatan ini";
        public const string COMMITTEE_ALREADY_APPLIED = "Anggota ini sudah melamar pada pendaftaran tim pelaksana ini";
        public const string TRIP_MEMBER_REQUIRED = "Isi anggota atau nama tamu rombongan";
        public const string CHECKLIST_OFFSET_DAY_INVALID = "Hari relatif checklist harus di antara -{0} dan {0}";

        // Jadwal & Presensi
        public const string ATTENDANCE_SESSION_NOT_OPEN = "Sesi presensi tidak ditemukan atau sedang tidak dibuka";
        public const string ATTENDANCE_SESSION_HAS_CHECKIN = "Sesi yang sudah ada check-in tidak dapat dibatalkan";
        public const string ATTENDANCE_LOCATION_REQUIRED = "Lokasi perangkat wajib dikirim untuk presensi ini";
        public const string ATTENDANCE_OUT_OF_RANGE = "Lokasi Anda {0} m dari titik presensi (maksimal {1} m)";
        public const string ATTENDANCE_NOT_IN_AUDIENCE = "Anda bukan peserta sasaran kegiatan ini";
        public const string ATTENDANCE_ALREADY_CHECKED_IN = "Anda sudah check-in pada sesi ini";
        public const string ATTENDANCE_ALREADY_RECORDED = "Presensi Anda pada sesi ini sudah dicatat pengurus";
        public const string ATTENDANCE_CHECKED_IN_STATUS = "Anggota ini sudah check-in; statusnya hanya bisa Hadir atau Terlambat";
        public const string ATTENDANCE_NOT_CHECKED_IN = "Anda belum check-in pada sesi ini";
        public const string ATTENDANCE_ALREADY_CHECKED_OUT = "Anda sudah check-out pada sesi ini";
        public const string USER_NOT_MEMBER = "Akun ini belum terhubung ke data anggota";
        public const string LOGIN_INVALID = "Username atau password salah";
        public const string PASSWORD_CURRENT_INVALID = "Password lama salah";

        // Keuangan
        public const string FINANCIAL_CATEGORY_IN_USE = "Kategori keuangan masih dipakai pengaturan iuran aktif";
        public const string FINANCIAL_CATEGORY_TYPE_MISMATCH = "Kategori keuangan harus berjenis {0}";
        public const string FINANCIAL_ACCOUNT_BALANCE_LOCKED = "Saldo awal tidak dapat diubah karena akun sudah memiliki transaksi atau saldo periode";
        public const string DUES_SETTING_HAS_BILLS = "Periode dan frekuensi iuran tidak dapat diubah karena tagihan iuran sudah dibuat";
        public const string AMOUNT_MUST_POSITIVE = "{0} harus lebih besar dari 0";
        public const string DATE_BEFORE_INVALID = "{0} tidak boleh sebelum {1}";
        public const string FINANCIAL_BALANCE_CLOSED = "Saldo periode akun {0} sudah ditutup; transaksi pada periode tersebut tidak dapat diubah";
        public const string FINANCIAL_TRANSACTION_LINKED = "Transaksi ini dibuat dari {0}; batalkan lewat dokumen asalnya";
        public const string DUES_BILL_PERIOD_INVALID = "Periode tagihan iuran {0} harus berformat {1}";
        public const string DUES_CATEGORY_REQUIRED = "Kategori pemasukan iuran belum diatur; isi pada pengaturan iuran atau saat pembayaran";
        public const string BUDGET_ITEM_REQUIRED = "Anggaran harus memiliki minimal 1 item sebelum diajukan";
        public const string BUDGET_NOT_APPROVED = "Item anggaran hanya bisa dipakai bila anggarannya sudah disetujui";
        public const string BUDGET_PROGRAM_PERIOD_MISMATCH = "Program kerja harus berasal dari periode anggaran yang sama";
        public const string FUNDING_SETTLEMENT_ACCOUNT_REQUIRED = "Akun dan kategori keuangan wajib diisi karena realisasi berbeda dengan dana yang dicairkan";
        public const string PERIOD_BALANCE_HAS_DRAFT = "Masih ada transaksi Draft pada periode dan akun ini; posting atau batalkan dulu";
        public const string PERIOD_BALANCE_NEXT_EXISTS = "Saldo periode berikutnya untuk akun ini sudah dibuat sehingga saldo ini tidak dapat dibuka kembali";
        public const string DEBT_PAYMENT_EXCEEDS = "Pembayaran melebihi sisa hutang ({0:N2})";
        public const string DEBT_AMOUNT_BELOW_PAID = "Nominal hutang tidak boleh lebih kecil dari yang sudah dibayar ({0:N2})";
        public const string DEBT_LOAN_AMOUNT_LOCKED = "Nominal hutang pinjaman uang tidak dapat diubah karena transaksi uang masuknya sudah dicatat";
        public const string DEBT_HAS_PAYMENTS = "Hutang yang sudah memiliki pembayaran tidak dapat dihapus";

        // Mitra & Pihak Luar
        public const string EXTERNAL_PARTY_BLACKLISTED = "Pihak luar ini masuk daftar hitam (blacklist)";
        public const string EXTERNAL_PARTY_CONTACT_MISMATCH = "Kontak bukan milik pihak luar yang dipilih";

        // Sponsorship
        public const string SPONSORSHIP_IN_KIND_REQUIRED = "Rincian barang / jasa / media wajib diisi untuk sponsorship non-dana";
        public const string SPONSORSHIP_PAYMENT_EXCEEDS = "Dana yang diterima melebihi sisa nilai sponsorship ({0:N2})";
        public const string SPONSORSHIP_HAS_PAYMENT = "Sponsorship yang sudah menerima dana tidak dapat dibatalkan; batalkan transaksinya dulu";
        public const string SPONSORSHIP_BENEFIT_PENDING = "Masih ada {0} benefit yang belum diberikan ke sponsor";
        public const string SPONSORSHIP_IN_USE = "Sponsorship sudah punya transaksi, MoU atau aset sehingga tidak dapat dihapus; batalkan alih-alih menghapus";
        public const string MOU_IN_HANDOVER = "MoU dipakai berita acara serah terima sehingga tidak dapat dihapus";

        // Pengadaan
        public const string PROCUREMENT_TARGET_ONE = "Pengadaan hanya boleh untuk salah satu: kegiatan atau program kerja";
        public const string PROCUREMENT_ITEM_REQUIRED = "Pengadaan harus memiliki minimal 1 item sebelum diajukan";
        public const string PROCUREMENT_ASSET_CATEGORY_REQUIRED = "Kategori aset wajib diisi untuk item yang masuk inventaris";
        public const string PROCUREMENT_FUNDING_REQUIRED = "Pengadaan lewat pengajuan dana wajib dihubungkan ke pengajuan dana yang sudah disetujui";
        public const string PROCUREMENT_FUNDING_USED = "Pengajuan dana ini sudah dipakai pengadaan lain";
        public const string PROCUREMENT_PAID_BY_FUNDING = "Pengadaan lewat pengajuan dana dibayar dari pengajuan dananya, bukan dicatat langsung";
        public const string PROCUREMENT_PAYMENT_VIA_REQUEST = "Pembayaran pengadaan dicatat lewat pengadaan (RecordPayment)";
        public const string PROCUREMENT_VENDOR_REQUIRED = "Isi vendor terdaftar atau nama vendor";
        public const string PROCUREMENT_ITEM_MISMATCH = "Item bukan milik pengadaan ini";
        public const string PROCUREMENT_RECEIVE_EXCEEDS = "Jumlah diterima item {0} melebihi sisa yang belum diterima ({1})";
        public const string PROCUREMENT_RECEIPT_ITEM_REQUIRED = "Penerimaan harus memiliki minimal 1 item";
        public const string PROCUREMENT_ASSET_NOT_ASSET_ITEM = "Aset hanya bisa dibuat dari item yang masuk inventaris dan tidak hilang";
        public const string PROCUREMENT_RECEIPT_HAS_ASSET = "Penerimaan yang item-nya sudah dibuat aset tidak dapat dihapus";
        public const string PROCUREMENT_HAS_PROGRESS = "Pengadaan yang sudah punya pembayaran atau penerimaan tidak dapat dibatalkan";

        // Danus & Merchandise
        public const string PRODUCT_STOCK_USES_VARIANT = "Produk bervarian memakai stok per varian; isi stok awal di masing-masing varian";
        public const string PRODUCT_HAS_OWN_STOCK = "Produk ini masih punya stok sendiri; varian hanya bisa ditambahkan bila stok produk 0";
        public const string PREORDER_CLOSE_INVALID = "Batas pre-order harus setelah waktu sekarang";
        public const string PREORDER_CLOSED = "Pre-order produk {0} sudah ditutup";
        public const string PRODUCT_VARIANT_REQUIRED = "Produk {0} memiliki varian; pilih variannya";
        public const string PRODUCT_NO_VARIANT = "Produk {0} tidak memiliki varian";
        public const string STOCK_NOT_TRACKED = "Stok produk {0} tidak dilacak";
        public const string STOCK_INSUFFICIENT = "Stok {0} tidak cukup (tersisa {1})";
        public const string STOCK_MOVEMENT_AUTO = "Mutasi {0} dicatat otomatis oleh pesanan penjualan";
        public const string STOCK_PROCUREMENT_ONLY_IN = "Pengadaan hanya bisa dicatat pada mutasi Masuk";
        public const string SALES_BUYER_REQUIRED = "Isi pembeli anggota atau nama pembeli";
        public const string SALES_ITEM_REQUIRED = "Pesanan harus memiliki minimal 1 item";
        public const string SALES_PAYMENT_EXCEEDS = "Pembayaran melebihi sisa tagihan pesanan ({0:N2})";
        public const string SALES_HAS_PAYMENT = "Pesanan yang sudah dibayar tidak dapat dibatalkan; batalkan pembayarannya dulu";
        public const string SALES_NOT_PAID = "Pesanan harus lunas sebelum diselesaikan";
        public const string SALES_PAYMENT_VIA_ORDER = "Pembayaran penjualan dicatat lewat pesanan (ReceivePayment)";
        public const string SALES_GUEST_LIMIT = "Pesanan tanpa login maksimal {0} jenis item dan {1} per item";
        public const string SALES_GUEST_NOT_FOUND = "Pesanan tidak ditemukan; periksa nomor pesanan dan nomor WhatsApp";

        // Inventaris
        public const string ASSET_CATEGORY_IN_USE = "Kategori aset masih dipakai aset yang belum dihapuskan";
        public const string ASSET_IN_USE = "Aset sedang dipinjam atau dalam perawatan";
        public const string ASSET_DISPOSED = "Aset sudah dihapuskan";
        public const string ASSET_UNIT_PREFIX_REQUIRED = "Awalan kode unit wajib diisi untuk aset yang dicatat per unit";
        public const string ASSET_UNIT_PREFIX_INVALID = "Awalan kode unit hanya boleh huruf dan angka tanpa spasi";
        public const string ASSET_NOT_PER_UNIT = "Aset ini tidak dicatat per unit";
        public const string ASSET_UNIT_IN_USE = "Unit aset sedang dipinjam atau dalam perawatan";
        public const string ASSET_UNIT_DISPOSED = "Unit aset sudah dihapus";
        public const string ASSET_PER_UNIT_ONLY = "Aset ini dicatat per unit; gunakan unit aset (kode / QR)";
        public const string ASSET_QUANTITY_INSUFFICIENT = "Jumlah {0} yang tersedia hanya {1}";
        public const string ASSET_UNIT_NOT_AVAILABLE = "Unit {0} sedang tidak tersedia";
        public const string ASSET_UNIT_MISMATCH = "Unit {0} bukan milik aset {1}";
        public const string BORROWER_MEMBER_REQUIRED = "Peminjam anggota wajib memilih anggota";
        public const string BORROWER_PARTY_REQUIRED = "Peminjam organisasi / instansi wajib memilih mitra";
        public const string BORROWER_NAME_REQUIRED = "Nama peminjam wajib diisi";
        public const string BORROW_SCHEME_MEMBER_ONLY = "Skema seikhlasnya hanya untuk peminjam anggota";
        public const string BORROW_ITEM_REQUIRED = "Peminjaman harus memiliki minimal 1 aset";
        public const string BORROW_UNIT_COUNT_MISMATCH = "Jumlah unit yang dipindai untuk {0} harus {1}";
        public const string BORROW_CHECKOUT_INCOMPLETE = "Semua aset peminjaman harus diambil sekaligus";
        public const string BORROW_RETURN_EXCEEDS = "Jumlah kembali + hilang untuk {0} melebihi sisa yang dipinjam ({1})";
        public const string BORROW_UNIT_NOT_BORROWED = "Unit {0} tidak sedang dipinjam pada peminjaman ini";
        public const string BORROW_WAIVE_EXCEEDS = "Potongan denda tidak boleh melebihi total denda ({0:N2})";
        public const string BORROW_PAYMENT_EXCEEDS = "Pembayaran melebihi sisa tagihan peminjaman ({0:N2})";
        public const string BORROW_HAS_PAYMENT = "Peminjaman yang sudah dibayar tidak dapat dibatalkan; batalkan pembayarannya dulu";
        public const string BORROW_PAYMENT_VIA_REQUEST = "Pembayaran peminjaman dicatat lewat peminjaman (ReceivePayment)";
        public const string BORROW_RETURN_DATE_INVALID = "Rencana tanggal kembali harus setelah tanggal pinjam";
        public const string MAINTENANCE_ASSET_BORROWED = "Aset masih ada yang dipinjam sehingga tidak bisa dirawat";
        public const string MAINTENANCE_COST_ACCOUNT_REQUIRED = "Akun dan kategori pengeluaran wajib diisi karena perawatan berbiaya";

        // Persuratan & Dokumen
        public const string LETTER_TEMPLATE_EMPTY = "Template surat harus memiliki isi (body) atau file template";
        public const string LETTER_DIRECTION_ONLY = "Aksi ini hanya untuk {0}";
        public const string LETTER_IN_USE = "Surat dipakai peminjaman aset sehingga tidak dapat dihapus";
        public const string LETTER_CONTENT_REQUIRED = "Surat keluar harus memiliki isi (body) atau file sebelum diajukan";

        // Prestasi & Sertifikat
        public const string POINT_RULE_ACTIVE_EXISTS = "Sudah ada aturan poin aktif untuk sumber, jenis kegiatan dan tingkat prestasi ini";
        public const string COMPETITION_IN_USE = "Lomba sudah punya peserta, prestasi, materi, kegiatan atau data keuangan sehingga tidak dapat dihapus; batalkan lomba alih-alih menghapus";
        public const string COMPETITION_REGISTRATION_CLOSED = "Pendaftaran lomba sedang tidak dibuka";
        public const string COMPETITION_ALREADY_REGISTERED = "Anggota ini sudah terdaftar pada lomba ini";
        public const string PARTICIPANT_ROLE_INVALID = "Peran peserta harus KETUA_TIM, ANGGOTA atau PEMBIMBING";
        public const string MATERIAL_FILE_REQUIRED = "Materi berjenis file wajib memiliki file";
        public const string MATERIAL_SOURCE_REQUIRED = "Materi wajib memiliki URL atau file";
        public const string ACHIEVEMENT_OWNER_REQUIRED = "Isi anggota atau nama tim peraih prestasi";
        public const string ACHIEVEMENT_HAS_CERTIFICATE = "Prestasi sudah dipakai sertifikat sehingga tidak dapat dihapus";
        public const string CERTIFICATE_REF_REQUIRED = "Sertifikat {0} wajib menyebut {1}";
        public const string CERTIFICATE_MEMBER_MISMATCH = "{0} bukan milik anggota penerima sertifikat";
        public const string POINT_ZERO = "Poin tidak boleh 0";

        // Formulir & Aspirasi
        public const string FORM_NOT_OPEN = "Formulir tidak sedang menerima jawaban";
        public const string FORM_CLOSE_TIME_PASSED = "Waktu tutup formulir sudah lewat; perbarui waktu tutup sebelum membuka";
        public const string FORM_QUESTION_REQUIRED = "Formulir harus memiliki minimal 1 pertanyaan sebelum dibuka";
        public const string FORM_HAS_RESPONSE = "Formulir sudah memiliki jawaban sehingga pertanyaan dan pengaturannya tidak dapat diubah";
        public const string FORM_REF_REQUIRED = "Formulir {0} wajib menyebut {1}";
        public const string FORM_RECRUITMENT_NOT_ANONYMOUS = "Formulir pendaftaran rekrutmen tidak boleh anonim";
        public const string FORM_IN_USE = "Formulir sudah dipakai rekrutmen tim pelaksana sehingga tidak dapat dihapus";
        public const string FORM_OPTIONS_REQUIRED = "Pertanyaan pilihan wajib memiliki minimal 2 pilihan yang berbeda";
        public const string FORM_ALREADY_RESPONDED = "Formulir ini sudah pernah diisi";
        public const string FORM_ANSWER_REQUIRED = "Pertanyaan \"{0}\" wajib dijawab";
        public const string FORM_ANSWER_INVALID = "Jawaban pertanyaan \"{0}\" tidak valid";
        public const string FORM_ANSWER_DUPLICATE = "Pertanyaan \"{0}\" dijawab lebih dari sekali";
        public const string FORM_QUESTION_NOT_IN_FORM = "Pertanyaan tidak termasuk formulir ini";
        public const string FORM_APPLICANT_MISMATCH = "Data pendaftar tidak cocok dengan rekrutmen formulir ini";
        public const string FORM_RESPONSE_IN_USE = "Jawaban formulir dipakai lamaran tim pelaksana sehingga tidak dapat dihapus";
        public const string FORM_RESPONSE_MISMATCH = "Jawaban formulir bukan milik anggota ini atau bukan dari formulir rekrutmen ini";
        public const string ASPIRATION_RESPONSE_REQUIRED = "Tanggapan wajib diisi untuk menyelesaikan atau menolak aspirasi";
        public const string ASPIRATION_NOT_OWNER = "Hanya pengirim yang dapat mengubah aspirasi ini, dan aspirasi anonim tidak dapat diubah";

        // E-Voting
        public const string ELECTION_KEY_NOT_SET = "Kunci enkripsi e-voting belum dikonfigurasi";
        public const string ELECTION_LOCKED = "Data pemilihan tidak dapat diubah karena status pemilihan {0}";
        public const string ELECTION_END_PASSED = "Waktu selesai pemilihan sudah lewat; perbarui jadwal sebelum membuka pemungutan suara";
        public const string ELECTION_CANDIDATE_REQUIRED = "Pemilihan harus memiliki minimal 1 kandidat terverifikasi";
        public const string ELECTION_VOTER_REQUIRED = "Pemilihan harus memiliki minimal 1 pemilih yang berhak";
        public const string ELECTION_PENDING_CANDIDATE = "Masih ada kandidat yang belum diverifikasi atau ditolak";
        public const string ELECTION_NOT_OPEN = "Pemungutan suara sedang tidak dibuka";
        public const string ELECTION_NOT_ELIGIBLE = "Anda tidak terdaftar sebagai pemilih yang berhak pada pemilihan ini";
        public const string ELECTION_ALREADY_VOTED = "Anda sudah memberikan suara pada pemilihan ini";
        public const string ELECTION_CANDIDATE_INVALID = "Kandidat tidak valid untuk pemilihan ini";
        public const string ELECTION_ALREADY_CANDIDATE = "Anggota sudah terdaftar sebagai kandidat pemilihan ini";
        public const string ELECTION_CANDIDATE_NUMBER_EXISTS = "Nomor urut kandidat sudah dipakai";
        public const string ELECTION_VOTER_HAS_VOTED = "Pemilih sudah memberikan suara sehingga tidak dapat diubah atau dihapus";
        public const string ELECTION_TALLY_MISMATCH = "Jumlah suara ({0}) tidak sama dengan jumlah pemilih yang sudah memilih ({1})";
        public const string ELECTION_VOTE_CORRUPT = "Ada suara yang tidak dapat didekripsi atau tidak valid; periksa kunci enkripsi";

        // Informasi & Log
        public const string ANNOUNCEMENT_SLUG_EXISTS = "Slug {0} sudah dipakai berita / pengumuman lain";
        public const string BANNER_LINK_REQUIRED = "Teks tombol banner membutuhkan link";
        public const string URL_INVALID = "{0} harus berupa URL http(s) atau path yang diawali /";
        public const string NOTIFICATION_NOT_OWNER = "Notifikasi ini bukan milik Anda";
        public const string NOTIFICATION_RECIPIENT_REQUIRED = "Pilih minimal 1 penerima notifikasi";

        // Notifikasi, Penomoran & Persetujuan
        public const string NUMBER_FORMAT_SEQ_REQUIRED = "Format nomor harus memuat placeholder {{seq}}";
        public const string NUMBER_FORMAT_PLACEHOLDER_INVALID = "Placeholder {0} tidak dikenal. Gunakan: {1}";
        public const string NUMBER_FORMAT_NOT_SET = "Format nomor untuk jenis dokumen {0} belum diatur";
        public const string DOCUMENT_SEQ_DECREASE = "Nomor terakhir tidak boleh lebih kecil dari {0} (nomor yang sudah terbit bisa terpakai ulang)";

        public const string APPROVAL_FLOW_ACTIVE_EXISTS = "Sudah ada alur persetujuan aktif untuk jenis dokumen ini";
        public const string APPROVAL_FLOW_STEP_REQUIRED = "Alur persetujuan aktif harus memiliki minimal 1 langkah";
        public const string APPROVAL_STEP_LEVEL_INVALID = "Urutan langkah persetujuan harus lebih besar dari 0";
        public const string APPROVAL_STEP_IN_USE = "Langkah persetujuan sudah dipakai dokumen sehingga tidak dapat dihapus";
        public const string APPROVAL_FLOW_NOT_SET = "Alur persetujuan aktif untuk jenis dokumen {0} belum diatur";
        public const string APPROVAL_NOT_APPROVER = "Anda bukan penyetuju langkah {0}";
        public const string APPROVAL_REVISION_NOT_ALLOWED = "Langkah {0} tidak boleh mengembalikan dokumen untuk revisi";

        public const string JOB_ALREADY_RUNNING = "Job {0} masih berjalan sejak {1:dd-MM-yyyy HH:mm}";
        public const string OUTBOX_CHANNEL_INVALID = "Antrian pesan hanya untuk saluran Email atau WhatsApp";
        public const string OUTBOX_EMAIL_INVALID = "Alamat email penerima tidak valid";
        public const string OUTBOX_WHATSAPP_INVALID = "Nomor WhatsApp penerima tidak valid";
        public const string MESSAGE_TEMPLATE_NOT_FOUND = "Template pesan {0} untuk saluran {1} tidak ditemukan atau tidak aktif";
    }
}
