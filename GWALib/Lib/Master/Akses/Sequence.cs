using GWALib.General;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Master.Akses
{
    /// <summary>
    /// Master Sequence (m_sequence): Id terakhir per tabel, dipakai SequenceUtility untuk membuat id. Primary key-nya nama tabel.
    /// LastId tidak boleh di bawah id terbesar tabelnya; menghapus baris membuat SequenceUtility mengisinya ulang dari MAX(id).
    /// </summary>
    public partial class Sequence : BaseLib
    {
        public class NewRow
        {
            public string TableName { get; set; } = null!;
            public int LastId { get; set; }
        }

        public string TableName { get; set; } = null!;
        public int LastId { get; set; }
        public DateTime? UpdDate { get; set; }

        EF.ModelContext _context = null!;
        private IQueryable<EF.MSequence> _Query
        {
            get
            {
                return _context.MSequences.AsQueryable();
            }
        }
        private static IQueryable<EF.MSequence> _Querys
        {
            get
            {
                return DBClass.GetContext().MSequences.AsQueryable();
            }
        }
        public Sequence()
        {
            _context = DBClass.GetContext();
        }
        public Sequence(string encKey)
        {
            _context = DBClass.GetContext();
            var tableName = encKey.DecryptString<Sequence>();
            var row = _Query.SingleOrDefault(x => x.TableName == tableName);
            if (row != null)
            {
                SetData(row);
            }
        }
        // isKey hanya pembeda dari constructor encKey.
        internal Sequence(string tableName, bool isKey)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.TableName == tableName);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal Sequence(EF.MSequence row)
        {
            SetData(row);
        }
        private void SetData(EF.MSequence row)
        {
            TableName = row.TableName;
            EncDecrKey = TableName.EncryptString<Sequence>();
            LastId = row.LastId;
            UpdDate = row.UpdDate;
        }
    }
}
