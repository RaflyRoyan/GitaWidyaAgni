-- Pengajuan tidak ikut kegiatan. Anggota pada audiens kegiatan (Semua Anggota, Pengurus, Divisi, Section, Tim Pelaksana) otomatis
-- terdaftar; mereka bisa mengajukan tidak ikut satu sesi (session_id) atau seluruh kegiatan (session_id NULL), disetujui pengurus.
CREATE TABLE t_activity_absence_request (
  id INT NOT NULL,
  activity_id INT NOT NULL,
  member_id INT NOT NULL,
  session_id INT NULL COMMENT 'NULL = seluruh kegiatan; diisi = satu sesi presensi',
  reason VARCHAR(500) NOT NULL COMMENT 'Alasan dari anggota',
  status INT NOT NULL DEFAULT 1 COMMENT '1=MENUNGGU, 2=DISETUJUI, 3=DITOLAK, 4=DITARIK',
  review_note VARCHAR(500) NULL COMMENT 'Catatan pengurus (wajib saat ditolak)',
  reviewed_by INT NULL COMMENT 'User yang menyetujui / menolak',
  reviewed_at DATETIME NULL,
  ins_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  ins_by INT NULL,
  upd_date DATETIME NULL,
  upd_by INT NULL,
  PRIMARY KEY (id),
  KEY idx_absence_activity (activity_id, status),
  KEY idx_absence_member (member_id),
  KEY fk_absence_session (session_id),
  KEY fk_absence_reviewer (reviewed_by),
  CONSTRAINT fk_absence_activity FOREIGN KEY (activity_id) REFERENCES t_activity (id),
  CONSTRAINT fk_absence_member FOREIGN KEY (member_id) REFERENCES m_member (id),
  CONSTRAINT fk_absence_session FOREIGN KEY (session_id) REFERENCES t_attendance_session (id),
  CONSTRAINT fk_absence_reviewer FOREIGN KEY (reviewed_by) REFERENCES m_user (id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Pengajuan tidak ikut kegiatan (per sesi / seluruh kegiatan)';
