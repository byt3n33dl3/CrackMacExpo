<#
.SYNOPSIS
    DotnetCat uninstaller script for x64 and x86 Windows systems.
.DESCRIPTION
    DotnetCat remote command-shell application uninstaller script
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

# Uninstaller only supports Windows operating systems
if (-not [RuntimeInformation]::IsOSPlatform([OSPlatform]::Windows)) {
    Show-Error "This uninstaller can only be used on Windows operating systems"
}
$User = [WindowsPrincipal]::new([WindowsIdentity]::GetCurrent())

# Uninstaller requires admin privileges
if (-not $User.IsInRole([WindowsBuiltInRole]::Administrator)) {
    Show-Error "The uninstaller must be run as an administrator"
}
$AppDir = $null

$ArchVarName = "PROCESSOR_ARCHITECTURE"
$EnvVarTarget = [EnvironmentVariableTarget]::Machine
$Architecture = [Environment]::GetEnvironmentVariable($ArchVarName, $EnvVarTarget)

# Validate architecture and assign architecture-specific variables
if ($Architecture -eq "AMD64") {
    $AppDir = "${env:ProgramFiles}\DotnetCat"
}
elseif ($Architecture -eq "x86") {
    $AppDir = "${env:ProgramFiles(x86)}\DotnetCat"
}
else {
    Show-Error "Unsupported processor architecture: '${Architecture}'"
}

# The application is not currently installed
if (-not (Test-Path $AppDir)) {
    Show-Status "DotnetCat is not currently installed on this system"
    exit 0
}

Show-Status "Removing the application files from '${AppDir}'..."
Remove-Item $AppDir -Force -Recurse 3>&1> $null

$EnvPath = [Environment]::GetEnvironmentVariable("PATH", $EnvVarTarget)

# Delete the installation directory from the environment path
if ($EnvPath.Contains($AppDir)) {
    $EnvPath = $EnvPath.Replace(";${AppDir}", $null)
    [Environment]::SetEnvironmentVariable("PATH", $EnvPath, $EnvVarTarget)

    if ($?) {
        Show-Status "Successfully removed '${AppDir}' from the environment path"
    }
    else {
        Show-Error "Failed to remove '${AppDir}' from the environment path"
    }
}

Show-Status "DotnetCat was successfully uninstalled"
