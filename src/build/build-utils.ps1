
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
