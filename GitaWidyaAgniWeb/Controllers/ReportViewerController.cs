using DevExpress.AspNetCore.Reporting.WebDocumentViewer;
using DevExpress.AspNetCore.Reporting.WebDocumentViewer.Native.Services;
using GitaWidyaAgniWeb.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Controllers
{
    /// <summary>
    /// Endpoint DevExpress Web Document Viewer (route "DXXRDV" dari base class). Report dibuat dan di-Bind oleh
    /// controller halaman (mis. Inventaris/Aset/LabelQr); endpoint ini hanya melayani permintaan viewer
    /// (render halaman, export, print) untuk user yang sudah login.
    /// Antiforgery dilewati karena client viewer tidak mengirim header token; dokumen hanya bisa diakses lewat id
    /// yang dibuat server untuk halaman yang sudah lolos cek hak akses.
    /// </summary>
    [LoginOnly, IgnoreAntiforgeryToken]
    public class ReportViewerController : WebDocumentViewerController
    {
        public ReportViewerController(IWebDocumentViewerMvcControllerService controllerService) : base(controllerService)
        {
        }
    }
}
