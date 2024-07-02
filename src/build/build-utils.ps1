
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

# $Configuration = "Release"
# $SolutionDir = Join-Path -Path $PSCommandPath -ChildPath ../.. -Resolve
# Write-Host "Solution directory: $SolutionDir, configuration: $Configuration"


# # calculate version
# [XML]$XmlVersionFile = Get-Content "$SolutionDir/Version.props"
# $VersionPrefix = $XmlVersionFile.Project.PropertyGroup.VersionPrefix
# $VersionSuffix = $XmlVersionFile.Project.PropertyGroup.VersionSuffix

# $PatchVersion = _ExecSafe { & git rev-list --count master }
# $VersionPrefix = "$VersionPrefix.$PatchVersion"

# $PackageVersion = $VersionPrefix 
# if ($VersionSuffix -ne "") {
#     $PackageVersion = "$VersionPrefix-$VersionSuffix"
# }
# Write-Host -ForegroundColor Cyan "VersionPrefix = $VersionPrefix, VersionSuffix = $VersionSuffix"
# Write-Host -ForegroundColor Cyan "PackageVersion = $PackageVersion"

# # build and pack
# Write-Host "Clean..."
# _ExecSafe { & dotnet clean --configuration $Configuration /p:VersionPrefix=$VersionPrefix /p:VersionSuffix=$VersionSuffix --verbosity quiet $SolutionDir }
# Write-Host "Restore..."
# _ExecSafe { & dotnet restore /p:VersionPrefix=$VersionPrefix /p:VersionSuffix=$VersionSuffix --verbosity quiet $SolutionDir }
# Write-Host "Test..."
# _ExecSafe { & dotnet test --configuration $Configuration /p:VersionPrefix=$VersionPrefix /p:VersionSuffix=$VersionSuffix --verbosity minimal $SolutionDir }
# Write-Host "Build..."
# _ExecSafe { & dotnet build --configuration $Configuration /p:VersionPrefix=$VersionPrefix /p:VersionSuffix=$VersionSuffix --verbosity minimal $SolutionDir }
# Write-Host "Pack..."
# _ExecSafe { & dotnet pack --configuration $Configuration /p:VersionPrefix=$VersionPrefix /p:VersionSuffix=$VersionSuffix $SolutionDir }

# publish
# _ExecSafe { & dotnet nuget push --source $LocalNugetRepo "$SolutionDir/src/Basalt.CommandLine/bin/$Configuration/Basalt.CommandLine.$PackageVersion.nupkg" }

# tag
