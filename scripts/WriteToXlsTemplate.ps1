# WriteToXlsTemplate.ps1
# 指定されたテンプレート xls の先頭シート（'レビュー記録表' があればそれ）に追記します。
# 実行前に Excel を閉じてください。
$xls = Join-Path $PSScriptRoot '..\artifacts\review\yyyymmdd_レビュー記録票_基本設計書_画面設計_メインメニュー (3).xls'
if(-not (Test-Path $xls)){
	Write-Error "テンプレートが見つかりません: $xls"
	exit 1
}
# Excel プロセスを終了（開いている場合）
Stop-Process -Name EXCEL -ErrorAction SilentlyContinue
# バックアップ
$bak = "$xls.bak.$((Get-Date).ToString('yyyyMMddHHmmss'))"
Copy-Item -LiteralPath $xls -Destination $bak -Force
# Excel COM 操作で追記
$excel = New-Object -ComObject Excel.Application
$excel.Visible = $false
$excel.DisplayAlerts = $false
$workbook = $excel.Workbooks.Open($xls)
# シート名が 'レビュー記録表' のものを探す。なければ先頭シート
$worksheet = $null
foreach($s in $workbook.Worksheets){ if($s.Name -eq 'レビュー記録表'){ $worksheet = $s; break } }
if(-not $worksheet){ $worksheet = $workbook.Worksheets.Item(1) }
# 最終行を取得
$used = $worksheet.UsedRange
if($used -ne $null -and $used.Rows.Count -gt 0){ $lastRow = [int]$used.Rows.Count } else { $lastRow = 0 }
$rowIndex = $lastRow + 1
# 追記データ（NO.2.0 から）
$rows = @(
	@('2.0','Controllers/StaffListController.cs','記述相違','page パラメータの上限チェックがないため、page が totalPages を超えるケースや totalPages が 0 になるケースでページ表示・操作が不正になる','コントローラ側で totalPages を最低 1 に設定し、page を 1..totalPages の範囲にクランプする処理を追加しました。pageSize=5 に基づく Skip/Take によるページング取得へ修正済み。','GitHub Copilot','2026/06/02','検討不足','佐野',''),
	@('2.1','Controllers/StaffListController.cs','記述漏れ','例外発生時に内部例外情報をそのままユーザーへ返している（情報漏洩の懸念）','catch 節を一般的なエラーメッセージへ置き換えました。運用では詳細はログへ記録する方針を推奨します。','GitHub Copilot','2026/06/02','検討不足','佐野',''),
	@('2.2','shinnzinn2026.Tests/Controllers/StaffListControllerTests.cs','その他','StaffList の認可・セッション・ページングに関する自動テストが存在しなかったため、回帰検知が困難であった','xUnit と InMemory DB を用いたテストを追加しました。セッション未設定で Login へリダイレクト、管理者でない場合 Login リダイレクト、page=2 のページングと ViewBag の検証を含むテストを作成。','GitHub Copilot','2026/06/02','習熟不足','佐野','')
)
try{
	foreach($r in $rows){
		for($i=0;$i -lt $r.Length;$i++){
			$worksheet.Cells.Item($rowIndex, $i+1).Value2 = $r[$i]
		}
		$rowIndex++
	}
	$workbook.Save()
	Write-Output "追記完了。バックアップ: $bak"
} catch {
	Write-Error "Excel 操作中にエラー: $_"
	throw
} finally {
	if($workbook -ne $null){ $workbook.Close($true); [System.Runtime.Interopservices.Marshal]::ReleaseComObject($workbook) | Out-Null }
	if($worksheet -ne $null){ [System.Runtime.Interopservices.Marshal]::ReleaseComObject($worksheet) | Out-Null }
	if($excel -ne $null){ $excel.Quit(); [System.Runtime.Interopservices.Marshal]::ReleaseComObject($excel) | Out-Null }
	[GC]::Collect(); [GC]::WaitForPendingFinalizers()
}
