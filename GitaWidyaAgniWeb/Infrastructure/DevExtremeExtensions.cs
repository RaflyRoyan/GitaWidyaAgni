using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Mvc.Builders;
using DevExtreme.AspNet.Mvc.Factories;

namespace GitaWidyaAgniWeb.Infrastructure
{
    /// <summary>
    /// Standar DataGrid semua halaman (diambil dari BEGateSecuritySystemWeb). Pakai ListGrid untuk grid daftar utama
    /// dan grid detail di modal; jangan menyetel ulang border, paging, filter, atau export per halaman.
    /// </summary>
    public static class DevExtremeExtensions
    {
        /// <summary>
        /// Grid list standar: garis baris + garis kolom (tanpa border luar), search, filter per baris, header filter,
        /// multi sort, pager 10/20/50 + info, export Excel &amp; PDF lewat handler global gridExport (js/grid-export.js).
        /// <paramref name="exportName"/> = nama file + judul PDF (atribut data-export-name).
        /// </summary>
        public static DataGridBuilder<T> ListGrid<T>(this DataGridBuilder<T> grid, string exportName)
        {
            return grid
                .ElementAttr("data-export-name", exportName)
                .ElementAttr("class", "gwa-list-grid")
                .ShowBorders(false)
                .ShowColumnLines(true)
                .ShowRowLines(true)
                .HoverStateEnabled(true)
                .ColumnAutoWidth(true)
                .AllowColumnResizing(true)
                .WordWrapEnabled(true)
                .NoDataText("Tidak ada data")
                .SearchPanel(s => s.Visible(true).Width(260).Placeholder("Cari..."))
                .FilterRow(f => f.Visible(true))
                .HeaderFilter(h => h.Visible(true))
                .Sorting(s => s.Mode(GridSortingMode.Multiple))
                .Paging(p => p.PageSize(10))
                .Pager(p => p.Visible(true).ShowPageSizeSelector(true).AllowedPageSizes(new[] { 10, 20, 50 }).ShowInfo(true).InfoText("Halaman {0} dari {1} ({2} data)"))
                .Export(e => e.Enabled(true).AllowExportSelectedData(false).Formats(new[] { "xlsx", "pdf" }))
                .OnExporting("gridExport");
        }

        /// <summary>Grid detail kecil di modal (tanpa filter baris dan export): garis, search, paging 10.</summary>
        public static DataGridBuilder<T> DetailGrid<T>(this DataGridBuilder<T> grid, string noDataText = "Tidak ada data")
        {
            return grid
                .ElementAttr("class", "gwa-list-grid")
                .ShowBorders(true)
                .ShowColumnLines(true)
                .ShowRowLines(true)
                .HoverStateEnabled(true)
                .ColumnAutoWidth(true)
                .WordWrapEnabled(true)
                .NoDataText(noDataText)
                .Paging(p => p.PageSize(10))
                .Pager(p => p.Visible(true).ShowInfo(true).InfoText("Halaman {0} dari {1} ({2} data)"));
        }

        /// <summary>Kolom "#" nomor urut lintas halaman.</summary>
        public static void AddRowNumber<T>(this CollectionFactory<DataGridColumnBuilder<T>> columns)
        {
            columns.Add()
                .Caption("#")
                .Width(50)
                .Alignment(HorizontalAlignment.Center)
                .AllowSorting(false)
                .AllowFiltering(false)
                .AllowHeaderFiltering(false)
                .CellTemplate(new JS("AppUI.rowNumberTemplate"));
        }

        /// <summary>Kolom tombol aksi (tidak ikut export, tanpa sort/filter); template dari script halaman.</summary>
        public static void AddAction<T>(this CollectionFactory<DataGridColumnBuilder<T>> columns, string template, string caption = "Aksi")
        {
            columns.Add()
                .Caption(caption)
                .Alignment(HorizontalAlignment.Center)
                .AllowSorting(false)
                .AllowFiltering(false)
                .AllowHeaderFiltering(false)
                .CssClass("gwa-grid-actions")
                .CellTemplate(new JS(template));
        }
    }
}
