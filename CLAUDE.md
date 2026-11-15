# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

GitaWidyaAgni is a .NET 8 backend library for managing **a single organization** (members, management periods, programs/activities, attendance, finance, letters, e-voting, procurement, sponsorship, inventory, …). There is no tenant, campus, UKM category or staff: the organization is the top level and no table carries `tenant_id` / `campus_id` / `organization_id`. Domain terms, comments and user-facing messages are in Indonesian.

The structure is copied from the author's ClubSphere project (`C:\Users\Royan\source\repos\ClubSphere`, see its CLAUDE.md). Follow its conventions, but drop every multi-tenant / campus / UKM rule.

- **GWAShared**: dependency-free helpers (`Results/`, `Enums/`, `Extensions/`, `Utilities/`, `Models/`).
- **GWALib**: data access and business layer, referencing GWAShared. `EF/` holds the database-first EF Core model (entities + `ModelContext`), scaffolded with Pomelo. Re-scaffolding overwrites these files: keep `OnConfiguring` commented out so the connection string is never committed.

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
      - Use `GetByCode` for QR scans and `MarkLabelPrinted` after printing.
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
  - `ManagementAssignment`:
    - A core position (`is_core`) can have only one current holder per (period, position, division).
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
    - Accepted checks the quota and creates an `m_member` through `Member.CreateRow`, which is now internal.
  - `PeriodHandover` and `PeriodHandoverItem`:
    - One handover per source period. Draft → Diajukan → Disetujui / Ditolak, and Ditolak can be edited and resubmitted.
    - Data, items and the LPJ file can be changed only in Draft or Ditolak.
    - `Approve` runs in one DB transaction. It closes the source period, activates the target period, ends the source period's assignments, and generates the number from format `BA_ST` when it is empty and the format exists.
    - Item `RefId` is checked against the table for its `ItemType`. `MouAktif` maps to `t_sponsorship_mou`.
- `Lib/Transaksi/Program/` (all new):
  - `WorkProgram`:
    - Maps `t_program`. The class name avoids clashing with the namespace.
    - Planned → Ongoing → Done, and Planned/Ongoing → Cancelled.
    - The previous program must come from another period. Delete is allowed only for a Planned program that nothing uses.
    - `internal GetOpenProgram` is available to other classes.
  - `ProgramTask`:
    - A task on a program takes the program's period; a general task needs `PeriodId`.
    - Belum → Proses → Selesai, and Selesai can be reopened to Proses.
    - Selesai records `completed_at` and `done_by`.
  - `Activity`:
    - The parent activity may be only one level deep.
    - Lomba requires `CompetitionId`, audience Divisi requires `DivisionId`, and audience TimPelaksana requires a parent. Fields that do not apply are cleared.
    - Featured requires public. `GetPublicList` serves the landing page.
    - Delete is allowed only for a Planned activity with no dependents. `internal GetActivity` and `GetOpenActivity` are available.
  - `ActivityCollaborator`: `Respond` moves Diundang to Diterima or Ditolak. Blacklisted parties are refused.
  - `ActivityParticipant`: unique per activity. Registering again after Cancelled reactivates the old row.
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
    - Expected members come from the activity audience through `internal GetExpectedMemberIds`.
    - A session cannot be cancelled once someone has checked in.
  - `AttendanceRecord`:
    - `CheckIn` / `CheckOut` take the QR token from the logged-in user's member. The session must be Open and within its time window. The member must be in the audience, unless the audience is all members.
    - The geofence is checked, and a photo is required (`ImageUtility`), stored in `t_attendance_record_in` / `_out`. Check-in after `open_at` plus tolerance is Terlambat.
    - `SetManual` lets staff record Izin, Sakit, Alpa or Hadir and correct automatic Alpa. A row that has a check-in can only be Hadir or Terlambat.
  - `MeetingMinutes`:
    - One per Open or Closed session. Draft → Final, and Final is locked.
    - The attachment is not loaded in lists. Filtering by activity happens before the projection (`Project`).
- Transaction order: Approval → Akses → Anggota → Program → TimPelaksana → Presensi → Keuangan → Sponsorship → Pengadaan → Merchandise → Inventaris → Surat → Prestasi → Formulir → EVoting → Information.

### Transactions not done yet (status 2026-11-15)

Done: `Approval`, `Akses`, `Anggota`, `Program`, `TimPelaksana`, `Presensi` (32 of 88 `t_*` tables). Still to do, in this order (56 tables):

