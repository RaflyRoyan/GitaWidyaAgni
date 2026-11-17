-- LPJ (laporan pertanggungjawaban) kegiatan & kepengurusan + bidang / jangka program kerja.
ALTER TABLE t_program
  ADD COLUMN division_id INT NULL COMMENT 'Bidang / divisi pelaksana program' AFTER pic_member_id,
  ADD COLUMN term INT NULL COMMENT '1=JANGKA_PENDEK, 2=JANGKA_PANJANG' AFTER division_id,
  ADD KEY fk_program_division (division_id),
  ADD CONSTRAINT fk_program_division FOREIGN KEY (division_id) REFERENCES m_division (id);

CREATE TABLE t_lpj (
  id INT NOT NULL,
  lpj_type INT NOT NULL COMMENT '1=KEGIATAN, 2=KEPENGURUSAN',
  activity_id INT NULL COMMENT 'Wajib untuk LPJ kegiatan',
  period_id INT NULL COMMENT 'Wajib untuk LPJ kepengurusan',
  title VARCHAR(200) NOT NULL,
  introduction TEXT NULL COMMENT 'Pendahuluan',
  background TEXT NULL COMMENT 'Latar belakang (kegiatan)',
  activity_kind TEXT NULL COMMENT 'Macam kegiatan (kegiatan)',
  purpose TEXT NULL COMMENT 'Maksud dan tujuan',
  activity_form TEXT NULL COMMENT 'Bentuk kegiatan (kegiatan)',
  vision_mission TEXT NULL COMMENT 'Visi dan misi (kepengurusan)',
  closing TEXT NULL COMMENT 'Penutup',
  status INT NOT NULL DEFAULT 1 COMMENT '1=DRAFT, 2=FINAL',
  document LONGBLOB NULL COMMENT 'PDF final',
  document_name VARCHAR(255) NULL,
  finalized_by INT NULL COMMENT 'User yang memfinalkan',
  finalized_at DATETIME NULL,
  ins_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  ins_by INT NULL,
  upd_date DATETIME NULL,
  upd_by INT NULL,
  PRIMARY KEY (id),
  UNIQUE KEY uq_lpj_activity (activity_id),
  UNIQUE KEY uq_lpj_period (period_id),
  CONSTRAINT fk_lpj_activity FOREIGN KEY (activity_id) REFERENCES t_activity (id),
  CONSTRAINT fk_lpj_period FOREIGN KEY (period_id) REFERENCES m_management_period (id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Laporan pertanggungjawaban kegiatan / kepengurusan';
