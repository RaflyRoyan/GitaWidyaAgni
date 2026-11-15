using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_borrow_request.status
    /// </summary>
    public enum BorrowRequestStatus
    {
        [Description("Menunggu")] Pending = 1,
        [Description("Disetujui")] Approved = 2,
        [Description("Ditolak")] Rejected = 3,
        [Description("Dipinjam")] Borrowed = 4,
        [Description("Dikembalikan")] Returned = 5,
        [Description("Terlambat")] Overdue = 6,
        [Description("Dibatalkan")] Cancelled = 7
    }
}
