
function _ExecSafe([scriptblock] $cmd) {
    $result = & $cmd
    if ($LASTEXITCODE) {
        Out-String -InputObject $result
        Write-Host "LASTEXITCODE = $LASTEXITCODE"
        Throw "Command [$cmd] failed"
    }
    return $result
}

function Get-ReleaseVersion() {
    $SolutionDir = Join-Path -Path $PSCommandPath -ChildPath ../.. -Resolve

    [XML]$XmlVersionFile = Get-Content "$SolutionDir/Version.props"
    $VersionPrefix = $XmlVersionFile.Project.PropertyGroup.VersionPrefix
    $VersionSuffix = $XmlVersionFile.Project.PropertyGroup.VersionSuffix

    $PatchVersion = _ExecSafe { & git rev-list --count master }
    $VersionPrefix = "$VersionPrefix.$PatchVersion"

    $PackageVersion = $VersionPrefix 
    if ($VersionSuffix -ne "") {
        $PackageVersion = "$VersionPrefix-$VersionSuffix"
    }

    return New-Object PSObject -Property @{ VersionPrefix = $VersionPrefix; VersionSuffix = $VersionSuffix; PackageVersion = $PackageVersion }
}

function Push-NugetPackageToLocalRepo() {
    param (
        [Parameter(Mandatory = $false)][String]$LocalNugetRepo = "nuget-local",
        [Parameter(Mandatory = $false)][String]$Configuration = "Debug"
    )
    $SolutionDir = Join-Path -Path $PSCommandPath -ChildPath ../.. -Resolve
    Write-Host "Solution directory: $SolutionDir"

    $VersionPrefix = "0.1.0"
    $VersionSuffix = "$Configuration-latest"
    $PackageVersion = "$VersionPrefix-$VersionSuffix"
    Write-Output "Package version: $PackageVersion"

    _ExecSafe { & dotnet build --configuration $Configuration /p:VersionPrefix=$VersionPrefix /p:VersionSuffix=$VersionSuffix $SolutionDir }
    _ExecSafe { & dotnet pack --configuration $Configuration /p:VersionPrefix=$VersionPrefix /p:VersionSuffix=$VersionSuffix $SolutionDir }
    _ExecSafe { & dotnet nuget push "$SolutionDir/src/BasaltHexagons.CommandLine/bin/$Configuration/BasaltHexagons.CommandLine.$PackageVersion.nupkg" --source $LocalNugetRepo }
}