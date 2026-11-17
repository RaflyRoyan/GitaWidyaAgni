using GWALib.Lib.Master.Akses;
using GWALib.Lib.Transaksi.Akses;
using GWALib.Lib.Transaksi.Approval;
using GWALib.Lib.Transaksi.EVoting;
using GWALib.Lib.Transaksi.Information;
using GWALib.Lib.Transaksi.Inventaris;
using GWALib.Lib.Transaksi.Presensi;
using GWAShared.Enums;
using GWAShared.Results;

namespace GitaWidyaAgniWeb.Infrastructure.Jobs
{
    /// <summary>
    /// Pengaturan job terjadwal (section "Jobs").
    /// Enabled: false mematikan semua job. Username: user yang dipakai job (ins_by / upd_by); kosong = user SUPERADMIN aktif pertama.
    /// </summary>
    public class JobOptions
    {
        public bool Enabled { get; set; } = true;
        public string? Username { get; set; }
        // Batas umur data yang dibersihkan job PURGE.
        public int NotificationRetentionDays { get; set; } = 90;
        public int ActivityLogRetentionDays { get; set; } = 365;
        public int TokenRetentionDays { get; set; } = 30;
        public int JobLogRetentionDays { get; set; } = 90;
        // Rentang hari ke depan untuk membuat sesi presensi dari jadwal rutin.
        public int SessionDaysAhead { get; set; } = 7;
    }

    /// <summary>
    /// Menjalankan job terjadwal README "Job terjadwal" di dalam proses web, dicek tiap menit.
    /// Setiap run dicatat lewat ScheduledJobLog (Start / Finish / Fail); jadwal berikutnya dihitung dari run terakhir di DB,
    /// sehingga restart aplikasi tidak menjalankan ulang job harian. SEND_OUTBOX hanya dicatat bila ada pesan yang jatuh tempo.
    /// Untuk beberapa instance web, aktifkan Jobs:Enabled di satu instance saja.
    /// </summary>
    public class ScheduledJobService : BackgroundService
    {
        public const string JOB_SEND_OUTBOX = "SEND_OUTBOX";
        public const string JOB_CLOSE_ELECTION = "CLOSE_EXPIRED_ELECTION";
        public const string JOB_BORROW_OVERDUE = "MARK_BORROW_OVERDUE";
        public const string JOB_GENERATE_SESSIONS = "GENERATE_ATTENDANCE_SESSIONS";
        public const string JOB_PURGE = "PURGE_OLD_DATA";
        private const int OUTBOX_BATCH = 50;

        private record Job(string Name, TimeSpan Interval, Func<string, CancellationToken, Task<(int Count, string? Message)>> Run, bool LogWhenIdle = true);

        private readonly JobOptions _options = new();
        private readonly MessageSender _sender;
        private readonly ILogger<ScheduledJobService> _logger;
        private readonly Dictionary<string, DateTime> _nextRun = new();
        private readonly List<Job> _jobs;
        private string? _user;

