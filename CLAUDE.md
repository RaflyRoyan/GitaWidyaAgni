# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

GitaWidyaAgni is a .NET 8 backend library for managing **a single organization** (members, management periods, programs/activities, attendance, finance, letters, e-voting, procurement, sponsorship, inventory, …). There is no tenant, campus, UKM category or staff: the organization is the top level and no table carries `tenant_id` / `campus_id` / `organization_id`. Domain terms, comments and user-facing messages are in Indonesian.

The structure is copied from the author's ClubSphere project (`C:\Users\Royan\source\repos\ClubSphere`, see its CLAUDE.md). Follow its conventions, but drop every multi-tenant / campus / UKM rule.

- **GWAShared**: dependency-free helpers (`Results/`, `Enums/`, `Extensions/`, `Utilities/`, `Models/`).
- **GWALib**: data access and business layer, referencing GWAShared. `EF/` holds the database-first EF Core model (entities + `ModelContext`), scaffolded with Pomelo. Re-scaffolding overwrites these files: keep `OnConfiguring` commented out so the connection string is never committed. The service layer (`Lib/`) does not exist yet.

## Schema sources

- The ERD artifact is at https://claude.ai/artifact/MvmojvM5z8kKz7SEwtjdJV. It has 130 tables in modules `inti`, `akses`, `sistem`, `anggota`, `program`, `panitia`, `presensi`, `keuangan`, `mitra`, `sponsor`, `pengadaan`, `danus`, `inventaris`, `surat`, `evoting`, `lomba`, and others.
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
  - `EncryptString` / `DecryptString` are keyed by type name plus today's date.
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
