# ApplyCsvsToExistingXls.ps1
# Reads three CSVs and writes values into existing formatted XLS template, then saves as new XLSX
$repo = Split-Path -Parent $PSScriptRoot
$reviewDir = Join-Path $repo 'artifacts\review'
$xlsPath = Join-Path $reviewDir '20260601_繝ｬ繝薙Η繝ｼ險倬鹸逾ｨ_蝓ｺ譛ｬ險ｭ險域嶌_逕ｻ髱｢險ｭ險・繝｡繧､繝ｳ繝｡繝九Η繝ｼ.xls'
if(-not (Test-Path $xlsPath)){ Write-Error "Template not found: $xlsPath"; exit 1 }
$bak = $xlsPath + '.bak.' + (Get-Date -Format yyyyMMddHHmmss)
Copy-Item -LiteralPath $xlsPath -Destination $bak -Force

$csvReview = Join-Path $reviewDir '20260601_review.csv'
$csvCheck = Join-Path $reviewDir '20260601_reviewcheck.csv'
$csvFinal = Join-Path $reviewDir '20260601_finalcheck.csv'
foreach($f in @($csvReview,$csvCheck,$csvFinal)){ if(-not (Test-Path $f)){ Write-Error "Missing CSV: $f"; exit 1 } }

$outXlsx = Join-Path $reviewDir '20260601_繝ｬ繝薙Η繝ｼ螳御ｺ・沿.xlsx'

$excel = New-Object -ComObject Excel.Application
$excel.Visible = $false
$excel.DisplayAlerts = $false
$wb = $excel.Workbooks.Open($xlsPath)

try{
	# 1) Append review rows (NO.2.*) to sheet '繝ｬ繝薙Η繝ｼ險倬鹸逾ｨ'
	$wsReview = $null
	foreach($s in $wb.Worksheets){ if($s.Name -eq '繝ｬ繝薙Η繝ｼ險倬鹸逾ｨ'){ $wsReview = $s; break } }
	if($null -eq $wsReview){ $wsReview = $wb.Worksheets.Item(1) }
	$used = $wsReview.UsedRange
	$lastRow = 0
	if($used -ne $null -and $used.Rows.Count -gt 0){ $lastRow = [int]$used.Rows.Count }
	$rIndex = $lastRow + 1
	$revLines = Get-Content -Path $csvReview -Encoding UTF8 | Where-Object { $_ -match '^\s*2\.' }
	foreach($line in $revLines){
		# simple split by comma; assumes CSV does not contain embedded commas in fields
		$fields = $line -split ','
		for($c=0;$c -lt $fields.Count;$c++){
			$val = $fields[$c] -replace '(^"|"$)', ''
			$val = $val.Trim()
			$wsReview.Cells.Item($rIndex,$c+1).Value2 = $val
		}
		$rIndex++
	}

	# 2) Update reviewcheck sheet: write 諡・ｽ・譌･莉・邨先棡) into columns where header places them (assume columns 6,7,8)
	$wsCheck = $null
	foreach($s in $wb.Worksheets){ if($s.Name -eq '繝ｬ繝薙Η繝ｼ繝√ぉ繝・け繝ｪ繧ｹ繝・){ $wsCheck = $s; break } }
	if($null -ne $wsCheck){
		$lines = Get-Content -Path $csvCheck -Encoding UTF8
		# find data lines starting with number
		$data = $lines | Where-Object { $_ -match '^\s*\d+' }
		foreach($ln in $data){
			$f = $ln -split ','
			$no = $f[0].Trim('"')
			if([string]::IsNullOrWhiteSpace($no)){ continue }
			# find row in sheet where column1 equals $no
			$found = $null
			$used = $wsCheck.UsedRange
			$rowCount = [int]$used.Rows.Count
			for($rr=1;$rr -le $rowCount;$rr++){
				$v = $wsCheck.Cells.Item($rr,1).Value2
				if($null -ne $v -and ($v.ToString().Trim() -eq $no)) { $found = $rr; break }
			}
			if($null -ne $found){
				# write 諡・ｽ・at col6, 譌･莉・col7, 邨先棡 col8 (0-based f indices: 5,6,7)
				$wsCheck.Cells.Item($found,6).Value2 = ($f.Count -gt 5) ? $f[5].Trim('"') : ''
				$wsCheck.Cells.Item($found,7).Value2 = ($f.Count -gt 6) ? $f[6].Trim('"') : ''
				$wsCheck.Cells.Item($found,8).Value2 = ($f.Count -gt 7) ? $f[7].Trim('"') : ''
			}
		}
	}

	# 3) Update finalcheck sheet similarly
	$wsFinal = $null
	foreach($s in $wb.Worksheets){ if($s.Name -eq '繝ｪ繝ｪ繝ｼ繧ｹ蜑肴怙邨ゅメ繧ｧ繝・け'){ $wsFinal = $s; break } }
	if($null -ne $wsFinal){
		$lines = Get-Content -Path $csvFinal -Encoding UTF8
		$data = $lines | Where-Object { $_ -match '^\s*\d+' }
		foreach($ln in $data){
			$f = $ln -split ','
			$no = $f[0].Trim('"')
			if([string]::IsNullOrWhiteSpace($no)){ continue }
			$found = $null
			$used = $wsFinal.UsedRange
			$rowCount = [int]$used.Rows.Count
			for($rr=1;$rr -le $rowCount;$rr++){
				$v = $wsFinal.Cells.Item($rr,1).Value2
				if($null -ne $v -and ($v.ToString().Trim() -eq $no)) { $found = $rr; break }
			}
			if($null -ne $found){
				$wsFinal.Cells.Item($found,6).Value2 = ($f.Count -gt 5) ? $f[5].Trim('"') : ''
				$wsFinal.Cells.Item($found,7).Value2 = ($f.Count -gt 6) ? $f[6].Trim('"') : ''
				$wsFinal.Cells.Item($found,8).Value2 = ($f.Count -gt 7) ? $f[7].Trim('"') : ''
			}
		}
	}

	# Save as new xlsx
	$wb.SaveAs((Resolve-Path $outXlsx).Path,51)
	Write-Output "Merged and saved to: $outXlsx (backup of original: $bak)"
} finally {
	$wb.Close($false)
	$excel.Quit()
	[System.Runtime.Interopservices.Marshal]::ReleaseComObject($wb) | Out-Null
	[System.Runtime.Interopservices.Marshal]::ReleaseComObject($excel) | Out-Null
	[GC]::Collect(); [GC]::WaitForPendingFinalizers();
}

