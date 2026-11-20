-- Kartu Tanda Anggota (KTA) digital: QR berisi token acak (bukan NIA), dibuat saat KTA pertama kali dicetak / diunduh.
-- Mengganti token (kartu hilang) membuat QR kartu lama tidak berlaku lagi di /kta/{token}.
ALTER TABLE m_member
  ADD COLUMN card_token CHAR(32) NULL COMMENT 'Token acak 128-bit (hex huruf besar) untuk QR KTA /kta/{token}; NULL = KTA belum pernah diterbitkan' AFTER address,
  ADD COLUMN card_issued_at DATETIME NULL COMMENT 'Waktu token KTA dibuat / diganti' AFTER card_token,
  ADD UNIQUE KEY uq_member_card_token (card_token);
