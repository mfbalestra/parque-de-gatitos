param(
    [Parameter(Mandatory=$true)][string]$OutputDirectory,
    [int]$Repetitions = 2,
    [ValidateSet(1,4,8)][int[]]$Counts = @(1,4,8)
)
$ErrorActionPreference = 'Stop'
if ($Repetitions -lt 1 -or $Repetitions -gt 5) { throw 'Repetitions debe estar entre 1 y 5.' }
$nativeExe = Join-Path $PSScriptRoot 'bin\Release\net10.0-windows\Gatitos.Windows.exe'
if (-not (Test-Path -LiteralPath $nativeExe)) { throw 'Compilá primero con dotnet build -c Release.' }
$resultDirectory = [IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Path $resultDirectory -Force | Out-Null
for ($run = 1; $run -le $Repetitions; $run++) {
    foreach ($count in $Counts) {
        $resultPath = Join-Path $resultDirectory "cats-$count-run-$run.json"
        $gpuPath = Join-Path $resultDirectory "gpu-$count-run-$run.json"
        if ((Test-Path -LiteralPath $resultPath) -or (Test-Path -LiteralPath $gpuPath)) { throw 'Ya hay resultados para esta cantidad y repetición. Usá otra carpeta.' }
        $arguments = @('--cats', [string]$count, '--output', ('"' + $resultPath + '"'))
        $measurementProcess = Start-Process -FilePath $nativeExe -ArgumentList $arguments -PassThru
        $gpuSamples = [Collections.Generic.List[object]]::new()
        $gpuError = $null
        $gpuWarnings = 0
        $elapsed = [Diagnostics.Stopwatch]::StartNew()
        while (-not $measurementProcess.HasExited) {
            if ($elapsed.Elapsed.TotalSeconds -gt 65) { throw "La medición excedió su límite. Revisá la instancia propia PID $($measurementProcess.Id)." }
            if ($gpuError) { $measurementProcess.WaitForExit(1000) | Out-Null; continue }
            try {
                # Other processes can disappear between GPU samples; retain valid samples for this PID.
                $counter = Get-Counter -Counter '\GPU Engine(*)\Utilization Percentage' -SampleInterval 1 -MaxSamples 1 -ErrorAction SilentlyContinue -ErrorVariable counterWarnings
                $gpuWarnings += $counterWarnings.Count
                if (-not $counter) { $gpuError = 'Windows no devolvió muestras de GPU.'; continue }
                $own = @($counter.CounterSamples | Where-Object { $_.InstanceName -like "pid_$($measurementProcess.Id)_*" -and $_.Status -eq 0 })
                $gpuSamples.Add([pscustomobject]@{ timestampUtc = $counter.Timestamp.ToUniversalTime().ToString('o'); engines = @($own | Select-Object InstanceName,CookedValue) })
            } catch { $gpuError = $_.Exception.Message }
        }
        [pscustomobject]@{ processId=$measurementProcess.Id; error=$gpuError; warnings=$gpuWarnings; samples=$gpuSamples } | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $gpuPath -Encoding utf8
        if ($measurementProcess.ExitCode -ne 0 -or -not (Test-Path -LiteralPath $resultPath)) { throw "Falló la medición de $count gatos." }
        Write-Output "Completado: $count gatos, repetición $run."
    }
}
