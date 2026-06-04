# AppendCsvRowsToSpecificXls.ps1
# Reads scripts/review_rows_20260602.csv and appends its rows to artifacts/review/20260602_レビュー記録票.xls
$repoRoot = Resolve-Path "$PSScriptRoot\.." | Select-Object -ExpandProperty Path
$xlsPath = Join-Path $repoRoot 'artifacts\review\20260602_レビュー記録票.xls'
$csvPath = Join-Path $repoRoot 'scripts\review_rows_20260602.csv'
if(-not (Test-Path $xlsPath)){ Write-Error "XLS not found: $xlsPath"; exit 1 }
if(-not (Test-Path $csvPath)){ Write-Error "CSV not found: $csvPath"; exit 1 }
$bak = $xlsPath + '.bak.' + (Get-Date -Format yyyyMMddHHmmss)
Copy-Item -LiteralPath $xlsPath -Destination $bak -Force
$rows = Import-Csv -Path $csvPath -Encoding UTF8
if(-not $rows -or $rows.Count -eq 0){ Write-Error 'CSV has no rows'; exit 1 }
$excel = New-Object -ComObject Excel.Application
$excel.Visible = $false
$excel.DisplayAlerts = $false
$wb = $excel.Workbooks.Open($xlsPath)
$ws = $null
foreach($s in $wb.Worksheets){ if($s.Name -eq 'レビュー記録表'){ $ws = $s; break } }
if(-not $ws){ $ws = $wb.Worksheets.Item(1) }
$used = $ws.UsedRange
$lastRow = 0
if($used -ne $null -and $used.Rows.Count -gt 0){ $lastRow = [int]$used.Rows.Count } else { $lastRow = 0 }
$startRow = $lastRow + 1
# Determine column order from CSV header
$cols = $rows[0].PSObject.Properties | ForEach-Object { $_.Name }
foreach($r in $rows){ for($i=0;$i -lt $cols.Count;$i++){ $val = $r.$($cols[$i]); $ws.Cells.Item($startRow, $i+1).Value2 = $val } $startRow++ }
$wb.Save()
$wb.Close($true)
$excel.Quit()
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($ws) | Out-Null
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($wb) | Out-Null
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($excel) | Out-Null
[GC]::Collect(); [GC]::WaitForPendingFinalizers(); Write-Output ("Append complete to $xlsPath (backup: $bak)")