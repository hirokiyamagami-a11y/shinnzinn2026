"# MergeCsvsToXlsx.ps1 - stable implementation (writes three CSVs into one XLSX, each as a sheet)"
$repo = Split-Path -Parent $PSScriptRoot
$reviewDir = Join-Path $repo 'artifacts\review'
$files = @(
	@{ Path = Join-Path $reviewDir '繝ｬ繝薙Η繝ｼ險倬鹸逾ｨ.csv'; Sheet = '繝ｬ繝薙Η繝ｼ險倬鹸逾ｨ' },
	@{ Path = Join-Path $reviewDir '繝ｬ繝薙Η繝ｼ繝√ぉ繝・け繝ｪ繧ｹ繝・csv'; Sheet = '繝ｬ繝薙Η繝ｼ繝√ぉ繝・け繝ｪ繧ｹ繝・ },
	@{ Path = Join-Path $reviewDir '繝ｪ繝ｪ繝ｼ繧ｹ蜑肴怙邨ゅメ繧ｧ繝・け.csv'; Sheet = '繝ｪ繝ｪ繝ｼ繧ｹ蜑肴怙邨ゅメ繧ｧ繝・け' }
)
$missing = $files | Where-Object { -not (Test-Path $_.Path) }
if($missing.Count -gt 0){ Write-Error ('Missing files:' + ($missing | ForEach-Object { " `n  - " + $_.Path } ) ) ; exit 1 }
$target = Join-Path $reviewDir 'merged_reviews.xlsx'
if(Test-Path $target){ $bak = $target + '.bak.' + (Get-Date -Format yyyyMMddHHmmss); Copy-Item -LiteralPath $target -Destination $bak -Force }
$excel = New-Object -ComObject Excel.Application
$excel.Visible = $false
$excel.DisplayAlerts = $false
$wb = $excel.Workbooks.Add()
$useFirst = $true
foreach($item in $files){
	$csv = Import-Csv -Path $item.Path -Encoding UTF8 -ErrorAction SilentlyContinue
	if($useFirst){
		$ws = $wb.Worksheets.Item(1)
		try{ $ws.Name = $item.Sheet } catch { }
		$useFirst = $false
	} else {
		$ws = $wb.Worksheets.Add()
		try{ $ws.Name = $item.Sheet } catch { }
	}
	if($null -ne $csv -and $csv.Count -gt 0){
		$cols = $csv[0].PSObject.Properties | ForEach-Object { $_.Name }
		for($c=0;$c -lt $cols.Count;$c++){ $ws.Cells.Item(1,$c+1).Value2 = $cols[$c] }
		$r = 2
		foreach($rowData in $csv){
			for($c=0;$c -lt $cols.Count;$c++){
				$val = $rowData.$($cols[$c])
				$ws.Cells.Item($r,$c+1).Value2 = $val
			}
			$r++
		}
	}
}
$wb.SaveAs((Resolve-Path $target).Path,51)
$wb.Close($true)
$excel.Quit()
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($wb) | Out-Null
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($excel) | Out-Null
[GC]::Collect(); [GC]::WaitForPendingFinalizers(); Write-Output ('Merged to ' + $target)

