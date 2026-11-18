-- Section musik (Brass, Battery, Front Ensemble, Colour Guard) terpisah dari divisi (Humas, Logistik, Danus, ...).
-- Setiap anggota punya satu section aktif; riwayat perpindahan section disimpan di t_section_member.
CREATE TABLE m_section (
  id INT NOT NULL,
  name VARCHAR(100) NOT NULL COMMENT 'mis. Brass, Battery, Front Ensemble, Colour Guard',
  description VARCHAR(500) NULL,
  sort_order INT NOT NULL DEFAULT 0,
  is_active INT NOT NULL DEFAULT 1 COMMENT '0=Tidak, 1=Ya',
  ins_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  ins_by INT NULL,
  upd_date DATETIME NULL,
  upd_by INT NULL,
  PRIMARY KEY (id),
  UNIQUE KEY uq_section_name (name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Section musik marching band; semua anggota punya section';

CREATE TABLE t_section_member (
  id INT NOT NULL,
  section_id INT NOT NULL,
  member_id INT NOT NULL,
  start_date DATE NOT NULL COMMENT 'Mulai di section ini',
  end_date DATE NULL COMMENT 'NULL = section aktif anggota saat ini',
  note VARCHAR(255) NULL COMMENT 'Alasan / event perpindahan',
  ins_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  ins_by INT NULL,
  upd_date DATETIME NULL,
  upd_by INT NULL,
  PRIMARY KEY (id),
  KEY fk_secmember_section (section_id),
  KEY idx_secmember_member (member_id, end_date),
  CONSTRAINT fk_secmember_section FOREIGN KEY (section_id) REFERENCES m_section (id),
  CONSTRAINT fk_secmember_member FOREIGN KEY (member_id) REFERENCES m_member (id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Riwayat section anggota';

ALTER TABLE t_activity
  ADD COLUMN section_id INT NULL COMMENT 'Wajib bila audience_type = SECTION' AFTER division_id,
  ADD KEY fk_activity_section (section_id),
  ADD CONSTRAINT fk_activity_section FOREIGN KEY (section_id) REFERENCES m_section (id),
  MODIFY COLUMN audience_type INT NOT NULL DEFAULT 1 COMMENT '1=SEMUA_ANGGOTA, 2=PENGURUS, 3=PESERTA_TERDAFTAR (t_activity_participant), 4=DIVISI (t_division_member), 5=TIM_PELAKSANA (t_committee_member kegiatan induk), 6=SECTION (t_section_member) | penentu siapa yang dicatat ALPA';

ALTER TABLE t_management_assignment
  ADD COLUMN section_id INT NULL COMMENT 'Section yang dipimpin (Section Leader)' AFTER division_id,
  ADD KEY fk_assignment_section (section_id),
  ADD CONSTRAINT fk_assignment_section FOREIGN KEY (section_id) REFERENCES m_section (id);
