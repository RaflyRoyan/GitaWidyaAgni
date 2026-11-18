-- Label QR unit aset berisi token acak (bukan kode unit), jadi URL publik /aset/{token} tidak bisa ditebak dari kode inventaris.
ALTER TABLE m_asset_unit
  ADD COLUMN qr_token CHAR(32) NULL COMMENT 'Token acak 128-bit (hex huruf besar) untuk URL QR /aset/{token}; label lama dengan kode unit tidak berlaku lagi' AFTER unit_code;

UPDATE m_asset_unit SET qr_token = UPPER(HEX(RANDOM_BYTES(16))) WHERE qr_token IS NULL;

ALTER TABLE m_asset_unit
  MODIFY COLUMN qr_token CHAR(32) NOT NULL COMMENT 'Token acak 128-bit (hex huruf besar) untuk URL QR /aset/{token}; label lama dengan kode unit tidak berlaku lagi',
  ADD UNIQUE KEY uq_asset_unit_qr_token (qr_token);
