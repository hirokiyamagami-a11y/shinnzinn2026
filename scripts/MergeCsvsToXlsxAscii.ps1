# MergeCsvsToXlsxAscii.ps1
# ASCII-only script: merge three CSVs into existing .xls template and save as merged xlsx (ASCII name)
$RepoRoot = Split-Path -Parent $PSScriptRoot
$ReviewDir = Join-Path $RepoRoot 'artifacts\review'
$tpl = Get-ChildItem -Path $ReviewDir -Filter '*.xls' -File | Select-Object -First 1
if($null -eq $tpl){ Write-Error "Template .xls not found in $ReviewDir"; exit 1 }
$TemplateXls = $tpl.FullName
$Backup = $TemplateXls + '.bak.' + (Get-Date -Format yyyyMMddHHmmss)
Copy-Item -LiteralPath $TemplateXls -Destination $Backup -Force
$outMerged = Join-Path $ReviewDir '20260601_merged.xlsx'

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
function SampleSheetText($sheet, $rowsToSample, $colsToSample){ $txt = @(); $used = $sheet.UsedRange; if($used -eq $null){ return $txt }; $maxRow = [int]$used.Rows.Count; $maxCol = [int]$used.Columns.Count; $rEnd = [math]::Min($rowsToSample,$maxRow); $cEnd = [math]::Min($colsToSample,$maxCol); for($r=1;$r -le $rEnd;$r++){ for($c=1;$c -le $cEnd;$c++){ $v = $sheet.Cells.Item($r,$c).Value2; if($null -ne $v){ $txt += $v.ToString().Trim() } } }; return $txt }

try{
	# Identify review sheet by matching header tokens from CSV
	$revHeader = (Get-Content -Path $CsvReview -Encoding UTF8 | Where-Object { $_.Trim().Length -gt 0 } | Select-Object -First 1)
	$revTokens = @()
	if($null -ne $revHeader){ $revTokens = $revHeader -split ',' | ForEach-Object { $_.Trim('"').Trim() } }

	$bestReview = $null; $bestScore = 0
	foreach($sh in $wb.Worksheets){ $sample = SampleSheetText $sh 6 10; $score = 0; foreach($t in $revTokens){ if($t -eq ''){ continue }; foreach($s in $sample){ if($s -eq $t){ $score++; break } } }; if($score -gt $bestScore){ $bestScore = $score; $bestReview = $sh } }
	if($null -eq $bestReview){ $bestReview = $wb.Worksheets.Item(1) }

	# Append review rows starting with '2.'
	$used = $bestReview.UsedRange
	$lastRow = 0
	if($used -ne $null -and $used.Rows.Count -gt 0){ $lastRow = [int]$used.Rows.Count }
	$r = $lastRow + 1
	$lines = Get-Content -Path $CsvReview -Encoding UTF8
	foreach($line in $lines){ $t = $line.Trim(); if($t.Length -eq 0){ continue }; $t2 = $t.TrimStart('"').Trim(); if($t2.StartsWith('2.')){ $cells = $line -split ','; for($c=0;$c -lt $cells.Length;$c++){ $val = TrimQuotes $cells[$c]; $bestReview.Cells.Item($r,$c+1).Value2 = $val }; $r++ } }

	# Find check and final sheets by scanning for numeric values in col1
	$wsCheck = $null; $wsFinal = $null
	foreach($sh in $wb.Worksheets){ $sample = SampleSheetText $sh 20 1; foreach($s in $sample){ if($s -match '^\d+' ){ if($null -eq $wsCheck){ $wsCheck = $sh } elseif($null -eq $wsFinal -and $sh -ne $wsCheck){ $wsFinal = $sh } ; break } } }

	if($null -ne $wsCheck){
		$lines = Get-Content -Path $CsvCheck -Encoding UTF8
		$used = $wsCheck.UsedRange
		$rowsCount = 0
		if($used -ne $null -and $used.Rows.Count -gt 0){ $rowsCount = [int]$used.Rows.Count }
		foreach($ln in $lines){ $t = $ln.Trim(); if($t.Length -eq 0){ continue }; if($t -notmatch '^\s*\d+') { continue }; $cols = $ln -split ','; $no = TrimQuotes $cols[0]; if($no -eq ''){ continue }; for($rr=1;$rr -le $rowsCount;$rr++){ $v = $wsCheck.Cells.Item($rr,1).Value2; if($null -ne $v -and $v.ToString().Trim() -eq $no){ $wsCheck.Cells.Item($rr,6).Value2 = (if($cols.Length -gt 5){ TrimQuotes $cols[5] } else {''}); $wsCheck.Cells.Item($rr,7).Value2 = (if($cols.Length -gt 6){ TrimQuotes $cols[6] } else {''}); $wsCheck.Cells.Item($rr,8).Value2 = (if($cols.Length -gt 7){ TrimQuotes $cols[7] } else {''}); break } }
		}
	}

	if($null -ne $wsFinal){
		$lines = Get-Content -Path $CsvFinal -Encoding UTF8
		$used = $wsFinal.UsedRange
		$rowsCount = 0
		if($used -ne $null -and $used.Rows.Count -gt 0){ $rowsCount = [int]$used.Rows.Count }
		foreach($ln in $lines){ $t = $ln.Trim(); if($t.Length -eq 0){ continue }; if($t -notmatch '^\s*\d+') { continue }; $cols = $ln -split ','; $no = TrimQuotes $cols[0]; if($no -eq ''){ continue }; for($rr=1;$rr -le $rowsCount;$rr++){ $v = $wsFinal.Cells.Item($rr,1).Value2; if($null -ne $v -and $v.ToString().Trim() -eq $no){ $wsFinal.Cells.Item($rr,6).Value2 = (if($cols.Length -gt 5){ TrimQuotes $cols[5] } else {''}); $wsFinal.Cells.Item($rr,7).Value2 = (if($cols.Length -gt 6){ TrimQuotes $cols[6] } else {''}); $wsFinal.Cells.Item($rr,8).Value2 = (if($cols.Length -gt 7){ TrimQuotes $cols[7] } else {''}); break } }
		}
	}

	# Save merged file (ASCII name)
	$wb.SaveAs((Resolve-Path $outMerged).Path,51)
	Write-Output "Saved merged file: $outMerged (backup: $Backup)"
}
finally{
	$wb.Close($false)
	$excel.Quit()
	[System.Runtime.Interopservices.Marshal]::ReleaseComObject($wb) | Out-Null
	[System.Runtime.Interopservices.Marshal]::ReleaseComObject($excel) | Out-Null
	[GC]::Collect(); [GC]::WaitForPendingFinalizers();
}
