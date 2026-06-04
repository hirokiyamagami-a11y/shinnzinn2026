# AppendTo20260601Review.ps1
$target = 'artifacts/review/20260601_review.csv'
if(-not (Test-Path $target)){
	Write-Error "対象ファイルが見つかりません: $target"
	exit 1
}
$bak = $target + '.bak.' + (Get-Date -Format yyyyMMddHHmmss)
Copy-Item -LiteralPath $target -Destination $bak -Force
$content = @'
2.0,Controllers/StaffListController.cs,記述相違,page パラメータの上限チェックがないため、page が totalPages を超えるケースや totalPages が 0 になるケースでページ表示・操作が不正になる,コントローラで totalPages を最低 1 に設定し、page を 1..totalPages の範囲にクランプする処理を追加。pageSize=5 に基づく Skip/Take によるページング取得へ修正済み。,GitHub Copilot,2026/06/02,検討不足,佐野,,,,,,,,
2.1,Controllers/StaffListController.cs,記述漏れ,例外発生時に内部例外情報をそのままユーザーへ返している（情報漏洩の懸念）,catch 節を一般的なエラーメッセージへ置き換えました。運用では詳細はログへ記録する方針を推奨します。,GitHub Copilot,2026/06/02,検討不足,佐野,,,,,,,,
2.2,shinnzinn2026.Tests/Controllers/StaffListControllerTests.cs,その他,StaffList の認可・セッション・ページングに関する自動テストが存在しなかったため、回帰検知が困難であった,xUnit と InMemory DB を用いたテストを追加しました。セッション未設定で Login へリダイレクト、管理者でない場合 Login リダイレクト、page=2 のページングと ViewBag の検証を含むテストを作成。,GitHub Copilot,2026/06/02,習熟不足,佐野,,,,,,,,
'@
[System.IO.File]::AppendAllText($target, [Environment]::NewLine + $content, [System.Text.Encoding]::UTF8)
Write-Output "Append complete (backup: $bak)"
