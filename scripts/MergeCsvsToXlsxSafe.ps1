# MergeCsvsToXlsxSafe.ps1
# Safely merge three CSVs into existing formatted .xls and save as new .xlsx
$RepoRoot = Split-Path -Parent $PSScriptRoot
$ReviewDir = Join-Path $RepoRoot 'artifacts\review'
$TemplateXls = Join-Path $ReviewDir '20260601_繝ｬ繝薙Η繝ｼ險倬鹸逾ｨ_蝓ｺ譛ｬ險ｭ險域嶌_逕ｻ髱｢險ｭ險・繝｡繧､繝ｳ繝｡繝九Η繝ｼ.xls'
if(-not (Test-Path $TemplateXls)){ Write-Error "Template not found: $TemplateXls"; exit 1 }
$Backup = $TemplateXls + '.bak.' + (Get-Date -Format yyyyMMddHHmmss)
Copy-Item -LiteralPath $TemplateXls -Destination $Backup -Force
$outXlsx = Join-Path $ReviewDir '20260601_繝ｬ繝薙Η繝ｼ螳御ｺ・沿.xlsx'

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

try{
	# 1) Append review rows starting with '2.' into sheet '繝ｬ繝薙Η繝ｼ險倬鹸逾ｨ'
	$wsReview = $null
	foreach($sh in $wb.Worksheets){ if($sh.Name -eq '繝ｬ繝薙Η繝ｼ險倬鹸逾ｨ'){ $wsReview = $sh; break } }
	if($null -eq $wsReview){ $wsReview = $wb.Worksheets.Item(1) }
	$used = $wsReview.UsedRange
	$lastRow = 0
	if($used -ne $null -and $used.Rows.Count -gt 0){ $lastRow = [int]$used.Rows.Count }
	$r = $lastRow + 1
	$lines = Get-Content -Path $CsvReview -Encoding UTF8
	foreach($line in $lines){ $t = $line.Trim(); if($t.Length -eq 0){ continue }; $t2 = $t.TrimStart('"').Trim(); if($t2.StartsWith('2.')){ $cells = $line -split ','; for($c=0;$c -lt $cells.Length;$c++){ $val = TrimQuotes $cells[$c]; $wsReview.Cells.Item($r,$c+1).Value2 = $val }; $r++ } }

	# 2) Update reviewcheck sheet: match NO. in column 1 and write columns 6-8
	$wsCheck = $null
	foreach($sh in $wb.Worksheets){ if($sh.Name -eq '繝ｬ繝薙Η繝ｼ繝√ぉ繝・け繝ｪ繧ｹ繝・){ $wsCheck = $sh; break } }
	if($null -ne $wsCheck){
		$lines = Get-Content -Path $CsvCheck -Encoding UTF8
		$used = $wsCheck.UsedRange
		$rowsCount = 0
		if($used -ne $null -and $used.Rows.Count -gt 0){ $rowsCount = [int]$used.Rows.Count }
		foreach($ln in $lines){ $t = $ln.Trim(); if($t.Length -eq 0){ continue }; $cols = $ln -split ','; $no = TrimQuotes $cols[0]; if($no -eq ''){ continue }; for($rr=1;$rr -le $rowsCount;$rr++){ $v = $wsCheck.Cells.Item($rr,1).Value2; if($null -ne $v -and $v.ToString().Trim() -eq $no){ $wsCheck.Cells.Item($rr,6).Value2 = (if($cols.Length -gt 5){ TrimQuotes $cols[5] } else {''}); $wsCheck.Cells.Item($rr,7).Value2 = (if($cols.Length -gt 6){ TrimQuotes $cols[6] } else {''}); $wsCheck.Cells.Item($rr,8).Value2 = (if($cols.Length -gt 7){ TrimQuotes $cols[7] } else {''}); break } }
		}
	}

	# 3) Update finalcheck sheet similarly
	$wsFinal = $null
	foreach($sh in $wb.Worksheets){ if($sh.Name -eq '繝ｪ繝ｪ繝ｼ繧ｹ蜑肴怙邨ゅメ繧ｧ繝・け'){ $wsFinal = $sh; break } }
	if($null -ne $wsFinal){
		$lines = Get-Content -Path $CsvFinal -Encoding UTF8
		$used = $wsFinal.UsedRange
		$rowsCount = 0
		if($used -ne $null -and $used.Rows.Count -gt 0){ $rowsCount = [int]$used.Rows.Count }
		foreach($ln in $lines){ $t = $ln.Trim(); if($t.Length -eq 0){ continue }; $cols = $ln -split ','; $no = TrimQuotes $cols[0]; if($no -eq ''){ continue }; for($rr=1;$rr -le $rowsCount;$rr++){ $v = $wsFinal.Cells.Item($rr,1).Value2; if($null -ne $v -and $v.ToString().Trim() -eq $no){ $wsFinal.Cells.Item($rr,6).Value2 = (if($cols.Length -gt 5){ TrimQuotes $cols[5] } else {''}); $wsFinal.Cells.Item($rr,7).Value2 = (if($cols.Length -gt 6){ TrimQuotes $cols[6] } else {''}); $wsFinal.Cells.Item($rr,8).Value2 = (if($cols.Length -gt 7){ TrimQuotes $cols[7] } else {''}); break } }
		}
	}

	# Save as new xlsx preserving formats
	$wb.SaveAs((Resolve-Path $outXlsx).Path,51)
	Write-Output "Saved merged file: $outXlsx (original backup: $Backup)"
}
finally{
	$wb.Close($false)
	$excel.Quit()
	[System.Runtime.Interopservices.Marshal]::ReleaseComObject($wb) | Out-Null
	[System.Runtime.Interopservices.Marshal]::ReleaseComObject($excel) | Out-Null
	[GC]::Collect(); [GC]::WaitForPendingFinalizers();
}

