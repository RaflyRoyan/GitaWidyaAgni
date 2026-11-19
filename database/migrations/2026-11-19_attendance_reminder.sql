-- Pengingat presensi (job SYNC_ATTENDANCE_SESSIONS): waktu pengingat check-in (saat check-in dibuka, 30 menit sebelum mulai)
-- dan pengingat check-out (15 menit sebelum selesai) dikirim, supaya tidak terkirim dua kali.
ALTER TABLE t_attendance_session
  ADD COLUMN reminder_sent_at DATETIME NULL COMMENT 'Pengingat check-in terkirim (in-app + WhatsApp bila ada template ATTENDANCE_REMINDER)' AFTER cancel_reason,
  ADD COLUMN checkout_reminder_sent_at DATETIME NULL COMMENT 'Pengingat check-out terkirim (template ATTENDANCE_CHECKOUT)' AFTER reminder_sent_at;
