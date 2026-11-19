# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

GitaWidyaAgni is a .NET 8 backend library for managing **a single organization** (members, management periods, programs/activities, attendance, finance, letters, e-voting, procurement, sponsorship, inventory, …). There is no tenant, campus, UKM category or staff: the organization is the top level and no table carries `tenant_id` / `campus_id` / `organization_id`. Domain terms, comments and user-facing messages are in Indonesian.

The structure is copied from the author's ClubSphere project (`C:\Users\Royan\source\repos\ClubSphere`, see its CLAUDE.md). Follow its conventions, but drop every multi-tenant / campus / UKM rule.

- **GWAShared**: dependency-free helpers (`Results/`, `Enums/`, `Extensions/`, `Utilities/`, `Models/`).
- **GWALib**: data access and business layer, referencing GWAShared. `EF/` holds the database-first EF Core model (entities + `ModelContext`), scaffolded with Pomelo. Re-scaffolding overwrites these files: keep `OnConfiguring` commented out so the connection string is never committed.
- **GitaWidyaAgniWeb**: ASP.NET Core 8 MVC app (Velzon template) for MB Gita Widya Agni UPN "Veteran" Jawa Timur. It has a public landing page, login, the dashboard shell, a management page for every sidebar menu, and the Area Anggota self-service pages (see "Web pages").
- **GWASeeder**: console that fills every table with sample data through GWALib.

## GWALib

- `General/` was ported from ClubLib: `BaseLib`, `DBClass` (`Configure` once, then `GetContext()`), `ServiceHandler` (an `ArgumentException` is a user-facing error), `Utilities/ImageUtility`, `ValidateUtility` and `ValueUtility`. `Lib/Transaksi/TransaksiHelper` was ported too. ClubSphere's CLAUDE.md "Data access & service pattern" and "Service layer" sections describe the class pattern (`<Name>.cs` + `<Name>.Services.cs`, SYNC/ASYNC/HELPER regions, `NewRow`, `GetDataList` / `Save` / `Update` / `Delete`, `CreateRow`, parent checks on the parent class). Follow them, minus every tenant / campus / organization rule.
- **Primary key ids come from `m_sequence`.** Use `SequenceUtility.GetNextId<EF.X>(context)` for one id, or `ReserveIds<EF.X>(context, n)` for a block, which returns the first id.
  - The increment is an atomic `UPDATE ... LAST_INSERT_ID(last_id + n)` that runs immediately, not at `SaveChanges`. Outside a transaction, a failed save leaves a gap. Inside the caller's `BeginTransaction` it rolls back.
  - A missing `m_sequence` row is created from `MAX(id)`.
  - It rejects `t_election_vote` (random id) and tables without a single int key.
  - `ValueUtility.GetNextId` (max + 1, with a filter) is only for per-parent order columns in composite keys, such as `m_asset_photo.seq_photo` and `step_level`, never for ids. A single-column int key, even one named `seq_photo` (`m_organization_photo`), comes from `m_sequence`.
- `General/ModelContextDefaults.cs` (partial `OnModelCreatingPartial`, outside the scaffolded files) removes the SQL default from every non-nullable numeric column. EF Core 7 otherwise treats 0 as "not set" and silently inserts the database DEFAULT instead, for example `is_active = 1` or `allow_revision = 1`. Because of this, `CreateRow` must set every numeric column explicitly.
- The `Akses/Sequence` master refuses a `LastId` below the table's current `MAX(id)` (`SequenceUtility.GetMaxId`).
- Done so far:
  - `Lib/Master/Menu/` (controller, controller action, header menu, header menu detail, sub menu). `m_sub_menu_group` no longer exists, so header menu details have no group.
  - `Lib/Master/Akses/` (user account, role, role action, role menu, user role, sequence). Staff, staff position and work unit are gone.
  - `Lib/Master/Organization/`:
    - `OrganizationProfile` is the single `m_organization` row. `GetData()` returns null until it is filled. `Save` is refused once a row exists, and there is no delete.
    - The profile query skips both blobs and exposes only `HasLogo` / `AdvisorSkFileName`. Fetch the files with `GetLogo` / `GetAdvisorSkFile`, and change them with `UpdateLogo` / `UpdateAdvisorSkFile`, where null removes the file.
    - Advisor data is plain columns on the profile; there is no advisor table or account.
    - `OrganizationPhoto` is keyed by `SeqPhoto` alone.
  - `Lib/Master/Approval/`:
    - `ApprovalFlow`: one active flow per document type (`GetEffective(documentType)`). It can be saved together with its steps. An active flow needs at least one step.
    - `ApprovalFlowStep`: approver Role → `RoleId`, Jabatan → `PositionId`, UserTertentu → `UserId`. The unused approver columns are cleared.
    - `DocumentNumberFormat`: one format per document type. Placeholders are `{seq} {type} {org} {month_roman} {month} {year} {yy}`. `{org}` is filled from `m_organization.code` (`OrganizationProfile.GetCode`, uppercase letters and digits).
    - `MessageTemplate`: one template per code and channel.
    - `UserNotificationPref`.
    - Type and template codes are normalised with `DocumentNumberFormat.ToTypeCode` (e.g. `surat und` → `SURAT_UND`).
  - `Lib/Master/Keuangan/`:
    - `FinancialAccount`: the name is unique. A bank account needs a bank name and an account number, and an e-wallet needs an account number. Cash accounts clear these fields. The opening balance is locked once the account has transactions or period balances.
    - `FinancialCategory`: the name is unique within its type, and the type is fixed. A category used by an active dues setting cannot be deactivated.
    - `DuesSetting`: the category must be Income. Period and frequency are locked once bills exist.
  - `Lib/Master/Mitra/`:
    - `ExternalParty` and `ExternalPartyContact` are ported from ClubLib `Sponsorship/Partner` and `PartnerContact`. The `party_type` column maps to the property `PartyType` of enum `ExternalPartyType`.
    - Party names are unique. Lists do not load the logo blob (`GetLogo` / `UpdateLogo`). A party can be saved together with its contacts.
    - `internal GetActiveExternalParty` rejects a Blacklist party with its own message.
    - Each party has at most one active primary contact. Making a contact primary demotes the previous one.
  - `Lib/Master/Sponsorship/`:
    - These classes are new; ClubLib has no equivalent.
    - `SponsorshipPackage` has a unique name, a price of at least 0, and `SeqNo` for display order (0 = append). Delete is soft. Use `internal GetPackage` (any status) or `GetActivePackage`. A package can be saved together with its benefits.
    - `PackageBenefit` (`m_sponsorship_package_benefit`) is a catalog only and is hard-deleted.
  - `Lib/Master/Merchandise/`:
    - `Product`: the name is unique, and lists do not load the photo blob.
    - Initial stock is written as a `t_product_stock_movement` adjustment ("Stok awal"). After that, stock changes only through stock movements.
    - A product with variants keeps its stock on the variants, and a variant can only be added while the product's own stock is 0.
    - The pre-order close time must be in the future. `activity_id` only needs to exist.
    - `ProductVariant`: the name is unique per product and the SKU is unique overall.
  - `Lib/Master/Inventaris/`:
    - `AssetCategory`, `Asset` and `AssetPhoto` (composite key) are ported from ClubLib.
    - `Asset` adds `TrackUnit`, `UnitPrefix`, `RentalFeePerDay` and `LateFeePerDay`. `TrackUnit` and `UnitPrefix` are fixed at creation. The prefix is required for per-unit assets, uppercase letters and digits only, and unique.
    - `AssetUnit` (`m_asset_unit`, new):
      - Codes are the prefix plus a 3-digit number, e.g. `HT001`. Numbers continue after deleted units and are never reused.
      - A per-unit asset creates `Quantity` units on save. Its `Quantity` is the count of units that are not deleted.
      - Adding or deleting a unit updates `Quantity` and writes a `t_asset_movement` row with `asset_unit_id`.
      - A unit cannot be deleted while borrowed or under maintenance. Disposing the asset deletes its remaining units.
      - Each unit has a random `qr_token` (16 bytes, 32 upper-case hex; migration `2026-11-18_asset_qr_token.sql`) set on create. The QR holds the token, not the unit code. Use `GetByQrToken` (public page), `GetByCodeOrToken` (internal scans), `GetByCode` for typed codes, and `MarkLabelPrinted` after printing.
  - `Lib/Master/Surat/`:
    - `DocumentCategory` has a unique name.
    - `LetterTemplate` has a unique name. `LetterType` is normalised to a type code (`ToTypeCode`). It needs an HTML body, a `.docx` file of at most 5 MB, or both. Lists do not load the file blob (`GetFile` / `UpdateFile`). `GetLetterTypes()` returns the letter types in use.
  - `Lib/Master/TimPelaksana/`:
    - `ChecklistTemplate` has a unique name and no campus or organization scope. It can be saved together with its items. `GetAvailableList(activityType)` returns active templates for that activity type plus templates without a type, type-specific ones first.
    - `ChecklistTemplateItem`: `OffsetDay` is within ±365 and `SeqNo` 0 means append.
  - `Lib/Master/Prestasi/PointRule` (new):
    - `ActivityType` applies only to Hadir, Panitia and Pemateri. `AchievementLevel` applies only to Prestasi. Fields that do not apply are cleared.
    - Only one active rule may exist per combination of source, activity type and level. Points must not be 0.
    - `GetEffective(source, activityType, level)` and `internal GetEffectiveRow` prefer the most specific rule and fall back to the general rule (NULL columns).
  - `Lib/Master/Information/SiteSetting`:
    - The setting key is unique and normalised to snake_case. The group is an uppercase code.
    - Lists do not load the image blob (`GetImage` / `UpdateImage`). `GetByKey(key)` is used by the landing page.
  - `UserRole.PeriodId` NULL means the role is not tied to a management period. Otherwise the period is checked with `ManagementPeriod.ValidateOpenPeriod`.
  - `Lib/Master/Anggota/`:
    - `ManagementPeriod`: at most one Active period (`GetActive()`). A Closed period is locked. Only a Draft period that no other table uses can be deleted. `internal ValidateOpenPeriod(context, periodId)` returns the row and is the check for other classes.
    - `Member`: NIM, member number (stored upper case) and user are each unique globally. `MemberType` and `JoinedAt` / `LeftAt` are new compared with ClubLib. Delete sets the member Inactive and fills `LeftAt`. Use `internal GetMember` (any status) or `GetActiveMember`.
    - `MemberPhoto` and `AlumniProfile`: alumni profiles are only for members with status Alumni.
    - `Division` and `Position`: names are unique and delete is soft (`is_active`). Use `internal ValidateActiveDivision` / `ValidateActivePosition`.
    - `Section` (`m_section`, migration `2026-11-18_section.sql`, EF entities written by hand): the music sections (Brass, Battery, Front Ensemble, Colour Guard). **Every member has a section; a division (`Division`) holds only members picked by the Ketua Umum** (Humas, Logistik, Danus, …). The name is unique and delete is soft, refused while the section has current members. `GetMemberCounts()` counts active members per current section. `internal ValidateActiveSection`.
    - `Member.NewRow.SectionId` is required on create: `CreateRow` adds the first `t_section_member` row (start = `JoinedAt` or today). `Member` exposes `CurrentSectionId` / `CurrentSectionName` (filtered include of the open history row).
- All `m_*` master tables now have a service class under `Lib/Master/`. Transactions go under `Lib/Transaksi/<Module>/`.
- `Lib/Transaksi/Approval/` (ported from ClubLib):
  - `DocumentNumberSeq`:
    - `GenerateNumber(type, date)` has a public version and an `internal` overload that takes a context. The overload runs inside the caller's `BeginTransaction`, so a rolled-back document does not use up a number.
    - The increment is an atomic `UPDATE ... LAST_INSERT_ID`. A new (type, period) row gets its id from `m_sequence`.
    - `{org}` requires the organization profile only when the format uses it.
    - `UpdateLastNumber` can only raise the number.
  - `MessageOutbox`:
    - Email and WhatsApp only. WhatsApp numbers are stored as `62…` digits.
    - `SaveFromTemplate` skips the message (Data null) when the user disabled that channel.
    - After 5 failed attempts the message becomes Gagal, and `Retry` resets it. The body of `RESET_PASSWORD` messages is masked once sent.
  - `ScheduledJobLog`: `Start` / `Finish` / `Fail`. A second run of the same job is refused, and a stale Berjalan row is closed as Gagal.
- `Lib/Transaksi/Akses/UserToken` (new):
  - Only the SHA-256 hash is stored. The raw token (256-bit, base64url) is returned once by `Issue` / `Rotate` as `IssuedToken`.
  - `Issue` needs no login, so it works for forgot-password. Issuing a reset or verify token revokes older unused tokens of the same type. Default lifetimes are reset 60 minutes, verify 24 hours and refresh 30 days.
  - `ResetPassword` uses the token and sets the password (`UserAccount.SetRowPassword`, now internal) in one save, and revokes the user's other tokens.
  - `Rotate` rotates refresh tokens. `RevokeAll` and `Purge` are also available.
  - Invalid, used, expired and inactive-user tokens all return the same `TOKEN_INVALID` message.
  - Revoking a token also sets `used_at`.
