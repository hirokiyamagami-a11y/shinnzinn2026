#!/usr/bin/env pwsh
# Run tests and export a short summary to CSV and XLSX in artifacts/review
$repoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $repoRoot

$outDir = Join-Path $repoRoot 'artifacts\review'
if (-not (Test-Path $outDir)) { New-Item -ItemType Directory -Path $outDir | Out-Null }

# Run dotnet test and capture output
Write-Output "Running dotnet test and generating TRX..."
$testResultsDir = Join-Path $repoRoot 'TestResults'
if (Test-Path $testResultsDir) { Remove-Item -Recurse -Force $testResultsDir }
New-Item -ItemType Directory -Path $testResultsDir | Out-Null

$dotnetOutput = dotnet test --no-build --logger "trx;LogFileName=tests.trx" --results-directory "$testResultsDir" 2>&1 | Out-String
Write-Output $dotnetOutput

$trxPath = Join-Path $testResultsDir 'tests.trx'
if (-not (Test-Path $trxPath)) {
	Write-Warning "TRX results not found, falling back to simple summary"
	# fallback: write minimal CSV
	$now = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
	$csvPath = Join-Path $outDir ('20260601_test_results.csv')
	$csvLine = "Date,Note`n$now,TRX not generated"
	[System.IO.File]::WriteAllText($csvPath, $csvLine, [System.Text.Encoding]::UTF8)
	Write-Output "Wrote CSV summary: $csvPath"
	Write-Output "Done."
	exit 0
}

# Parse TRX XML for detailed test results
[xml]$trx = Get-Content -Path $trxPath -Raw
$results = @()
foreach ($r in $trx.TestRun.Results.UnitTestResult) {
	$testName = $r.testName
	$outcome = $r.outcome
	$duration = $r.duration
	$errorMessage = ''
	$stackTrace = ''
	if ($r.Output -ne $null -and $r.Output.ErrorInfo -ne $null) {
		$errorMessage = ($r.Output.ErrorInfo.Message -as [string]) -replace '\r',''
		$stackTrace = ($r.Output.ErrorInfo.StackTrace -as [string]) -replace '\r',''
	}
	$results += [PSCustomObject]@{
		TestName = $testName
		Outcome = $outcome
		Duration = $duration
		ErrorMessage = $errorMessage
		StackTrace = $stackTrace
	}
}

# Write detailed CSV
$now = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
$csvPath = Join-Path $outDir '20260601_test_results_detailed.csv'
$results | Export-Csv -Path $csvPath -NoTypeInformation -Encoding UTF8
Write-Output "Wrote detailed CSV: $csvPath"

# Write detailed XLSX via Excel COM
try {
	$excel = New-Object -ComObject Excel.Application
	$excel.Visible = $false
	$excel.DisplayAlerts = $false
	$wb = $excel.Workbooks.Add()
	$ws = $wb.Worksheets.Item(1)
	# headers
	$headers = @('TestName','Outcome','Duration','ErrorMessage','StackTrace')
	for ($i=0; $i -lt $headers.Count; $i++) { $ws.Cells.Item(1, $i+1).Value2 = $headers[$i] }
	# rows
	$row = 2
	foreach ($it in $results) {
		$ws.Cells.Item($row,1).Value2 = $it.TestName
		$ws.Cells.Item($row,2).Value2 = $it.Outcome
		$ws.Cells.Item($row,3).Value2 = $it.Duration
		$ws.Cells.Item($row,4).Value2 = $it.ErrorMessage
		$ws.Cells.Item($row,5).Value2 = $it.StackTrace
		$row++
	}

	$xlsxPath = Join-Path $outDir '20260601_test_results_detailed.xlsx'
	$wb.SaveAs($xlsxPath, 51)
	Write-Output "Saved detailed XLSX: $xlsxPath"
} catch {
	Write-Error "Failed to write detailed XLSX: $_"
} finally {
	if ($null -ne $wb) { $wb.Close($false) }
	if ($null -ne $excel) { $excel.Quit(); [System.Runtime.Interopservices.Marshal]::ReleaseComObject($excel) | Out-Null }
	[GC]::Collect(); [GC]::WaitForPendingFinalizers();
}

Write-Output "Done."
