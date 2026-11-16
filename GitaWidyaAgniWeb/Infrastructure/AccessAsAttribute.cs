namespace GitaWidyaAgniWeb.Infrastructure
{
    /// <summary>
    /// Hak akses aksi ini dicek sebagai aksi lain di controller yang sama, sesuai kosakata aksi yang di-seed
    /// (Index, Create, Edit, Delete, Submit, Approve, Process, Payment). Contoh: endpoint data <c>List</c> memakai
    /// <c>[AccessAs("Index")]</c>, sehingga role yang boleh membuka halaman juga boleh membaca datanya.
    /// Bisa dipasang di controller (berlaku untuk semua aksi tanpa atribut sendiri) atau di aksi.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class AccessAsAttribute : Attribute
    {
        public string Action { get; }

        public AccessAsAttribute(string action)
        {
            Action = action;
        }
    }

    /// <summary>Nama aksi baku yang terdaftar di m_controller_action (lihat GWASeeder/Seed.Akses.cs).</summary>
    public static class Acc
    {
        public const string Index = "Index";
        public const string Create = "Create";
        public const string Edit = "Edit";
        public const string Delete = "Delete";
        public const string Submit = "Submit";
        public const string Approve = "Approve";
        public const string Process = "Process";
        public const string Payment = "Payment";
    }
}
