# AppendToFilledCsv.ps1
# artifacts/review/... (3)-filled.csv に NO.2.0 から追記します
$csv = Join-Path $PSScriptRoot '..\artifacts\review\yyyymmdd_レビュー記録票_基本設計_画面設計_メインメニュー (3)-filled.csv'
if(-not (Test-Path $csv)){
	Write-Error "CSV が見つかりません: $csv"
	Exit 1
}
# バックアップ
$bak = "$csv.bak.$((Get-Date).ToString('yyyyMMddHHmmss'))"
Copy-Item -LiteralPath $csv -Destination $bak -Force
# 追記行を定義（各値は CSV の既存フォーマットに沿ってダブルクォートで囲む）
$rows = @(
	@('2.0','Controllers/StaffListController.cs','記述相違','page パラメータの上限チェックがないため、page が totalPages を超えるケースや totalPages が 0 になるケースでページ表示・操作が不正になる','コントローラ側で totalPages を最低 1 に設定し、page を 1..totalPages の範囲にクランプする処理を追加しました。pageSize=5 に基づく Skip/Take によるページング取得へ修正済み。','GitHub Copilot','2026/06/02','検討不足','佐野',''),
	@('2.1','Controllers/StaffListController.cs','記述漏れ','例外発生時に内部例外情報をそのままユーザーへ返している（情報漏洩の懸念）','catch 節を一般的なエラーメッセージへ置き換えました。運用では詳細はログへ記録する方針を推奨します。','GitHub Copilot','2026/06/02','検討不足','佐野',''),
	@('2.2','shinnzinn2026.Tests/Controllers/StaffListControllerTests.cs','その他','StaffList の認可・セッション・ページングに関する自動テストが存在しなかったため、回帰検知が困難であった','xUnit と InMemory DB を用いたテストを追加しました。セッション未設定で Login へリダイレクト、管理者でない場合 Login リダイレクト、page=2 のページングと ViewBag の検証を含むテストを作成。','GitHub Copilot','2026/06/02','習熟不足','佐野','')
)
# 既存フォーマットに合わせて各値を"で囲み、内部の"は""でエスケープ
function ToCsvLine($arr){
	$escaped = $arr | ForEach-Object { '"' + ($_ -replace '"','""') + '"' }
	return ($escaped -join ',')
}
foreach($r in $rows){
	$line = ToCsvLine $r
	[System.IO.File]::AppendAllText($csv, $line + [Environment]::NewLine, [System.Text.Encoding]::UTF8)
}
Write-Output "追記完了 (backup: $bak)"