        public ScheduledJobService(IConfiguration config, MessageSender sender, ILogger<ScheduledJobService> logger)
        {
            config.GetSection("Jobs").Bind(_options);
            _sender = sender;
            _logger = logger;
            _jobs = new()
            {
                new Job(JOB_SEND_OUTBOX, TimeSpan.FromMinutes(1), SendOutboxAsync, LogWhenIdle: false),
                new Job(JOB_CLOSE_ELECTION, TimeSpan.FromMinutes(5), (u, t) => Task.FromResult((Election.CloseExpired(), (string?)null))),
                new Job(JOB_BORROW_OVERDUE, TimeSpan.FromMinutes(30), (u, t) => Task.FromResult((BorrowRequest.MarkOverdue(u).Ok(), (string?)null))),
                new Job(JOB_GENERATE_SESSIONS, TimeSpan.FromHours(6), GenerateSessionsAsync),
                new Job(JOB_PURGE, TimeSpan.FromDays(1), PurgeAsync)
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.Enabled)
            {
                _logger.LogInformation("Job terjadwal dimatikan (Jobs:Enabled = false)");
                return;
            }
            // Beri waktu aplikasi selesai start.
            await Delay(TimeSpan.FromSeconds(20), stoppingToken);
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
            while (!stoppingToken.IsCancellationRequested)
            {
                await RunCycleAsync(stoppingToken);
                try
                {
                    if (!await timer.WaitForNextTickAsync(stoppingToken))
                    {
                        break;
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private async Task RunCycleAsync(CancellationToken token)
        {
            try
            {
                _user ??= ResolveUser();
                if (_user == null)
                {
                    _logger.LogWarning("Job terjadwal belum jalan: tidak ada user job (isi Jobs:Username atau buat user SUPERADMIN)");
                    return;
                }
                foreach (var job in _jobs)
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    await RunIfDueAsync(job, token);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Siklus job terjadwal gagal");
            }
        }

        private async Task RunIfDueAsync(Job job, CancellationToken token)
        {
            var now = DateTime.Now;
            if (!_nextRun.TryGetValue(job.Name, out var next))
            {
                // Pertama kali: lanjutkan dari run terakhir yang tercatat.
                var last = ScheduledJobLog.GetLastRun(job.Name);
                next = last == null ? now : last.StartedAt + job.Interval;
            }
            if (next > now)
            {
                _nextRun[job.Name] = next;
                return;
            }
            _nextRun[job.Name] = now + job.Interval;

            if (!job.LogWhenIdle)
            {
                var (count, message) = await job.Run(_user!, token);
                if (count > 0)
                {
                    _logger.LogInformation("{Job}: {Count} diproses. {Message}", job.Name, count, message);
                }
                return;
            }

            var start = ScheduledJobLog.Start(job.Name, _user!);
            if (start.Status != ResponseStatus.Success || start.Data == null)
            {
                _logger.LogWarning("{Job} tidak dijalankan: {Message}", job.Name, start.Message);
                return;
            }
            try
            {
                var (count, message) = await job.Run(_user!, token);
                start.Data.Finish(count, message, _user!);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Job {Job} gagal", job.Name);
                start.Data.Fail(ex.Message, 0, _user!);
            }
        }

        // Dicatat di ScheduledJobLog hanya bila ada pesan jatuh tempo pada saluran yang sudah dikonfigurasi.
        private async Task<(int, string?)> SendOutboxAsync(string user, CancellationToken token)
        {
            var due = MessageOutbox.GetDueList(OUTBOX_BATCH).Where(x => _sender.CanSend(x.Channel)).ToList();
            if (due.Count == 0)
            {
                return (0, null);
            }
            var start = ScheduledJobLog.Start(JOB_SEND_OUTBOX, user);
            if (start.Status != ResponseStatus.Success || start.Data == null)
            {
                return (0, start.Message);
            }
            int sent = 0, failed = 0;
            foreach (var message in due)
            {
                try
                {
                    await _sender.SendAsync(message, token);
                    message.MarkSent(user);
                    sent++;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    failed++;
                    message.MarkFailed(ex.Message, user);
                }
            }
            var summary = $"{sent} terkirim, {failed} gagal";
            start.Data.Finish(sent, summary, user);
            return (sent, summary);
        }

        private Task<(int, string?)> GenerateSessionsAsync(string user, CancellationToken token)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var count = ActivitySchedule.GenerateSessions(today, today.AddDays(_options.SessionDaysAhead), null, user).Ok();
            return Task.FromResult((count, (string?)$"{today:yyyy-MM-dd} s.d. +{_options.SessionDaysAhead} hari"));
        }

        private Task<(int, string?)> PurgeAsync(string user, CancellationToken token)
        {
            var now = DateTime.Now;
            var notifications = Notification.Purge(_options.NotificationRetentionDays);
            var logs = ActivityLog.Purge(now.AddDays(-_options.ActivityLogRetentionDays));
            var tokens = UserToken.Purge(now.AddDays(-_options.TokenRetentionDays), user).Ok();
            var jobs = ScheduledJobLog.Purge(now.AddDays(-_options.JobLogRetentionDays), user).Ok();
            return Task.FromResult((notifications + logs + tokens + jobs,
                (string?)$"notifikasi {notifications}, log aktivitas {logs}, token {tokens}, log job {jobs}"));
        }

        private string? ResolveUser()
        {
            var users = UserAccount.GetDataList(FilterAktif.Aktif);
            var user = !string.IsNullOrWhiteSpace(_options.Username)
                ? users.FirstOrDefault(x => string.Equals(x.Username, _options.Username, StringComparison.OrdinalIgnoreCase))
                : users.OrderBy(x => x.Id).FirstOrDefault(x => UserAccess.GetRoleCodes(x.Id).Contains(UserAccess.SUPERADMIN));
            return user?.Id.ToString();
        }

        private static async Task Delay(TimeSpan delay, CancellationToken token)
        {
            try
            {
                await Task.Delay(delay, token);
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
