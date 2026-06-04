# AppendReviewsTo20260602Xls.ps1
# 対象: artifacts/review/20260602_レビュー記録票.xls
# 実行: バックアップを作成し、NO.2.0～2.2 のレビュー行をシート 'レビュー記録表'（なければ先頭シート）へ追記する
$repoRoot = Resolve-Path "$PSScriptRoot\.." | Select-Object -ExpandProperty Path
$target = Join-Path $repoRoot 'artifacts\review\20260602_レビュー記録票.xls'
if(-not (Test-Path $target)){
	Write-Error "対象ファイルが見つかりません: $target"
	exit 1
}
# backup
$bak = $target + '.bak.' + (Get-Date -Format yyyyMMddHHmmss)
Copy-Item -LiteralPath $target -Destination $bak -Force
# Prepare rows to append (10 columns to match template)
$rows = @(
	@('2.0','Controllers/StaffListController.cs','記述相違','page パラメータの上限チェックがないため、page が totalPages を超えるケースや totalPages が 0 になるケースでページ表示・操作が不正になる','コントローラで totalPages を最低 1 に設定し、page を 1..totalPages の範囲にクランプする処理を追加。pageSize=5 に基づく Skip/Take によるページング取得へ修正済み。','GitHub Copilot','2026/06/02','検討不足','佐野',''),
	@('2.1','Controllers/StaffListController.cs','記述漏れ','例外発生時に内部例外情報をそのままユーザーへ返している（情報漏洩の懸念）','catch 節を一般的なエラーメッセージへ置き換えました。運用では詳細はログへ記録する方針を推奨します。','GitHub Copilot','2026/06/02','検討不足','佐野',''),
	@('2.2','shinnzinn2026.Tests/Controllers/StaffListControllerTests.cs','その他','StaffList の認可・セッション・ページングに関する自動テストが存在しなかったため、回帰検知が困難であった','xUnit と InMemory DB を用いたテストを追加しました。セッション未設定で Login へリダイレクト、管理者でない場合 Login リダイレクト、page=2 のページングと ViewBag の検証を含むテストを作成。','GitHub Copilot','2026/06/02','習熟不足','佐野','')
)
# Open Excel COM and append
$excel = New-Object -ComObject Excel.Application
$excel.Visible = $false
$excel.DisplayAlerts = $false
$wb = $excel.Workbooks.Open($target)
$ws = $null
foreach($s in $wb.Worksheets){ if($s.Name -eq 'レビュー記録表'){ $ws = $s; break } }
if(-not $ws){ $ws = $wb.Worksheets.Item(1) }
$used = $ws.UsedRange
$lastRow = 0
if($used -ne $null -and $used.Rows.Count -gt 0){ $lastRow = [int]$used.Rows.Count } else { $lastRow = 0 }
$rowIndex = $lastRow + 1
foreach($r in $rows){ for($i=0; $i -lt 10; $i++){ $ws.Cells.Item($rowIndex, $i+1).Value2 = $r[$i] } $rowIndex++ }
$wb.Save()
$wb.Close($true)
$excel.Quit()
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($ws) | Out-Null
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($wb) | Out-Null
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($excel) | Out-Null
[GC]::Collect(); [GC]::WaitForPendingFinalizers(); Write-Output ("Append complete to $target (backup: $bak)")