[CmdletBinding()]
param(
    [string]$src = "../../ip-intelligence-dotnet-examples/Examples",
    [string]$dst = ".",
    [switch]$back
)

if ($back) {
    $t = $src
    $src = $dst
    $dst = $t
}

$Files = (
    Get-ChildItem `
        -Path $src `
        -Recurse `
        -Include "*.cs" `
        -Exclude "*.Assembly*.cs" `
    | Select-Object -ExpandProperty "FullName" `
    | ForEach-Object {
        $RelativePath = (Resolve-Path -Path $_ -Relative -RelativeBasePath $src)
        [pscustomobject]@{
            FullPath = ( $_ )
            RelPath  = $RelativePath
            DstPath  = (Join-Path $dst $RelativePath)
        }
    })

foreach ($NextFile in $Files) {
    Write-Debug "  - '$($NextFile.RelPath)' -> '$($NextFile.DstPath)'"
    Copy-Item -Path $NextFile.FullPath -Destination $NextFile.DstPath -Force
}
