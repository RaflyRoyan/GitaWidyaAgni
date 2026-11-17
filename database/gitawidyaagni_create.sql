-- =====================================================================================
-- GitaWidyaAgni: skema database lengkap (MySQL 8, utf8mb4_0900_ai_ci)
-- Dihasilkan dari database pengembangan pada 2026-11-17 (mysqldump --no-data), 132 tabel,
-- sudah termasuk migrasi 2026-11-17 (t_lpj, t_program.division_id / term).
--
-- Pakai untuk database BARU. Database lama cukup menjalankan berkas di database/migrations/
-- yang tanggalnya lebih baru dari skemanya.
--
--   mysql -u <user> -p < database/gitawidyaagni_create.sql
--
-- Tidak ada AUTO_INCREMENT: id diambil dari m_sequence oleh aplikasi (baris dibuat otomatis),
-- kecuali t_election_vote.id yang acak. Arti setiap kolom kode int ada di COMMENT kolomnya.
-- Isi data contoh + akun admin: dotnet run --project GWASeeder  (lihat README.md).
-- =====================================================================================

CREATE DATABASE IF NOT EXISTS `gitawidyaagni` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;
USE `gitawidyaagni`;


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_alumni_profile` (
  `id` int NOT NULL,
  `member_id` int NOT NULL,
  `graduation_year` smallint DEFAULT NULL,
  `current_job` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `company` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `industry` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `city` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `linkedin_url` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `is_contactable` int NOT NULL COMMENT '0=Tidak, 1=Ya | alumni setuju dihubungi pengurus / adik tingkat',
  `notes` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_alumni_member` (`member_id`),
  CONSTRAINT `fk_alumni_member` FOREIGN KEY (`member_id`) REFERENCES `m_member` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_approval_flow` (
  `id` int NOT NULL,
  `document_type` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'FUNDING_REQUEST, PROCUREMENT, LETTER, ACTIVITY_REPORT, BUDGET, BORROW_REQUEST, PERIOD_HANDOVER',
  `name` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `description` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `is_active` int NOT NULL DEFAULT '1' COMMENT '0=Tidak, 1=Ya',
  `active_key` int GENERATED ALWAYS AS (if((`is_active` = 1),1,NULL)) STORED COMMENT 'Kolom bantu: satu alur aktif per jenis dokumen',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_approval_flow_active` (`document_type`,`active_key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_approval_flow_step` (
  `id` int NOT NULL,
  `flow_id` int NOT NULL,
  `step_level` int NOT NULL COMMENT 'Urutan, mulai 1',
  `step_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'mis. Bendahara, Ketua Umum',
  `approver_type` int NOT NULL COMMENT '1=ROLE (role pengurus), 2=JABATAN (position_id), 3=USER_TERTENTU (user_id)',
  `position_id` int DEFAULT NULL COMMENT 'Jabatan pengurus yang menyetujui (approver_type=2)',
  `role_id` int DEFAULT NULL COMMENT 'Role yang menyetujui (approver_type=1)',
  `user_id` int DEFAULT NULL COMMENT 'User tertentu yang menyetujui (approver_type=3)',
  `allow_revision` int NOT NULL DEFAULT '1' COMMENT '0=Tidak, 1=Ya | boleh mengembalikan untuk revisi',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_flow_step` (`flow_id`,`step_level`),
  KEY `fk_apstep_role` (`role_id`),
  KEY `fk_apstep_user` (`user_id`),
  KEY `fk_m_approval_flow_step_position_id` (`position_id`),
  CONSTRAINT `fk_apstep_flow` FOREIGN KEY (`flow_id`) REFERENCES `m_approval_flow` (`id`),
  CONSTRAINT `fk_apstep_role` FOREIGN KEY (`role_id`) REFERENCES `m_role` (`id`),
  CONSTRAINT `fk_apstep_user` FOREIGN KEY (`user_id`) REFERENCES `m_user` (`id`),
  CONSTRAINT `fk_m_approval_flow_step_position_id` FOREIGN KEY (`position_id`) REFERENCES `m_position` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_asset` (
  `id` int NOT NULL,
  `category_id` int NOT NULL,
  `asset_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `name` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `brand` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `specification` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `quantity` int NOT NULL DEFAULT '1',
  `unit` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT 'unit',
  `track_unit` int NOT NULL COMMENT '1=dicatat per unit (punya kode/QR di m_asset_unit), 0=cukup jumlah',
  `unit_prefix` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Awalan kode unit, mis. HT, KAM',
  `acquisition_date` date DEFAULT NULL,
  `acquisition_cost` decimal(15,2) DEFAULT NULL,
  `acquisition_source` int NOT NULL DEFAULT '1' COMMENT '1=BELI, 2=HIBAH, 3=SPONSOR, 4=LAINNYA',
  `procurement_item_id` int DEFAULT NULL COMMENT 'Asal dari item pengadaan',
  `sponsorship_id` int DEFAULT NULL COMMENT 'Asal dari sponsor barang',
  `rental_fee_per_day` decimal(15,2) NOT NULL COMMENT 'Tarif sewa per hari per unit (0 = gratis)',
  `late_fee_per_day` decimal(15,2) NOT NULL COMMENT 'Denda telat per hari per unit (0 = tanpa denda)',
  `location` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `asset_condition` int NOT NULL DEFAULT '1' COMMENT '1=BAIK, 2=RUSAK_RINGAN, 3=RUSAK_BERAT, 4=HILANG',
  `status` int NOT NULL DEFAULT '1' COMMENT '1=AVAILABLE, 2=BORROWED, 3=MAINTENANCE, 4=DISPOSED',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_asset_code` (`asset_code`),
  KEY `fk_asset_category` (`category_id`),
  KEY `fk_asset_pgditem` (`procurement_item_id`),
  KEY `fk_asset_sponsorship` (`sponsorship_id`),
  CONSTRAINT `fk_asset_category` FOREIGN KEY (`category_id`) REFERENCES `m_asset_category` (`id`),
  CONSTRAINT `fk_asset_pgditem` FOREIGN KEY (`procurement_item_id`) REFERENCES `t_procurement_item` (`id`),
  CONSTRAINT `fk_asset_sponsorship` FOREIGN KEY (`sponsorship_id`) REFERENCES `t_sponsorship` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_asset_category` (
  `id` int NOT NULL,
  `name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `description` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `is_active` int NOT NULL DEFAULT '1' COMMENT '0=Tidak Aktif, 1=Aktif',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_assetcat_name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_asset_photo` (
  `asset_id` int NOT NULL,
  `seq_photo` int NOT NULL,
  `photo` mediumblob NOT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int NOT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`asset_id`,`seq_photo`),
  CONSTRAINT `m_asset_photo_m_asset_FK` FOREIGN KEY (`asset_id`) REFERENCES `m_asset` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_asset_unit` (
  `id` int NOT NULL,
  `asset_id` int NOT NULL,
  `unit_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'Kode inventaris unit: unit_prefix + nomor urut, dicetak sebagai QR (berisi URL halaman unit)',
  `serial_number` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Nomor seri pabrik (opsional)',
  `unit_condition` int NOT NULL DEFAULT '1' COMMENT '1=BAIK, 2=RUSAK_RINGAN, 3=RUSAK_BERAT',
  `status` int NOT NULL DEFAULT '1' COMMENT '1=TERSEDIA, 2=DIPINJAM, 3=PERAWATAN, 4=HILANG, 5=DIHAPUS',
  `label_printed_at` datetime DEFAULT NULL COMMENT 'Kapan label QR dicetak (NULL = belum)',
  `notes` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ins_date` datetime NOT NULL,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_asset_unit_code` (`unit_code`),
  KEY `fk_m_asset_unit_asset_id` (`asset_id`),
  CONSTRAINT `fk_m_asset_unit_asset_id` FOREIGN KEY (`asset_id`) REFERENCES `m_asset` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Unit fisik aset berkode QR';
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_checklist_template` (
  `id` int NOT NULL,
  `name` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'mis. Seminar, Lomba luar kota, Konser',
  `activity_type` int DEFAULT NULL COMMENT 'Opsional: cocok untuk t_activity.activity_type tertentu',
  `description` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `is_active` int NOT NULL DEFAULT '1' COMMENT '0=Tidak, 1=Ya',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_checklist_template_item` (
  `id` int NOT NULL,
  `template_id` int NOT NULL,
  `section_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Sie tujuan, dicocokkan dengan t_committee_section.name',
  `title` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `offset_day` int NOT NULL COMMENT 'Hari relatif terhadap hari H, negatif = sebelum (mis. -21)',
  `priority` int NOT NULL DEFAULT '2' COMMENT '1=RENDAH, 2=SEDANG, 3=TINGGI',
  `is_milestone` int NOT NULL COMMENT '0=Tidak, 1=Ya | disalin ke t_activity_checklist',
  `seq_no` int NOT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `ix_cltitem_template` (`template_id`,`seq_no`),
  CONSTRAINT `fk_cltitem_template` FOREIGN KEY (`template_id`) REFERENCES `m_checklist_template` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_controller` (
  `id` int NOT NULL,
  `name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'Nama controller, mis. Member',
  `index_action` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT 'Index',
  `description` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `is_active` int NOT NULL DEFAULT '1' COMMENT '0=Tidak Aktif, 1=Aktif',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_controller_name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_controller_action` (
  `id` int NOT NULL,
  `controller_id` int NOT NULL,
  `action` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `need_auth` int NOT NULL DEFAULT '1' COMMENT '0=Non Auth, 1=Auth',
  `description` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `is_active` int NOT NULL DEFAULT '1' COMMENT '0=Tidak Aktif, 1=Aktif',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_controller_action` (`controller_id`,`action`),
  CONSTRAINT `fk_ctrlaction_ctrl` FOREIGN KEY (`controller_id`) REFERENCES `m_controller` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_division` (
  `id` int NOT NULL,
  `name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'mis. Humas, Acara, Tim Futsal Putra',
  `division_type` int NOT NULL DEFAULT '1' COMMENT '1=DIVISI/BIDANG, 2=TIM',
  `description` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `sort_order` int NOT NULL,
  `is_active` int NOT NULL DEFAULT '1' COMMENT '0=Tidak, 1=Ya',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_division_name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_document_category` (
  `id` int NOT NULL,
  `name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `description` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `is_active` int NOT NULL DEFAULT '1' COMMENT '0=Tidak Aktif, 1=Aktif',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_doccat_name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_document_number_format` (
  `id` int NOT NULL,
  `document_type` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'mis. SURAT_UND, SURAT_SK, TRX, PD, PJM, PGD, SPN, HTG, BA_ST, SERTIFIKAT, NIA, PROPOSAL',
  `number_format` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'Placeholder: {seq} {type} {org} {month_roman} {month} {year} {yy}',
  `seq_padding` int NOT NULL DEFAULT '3' COMMENT 'Jumlah digit nomor urut, mis. 3 -> 007',
  `reset_period` int NOT NULL DEFAULT '1' COMMENT '1=TAHUNAN, 2=BULANAN, 3=TIDAK_PERNAH',
  `description` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `is_active` int NOT NULL DEFAULT '1' COMMENT '0=Tidak, 1=Ya',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_number_format` (`document_type`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_dues_setting` (
  `id` int NOT NULL,
  `period_id` int DEFAULT NULL,
  `name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'mis. Iuran Bulanan 2026',
  `amount` decimal(15,2) NOT NULL,
  `frequency` int NOT NULL DEFAULT '1' COMMENT '1=MONTHLY, 2=SEMESTER, 3=YEARLY, 4=ONCE',
  `category_id` int DEFAULT NULL COMMENT 'Kategori pemasukan saat iuran dibayar',
  `is_active` int NOT NULL DEFAULT '1' COMMENT '0=Tidak Aktif, 1=Aktif',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_dues_category` (`category_id`),
  KEY `fk_dues_period` (`period_id`),
  CONSTRAINT `fk_dues_category` FOREIGN KEY (`category_id`) REFERENCES `m_financial_category` (`id`),
  CONSTRAINT `fk_dues_period` FOREIGN KEY (`period_id`) REFERENCES `m_management_period` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_external_party` (
  `id` int NOT NULL,
  `name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `party_type` int NOT NULL COMMENT '1=PERUSAHAAN/BRAND, 2=INSTANSI_PEMERINTAH, 3=MEDIA_PARTNER, 4=KOMUNITAS/ORGANISASI, 5=VENDOR/TOKO, 6=LEMBAGA_PENDIDIKAN, 7=LAINNYA',
  `industry` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `address` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `city` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `phone` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `email` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `website` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `instagram` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `logo` mediumblob,
  `logo_file_name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `logo_content_type` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `notes` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `status` int NOT NULL DEFAULT '1' COMMENT '1=AKTIF, 2=TIDAK_AKTIF, 3=BLACKLIST',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_party_name` (`name`),
  KEY `ix_party_type` (`party_type`,`status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Pihak luar: brand, organisasi lain, vendor';
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_external_party_contact` (
  `id` int NOT NULL,
  `external_party_id` int NOT NULL,
  `name` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `position` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `phone` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `email` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `is_primary` int NOT NULL COMMENT '0=Tidak, 1=Ya | maksimal satu kontak utama aktif per mitra',
  `is_active` int NOT NULL DEFAULT '1' COMMENT '0=Tidak, 1=Ya',
  `notes` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `ix_contact_party` (`external_party_id`,`is_active`,`is_primary`),
  CONSTRAINT `fk_contact_party` FOREIGN KEY (`external_party_id`) REFERENCES `m_external_party` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Kontak PIC pihak luar';
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_financial_account` (
  `id` int NOT NULL,
  `name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `account_type` int NOT NULL DEFAULT '1' COMMENT '1=CASH, 2=BANK, 3=E_WALLET',
  `bank_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `account_number` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `account_holder` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `opening_balance` decimal(15,2) NOT NULL,
  `is_active` int NOT NULL DEFAULT '1' COMMENT '0=Tidak, 1=Ya',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_financial_category` (
  `id` int NOT NULL,
  `name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `category_type` int NOT NULL COMMENT '1=INCOME, 2=EXPENSE',
  `is_active` int NOT NULL DEFAULT '1' COMMENT '0=Tidak Aktif, 1=Aktif',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_fincat_name` (`category_type`,`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_header_menu` (
  `id` int NOT NULL,
  `name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `section` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Seksi / grup menu di sidebar',
  `controller_id` int DEFAULT NULL COMMENT 'Diisi jika header langsung ke halaman (tanpa submenu)',
  `icon` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `description` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `seq` int NOT NULL,
  `is_active` int NOT NULL DEFAULT '1' COMMENT '0=Tidak Aktif, 1=Aktif',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_headermenu_ctrl` (`controller_id`),
  CONSTRAINT `fk_headermenu_ctrl` FOREIGN KEY (`controller_id`) REFERENCES `m_controller` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_header_menu_detail` (
  `id` int NOT NULL,
  `header_menu_id` int NOT NULL,
  `sub_menu_id` int NOT NULL,
  `seq` int NOT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_headermenu_detail` (`header_menu_id`,`sub_menu_id`),
  KEY `fk_hmd_sub` (`sub_menu_id`),
  CONSTRAINT `fk_hmd_header` FOREIGN KEY (`header_menu_id`) REFERENCES `m_header_menu` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_hmd_sub` FOREIGN KEY (`sub_menu_id`) REFERENCES `m_sub_menu` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_letter_template` (
  `id` int NOT NULL,
  `name` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `letter_type` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'UNDANGAN, PERMOHONAN, PEMINJAMAN, SK, KETERANGAN, ...',
  `body` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci COMMENT 'Template HTML dengan placeholder',
  `file_data` mediumblob COMMENT 'File template .docx (opsional, selain body)',
  `file_name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `content_type` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `is_active` int NOT NULL DEFAULT '1' COMMENT '0=Tidak Aktif, 1=Aktif',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_management_period` (
  `id` int NOT NULL,
  `name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'mis. Periode 2026/2027',
  `start_date` date NOT NULL,
  `end_date` date NOT NULL,
  `status` int NOT NULL DEFAULT '1' COMMENT '1=DRAFT, 2=ACTIVE, 3=CLOSED',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `ix_period_org_status` (`status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_member` (
  `id` int NOT NULL,
  `user_id` int DEFAULT NULL COMMENT 'NULL jika belum punya akun login',
  `member_number` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Nomor Induk Anggota (NIA)',
  `member_type` int NOT NULL DEFAULT '1' COMMENT '1=ANGGOTA, 2=PENGURUS, 4=ALUMNI (3 tidak dipakai)',
  `joined_at` date DEFAULT NULL COMMENT 'Tanggal bergabung',
  `left_at` date DEFAULT NULL COMMENT 'Tanggal keluar',
  `member_category` int NOT NULL DEFAULT '1' COMMENT '1=MAHASISWA, 2=UMUM',
  `nim` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Wajib untuk MAHASISWA',
  `fullname` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `gender` int DEFAULT NULL COMMENT '1=L, 2=P',
  `birth_date` date DEFAULT NULL,
  `fakultas` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `program_studi` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `angkatan` smallint DEFAULT NULL,
  `institution` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Asal instansi / kampus lain / pekerjaan (untuk UMUM)',
  `phone` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `email` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `address` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `status` int NOT NULL DEFAULT '1' COMMENT '1=ACTIVE, 2=ALUMNI, 3=INACTIVE',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_member_nim` (`nim`),
  UNIQUE KEY `uq_member_number` (`member_number`),
  UNIQUE KEY `uq_member_user` (`user_id`),
  CONSTRAINT `fk_member_user` FOREIGN KEY (`user_id`) REFERENCES `m_user` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_member_photo` (
  `id` int NOT NULL,
  `member_id` int NOT NULL,
  `photo` mediumblob NOT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int NOT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `m_member_photo_m_member_FK` (`member_id`),
  CONSTRAINT `m_member_photo_m_member_FK` FOREIGN KEY (`member_id`) REFERENCES `m_member` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_message_template` (
  `id` int NOT NULL,
  `template_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'mis. RESET_PASSWORD, APPROVAL_WAITING, CHECKLIST_DUE, MOU_EXPIRING, FUNDING_UNSETTLED',
  `channel` int NOT NULL COMMENT '1=IN_APP, 2=EMAIL, 3=WHATSAPP',
  `subject` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Untuk email / judul notifikasi',
  `body` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'Isi dengan placeholder, mis. Halo {nama}, kode reset Anda {kode}',
  `description` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `is_active` int NOT NULL DEFAULT '1' COMMENT '0=Tidak, 1=Ya',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_message_template` (`template_code`,`channel`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_organization` (
  `id` int NOT NULL,
  `code` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'Kode singkat organisasi, mis. GWA | isi placeholder {org} pada penomoran dokumen',
  `name` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `description` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `vision` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `mission` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `email` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `phone` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `instagram` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `founded_date` date DEFAULT NULL,
  `logo` mediumblob COMMENT 'Logo organisasi (dikompres aplikasi)',
  `advisor_name` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Nama pembina (hanya dicatat, bukan akun)',
  `advisor_phone` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Kontak pembina',
  `advisor_email` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Email pembina',
  `advisor_sk_number` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Nomor SK pembina',
  `advisor_sk_file` mediumblob COMMENT 'File SK pembina',
  `advisor_sk_file_name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Nama file SK pembina',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Profil organisasi (satu baris)';
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_organization_photo` (
  `seq_photo` int NOT NULL,
  `photo` mediumblob NOT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int NOT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`seq_photo`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_point_rule` (
  `id` int NOT NULL,
  `source_type` int NOT NULL COMMENT '1=HADIR_KEGIATAN, 2=PANITIA / TIM_PELAKSANA (t_committee_member), 3=PEMATERI, 4=PENGURUS, 5=PRESTASI, 6=LAINNYA',
  `activity_type` int DEFAULT NULL COMMENT 'Opsional: aturan khusus per jenis kegiatan (t_activity.activity_type)',
  `achievement_level` int DEFAULT NULL COMMENT 'Opsional: aturan khusus per tingkat prestasi',
  `points` int NOT NULL,
  `description` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `is_active` int NOT NULL DEFAULT '1' COMMENT '0=Tidak, 1=Ya',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `ix_point_rule` (`source_type`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_position` (
  `id` int NOT NULL,
  `name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'Ketua, Wakil, Sekretaris, Bendahara, Kadiv, ...',
  `description` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `is_core` int NOT NULL COMMENT '0=Tidak, 1=Ya | 1 = pengurus inti',
  `sort_order` int NOT NULL,
  `is_active` int NOT NULL DEFAULT '1' COMMENT '0=Tidak Aktif, 1=Aktif',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_position_name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_product` (
  `id` int NOT NULL,
  `name` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `product_type` int NOT NULL DEFAULT '1' COMMENT '1=MERCHANDISE, 2=MAKANAN_MINUMAN, 3=JASA, 4=LAINNYA',
  `description` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `price` decimal(15,2) NOT NULL COMMENT 'Harga jual dasar',
  `cost_price` decimal(15,2) DEFAULT NULL COMMENT 'Harga modal per unit (untuk hitung untung)',
  `unit` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT 'pcs',
  `track_stock` int NOT NULL DEFAULT '1' COMMENT '0=Tidak dilacak (jasa/pre-order), 1=Ya',
  `stock` int NOT NULL COMMENT 'Stok produk tanpa varian; produk bervarian memakai stok varian',
  `is_preorder` int NOT NULL COMMENT '0=Tidak, 1=Ya',
  `preorder_close_at` datetime DEFAULT NULL,
  `photo` mediumblob,
  `photo_file_name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `photo_content_type` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `activity_id` int DEFAULT NULL COMMENT 'Opsional: danus untuk kegiatan tertentu',
  `is_active` int NOT NULL DEFAULT '1' COMMENT '0=Tidak, 1=Ya',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_product_name` (`name`),
  KEY `fk_product_activity` (`activity_id`),
  CONSTRAINT `fk_product_activity` FOREIGN KEY (`activity_id`) REFERENCES `t_activity` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_product_variant` (
  `id` int NOT NULL,
  `product_id` int NOT NULL,
  `name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'mis. S, M, L, XL, Hitam-L',
  `price` decimal(15,2) DEFAULT NULL COMMENT 'NULL = ikut harga produk',
  `stock` int NOT NULL,
  `sku` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `seq_no` int NOT NULL,
  `is_active` int NOT NULL DEFAULT '1' COMMENT '0=Tidak, 1=Ya',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_variant_name` (`product_id`,`name`),
  CONSTRAINT `fk_variant_product` FOREIGN KEY (`product_id`) REFERENCES `m_product` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_role` (
  `id` int NOT NULL,
  `code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'SUPERADMIN, ADMIN, KETUA, BENDAHARA, SEKRETARIS, ANGGOTA, ...',
  `name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `description` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `is_active` int NOT NULL DEFAULT '1' COMMENT '0=Tidak Aktif, 1=Aktif',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_role_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_role_action` (
  `id` int NOT NULL,
  `role_id` int NOT NULL,
  `controller_action_id` int NOT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_role_action` (`role_id`,`controller_action_id`),
  KEY `fk_roleaction_action` (`controller_action_id`),
  CONSTRAINT `fk_roleaction_action` FOREIGN KEY (`controller_action_id`) REFERENCES `m_controller_action` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_roleaction_role` FOREIGN KEY (`role_id`) REFERENCES `m_role` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_role_menu` (
  `id` int NOT NULL,
  `role_id` int NOT NULL,
  `header_menu_id` int NOT NULL,
  `sub_menu_id` int DEFAULT NULL COMMENT 'NULL = akses ke header menu saja',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_role_menu` (`role_id`,`header_menu_id`,`sub_menu_id`),
  KEY `fk_rolemenu_header` (`header_menu_id`),
  KEY `fk_rolemenu_sub` (`sub_menu_id`),
  CONSTRAINT `fk_rolemenu_header` FOREIGN KEY (`header_menu_id`) REFERENCES `m_header_menu` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_rolemenu_role` FOREIGN KEY (`role_id`) REFERENCES `m_role` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_rolemenu_sub` FOREIGN KEY (`sub_menu_id`) REFERENCES `m_sub_menu` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_sequence` (
  `table_name` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `last_id` int NOT NULL,
  `upd_date` datetime DEFAULT NULL,
  PRIMARY KEY (`table_name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_site_setting` (
  `id` int NOT NULL,
  `setting_group` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'mis. TENTANG, KONTAK, SOSMED, FOOTER',
  `setting_key` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'mis. about_text, contact_email, instagram_url',
  `setting_value` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `setting_image` mediumblob COMMENT 'Untuk pengaturan berupa gambar, mis. logo',
  `description` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_site_setting` (`setting_key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_sponsorship_package` (
  `id` int NOT NULL,
  `name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'Nama paket, mis. Paket 1',
  `price` decimal(15,2) NOT NULL COMMENT 'Nilai paket',
  `seq_no` int NOT NULL DEFAULT '1' COMMENT 'Urutan tampil',
  `is_active` int NOT NULL DEFAULT '1' COMMENT '1=AKTIF, 0=NONAKTIF',
  `ins_date` datetime NOT NULL,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_package_name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Paket sponsorship milik organisasi';
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_sponsorship_package_benefit` (
  `id` int NOT NULL,
  `package_id` int NOT NULL,
  `title` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `description` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `seq_no` int NOT NULL DEFAULT '1' COMMENT 'Urutan benefit',
  `ins_date` datetime NOT NULL,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_m_sponsorship_package_benefit_package_id` (`package_id`),
  CONSTRAINT `fk_m_sponsorship_package_benefit_package_id` FOREIGN KEY (`package_id`) REFERENCES `m_sponsorship_package` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Daftar benefit per paket';
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_sub_menu` (
  `id` int NOT NULL,
  `name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `controller_id` int DEFAULT NULL,
  `icon` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `description` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `is_active` int NOT NULL DEFAULT '1' COMMENT '0=Tidak Aktif, 1=Aktif',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_submenu_ctrl` (`controller_id`),
  CONSTRAINT `fk_submenu_ctrl` FOREIGN KEY (`controller_id`) REFERENCES `m_controller` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_user` (
  `id` int NOT NULL,
  `username` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `password` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `reset_key` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Kode reset password; dikosongkan (NULL) setelah dipakai',
  `is_active` int NOT NULL DEFAULT '1' COMMENT '0=Tidak Aktif, 1=Aktif',
  `last_login_at` datetime DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_user_username` (`username`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_user_notification_pref` (
  `id` int NOT NULL,
  `user_id` int NOT NULL,
  `notif_type` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'Sama dengan t_notification.notif_type / template_code',
  `channel` int NOT NULL COMMENT '1=IN_APP, 2=EMAIL, 3=WHATSAPP',
  `is_enabled` int NOT NULL DEFAULT '1' COMMENT '0=Tidak, 1=Ya',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_notif_pref` (`user_id`,`notif_type`,`channel`),
  CONSTRAINT `fk_notifpref_user` FOREIGN KEY (`user_id`) REFERENCES `m_user` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_user_role` (
  `id` int NOT NULL,
  `user_id` int NOT NULL,
  `role_id` int NOT NULL,
  `period_id` int DEFAULT NULL COMMENT 'NULL = tidak terikat periode (mis. admin aplikasi). Jika periode CLOSED, role hanya boleh baca',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_user_role` (`user_id`,`role_id`,`period_id`),
  KEY `fk_userrole_period` (`period_id`),
  KEY `fk_userrole_role` (`role_id`),
  CONSTRAINT `fk_userrole_period` FOREIGN KEY (`period_id`) REFERENCES `m_management_period` (`id`),
  CONSTRAINT `fk_userrole_role` FOREIGN KEY (`role_id`) REFERENCES `m_role` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_userrole_user` FOREIGN KEY (`user_id`) REFERENCES `m_user` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_achievement` (
  `id` int NOT NULL,
  `competition_id` int DEFAULT NULL,
  `member_id` int DEFAULT NULL COMMENT 'NULL jika prestasi tim / organisasi',
  `team_name` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `title` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `rank_title` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Juara 1, Finalis, Medali Emas, ...',
  `level` int NOT NULL DEFAULT '4' COMMENT '1=INTERNAL, 2=KAMPUS, 3=REGIONAL, 4=NASIONAL, 5=INTERNASIONAL',
  `achieved_date` date NOT NULL,
  `certificate` mediumblob COMMENT 'File sertifikat',
  `certificate_file_name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `photo` mediumblob,
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `is_public` int NOT NULL DEFAULT '1' COMMENT '0=Private, 1=Public',
  `is_featured` int NOT NULL COMMENT '0=Tidak, 1=Ya | disorot di landing page',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_achievement_competition` (`competition_id`),
  KEY `fk_achievement_member` (`member_id`),
  CONSTRAINT `fk_achievement_competition` FOREIGN KEY (`competition_id`) REFERENCES `t_competition` (`id`),
  CONSTRAINT `fk_achievement_member` FOREIGN KEY (`member_id`) REFERENCES `m_member` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_activity` (
  `id` int NOT NULL,
  `program_id` int DEFAULT NULL COMMENT 'NULL = kegiatan insidental di luar proker',
  `parent_activity_id` int DEFAULT NULL COMMENT 'Kegiatan induk (event/lomba) untuk rapat/latihan persiapan; maksimal 1 tingkat',
  `competition_id` int DEFAULT NULL COMMENT 'Wajib untuk activity_type = 6 (LOMBA)',
  `activity_type` int NOT NULL DEFAULT '1' COMMENT '1=EVENT, 2=RAPAT, 3=LATIHAN, 4=PERTEMUAN, 5=LAINNYA, 6=LOMBA, 7=PIKET (mis. satu kegiatan "Piket Senin" + jadwal rutin + petugas di t_activity_participant)',
  `audience_type` int NOT NULL DEFAULT '1' COMMENT '1=SEMUA_ANGGOTA, 2=PENGURUS, 3=PESERTA_TERDAFTAR (t_activity_participant), 4=DIVISI (t_division_member), 5=TIM_PELAKSANA (t_committee_member kegiatan induk) | penentu siapa yang dicatat ALPA',
  `division_id` int DEFAULT NULL COMMENT 'Diisi jika audience_type = 4 (rapat divisi / latihan tim)',
  `title` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `location` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `latitude` decimal(10,7) DEFAULT NULL,
  `longitude` decimal(10,7) DEFAULT NULL,
  `start_at` datetime NOT NULL,
  `end_at` datetime DEFAULT NULL,
  `is_public` int NOT NULL COMMENT '0=Private, 1=Public | 1 = tampil di landing page',
  `is_featured` int NOT NULL COMMENT '0=Tidak, 1=Ya | disorot di landing page',
  `status` int NOT NULL DEFAULT '1' COMMENT '1=PLANNED, 2=ONGOING, 3=DONE, 4=CANCELLED',
  `created_by` int NOT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_activity_competition` (`competition_id`),
  KEY `fk_activity_creator` (`created_by`),
  KEY `fk_activity_division` (`division_id`),
  KEY `fk_activity_parent` (`parent_activity_id`),
  KEY `fk_activity_program` (`program_id`),
  KEY `ix_activity_start` (`start_at`),
  KEY `ix_activity_type` (`activity_type`,`start_at`),
  CONSTRAINT `fk_activity_competition` FOREIGN KEY (`competition_id`) REFERENCES `t_competition` (`id`),
  CONSTRAINT `fk_activity_creator` FOREIGN KEY (`created_by`) REFERENCES `m_user` (`id`),
  CONSTRAINT `fk_activity_division` FOREIGN KEY (`division_id`) REFERENCES `m_division` (`id`),
  CONSTRAINT `fk_activity_parent` FOREIGN KEY (`parent_activity_id`) REFERENCES `t_activity` (`id`),
  CONSTRAINT `fk_activity_program` FOREIGN KEY (`program_id`) REFERENCES `t_program` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_activity_checklist` (
  `id` int NOT NULL,
  `activity_id` int NOT NULL,
  `section_id` int DEFAULT NULL,
  `template_item_id` int DEFAULT NULL COMMENT 'Asal template',
  `title` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `due_date` date DEFAULT NULL,
  `pic_member_id` int DEFAULT NULL,
  `priority` int NOT NULL DEFAULT '2' COMMENT '1=RENDAH, 2=SEDANG, 3=TINGGI',
  `is_milestone` int NOT NULL COMMENT '0=Tidak, 1=Ya | tampil di timeline kegiatan',
  `status` int NOT NULL DEFAULT '1' COMMENT '1=BELUM, 2=PROSES, 3=SELESAI, 4=BATAL',
  `done_at` datetime DEFAULT NULL,
  `done_by` int DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_checklist_template_item` (`activity_id`,`template_item_id`),
  KEY `fk_checklist_doneby` (`done_by`),
  KEY `fk_checklist_pic` (`pic_member_id`),
  KEY `fk_checklist_section` (`section_id`),
  KEY `fk_checklist_titem` (`template_item_id`),
  KEY `ix_checklist_due` (`activity_id`,`status`,`due_date`),
  KEY `ix_checklist_milestone` (`activity_id`,`is_milestone`,`due_date`),
  CONSTRAINT `fk_checklist_activity` FOREIGN KEY (`activity_id`) REFERENCES `t_activity` (`id`),
  CONSTRAINT `fk_checklist_doneby` FOREIGN KEY (`done_by`) REFERENCES `m_user` (`id`),
  CONSTRAINT `fk_checklist_pic` FOREIGN KEY (`pic_member_id`) REFERENCES `m_member` (`id`),
  CONSTRAINT `fk_checklist_section` FOREIGN KEY (`section_id`) REFERENCES `t_committee_section` (`id`),
  CONSTRAINT `fk_checklist_titem` FOREIGN KEY (`template_item_id`) REFERENCES `m_checklist_template_item` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_activity_collaborator` (
  `id` int NOT NULL,
  `activity_id` int NOT NULL,
  `external_party_id` int NOT NULL COMMENT 'Mitra luar yang diajak kolaborasi',
  `collab_role` int NOT NULL DEFAULT '1' COMMENT '1=PENYELENGGARA_BERSAMA, 2=PENDUKUNG',
  `status` int NOT NULL DEFAULT '1' COMMENT '1=DIUNDANG, 2=DITERIMA, 3=DITOLAK',
  `notes` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `responded_by` int DEFAULT NULL,
  `responded_at` datetime DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_collaborator` (`activity_id`,`external_party_id`),
  KEY `fk_collab_org` (`external_party_id`),
  KEY `fk_collab_responder` (`responded_by`),
  CONSTRAINT `fk_collab_activity` FOREIGN KEY (`activity_id`) REFERENCES `t_activity` (`id`),
  CONSTRAINT `fk_collab_responder` FOREIGN KEY (`responded_by`) REFERENCES `m_user` (`id`),
  CONSTRAINT `fk_t_activity_collaborator_external_party_id` FOREIGN KEY (`external_party_id`) REFERENCES `m_external_party` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_activity_log` (
  `id` int NOT NULL,
  `user_id` int DEFAULT NULL,
  `module` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `action` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'CREATE, UPDATE, DELETE, LOGIN, APPROVE, ...',
  `entity_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Nama tabel',
  `entity_id` int DEFAULT NULL,
  `old_value` json DEFAULT NULL,
  `new_value` json DEFAULT NULL,
  `ip_address` varchar(45) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `user_agent` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `ix_log_entity` (`entity_name`,`entity_id`),
  KEY `ix_log_user` (`user_id`,`ins_date`),
  CONSTRAINT `fk_log_user` FOREIGN KEY (`user_id`) REFERENCES `m_user` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_activity_participant` (
  `id` int NOT NULL,
  `activity_id` int NOT NULL,
  `member_id` int NOT NULL,
  `participant_role` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT 'PESERTA' COMMENT 'PESERTA, PANITIA, PEMATERI, ...',
  `registration_status` int NOT NULL DEFAULT '1' COMMENT '1=REGISTERED, 2=CONFIRMED, 3=CANCELLED',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_activity_participant` (`activity_id`,`member_id`),
  KEY `fk_participant_member` (`member_id`),
  CONSTRAINT `fk_participant_activity` FOREIGN KEY (`activity_id`) REFERENCES `t_activity` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_participant_member` FOREIGN KEY (`member_id`) REFERENCES `m_member` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_activity_report` (
  `id` int NOT NULL,
  `activity_id` int DEFAULT NULL,
  `program_id` int DEFAULT NULL,
  `submitted_by` int NOT NULL,
  `title` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `summary` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `file_data` mediumblob COMMENT 'File laporan kegiatan',
  `file_name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `content_type` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `status` int NOT NULL DEFAULT '1' COMMENT '1=DRAFT, 2=SUBMITTED, 3=REVISION, 4=APPROVED',
  `submitted_at` datetime DEFAULT NULL,
  `approved_by` int DEFAULT NULL,
  `approved_at` datetime DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_report_activity` (`activity_id`),
  KEY `fk_report_approver` (`approved_by`),
  KEY `fk_report_program` (`program_id`),
  KEY `fk_report_submitter` (`submitted_by`),
  CONSTRAINT `fk_report_activity` FOREIGN KEY (`activity_id`) REFERENCES `t_activity` (`id`),
  CONSTRAINT `fk_report_approver` FOREIGN KEY (`approved_by`) REFERENCES `m_user` (`id`),
  CONSTRAINT `fk_report_program` FOREIGN KEY (`program_id`) REFERENCES `t_program` (`id`),
  CONSTRAINT `fk_report_submitter` FOREIGN KEY (`submitted_by`) REFERENCES `m_user` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_activity_rundown` (
  `id` int NOT NULL,
  `activity_id` int NOT NULL,
  `rundown_date` date NOT NULL,
  `start_time` time NOT NULL,
  `end_time` time DEFAULT NULL,
  `title` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'mis. Berangkat dari kampus, Technical meeting, Tampil',
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `location_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `section_id` int DEFAULT NULL COMMENT 'Sie penanggung jawab',
  `pic_member_id` int DEFAULT NULL,
  `seq_no` int NOT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_rundown_pic` (`pic_member_id`),
  KEY `fk_rundown_section` (`section_id`),
  KEY `ix_rundown_activity` (`activity_id`,`rundown_date`,`start_time`),
  CONSTRAINT `fk_rundown_activity` FOREIGN KEY (`activity_id`) REFERENCES `t_activity` (`id`),
  CONSTRAINT `fk_rundown_pic` FOREIGN KEY (`pic_member_id`) REFERENCES `m_member` (`id`),
  CONSTRAINT `fk_rundown_section` FOREIGN KEY (`section_id`) REFERENCES `t_committee_section` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_activity_schedule` (
  `id` int NOT NULL,
  `activity_id` int NOT NULL,
  `day_of_week` int NOT NULL COMMENT '1=Senin, 2=Selasa, 3=Rabu, 4=Kamis, 5=Jumat, 6=Sabtu, 7=Minggu',
  `start_time` time NOT NULL,
  `end_time` time NOT NULL,
  `location` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Nama tempat, mis. GOR Kampus',
  `latitude` decimal(10,7) DEFAULT NULL,
  `longitude` decimal(10,7) DEFAULT NULL,
  `radius_meter` int DEFAULT NULL,
  `late_tolerance_minute` int NOT NULL DEFAULT '10',
  `valid_from` date NOT NULL,
  `valid_until` date DEFAULT NULL COMMENT 'NULL = berlaku sampai kegiatan selesai / dinonaktifkan',
  `is_active` int NOT NULL DEFAULT '1' COMMENT '0=Tidak, 1=Ya',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `ix_schedule_activity` (`activity_id`,`is_active`,`day_of_week`),
  CONSTRAINT `fk_schedule_activity` FOREIGN KEY (`activity_id`) REFERENCES `t_activity` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_activity_trip_member` (
  `id` int NOT NULL,
  `activity_id` int NOT NULL,
  `member_id` int DEFAULT NULL COMMENT 'NULL untuk non-anggota (pelatih eksternal, sopir)',
  `guest_name` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Wajib jika member_id NULL',
  `trip_role` int NOT NULL DEFAULT '1' COMMENT '1=PESERTA, 2=OFFICIAL, 3=PELATIH, 4=KRU, 5=LAINNYA',
  `vehicle` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `room` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `emergency_contact_name` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `emergency_contact_phone` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `permission_status` int NOT NULL DEFAULT '1' COMMENT '1=BELUM, 2=ADA, 3=TIDAK_PERLU (izin orang tua/instansi)',
  `permission_file` mediumblob,
  `permission_file_name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `permission_content_type` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `notes` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_trip_member` (`activity_id`,`member_id`),
  KEY `fk_trip_member` (`member_id`),
  CONSTRAINT `fk_trip_activity` FOREIGN KEY (`activity_id`) REFERENCES `t_activity` (`id`),
  CONSTRAINT `fk_trip_member` FOREIGN KEY (`member_id`) REFERENCES `m_member` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_announcement` (
  `id` int NOT NULL,
  `author_user_id` int NOT NULL,
  `category` int NOT NULL DEFAULT '1' COMMENT '1=BERITA, 2=PENGUMUMAN, 3=AGENDA',
  `title` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `slug` varchar(220) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `excerpt` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `content` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `cover_photo` mediumblob COMMENT 'Foto sampul berita',
  `is_public` int NOT NULL DEFAULT '1' COMMENT '0=Private, 1=Public | 0 = hanya untuk anggota login',
  `is_featured` int NOT NULL COMMENT '0=Tidak, 1=Ya | disorot di landing page',
  `status` int NOT NULL DEFAULT '1' COMMENT '1=DRAFT, 2=PUBLISHED, 3=ARCHIVED',
  `published_at` datetime DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_announcement_slug` (`slug`),
  KEY `fk_announcement_author` (`author_user_id`),
  KEY `ix_announcement_pub` (`status`,`published_at`),
  CONSTRAINT `fk_announcement_author` FOREIGN KEY (`author_user_id`) REFERENCES `m_user` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_aspiration` (
  `id` int NOT NULL,
  `user_id` int DEFAULT NULL COMMENT 'NULL jika dikirim anonim',
  `is_anonymous` int NOT NULL COMMENT '0=Tidak, 1=Ya',
  `category` int NOT NULL DEFAULT '1' COMMENT '1=ASPIRASI, 2=PENGADUAN, 3=SARAN',
  `title` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `content` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `attachment` mediumblob,
  `attachment_file_name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `status` int NOT NULL DEFAULT '1' COMMENT '1=BARU, 2=DIPROSES, 3=SELESAI, 4=DITOLAK',
  `response` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `responded_by` int DEFAULT NULL,
  `responded_at` datetime DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL COMMENT 'Biarkan NULL jika anonim',
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_aspiration_responder` (`responded_by`),
  KEY `fk_aspiration_user` (`user_id`),
  KEY `ix_aspiration_target` (`status`),
  CONSTRAINT `fk_aspiration_responder` FOREIGN KEY (`responded_by`) REFERENCES `m_user` (`id`),
  CONSTRAINT `fk_aspiration_user` FOREIGN KEY (`user_id`) REFERENCES `m_user` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_asset_inspection` (
  `id` int NOT NULL,
  `asset_id` int NOT NULL,
  `asset_unit_id` int DEFAULT NULL COMMENT 'Unit yang diinspeksi (aset track_unit=1)',
  `borrow_item_id` int DEFAULT NULL COMMENT 'Terisi jika inspeksi saat pengembalian',
  `inspected_by` int NOT NULL,
  `inspected_at` datetime NOT NULL,
  `item_condition` int NOT NULL COMMENT '1=BAIK, 2=RUSAK_RINGAN, 3=RUSAK_BERAT, 4=HILANG',
  `notes` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `photo` mediumblob,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_inspection_asset` (`asset_id`),
  KEY `fk_inspection_borrow` (`borrow_item_id`),
  KEY `fk_inspection_inspector` (`inspected_by`),
  KEY `fk_t_asset_inspection_asset_unit_id` (`asset_unit_id`),
  CONSTRAINT `fk_inspection_asset` FOREIGN KEY (`asset_id`) REFERENCES `m_asset` (`id`),
  CONSTRAINT `fk_inspection_borrow` FOREIGN KEY (`borrow_item_id`) REFERENCES `t_borrow_item` (`id`),
  CONSTRAINT `fk_inspection_inspector` FOREIGN KEY (`inspected_by`) REFERENCES `m_user` (`id`),
  CONSTRAINT `fk_t_asset_inspection_asset_unit_id` FOREIGN KEY (`asset_unit_id`) REFERENCES `m_asset_unit` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_asset_movement` (
  `id` int NOT NULL,
  `asset_id` int NOT NULL,
  `asset_unit_id` int DEFAULT NULL COMMENT 'Unit yang bergerak (aset track_unit=1)',
  `movement_type` int NOT NULL COMMENT '1=IN, 2=OUT, 3=BORROW, 4=RETURN, 5=ADJUSTMENT, 6=DISPOSAL',
  `quantity` int NOT NULL,
  `movement_date` datetime NOT NULL,
  `reference_type` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `reference_id` int DEFAULT NULL,
  `notes` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `created_by` int NOT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_movement_asset` (`asset_id`),
  KEY `fk_movement_creator` (`created_by`),
  KEY `fk_t_asset_movement_asset_unit_id` (`asset_unit_id`),
  KEY `ix_movement_ref` (`reference_type`,`reference_id`),
  CONSTRAINT `fk_movement_asset` FOREIGN KEY (`asset_id`) REFERENCES `m_asset` (`id`),
  CONSTRAINT `fk_movement_creator` FOREIGN KEY (`created_by`) REFERENCES `m_user` (`id`),
  CONSTRAINT `fk_t_asset_movement_asset_unit_id` FOREIGN KEY (`asset_unit_id`) REFERENCES `m_asset_unit` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_attendance_record` (
  `id` int NOT NULL,
  `session_id` int NOT NULL,
  `member_id` int NOT NULL,
  `check_in_at` datetime DEFAULT NULL,
  `check_out_at` datetime DEFAULT NULL,
  `status` int NOT NULL DEFAULT '1' COMMENT '1=HADIR, 2=TERLAMBAT, 3=IZIN, 4=SAKIT, 5=ALPA',
  `notes` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_attendance_session_member` (`session_id`,`member_id`),
  KEY `fk_attendance_member` (`member_id`),
  CONSTRAINT `fk_attendance_member` FOREIGN KEY (`member_id`) REFERENCES `m_member` (`id`),
  CONSTRAINT `fk_attendance_session` FOREIGN KEY (`session_id`) REFERENCES `t_attendance_session` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_attendance_record_in` (
  `id` int NOT NULL,
  `attendance_id` int NOT NULL,
  `latitude` decimal(10,7) DEFAULT NULL,
  `longitude` decimal(10,7) DEFAULT NULL,
  `photo` mediumblob NOT NULL COMMENT 'Foto check-in (wajib)',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_record_in_attendance` (`attendance_id`),
  CONSTRAINT `fk_record_in_attendance` FOREIGN KEY (`attendance_id`) REFERENCES `t_attendance_record` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_attendance_record_out` (
  `id` int NOT NULL,
  `attendance_id` int NOT NULL,
  `latitude` decimal(10,7) DEFAULT NULL,
  `longitude` decimal(10,7) DEFAULT NULL,
  `photo` mediumblob NOT NULL COMMENT 'Foto check-out (wajib)',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_record_out_attendance` (`attendance_id`),
  CONSTRAINT `fk_record_out_attendance` FOREIGN KEY (`attendance_id`) REFERENCES `t_attendance_record` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_attendance_session` (
  `id` int NOT NULL,
  `activity_id` int NOT NULL,
  `schedule_id` int DEFAULT NULL COMMENT 'Terisi jika sesi dibuat otomatis dari jadwal rutin',
  `name` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'mis. Hari 1 - Sesi Pagi',
  `open_at` datetime NOT NULL,
  `close_at` datetime NOT NULL,
  `late_tolerance_minute` int NOT NULL DEFAULT '10' COMMENT 'Check-in lewat open_at + menit ini = TERLAMBAT',
  `location` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Nama tempat',
  `qr_token` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `latitude` decimal(10,7) DEFAULT NULL,
  `longitude` decimal(10,7) DEFAULT NULL,
  `radius_meter` int DEFAULT NULL COMMENT 'Radius geofence yang diizinkan',
  `status` int NOT NULL DEFAULT '1' COMMENT '1=SCHEDULED, 2=OPEN, 3=CLOSED, 4=CANCELLED',
  `cancel_reason` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Diisi jika status 4, mis. libur UTS',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_session_qr` (`qr_token`),
  UNIQUE KEY `uq_session_schedule_date` (`schedule_id`,`open_at`),
  KEY `fk_session_activity` (`activity_id`),
  CONSTRAINT `fk_session_activity` FOREIGN KEY (`activity_id`) REFERENCES `t_activity` (`id`),
  CONSTRAINT `fk_session_schedule` FOREIGN KEY (`schedule_id`) REFERENCES `t_activity_schedule` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_banner` (
  `id` int NOT NULL,
  `title` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `subtitle` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `image` mediumblob NOT NULL,
  `button_text` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `link_url` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `seq` int NOT NULL,
  `start_at` datetime DEFAULT NULL COMMENT 'NULL = langsung tayang',
  `end_at` datetime DEFAULT NULL COMMENT 'NULL = tanpa batas',
  `is_active` int NOT NULL DEFAULT '1' COMMENT '0=Tidak, 1=Ya',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `ix_banner_active` (`is_active`,`seq`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_borrow_item` (
  `id` int NOT NULL,
  `borrow_request_id` int NOT NULL,
  `asset_id` int NOT NULL,
  `quantity` int NOT NULL DEFAULT '1',
  `returned_quantity` int NOT NULL,
  `condition_on_borrow` int DEFAULT NULL COMMENT '1=BAIK, 2=RUSAK_RINGAN, 3=RUSAK_BERAT, 4=HILANG',
  `condition_on_return` int DEFAULT NULL COMMENT '1=BAIK, 2=RUSAK_RINGAN, 3=RUSAK_BERAT, 4=HILANG',
  `returned_at` datetime DEFAULT NULL,
  `rental_fee_per_day` decimal(15,2) NOT NULL COMMENT 'Salinan tarif sewa saat pinjam',
  `rental_days` int NOT NULL COMMENT 'Lama sewa (hari)',
  `rental_fee` decimal(15,2) NOT NULL COMMENT 'rental_days x rental_fee_per_day x quantity',
  `late_fee_per_day` decimal(15,2) NOT NULL COMMENT 'Salinan tarif denda saat pinjam',
  `late_days` int NOT NULL COMMENT 'Hari terlambat (track_unit=0); untuk track_unit=1 dari unit terakhir',
  `late_fee` decimal(15,2) NOT NULL COMMENT 'track_unit=0: late_days x late_fee_per_day x quantity; track_unit=1: SUM(t_borrow_item_unit.late_fee)',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_borrow_item` (`borrow_request_id`,`asset_id`),
  KEY `fk_borrowitem_asset` (`asset_id`),
  CONSTRAINT `fk_borrowitem_asset` FOREIGN KEY (`asset_id`) REFERENCES `m_asset` (`id`),
  CONSTRAINT `fk_borrowitem_request` FOREIGN KEY (`borrow_request_id`) REFERENCES `t_borrow_request` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_borrow_item_unit` (
  `id` int NOT NULL,
  `borrow_item_id` int NOT NULL,
  `asset_unit_id` int NOT NULL,
  `taken_at` datetime NOT NULL COMMENT 'Waktu unit di-scan saat diambil',
  `condition_out` int NOT NULL COMMENT '1=BAIK, 2=RUSAK_RINGAN, 3=RUSAK_BERAT',
  `returned_at` datetime DEFAULT NULL COMMENT 'Waktu unit di-scan saat dikembalikan',
  `condition_in` int DEFAULT NULL COMMENT '1=BAIK, 2=RUSAK_RINGAN, 3=RUSAK_BERAT',
  `return_status` int NOT NULL DEFAULT '1' COMMENT '1=BELUM_KEMBALI, 2=KEMBALI, 3=KEMBALI_RUSAK, 4=HILANG',
  `late_days` int NOT NULL COMMENT 'MAX(0, CEIL(hari returned_at - expected_return_date))',
  `late_fee` decimal(15,2) NOT NULL COMMENT 'late_days x t_borrow_item.late_fee_per_day',
  `notes` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ins_date` datetime NOT NULL,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_borrow_item_unit` (`borrow_item_id`,`asset_unit_id`),
  KEY `fk_t_borrow_item_unit_asset_unit_id` (`asset_unit_id`),
  CONSTRAINT `fk_t_borrow_item_unit_asset_unit_id` FOREIGN KEY (`asset_unit_id`) REFERENCES `m_asset_unit` (`id`),
  CONSTRAINT `fk_t_borrow_item_unit_borrow_item_id` FOREIGN KEY (`borrow_item_id`) REFERENCES `t_borrow_item` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Unit yang diambil dan dikembalikan per peminjaman';
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_borrow_request` (
  `id` int NOT NULL,
  `borrower_type` int NOT NULL DEFAULT '1' COMMENT '1=ANGGOTA (member_id), 2=ORGANISASI/INSTANSI (external_party_id), 3=PERORANGAN_LUAR (borrower_name)',
  `member_id` int DEFAULT NULL COMMENT 'Diisi jika borrower_type=1',
  `external_party_id` int DEFAULT NULL COMMENT 'Organisasi peminjam, diisi jika borrower_type=2',
  `contact_id` int DEFAULT NULL COMMENT 'PIC dari organisasi peminjam',
  `borrower_name` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Nama peminjam saat transaksi (wajib untuk borrower_type=3)',
  `borrower_institution` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Asal instansi peminjam',
  `borrower_phone` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Kontak peminjam',
  `borrower_email` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Email peminjam',
  `guarantee` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Jaminan, mis. KTM/KTP',
  `letter_id` int DEFAULT NULL COMMENT 'Surat permohonan peminjaman (opsional)',
  `request_number` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `purpose` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `borrow_date` datetime NOT NULL,
  `expected_return_date` datetime NOT NULL,
  `actual_return_date` datetime DEFAULT NULL,
  `rental_scheme` int NOT NULL DEFAULT '1' COMMENT '1=TARIF_TETAP, 2=SEIKHLASNYA (anggota, sewa tidak ditagih), 3=GRATIS',
  `total_rental_fee` decimal(15,2) NOT NULL COMMENT 'SUM(t_borrow_item.rental_fee); 0 untuk skema 2 dan 3',
  `total_late_fee` decimal(15,2) NOT NULL COMMENT 'SUM(t_borrow_item.late_fee), dihitung otomatis saat pengembalian',
  `late_fee_waived` decimal(15,2) NOT NULL COMMENT 'Potongan/penghapusan denda oleh pengurus',
  `waive_note` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Alasan potongan denda',
  `paid_amount` decimal(15,2) NOT NULL COMMENT 'SUM(t_financial_transaction.amount) dengan borrow_request_id ini',
  `payment_status` int NOT NULL DEFAULT '1' COMMENT '1=BELUM_BAYAR, 2=SEBAGIAN, 3=LUNAS, 4=TIDAK_ADA_TAGIHAN. Lunas jika paid_amount >= total_rental_fee + total_late_fee - late_fee_waived',
  `approved_by` int DEFAULT NULL,
  `approved_at` datetime DEFAULT NULL,
  `status` int NOT NULL DEFAULT '1' COMMENT '1=PENDING, 2=APPROVED, 3=REJECTED, 4=BORROWED, 5=RETURNED, 6=OVERDUE, 7=CANCELLED',
  `notes` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_borrow_number` (`request_number`),
  KEY `fk_borrow_approver` (`approved_by`),
  KEY `fk_borrow_member` (`member_id`),
  KEY `fk_t_borrow_request_contact_id` (`contact_id`),
  KEY `fk_t_borrow_request_external_party_id` (`external_party_id`),
  KEY `fk_t_borrow_request_letter_id` (`letter_id`),
  KEY `ix_borrow_status` (`status`),
  CONSTRAINT `fk_borrow_approver` FOREIGN KEY (`approved_by`) REFERENCES `m_user` (`id`),
  CONSTRAINT `fk_borrow_member` FOREIGN KEY (`member_id`) REFERENCES `m_member` (`id`),
  CONSTRAINT `fk_t_borrow_request_contact_id` FOREIGN KEY (`contact_id`) REFERENCES `m_external_party_contact` (`id`),
  CONSTRAINT `fk_t_borrow_request_external_party_id` FOREIGN KEY (`external_party_id`) REFERENCES `m_external_party` (`id`),
  CONSTRAINT `fk_t_borrow_request_letter_id` FOREIGN KEY (`letter_id`) REFERENCES `t_letter` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_budget` (
  `id` int NOT NULL,
  `period_id` int NOT NULL,
  `name` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `total_amount` decimal(15,2) NOT NULL,
  `status` int NOT NULL DEFAULT '1' COMMENT '1=DRAFT, 2=SUBMITTED, 3=APPROVED, 4=REJECTED',
  `approved_by` int DEFAULT NULL,
  `approved_at` datetime DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_budget_approver` (`approved_by`),
  KEY `fk_budget_period` (`period_id`),
  CONSTRAINT `fk_budget_approver` FOREIGN KEY (`approved_by`) REFERENCES `m_user` (`id`),
  CONSTRAINT `fk_budget_period` FOREIGN KEY (`period_id`) REFERENCES `m_management_period` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_budget_document` (
  `id` int NOT NULL,
  `budget_id` int NOT NULL,
  `file_name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `file_type` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `file_data` mediumblob NOT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_budgetdoc_budget` (`budget_id`),
  CONSTRAINT `fk_budgetdoc_budget` FOREIGN KEY (`budget_id`) REFERENCES `t_budget` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_budget_item` (
  `id` int NOT NULL,
  `budget_id` int NOT NULL,
  `category_id` int NOT NULL,
  `program_id` int DEFAULT NULL,
  `name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `quantity` decimal(12,2) NOT NULL DEFAULT '1.00',
  `unit` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `unit_price` decimal(15,2) NOT NULL,
  `amount` decimal(15,2) NOT NULL COMMENT 'quantity x unit_price',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_budgetitem_budget` (`budget_id`),
  KEY `fk_budgetitem_category` (`category_id`),
  KEY `fk_budgetitem_program` (`program_id`),
  CONSTRAINT `fk_budgetitem_budget` FOREIGN KEY (`budget_id`) REFERENCES `t_budget` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_budgetitem_category` FOREIGN KEY (`category_id`) REFERENCES `m_financial_category` (`id`),
  CONSTRAINT `fk_budgetitem_program` FOREIGN KEY (`program_id`) REFERENCES `t_program` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_certificate` (
  `id` int NOT NULL,
  `member_id` int NOT NULL,
  `certificate_type` int NOT NULL COMMENT '1=PESERTA, 2=PANITIA / TIM_PELAKSANA (t_committee_member), 3=PEMATERI, 4=PENGURUS, 5=PRESTASI, 6=LAINNYA',
  `activity_id` int DEFAULT NULL,
  `assignment_id` int DEFAULT NULL COMMENT 'Untuk sertifikat kepengurusan',
  `achievement_id` int DEFAULT NULL COMMENT 'Untuk sertifikat prestasi',
  `certificate_number` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `title` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `issued_date` date NOT NULL,
  `verification_code` char(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'Kode acak untuk cek keaslian sertifikat secara publik',
  `file_data` mediumblob,
  `file_name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `status` int NOT NULL DEFAULT '1' COMMENT '1=TERBIT, 2=DICABUT',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_certificate_number` (`certificate_number`),
  UNIQUE KEY `uq_certificate_verify` (`verification_code`),
  KEY `fk_cert_achievement` (`achievement_id`),
  KEY `fk_cert_activity` (`activity_id`),
  KEY `fk_cert_assignment` (`assignment_id`),
  KEY `fk_cert_member` (`member_id`),
  CONSTRAINT `fk_cert_achievement` FOREIGN KEY (`achievement_id`) REFERENCES `t_achievement` (`id`),
  CONSTRAINT `fk_cert_activity` FOREIGN KEY (`activity_id`) REFERENCES `t_activity` (`id`),
  CONSTRAINT `fk_cert_assignment` FOREIGN KEY (`assignment_id`) REFERENCES `t_management_assignment` (`id`),
  CONSTRAINT `fk_cert_member` FOREIGN KEY (`member_id`) REFERENCES `m_member` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_committee_application` (
  `id` int NOT NULL,
  `recruitment_id` int NOT NULL,
  `member_id` int NOT NULL,
  `section_choice_1_id` int NOT NULL,
  `section_choice_2_id` int DEFAULT NULL,
  `reason` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `form_response_id` int DEFAULT NULL,
  `status` int NOT NULL DEFAULT '1' COMMENT '1=DAFTAR, 2=DITERIMA, 3=DITOLAK, 4=MUNDUR',
  `reviewed_by` int DEFAULT NULL,
  `reviewed_at` datetime DEFAULT NULL,
  `review_note` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_committee_application` (`recruitment_id`,`member_id`),
  KEY `fk_comapp_choice1` (`section_choice_1_id`),
  KEY `fk_comapp_choice2` (`section_choice_2_id`),
  KEY `fk_comapp_member` (`member_id`),
  KEY `fk_comapp_response` (`form_response_id`),
  KEY `fk_comapp_reviewer` (`reviewed_by`),
  CONSTRAINT `fk_comapp_choice1` FOREIGN KEY (`section_choice_1_id`) REFERENCES `t_committee_section` (`id`),
  CONSTRAINT `fk_comapp_choice2` FOREIGN KEY (`section_choice_2_id`) REFERENCES `t_committee_section` (`id`),
  CONSTRAINT `fk_comapp_member` FOREIGN KEY (`member_id`) REFERENCES `m_member` (`id`),
  CONSTRAINT `fk_comapp_recruitment` FOREIGN KEY (`recruitment_id`) REFERENCES `t_committee_recruitment` (`id`),
  CONSTRAINT `fk_comapp_response` FOREIGN KEY (`form_response_id`) REFERENCES `t_form_response` (`id`),
  CONSTRAINT `fk_comapp_reviewer` FOREIGN KEY (`reviewed_by`) REFERENCES `m_user` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_committee_member` (
  `id` int NOT NULL,
  `activity_id` int NOT NULL,
  `section_id` int DEFAULT NULL COMMENT 'NULL untuk pengurus inti (ketua pelaksana, sekretaris, bendahara)',
  `member_id` int NOT NULL,
  `committee_role` int NOT NULL COMMENT '1=KETUA_PELAKSANA, 2=WAKIL_KETUA, 3=SEKRETARIS, 4=BENDAHARA, 5=KOORDINATOR_SIE, 6=STAF, 7=OFFICIAL, 8=PELATIH',
  `job_desc` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `status` int NOT NULL DEFAULT '1' COMMENT '1=AKTIF, 2=MUNDUR, 3=DIKELUARKAN',
  `application_id` int DEFAULT NULL COMMENT 'Jika berasal dari pendaftaran tim pelaksana',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_committee_member` (`activity_id`,`member_id`),
  KEY `fk_commem_application` (`application_id`),
  KEY `fk_commem_member` (`member_id`),
  KEY `fk_commem_section` (`section_id`),
  KEY `ix_committee_role` (`activity_id`,`committee_role`,`status`),
  CONSTRAINT `fk_commem_activity` FOREIGN KEY (`activity_id`) REFERENCES `t_activity` (`id`),
  CONSTRAINT `fk_commem_application` FOREIGN KEY (`application_id`) REFERENCES `t_committee_application` (`id`),
  CONSTRAINT `fk_commem_member` FOREIGN KEY (`member_id`) REFERENCES `m_member` (`id`),
  CONSTRAINT `fk_commem_section` FOREIGN KEY (`section_id`) REFERENCES `t_committee_section` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_committee_recruitment` (
  `id` int NOT NULL,
  `activity_id` int NOT NULL,
  `open_at` datetime NOT NULL,
  `close_at` datetime NOT NULL,
  `quota` int DEFAULT NULL,
  `form_id` int DEFAULT NULL COMMENT 'Pertanyaan tambahan (t_form)',
  `status` int NOT NULL DEFAULT '1' COMMENT '1=DRAFT, 2=BUKA, 3=TUTUP',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_committee_recruitment` (`activity_id`),
  KEY `fk_comrec_form` (`form_id`),
  CONSTRAINT `fk_comrec_activity` FOREIGN KEY (`activity_id`) REFERENCES `t_activity` (`id`),
  CONSTRAINT `fk_comrec_form` FOREIGN KEY (`form_id`) REFERENCES `t_form` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_committee_section` (
  `id` int NOT NULL,
  `activity_id` int NOT NULL,
  `name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'Acara, Konsumsi, Perlengkapan, Humas, Danus, Dokumentasi, Official, Transportasi & Akomodasi, Kostum, dll.',
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci COMMENT 'Job desk sie',
  `coordinator_member_id` int DEFAULT NULL,
  `seq_no` int NOT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_section_name` (`activity_id`,`name`),
  KEY `fk_section_coordinator` (`coordinator_member_id`),
  CONSTRAINT `fk_section_activity` FOREIGN KEY (`activity_id`) REFERENCES `t_activity` (`id`),
  CONSTRAINT `fk_section_coordinator` FOREIGN KEY (`coordinator_member_id`) REFERENCES `m_member` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_competition` (
  `id` int NOT NULL,
  `name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `organizer` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Penyelenggara',
  `level` int NOT NULL DEFAULT '4' COMMENT '1=INTERNAL, 2=KAMPUS, 3=REGIONAL, 4=NASIONAL, 5=INTERNASIONAL',
  `category` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `registration_deadline` datetime DEFAULT NULL,
  `start_date` date DEFAULT NULL,
  `end_date` date DEFAULT NULL,
  `location` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `poster` mediumblob,
  `link_url` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `is_public` int NOT NULL DEFAULT '1' COMMENT '0=Private, 1=Public',
  `is_featured` int NOT NULL COMMENT '0=Tidak, 1=Ya | disorot di landing page',
  `status` int NOT NULL DEFAULT '1' COMMENT '1=UPCOMING, 2=REGISTRATION, 3=ONGOING, 4=FINISHED, 5=CANCELLED',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_competition_material` (
  `id` int NOT NULL,
  `competition_id` int DEFAULT NULL COMMENT 'NULL = materi umum, tidak terikat lomba tertentu',
  `title` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `material_type` int NOT NULL DEFAULT '1' COMMENT '1=FILE, 2=LINK, 3=VIDEO',
  `file_data` longblob COMMENT 'NULL jika materi berupa link (url)',
  `file_name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `content_type` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `url` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `visibility` int NOT NULL DEFAULT '2' COMMENT '1=PUBLIC, 2=MEMBER, 3=PARTICIPANT',
  `uploaded_by` int NOT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_material_competition` (`competition_id`),
  KEY `fk_material_uploader` (`uploaded_by`),
  CONSTRAINT `fk_material_competition` FOREIGN KEY (`competition_id`) REFERENCES `t_competition` (`id`),
  CONSTRAINT `fk_material_uploader` FOREIGN KEY (`uploaded_by`) REFERENCES `m_user` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_competition_participant` (
  `id` int NOT NULL,
  `competition_id` int NOT NULL,
  `member_id` int NOT NULL,
  `team_name` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `participant_role` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT 'ANGGOTA' COMMENT 'KETUA_TIM, ANGGOTA, PEMBIMBING',
  `status` int NOT NULL DEFAULT '1' COMMENT '1=REGISTERED, 2=PARTICIPATING, 3=FINISHED, 4=WITHDRAWN',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_competition_participant` (`competition_id`,`member_id`),
  KEY `fk_compart_member` (`member_id`),
  CONSTRAINT `fk_compart_competition` FOREIGN KEY (`competition_id`) REFERENCES `t_competition` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_compart_member` FOREIGN KEY (`member_id`) REFERENCES `m_member` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_debt` (
  `id` int NOT NULL,
  `period_id` int NOT NULL COMMENT 'Periode saat hutang dibuat',
  `debt_number` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `creditor_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'Pihak pemberi hutang',
  `creditor_contact` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `description` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `amount` decimal(15,2) NOT NULL,
  `paid_amount` decimal(15,2) NOT NULL,
  `debt_date` date NOT NULL,
  `due_date` date DEFAULT NULL,
  `status` int NOT NULL DEFAULT '1' COMMENT '1=BELUM_BAYAR, 2=SEBAGIAN, 3=LUNAS, 4=DIHAPUSKAN',
  `transaction_id` int DEFAULT NULL COMMENT 'Transaksi uang masuk jika hutang berupa pinjaman uang',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_debt_number` (`debt_number`),
  KEY `fk_debt_period` (`period_id`),
  KEY `fk_debt_trx` (`transaction_id`),
  KEY `ix_debt_status` (`status`),
  CONSTRAINT `fk_debt_period` FOREIGN KEY (`period_id`) REFERENCES `m_management_period` (`id`),
  CONSTRAINT `fk_debt_trx` FOREIGN KEY (`transaction_id`) REFERENCES `t_financial_transaction` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_debt_attachment` (
  `id` int NOT NULL,
  `debt_id` int NOT NULL,
  `file_data` mediumblob NOT NULL COMMENT 'Bukti hutang (surat perjanjian, nota, faktur, foto)',
  `file_name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `file_type` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_debtattach_debt` (`debt_id`),
  CONSTRAINT `fk_debtattach_debt` FOREIGN KEY (`debt_id`) REFERENCES `t_debt` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_debt_payment` (
  `id` int NOT NULL,
  `debt_id` int NOT NULL,
  `transaction_id` int NOT NULL COMMENT 'Transaksi pengeluaran untuk pembayaran',
  `amount` decimal(15,2) NOT NULL,
  `paid_date` date NOT NULL,
  `notes` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_debtpay_trx` (`transaction_id`),
  KEY `fk_debtpay_debt` (`debt_id`),
  CONSTRAINT `fk_debtpay_debt` FOREIGN KEY (`debt_id`) REFERENCES `t_debt` (`id`),
  CONSTRAINT `fk_debtpay_trx` FOREIGN KEY (`transaction_id`) REFERENCES `t_financial_transaction` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_division_member` (
  `id` int NOT NULL,
  `division_id` int NOT NULL,
  `member_id` int NOT NULL,
  `period_id` int DEFAULT NULL COMMENT 'NULL = keanggotaan tim tidak terikat periode',
  `member_role` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT 'ANGGOTA' COMMENT 'mis. KETUA, KAPTEN, ANGGOTA',
  `joined_at` date DEFAULT NULL,
  `left_at` date DEFAULT NULL,
  `is_active` int NOT NULL DEFAULT '1' COMMENT '0=Tidak, 1=Ya',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_division_member` (`division_id`,`member_id`,`period_id`),
  KEY `fk_divmember_member` (`member_id`),
  KEY `fk_divmember_period` (`period_id`),
  CONSTRAINT `fk_divmember_division` FOREIGN KEY (`division_id`) REFERENCES `m_division` (`id`),
  CONSTRAINT `fk_divmember_member` FOREIGN KEY (`member_id`) REFERENCES `m_member` (`id`),
  CONSTRAINT `fk_divmember_period` FOREIGN KEY (`period_id`) REFERENCES `m_management_period` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_document` (
  `id` int NOT NULL,
  `category_id` int NOT NULL,
  `uploaded_by` int NOT NULL,
  `title` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `file_data` longblob NOT NULL,
  `file_name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `content_type` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `file_size` int DEFAULT NULL,
  `visibility` int NOT NULL DEFAULT '3' COMMENT '1=PUBLIC, 2=MEMBER, 3=PENGURUS',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_document_category` (`category_id`),
  KEY `fk_document_uploader` (`uploaded_by`),
  CONSTRAINT `fk_document_category` FOREIGN KEY (`category_id`) REFERENCES `m_document_category` (`id`),
  CONSTRAINT `fk_document_uploader` FOREIGN KEY (`uploaded_by`) REFERENCES `m_user` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_document_number_seq` (
  `id` int NOT NULL,
  `document_type` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `period_key` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'Sesuai reset_period: 2026 (tahunan), 2026-10 (bulanan), ALL (tidak pernah)',
  `last_number` int NOT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_docnum_seq` (`document_type`,`period_key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_dues_bill` (
  `id` int NOT NULL,
  `dues_setting_id` int NOT NULL,
  `member_id` int NOT NULL,
  `bill_period` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'mis. 2026-10, 2026-GANJIL, 2026',
  `amount` decimal(15,2) NOT NULL,
  `due_date` date DEFAULT NULL,
  `status` int NOT NULL DEFAULT '1' COMMENT '1=UNPAID, 2=PAID, 3=WAIVED',
  `paid_at` datetime DEFAULT NULL,
  `transaction_id` int DEFAULT NULL COMMENT 'Transaksi kas masuk saat dibayar',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_dues_bill` (`dues_setting_id`,`member_id`,`bill_period`),
  KEY `fk_duesbill_trx` (`transaction_id`),
  KEY `ix_dues_status` (`member_id`,`status`),
  CONSTRAINT `fk_duesbill_member` FOREIGN KEY (`member_id`) REFERENCES `m_member` (`id`),
  CONSTRAINT `fk_duesbill_setting` FOREIGN KEY (`dues_setting_id`) REFERENCES `m_dues_setting` (`id`),
  CONSTRAINT `fk_duesbill_trx` FOREIGN KEY (`transaction_id`) REFERENCES `t_financial_transaction` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_election` (
  `id` int NOT NULL,
  `period_id` int DEFAULT NULL COMMENT 'Periode kepengurusan yang akan dipilih',
  `title` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `start_at` datetime NOT NULL,
  `end_at` datetime NOT NULL,
  `status` int NOT NULL DEFAULT '1' COMMENT '1=DRAFT, 2=REGISTRATION, 3=OPEN, 4=CLOSED, 5=FINALIZED',
  `finalized_at` datetime DEFAULT NULL,
  `finalized_by` int DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_election_finalizer` (`finalized_by`),
  KEY `fk_election_period` (`period_id`),
  CONSTRAINT `fk_election_finalizer` FOREIGN KEY (`finalized_by`) REFERENCES `m_user` (`id`),
  CONSTRAINT `fk_election_period` FOREIGN KEY (`period_id`) REFERENCES `m_management_period` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_election_audit_event` (
  `id` int NOT NULL,
  `election_id` int NOT NULL,
  `actor_user_id` int DEFAULT NULL,
  `event_type` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'OPENED, CLOSED, CANDIDATE_VERIFIED, RESULT_FINALIZED, ...',
  `description` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ip_address` varchar(45) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_elaudit_actor` (`actor_user_id`),
  KEY `fk_elaudit_election` (`election_id`),
  CONSTRAINT `fk_elaudit_actor` FOREIGN KEY (`actor_user_id`) REFERENCES `m_user` (`id`),
  CONSTRAINT `fk_elaudit_election` FOREIGN KEY (`election_id`) REFERENCES `t_election` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_election_candidate` (
  `id` int NOT NULL,
  `election_id` int NOT NULL,
  `member_id` int NOT NULL,
  `candidate_number` int DEFAULT NULL,
  `vision` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `mission` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `photo` mediumblob,
  `status` int NOT NULL DEFAULT '1' COMMENT '1=REGISTERED, 2=VERIFIED, 3=REJECTED, 4=WITHDRAWN',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_candidate_member` (`election_id`,`member_id`),
  UNIQUE KEY `uq_candidate_number` (`election_id`,`candidate_number`),
  KEY `fk_candidate_member` (`member_id`),
  CONSTRAINT `fk_candidate_election` FOREIGN KEY (`election_id`) REFERENCES `t_election` (`id`),
  CONSTRAINT `fk_candidate_member` FOREIGN KEY (`member_id`) REFERENCES `m_member` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_election_eligible_voter` (
  `id` int NOT NULL,
  `election_id` int NOT NULL,
  `member_id` int NOT NULL,
  `eligibility_status` int NOT NULL DEFAULT '1' COMMENT '1=ELIGIBLE, 2=INELIGIBLE',
  `voted_at` datetime DEFAULT NULL COMMENT 'Terisi saat sudah memilih (tanpa menyimpan pilihannya)',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_eligible_voter` (`election_id`,`member_id`),
  KEY `fk_voter_member` (`member_id`),
  CONSTRAINT `fk_voter_election` FOREIGN KEY (`election_id`) REFERENCES `t_election` (`id`),
  CONSTRAINT `fk_voter_member` FOREIGN KEY (`member_id`) REFERENCES `m_member` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_election_result` (
  `id` int NOT NULL,
  `election_id` int NOT NULL,
  `candidate_id` int NOT NULL,
  `vote_count` int NOT NULL,
  `vote_percentage` decimal(5,2) NOT NULL,
  `rank_no` int DEFAULT NULL,
  `is_winner` int NOT NULL COMMENT '0=Lose, 1=Winner',
  `calculated_at` datetime NOT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_election_result` (`election_id`,`candidate_id`),
  KEY `fk_result_candidate` (`candidate_id`),
  CONSTRAINT `fk_result_candidate` FOREIGN KEY (`candidate_id`) REFERENCES `t_election_candidate` (`id`),
  CONSTRAINT `fk_result_election` FOREIGN KEY (`election_id`) REFERENCES `t_election` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_election_vote` (
  `id` int NOT NULL COMMENT 'ACAK (bukan dari m_sequence) agar urutan memilih tidak terbaca',
  `election_id` int NOT NULL,
  `ballot_reference` char(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'Hash acak untuk verifikasi pemilih tanpa membuka identitas',
  `encrypted_choice` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'AES-256-GCM: id kandidat + nilai acak. Didekripsi hanya saat finalisasi',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Isi tanggal saja (jam 00:00), jangan waktu persis',
  `ins_by` int DEFAULT NULL COMMENT 'Selalu NULL, jangan isi id pemilih',
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_ballot_reference` (`ballot_reference`),
  KEY `ix_vote_election` (`election_id`),
  CONSTRAINT `fk_vote_election` FOREIGN KEY (`election_id`) REFERENCES `t_election` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_financial_attachment` (
  `id` int NOT NULL,
  `transaction_id` int NOT NULL,
  `file_data` mediumblob NOT NULL COMMENT 'Bukti transaksi (nota, kuitansi, foto)',
  `file_name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `file_type` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_finattach_trx` (`transaction_id`),
  CONSTRAINT `fk_finattach_trx` FOREIGN KEY (`transaction_id`) REFERENCES `t_financial_transaction` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_financial_transaction` (
  `id` int NOT NULL,
  `account_id` int NOT NULL,
  `category_id` int NOT NULL,
  `period_id` int DEFAULT NULL,
  `budget_item_id` int DEFAULT NULL COMMENT 'Realisasi terhadap item anggaran',
  `funding_request_id` int DEFAULT NULL COMMENT 'Pencairan dari pengajuan dana',
  `activity_id` int DEFAULT NULL COMMENT 'Kegiatan (event, rapat, latihan, ...)',
  `program_id` int DEFAULT NULL COMMENT 'Program kerja',
  `competition_id` int DEFAULT NULL COMMENT 'Lomba',
  `sponsorship_id` int DEFAULT NULL COMMENT 'Pemasukan dari sponsor',
  `procurement_id` int DEFAULT NULL COMMENT 'Pengeluaran untuk pengadaan barang',
  `sales_order_id` int DEFAULT NULL COMMENT 'Pembayaran penjualan danus/merchandise',
  `borrow_request_id` int DEFAULT NULL COMMENT 'Pembayaran sewa/sumbangan/denda peminjaman; jenisnya lewat category_id',
  `transaction_number` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `transaction_type` int NOT NULL COMMENT '1=INCOME, 2=EXPENSE',
  `amount` decimal(15,2) NOT NULL,
  `transaction_date` date NOT NULL,
  `description` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `status` int NOT NULL DEFAULT '1' COMMENT '1=DRAFT, 2=POSTED, 3=VOID',
  `created_by` int NOT NULL,
  `approved_by` int DEFAULT NULL,
  `approved_at` datetime DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_trx_number` (`transaction_number`),
  KEY `fk_t_financial_transaction_borrow_request_id` (`borrow_request_id`),
  KEY `fk_trx_account` (`account_id`),
  KEY `fk_trx_activity` (`activity_id`),
  KEY `fk_trx_approver` (`approved_by`),
  KEY `fk_trx_budgetitem` (`budget_item_id`),
  KEY `fk_trx_category` (`category_id`),
  KEY `fk_trx_competition` (`competition_id`),
  KEY `fk_trx_creator` (`created_by`),
  KEY `fk_trx_funding` (`funding_request_id`),
  KEY `fk_trx_period` (`period_id`),
  KEY `fk_trx_procurement` (`procurement_id`),
  KEY `fk_trx_program` (`program_id`),
  KEY `fk_trx_sales` (`sales_order_id`),
  KEY `fk_trx_sponsorship` (`sponsorship_id`),
  KEY `ix_trx_date` (`transaction_date`),
  CONSTRAINT `fk_t_financial_transaction_borrow_request_id` FOREIGN KEY (`borrow_request_id`) REFERENCES `t_borrow_request` (`id`),
  CONSTRAINT `fk_trx_account` FOREIGN KEY (`account_id`) REFERENCES `m_financial_account` (`id`),
  CONSTRAINT `fk_trx_activity` FOREIGN KEY (`activity_id`) REFERENCES `t_activity` (`id`),
  CONSTRAINT `fk_trx_approver` FOREIGN KEY (`approved_by`) REFERENCES `m_user` (`id`),
  CONSTRAINT `fk_trx_budgetitem` FOREIGN KEY (`budget_item_id`) REFERENCES `t_budget_item` (`id`),
  CONSTRAINT `fk_trx_category` FOREIGN KEY (`category_id`) REFERENCES `m_financial_category` (`id`),
  CONSTRAINT `fk_trx_competition` FOREIGN KEY (`competition_id`) REFERENCES `t_competition` (`id`),
  CONSTRAINT `fk_trx_creator` FOREIGN KEY (`created_by`) REFERENCES `m_user` (`id`),
  CONSTRAINT `fk_trx_funding` FOREIGN KEY (`funding_request_id`) REFERENCES `t_funding_request` (`id`),
  CONSTRAINT `fk_trx_period` FOREIGN KEY (`period_id`) REFERENCES `m_management_period` (`id`),
  CONSTRAINT `fk_trx_procurement` FOREIGN KEY (`procurement_id`) REFERENCES `t_procurement_request` (`id`),
  CONSTRAINT `fk_trx_program` FOREIGN KEY (`program_id`) REFERENCES `t_program` (`id`),
  CONSTRAINT `fk_trx_sales` FOREIGN KEY (`sales_order_id`) REFERENCES `t_sales_order` (`id`),
  CONSTRAINT `fk_trx_sponsorship` FOREIGN KEY (`sponsorship_id`) REFERENCES `t_sponsorship` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_form` (
  `id` int NOT NULL,
  `form_type` int NOT NULL COMMENT '1=EVALUASI_KEGIATAN, 2=PENDAFTARAN_REKRUTMEN, 3=SURVEI_UMUM',
  `activity_id` int DEFAULT NULL COMMENT 'Untuk evaluasi kegiatan',
  `recruitment_id` int DEFAULT NULL COMMENT 'Untuk pertanyaan tambahan rekrutmen',
  `title` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `description` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `is_anonymous` int NOT NULL COMMENT '0=Tidak, 1=Ya | jawaban tidak menyimpan identitas',
  `open_at` datetime DEFAULT NULL,
  `close_at` datetime DEFAULT NULL,
  `status` int NOT NULL DEFAULT '1' COMMENT '1=DRAFT, 2=OPEN, 3=CLOSED',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_form_activity` (`activity_id`),
  KEY `fk_form_recruitment` (`recruitment_id`),
  KEY `ix_form_org` (`form_type`,`status`),
  CONSTRAINT `fk_form_activity` FOREIGN KEY (`activity_id`) REFERENCES `t_activity` (`id`),
  CONSTRAINT `fk_form_recruitment` FOREIGN KEY (`recruitment_id`) REFERENCES `t_recruitment` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_form_answer` (
  `id` int NOT NULL,
  `response_id` int NOT NULL,
  `question_id` int NOT NULL,
  `answer_text` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Teks / pilihan / angka skala / tanggal',
  `answer_file` mediumblob COMMENT 'Untuk pertanyaan tipe FILE',
  `answer_file_name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_answer` (`response_id`,`question_id`),
  KEY `fk_answer_question` (`question_id`),
  CONSTRAINT `fk_answer_question` FOREIGN KEY (`question_id`) REFERENCES `t_form_question` (`id`),
  CONSTRAINT `fk_answer_response` FOREIGN KEY (`response_id`) REFERENCES `t_form_response` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_form_question` (
  `id` int NOT NULL,
  `form_id` int NOT NULL,
  `seq` int NOT NULL,
  `question_text` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `question_type` int NOT NULL COMMENT '1=TEKS_SINGKAT, 2=PARAGRAF, 3=PILIHAN_GANDA, 4=KOTAK_CENTANG, 5=SKALA_1_5, 6=TANGGAL, 7=FILE',
  `options` json DEFAULT NULL COMMENT 'Daftar pilihan untuk tipe 3 dan 4, mis. ["Ya","Tidak"]',
  `is_required` int NOT NULL COMMENT '0=Tidak, 1=Ya',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `ix_question_form` (`form_id`,`seq`),
  CONSTRAINT `fk_question_form` FOREIGN KEY (`form_id`) REFERENCES `t_form` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_form_response` (
  `id` int NOT NULL,
  `form_id` int NOT NULL,
  `member_id` int DEFAULT NULL COMMENT 'NULL jika form anonim atau responden bukan anggota',
  `applicant_id` int DEFAULT NULL COMMENT 'Diisi untuk jawaban formulir rekrutmen',
  `submitted_at` datetime NOT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_response_applicant` (`applicant_id`),
  KEY `fk_response_member` (`member_id`),
  KEY `ix_response_form` (`form_id`),
  CONSTRAINT `fk_response_applicant` FOREIGN KEY (`applicant_id`) REFERENCES `t_recruitment_applicant` (`id`),
  CONSTRAINT `fk_response_form` FOREIGN KEY (`form_id`) REFERENCES `t_form` (`id`),
  CONSTRAINT `fk_response_member` FOREIGN KEY (`member_id`) REFERENCES `m_member` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_funding_approval` (
  `id` int NOT NULL,
  `request_id` int NOT NULL,
  `approver_user_id` int NOT NULL,
  `approval_level` int NOT NULL DEFAULT '1',
  `flow_step_id` int DEFAULT NULL COMMENT 'Langkah m_approval_flow_step yang diputuskan',
  `decision` int NOT NULL DEFAULT '1' COMMENT '1=PENDING, 2=APPROVED, 3=REJECTED, 4=REVISION',
  `note` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `decided_at` datetime DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_funding_approval` (`request_id`,`approval_level`),
  KEY `fk_fundappr_step` (`flow_step_id`),
  KEY `fk_fundappr_user` (`approver_user_id`),
  CONSTRAINT `fk_fundappr_request` FOREIGN KEY (`request_id`) REFERENCES `t_funding_request` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_fundappr_step` FOREIGN KEY (`flow_step_id`) REFERENCES `m_approval_flow_step` (`id`),
  CONSTRAINT `fk_fundappr_user` FOREIGN KEY (`approver_user_id`) REFERENCES `m_user` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_funding_realization` (
  `id` int NOT NULL,
  `funding_request_id` int NOT NULL,
  `realization_date` date NOT NULL,
  `description` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'mis. Konsumsi rapat 20 orang',
  `quantity` decimal(12,2) DEFAULT NULL,
  `unit` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `amount` decimal(15,2) NOT NULL,
  `receipt` mediumblob COMMENT 'Foto / scan nota',
  `receipt_file_name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `receipt_content_type` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `notes` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `ix_realization_request` (`funding_request_id`,`realization_date`),
  CONSTRAINT `fk_realization_request` FOREIGN KEY (`funding_request_id`) REFERENCES `t_funding_request` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_funding_request` (
  `id` int NOT NULL,
  `period_id` int DEFAULT NULL,
  `budget_item_id` int DEFAULT NULL,
  `program_id` int DEFAULT NULL,
  `activity_id` int DEFAULT NULL,
  `competition_id` int DEFAULT NULL COMMENT 'Lomba',
  `requested_by` int NOT NULL,
  `request_number` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `purpose` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `amount` decimal(15,2) NOT NULL,
  `realized_amount` decimal(15,2) NOT NULL COMMENT 'Total terpakai (jumlah t_funding_realization)',
  `returned_amount` decimal(15,2) NOT NULL COMMENT 'Sisa dana yang dikembalikan ke kas',
  `status` int NOT NULL DEFAULT '1' COMMENT '1=DRAFT, 2=SUBMITTED, 3=APPROVED, 4=REJECTED, 5=DISBURSED',
  `settlement_status` int NOT NULL DEFAULT '1' COMMENT '1=BELUM, 2=DIAJUKAN, 3=SELESAI',
  `settled_by` int DEFAULT NULL COMMENT 'Bendahara yang memeriksa',
  `settled_at` datetime DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_funding_number` (`request_number`),
  KEY `fk_funding_activity` (`activity_id`),
  KEY `fk_funding_budgetitem` (`budget_item_id`),
  KEY `fk_funding_competition` (`competition_id`),
  KEY `fk_funding_period` (`period_id`),
  KEY `fk_funding_program` (`program_id`),
  KEY `fk_funding_requester` (`requested_by`),
  KEY `fk_funding_settler` (`settled_by`),
  KEY `ix_funding_settlement` (`settlement_status`),
  CONSTRAINT `fk_funding_activity` FOREIGN KEY (`activity_id`) REFERENCES `t_activity` (`id`),
  CONSTRAINT `fk_funding_budgetitem` FOREIGN KEY (`budget_item_id`) REFERENCES `t_budget_item` (`id`),
  CONSTRAINT `fk_funding_competition` FOREIGN KEY (`competition_id`) REFERENCES `t_competition` (`id`),
  CONSTRAINT `fk_funding_period` FOREIGN KEY (`period_id`) REFERENCES `m_management_period` (`id`),
  CONSTRAINT `fk_funding_program` FOREIGN KEY (`program_id`) REFERENCES `t_program` (`id`),
  CONSTRAINT `fk_funding_requester` FOREIGN KEY (`requested_by`) REFERENCES `m_user` (`id`),
  CONSTRAINT `fk_funding_settler` FOREIGN KEY (`settled_by`) REFERENCES `m_user` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_gallery` (
  `id` int NOT NULL,
  `activity_id` int DEFAULT NULL,
  `title` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `event_date` date DEFAULT NULL,
  `is_public` int NOT NULL DEFAULT '1' COMMENT '0=Private, 1=Public',
  `status` int NOT NULL DEFAULT '1' COMMENT '1=DRAFT, 2=PUBLISHED, 3=ARCHIVED',
  `published_at` datetime DEFAULT NULL,
  `is_featured` int NOT NULL COMMENT '0=Tidak, 1=Ya | disorot di landing page',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_gallery_activity` (`activity_id`),
  CONSTRAINT `fk_gallery_activity` FOREIGN KEY (`activity_id`) REFERENCES `t_activity` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_gallery_photo` (
  `id` int NOT NULL,
  `gallery_id` int NOT NULL,
  `photo` mediumblob NOT NULL,
  `caption` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `seq` int NOT NULL,
  `is_cover` int NOT NULL COMMENT '0=Tidak, 1=Ya | foto sampul galeri',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_galphoto_gallery` (`gallery_id`),
  CONSTRAINT `fk_galphoto_gallery` FOREIGN KEY (`gallery_id`) REFERENCES `t_gallery` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_letter` (
  `id` int NOT NULL,
  `period_id` int DEFAULT NULL,
  `template_id` int DEFAULT NULL,
  `direction` int NOT NULL COMMENT '1=IN, 2=OUT | IN = surat masuk, OUT = surat keluar',
  `letter_type` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `letter_number` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'OUT: diisi saat approved; IN: nomor dari pengirim',
  `subject` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `letter_date` date DEFAULT NULL,
  `received_date` date DEFAULT NULL COMMENT 'Khusus surat masuk',
  `sender` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `recipient` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `summary` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `body` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `file_data` mediumblob COMMENT 'File PDF final / scan surat masuk',
  `file_name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `content_type` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `status` int NOT NULL DEFAULT '1' COMMENT '1=DRAFT, 2=WAITING_APPROVAL, 3=APPROVED, 4=REJECTED, 5=SENT, 6=RECEIVED, 7=ARCHIVED',
  `created_by` int NOT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_letter_number` (`direction`,`letter_number`),
  KEY `fk_letter_creator` (`created_by`),
  KEY `fk_letter_period` (`period_id`),
  KEY `fk_letter_template` (`template_id`),
  KEY `ix_letter_status` (`direction`,`status`),
  CONSTRAINT `fk_letter_creator` FOREIGN KEY (`created_by`) REFERENCES `m_user` (`id`),
  CONSTRAINT `fk_letter_period` FOREIGN KEY (`period_id`) REFERENCES `m_management_period` (`id`),
  CONSTRAINT `fk_letter_template` FOREIGN KEY (`template_id`) REFERENCES `m_letter_template` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_letter_approval` (
  `id` int NOT NULL,
  `letter_id` int NOT NULL,
  `approver_user_id` int NOT NULL,
  `position_id` int DEFAULT NULL COMMENT 'Jabatan penandatangan (Ketua, Sekretaris, ...)',
  `approval_level` int NOT NULL DEFAULT '1',
  `flow_step_id` int DEFAULT NULL COMMENT 'Langkah m_approval_flow_step yang diputuskan',
  `decision` int NOT NULL DEFAULT '1' COMMENT '1=PENDING, 2=APPROVED, 3=REJECTED',
  `note` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `signature` mediumblob COMMENT 'Gambar tanda tangan',
  `decided_at` datetime DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_letter_approval` (`letter_id`,`approval_level`),
  KEY `fk_letterappr_position` (`position_id`),
  KEY `fk_letterappr_step` (`flow_step_id`),
  KEY `fk_letterappr_user` (`approver_user_id`),
  CONSTRAINT `fk_letterappr_letter` FOREIGN KEY (`letter_id`) REFERENCES `t_letter` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_letterappr_position` FOREIGN KEY (`position_id`) REFERENCES `m_position` (`id`),
  CONSTRAINT `fk_letterappr_step` FOREIGN KEY (`flow_step_id`) REFERENCES `m_approval_flow_step` (`id`),
  CONSTRAINT `fk_letterappr_user` FOREIGN KEY (`approver_user_id`) REFERENCES `m_user` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_letter_disposition` (
  `id` int NOT NULL,
  `letter_id` int NOT NULL,
  `from_user_id` int NOT NULL,
  `to_member_id` int NOT NULL,
  `instruction` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `due_date` date DEFAULT NULL,
  `status` int NOT NULL DEFAULT '1' COMMENT '1=OPEN, 2=DONE',
  `done_at` datetime DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_disposition_from` (`from_user_id`),
  KEY `fk_disposition_letter` (`letter_id`),
  KEY `fk_disposition_to` (`to_member_id`),
  CONSTRAINT `fk_disposition_from` FOREIGN KEY (`from_user_id`) REFERENCES `m_user` (`id`),
  CONSTRAINT `fk_disposition_letter` FOREIGN KEY (`letter_id`) REFERENCES `t_letter` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_disposition_to` FOREIGN KEY (`to_member_id`) REFERENCES `m_member` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_lpj` (
  `id` int NOT NULL,
  `lpj_type` int NOT NULL COMMENT '1=KEGIATAN, 2=KEPENGURUSAN',
  `activity_id` int DEFAULT NULL COMMENT 'Wajib untuk LPJ kegiatan',
  `period_id` int DEFAULT NULL COMMENT 'Wajib untuk LPJ kepengurusan',
  `title` varchar(200) NOT NULL,
  `introduction` text COMMENT 'Pendahuluan',
  `background` text COMMENT 'Latar belakang (kegiatan)',
  `activity_kind` text COMMENT 'Macam kegiatan (kegiatan)',
  `purpose` text COMMENT 'Maksud dan tujuan',
  `activity_form` text COMMENT 'Bentuk kegiatan (kegiatan)',
  `vision_mission` text COMMENT 'Visi dan misi (kepengurusan)',
  `closing` text COMMENT 'Penutup',
  `status` int NOT NULL DEFAULT '1' COMMENT '1=DRAFT, 2=FINAL',
  `document` longblob COMMENT 'PDF final',
  `document_name` varchar(255) DEFAULT NULL,
  `finalized_by` int DEFAULT NULL COMMENT 'User yang memfinalkan',
  `finalized_at` datetime DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_lpj_activity` (`activity_id`),
  UNIQUE KEY `uq_lpj_period` (`period_id`),
  CONSTRAINT `fk_lpj_activity` FOREIGN KEY (`activity_id`) REFERENCES `t_activity` (`id`),
  CONSTRAINT `fk_lpj_period` FOREIGN KEY (`period_id`) REFERENCES `m_management_period` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Laporan pertanggungjawaban kegiatan / kepengurusan';
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_maintenance_log` (
  `id` int NOT NULL,
  `asset_id` int NOT NULL,
  `asset_unit_id` int DEFAULT NULL COMMENT 'Unit yang dirawat (aset track_unit=1)',
  `maintenance_date` date NOT NULL,
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `vendor` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `cost` decimal(15,2) NOT NULL,
  `status` int NOT NULL DEFAULT '1' COMMENT '1=SCHEDULED, 2=IN_PROGRESS, 3=DONE',
  `finished_at` datetime DEFAULT NULL,
  `transaction_id` int DEFAULT NULL COMMENT 'Link ke pengeluaran kas jika ada biaya',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_maintenance_asset` (`asset_id`),
  KEY `fk_maintenance_trx` (`transaction_id`),
  KEY `fk_t_maintenance_log_asset_unit_id` (`asset_unit_id`),
  CONSTRAINT `fk_maintenance_asset` FOREIGN KEY (`asset_id`) REFERENCES `m_asset` (`id`),
  CONSTRAINT `fk_maintenance_trx` FOREIGN KEY (`transaction_id`) REFERENCES `t_financial_transaction` (`id`),
  CONSTRAINT `fk_t_maintenance_log_asset_unit_id` FOREIGN KEY (`asset_unit_id`) REFERENCES `m_asset_unit` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_management_assignment` (
  `id` int NOT NULL,
  `period_id` int NOT NULL,
  `member_id` int NOT NULL,
  `position_id` int NOT NULL,
  `division_id` int DEFAULT NULL COMMENT 'Divisi yang dipimpin, mis. untuk jabatan Kadiv',
  `sk_number` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Nomor SK pengangkatan',
  `start_date` date NOT NULL,
  `end_date` date DEFAULT NULL,
  `is_current` int NOT NULL DEFAULT '1' COMMENT '0=Tidak, 1=Ya',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_assignment_division` (`division_id`),
  KEY `fk_assignment_member` (`member_id`),
  KEY `fk_assignment_position` (`position_id`),
  KEY `ix_assignment_current` (`period_id`,`is_current`),
  CONSTRAINT `fk_assignment_division` FOREIGN KEY (`division_id`) REFERENCES `m_division` (`id`),
  CONSTRAINT `fk_assignment_member` FOREIGN KEY (`member_id`) REFERENCES `m_member` (`id`),
  CONSTRAINT `fk_assignment_period` FOREIGN KEY (`period_id`) REFERENCES `m_management_period` (`id`),
  CONSTRAINT `fk_assignment_position` FOREIGN KEY (`position_id`) REFERENCES `m_position` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_meeting_minutes` (
  `id` int NOT NULL,
  `session_id` int NOT NULL,
  `chaired_by_member_id` int DEFAULT NULL COMMENT 'Pimpinan rapat',
  `notulis_member_id` int DEFAULT NULL COMMENT 'Penulis notulen',
  `agenda` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `content` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'Isi / jalannya rapat',
  `decisions` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Keputusan rapat',
  `follow_up` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Tindak lanjut + penanggung jawab',
  `file_data` mediumblob COMMENT 'Lampiran (foto papan tulis, materi, dll.)',
  `file_name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `content_type` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `status` int NOT NULL DEFAULT '1' COMMENT '1=DRAFT, 2=FINAL',
  `finalized_by` int DEFAULT NULL,
  `finalized_at` datetime DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_minutes_session` (`session_id`),
  KEY `fk_minutes_chair` (`chaired_by_member_id`),
  KEY `fk_minutes_finalizer` (`finalized_by`),
  KEY `fk_minutes_notulis` (`notulis_member_id`),
  CONSTRAINT `fk_minutes_chair` FOREIGN KEY (`chaired_by_member_id`) REFERENCES `m_member` (`id`),
  CONSTRAINT `fk_minutes_finalizer` FOREIGN KEY (`finalized_by`) REFERENCES `m_user` (`id`),
  CONSTRAINT `fk_minutes_notulis` FOREIGN KEY (`notulis_member_id`) REFERENCES `m_member` (`id`),
  CONSTRAINT `fk_minutes_session` FOREIGN KEY (`session_id`) REFERENCES `t_attendance_session` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_member_point` (
  `id` int NOT NULL,
  `member_id` int NOT NULL,
  `period_id` int DEFAULT NULL,
  `source_type` int NOT NULL COMMENT '1=HADIR_KEGIATAN, 2=PANITIA / TIM_PELAKSANA (t_committee_member), 3=PEMATERI, 4=PENGURUS, 5=PRESTASI, 6=LAINNYA',
  `ref_id` int DEFAULT NULL COMMENT 'id sumber: t_attendance_record, t_activity_participant, t_management_assignment, t_achievement',
  `points` int NOT NULL,
  `earned_date` date NOT NULL,
  `notes` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_member_point_source` (`member_id`,`source_type`,`ref_id`),
  KEY `fk_point_period` (`period_id`),
  KEY `ix_point_member` (`member_id`,`earned_date`),
  CONSTRAINT `fk_point_member` FOREIGN KEY (`member_id`) REFERENCES `m_member` (`id`),
  CONSTRAINT `fk_point_period` FOREIGN KEY (`period_id`) REFERENCES `m_management_period` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_message_outbox` (
  `id` int NOT NULL,
  `user_id` int DEFAULT NULL COMMENT 'Penerima jika punya akun',
  `recipient` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'Alamat email / nomor WhatsApp',
  `channel` int NOT NULL COMMENT '2=EMAIL, 3=WHATSAPP',
  `template_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `subject` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `body` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'Isi final. Untuk RESET_PASSWORD, kosongkan/samarkan kode setelah terkirim',
  `ref_type` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Sumber pesan, mis. t_procurement_request',
  `ref_id` int DEFAULT NULL,
  `status` int NOT NULL DEFAULT '1' COMMENT '1=MENUNGGU, 2=TERKIRIM, 3=GAGAL, 4=DIBATALKAN',
  `attempt_count` int NOT NULL,
  `last_error` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `scheduled_at` datetime NOT NULL COMMENT 'Kapan boleh dikirim',
  `sent_at` datetime DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_outbox_user` (`user_id`),
  KEY `ix_outbox_queue` (`status`,`scheduled_at`),
  KEY `ix_outbox_ref` (`ref_type`,`ref_id`),
  CONSTRAINT `fk_outbox_user` FOREIGN KEY (`user_id`) REFERENCES `m_user` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_notification` (
  `id` int NOT NULL,
  `user_id` int NOT NULL,
  `notif_type` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'LETTER_APPROVAL, FUNDING_APPROVAL, BORROW_DUE, DUES_BILL, ...',
  `title` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `message` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `link_url` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ref_type` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ref_id` int DEFAULT NULL,
  `read_at` datetime DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `ix_notification_unread` (`user_id`,`read_at`),
  CONSTRAINT `fk_notification_user` FOREIGN KEY (`user_id`) REFERENCES `m_user` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_period_balance` (
  `id` int NOT NULL,
  `period_id` int NOT NULL,
  `account_id` int NOT NULL,
  `opening_balance` decimal(15,2) NOT NULL COMMENT 'Saldo awal = closing_balance periode sebelumnya',
  `total_income` decimal(15,2) NOT NULL,
  `total_expense` decimal(15,2) NOT NULL,
  `closing_balance` decimal(15,2) DEFAULT NULL COMMENT 'Diisi saat periode ditutup',
  `status` int NOT NULL DEFAULT '1' COMMENT '1=OPEN, 2=CLOSED',
  `closed_at` datetime DEFAULT NULL,
  `closed_by` int DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_balance_period_account` (`period_id`,`account_id`),
  KEY `fk_balance_account` (`account_id`),
  KEY `fk_balance_closer` (`closed_by`),
  CONSTRAINT `fk_balance_account` FOREIGN KEY (`account_id`) REFERENCES `m_financial_account` (`id`),
  CONSTRAINT `fk_balance_closer` FOREIGN KEY (`closed_by`) REFERENCES `m_user` (`id`),
  CONSTRAINT `fk_balance_period` FOREIGN KEY (`period_id`) REFERENCES `m_management_period` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_period_handover` (
  `id` int NOT NULL,
  `from_period_id` int NOT NULL,
  `to_period_id` int NOT NULL,
  `handover_number` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Nomor berita acara',
  `handover_date` date NOT NULL,
  `handed_by_member_id` int DEFAULT NULL COMMENT 'Ketua periode lama',
  `received_by_member_id` int DEFAULT NULL COMMENT 'Ketua periode baru',
  `notes` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `lpj_document` mediumblob COMMENT 'Dokumen LPJ akhir periode',
  `lpj_file_name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `status` int NOT NULL DEFAULT '1' COMMENT '1=DRAFT, 2=DIAJUKAN, 3=DISETUJUI, 4=DITOLAK',
  `approved_by` int DEFAULT NULL,
  `approved_at` datetime DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_handover_from` (`from_period_id`),
  KEY `fk_handover_approver` (`approved_by`),
  KEY `fk_handover_handed` (`handed_by_member_id`),
  KEY `fk_handover_received` (`received_by_member_id`),
  KEY `fk_handover_to` (`to_period_id`),
  CONSTRAINT `fk_handover_approver` FOREIGN KEY (`approved_by`) REFERENCES `m_user` (`id`),
  CONSTRAINT `fk_handover_from` FOREIGN KEY (`from_period_id`) REFERENCES `m_management_period` (`id`),
  CONSTRAINT `fk_handover_handed` FOREIGN KEY (`handed_by_member_id`) REFERENCES `m_member` (`id`),
  CONSTRAINT `fk_handover_received` FOREIGN KEY (`received_by_member_id`) REFERENCES `m_member` (`id`),
  CONSTRAINT `fk_handover_to` FOREIGN KEY (`to_period_id`) REFERENCES `m_management_period` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_period_handover_item` (
  `id` int NOT NULL,
  `handover_id` int NOT NULL,
  `item_type` int NOT NULL COMMENT '1=SALDO, 2=ASET, 3=PROGRAM, 4=PIUTANG, 5=HUTANG, 6=PINJAMAN, 7=DOKUMEN, 8=LAINNYA, 9=MOU_AKTIF, 10=KEWAJIBAN_SPONSOR, 11=SPONSOR_BERJALAN, 12=PENGADAAN_BERJALAN',
  `ref_id` int DEFAULT NULL COMMENT 'id sesuai item_type: m_financial_account, m_asset, t_program, t_dues_bill, t_debt, t_borrow_request, t_document, -, t_partnership_agreement, t_sponsorship_benefit, t_sponsorship, t_procurement_request',
  `description` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `amount` decimal(15,2) DEFAULT NULL,
  `quantity` int DEFAULT NULL,
  `item_condition` int DEFAULT NULL COMMENT '1=BAIK, 2=RUSAK_RINGAN, 3=RUSAK_BERAT, 4=HILANG',
  `notes` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `ix_handover_item_type` (`handover_id`,`item_type`),
  CONSTRAINT `fk_handover_item` FOREIGN KEY (`handover_id`) REFERENCES `t_period_handover` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_procurement_approval` (
  `id` int NOT NULL,
  `request_id` int NOT NULL,
  `approval_level` int NOT NULL COMMENT 'Urutan langkah m_approval_flow (PROCUREMENT), mis. 1=Bendahara, 2=Ketua Umum',
  `flow_step_id` int DEFAULT NULL COMMENT 'Langkah m_approval_flow_step yang diputuskan',
  `approver_user_id` int NOT NULL,
  `decision` int NOT NULL COMMENT '1=SETUJU, 2=REVISI, 3=TOLAK',
  `note` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `signature` mediumblob,
  `signature_file_name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `signature_content_type` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `decided_at` datetime NOT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_pgdappr_step` (`flow_step_id`),
  KEY `fk_pgdappr_user` (`approver_user_id`),
  KEY `ix_pgdappr_request` (`request_id`,`approval_level`),
  CONSTRAINT `fk_pgdappr_request` FOREIGN KEY (`request_id`) REFERENCES `t_procurement_request` (`id`),
  CONSTRAINT `fk_pgdappr_step` FOREIGN KEY (`flow_step_id`) REFERENCES `m_approval_flow_step` (`id`),
  CONSTRAINT `fk_pgdappr_user` FOREIGN KEY (`approver_user_id`) REFERENCES `m_user` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_procurement_item` (
  `id` int NOT NULL,
  `request_id` int NOT NULL,
  `item_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `specification` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci COMMENT 'Merek, ukuran, spesifikasi',
  `asset_category_id` int DEFAULT NULL COMMENT 'Wajib jika is_asset = 1',
  `is_asset` int NOT NULL COMMENT '0=Habis pakai, 1=Masuk inventaris sebagai aset',
  `quantity` int NOT NULL,
  `unit` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT 'pcs' COMMENT 'pcs, set, lusin, dll.',
  `estimated_unit_price` decimal(15,2) NOT NULL,
  `actual_unit_price` decimal(15,2) DEFAULT NULL,
  `external_party_id` int DEFAULT NULL COMMENT 'Vendor (m_partner)',
  `received_quantity` int NOT NULL COMMENT 'Dihitung dari penerimaan, tidak boleh melebihi quantity',
  `notes` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_pgditem_category` (`asset_category_id`),
  KEY `fk_pgditem_party` (`external_party_id`),
  KEY `fk_pgditem_request` (`request_id`),
  CONSTRAINT `fk_pgditem_category` FOREIGN KEY (`asset_category_id`) REFERENCES `m_asset_category` (`id`),
  CONSTRAINT `fk_pgditem_party` FOREIGN KEY (`external_party_id`) REFERENCES `m_external_party` (`id`),
  CONSTRAINT `fk_pgditem_request` FOREIGN KEY (`request_id`) REFERENCES `t_procurement_request` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_procurement_quotation` (
  `id` int NOT NULL,
  `request_id` int NOT NULL,
  `item_id` int DEFAULT NULL COMMENT 'NULL = penawaran untuk seluruh usulan',
  `external_party_id` int DEFAULT NULL,
  `vendor_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Jika vendor belum terdaftar di m_partner',
  `quoted_amount` decimal(15,2) NOT NULL,
  `file` mediumblob,
  `file_name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `content_type` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `is_selected` int NOT NULL COMMENT '0=Tidak, 1=Ya',
  `notes` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_quote_item` (`item_id`),
  KEY `fk_quote_party` (`external_party_id`),
  KEY `fk_quote_request` (`request_id`),
  CONSTRAINT `fk_quote_item` FOREIGN KEY (`item_id`) REFERENCES `t_procurement_item` (`id`),
  CONSTRAINT `fk_quote_party` FOREIGN KEY (`external_party_id`) REFERENCES `m_external_party` (`id`),
  CONSTRAINT `fk_quote_request` FOREIGN KEY (`request_id`) REFERENCES `t_procurement_request` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_procurement_receipt` (
  `id` int NOT NULL,
  `request_id` int NOT NULL,
  `receipt_date` date NOT NULL,
  `received_by_member_id` int NOT NULL,
  `notes` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `photo` mediumblob,
  `photo_file_name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `photo_content_type` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_pgdrcpt_member` (`received_by_member_id`),
  KEY `fk_pgdrcpt_request` (`request_id`),
  CONSTRAINT `fk_pgdrcpt_member` FOREIGN KEY (`received_by_member_id`) REFERENCES `m_member` (`id`),
  CONSTRAINT `fk_pgdrcpt_request` FOREIGN KEY (`request_id`) REFERENCES `t_procurement_request` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_procurement_receipt_item` (
  `id` int NOT NULL,
  `receipt_id` int NOT NULL,
  `item_id` int NOT NULL,
  `quantity` int NOT NULL,
  `item_condition` int NOT NULL DEFAULT '1' COMMENT '1=BAIK, 2=RUSAK_RINGAN, 3=RUSAK_BERAT, 4=HILANG (sama dengan m_asset.asset_condition)',
  `notes` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_receipt_item` (`receipt_id`,`item_id`),
  KEY `fk_rcptitem_item` (`item_id`),
  CONSTRAINT `fk_rcptitem_item` FOREIGN KEY (`item_id`) REFERENCES `t_procurement_item` (`id`),
  CONSTRAINT `fk_rcptitem_receipt` FOREIGN KEY (`receipt_id`) REFERENCES `t_procurement_receipt` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_procurement_request` (
  `id` int NOT NULL,
  `request_number` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'PGD-{org}-{yyyy}-{seq}',
  `period_id` int DEFAULT NULL,
  `requested_by_member_id` int NOT NULL,
  `request_date` date NOT NULL,
  `title` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `reason` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'Alasan kebutuhan',
  `activity_id` int DEFAULT NULL,
  `program_id` int DEFAULT NULL COMMENT 'Tujuan: maksimal satu dari activity/program',
  `budget_item_id` int DEFAULT NULL COMMENT 'Item anggaran sumber dana',
  `payment_method` int NOT NULL DEFAULT '1' COMMENT '1=DIBAYAR_LANGSUNG (transaksi pengeluaran oleh bendahara), 2=PENGAJUAN_DANA (uang muka lalu dipertanggungjawabkan)',
  `funding_request_id` int DEFAULT NULL COMMENT 'Wajib jika payment_method = 2 mulai status DIBELI',
  `needed_by_date` date DEFAULT NULL,
  `total_estimate` decimal(15,2) NOT NULL COMMENT 'Dihitung dari item',
  `total_actual` decimal(15,2) NOT NULL COMMENT 'Jumlah transaksi POSTED dengan procurement_id ini',
  `is_over_budget` int NOT NULL COMMENT '0=Tidak, 1=Ya | total melebihi sisa item anggaran saat diajukan; hanya peringatan, tidak menolak',
  `budget_remaining` decimal(15,2) DEFAULT NULL COMMENT 'Sisa item anggaran saat diajukan (untuk ditampilkan ke approver)',
  `status` int NOT NULL DEFAULT '1' COMMENT '1=DRAFT, 2=DIAJUKAN, 3=REVISI, 4=DISETUJUI, 5=DITOLAK, 6=DIBELI, 7=DITERIMA_SEBAGIAN, 8=SELESAI, 9=BATAL',
  `reject_reason` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_procurement_number` (`request_number`),
  KEY `fk_pgd_activity` (`activity_id`),
  KEY `fk_pgd_budgetitem` (`budget_item_id`),
  KEY `fk_pgd_funding` (`funding_request_id`),
  KEY `fk_pgd_period` (`period_id`),
  KEY `fk_pgd_program` (`program_id`),
  KEY `fk_pgd_requester` (`requested_by_member_id`),
  KEY `ix_procurement_status` (`status`),
  CONSTRAINT `fk_pgd_activity` FOREIGN KEY (`activity_id`) REFERENCES `t_activity` (`id`),
  CONSTRAINT `fk_pgd_budgetitem` FOREIGN KEY (`budget_item_id`) REFERENCES `t_budget_item` (`id`),
  CONSTRAINT `fk_pgd_funding` FOREIGN KEY (`funding_request_id`) REFERENCES `t_funding_request` (`id`),
  CONSTRAINT `fk_pgd_period` FOREIGN KEY (`period_id`) REFERENCES `m_management_period` (`id`),
  CONSTRAINT `fk_pgd_program` FOREIGN KEY (`program_id`) REFERENCES `t_program` (`id`),
  CONSTRAINT `fk_pgd_requester` FOREIGN KEY (`requested_by_member_id`) REFERENCES `m_member` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_product_stock_movement` (
  `id` int NOT NULL,
  `product_id` int NOT NULL,
  `variant_id` int DEFAULT NULL,
  `movement_type` int NOT NULL COMMENT '1=MASUK (produksi/beli), 2=TERJUAL, 3=PENYESUAIAN, 4=RETUR, 5=RUSAK/HILANG',
  `quantity` int NOT NULL COMMENT 'Positif = bertambah, negatif = berkurang',
  `movement_date` datetime NOT NULL,
  `order_id` int DEFAULT NULL COMMENT 'Untuk TERJUAL / RETUR',
  `procurement_id` int DEFAULT NULL COMMENT 'Untuk MASUK dari pengadaan',
  `notes` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_stock_order` (`order_id`),
  KEY `fk_stock_procurement` (`procurement_id`),
  KEY `fk_stock_variant` (`variant_id`),
  KEY `ix_stock_product` (`product_id`,`variant_id`,`movement_date`),
  CONSTRAINT `fk_stock_order` FOREIGN KEY (`order_id`) REFERENCES `t_sales_order` (`id`),
  CONSTRAINT `fk_stock_procurement` FOREIGN KEY (`procurement_id`) REFERENCES `t_procurement_request` (`id`),
  CONSTRAINT `fk_stock_product` FOREIGN KEY (`product_id`) REFERENCES `m_product` (`id`),
  CONSTRAINT `fk_stock_variant` FOREIGN KEY (`variant_id`) REFERENCES `m_product_variant` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_program` (
  `id` int NOT NULL,
  `period_id` int NOT NULL,
  `previous_program_id` int DEFAULT NULL COMMENT 'Program periode sebelumnya yang dilanjutkan',
  `pic_member_id` int DEFAULT NULL COMMENT 'Penanggung jawab',
  `division_id` int DEFAULT NULL COMMENT 'Bidang / divisi pelaksana program',
  `term` int DEFAULT NULL COMMENT '1=JANGKA_PENDEK, 2=JANGKA_PANJANG',
  `title` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `start_date` date DEFAULT NULL,
  `end_date` date DEFAULT NULL,
  `budget_amount` decimal(15,2) NOT NULL,
  `status` int NOT NULL DEFAULT '1' COMMENT '1=PLANNED, 2=ONGOING, 3=DONE, 4=CANCELLED',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_program_period` (`period_id`),
  KEY `fk_program_pic` (`pic_member_id`),
  KEY `fk_program_previous` (`previous_program_id`),
  KEY `fk_program_division` (`division_id`),
  CONSTRAINT `fk_program_division` FOREIGN KEY (`division_id`) REFERENCES `m_division` (`id`),
  CONSTRAINT `fk_program_period` FOREIGN KEY (`period_id`) REFERENCES `m_management_period` (`id`),
  CONSTRAINT `fk_program_pic` FOREIGN KEY (`pic_member_id`) REFERENCES `m_member` (`id`),
  CONSTRAINT `fk_program_previous` FOREIGN KEY (`previous_program_id`) REFERENCES `t_program` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_program_task` (
  `id` int NOT NULL,
  `period_id` int NOT NULL,
  `program_id` int DEFAULT NULL COMMENT 'NULL = tugas umum pengurus (bukan bagian program kerja)',
  `division_id` int DEFAULT NULL COMMENT 'Opsional: tugas untuk divisi tertentu',
  `responsible_member_id` int DEFAULT NULL,
  `title` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `priority` int NOT NULL DEFAULT '2' COMMENT '1=RENDAH, 2=SEDANG, 3=TINGGI',
  `start_date` date DEFAULT NULL,
  `due_date` date DEFAULT NULL,
  `is_milestone` int NOT NULL COMMENT '0=Tidak, 1=Ya | tampil di timeline periode',
  `status` int NOT NULL DEFAULT '1' COMMENT '1=BELUM, 2=PROSES, 3=SELESAI, 4=BATAL (sama dengan t_activity_checklist)',
  `completed_at` datetime DEFAULT NULL,
  `done_by` int DEFAULT NULL COMMENT 'User yang menandai selesai',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_task_division` (`division_id`),
  KEY `fk_task_doneby` (`done_by`),
  KEY `fk_task_member` (`responsible_member_id`),
  KEY `fk_task_period` (`period_id`),
  KEY `fk_task_program` (`program_id`),
  KEY `ix_task_milestone` (`period_id`,`is_milestone`,`due_date`),
  KEY `ix_task_period` (`period_id`,`status`,`due_date`),
  CONSTRAINT `fk_task_division` FOREIGN KEY (`division_id`) REFERENCES `m_division` (`id`),
  CONSTRAINT `fk_task_doneby` FOREIGN KEY (`done_by`) REFERENCES `m_user` (`id`),
  CONSTRAINT `fk_task_member` FOREIGN KEY (`responsible_member_id`) REFERENCES `m_member` (`id`),
  CONSTRAINT `fk_task_period` FOREIGN KEY (`period_id`) REFERENCES `m_management_period` (`id`),
  CONSTRAINT `fk_task_program` FOREIGN KEY (`program_id`) REFERENCES `t_program` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_recruitment` (
  `id` int NOT NULL,
  `period_id` int DEFAULT NULL,
  `title` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `open_date` datetime NOT NULL,
  `close_date` datetime NOT NULL,
  `quota` int DEFAULT NULL,
  `accept_public` int NOT NULL COMMENT '0=Hanya mahasiswa, 1=Juga menerima pendaftar umum',
  `status` int NOT NULL DEFAULT '1' COMMENT '1=DRAFT, 2=OPEN, 3=CLOSED',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_recruitment_period` (`period_id`),
  CONSTRAINT `fk_recruitment_period` FOREIGN KEY (`period_id`) REFERENCES `m_management_period` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_recruitment_applicant` (
  `id` int NOT NULL,
  `recruitment_id` int NOT NULL,
  `member_id` int DEFAULT NULL COMMENT 'Terisi jika sudah terdaftar sebagai m_member',
  `applicant_category` int NOT NULL DEFAULT '1' COMMENT '1=MAHASISWA, 2=UMUM',
  `nim` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Wajib untuk MAHASISWA',
  `fullname` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `email` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `phone` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `program_studi` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `angkatan` smallint DEFAULT NULL,
  `institution` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Asal instansi / kampus lain / pekerjaan (untuk UMUM)',
  `motivation` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `status` int NOT NULL DEFAULT '1' COMMENT '1=SUBMITTED, 2=SCREENING, 3=INTERVIEW, 4=ACCEPTED, 5=REJECTED',
  `reviewed_by` int DEFAULT NULL,
  `reviewed_at` datetime DEFAULT NULL,
  `notes` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_applicant_email` (`recruitment_id`,`email`),
  UNIQUE KEY `uq_applicant` (`recruitment_id`,`nim`),
  KEY `fk_applicant_member` (`member_id`),
  KEY `fk_applicant_reviewer` (`reviewed_by`),
  CONSTRAINT `fk_applicant_member` FOREIGN KEY (`member_id`) REFERENCES `m_member` (`id`),
  CONSTRAINT `fk_applicant_recruitment` FOREIGN KEY (`recruitment_id`) REFERENCES `t_recruitment` (`id`),
  CONSTRAINT `fk_applicant_reviewer` FOREIGN KEY (`reviewed_by`) REFERENCES `m_user` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_sales_order` (
  `id` int NOT NULL,
  `order_number` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'Dari penomoran terpusat, jenis DANUS',
  `period_id` int DEFAULT NULL,
  `activity_id` int DEFAULT NULL COMMENT 'Opsional: penjualan untuk kegiatan tertentu',
  `buyer_member_id` int DEFAULT NULL COMMENT 'Pembeli anggota',
  `buyer_name` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Pembeli non-anggota',
  `buyer_phone` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `order_date` date NOT NULL,
  `total_amount` decimal(15,2) NOT NULL COMMENT 'Jumlah subtotal item',
  `paid_amount` decimal(15,2) NOT NULL COMMENT 'Jumlah transaksi POSTED dengan sales_order_id ini',
  `payment_status` int NOT NULL DEFAULT '1' COMMENT '1=BELUM_BAYAR, 2=SEBAGIAN, 3=LUNAS',
  `fulfillment_status` int NOT NULL DEFAULT '1' COMMENT '1=DIPESAN, 2=SIAP_DIAMBIL, 3=SELESAI, 4=BATAL',
  `pic_member_id` int DEFAULT NULL COMMENT 'Penjual / penanggung jawab',
  `notes` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_sales_order_number` (`order_number`),
  KEY `fk_sales_activity` (`activity_id`),
  KEY `fk_sales_buyer` (`buyer_member_id`),
  KEY `fk_sales_period` (`period_id`),
  KEY `fk_sales_pic` (`pic_member_id`),
  KEY `ix_sales_status` (`payment_status`,`fulfillment_status`),
  CONSTRAINT `fk_sales_activity` FOREIGN KEY (`activity_id`) REFERENCES `t_activity` (`id`),
  CONSTRAINT `fk_sales_buyer` FOREIGN KEY (`buyer_member_id`) REFERENCES `m_member` (`id`),
  CONSTRAINT `fk_sales_period` FOREIGN KEY (`period_id`) REFERENCES `m_management_period` (`id`),
  CONSTRAINT `fk_sales_pic` FOREIGN KEY (`pic_member_id`) REFERENCES `m_member` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_sales_order_item` (
  `id` int NOT NULL,
  `order_id` int NOT NULL,
  `product_id` int NOT NULL,
  `variant_id` int DEFAULT NULL,
  `quantity` int NOT NULL,
  `unit_price` decimal(15,2) NOT NULL COMMENT 'Harga saat dipesan',
  `subtotal` decimal(15,2) NOT NULL,
  `notes` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'mis. nama sablon',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_salesitem_order` (`order_id`),
  KEY `fk_salesitem_product` (`product_id`),
  KEY `fk_salesitem_variant` (`variant_id`),
  CONSTRAINT `fk_salesitem_order` FOREIGN KEY (`order_id`) REFERENCES `t_sales_order` (`id`),
  CONSTRAINT `fk_salesitem_product` FOREIGN KEY (`product_id`) REFERENCES `m_product` (`id`),
  CONSTRAINT `fk_salesitem_variant` FOREIGN KEY (`variant_id`) REFERENCES `m_product_variant` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_scheduled_job_log` (
  `id` int NOT NULL,
  `job_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'mis. SEND_OUTBOX, REMIND_CHECKLIST, EXPIRE_MOU, CLOSE_ATTENDANCE',
  `started_at` datetime NOT NULL,
  `finished_at` datetime DEFAULT NULL,
  `status` int NOT NULL DEFAULT '1' COMMENT '1=BERJALAN, 2=SUKSES, 3=GAGAL',
  `processed_count` int NOT NULL,
  `message` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `ix_joblog` (`job_name`,`started_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_sponsorship` (
  `id` int NOT NULL,
  `sponsorship_number` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'SPN-{org}-{yyyy}-{seq}',
  `external_party_id` int NOT NULL,
  `contact_id` int DEFAULT NULL,
  `package_id` int DEFAULT NULL COMMENT 'Paket yang dipilih (NULL = kesepakatan khusus)',
  `package_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Salinan nama paket saat dicatat',
  `period_id` int DEFAULT NULL,
  `activity_id` int DEFAULT NULL COMMENT 'Kegiatan yang disponsori (opsional)',
  `sponsorship_form` int NOT NULL DEFAULT '1' COMMENT '1=DANA, 2=BARANG, 3=JASA, 4=MEDIA',
  `amount` decimal(15,2) NOT NULL COMMENT 'Nilai sponsorship yang disepakati',
  `in_kind_description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci COMMENT 'Rincian jika berupa barang/jasa',
  `start_date` date DEFAULT NULL,
  `end_date` date DEFAULT NULL,
  `pic_member_id` int DEFAULT NULL,
  `status` int NOT NULL DEFAULT '1' COMMENT '1=AKTIF, 2=SELESAI, 3=BATAL',
  `notes` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ins_date` datetime NOT NULL,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_sponsorship_number` (`sponsorship_number`),
  KEY `fk_spn_activity` (`activity_id`),
  KEY `fk_spn_contact` (`contact_id`),
  KEY `fk_spn_party` (`external_party_id`),
  KEY `fk_spn_period` (`period_id`),
  KEY `fk_spn_pic` (`pic_member_id`),
  KEY `fk_t_sponsorship_package_id` (`package_id`),
  CONSTRAINT `fk_spn_activity` FOREIGN KEY (`activity_id`) REFERENCES `t_activity` (`id`),
  CONSTRAINT `fk_spn_contact` FOREIGN KEY (`contact_id`) REFERENCES `m_external_party_contact` (`id`),
  CONSTRAINT `fk_spn_party` FOREIGN KEY (`external_party_id`) REFERENCES `m_external_party` (`id`),
  CONSTRAINT `fk_spn_period` FOREIGN KEY (`period_id`) REFERENCES `m_management_period` (`id`),
  CONSTRAINT `fk_spn_pic` FOREIGN KEY (`pic_member_id`) REFERENCES `m_member` (`id`),
  CONSTRAINT `fk_t_sponsorship_package_id` FOREIGN KEY (`package_id`) REFERENCES `m_sponsorship_package` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Sponsorship: brand memilih paket';
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_sponsorship_benefit` (
  `id` int NOT NULL,
  `sponsorship_id` int NOT NULL,
  `title` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'Logo di banner, booth, adlibs MC, posting Instagram, dll.',
  `description` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `seq_no` int NOT NULL DEFAULT '1' COMMENT 'Urutan benefit',
  `is_done` int NOT NULL COMMENT '1=sudah diberikan ke sponsor',
  `done_at` datetime DEFAULT NULL COMMENT 'Kapan dicentang',
  `done_by` int DEFAULT NULL COMMENT 'User yang mencentang',
  `proof_file` mediumblob COMMENT 'Bukti, mis. foto banner atau screenshot postingan',
  `proof_file_name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `proof_content_type` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `notes` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_benefit_spn` (`sponsorship_id`),
  KEY `fk_t_sponsorship_benefit_done_by` (`done_by`),
  CONSTRAINT `fk_benefit_spn` FOREIGN KEY (`sponsorship_id`) REFERENCES `t_sponsorship` (`id`),
  CONSTRAINT `fk_t_sponsorship_benefit_done_by` FOREIGN KEY (`done_by`) REFERENCES `m_user` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Checklist benefit per sponsorship';
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_sponsorship_mou` (
  `id` int NOT NULL,
  `sponsorship_id` int NOT NULL,
  `mou_number` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'Nomor MoU/PKS',
  `signed_date` date DEFAULT NULL,
  `start_date` date DEFAULT NULL,
  `end_date` date DEFAULT NULL,
  `document` longblob COMMENT 'File MoU',
  `document_file_name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `document_content_type` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `notes` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ins_date` datetime NOT NULL,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_sponsorship_mou_number` (`mou_number`),
  KEY `fk_t_sponsorship_mou_sponsorship_id` (`sponsorship_id`),
  CONSTRAINT `fk_t_sponsorship_mou_sponsorship_id` FOREIGN KEY (`sponsorship_id`) REFERENCES `t_sponsorship` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='MoU sponsorship';
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_timeline_plan` (
  `id` int NOT NULL,
  `period_id` int DEFAULT NULL,
  `title` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `description` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `plan_type` int NOT NULL DEFAULT '1' COMMENT '1=EVENT, 2=RAPAT, 3=LATIHAN, 4=PERTEMUAN, 5=DEADLINE, 6=LAINNYA',
  `plan_year` smallint NOT NULL,
  `plan_month` int NOT NULL COMMENT '1-12',
  `start_date` date DEFAULT NULL COMMENT 'NULL = tanggal belum pasti',
  `end_date` date DEFAULT NULL,
  `status` int NOT NULL DEFAULT '1' COMMENT '1=RENCANA, 2=TERJADWAL, 3=SELESAI, 4=BATAL',
  `activity_id` int DEFAULT NULL COMMENT 'Terisi saat rencana dijadikan kegiatan',
  `program_id` int DEFAULT NULL COMMENT 'Program kerja terkait',
  `pic_member_id` int DEFAULT NULL COMMENT 'Penanggung jawab',
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_plan_activity` (`activity_id`),
  KEY `fk_plan_period` (`period_id`),
  KEY `fk_plan_pic` (`pic_member_id`),
  KEY `fk_plan_program` (`program_id`),
  KEY `ix_plan_org_year` (`plan_year`,`plan_month`),
  CONSTRAINT `fk_plan_activity` FOREIGN KEY (`activity_id`) REFERENCES `t_activity` (`id`),
  CONSTRAINT `fk_plan_period` FOREIGN KEY (`period_id`) REFERENCES `m_management_period` (`id`),
  CONSTRAINT `fk_plan_pic` FOREIGN KEY (`pic_member_id`) REFERENCES `m_member` (`id`),
  CONSTRAINT `fk_plan_program` FOREIGN KEY (`program_id`) REFERENCES `t_program` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_user_token` (
  `id` int NOT NULL,
  `user_id` int NOT NULL,
  `token_type` int NOT NULL COMMENT '1=RESET_PASSWORD, 2=VERIFY_EMAIL, 3=REFRESH',
  `token_hash` char(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'Simpan hash (SHA-256), bukan token mentah',
  `expires_at` datetime NOT NULL,
  `used_at` datetime DEFAULT NULL,
  `ip_address` varchar(45) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `user_agent` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ins_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ins_by` int DEFAULT NULL,
  `upd_date` datetime DEFAULT NULL,
  `upd_by` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_token_hash` (`token_hash`),
  KEY `ix_token_user` (`user_id`,`token_type`),
  CONSTRAINT `fk_token_user` FOREIGN KEY (`user_id`) REFERENCES `m_user` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