| # | Folder | Tables | Notes for the design |
|---|---|---|---|
| 1 | `Keuangan` (13) | `t_financial_transaction`, `t_financial_attachment`, `t_dues_bill`, `t_budget`, `t_budget_item`, `t_budget_document`, `t_funding_request`, `t_funding_approval`, `t_funding_realization`, `t_period_balance`, `t_debt`, `t_debt_attachment`, `t_debt_payment` | Draft → Posted → Void transactions numbered with `DocumentNumberSeq`. Funding requests follow `m_approval_flow` (FUNDING_REQUEST) step by step. Dues bills are generated from `m_dues_setting` and their payment creates an Income transaction. Debt `paid_amount` / status come from the payments. Period balances close the period. Lock `m_financial_account` opening balance once used. |
| 2 | `Sponsorship` (3) | `t_sponsorship`, `t_sponsorship_benefit`, `t_sponsorship_mou` | Copy benefits from `m_sponsorship_package_benefit`. Income through `t_financial_transaction.sponsorship_id`. Refuse blacklisted parties. |
| 3 | `Pengadaan` (6) | `t_procurement_request`, `t_procurement_item`, `t_procurement_quotation`, `t_procurement_approval`, `t_procurement_receipt`, `t_procurement_receipt_item` | Approval via `m_approval_flow` (PROCUREMENT). A receipt can create `m_asset` / `m_asset_unit` (`procurement_item_id`) and a product stock movement. |
| 4 | `Merchandise` (3) | `t_sales_order`, `t_sales_order_item`, `t_product_stock_movement` | Stock per product or per variant (`track_stock`), pre-order, payment through `sales_order_id`. |
| 5 | `Inventaris` (6) | `t_borrow_request`, `t_borrow_item`, `t_borrow_item_unit`, `t_asset_movement`, `t_asset_inspection`, `t_maintenance_log` | Lending of quantity or per-unit assets (QR), rental scheme / late fee, return status per unit, asset / unit status updates, `AssetUnit` integration. `Asset.CreateMovement` is already internal. |
| 6 | `Surat` (4) | `t_letter`, `t_letter_approval`, `t_letter_disposition`, `t_document` | Letter numbering via `DocumentNumberSeq`, approval via `m_approval_flow` (LETTER) or position, disposition to members. |
| 7 | `Prestasi` (6) | `t_competition`, `t_competition_material`, `t_competition_participant`, `t_achievement`, `t_certificate`, `t_member_point` | Certificate number plus `verification_code`. Points via `PointRule.GetEffectiveRow`, unique per (member, source, ref). |
| 8 | `Formulir` (5) | `t_form`, `t_form_question`, `t_form_response`, `t_form_answer`, `t_aspiration` | Forms for activity evaluation and recruitment questions (`Recruitment` / `CommitteeRecruitment` already reference `t_form`). Anonymous aspirations. |
| 9 | `EVoting` (6) | `t_election`, `t_election_candidate`, `t_election_eligible_voter`, `t_election_vote`, `t_election_result`, `t_election_audit_event` | `t_election_vote.id` is random (not `m_sequence`). The vote must be anonymous and encrypted. Audit events, and results on finalize. |
| 10 | `Information` (4) | `t_announcement`, `t_banner`, `t_notification`, `t_activity_log` | Landing page content, in-app notifications (use `UserNotificationPref`), activity log. |

Open items not tied to one folder:
- **Pending user decisions:**
  - The DDL comment on `t_period_handover_item.ref_id` still names `t_partnership_agreement` for MOU_AKTIF. The code uses `t_sponsorship_mou`; fixing the comment awaits the user's OK.
  - There is no column for an email-verified flag (`UserToken.VerifyEmail` only consumes the token) and none for report revision notes (`ActivityReport`). Adding them is up to the user.
- **Not committed:** nothing since commit `c82d762` has been committed. The user wants one commit after the transactions are done; commit only when asked.
- **Test programs:** the test scenarios are in the scratchpad (`tests/*.cs`, run through `seqtest`) and are not part of the repo. Every test cleaned up its data and reset `m_sequence` to 0.

## Schema sources

- The ERD artifact is at https://claude.ai/artifact/MvmojvM5z8kKz7SEwtjdJV. It has 131 tables in modules `inti`, `akses`, `sistem`, `anggota`, `program`, `panitia`, `presensi`, `keuangan`, `mitra`, `sponsor`, `pengadaan`, `danus`, `inventaris`, `surat`, `evoting`, `lomba`, and others.
- The MySQL 8 DDL is at `C:\Users\Royan\Documents\gitawidyaagni-sql\gitawidyaagni_create.sql` (database `gitawidyaagni`). The meaning of every coded `int` column is in its `COMMENT` (`1=..., 2=...`).
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
