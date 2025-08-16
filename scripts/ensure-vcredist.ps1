param(
    [ValidateSet('x64','x86','both')]
    [string]$Architecture = 'x64',
    [switch]$ContinueOnError
)

Write-Host "=== Ensure VC++ Redistributable (2015-2022) ==="

function Test-VCRedistInstalled {
    param(
        [Parameter(Mandatory=$true)]
        [ValidateSet('x64','x86')]
        [string]$Arch
    )

    $keyMap = @{ 
        'x64' = 'HKLM:\SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64'
        'x86' = 'HKLM:\SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x86'
    }

    try {
        $props = Get-ItemProperty -Path $keyMap[$Arch] -ErrorAction SilentlyContinue
        if ($null -ne $props -and $props.Installed -eq 1) {
            return @{ Installed = $true; Version = $props.Version }
        }
    } catch { }

    return @{ Installed = $false; Version = $null }
}

function Install-VCRedist {
    param(
        [Parameter(Mandatory=$true)]
        [ValidateSet('x64','x86')]
        [string]$Arch
    )

    $installerUrl = if ($Arch -eq 'x64') { 'https://aka.ms/vs/17/release/vc_redist.x64.exe' } else { 'https://aka.ms/vs/17/release/vc_redist.x86.exe' }

    try {
        if (Get-Command choco -ErrorAction SilentlyContinue) {
            Write-Host "Attempting install via Chocolatey: vcredist140 ($Arch)"
            choco install vcredist140 -y --no-progress | Out-Host
        } else {
            Write-Host "Chocolatey not available; skipping choco install"
        }
    } catch {
        Write-Host "Chocolatey install attempt failed: $($_.Exception.Message)"
    }

    $status = Test-VCRedistInstalled -Arch $Arch
    if ($status.Installed) { return $true }

    try {
        $tempPath = Join-Path $env:RUNNER_TEMP "vc_redist.$Arch.exe"
        Write-Host "Downloading VC++ Redistributable from $installerUrl"
        Invoke-WebRequest -Uri $installerUrl -OutFile $tempPath -UseBasicParsing
        Write-Host "Running installer for $Arch..."
        $args = "/install /quiet /norestart"
        $process = Start-Process -FilePath $tempPath -ArgumentList $args -Wait -PassThru -NoNewWindow
        Write-Host "Installer exit code ($Arch): $($process.ExitCode)"
    } catch {
        Write-Host "Failed to run VC++ installer for $Arch: $($_.Exception.Message)"
    }

    $status2 = Test-VCRedistInstalled -Arch $Arch
    return $status2.Installed
}

$archesToEnsure = switch ($Architecture) {
    'both' { @('x64','x86') }
    'x86'  { @('x86') }
    default { @('x64') }
}

$allOk = $true
foreach ($arch in $archesToEnsure) {
    $status = Test-VCRedistInstalled -Arch $arch
    if ($status.Installed) {
        Write-Host "Detected VC++ $arch installed (Version: $($status.Version))"
        continue
    }

    Write-Host "VC++ $arch not detected. Attempting installation..."
    $ok = Install-VCRedist -Arch $arch
    if ($ok) {
        $statusPost = Test-VCRedistInstalled -Arch $arch
        Write-Host "✅ Ensured VC++ $arch (Version: $($statusPost.Version))"
    } else {
        Write-Host "⚠️ Unable to verify VC++ $arch installation after attempts"
        $allOk = $false
    }
}

if ($allOk) {
    Write-Host "✅ VC++ Redistributable ensured for: $($archesToEnsure -join ', ')"
    exit 0
} else {
    if ($ContinueOnError) {
        Write-Host "⚠️ Continuing despite failure to verify VC++ installation"
        exit 0
    } else {
        Write-Host "❌ Failed to ensure VC++ Redistributable"
        exit 1
    }
}