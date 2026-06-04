# MergeCsvsToXlsxByHeader.ps1
# Merge three CSVs into existing formatted .xls and save as new .xlsx
$RepoRoot = Split-Path -Parent $PSScriptRoot
$ReviewDir = Join-Path $RepoRoot 'artifacts\review'
$TemplateXls = Join-Path $ReviewDir '20260601_レビュー記録票_基本設計書_画面設計_メインメニュー.xls'
if(-not (Test-Path $TemplateXls)){ Write-Error "Template not found: $TemplateXls"; exit 1 }
$Backup = $TemplateXls + '.bak.' + (Get-Date -Format yyyyMMddHHmmss)
Copy-Item -LiteralPath $TemplateXls -Destination $Backup -Force
$outXlsx = Join-Path $ReviewDir '20260601_レビュー完了版.xlsx'

$CsvReview = Join-Path $ReviewDir '20260601_review.csv'
$CsvCheck = Join-Path $ReviewDir '20260601_reviewcheck.csv'
$CsvFinal = Join-Path $ReviewDir '20260601_finalcheck.csv'
foreach($f in @($CsvReview,$CsvCheck,$CsvFinal)){
	if(-not (Test-Path $f)){ Write-Error "Missing CSV: $f"; exit 1 }
}

$excel = New-Object -ComObject Excel.Application
$excel.Visible = $false
$excel.DisplayAlerts = $false
$wb = $excel.Workbooks.Open((Resolve-Path $TemplateXls).Path)

function TrimQuotes([string]$s){ if($null -eq $s){ return '' }; return $s.Trim().Trim('"') }
function GetCsvHeaderTokens([string]$path){ $lines = Get-Content -Path $path -Encoding UTF8; foreach($l in $lines){ $t = $l.Trim(); if($t.Length -gt 0){ $parts = $t -split ','; return $parts | ForEach-Object { TrimQuotes $_ } } }; return @() }
function SampleSheetText($sheet, $rowsToSample, $colsToSample){ $txt = @(); $used = $sheet.UsedRange; if($used -eq $null){ return $txt }; $maxRow = [int]$used.Rows.Count; $maxCol = [int]$used.Columns.Count; $rEnd = [math]::Min($rowsToSample,$maxRow); $cEnd = [math]::Min($colsToSample,$maxCol); for($r=1;$r -le $rEnd;$r++){ for($c=1;$c -le $cEnd;$c++){ $v = $sheet.Cells.Item($r,$c).Value2; if($null -ne $v){ $txt += $v.ToString().Trim() } } }; return $txt }
function FindSheetByHeaderTokens($wb, $tokens){ $best = $null; $bestScore = 0; foreach($sh in $wb.Worksheets){ $sample = SampleSheetText $sh 6 10; $score = 0; foreach($t in $tokens){ if($t -eq ''){ continue }; foreach($s in $sample){ if($s -eq $t){ $score++ ; break } } }; if($score -gt $bestScore){ $bestScore = $score; $best = $sh } }; if($bestScore -gt 0){ return $best } else { return $null } }

