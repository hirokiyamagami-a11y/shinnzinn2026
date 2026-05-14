using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace shinnzinn2026.Work_List
{
    // クラス名はcshtml側の「@model」で指定したものと一致させる必要があります
    public class WorkListModel : PageModel
    {
        // 画面に表示する年月の初期値（今の年月）
        [BindProperty(SupportsGet = true)]
        public int SelectedYear { get; set; } = DateTime.Now.Year;

        [BindProperty(SupportsGet = true)]
        public int SelectedMonth { get; set; } = DateTime.Now.Month;

        // ページが読み込まれた時に動く処理
        public void OnGet()
        {
            // ここに「DBからデータを取ってくる」などの処理を後で書きます
            // 今はレイアウト確認用なので、空っぽでも大丈夫です
        }
    }
}