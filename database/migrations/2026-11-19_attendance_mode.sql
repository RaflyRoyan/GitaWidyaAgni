-- Mode presensi: 1 = Tanpa QR (anggota check-in langsung dari daftar sesi hari ini, dengan lokasi + selfie),
-- 2 = Wajib scan QR (mis. rapat). Sesi dibuka otomatis 30 menit sebelum mulai dan ditutup otomatis setelah selesai.
-- Data lama: jadwal / sesi yang sudah ada tetap memakai QR seperti sebelumnya.
ALTER TABLE t_activity_schedule
  ADD COLUMN attendance_mode INT NOT NULL DEFAULT 2 COMMENT '1=TANPA_QR, 2=QR' AFTER late_tolerance_minute;

ALTER TABLE t_attendance_session
  ADD COLUMN attendance_mode INT NOT NULL DEFAULT 2 COMMENT '1=TANPA_QR, 2=QR' AFTER late_tolerance_minute;