- `Lib/Transaksi/Anggota/` (all new):
  - `DivisionMember`:
    - Unique per (division, member, period), with NULL period checked in code.
    - The role is an uppercase code. Delete sets the row inactive and fills `left_at`.
  - `SectionMember` (`t_section_member`): section history. The open row (`end_date` NULL) is the current section. `Move(memberId, sectionId, startDate, note)` closes the current row on `startDate − 1` and opens a new one (same start date = correction of the current row; a start before the current one is refused), so members can switch section per event. `Update` changes only the note. `Delete` removes a wrong row and reopens the previous one; the only row of an Active member cannot be deleted. `internal MemberIdsAt(context, sectionId, date)` gives the members in a section on a date.
  - `ManagementAssignment`:
    - A core position (`is_core`) can have only one current holder per (period, position, division, section).
    - `SectionId` (`t_management_assignment.section_id`) marks a Section Leader assignment. An assignment leads a division or a section, never both.
    - Assigning a member whose type is Anggota changes the type to Pengurus.
    - `End` closes an assignment, and an ended assignment is locked. Delete is refused when a certificate uses the assignment.
    - `internal EndAllInPeriod` is used by the period handover.
  - `Recruitment`:
    - Draft → Open → Closed. A closed recruitment can be reopened if its close date is in the future.
    - `GetOpenList` lists recruitments open to the public.
    - The quota limits accepted applicants. Only a Draft without applicants or forms can be deleted.
  - `RecruitmentApplicant`:
    - `Apply` needs no login. It requires status Open within the date range. Umum applicants are allowed only if `accept_public`. NIM and email are unique per recruitment. The applicant is linked to an existing member by NIM or email.
    - `Review` moves the status forward only; Accepted and Rejected are final.
    - Accepted checks the quota and creates an `m_member` through `Member.CreateRow`, which is now internal. `Review(status, notes, user, sectionId)`: the section is required when a new member is created.
  - `PeriodHandover` and `PeriodHandoverItem`:
    - One handover per source period. Draft → Diajukan → Disetujui / Ditolak, and Ditolak can be edited and resubmitted.
    - Data, items and the LPJ file can be changed only in Draft or Ditolak.
    - `Approve` runs in one DB transaction. It closes the source period, activates the target period, ends the source period's assignments, and generates the number from format `BA_ST` when it is empty and the format exists.
    - Item `RefId` is checked against the table for its `ItemType`. `MouAktif` maps to `t_sponsorship_mou`.
- `Lib/Transaksi/Program/` (all new):
  - `WorkProgram`:
    - Maps `t_program`. The class name avoids clashing with the namespace.
    - Planned → Ongoing → Done, and Planned/Ongoing → Cancelled.
    - `DivisionId` (bidang, active division, checked only when changed) and `Term` (`ProgramTerm`: Jangka Pendek / Panjang) are optional and group the program evaluation in the LPJ kepengurusan.
    - The previous program must come from another period. Delete is allowed only for a Planned program that nothing uses.
    - `internal GetOpenProgram` is available to other classes.
  - `ProgramTask`:
    - A task on a program takes the program's period; a general task needs `PeriodId`.
    - Belum → Proses → Selesai, and Selesai can be reopened to Proses.
    - Selesai records `completed_at` and `done_by`.
  - `Activity`:
    - The parent activity may be only one level deep.
    - Lomba requires `CompetitionId`, audience Divisi requires `DivisionId`, audience Section (6, sectional practice) requires `SectionId` (`t_activity.section_id`), and audience TimPelaksana requires a parent. Fields that do not apply are cleared.
    - Featured requires public. `GetPublicList` serves the landing page.
    - Delete is allowed only for a Planned activity with no dependents. `internal GetActivity` and `GetOpenActivity` are available.
  - `ActivityCollaborator`: `Respond` moves Diundang to Diterima or Ditolak. Blacklisted parties are refused.
  - `ActivityParticipant`: unique per activity. Registering again after Cancelled reactivates the old row.
  - `ActivityAbsenceRequest` (`t_activity_absence_request`, migration `2026-11-18_activity_absence.sql`, entity written by hand): members of an activity audience other than Peserta Terdaftar (Semua Anggota, Pengurus, Divisi, Section, Tim Pelaksana — e.g. latihan rutin) are **automatically** in it and never register. They request not to attend one session (`SessionId`) or the whole activity (`SessionId` NULL) with a reason (`Submit`, by the logged-in member). Menunggu → Disetujui / Ditolak (note required) by staff, Menunggu → Ditarik by the member. No duplicate open request per session or whole activity. Approved whole activity = removed from `GetExpectedMemberIds` (no Alpa); approved session = recorded Izin when the session closes (an existing Alpa without check-in becomes Izin on approval). Check-in still accepts them (`GetExpectedMemberIds(..., excludeAbsent: false)`). Decisions notify the member (`IZIN_KEGIATAN`). `GetAutoActivityIds(memberId, ids)` lists the activities a member follows automatically.
  - `ActivityReport`:
    - Needs an activity or a program; the program is filled from the activity.
    - Draft → Submitted → Approved, and Submitted → Revision. The file is not loaded in lists.
  - `Gallery`:
    - Draft → Published → Archived. Publishing needs at least one photo, and only a Draft can be deleted.
    - Photos are removed in the same save as the gallery, using stub entities that carry `GalleryId` so EF deletes them first.
  - `GalleryPhoto`: the first photo becomes the cover, `SetCover` moves it, and deleting the cover passes it to the next photo.
  - `TimelinePlan`:
    - `StartDate` sets `PlanYear` and `PlanMonth`.
    - `LinkActivity` sets status Terjadwal and copies the activity's dates and program.
  - `Lpj` (`t_lpj`, added 2026-11-17 with `t_program.division_id` / `term`; DDL in `database/migrations/2026-11-17_lpj.sql`, EF entity written by hand to match):
    - LPJ Kegiatan (one per parent activity; sub-activities are refused and reported inside the parent) or LPJ Kepengurusan (one per period).
    - The table holds only the narrative sections (introduction, background, activity kind, purpose, activity form, vision/mission, closing). Empty sections are pre-filled on create from the organization profile (description, vision, mission), the activity description and the sub-activity titles, plus a standard closing.
    - Draft → Final (`Finalize(pdf)` stores the PDF; a kepengurusan LPJ also sets `t_period_handover.lpj_document` of the handover from that period when it is Draft / Ditolak, reported as `HandoverAttached`) → Draft (`Reopen` drops the PDF). Update and delete only while Draft. Lists skip the document blob (`GetDocument`).
    - `GetKegiatanData()` (parent + sub-activities): schedules (activity dates and active weekly schedules), results (achievements of the linked competitions), up to 12 gallery photos, budget lines (Posted transactions of the activities, their sponsorships and funding requests; expenses with a budget item show the RAB line qty × unit price next to the realized sum), signers (Ketua Pelaksana, Bendahara, Ketua Umum).
    - `GetKepengurusanData()`: programs of the period by bidang and jangka, parent activities of type Event / Lomba / Lainnya in the period, non-disposed assets with condition counts, assignments of the period (position, name, NIA, faculty / study program, latest photo; no address or phone), up to 24 gallery photos, signers (Ketua Umum, Sekretaris, Pembina from the profile).
- `Lib/Transaksi/TimPelaksana/` (all new):
  - `CommitteeSection`:
    - The name is unique per activity. Delete is refused when members, rundown, checklist or applications use the section.
    - `internal GetSection(context, sectionId, activityId)` checks that the section belongs to the activity.
  - `CommitteeMember`:
    - One row per activity and member, and re-adding a member who resigned or was removed reactivates the row.
    - Core roles (Ketua Pelaksana, Wakil, Sekretaris, Bendahara) have no section and one holder per activity.
    - Koordinator Sie and Staf require a section. Koordinator Sie has one holder per section and is synced to `t_committee_section.coordinator_member_id`.
    - `internal AddRow` is used by `CommitteeApplication.Accept`.
  - `CommitteeRecruitment`:
    - One per activity. Draft → Buka → Tutup, with the same reopen rule as `Recruitment`.
    - The quota limits accepted applications.
  - `CommitteeApplication`:
    - `Apply` requires the recruitment open and in range, both sections in the activity, and the member not already active on the committee. Re-applying after Mundur reactivates the row.
    - `Accept(sectionId?)` checks the quota and adds the member as Staf, using choice 1 when no section is given.
    - `Reject` and `Withdraw` are also available.
  - `ActivityRundown`: sorted by date, start time, then `SeqNo`.
  - `ActivityChecklist`:
    - Same status flow as `ProgramTask`.
    - `GenerateFromTemplate` sets the due date to the activity start date plus `OffsetDay` and matches the section by name, case-insensitive.
    - Template items that were already generated are skipped (`uq_checklist_template_item`).
  - `ActivityTripMember`:
    - Either a member (unique per activity) or a guest name is required.
    - The permission file is not loaded in lists. Uploading it sets status Ada, and removing it sets Belum.
- `Lib/Transaksi/Presensi/` (all new):
  - `GeoFence` (internal): validates a point plus radius, and checks the position with the haversine formula. A session without a point and radius skips the location check.
  - `ActivitySchedule`:
    - A weekly schedule using `ScheduleDay` (1 = Monday). Delete sets it inactive.
    - `GenerateSessions(from, to, activityId?)` creates one session per schedule per matching date within `valid_from`..`valid_until`, for Planned or Ongoing activities only. Sessions that already exist are skipped. Location and tolerance are copied to the session.
  - `AttendanceSession`:
    - Scheduled → Open → Closed, and Scheduled/Open → Cancelled with a reason.
    - `Open` creates a new random `qr_token`; calling it again rotates the token.
    - `Close` clears the token and records Alpa for expected members with no record.
    - Expected members come from the activity audience through `internal GetExpectedMemberIds(context, activity, sessionOpenAt)`; audience Section uses the section history on the session date.
    - A session cannot be cancelled once someone has checked in.
  - `AttendanceRecord`:
    - `CheckIn` / `CheckOut` take the QR token from the logged-in user's member. The session must be Open and within its time window. The member must be in the audience, unless the audience is all members.
    - The geofence is checked, and a photo is required (`ImageUtility`), stored in `t_attendance_record_in` / `_out`. Check-in after `open_at` plus tolerance is Terlambat.
    - `SetManual` lets staff record Izin, Sakit, Alpa or Hadir and correct automatic Alpa. A row that has a check-in can only be Hadir or Terlambat.
  - `MeetingMinutes`:
    - One per Open or Closed session. Draft → Final, and Final is locked.
    - The attachment is not loaded in lists. Filtering by activity happens before the projection (`Project`).
- Shared transaction helpers:
  - `TransaksiHelper.GenerateNumber(context, type, date, fallbackPrefix, id, userId)` uses `DocumentNumberSeq` when an active format exists for the type, otherwise `PREFIX/yyyyMMdd/id`. Call it inside `BeginTransaction`.
  - `TransaksiHelper` also has `ToNullable`, `AppendNote` (adds a reason to a text column, truncated), `ValidateFile` and `ValidatePositive`.
  - `Approval/ApprovalHelper` (internal): `GetActiveSteps(context, documentType)` returns the active flow's steps, or fails when there is no flow or no step. `IsApprover` / `ValidateApprover` check one step: Role means the user has that role with no period or an Active period; Jabatan means the user's member currently holds that position in an Active period; UserTertentu means that user. Reuse it for Pengadaan and Surat.