try{
	# Identify sheets by matching headers from CSVs
	$revTokens = GetCsvHeaderTokens $CsvReview
	$chkTokens = GetCsvHeaderTokens $CsvCheck
	$finTokens = GetCsvHeaderTokens $CsvFinal

	$wsReview = FindSheetByHeaderTokens $wb $revTokens
	if($null -eq $wsReview){ $wsReview = $wb.Worksheets.Item(1) }

	# Append review rows starting with '2.'
	$used = $wsReview.UsedRange
	$lastRow = 0
	if($used -ne $null -and $used.Rows.Count -gt 0){ $lastRow = [int]$used.Rows.Count }
	$r = $lastRow + 1
	$lines = Get-Content -Path $CsvReview -Encoding UTF8
	foreach($line in $lines){ $t = $line.Trim(); if($t.Length -eq 0){ continue }; $t2 = $t.TrimStart('"').Trim(); if($t2.StartsWith('2.')){ $cells = $line -split ','; for($c=0;$c -lt $cells.Length;$c++){ $val = TrimQuotes $cells[$c]; $wsReview.Cells.Item($r,$c+1).Value2 = $val }; $r++ } }

	# Review check sheet
	$wsCheck = FindSheetByHeaderTokens $wb $chkTokens
	if($null -eq $wsCheck){ # fallback: choose sheet where column1 contains numeric entries
		foreach($sh in $wb.Worksheets){ $sample = SampleSheetText $sh 10 1; foreach($s in $sample){ if($s -match '^\d+' ){ $wsCheck = $sh; break } }; if($null -ne $wsCheck){ break } }
	}
	if($null -ne $wsCheck){
		$lines = Get-Content -Path $CsvCheck -Encoding UTF8
		$used = $wsCheck.UsedRange
		$rowsCount = 0
		if($used -ne $null -and $used.Rows.Count -gt 0){ $rowsCount = [int]$used.Rows.Count }
		foreach($ln in $lines){ $t = $ln.Trim(); if($t.Length -eq 0){ continue }; if($t -notmatch '^\s*\d+') { continue }; $cols = $ln -split ','; $no = TrimQuotes $cols[0]; if($no -eq ''){ continue }; for($rr=1;$rr -le $rowsCount;$rr++){ $v = $wsCheck.Cells.Item($rr,1).Value2; if($null -ne $v -and $v.ToString().Trim() -eq $no){ $wsCheck.Cells.Item($rr,6).Value2 = (if($cols.Length -gt 5){ TrimQuotes $cols[5] } else {''}); $wsCheck.Cells.Item($rr,7).Value2 = (if($cols.Length -gt 6){ TrimQuotes $cols[6] } else {''}); $wsCheck.Cells.Item($rr,8).Value2 = (if($cols.Length -gt 7){ TrimQuotes $cols[7] } else {''}); break } }
		}
	}

	# Final check sheet
	$wsFinal = FindSheetByHeaderTokens $wb $finTokens
	if($null -eq $wsFinal){ foreach($sh in $wb.Worksheets){ $sample = SampleSheetText $sh 10 1; foreach($s in $sample){ if($s -match '^\d+' ){ $wsFinal = $sh; break } }; if($null -ne $wsFinal){ break } } }
	if($null -ne $wsFinal){
		$lines = Get-Content -Path $CsvFinal -Encoding UTF8
		$used = $wsFinal.UsedRange
		$rowsCount = 0
		if($used -ne $null -and $used.Rows.Count -gt 0){ $rowsCount = [int]$used.Rows.Count }
		foreach($ln in $lines){ $t = $ln.Trim(); if($t.Length -eq 0){ continue }; if($t -notmatch '^\s*\d+') { continue }; $cols = $ln -split ','; $no = TrimQuotes $cols[0]; if($no -eq ''){ continue }; for($rr=1;$rr -le $rowsCount;$rr++){ $v = $wsFinal.Cells.Item($rr,1).Value2; if($null -ne $v -and $v.ToString().Trim() -eq $no){ $wsFinal.Cells.Item($rr,6).Value2 = (if($cols.Length -gt 5){ TrimQuotes $cols[5] } else {''}); $wsFinal.Cells.Item($rr,7).Value2 = (if($cols.Length -gt 6){ TrimQuotes $cols[6] } else {''}); $wsFinal.Cells.Item($rr,8).Value2 = (if($cols.Length -gt 7){ TrimQuotes $cols[7] } else {''}); break } }
		}
	}

	# Save
	$wb.SaveAs((Resolve-Path $outXlsx).Path,51)
	Write-Output "Saved merged file: $outXlsx (backup: $Backup)"
}
finally{
	$wb.Close($false)
	$excel.Quit()
	[System.Runtime.Interopservices.Marshal]::ReleaseComObject($wb) | Out-Null
	[System.Runtime.Interopservices.Marshal]::ReleaseComObject($excel) | Out-Null
	[GC]::Collect(); [GC]::WaitForPendingFinalizers();
}
