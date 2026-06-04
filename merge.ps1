Stop-Process -Name EXCEL -Force -ErrorAction SilentlyContinue

# ⚠️固定パスを完全廃止！今VS2026が開いている「現在の場所」から本物を自動追跡します
$currentDir = Get-Location
$d = Join-Path $currentDir "artifacts\review"

if ($currentDir -match 'artifacts\\review$') { $d = $currentDir }
if (-not (Test-Path $d)) {
    $d = (Get-ChildItem -Recurse -Directory -Filter "review" | Where-Object { $_.FullName -match 'artifacts' } | Select-Object -First 1).FullName
}

Write-Host "=== [AUTOMATIC PATH DETECTED] ===" -ForegroundColor Cyan
Write-Host "Real working directory: $d"

# ⚠️偽物の「test_results」や「SUCCESS」を絶対に除外して、本物のExcelだけを自動で掴みます
$f = (Get-ChildItem -Path $d -Filter "*.xls*" | Where-Object { $_.Name -notmatch 'test_results' -and $_.Name -notmatch 'SUCCESS' -and $_.Name -notmatch 'OUT' } | Sort-Object Length -Descending | Select-Object -First 1).FullName

if (!$f) {
    Write-Host "CRITICAL: Real Excel file not found in $d" -ForegroundColor Red
    exit 1
}

$e = New-Object -ComObject Excel.Application
$e.Visible = $false
$e.DisplayAlerts = $false
try {
    $wb = $e.Workbooks.Open($f)
    function T($s){ if(!$s){return ''}; return $s.Trim('"').Trim() }

    # 1. review.csv の流し込み
    $c1 = Join-Path $d '20260601_review.csv'
    if(Test-Path $c1){
        $ws = $wb.Worksheets.Item(1)
        foreach($l in (Get-Content $c1 -Encoding Default)){
            if([string]::IsNullOrWhiteSpace($l)){continue}
            $fields = $l -split ','
            $no = T $fields[0]
            if($no -match '^2\.'){
                $r = 14
                while($ws.Cells.Item($r,1).Value2 -ne $null){$r++}
                for($i=0;$i -lt $fields.Length;$i++){ $ws.Cells.Item($r,$i+1).Value2 = T $fields[$i] }
            }
        }
        Write-Host ">> Successfully wrote review.csv to Sheet 1!" -ForegroundColor Green
    }

    # 2 & 3. チェックリストの流し込み
    $u = {
        param($cp,$si)
        if(!(Test-Path $cp)){return}
        $ws = $wb.Worksheets.Item($si)
        foreach($ln in (Get-Content $cp -Encoding Default)){
            if([string]::IsNullOrWhiteSpace($ln)){continue}
            $cols = $ln -split ','
            $id = T $cols[0]
            if($id -match '^\d+'){
                for($row=14;$row -le 300;$row++){
                    $v = $ws.Cells.Item($row,1).Value2
                    if($null -ne $v -and $v.ToString().Trim() -eq $id){
                        foreach($cv in $cols){
                            $tv = T $cv
                            if($tv -eq 'Copilot'){$ws.Cells.Item($row,6).Value2='Copilot'}
                            if($tv -eq 'OK'){$ws.Cells.Item($row,8).Value2='OK'}
                            if($tv -match '\d{4}/\d{2}/\d{2}' -or $tv -match '2026\d{4}'){$ws.Cells.Item($row,7).Value2=$tv}
                        }
                        break
                    }
                }
            }
        }
    }

    & $u (Join-Path $d '20260601_reviewcheck.csv') 2
    & $u (Join-Path $d '20260601_finalcheck.csv') 3

    # 新しいファイルは作らず、本物そのものに直接上書き保存します！
    $wb.Save()
    $wb.Close($false)
    Write-Host "=== [SUCCESS: EXCEL DIRECTLY UPDATED] ===" -ForegroundColor Yellow
    
    # その本物ファイルをパッと自動で開きます
    Start-Process $f
} finally {
    $e.Quit()
    [System.Runtime.InteropServices.Marshal]::ReleaseComObject($e)|Out-Null
    [GC]::Collect()
}