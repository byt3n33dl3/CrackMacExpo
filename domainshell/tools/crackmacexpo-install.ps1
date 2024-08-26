<#
.SYNOPSIS
    DotnetCat installer script for x64 and x86 Windows systems.
.DESCRIPTION
    DotnetCat remote command-shell application installer script
    for x64 and x86 Windows systems.
.LINK
    Application repository: https://github.com/vandavey/DotnetCat
#>
using namespace System.Runtime.InteropServices
using namespace System.Security.Principal

[CmdletBinding()]
param ()

# Write an error message to stderr and exit
function Show-Error {
    $Symbol = "[x]"

    if ($PSVersionTable.PSVersion.Major -ge 7) {
        $Symbol = "`e[91m${Symbol}`e[0m"
    }
    [Console]::Error.WriteLine("${Symbol} ${args}`n")
    exit 1
}

# Write a status message to stdout
function Show-Status {
    $Symbol = "[*]"

    if ($PSVersionTable.PSVersion.Major -ge 7) {
        $Symbol = "`e[96m${Symbol}`e[0m"
    }
    Write-Output "${Symbol} ${args}"
}

# Installer only supports Windows operating systems
if (-not [RuntimeInformation]::IsOSPlatform([OSPlatform]::Windows)) {
    Show-Error "This installer can only be used on Windows operating systems"
}
$User = [WindowsPrincipal]::new([WindowsIdentity]::GetCurrent())

# Installer requires admin privileges
if (-not $User.IsInRole([WindowsBuiltInRole]::Administrator)) {
    Show-Error "The installer must be run as an administrator"
}

$AppDir = $ZipUrl = $null
$RepoRoot = "https://raw.githubusercontent.com/vandavey/DotnetCat/master"

$ArchVarName = "PROCESSOR_ARCHITECTURE"
$EnvVarTarget = [EnvironmentVariableTarget]::Machine
$Architecture = [Environment]::GetEnvironmentVariable($ArchVarName, $EnvVarTarget)

# Validate architecture and assign architecture-specific variables
if ($Architecture -eq "AMD64") {
    $AppDir = "${env:ProgramFiles}\DotnetCat"
    $ZipUrl = "${RepoRoot}/src/DotnetCat/bin/Zips/DotnetCat_win-x64.zip"
}
elseif ($Architecture -eq "x86") {
    $AppDir = "${env:ProgramFiles(x86)}\DotnetCat"
    $ZipUrl = "${RepoRoot}/src/DotnetCat/bin/Zips/DotnetCat_win-x86.zip"
}
else {
    Show-Error "Unsupported processor architecture: '${Architecture}'"
}

# Remove the existing installation
if (Test-Path $AppDir) {
    Show-Status "Removing existing installation from '${AppDir}'..."
    Remove-Item $AppDir -Force -Recurse 3>&1> $null
}
$ZipPath = "${AppDir}\dncat.zip"

New-Item $AppDir -Force -ItemType Directory 3>&1> $null
Show-Status "Downloading temporary zip file to '${ZipPath}'..."

# Download the temporary application zip file
try {
    Invoke-WebRequest $ZipUrl -DisableKeepAlive -OutFile $ZipPath 3>&1> $null
}
catch {
    Show-Error "Failed to download '${ZipUrl}'. $($Error[0].Exception.Message)"
}

Show-Status "Installing '${ZipPath}' contents to '${AppDir}'..."
Expand-Archive $ZipPath $AppDir -Force 3>&1> $null

Show-Status "Deleting temporary zip file '${ZipPath}'..."
Remove-Item $ZipPath -Force 3>&1> $null

$EnvPath = [Environment]::GetEnvironmentVariable("PATH", $EnvVarTarget)

# Add the application directory to the environment path
if (-not $EnvPath.Contains($AppDir)) {
    if (-not $EnvPath.EndsWith(";")) {
        $EnvPath += ";"
    }

    $EnvPath += $AppDir
    [Environment]::SetEnvironmentVariable("PATH", $EnvPath, $EnvVarTarget)

    if ($?) {
        Show-Status "Successfully added '${AppDir}' to the environment path"
    }
    else {
        Show-Error "Failed to add '${AppDir}' to the environment path"
    }
}
else {
    Show-Status "The environment path already contains '${AppDir}'"
}

Show-Status "DotnetCat was successfully installed, please restart your shell"
