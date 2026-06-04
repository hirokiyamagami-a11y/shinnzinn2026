# ApplyCsvToXls2.ps1
# Find the first '*-filled.csv' and the first '*.xls' in artifacts/review and append rows with NO. starting with 2.
$repoRoot = Resolve-Path "$PSScriptRoot\.." | Select-Object -ExpandProperty Path
$reviewDir = Join-Path $repoRoot 'artifacts\review'
$csvFile = Get-ChildItem -Path $reviewDir -Filter '*-filled.csv' | Select-Object -First 1
$xlsFile = Get-ChildItem -Path $reviewDir -Filter '*.xls' | Select-Object -First 1
if(-not $csvFile){ Write-Error 'No filled CSV found in review folder'; Get-ChildItem -Path $reviewDir | Select-Object Name; exit 1 }
if(-not $xlsFile){ Write-Error 'No xls template found in review folder'; Get-ChildItem -Path $reviewDir | Select-Object Name; exit 1 }
$csvPath = $csvFile.FullName
$xlsPath = $xlsFile.FullName
# backup
$bak = $xlsPath + '.bak.' + (Get-Date -Format yyyyMMddHHmmss)
Copy-Item -LiteralPath $xlsPath -Destination $bak -Force
# read CSV
$rows = Import-Csv -Path $csvPath -Encoding UTF8
if(-not $rows -or $rows.Count -eq 0){ Write-Error 'CSV is empty'; exit 1 }
# open excel
$excel = New-Object -ComObject Excel.Application
$excel.Visible = $false
$excel.DisplayAlerts = $false
$wb = $excel.Workbooks.Open($xlsPath)
# find sheet named 'レビュー記録表' else first
$ws = $null
foreach($s in $wb.Worksheets){ if($s.Name -eq 'レビュー記録表'){ $ws = $s; break } }
if(-not $ws){ $ws = $wb.Worksheets.Item(1) }
$used = $ws.UsedRange
$lastRow = 0
if($used -ne $null -and $used.Rows.Count -gt 0){ $lastRow = [int]$used.Rows.Count } else { $lastRow = 0 }
$rowIndex = $lastRow + 1
# determine property order from first row
$propNames = $rows[0].PSObject.Properties | ForEach-Object { $_.Name }
# append rows where NO. starts with '2.'
foreach($r in $rows){ if($r.'NO.' -match '^2\.'){
	for($i=0; $i -lt $propNames.Count; $i++){
		$prop = $propNames[$i]
		$val = $r.$prop
		$ws.Cells.Item($rowIndex, $i+1).Value2 = $val
	}
	$rowIndex++
} }
$wb.Save()
$wb.Close($true)
$excel.Quit()
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($ws) | Out-Null
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($wb) | Out-Null
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($excel) | Out-Null
[GC]::Collect(); [GC]::WaitForPendingFinalizers(); Write-Output ('Append complete to ' + $xlsPath + ' (backup: ' + $bak + ')')