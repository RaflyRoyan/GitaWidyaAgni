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
        public const string ROLE_MENU_SUBMENU_NOT_IN_HEADER = "Sub Menu tidak terdaftar pada Header Menu yang dipilih";

        // Profil Organisasi
        public const string ADVISOR_FILE_NAME_REQUIRED = "Nama file SK harus diisi bila file SK diunggah";

        // Anggota & Kepengurusan
        public const string PERIOD_DATE_INVALID = "Tanggal selesai periode tidak boleh sebelum tanggal mulai";
        public const string PERIOD_ACTIVE_EXISTS = "Sudah ada periode kepengurusan aktif";
        public const string PERIOD_CLOSED = "Periode kepengurusan sudah ditutup";
        public const string PERIOD_DELETE_NOT_DRAFT = "Hanya periode kepengurusan berstatus Draft yang dapat dihapus";
        public const string PERIOD_IN_USE = "Periode kepengurusan sudah dipakai data lain sehingga tidak dapat dihapus";
        public const string USER_MEMBER_USED = "Akun user ini sudah dipakai oleh anggota lain";
        public const string MEMBER_NOT_ALUMNI = "Profil alumni hanya untuk anggota berstatus Alumni";

        // Tim Pelaksana
        public const string CHECKLIST_OFFSET_DAY_INVALID = "Hari relatif checklist harus di antara -{0} dan {0}";

        // Keuangan
        public const string FINANCIAL_CATEGORY_IN_USE = "Kategori keuangan masih dipakai pengaturan iuran aktif";
        public const string FINANCIAL_CATEGORY_TYPE_MISMATCH = "Kategori keuangan harus berjenis {0}";
        public const string FINANCIAL_ACCOUNT_BALANCE_LOCKED = "Saldo awal tidak dapat diubah karena akun sudah memiliki transaksi atau saldo periode";
        public const string DUES_SETTING_HAS_BILLS = "Periode dan frekuensi iuran tidak dapat diubah karena tagihan iuran sudah dibuat";

        // Mitra & Pihak Luar
        public const string EXTERNAL_PARTY_BLACKLISTED = "Pihak luar ini masuk daftar hitam (blacklist)";
        public const string EXTERNAL_PARTY_CONTACT_MISMATCH = "Kontak bukan milik pihak luar yang dipilih";

        // Danus & Merchandise
        public const string PRODUCT_STOCK_USES_VARIANT = "Produk bervarian memakai stok per varian; isi stok awal di masing-masing varian";
        public const string PRODUCT_HAS_OWN_STOCK = "Produk ini masih punya stok sendiri; varian hanya bisa ditambahkan bila stok produk 0";
        public const string PREORDER_CLOSE_INVALID = "Batas pre-order harus setelah waktu sekarang";

        // Inventaris
        public const string ASSET_CATEGORY_IN_USE = "Kategori aset masih dipakai aset yang belum dihapuskan";
        public const string ASSET_IN_USE = "Aset sedang dipinjam atau dalam perawatan";
        public const string ASSET_DISPOSED = "Aset sudah dihapuskan";

        // Persuratan & Dokumen
        public const string LETTER_TEMPLATE_EMPTY = "Template surat harus memiliki isi (body) atau file template";

        // Notifikasi, Penomoran & Persetujuan
        public const string NUMBER_FORMAT_SEQ_REQUIRED = "Format nomor harus memuat placeholder {{seq}}";
        public const string NUMBER_FORMAT_PLACEHOLDER_INVALID = "Placeholder {0} tidak dikenal. Gunakan: {1}";
        public const string NUMBER_FORMAT_NOT_SET = "Format nomor untuk jenis dokumen {0} belum diatur";
        public const string DOCUMENT_SEQ_DECREASE = "Nomor terakhir tidak boleh lebih kecil dari {0} (nomor yang sudah terbit bisa terpakai ulang)";

        public const string APPROVAL_FLOW_ACTIVE_EXISTS = "Sudah ada alur persetujuan aktif untuk jenis dokumen ini";
        public const string APPROVAL_FLOW_STEP_REQUIRED = "Alur persetujuan aktif harus memiliki minimal 1 langkah";
        public const string APPROVAL_STEP_LEVEL_INVALID = "Urutan langkah persetujuan harus lebih besar dari 0";
        public const string APPROVAL_STEP_IN_USE = "Langkah persetujuan sudah dipakai dokumen sehingga tidak dapat dihapus";

        public const string JOB_ALREADY_RUNNING = "Job {0} masih berjalan sejak {1:dd-MM-yyyy HH:mm}";
        public const string OUTBOX_CHANNEL_INVALID = "Antrian pesan hanya untuk saluran Email atau WhatsApp";
        public const string OUTBOX_EMAIL_INVALID = "Alamat email penerima tidak valid";
        public const string OUTBOX_WHATSAPP_INVALID = "Nomor WhatsApp penerima tidak valid";
        public const string MESSAGE_TEMPLATE_NOT_FOUND = "Template pesan {0} untuk saluran {1} tidak ditemukan atau tidak aktif";
    }
}