- `Lib/Transaksi/Keuangan/` (all new):
  - `FinancialTransaction`:
    - Draft → Posted → Void, and Draft → Void. Only a Draft can be edited or deleted. The number uses format `FIN_TRX` (fallback `TRX`).
    - The category type must equal the transaction type. An empty period means the Active period.
    - A closed `t_period_balance` for (period, account) blocks create, post and void (`internal ValidateBalanceOpen`).
    - `FundingRequestId` is set only by `FundingRequest`, `ProcurementId` only by `ProcurementRequest.RecordPayment`, `SalesOrderId` only by `SalesOrder.ReceivePayment`, and `BorrowRequestId` only by `BorrowRequest.ReceivePayment`. `Save` refuses the last three. A transaction linked to a funding request, procurement, sales order, borrow request, maintenance, dues bill, debt or debt payment can only be voided through that document.
    - `internal CreateRow(context, newrow, fundingRequestId, posted, userId)` is used by the other documents, which create Posted transactions. `internal VoidRow` voids without the link check. `OpenPeriodOrDefault` keeps a document's period unless it is Closed.
  - `FinancialAttachment`, `BudgetDocument` (10 MB) and `DebtAttachment`: file blobs, at most 5 MB unless noted, not loaded in lists (`GetFile`). Financial attachments are refused on a Void transaction.
  - `Budget` and `BudgetItem`:
    - Draft → Submitted → Approved / Rejected, and Rejected can be edited and resubmitted. Submitting needs at least one item.
    - Items change only in Draft / Rejected. `Amount` = quantity × unit price. `TotalAmount` is recalculated on every item change (`internal RecalculateTotal`, which loads the items and sums the context's Local view).
    - A program on an item must belong to the budget's period. `internal GetApprovedItem` is the check for transactions and funding requests.
    - `BudgetItem.RealizedAmount` (lists only) = Posted expenses minus Posted income that use the item.
    - Approve / Reject records the decider in `approved_by`. There is no column for a rejection note.
  - `DuesBill`:
    - `Generate(settingId, billPeriod, dueDate, memberIds?)` bills Active members (or the given ones) that have no bill for that period yet, and copies the amount. The period format follows the frequency: `yyyy-MM`, `yyyy-GANJIL` / `yyyy-GENAP`, `yyyy`, or free text for Once (default `SEKALI`).
    - `Pay(accountId, date, categoryId?)` creates a Posted Income transaction (the category defaults to the setting's). `CancelPayment` voids it and returns to Unpaid. `Waive` / `Unwaive` are also available.
    - Only an Unpaid bill can be edited or deleted.
  - `FundingRequest`, `FundingApproval` and `FundingRealization`:
    - Draft → Submitted → Approved → Disbursed, Submitted → Rejected (final), and Submitted → Draft on a revision request. The number uses format `FUNDING_REQUEST` (fallback `PD`).
    - The period defaults to the budget item's period, then the Active period. The program defaults to the budget item's or the activity's program.
    - Approval follows the active `FUNDING_REQUEST` flow. The current step is the lowest step level not Approved in this round. `RequestRevision` needs `allow_revision`.
    - `approver_user_id` is NOT NULL, so a `t_funding_approval` row is written when someone decides, with that user as approver. Resubmitting sets earlier decisions back to Pending and keeps the approver and note as history.
    - `Disburse` creates a Posted Expense transaction. Realizations can change only while Disbursed with settlement Belum, and they recalculate `RealizedAmount`. The receipt is optional (5 MB, `GetReceipt` / `UpdateReceipt`).
    - Settlement Belum → Diajukan → Selesai, and `ReturnSettlement` goes back to Belum. `Settle` creates Income for unused money (`ReturnedAmount`) or Expense for overspending, and then needs an account and category.
  - `PeriodBalance`:
    - `Open(periodId, accountId?)` creates rows for one account or all active accounts without one. The opening balance is the latest earlier Closed balance of the account, else the account's opening balance.
    - Totals come from Posted transactions (`Refresh`). `Close` refuses while Draft transactions exist, then fills the closing balance and locks the (period, account) transactions. `Reopen` is refused once a later period has a balance for the account. Only an Open row can be deleted.
  - `Debt` and `DebtPayment`:
    - The number uses format `DEBT` (fallback `HT`). The period is required and defaults to the Active period.
    - A cash loan (`LoanAccountId` + `LoanCategoryId`) creates a Posted Income transaction and locks the amount.
    - `DebtPayment.Save` creates a Posted Expense transaction, refuses more than the remaining amount, and recalculates `paid_amount` and the status (`internal RecalculatePaid`). `DebtPayment.Delete` voids the transaction. There is no payment update.
    - `WriteOff` sets Dihapuskan (final). A debt can be edited only while BelumBayar / Sebagian, and deleted only while BelumBayar without payments, which voids the loan transaction.
- `Lib/Transaksi/Sponsorship/` (all new):
  - `SponsorshipDeal` maps `t_sponsorship`. The class name avoids clashing with the namespace.
    - The number uses format `SPONSORSHIP` (fallback `SPN`). The party must be Active (`GetActiveExternalParty`, so Blacklist is refused) and the contact must belong to it.
    - The package is optional and fixed after creation. Its name is copied to `package_name`, an amount of 0 takes the package price, and the package benefits are copied.
    - Dana needs an amount above 0. Other forms need `InKindDescription`.
    - Aktif → Selesai (all benefits done) / Batal (no money received). It can be edited only while Aktif, and the amount cannot go below the money received.
    - `ReceivePayment` creates a Posted Income transaction with `sponsorship_id` and refuses more than the remaining amount. `ReceivedAmount` = Posted Income transactions of the sponsorship, so voiding one of them through `FinancialTransaction.Void` is allowed.
    - Delete is allowed only while Aktif without transactions, MoU or assets, and it also deletes the benefits.
  - `SponsorshipBenefit`: a checklist, `SeqNo` 0 means append. Changes need the sponsorship Aktif. `MarkDone` records time, user and an optional proof (5 MB, `GetProof` / `UpdateProof`). `MarkUndone` keeps the proof. `internal CopyFromPackage` is used on create.
  - `SponsorshipMou`: the number is unique; when empty it uses format `MOU` (fallback `MOU`). It can change while the sponsorship is Aktif or Selesai. The document is at most 10 MB (`GetDocument` / `UpdateDocument`). Delete is refused when a handover item (MouAktif) uses it. `GetActiveList(date)` lists MoUs of Aktif sponsorships that have not ended.
- `Lib/Transaksi/Pengadaan/` (all new):
  - `ProcurementRequest`:
    - The number uses format `PROCUREMENT` (fallback `PGD`). The requester defaults to the logged-in user's member. An activity or a program may be set, but not both. The period defaults to the budget item's period, then the Active period.
    - Draft → Diajukan → Disetujui / Revisi / Ditolak, and Revisi can be edited and resubmitted. Then Disetujui → Dibeli → DiterimaSebagian → Selesai, where the last two follow the receipts (`internal RecalculateReceiveStatus`).
    - It can be cancelled from Draft, Diajukan, Revisi or Disetujui, or from Dibeli when there is no payment or receipt. The reason goes to `reject_reason`. Only a Draft can be deleted, which also deletes its items and quotations (there is no cascade).
    - `Submit` needs at least one item, recalculates `TotalEstimate`, and compares it with the budget item's remainder (amount minus Posted expenses plus income). It only sets the `IsOverBudget` / `BudgetRemaining` warning and never refuses.
    - Approval follows the active `PROCUREMENT` flow. `t_procurement_approval` has no unique key, so every decision is a log row. A round restarts after the last Revisi decision, and the current step is the lowest step not Setuju in this round. The signature is optional (2 MB).
    - `Purchase(fundingRequestId?)`: the PengajuanDana method needs an Approved or Disbursed funding request that no other procurement uses.
    - `RecordPayment` is for the DibayarLangsung method only. It creates a Posted Expense transaction with `procurement_id`, budget item, activity and program. `CancelPayment(transactionId)` voids it. `TotalActual` = the procurement's Posted Expense transactions.
  - `ProcurementItem`: an asset item needs an active asset category, and the vendor must be an active party. Items change only in Draft / Revisi and recalculate `TotalEstimate`. `SetActual(price, partyId?)` works from Disetujui to DiterimaSebagian. `ReceivedQuantity` changes only through receipts.
  - `ProcurementQuotation`:
    - `ItemId` NULL means a quote for the whole request. It needs either an active party or a vendor name.
    - It can change in Draft / Diajukan / Revisi / Disetujui. `Select` keeps one selected quote per scope (the same item, or the whole request), and an item quote with a party sets the item's vendor.
    - The file is at most 5 MB.
  - `ProcurementReceipt` and `ProcurementReceiptItem`:
    - A receipt and its lines are saved in one transaction, only while the request is Dibeli / DiterimaSebagian. The received quantity cannot exceed what is left of the item.
    - `ProcurementReceiptItem.NewRow.Asset` (an `Asset.NewRow`) creates an `m_asset` for an asset item that is not Hilang, through `Asset.CreateRow`, which is now internal and takes `procurementItemId`. Quantity, source Beli and condition come from the line; the category, date and cost (actual or estimated price × quantity) are filled when empty.
    - Delete subtracts the received quantities and is refused when an asset was created for any of its items.
  - There is no product stock movement on receipt, because `t_procurement_item` has no product column. Merchandise records stock-in movements with `procurement_id`.
- `Lib/Transaksi/Merchandise/` (all new):
  - `ProductStockMovement`:
    - A stock ledger. `Quantity` is signed, and each movement changes `m_product.stock` (product without variants) or `m_product_variant.stock` at once. Stock may never go negative. Only products with `track_stock` Dilacak.
    - Manual `Save` accepts Masuk (positive, optional `ProcurementId`), Penyesuaian (signed) and RusakHilang (entered positive, stored negative). Terjual and Retur come only from `SalesOrder`. There is no update or delete; correct with Penyesuaian.
    - `internal GetSellable(context, productId, variantId)` requires an active product, and an active variant exactly when the product has variants. `internal Apply` writes the movement and the stock.
  - `SalesOrder` and `SalesOrderItem`:
    - The number uses format `DANUS` (fallback `DNS`). The buyer is a member or a buyer name; a member buyer clears the name.
    - Items (at least one) are created with the order and never edited separately; cancel and recreate instead. The price is copied (variant price, else product price). A ready-stock tracked product is deducted with a Terjual movement. A pre-order product must still be open and is not deducted.
    - Fulfillment Dipesan → SiapDiambil → Selesai (requires Lunas). Dipesan / SiapDiambil → Batal is refused once money was received, and returns the stock with Retur movements.
    - `ReceivePayment` creates a Posted Income transaction with `sales_order_id`, and refuses more than the remainder. `CancelPayment(transactionId)` voids it. `PaidAmount` / `PaymentStatus` come from Posted Income; a total of 0 is Lunas.
    - `SaveGuest(GuestRow)` needs no login (public shop): name and WhatsApp number (at least 8 digits) are required, at most `GUEST_MAX_LINES` (10) lines and `GUEST_MAX_QUANTITY` (20) per line, the date is today, and `ins_by` stays NULL. To allow this, the user id is `int?` along the chain (`TransaksiHelper.GenerateNumber`, `DocumentNumberSeq.GenerateNumber`, `SalesOrderItem.CreateRow`, `ProductStockMovement.Apply`).
    - `FindGuestOrder(number, phone)` looks up a guest order (no member buyer) by number plus phone; phones are compared as digits with a leading `0` read as `62`.
- `Lib/Transaksi/Inventaris/` (all new):
  - `InventarisHelper` (internal):
    - `GetOutstanding` returns the quantity of an asset still out on Borrowed / Overdue requests. It loads into the context and reads Local, so status changes in the same command count.
    - `RefreshAssetStatus` sets Borrowed or Available. A per-unit asset is Borrowed when no unit is Tersedia and some unit is Dipinjam. A quantity asset is Borrowed when nothing is available. Disposed and Maintenance are left alone.
    - `AddMovement` writes `t_asset_movement` with `reference_type` `BORROW_REQUEST` / `MAINTENANCE` / `MANUAL`. `GetLateDays` = MAX(0, CEIL(days late)).
  - `BorrowRequest`:
    - The number uses format `BORROW` (fallback `PJM`). Borrower Anggota (active member), OrganisasiInstansi (active party plus optional contact) or PeroranganLuar (name required). The columns of the other types are cleared, and the name defaults to the member's or party's name.
    - Seikhlasnya is for members only. Rental fee = rental days (CEIL, at least 1) × the asset's rate × quantity, only for TarifTetap. Rates are copied from the asset.
    - Pending → Approved / Rejected, Pending / Approved → Cancelled (refused once paid), Approved → Borrowed through `Checkout(lines)`, and → Returned when every item is fully returned. `MarkOverdue` (for a job) sets Borrowed past the expected date to Overdue.
    - `Checkout` takes all items at once. Quantity assets need enough available quantity, which is checked before the status changes. Per-unit assets need exactly `Quantity` scanned unit codes of Tersedia units. Units and assets are updated and Borrow movements are written.
    - `WaiveLateFee(amount, note)` cannot exceed the late fee. `ReceivePayment` creates Posted Income with `borrow_request_id`; the category tells rent, donation or fine apart. It refuses more than the amount due, except for Seikhlasnya.
    - `PaymentStatus` is TidakAdaTagihan when nothing is due or paid. Only Pending / Rejected / Cancelled requests can be deleted (items cascade).
  - `BorrowItem`:
    - Unique per (request, asset). Items change only while the request is Pending, and the quantity cannot exceed the asset's stock (for per-unit assets, units that are not deleted or lost).
    - `ReturnQuantity(returned, lost, condition)` is for quantity assets and may be done in parts. Lost units reduce `m_asset.quantity` with an Out movement. The late fee = late days × rate × count. Every return writes a return inspection.
  - `BorrowItemUnit`:
    - Created by Checkout. `ReturnUnit(requestId, unitCode, condition, lost)` scans the QR: the unit becomes Tersedia with the new condition, or Hilang.
    - Unit late fee = late days × the item's rate. The item's returned count, late days and late fee are recalculated from its units.
  - `AssetMovement`: a history list. Manual `Save` is for quantity assets only: In, Out (cannot go below the quantity still borrowed) and signed Adjustment. Per-unit assets use `AssetUnit`.
  - `AssetInspection`:
    - Manual `Save` updates the condition. A per-unit asset needs a unit, and Hilang marks the unit Hilang (refused while it is borrowed). A quantity asset updates `asset_condition`.
    - Return inspections (`BorrowItemId` set) are only a record and cannot be deleted. The photo is not loaded in lists (`GetPhoto`).
  - `MaintenanceLog`:
    - A per-unit asset needs a unit. Scheduled → InProgress → Done, and Scheduled → Done directly.
    - `Start` moves the unit (must be Tersedia) to Perawatan, or the quantity asset (nothing borrowed) to Maintenance. `Finish(condition?, accountId?, categoryId?)` restores the status and can update the condition. When an account and category are given and the cost is above 0, it creates a Posted Expense transaction that can only be voided through the maintenance.
  - `Asset.Delete` (dispose) is also refused while part of a quantity asset is still borrowed.
- `Lib/Transaksi/Surat/` (all new):
  - `Letter`:
    - The letter type is normalised with `LetterTemplate.ToTypeCode`. A template, if given, fills the type and the body when they are empty. The period defaults to the Active period. The number is unique per direction.
    - Outgoing (Out): Draft → WaitingApproval (needs a body or a file) → Approved / Rejected, and Rejected can be edited and resubmitted. Then Approved → Sent → Archived.
    - Approval follows the active `LETTER` flow step by step, like `FundingRequest`. Without a flow there is one level-1 approval by the signer, with an optional position.
    - On final approval, `LetterDate` defaults to today and the number is generated inside a DB transaction: the letter type's format if active, else format `LETTER`, else `SRT/yyyyMMdd/id`. A number entered by hand is kept.
    - Incoming (In) starts as Received (sender's number, received date and sender required) → Archived.
    - The file is at most 10 MB (`GetFile` / `UpdateFile`). A final file can still be uploaded while Approved.
    - Delete is allowed for Out in Draft / Rejected and In while Received, and is refused when a borrow request uses the letter. Approvals and dispositions cascade.
  - `LetterApproval`: unique per (letter, level), written on decision, reset to Pending on resubmit (same pattern as `FundingApproval`). It stores the position and an optional signature (2 MB, `GetSignature`).
  - `LetterDisposition`: for In letters (Received / Archived) or Out letters that are Approved / Sent / Archived. `FromUserId` is the logged-in user. Open → Done (`MarkDone`) → `Reopen`. It can be edited or deleted only while Open, and `GetDataList(toMemberId:)` lists a member's dispositions.
  - `Document`: the category must be active. The file is required, at most 20 MB, not loaded in lists (`GetFile` / `UpdateFile`), and `FileSize` is filled automatically. `GetDataList(visibilities)` filters what the caller may see. Delete is permanent.
- `Lib/Transaksi/Prestasi/` (all new):
  - `Competition`:
    - Status moves forward only: Upcoming → Registration → Ongoing → Finished (skipping is allowed), and anything except Finished → Cancelled. Finished also marks Registered / Participating participants as Finished.
    - Featured requires public. `GetPublicList` serves the landing page. The poster is at most 5 MB (`GetPoster` / `UpdatePoster`).
    - Only an Upcoming competition with no participants, achievements, materials, activities, transactions or funding requests can be deleted.
  - `CompetitionMaterial`:
    - `CompetitionId` NULL means a general material, and `GetDataList(0)` lists only general ones.
    - File needs a file (20 MB), Link needs a URL, and Video needs a URL or a file. `GetDataList(visibilities)` filters by visibility.
  - `CompetitionParticipant`:
    - Unique per (competition, member). `Register` needs status Registration before the deadline, and registering again after Withdrawn reactivates the row.
    - The role is KETUA_TIM, ANGGOTA or PEMBIMBING. Registered → Participating → Finished, and Registered / Participating → Withdrawn.
  - `Achievement`:
    - Needs a member or a team name. The level defaults to the competition's level. Featured requires public, and `GetPublicList` serves the landing page.
    - A member achievement gets Prestasi points (`MemberPoint.Award`, RefId = achievement). They are recalculated on update, moved when the member changes, and removed on delete. Delete is refused when a certificate uses the achievement.
    - The certificate file and the photo are each at most 5 MB.
  - `Certificate`:
    - The number uses format `CERTIFICATE` (fallback `SRT-C`) and is unique. `VerificationCode` is 12 random characters without look-alikes, and unique.
    - Required reference by type: Peserta / Panitia / Pemateri need an activity, Pengurus needs the member's own assignment, and Prestasi needs the member's own achievement. Other columns are cleared.
    - Terbit ↔ Dicabut (`Revoke` / `Restore`). `Verify(code)` is the public check. There is no delete.
  - `MemberPoint`:
    - Unique per (member, source, ref). `internal Award` uses `PointRule.GetEffectiveRow`; without an active rule no point is given and existing rows are kept. Running it again updates the points. `internal Revoke` removes points.
    - `GenerateForActivity(activityId)` gives Hadir points (RefId = `t_attendance_record` with Hadir / Terlambat) and Panitia points (RefId = active `t_committee_member`). `GenerateForPeriod(periodId)` gives Pengurus points (RefId = `t_management_assignment`).
    - Manual `Save` cannot have 0 points; use it for Pemateri and Lainnya. `GetLeaderboard(periodId, top)` ranks members.
- `Lib/Transaksi/Formulir/` (all new):
  - `Form`:
    - EvaluasiKegiatan needs an activity. PendaftaranRekrutmen may name a member recruitment (answered by applicants without login), or leave it empty for a committee recruitment (`t_committee_recruitment.form_id`), and cannot be anonymous. SurveiUmum has no reference. References that do not apply are cleared.
    - Draft → Open → Closed, and Closed can be reopened when the close time is empty or in the future. Open needs at least one question. `GetOpenList` lists forms that are Open within their time window.
    - Once a response exists, the questions, type, references and anonymous mode are locked (`internal GetEditableForm`). Only a Draft that no committee recruitment uses can be deleted, and its questions go with it.
    - `GetSummary()` returns per question the answer count, counts per option (choices and scale) and the scale average.
    - `CommitteeRecruitment` now accepts only a PendaftaranRekrutmen form without a member recruitment.
  - `FormQuestion`: `Seq` 0 means append. PilihanGanda / KotakCentang need at least 2 distinct options, stored as a JSON array in `options`; other types clear it. Changes only while the form has no responses.
  - `FormResponse`:
    - Saved with its answers and never edited. The form must be Open within its time window.
    - `Submit` (login): an anonymous form stores no identity at all (`member_id` and `ins_by` NULL, so there is no one-response limit). A form that is not anonymous allows one response per user, and `member_id` is the user's member if any. A committee recruitment form requires an active member. Member recruitment forms are refused here.
    - `SubmitApplicant` (no login): the applicant must belong to the form's recruitment, the email must match, and the status must not be final. One response per applicant.
    - `GetMyResponse(formId, user)`. Delete is refused when a committee application uses the response; answers are removed through stubs.
    - `CommitteeApplication` now requires the response to come from the recruitment's form and belong to the applying member.
  - `FormAnswer`: unique per (response, question), built by `internal BuildRows`. TeksSingkat at most 255, Paragraf at most 2000, PilihanGanda exactly one option, KotakCentang a JSON array of options (input `Choices`), scale 1-5, Tanggal stored as `yyyy-MM-dd`, File at most 5 MB (`GetFile`). Required questions must be answered; empty optional answers create no row.
  - `Aspiration`:
    - `Submit` needs login. Anonymous stores no identity (`user_id` and `ins_by` NULL), so it is not in `GetMyList` and the sender cannot edit it.
    - Baru → Diproses → Selesai / Ditolak, and Baru → Selesai / Ditolak directly (`Respond`). Selesai / Ditolak need a response text.
    - Only the sender can edit (category, title, content, attachment) while Baru. Delete is allowed while Baru by the sender, or once Ditolak. The attachment is at most 5 MB (`GetAttachment` / `UpdateAttachment`).
- `Lib/Transaksi/EVoting/` (all new):
  - `ElectionCrypto` (internal):
    - AES-256-GCM. The 32-byte master key is passed by the app at startup with `ElectionVote.ConfigureKey(base64)`, from a secret, never from the database. Opening, voting and finalizing fail with `ELECTION_KEY_NOT_SET` without it. The same key must stay in place until every election that used it is finalized.
    - The per-election key comes from HKDF-SHA256 (info = election id). The AAD is election id + ballot reference, so a ciphertext cannot be moved to another row or election. Stored as `v1:` + base64(nonce | ciphertext | tag), with plaintext `{candidateId}|{random}`.
  - `Election`:
    - Draft → Registration → Open → Closed → Finalized, and Draft → Open directly. The period is optional and must not be Closed.
    - `OpenVoting` needs at least 1 Verified candidate, no Registered candidate left undecided, at least 1 Eligible voter and an end time in the future. It numbers Verified candidates that have no number.
    - Votes are accepted only while Open within `StartAt`..`EndAt`. `CloseExpired` is for a job.
    - `Finalize` decrypts every vote and requires the vote count to equal the voters with `voted_at`. It writes `t_election_result` for every Verified candidate: percentage of total votes, competition rank, and a winner only for a unique rank 1 (a tie has no winner).
    - The title and description can change until Finalized. The period and schedule can change only in Draft / Registration; `EndAt` may be extended while Open.
    - Delete is allowed only in Draft / Registration and removes candidates, voters and audit events. `internal GetEditableElection` / `GetOpenElection` are available.
  - `ElectionCandidate`:
    - Unique per (election, member) and per number. Every change requires the election to be Draft / Registration, and the member must be active. Re-registering after Rejected / Withdrawn reactivates the row.
    - Registered → Verified / Rejected, Rejected → Verified, and Registered / Verified → Withdrawn. Rejected / Withdrawn free the number.
    - The photo goes through `ImageUtility` and is not loaded in lists (`GetPhoto` / `UpdatePhoto`). `GetBallot` returns Verified candidates by number.
  - `ElectionEligibleVoter`:
    - Unique per (election, member). `Add(memberIds)` / `Generate(memberTypes?)` add active members, skipping existing ones. They and `SetEligibility` work while Draft / Registration / Open. Delete works only in Draft / Registration.
    - A voter who has voted cannot be changed or deleted. Use `GetMyStatus` for the logged-in user.
  - `ElectionVote`:
    - The id is random, `ins_by` is always NULL and `ins_date` holds the date only. Nothing links a vote to a voter.
    - `Cast` runs in one transaction. It validates the voter, sets `voted_at` with a conditional `ExecuteUpdate` (`voted_at IS NULL`) to stop double voting, then stores the encrypted vote. It returns a `VoteReceipt` holding a random token, shown only once; `ballot_reference` is its SHA-256 hex.
    - `VerifyReceipt(electionId, receipt)` needs no login. `GetDataList` is a bulletin board of ballot references without choices.
  - `ElectionResult`: read-only, written by `Finalize`, sorted by rank.
  - `ElectionAuditEvent`: read-only, written by the other classes. Events are CREATED, UPDATED, REGISTRATION_OPENED, OPENED, CLOSED, RESULT_FINALIZED, CANDIDATE_*, VOTERS_ADDED, VOTER_CHANGED and VOTER_DELETED. Individual votes are deliberately not audited. `ip_address` comes from the `internal Add` argument, else from `General/RequestInfo.IpAddress` (an AsyncLocal the web fills per request).
- `Lib/Transaksi/Information/` (all new):
  - `InformationHelper.ValidateLink` (internal): a link is an http(s) URL or a path starting with `/` (not `//`).
  - `Announcement`:
    - The author is the logged-in user at creation. The slug is unique, lowercase a-z 0-9 with hyphens (diacritics dropped, `internal ToSlug`). An empty slug is made from the title, with `-2`, `-3`, … on collision. A manual slug that collides is refused, and an empty slug on update keeps the old one.
    - Draft → Published → Archived, Published → Draft (`Unpublish`) and Archived → Published. `PublishedAt` is set on the first publish. Featured requires public, and Private means logged-in members only.
    - Delete is allowed only for Draft / Archived.
    - Lists load neither the content nor the cover; the encKey constructor and `GetBySlug(slug, publicOnly)` load the content. `GetPublishedList(publicOnly, category, featuredOnly)` serves the landing page and the member page.
    - The cover goes through `ImageUtility` (`GetCover` / `UpdateCover`).
  - `Banner`:
    - The image is required, at most 5 MB, JPEG / PNG / WebP by magic bytes, and stored unchanged (not resized, because it is a hero image). It is not loaded in lists (`GetImage` / `UpdateImage`).
    - `Seq` 0 means append. Empty `StartAt` / `EndAt` mean no limit. Button text needs a link. `GetActiveList` serves the landing page. Delete is permanent.
  - `Notification`:
    - `Send(UserIds, …)` and `internal Add(context, userId, …)` create in-app notifications. The type is normalised with `ToTypeCode`, the same code as the template / preference. Recipients with InApp disabled in `m_user_notification_pref` are skipped (no row means enabled).
    - Recipients manage only their own notifications: `GetMyList`, `GetUnreadCount`, `MarkRead`, `MarkAllRead` (`ExecuteUpdate`) and `Delete`. `Purge(olderThanDays)` is for a job and deletes read notifications.
  - `ActivityLog`:
    - `Log(NewRow)` needs no login (e.g. LOGIN_FAILED); the user comes from `NewRow.UserId`. Other modules use `internal Add(context, …)`. There is no update or delete except `Purge(before)` for a job.
    - Module and Action are uppercase codes, and EntityName is the lowercase table name.
    - Old / new values accept an object or a JSON string and are stored as JSON. Values under keys containing password / token / secret / hash / key / signature become `***`. Lists do not load the old / new values.
  - `NotifyHelper` (internal) sends in-app notifications from other modules inside their context: `ToUsers` / `ToUser`, `ToMembers` / `ToMember` (members without a user are skipped), and `ToApprovers(step)` (users from `ApprovalHelper.GetApproverUserIds`, same rules as `IsApprover`). The acting user never gets their own notification, and validation errors are swallowed so a notification never cancels the main change. Type codes and link paths (web pages) are constants there. It is wired into:
    - Letter, FundingRequest and ProcurementRequest: submit notifies the first step's approvers, an approval that is not the last notifies the next step, and a final decision (approve / reject / revision) notifies the creator (procurement also the requesting member).
    - LetterDisposition (target member), DuesBill.Generate (billed members), Election.OpenVoting (eligible voters), CommitteeApplication accept / reject, SalesOrder SiapDiambil (member buyer), BorrowRequest approve / reject / overdue (member borrower), Certificate create, Aspiration.Respond (non-anonymous sender), ManagementAssignment create, and Announcement on its first publish (all active members with a user).
  - `ActivityLog`: the web logs every successful write through a global filter (see the web section). `ActivityLog` / `ElectionAuditEvent` take IP and user agent from `RequestInfo` when the caller gives none.
- Read-only child classes (`ProcurementReceiptItem`, `BorrowItemUnit`, `SalesOrderItem`, `LetterApproval`, `LetterDisposition`, `AssetMovement`, `Document`, `Competition`) also follow the `<Name>.cs` + `<Name>.Services.cs` split.
- Transaction order: Approval → Akses → Anggota → Program → TimPelaksana → Presensi → Keuangan → Sponsorship → Pengadaan → Merchandise → Inventaris → Surat → Prestasi → Formulir → EVoting → Information.

### Status (2026-11-16)

- **Library:** all 88 `t_*` tables have a service class. Every transaction module is done: `Approval`, `Akses`, `Anggota`, `Program`, `TimPelaksana`, `Presensi`, `Keuangan`, `Sponsorship`, `Pengadaan`, `Merchandise`, `Inventaris`, `Surat`, `Prestasi`, `Formulir`, `EVoting` and `Information`.
- **Database testing:** every module has run against MySQL through `GWASeeder`, and all 131 tables of that time are filled (`t_lpj`, added later, is not seeded). Earlier modules were also tested on the previous device. A Keuangan scenario program (`fintest`) existed in a scratchpad; it is not part of the repo.
- **Web (2026-11-17):** foundation, auth, landing page, login, dashboard shell, all 37 management pages and the 11 Area Anggota pages are built (see "Web pages"). Every admin page was tested over HTTPS as admin. Area Anggota pages were only tested as admin (no linked member), so their member views still need a run with a member account.
- **Library additions for the web:** `Member.GetByUser(user)`, `UserAccount.ChangeOwnPassword(current, new, user)` (message `PASSWORD_CURRENT_INVALID`), and a fix in `MemberPoint.GetLeaderboard` (order before projecting to the record, which EF could not translate).

Open items not tied to one folder:
- **Pending user decisions:**
  - The DDL comment on `t_period_handover_item.ref_id` still names `t_partnership_agreement` for MOU_AKTIF. The code uses `t_sponsorship_mou`; fixing the comment awaits the user's OK.
  - There is no column for an email-verified flag (`UserToken.VerifyEmail` only consumes the token) and none for report revision notes (`ActivityReport`). Adding them is up to the user.
- **Commits:** `861fd70` holds the library. The web app, the seeder and the docs were committed and pushed after it. Commit only when asked.
- **Security:** `GWALib/EF/ModelContext.cs:282` has a commented DB root password that is already in git history. Rotate that password and remove the line.
- **Database scripts (in the repo):** `database/gitawidyaagni_create.sql` is a schema-only `mysqldump` of the dev database (135 tables since 2026-11-18, column COMMENTs; the 2026-11-18 version was loaded into an empty database and seeded). Later schema changes go to `database/migrations/YYYY-MM-DD_<name>.sql`, then regenerate the create script (`database/README.md`). MySQL 8 is in `C:\Program Files\MySQL\MySQL Server 8.0\bin` and is not on PATH.

## GitaWidyaAgniWeb

ASP.NET Core 8 MVC. The template is Velzon (`wwwroot/assets`), plus DevExtreme 23.2 from a local NuGet feed (`NuGet.config` points to `C:\Program Files\DevExpress 23.2\...`).

- **Configuration:**
  - `ConnectionStrings:Default` is passed to `DBClass.Configure`, and startup fails when it is empty.
  - The real value lives in `appsettings.Development.json`, which is git-ignored. `appsettings.json` holds only a placeholder; never put a real password in it.
  - `GWASeeder` reads the connection string from env `GWA_DB`, then `appsettings.Development.json`, then `appsettings.json`.
  - The other values below come from user-secrets (`UserSecretsId` in the csproj) or environment variables.
  - `EVoting:Key` is passed to `ElectionVote.ConfigureKey`. When it is empty, e-voting is refused.
  - `Bootstrap:AdminUsername` / `Bootstrap:AdminPassword` call `UserAccount.BootstrapAdmin`, which creates role SUPERADMIN, the user and a `m_user_role` row, and only does so while `m_user` is empty.
- **Auth:** cookie authentication (`gwa.auth`, 8 hours sliding).
  - Claims: `NameIdentifier` = `m_user.id`, which is the `user` string GWALib expects (`User.GetLibUser()` / `BaseController.LibUser`). Also `Name` = username and `Role` = role codes.
  - `LoginController` uses `UserAccount.LoginAsync`. It gives one message for a wrong username, wrong password or inactive user, updates `last_login_at` and rehashes the password when needed. It logs LOGIN / LOGIN_FAILED / LOGOUT through `ActivityLog`. Logout is a POST.
- **Authorization:** global filters, in this order:
  1. `AuthorizeFilter` requires login.
  2. `AccessFilter` checks the action against the user's roles.
  3. `AutoValidateAntiforgeryToken` checks the antiforgery token.

  `AccessFilter` decides per action:
  - `[AllowAnonymous]`: open to everyone.
  - `[LoginOnly]`: any logged-in user.
  - An action registered in `m_controller_action` with `need_auth = 1`: requires `m_role_action` on one of the user's roles.
  - `need_auth = 0`: any logged-in user.
  - **Not registered: SUPERADMIN only**, so every new page must be registered in the Controller Action master.
  - SUPERADMIN bypasses all of these checks.
  - When refused, AJAX requests get 403 and other requests are redirected to `/Login/Denied`.
- **`UserAccess` (GWALib, `Lib/Master/Akses`):** roles count when the role is active and the `m_user_role` row has no period or an Active one. It provides `GetRoleCodes`, `GetAllowedActions` (null means everything), `GetRegisteredActions` and `GetMenu`.
  - `GetMenu`: a header with a controller is a direct link and needs a `m_role_menu` row for the header. A header without a controller is a group showing only granted sub menus; empty groups are hidden.
- **`AccessService` (web):** caches actions, menu and app name for 5 minutes. `Clear(userId)` runs on login and logout.
- **Layout:**
  - The sidebar (`_Navbar`) is built from `GetMenu`, with a `Section` title shown as a menu title.
  - The app name comes from `m_organization.name`, falling back to "Gita Widya Agni".
  - The logo is served by `/Brand/Logo` from the organization profile, falling back to `assets/images/gwa-logo.png` (the official MB GWA logo, made transparent; `gwa-logo-sm.png` and `favicon.ico` come from the same image).
- **Public site:** the default route is `Landing/Index`. `LandingController` is `[AllowAnonymous]` and uses `_LayoutLanding`, which has a top navbar with a Login button (Dashboard when logged in).
  - Sections: hero (active banners in a swiper, else logo + description), Tentang (profile description, vision, mission lines), Kegiatan (`Activity.GetPublicList`), Berita (3 latest public), Prestasi (`Achievement.GetPublicList`) and Kontak. The Instagram / YouTube fallback is `mbgwaupn`.
  - `/Landing/Berita?page=` and `/berita/{slug}`. Banner images and news covers are served by `BannerImage` / `NewsCover` with the item's encKey.
  - Each section is loaded in its own try/catch, so a failing table does not break the page.
  - **Design:** a warm **light** theme taken from the logo colours. The user does not want any dark section on the landing page.
    - Styles live in `wwwroot/css/landing.css`, not in Velzon's `app.css`, and the layout loads only Bootstrap grid + Remix icons from Velzon.
    - Tokens on `:root`: ivory `#fbf8f2`, cream tint `#fdf3d6`, blush tint `#fcebe6`, gold `#e8b52a`, gold text `#8a6100` (for contrast on light backgrounds), flame `#c8202a`, leaf `#1f7a45`.
    - Alternate sections use `gwa-cream` / `gwa-blush`. The hero shows banners in a framed photo slideshow with a logo badge, or the logo emblem when there are no banners.
    - Fonts: Playfair Display (headings) + Inter (body) from Google Fonts.
    - Sections: hero with banner slideshow, emblem and stats, gold marquee, Tentang (collage from `m_organization_photo`, else gallery covers), Seksi Musik, Agenda, Prestasi, Galeri, Berita, a CTA (open recruitment + WhatsApp from `m_site_setting`) and Kontak.
    - Reveal-on-scroll, `prefers-reduced-motion` respected, focus rings, 44px targets.
  - Images that use an encKey (`BannerImage`, `NewsCover`) take it as query `?key=`, because an encKey can contain `/` (`%2F`), which 404s as a route segment. `GalleryCover/{id}` (public galleries only) and `OrganizationImage/{seq}` use the int id.
  - Logout returns to the landing page.
- **Login page:** `_LayoutLogin` uses the landing tokens (`landing.css`) plus `wwwroot/css/login.css`, and no longer uses Velzon `app.css`.
  - Layout is a split screen: a cream showcase on the left (first `m_organization_photo` in a tilted frame, a floating logo emblem and a quote) and the form on the right. Below 992px it collapses to a compact header.
  - The form has visible labels with icons, a show/hide password button (`aria-pressed`), a Caps Lock hint, a `role="alert"` error linked through `aria-describedby`, focus on the first empty field instead of an alert, and a loading state on submit that resets on `pageshow`.
  - Animation follows the GateSafetySecurity login (an animated SVG scene in the left panel), themed for a marching band and kept light: music staff lines with a flowing dashed line, beamed notes travelling along the staff (SMIL `animateMotion`), pulsing beat dots, a slowly rotating snare-drum head (top right), an equalizer (hidden below 1200px), beat rings around the logo emblem, a slow zoom on the photo, staggered fade-up entrance of the panels and form fields, a shake on the error alert and a gold shine on the Masuk button. Entrance animations use fill-mode `backwards` so hover transforms still work. `prefers-reduced-motion` turns them off and pauses SMIL (`pauseAnimations`).
  - `Denied` uses the same layout with the `.auth-state` block.
- **Dashboard page (`Home/Index`, `Models/HomeVM`):** data from GWALib `Lib/Transaksi/Information/Dashboard.GetSummary(user)`, built from aggregate queries.
  - Contents:
    - a greeting hero with the active period, days left and the next agenda item;
    - stat cards: members, upcoming activities, cash balance, unpaid dues;
    - a DevExtreme bar chart of cash flow over 6 months and a doughnut of active members per section (`Dashboard.Sections`; `Divisions` is still computed);
    - the agenda (upcoming attendance sessions plus activities not yet started);
    - "Perlu tindakan": pending absence requests (Kegiatan), letters, funding, procurement and borrow requests, plus the user's own open dispositions and unpaid dues;
    - the top 5 points and up to 8 menu shortcuts.
  - Each section shows only when `HomeVM.Can(controller)` allows `<controller>/index` (SUPERADMIN sees all). Styles are under "Dashboard" in `gwa-theme.css`.
- **Dashboard theme (Velzon):** the same palette as landing and login (DevExtreme widgets use the SurabayaTaxWeb Material theme, see above).
  - `_Layout` loads `assets/css/bootstrap.gwa.min.css`, `app.gwa.min.css` and `custom.gwa.min.css` instead of Velzon's originals, which stay untouched.
  - The `.gwa` copies are made with sed from the originals, replacing primary navy `#405189` and its shades/rgb with bronze `#9a6b00`, secondary `#3577f1` with flame `#c8202a`, success `#0ab39c` with leaf `#1f7a45`, warning `#f7b84b` with gold `#e8b52a`, and body `#f3f3f9` with `#f8f4ec`. Hover shades are mapped too.
  - If Velzon is updated, regenerate the copies with the same mapping.
  - Gold itself is too light for text on white, so bronze is the primary.
  - `wwwroot/css/gwa-theme.css` adds Inter / Playfair, cards, loader colours and plugin accents (sweetalert, flatpickr, pagination). `.gwa-welcome` is the dashboard greeting block.
  - **Sidebar & header follow the SurabayaTaxWeb model** (`C:\Users\hisya\source\repos\surabayataxnextgen\SurabayaTaxWeb`: `theme.css`, `sidebar-submenu-fix.css`, `Components/Header`, `Components/NavBar`). Styles live in `wwwroot/css/gwa-sidebar.css`, loaded last.
    - Sidebar: a brand gradient (`--sidebar-1..3`, flame `#9e1720` → `#c0212a` → burnt orange `#cf4a1d`, so white text stays readable). Inactive links are dimmed, the active item is a translucent white pill with a gold left bar, and the active group gets a darker background.
    - Sidebar brand box: a big logo with "GITA WIDYA AGNI / BACK OFFICE" beneath it, and a faint logo illustration at the bottom.
    - Collapsed sidebar (`data-sidebar-size=sm`): flyout panels titled from `data-menu-title`, set by `fnInitSidebarMenu` in `_Layout`, which also shifts low panels up.
    - `data-sidebar="dark"` is kept only so Velzon uses white text and `logo-light`.
    - Header: hamburger, page title (Playfair), a link to the public site, fullscreen, and a user dropdown (gold initials avatar, member full name, active role names, Dashboard / Beranda Publik / POST Logout). Its data comes from `UserAccess.GetProfile` (cached in `AccessService.GetProfile`, cleared on login/logout). There is no dark-mode toggle (the user does not want dark styling).
  - `_DevExtremeLayout` is unused; `_Layout` loads DevExtreme itself.
- **Management page pattern** (copied from BEGateSecuritySystemWeb, `C:\Users\hisya\source\repos\GateSafetySecurity`, example `Master/DamageType`; GWA reference pages: `Akses/User`, `Akses/Role`, `Akses/Menu`, `Pengaturan/Pengaturan`):
  - **Areas follow the sidebar header**: a header with sub menus is one area holding all its controllers, e.g. header Akses → `Controllers/Akses/{User,Role,Menu}Controller.cs` with `[Area("Akses")]`, URL `/Akses/User`. A header that links directly gets its own area (`[Area("Pengaturan")] PengaturanController` → `/Pengaturan/Pengaturan`). The same folder appears in `Models/<Area>/<Name>VM.cs`, `Views/<Area>/<Name>/*.cshtml` and `wwwroot/Scripts/<Area>/<Name>.js`.
    - Views live under `Views/`, not `Areas/`: `Program.cs` sets `AreaViewLocationFormats` to `/Views/{2}/{1}/{0}.cshtml` + `/Views/Shared/{0}.cshtml`, and `BaseController.URLView` (`~/Views/<Area>/<Controller>/`) gives absolute paths.
    - Route `{area:exists}/{controller}/{action=Index}/{id?}` comes before the default route. Landing, Login, Home and Brand have no area; links to them from shared views pass `new { area = "" }`.
    - Access keys stay `controller/action` (no area). The sidebar builds URLs with `AccessService.AreaOf(controller)` (controller → area from the action descriptors), so `m_controller` needs no area column.
  - **Model `<Name>VM`** (nested classes, like DamageTypeVM): `Index`; `List` (`Rows` from `Method`); `Row` (grid DTO with `EncDecrKey` and `Aktif` bool); `Add` (`NewRow Row` + option lists) and `Edit : Add` (loads by encKey, plus header info); `NewRow` = the posted form with DataAnnotations (`[Required]`, `[StringLength]`, `[Range]`, `[Display(Name)]`; dxForm turns them into client validation and labels; braces in messages must be doubled, `{{seq}}`); extra modal models; static `Method` holding every GWALib call. Pages with tabs use `List<X>` / `Add<X>` / `Edit<X>` / `New<X>` per tab.
  - **Controller** (`Infrastructure/BaseController`): `Index() => HalamanIndex(new VM.Index())`; `List()` → `Bagian(() => new VM.List())` (partial `_List`); `Add()` / `Edit(encKey)` → `Bagian(...)` (partials `_Add` / `_Edit`, both render the shared `_Form`; Edit stores the encKey in `TempData[KEY_EDIT]`); POST `SimpanBaru` / `Update([FromBody] VM.NewRow model)` → `Aksi(user => ..., "pesan", KEY_EDIT)`.
    - `Aksi` runs `ValidateModel()` (first DataAnnotations message → `ArgumentException`) and always returns `Json(Response)` `{ Status (1 ok / 0 error), Message, Data }`. `ArgumentException` → `JsonError`; other exceptions → `JsonServerError` (logged, "⚠ Server Error").
    - `EditKey(KEY_EDIT)` reads the TempData key or fails with "Form edit sudah kedaluwarsa". TempData is read once; a failed save keeps it.
    - Row actions post `GwaVM.KeyInput { Key }`. Files go as FormData (`[FromForm]` + `IFormFile`, `BacaBerkasAsync`).
  - **GWALib calls**: `response.Ok()` (`Infrastructure/LibExtensions`) returns Data or throws `ArgumentException(message)`; `LibExtensions.Load<T>(encKey, "Nama")` loads a row through its encKey constructor and throws a friendly message when the key is missing, invalid, expired (encKeys change daily) or the row is gone. Nested references are set from ids (`obj.ControllerMenu = new ControllerMenu { Id = id }`).
  - **Access**: action names that are not seeded vocabulary carry `[AccessAs(Acc.Index)]` (reads, GET partials) or `Acc.Create/Edit/Delete/Submit/Approve/Process/Payment` (writes). `AccessFilter` checks `controller/<AccessAs action>`; the attribute on the action wins over one on the controller. AJAX requests without a session get 401 (cookie events in `Program.cs`).
  - **Page title**: `_Layout` renders `ViewComponents/PageTitle` (title on the left, breadcrumb Dashboard › Area › [Controller] › Title on the right) from `ViewData["Title"]`. Override the area text with `ViewData["BreadcrumbArea"]`; hide it with `ViewData["HidePageTitle"] = true` (dashboard). Views never write their own `page-title-box`.
  - **Index.cshtml** = one `.card`:
    - `card-header`: title with icon + `.gwa-card-intro`.
    - `card-body`: `<partial name="_Toolbar" model='new ToolbarModel("Tambah X", "X.add()", (css, icon, text)...)' />` (Add `btn-outline-primary` + "Tampilkan Legenda" `btn-outline-info` with an inline legend of the grid buttons; several toolbars on one page set `{ LegendId = "..." }`), then `<div id="target">` for the grid. Tabs put one toolbar + `#target<Tab>` in each pane.
    - Then `window.<Name>Url = { list, add, edit, simpanBaru, update, ... }` and the page script.
  - **Grids**: always `Html.DevExtreme().DataGrid<VM.Row>().ID(..).DataSource(Model.Rows).KeyExpr("EncDecrKey").ListGrid("Export name")`. Never set borders, paging, filters or export per page.
    - `ListGrid` (`Infrastructure/DevExtremeExtensions`) gives row + column lines, search, filter row, header filter, multi sort, pager 10/20/50 with info, and **export Excel + PDF** through the global `gridExport` (`wwwroot/js/grid-export.js`). The PDF header is bronze `#9a6b00`, like the grid header.
    - Columns: `columns.AddRowNumber()` first, `AddFor(..)` with `CellTemplate(new JS("X.fooTemplate"))` / `AppUI.codeTemplate` / `AppUI.yesNoTemplate` / `AppUI.countBadgeTemplate`, and `columns.AddAction("X.actionTemplate")` last (no sort/filter, left out of exports).
    - Inactive rows: `.OnRowPrepared("AppUI.inactiveRowPrepared")` (`Aktif == false` → `bg-soft-danger`). Small child grids in a modal use `.DetailGrid("no data text")`. Trees use `TreeList`, child rows `MasterDetail`, small editable child lists `Editing(Cell)`.
  - **Buttons** are always Bootstrap, never `dxButton`. Grid actions are `AppUI.actionButton({ css: 'btn-outline-warning', icon, title, onClick })`: success = child data / access, info or secondary = view, warning = edit, danger = deactivate / delete. Modal footer: `btn-light` Batal + `btn-success` Simpan / Simpan Perubahan.
  - **Modal forms** (`_Form`): `<div class="gwa-mform">` holding
    - an optional `.gwa-mform-info` summary (Edit) and `.gwa-mform-section` titles;
    - `Html.DevExtreme().Form<VM.NewRow>().ID("frmX").FormData(Model.Row).LabelLocation(Top).ShowColonAfterLabel(false).ColCount(n).ColCountByScreen(c => c.Xs(1).Sm(1))` with `AddSimpleFor(..).Editor(..)`; status (`IsActive` SelectBox) only in Edit; options from `GwaVM.Opsi<TEnum>()` / `GwaVM.OpsiAktif`;
    - `.gwa-mform-add` for an inline "add child" panel, `.gwa-mform-panel` around trees and grids, `.gwa-mform-note`, and `.gwa-mform-footer`.
    - Files: a separate `FileUploader().UploadMode(UseForm)`, read with `AppUI.dxFile(id)` and sent with `AppUI.postForm(url, AppUI.toFormData(data))`.
    - Modals load into the shared `_Modal.cshtml`: `staticBackdrop` (+ size `'lg'`), `staticBackdropLarge`, `staticBackdropFullScreen`.
  - **Page JS** (`wwwroot/Scripts/<Area>/<Name>.js`) is a module `window.<Name> = (function () { ...; return { add, edit, save, update, ...Template }; })();`.
    - It loads the grid on ready (`AppUI.loadPartial('#target', XUrl.list)`) and opens modals with `AppUI.openModal(MODAL, title, url, data, size)`.
    - It saves with `AppUI.submitForm('#frmX', url, extra, MODAL).done(refresh)` and confirms row actions with `AppUI.confirmPost(title, text, url, { Key }, label, 'btn-danger').done(refresh)`.
    - It writes text only through `.text()`.
  - **AppUI** (`wwwroot/js/site.js`, loaded by `_Layout`; replaces the old `Gwa.js`):
    - Requests: `postJson`, `postForm`, `toFormData`, `getJson`, `postAndClose`, `submitForm`, `confirm`, `confirmPost`, `promptReason`, `reasonPost`, `loadPartial`, `openModal`, `closeModal`, alerts.
    - Grid helpers: `rowNumberTemplate`, `yesNoTemplate`, `badgeTemplate(map)`, `codeTemplate`, `countBadgeTemplate`, `createdTemplate`, `actionButton`, `inactiveRowPrepared`, `gridRows`, `removeGridRow`. Values: `dxValue`, `dxFile`, `rupiah`, `toIsoDate`.
    - It adds the antiforgery header (`@Html.AntiForgeryToken()` in `_Layout`, or the `csrf-token` meta) to every non-GET jQuery request, redirects to login on 401, warns on 403, and re-measures grids / tree lists when a tab or modal opens.
    - Indonesian DevExtreme texts are in `Scripts/Shared/dx.messages.id.js` (23.2.5 ships no `id` locale).
  - **Layout**: `_Layout` loads jQuery + DevExtreme (`dx.all.js`, `dx.aspnet.mvc/data.js`), `dx.messages.id.js` and `js/grid-export.js` in `<head>`, because Razor helpers emit inline scripts. exceljs, FileSaver, jsPDF (`js/devextreme/jspdf.umd.min.js`) and `js/site.js` load at the end of the body. There is only one jQuery; do not add another after it, or the dx plugins are lost. Sections: `Styles` (head) and `Scripts`.
  - **Look**: DevExtreme theme `css/devextreme/dx.material.blue.light.compact.css` (same as SurabayaTaxWeb), then `css/gwa-devextreme.css`, which holds the BEGate standards in GWA colours: bronze grid header with white uppercase text, ivory zebra rows, gold hover, bronze pager, `.gwa-list-grid`, `.gwa-legend*`, `.gwa-mform*`, `.gwa-cell-main/-sub`. The page loader (`#loader` + `assets/css/spinner.css`) and the AJAX overlay (`#loading1`, toggled by `AppUI.showLoading/hideLoading`) are the same as SurabayaTaxWeb; only the logo differs.
  - Changes to roles, menus or users call `AccessService.ClearAll()` / `Clear(userId)` so the sidebar and access checks update immediately.
  - JSON uses PascalCase (`LibJson`); `EnumHelper.Text(enum)` gives an enum's `[Description]`.
- Stop a running web process before building from the IDE. Otherwise the build fails with `MSB3027 ... locked by "GitaWidyaAgniWeb"`.
- The `Secure` auth cookie is not sent over plain `http://` by non-browser clients. To test logged-in pages with curl, run the app on `https://localhost:<port>` and use `curl -k` with a cookie jar.

### Web pages

The sidebar menu and the Controller Action rows exist in the database (seeded by `GWASeeder`). Every page in the table below is built with the DevExtreme pattern (2026-11-16 / 17). Areas follow the sidebar header: `Akses`, `Pengaturan`, `Organisasi`, `Anggota`, `ProgramKerja`, `Kegiatan`, `Keuangan`, `Inventaris`, `Pengadaan`, `Merchandise`, `Kemitraan`, `Persuratan`, `Prestasi`, `Formulir`, `EVoting`, `Informasi`, plus `AreaAnggota` for the member pages.

Public pages added on the landing controller: `/rekrutmen/{id?}` (open recruitment apply), `/sertifikat/verifikasi?kode=` (certificate check), `/toko` (guest merchandise order + tracking) and `/aset/{unitCode}` (asset unit detail from a QR label scan).

**Rules for each page:**
- Use the controller name exactly as seeded. The access catalog (controllers, actions, menu, roles) is `GWASeeder/Seed.Akses.cs`.
- Action vocabulary (seeded per controller; any other action name is unregistered, so only SUPERADMIN can open it):
  - `Index`: the page and every read endpoint (lists, details, file downloads).
  - `Create` / `Edit` / `Delete`: the main data and child data on that page.
  - `Submit` (send to approval), `Approve` (approve / reject / request revision), `Process` (status changes such as open/close, publish, checkout, return), `Payment` (receive / record / cancel payments that create transactions). Only the controllers listed in the catalog have them.
  - Endpoints with other method names must be mapped onto one of these names (the web needs an attribute for that, since `AccessFilter` checks `controller/action`).
- After adding a controller or action to the catalog, run `dotnet run --project GWASeeder -- --sync-akses`.
- Inherit `BaseController` and pass `LibUser` to GWALib.
- Use `_Layout`, the `[Area]` folder structure, `ListGrid` (export Excel + PDF) and the Index / `_List` / `_Add` / `_Edit` / `_Form` + JS module split of the management page pattern above.

Suggested order: Akses & Pengaturan first, so admins can manage roles and menus from the UI, then the modules in the order below.

| # | Sidebar header / sub menu | Controller | Main GWALib classes |
|---|---|---|---|
| 1 | Akses › User | `User` | `UserAccount`, `UserRole` (assign roles per period), reset password via `UserToken` |
| 2 | Akses › Role | `Role` | `Role`, `RoleAction` (`SaveRoleActions`), `RoleMenu` (`SaveRoleMenus`) |
| 3 | Akses › Menu | `Menu` | `ControllerMenu` (+ `SaveControllerActions`), `ControllerAction`, `HeaderMenu` (+ `SaveHeaderMenuDetails`), `SubMenu` |
| 4 | Pengaturan Sistem | `Pengaturan` | `SiteSetting`, `DocumentNumberFormat`, `ApprovalFlow` / `ApprovalFlowStep`, `MessageTemplate`, `Sequence`, `ScheduledJobLog`, `MessageOutbox`, `ActivityLog` (read-only) |
| 5 | Organisasi › Profil Organisasi | `Organisasi` | `OrganizationProfile` (logo, SK pembina), `OrganizationPhoto` |
| 6 | Organisasi › Periode | `Periode` | `ManagementPeriod`, `PeriodHandover` / `PeriodHandoverItem` (submit / approve) |
| 6a | Organisasi › Section | `Section` | `Section`, `SectionMember` (section history: move, delete wrong rows). The member history modal (`RiwayatAnggota`, `Scripts/Organisasi/SectionRiwayat.js`) is also opened from Data Anggota |
| 7 | Organisasi › Divisi | `Divisi` | `Division`, `DivisionMember` |
| 8 | Organisasi › Jabatan | `Jabatan` | `Position` |
| 9 | Organisasi › Kepengurusan | `Kepengurusan` | `ManagementAssignment` (assign / end) |
| 10 | Anggota › Data Anggota | `Anggota` | `Member`, `MemberPhoto`, link to `UserAccount` |
| 11 | Anggota › Alumni | `Alumni` | `AlumniProfile` |
| 12 | Anggota › Rekrutmen | `Rekrutmen` | `Recruitment` (open / close), `RecruitmentApplicant` (review), `Form` for extra questions; public apply page (`Apply` + `FormResponse.SubmitApplicant`) |
| 13 | Program Kerja | `ProgramKerja` | `WorkProgram` (bidang + jangka), `ProgramTask`, `TimelinePlan` |
| 13a | LPJ (area `Lpj`, `/Lpj/Lpj`) | `Lpj` | `Lpj` (narrative form, preview, finalize to PDF, reopen, download); roles KETUA, WAKIL, SEKRETARIS |
| 14 | Kegiatan › Daftar Kegiatan | `Kegiatan` | `Activity`, `ActivityParticipant`, `ActivityCollaborator`, `ActivityReport`, `ActivityAbsenceRequest` (row action "Pengajuan tidak ikut": approve / reject, Approve action; grid column Izin Menunggu) |
| 15 | Kegiatan › Tim Pelaksana | `TimPelaksana` | `CommitteeSection`, `CommitteeMember`, `CommitteeRecruitment` / `CommitteeApplication`, `ActivityRundown`, `ActivityChecklist` (+ `GenerateFromTemplate`, `ChecklistTemplate` master), `ActivityTripMember` |
| 16 | Kegiatan › Presensi | `Presensi` | `ActivitySchedule` (+ `GenerateSessions`), `AttendanceSession` (open with QR / close / cancel), `AttendanceRecord` (`CheckIn` / `CheckOut` with camera + location, `SetManual`), `MeetingMinutes` |
| 17 | Kegiatan › Galeri | `Galeri` | `Gallery`, `GalleryPhoto` (cover, publish) |
| 18 | Keuangan › Transaksi | `Transaksi` | `FinancialAccount`, `FinancialCategory`, `FinancialTransaction` (post / void), `FinancialAttachment`, `PeriodBalance` |
| 19 | Keuangan › Anggaran | `Anggaran` | `Budget`, `BudgetItem`, `BudgetDocument` (submit / approve) |
| 20 | Keuangan › Pengajuan Dana | `PengajuanDana` | `FundingRequest` (approval flow, disburse, settle), `FundingRealization`, `FundingApproval` (history) |
| 21 | Keuangan › Iuran | `Iuran` | `DuesSetting`, `DuesBill` (generate / pay / waive) |
| 22 | Keuangan › Hutang | `Hutang` | `Debt`, `DebtPayment`, `DebtAttachment` |
| 23 | Inventaris › Aset | `Aset` | `AssetCategory`, `Asset`, `AssetUnit` (QR labels), `AssetPhoto`, `AssetMovement`, `AssetInspection`, `MaintenanceLog` |
| 24 | Inventaris › Peminjaman | `Peminjaman` | `BorrowRequest` (approve / checkout by QR scan / payment), `BorrowItem` (`ReturnQuantity`), `BorrowItemUnit` (`ReturnUnit`) |
| 25 | Pengadaan | `Pengadaan` | `ProcurementRequest` (approval flow / purchase / payment), `ProcurementItem`, `ProcurementQuotation`, `ProcurementReceipt` (+ asset creation) |
| 26 | Merchandise | `Merchandise` | `Product`, `ProductVariant`, `ProductStockMovement`, `SalesOrder` (payment / ready / finish / cancel) |
| 27 | Kemitraan › Mitra | `Mitra` | `ExternalParty`, `ExternalPartyContact` |
| 28 | Kemitraan › Sponsorship | `Sponsorship` | `SponsorshipPackage` / `PackageBenefit`, `SponsorshipDeal`, `SponsorshipBenefit`, `SponsorshipMou` |
| 29 | Persuratan › Surat | `Surat` | `LetterTemplate`, `Letter` (approval flow / sent / archive), `LetterApproval`, `LetterDisposition` |
| 30 | Persuratan › Arsip Dokumen | `Dokumen` | `DocumentCategory`, `Document` |
| 31 | Prestasi › Lomba & Prestasi | `Prestasi` | `Competition`, `CompetitionMaterial`, `CompetitionParticipant`, `Achievement`, `PointRule`, `MemberPoint` (+ leaderboard) |
| 32 | Prestasi › Sertifikat | `Sertifikat` | `Certificate` (revoke / restore); public verify page (`Certificate.Verify`) |
| 33 | Formulir › Formulir | `Formulir` | `Form`, `FormQuestion` (builder), `FormResponse` / `FormAnswer`, `GetSummary` |
| 34 | Formulir › Aspirasi | `Aspirasi` | `Aspiration` (respond) |
| 35 | E-Voting | `EVoting` | `Election` (registration / open / close / finalize), `ElectionCandidate`, `ElectionEligibleVoter`, `ElectionResult`, `ElectionAuditEvent`; voter page `ElectionVote.Cast` + `VerifyReceipt` |
| 36 | Informasi › Berita | `Berita` | `Announcement` (publish / archive, cover) |
| 37 | Informasi › Banner | `Banner` | `Banner` (image, schedule) |

**Member self-service pages** (area `AreaAnggota`, URL `/AreaAnggota/<Controller>`). The menu header "Area Anggota" is seeded for every role, with controllers `ProfilSaya`, `PresensiSaya`, `KegiatanSaya`, `IuranSaya`, `PeminjamanSaya`, `Toko`, `FormulirSaya`, `Pemilihan`, `PrestasiSaya`, `DisposisiSaya` and `Notifikasi` (only `Index` registered, need_auth 0).
- They inherit `Controllers/AreaAnggota/AreaAnggotaController` (`[Area("AreaAnggota"), LoginOnly]`). `SayaAtauNull()` / `Saya()` return the logged-in user's member (`Member.GetByUser`). Without a member the page shows `Views/AreaAnggota/_BukanAnggota.cshtml`.
- Every action uses the logged-in member, never a member id from the request. Cancelling or finishing a child row (participant, application, order, borrow request, disposition, certificate download) first checks that the row belongs to that member.
- The views are plain Bootstrap cards (no DevExtreme grids) with small inline scripts.
- Pages:
  - `ProfilSaya`: contact edit, own photo, positions and divisions, change password (old password required).
  - `PresensiSaya`: QR scan with `js/html5-qrcode.min.js` (2.3.8, or type the code), selfie upload, geolocation, check-in / check-out, history.
  - `KegiatanSaya`: activities the member follows automatically show "Otomatis terdaftar" with an inline "Ajukan tidak ikut" form (one upcoming session or the whole activity) and a "Pengajuan Tidak Ikut" list with withdraw; Peserta Terdaftar activities keep register / cancel. Also apply / withdraw for open committee recruitments (the member's own form response is attached when the recruitment has a form), committee duties.
  - `IuranSaya`: own dues bills (read only).
  - `PeminjamanSaya`: borrow request as member (borrower type Anggota) and cancel.
  - `Toko`: catalog with variants, client-side cart, order as member, own orders and cancel.
  - `FormulirSaya`: fill open forms (`Isi`; answers posted as FormData `q{id}` / `f{id}`), send and delete own aspirations.
  - `Pemilihan`: ballot, `ElectionVote.Cast` with a one-time receipt dialog, receipt check, final results.
  - `PrestasiSaya`: achievements, points with rank, competitions, certificates with download and verification link.
  - `DisposisiSaya`: own dispositions, mark done.
  - `Notifikasi`: list, mark read / all read, delete. `_Header` shows a bell with the unread count (`Notification.GetUnreadCount`).
- Guests order on the public `/toko` page (`LandingController.Toko`, `Models/Landing/TokoVM.cs`): catalog with a quantity per product / variant (form keys `Qty[{productId}-{variantId or 0}]`), name + WhatsApp, a honeypot field `Website`, `SalesOrder.SaveGuest`, a WhatsApp confirm link to `m_site_setting.whatsapp`, and order tracking by number + WhatsApp (`FindGuestOrder`). Product photos for it come from `/Landing/ProdukFoto?key=` (active products only). `TokoProduk.LoadKatalog()` is shared with Area Anggota `Toko`, which links non-members to `/toko`. The landing navbar and footer have a Toko link.

**Cross-cutting web services:**
- `Infrastructure/ActivityLogFilter` (global): every successful non-GET action of a logged-in user that returns `Json(Response)` with Status Success is written with `ActivityLog.LogAsync`. Module = area (else controller), Action = action name, EntityName = controller, EntityId = `Data.Id` when present, NewValue = the action arguments as JSON (files become name + size; GWALib masks secret-looking keys, including `Key`). `Notifikasi` and `Login` are skipped. A logging failure only writes a warning.
- `RequestInfoMiddleware` (after `UseRouting`) fills `GWALib.General.RequestInfo` with the IP and user agent.
- `Infrastructure/Jobs/ScheduledJobService` (hosted service, checked every minute): `SEND_OUTBOX` 1 min, `CLOSE_EXPIRED_ELECTION` 5 min, `MARK_BORROW_OVERDUE` 30 min, `GENERATE_ATTENDANCE_SESSIONS` 6 h (today to +7 days), `PURGE_OLD_DATA` daily (notifications 90 days, activity log 365, tokens 30, job log 90).
  - Each run is logged with `ScheduledJobLog`, and the next run is computed from the last logged run, so a restart does not repeat daily jobs. `SEND_OUTBOX` writes a log row only when messages are due.
  - Config section `Jobs`: `Enabled` (default true; enable it on one instance only), `Username` (the job user; empty = first active SUPERADMIN user), plus the retention days and `SessionDaysAhead`.
- `Infrastructure/Jobs/MessageSender`: email over SMTP (`Smtp:Host/Port/EnableSsl/Username/Password/From/FromName`) and WhatsApp over a Fonnte-style HTTP gateway (`WhatsApp:Url`, `WhatsApp:Token` sent as `Authorization`; POST form `target` + `message`; a body with `"status":false` counts as failed). A channel without configuration is not sent and its messages stay Menunggu. Put the password and token in user-secrets.
- `_DevExtremeLayout` stays unthemed and unused (`_Layout` loads DevExtreme itself).
- **DevExpress Reports** (`DevExpress.AspNetCore.Reporting` 23.2.5; `NuGet.config` also points to `C:\Program Files\DevExpress 23.2\Components\System\Components\Packages`). Printed documents use XtraReport + Web Document Viewer, not browser printing.
  - `Program.cs`: `AddDevExpressControls`, `ConfigureReportingServices(... UseCachedReportSourceBuilder())`, `UseDevExpressControls` after `UseStaticFiles`.
  - `Controllers/ReportViewerController` (route `DXXRDV`) is `[LoginOnly, IgnoreAntiforgeryToken]`, because the client viewer sends no antiforgery header.
  - The client viewer files are in `wwwroot/lib/dxreporting-23.2.5` (`devexpress-reporting` and `@devexpress/analytics-core` 23.2.5 from unpkg, knockout from the DevExtreme install). jQuery and `dx.all.js` come from the project, which is the same 23.2.5.
  - Pattern: the page action builds the report and returns a standalone viewer view (`Layout = null`, `Html.DevExpress().WebDocumentViewer(...).Bind(report)`). The page JS loads that view in an iframe inside `staticBackdropFullScreen`.
  - Reports are code-based classes in `Reports/` (no .repx). Controls created in code use the default unit (1/100 inch) even when `ReportUnit` is changed, so write sizes in 0.1 mm and convert them (`M` / `R` helpers).
  - Inventaris QR labels:
    - `Reports/LabelQrUnitReport` is A4 with 4 columns of 46 × 53 mm labels (logo, organization name, QR, code, asset name + serial).
    - The QR holds the public URL `{App:PublicUrl or request host}/aset/{qrToken}` (`Infrastructure/AsetLabel.PublicUrl`), so the URL cannot be guessed from inventory codes; the unit code is only printed as text. Labels printed before 2026-11-18 (URL with the unit code) no longer open and must be reprinted. Set `App:PublicUrl` to the production domain before printing real labels.
    - Scanning opens `LandingController.AsetPublik` without login: unit code, asset name / code, category, brand, serial, condition, unit status, location, specification, the first asset photo (`/Landing/AsetFoto?kode={qrToken}`), and a WhatsApp link to the organization. It never shows cost, rates or borrowers. Only tokens are accepted (a plain unit code returns 404). Unknown or deleted units return 404, and a logged-in user also gets a dashboard link.
    - Internal scanners (borrow checkout and return in `Peminjaman.js` / `PeminjamanVM`, and the unit code on inspection / maintenance in `AsetVM.Method.UnitId`) accept the scanned URL, the token or a typed unit code. `AsetLabel.KodeUnit` (server) and `unitCode()` (`Peminjaman.js`) take the last path segment in upper case; `AsetLabel.ResolveKode` / `AssetUnit.GetByCodeOrToken` map a token to its unit.
    - `Aset/CetakLabel` (POST, Edit) checks that the units belong to the asset, marks them label-printed and stores the rows in `IMemoryCache` for 10 minutes, keyed by user + id.
    - `Aset/LabelQr?id=` (Index) shows the viewer. The asset page no longer loads `js/qrcode.min.js`; Presensi still uses it.
  - The standalone viewer page is shared: `Views/Shared/_ReportViewer.cshtml` (model = any `XtraReport`), opened with `AppUI.openReport(title, url)` (iframe in `staticBackdropFullScreen`).
  - **Base class `Reports/DocumentReport`** (document reports: LPJ, Persuratan):
    - A4 portrait (or landscape), Times New Roman 12, page number in the footer.
    - Content is a sequence of blocks; each block is a `DetailReportBand` over `List<Row>` (C1..C8, Bold, Shade, Bullet, Img, Img2), so paragraphs and table rows break cleanly across pages.
    - Blocks: `Letterhead(ReportBrand)` (logo, organization name, institution, address + contact, double rule), `TitleBlock`, `Section` / `Paragraphs` (lines starting with `- ` print as bullets), `Html` (XRRichText, letter bodies), `Table`, `Fields` (label : value without borders), `Signatures(place?, date, signers, knownLast)` (signature image above the name when present), and `Stamp` (diagonal watermark).
    - A chapter heading is a `GroupHeader` WithFirstDetail above paragraphs and a `ReportHeader` above tables, because two field-less group headers print in the wrong order.
    - Never loop on a list's `Count` while adding to it (the first disposition sheet hung the server this way).
  - **`Reports/ReportBrand.Load(webRoot)`** supplies:
    - name and logo from the organization profile (fallback `assets/images/gwa-logo.png`);
    - address from site setting `alamat`;
    - contact from the profile phone / email and Instagram, or WhatsApp;
    - signing city from site setting `kota` (fallback Surabaya).
  - **`BaseController.Laporan(() => report)`** returns the shared viewer page. `?pdf=1` downloads the PDF directly. An `ArgumentException` is shown as plain text.
  - **LPJ:** `Reports/LpjReport` (`LpjReport.Kegiatan` / `Kepengurusan`) follows the MB GWA LPJ templates: cover (title, logo, institution, year), numbered chapters, signatures, then appendices. The appendices are:
    - documentation photos, 2 per row;
    - budget realization: income, expenses per category with RAB vs realization vs difference, and a summary;
    - inventory list;
    - officers table with photos.
  - **Persuratan:** `Reports/PersuratanReport`, with data assembled by `Models/Persuratan/PersuratanCetak`. All reports use the letterhead.
    - **Cetak surat** (`Surat/Cetak?key=`, outgoing letters only):
      - Layout: place + date, Nomor / Lampiran / Perihal, recipient, the HTML body (plain text is converted), then signers from the letter approvals in level order.
      - A signer's role is the approval position, else the approver's active role name. The signature image is printed when one is stored.
      - Letters not yet Approved / Sent / Archived carry a DRAFT watermark (DITOLAK when rejected).
    - **Pratinjau template** (`Surat/CetakTemplate?key=`): the template body on the letterhead with a CONTOH watermark.
    - **Lembar disposisi** (`Surat/LembarDisposisi?key=`, incoming letters and approved outgoing ones): letter details, a disposition table (date / from, to, instruction, due date, status, initials) padded with empty rows for handwriting, and a notes box.
    - **Agenda surat** (`Surat/Agenda?arah=1|2&dari=&sampai=`, landscape): incoming letters by received date with their disposition targets, or outgoing letters by letter date.
    - **Daftar arsip dokumen** (`Dokumen/CetakDaftar`, landscape): documents grouped by category.
    - Buttons: row actions "Cetak surat" / "Lembar disposisi" / "Pratinjau di kop surat", and toolbar buttons "Cetak Agenda Surat Masuk/Keluar" (asks for a date range) and "Cetak Daftar Arsip". `ToolbarModel.Extra` adds such buttons.
  - **Sertifikat:** `Reports/SertifikatReport` (A4 landscape, one page). It has a gold frame, the logo, the title, the number, the recipient name and NIA, a role sentence for the certificate type, and the place and date. Its data comes from `Certificate.GetPrintData()` in GWALib (`Certificate.Data.cs`), which gives the activity, position or achievement reference.
    - Signers: Ketua Pelaksana + Ketua Umum for activity certificates; Ketua Umum + Pembina for the others. Ketua Umum is taken from the period that contains the issue date, else the Active period.
    - A QR links to `/sertifikat/verifikasi?kode=` (base from `AsetLabel.BaseUrl`). A revoked certificate gets a DICABUT watermark.
    - Admin: `Sertifikat/Cetak?key=` (Index). Member: `AreaAnggota/PrestasiSaya/CetakSertifikat?key=&pdf=1`, only for the member's own Terbit certificates. An uploaded file is still downloadable separately.
  - **Peminjaman (invoice + kuitansi):** `Reports/PeminjamanReport`, with data from `Models/Inventaris/PeminjamanCetak` (borrow request, items with unit codes, Posted Income transactions of the request, active bank / e-wallet accounts, and the current Bendahara of the Active period as signer).
    - **Invoice** (`Peminjaman/Invoice?key=`, Index): borrower, borrow times, scheme, guarantee, a table of rent and late-fee lines, a summary (rent, late fee, waiver, total, one line per payment, remaining), notes, and transfer accounts while something is still due. Optional empty fields are left out so it fits one page. Watermark LUNAS / DITOLAK / DIBATALKAN. The grid shows the action except for Rejected / Cancelled.
    - **Kuitansi** (`Peminjaman/Kuitansi?key=&trx=`, Index): `trx` = one payment, empty = all payments combined. It has the amount in words (`DocumentReport.Terbilang`), an amount box, and signatures of the payer and the Bendahara. The grid action shows when something is paid; the Detail modal has a per-payment button that opens in a new tab, so modals are not stacked.
    - `DocumentReport.Signatures` takes an optional `gap` (space above the place/date line; default 30).
  - `Lpj/Pratinjau?key=` builds the report from current data; `Lpj/Finalisasi` (Process) exports it with `ExportToPdf` and calls `Lpj.Finalize`; `Lpj/Unduh?key=` returns the stored PDF.
  - The DevExpress install on this device is a trial: exported PDFs carry "created using the trial version of DevExpress components". A licensed install removes it.

## GWASeeder

A console project, part of the solution, that fills every table except `t_lpj` (134 of 135) with sample data for MB Gita Widya Agni through the GWALib services, so the validation rules run too.

- **Run:** `dotnet run --project GWASeeder` (empty database), `-- --reset` (deletes every row of every model table with FOREIGN_KEY_CHECKS off, after typing RESET or `--yes`, then seeds) or `-- --sync-akses` (access catalog only, on a seeded database). Other options: `--evoting-key <base64>`, `--admin-password <pw>`.
  - The connection string comes from env `GWA_DB`, then `GitaWidyaAgniWeb/appsettings.Development.json`, then `appsettings.json`.
  - The e-voting key comes from the argument, env `GWA_EVOTING_KEY` or `EVoting:Key`. When none is given, a random key is generated and printed. The web must use the same key to finalize elections.
- **Admin:** when `m_user` is empty, the seeder creates `admin` (SUPERADMIN) through `UserAccount.BootstrapAdmin`, with the password from `--admin-password`, env `GWA_ADMIN_PASSWORD` or `admin12345`. Without `--reset` it refuses to run when `m_organization` already has a row.
- **Access catalog (`Seed.Akses.cs`):** `SyncAccess` is idempotent. It creates or updates controllers, header menus (matched by name), sub menus (matched by controller) and the catalog roles, and replaces the action list, menu details, role actions and role menus of catalog roles with the catalog. Roles outside the catalog are left alone. A catalog role gets every action of its controllers plus the Area Anggota menu.
- **Roles:** KETUA and WAKIL (all operational modules), SEKRETARIS, BENDAHARA, KADIV (Kepala Divisi), SECTION_LEADER (Section, Kegiatan, Presensi, Galeri, Aset, Peminjaman, Prestasi), ANGGOTA (Area Anggota only). Akses and Pengaturan Sistem stay SUPERADMIN only.
- **Users:** memorable usernames, password `gwa12345`: `ketua`, `wakil`, `sekretaris`, `bendahara`, `leader.brass` / `.battery` / `.guard` / `.front` (Section Leader), `kadiv.logistik` / `.humas`, `ketua.lama`, `anggota01`…`anggota11`, `alumni01`, `alumni02`. Positions are Ketua Umum, Wakil Ketua Umum, Sekretaris, Bendahara, Kepala Divisi, Staf Divisi and Section Leader. Sections: Brass, Battery, Front Ensemble, Colour Guard (every member gets one, a few move with history). Divisions (dummy): Humas & Media, Logistik, Danus with a few picked members. The seeder prints the account table at the end, and README lists it.
- **Code layout:** files are split per module (`Seed.Akses.cs`, `Seed.Reset.cs`, `Seed.Master.cs`, `Seed.Anggota.cs`, `Seed.Program.cs`, `Seed.Keuangan.cs`, `Seed.Lain.cs`), and `Seed.Run.cs` holds the order.
- **Workflow:** approvals, check-in, votes and form responses are done by the right member's user (`UserOf(member)`); the rest by admin.
- **Order matters:**
  - The old period's data goes in before the handover is approved, because a Closed period is locked.
  - Activities with status Done are only finished at the end of `SeedCommittee`, because Done locks their child data.
- Running the seeder end to end is the first test against a database for Keuangan … Information. It passed on 2026-11-16 after these fixes in the seeder itself (no library changes):
  - Alumni cannot be participants.
  - Incoming letters need a letter type.
  - Letter templates need a real `.docx` (zip).
  - `AttendanceSession.Close` returns `Response<int>`.

## Schema sources

- The ERD artifact is at https://claude.ai/artifact/MvmojvM5z8kKz7SEwtjdJV. It has 131 tables in modules `inti`, `akses`, `sistem`, `anggota`, `program`, `panitia`, `presensi`, `keuangan`, `mitra`, `sponsor`, `pengadaan`, `danus`, `inventaris`, `surat`, `evoting`, `lomba`, and others.
- The MySQL 8 DDL is `database/gitawidyaagni_create.sql` (database `gitawidyaagni`). The meaning of every coded `int` column is in its `COMMENT` (`1=..., 2=...`).
- `m_*` tables are masters and `t_*` tables are transactions.
- There is no AUTO_INCREMENT: ids come from `m_sequence`, except `t_election_vote.id`, which is random.

## Commands

```
dotnet build GitaWidyaAgni.sln
dotnet build GWAShared/GWAShared.csproj
```

There are no tests or linters. About 18 build warnings (SYSLIB0021/0022, CS860x) come from the helper files copied unchanged from ClubShared.

## GWAShared

- **Copied unchanged from ClubShared** (only the namespace was changed): `Results/Response`, `ResponseFactory`, `ResponseMessages`, `Extensions/*`, `Utilities/*`, `Models/*`, `Enums/ResponseStatus`, `Enums/Common/FilterAktif`. Behaviour notes from ClubSphere's CLAUDE.md still apply:
  - `MappingData` needs an `internal` constructor that takes the source row.
  - `EncryptString` / `DecryptString` are keyed by type name plus today's date. The key is 32 characters, so a class that uses them must have a name of **at most 24 characters**; a longer name throws at runtime. This is why `m_sponsorship_package_benefit` maps to `PackageBenefit`.
  - `ValidateDAModel` enforces `[StringLength]`.
- **`Results/ExceptionMessage.cs` was rebuilt** for this domain. Tenant, campus, staff, facility, assessment and sub-menu-group messages are gone, and partner messages became `EXTERNAL_PARTY_*`. Add new messages here as constants, grouped by module.
- **Enums**:
  - `Enums/<Folder>/` has one enum per file, all in namespace `GWAShared.Enums`.
  - Each enum's `<summary>` lists **every** `table.column` it is used for, plus an optional `| note`.
  - Values carry an Indonesian `[Description]`.
  - They are generated to match the SQL comments. When the schema changes, keep the summary column lists and values in sync with the DDL.
- **Enums that intentionally cover more values than a single column uses:**
  - `ItemCondition`: unit columns use only 1–3.
  - `NotificationChannel`: `t_message_outbox` uses only 2–3.
  - `PaymentStatus`: `TidakAdaTagihan = 4` exists only for `t_borrow_request`.
  - `MemberType`: 1, 2 and 4, with 3 unused.
- `t_procurement_approval.approval_level` is a step order, not a code, so it has no enum.
- `ScheduleDay` is 1=Senin … 7=Minggu and is not interchangeable with `System.DayOfWeek`.
