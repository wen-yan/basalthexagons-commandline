param (
    [Parameter(Mandatory = $false)][String]$LocalNugetRepo = "nuget-local",
    [Parameter(Mandatory = $false)][String]$Configuration = "Debug"
)

function ExecSafe([scriptblock] $cmd) {
    $result = & $cmd
    if ($LASTEXITCODE) {
        Out-String -InputObject $result
        Write-Host "LASTEXITCODE = $LASTEXITCODE"
        Throw "Command [$cmd] failed"
    }
    return $result
}

$SolutionDir = Join-Path -Path $PSCommandPath -ChildPath ../.. -Resolve
Write-Host "Solution directory: $SolutionDir"

$VersionPrefix = "0.1.0"
$VersionSuffix = "debug-latest"
$PackageVersion = "$VersionPrefix-$VersionSuffix"
Write-Output "Package version: $PackageVersion"


ExecSafe { & dotnet pack --configuration $Configuration /p:VersionPrefix=$VersionPrefix /p:VersionSuffix=$VersionSuffix $SolutionDir }
ExecSafe { & dotnet nuget push --source $LocalNugetRepo "$SolutionDir/src/Basalt.CommandLine/bin/$Configuration/Basalt.CommandLine.$PackageVersion.nupkg" }
