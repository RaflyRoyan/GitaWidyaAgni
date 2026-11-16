/*
 * Export DataGrid bersama (Excel + PDF) untuk semua halaman. Dimuat global di _Layout
 * (butuh exceljs, FileSaver dan jsPDF yang juga dimuat di _Layout; diambil dari BEGateSecuritySystemWeb).
 *
 * Pemakaian di grid:
 *   .ElementAttr("data-export-name", "User")   // nama file + judul PDF (opsional; default = judul halaman)
 *   .Export(e => e.Enabled(true).AllowExportSelectedData(false).Formats(new[] { "xlsx", "pdf" }))
 *   .OnExporting("gridExport")
 *
 * Kolom tanpa DataField (Action, "#") tidak ikut diexport. Nilai boolean ditulis Yes/No.
 */
(function () {
    function exportName(grid) {
        const fromAttr = $.trim(grid.element().attr('data-export-name') || '');
        if (fromAttr) return fromAttr;
        const pageTitle = $.trim($('.page-title-box h4').first().text());
        return pageTitle || 'Data';
    }

    function fileStamp() {
        const d = new Date();
        const pad = function (n) { return String(n).padStart(2, '0'); };
        return d.getFullYear() + pad(d.getMonth() + 1) + pad(d.getDate()) + '_' + pad(d.getHours()) + pad(d.getMinutes());
    }

    function cellText(gridCell) {
        if (gridCell.rowType === 'data' && typeof gridCell.value === 'boolean') return gridCell.value ? 'Yes' : 'No';
        return null;
    }

    // Sembunyikan sementara kolom non-data (Action, nomor urut) selama export; dikembalikan sesudahnya.
    function hideNonDataColumns(grid) {
        const hidden = [];
        grid.beginUpdate();
        grid.getVisibleColumns().forEach(function (col) {
            // col.type terisi untuk kolom command (panah detail/group) — itu tidak diexport exporter, jangan disentuh.
            if (!col.dataField && !col.type && col.index != null) {
                grid.columnOption(col.index, 'visible', false);
                hidden.push(col.index);
            }
        });
        grid.endUpdate();
        return function restore() {
            grid.beginUpdate();
            hidden.forEach(function (index) { grid.columnOption(index, 'visible', true); });
            grid.endUpdate();
        };
    }

    function exportExcel(grid, name) {
        const workbook = new ExcelJS.Workbook();
        const worksheet = workbook.addWorksheet(name.substring(0, 31) || 'Data');
        return DevExpress.excelExporter.exportDataGrid({
            component: grid,
            worksheet: worksheet,
            autoFilterEnabled: true,
            customizeCell: function (options) {
                const text = cellText(options.gridCell);
                if (text !== null) options.excelCell.value = text;
            }
        }).then(function () {
            return workbook.xlsx.writeBuffer();
        }).then(function (buffer) {
            saveAs(new Blob([buffer], { type: 'application/octet-stream' }), name + '_' + fileStamp() + '.xlsx');
        });
    }

    function exportPdf(grid, name) {
        const doc = new window.jspdf.jsPDF({ orientation: 'landscape', unit: 'pt', format: 'a4' });
        doc.setFontSize(13);
        doc.text(name, 40, 36);
        doc.setFontSize(8);
        doc.setTextColor(110);
        doc.text('Dicetak: ' + new Date().toLocaleString('id-ID'), 40, 50);
        return DevExpress.pdfExporter.exportDataGrid({
            jsPDFDocument: doc,
            component: grid,
            topLeft: { x: 0, y: 50 },
            margin: { top: 10, left: 40, right: 40, bottom: 30 },
            customizeCell: function (options) {
                const text = cellText(options.gridCell);
                if (text !== null) options.pdfCell.text = text;
                if (options.gridCell.rowType === 'header') {
                    options.pdfCell.backgroundColor = '#9a6b00'; // bronze GWA, sama dengan header grid (gwa-theme.css)
                    options.pdfCell.textColor = '#ffffff';
                    options.pdfCell.font = { style: 'bold' };
                }
            }
        }).then(function () {
            doc.save(name + '_' + fileStamp() + '.pdf');
        });
    }

    /** Handler onExporting DataGrid: format dari menu export (xlsx / pdf). */
    window.gridExport = function (e) {
        e.cancel = true;
        const grid = e.component;
        const name = exportName(grid);
        const restore = hideNonDataColumns(grid);
        const run = e.format === 'pdf' ? exportPdf : exportExcel;

        new Promise(function (resolve) { resolve(run(grid, name)); }).then(restore, function (err) {
            restore();
            console.error(err);
            AppUI.alertError('Export ' + (e.format === 'pdf' ? 'PDF' : 'Excel') + ' gagal.');
        });
    };
})();
