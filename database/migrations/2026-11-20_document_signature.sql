-- Tanda tangan elektronik (QR) pada laporan cetak: setiap penanda tangan di laporan yang memakai QR mendapat token acak.
-- QR berisi {App:PublicUrl}/ttd/{token}; halaman itu menampilkan dokumen, penanda tangan, dan penerbitnya.
-- Cetak ulang dokumen yang sama (jenis + judul + referensi + penanda tangan + tanggal) memakai token yang sama (fingerprint).
CREATE TABLE t_document_signature (
  id INT NOT NULL COMMENT 'Dari m_sequence',
  token CHAR(32) NOT NULL COMMENT 'Token acak 128-bit (hex huruf besar) untuk QR /ttd/{token}',
  fingerprint CHAR(64) NOT NULL COMMENT 'SHA-256 hex dari jenis|judul|referensi|jabatan|nama|tanggal, untuk memakai ulang token saat dicetak ulang',
  doc_type VARCHAR(40) NOT NULL COMMENT 'Kode jenis dokumen, mis. LAPORAN_KEUANGAN, RAPOR, SURAT',
  doc_title VARCHAR(200) NOT NULL COMMENT 'Judul dokumen saat dicetak',
  doc_ref VARCHAR(255) NULL COMMENT 'Keterangan tambahan: nomor surat, periode, filter laporan',
  signer_role VARCHAR(100) NOT NULL COMMENT 'Jabatan penanda tangan seperti tercetak',
  signer_name VARCHAR(150) NOT NULL COMMENT 'Nama penanda tangan seperti tercetak',
  signer_number VARCHAR(50) NULL COMMENT 'NIA penanda tangan bila ada',
  sign_date DATE NOT NULL COMMENT 'Tanggal tanda tangan pada dokumen',
  print_count INT NOT NULL COMMENT 'Berapa kali dokumen ini dicetak / dibuka',
  last_printed_at DATETIME NOT NULL COMMENT 'Terakhir dicetak / dibuka',
  last_printed_by INT NULL COMMENT 'User yang terakhir mencetak',
  ins_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Pertama diterbitkan',
  ins_by INT NULL COMMENT 'User yang pertama mencetak (penerbit)',
  upd_date DATETIME NULL,
  upd_by INT NULL,
  PRIMARY KEY (id),
  UNIQUE KEY uq_document_signature_token (token),
  UNIQUE KEY uq_document_signature_fingerprint (fingerprint),
  KEY fk_document_signature_ins_by (ins_by),
  KEY fk_document_signature_printed_by (last_printed_by),
  CONSTRAINT fk_document_signature_ins_by FOREIGN KEY (ins_by) REFERENCES m_user (id),
  CONSTRAINT fk_document_signature_printed_by FOREIGN KEY (last_printed_by) REFERENCES m_user (id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Tanda tangan elektronik (QR) pada laporan cetak';
