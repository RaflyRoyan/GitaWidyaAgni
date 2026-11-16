namespace GitaWidyaAgniWeb.Infrastructure
{
    /// <summary>Model partial _Toolbar: tombol Tambah (opsional) + legenda tombol aksi grid.</summary>
    public class ToolbarModel
    {
        public string? AddText { get; set; }
        public string? AddOnClick { get; set; }
        public IEnumerable<(string Css, string Icon, string Text)> Legend { get; set; }
        /// <summary>Id panel legenda; beri id lain bila satu halaman punya beberapa toolbar (mis. per tab).</summary>
        public string LegendId { get; set; } = "legendInline";

        public ToolbarModel(string? addText, string? addOnClick, params (string Css, string Icon, string Text)[] legend)
        {
            AddText = addText;
            AddOnClick = addOnClick;
            Legend = legend;
        }
    }
}
