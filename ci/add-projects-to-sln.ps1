<#
.SYNOPSIS
  Add a solution folder to a .sln and add references to every project under a directory, nested under that folder.

.PARAMETER Solution
  Path to the .sln file to modify.

.PARAMETER Name
  Name of the solution folder to create.

.PARAMETER Path
  Directory to search recursively for project files (*.csproj, *.vbproj, *.fsproj, *.vcxproj).

.PARAMETER ProjectTypes
  Optional array of project file patterns to include. Defaults to csproj,vbproj,fsproj,vcxproj.

.PARAMETER Force
  Overwrite existing solution folder with same name if specified.

.PARAMETER CreateBackup
  Create a backup of the solution file before modifying it. If not specified, no backup will be created.

.EXAMPLE
  .\dotnet\add-projects-to-sln.ps1 -Solution .\My.sln -Name "AllProjects" -Path ..\src -Force -CreateBackup
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [ValidateScript({ Test-Path $_ -PathType Leaf })]
    [string]$Solution,

    [Parameter(Mandatory = $true, Position = 1)]
    [string]$Name,

    [Parameter(Mandatory = $true, Position = 2)]
    [ValidateScript({ Test-Path $_ -PathType Container })]
    [string]$Path,

    [string[]]$ProjectTypes = @('*.csproj','*.vbproj','*.fsproj','*.vcxproj'),

    [switch]$Force,
    
    [switch]$CreateBackup
)

# --- setup
Write-Debug "ENTRY: script start"
try {
    Write-Debug "Branch: resolve paths"
    $solutionFile = (Resolve-Path $Solution).ProviderPath
    $searchRoot = (Resolve-Path $Path).ProviderPath
} catch {
    Write-Error "Critical: failed to resolve paths: $_"
    exit 2
}

$backupPath = "$solutionFile.bak.$((Get-Date).ToString('yyyyMMddHHmmss'))"
$sfTypeGuid = "{2150E333-8FDC-42A3-9474-1A3956D46DE8}"  # Solution Folder project type GUID

# Read solution
Write-Debug "Branch: read solution file"
try {
    $slnText = Get-Content -LiteralPath $solutionFile -Raw -ErrorAction Stop
} catch {
    Write-Error "Critical: cannot read solution file '$solutionFile': $_"
    exit 3
}
$slnLines = $slnText -split "`r?`n"

# Collect existing projects
Write-Debug "Branch: scan existing Project entries"
$projectEntryRegex = '^\s*Project\("(?<typeGuid>{[^}]+})"\)\s*=\s*"(?<projName>[^"]+)"\s*,\s*"(?<relPath>[^"]+)"\s*,\s*"(?<projGuid>{[^}]+})"\s*$'
$existingProjects = @{}
for ($i=0; $i -lt $slnLines.Count; $i++) {
    $line = $slnLines[$i]
    $m = [regex]::Match($line, $projectEntryRegex)
    if ($m.Success) {
        $rel = $m.Groups['relPath'].Value
        $guid = $m.Groups['projGuid'].Value
        $existingProjects[$rel.ToLower()] = $guid
    }
}
Write-Host "Info: found $($existingProjects.Count) existing project(s) in solution"

# Find insert index (before first "Global")
Write-Debug "Branch: find Global boundary"
# Modified approach to find Global section - more robust
$globalIndex = -1
for ($i=0; $i -lt $slnLines.Count; $i++) {
    if ($slnLines[$i].Trim() -eq "Global") {
        $globalIndex = $i + 1  # +1 because LineNumber is 1-based
        break
    }
}

if ($globalIndex -eq -1) {
    Write-Error "Critical: solution file doesn't contain a Global section; unexpected .sln format."
    exit 4
}

$insertIndex = $globalIndex - 1
Write-Debug "Global at line $globalIndex; insert index $insertIndex"

# Find project files
Write-Debug "Branch: search project files"
$projFiles = @()
foreach ($pattern in $ProjectTypes) {
    Write-Debug "Pattern: $pattern"
    try {
        $projFiles += Get-ChildItem -Path $searchRoot -Filter $pattern -Recurse -File -ErrorAction SilentlyContinue
    } catch {
        Write-Debug "Warning: search error for pattern '$pattern': $_"
    }
}
$projFiles = $projFiles | Sort-Object FullName -Unique

if (-not $projFiles -or $projFiles.Count -eq 0) {
    Write-Host "Info: no project files found under $searchRoot for patterns $ProjectTypes"
    # Continue execution to create the solution folder even when no projects are found
} else {
    Write-Host "Info: found $($projFiles.Count) project file(s) under $searchRoot"
}

# Helpers
function Get-ProjectGuidFromFile([string]$projPath) {
    Write-Debug "Branch (helper): read ProjectGuid from $projPath"
    try {
        $content = Get-Content -LiteralPath $projPath -Raw -ErrorAction Stop
        $m = [regex]::Match($content, '<\s*ProjectGuid\s*>\s*(?<g>{[^<]+})\s*<\s*/\s*ProjectGuid\s*>', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
        if ($m.Success) { return $m.Groups['g'].Value }
        $m2 = [regex]::Match($content, '<\s*ProjectGuid\s*>\s*(?<g>[0-9A-Fa-f\-]{36})\s*<\s*/\s*ProjectGuid\s*>', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
        if ($m2.Success) { return "{0}{1}{2}" -f '{',$m2.Groups['g'].Value,'}' }
    } catch {
        Write-Debug "Helper warning: failed to read '$projPath': $_"
    }
    return $null
}

function Get-StableGuidForPath([string]$path) {
    Write-Debug "Branch (helper): generate stable GUID for path $path"
    try {
        $bytes = [System.Text.Encoding]::UTF8.GetBytes((Resolve-Path $path).ProviderPath.ToLowerInvariant())
        $md5 = [System.Security.Cryptography.MD5]::Create()
        $hash = $md5.ComputeHash($bytes)
        $md5.Dispose()
        $guid = [System.Guid]::New($hash)
        return ("{"+$guid.ToString().ToUpper()+"}")
    } catch {
        Write-Error "Critical: failed to generate stable GUID for '$path': $_"
        exit 5
    }
}

# Detect existing solution folder by name
Write-Debug "Branch: detect existing solution folder named '$Name'"
$existingFolderGuid = $null
for ($i=0; $i -lt $slnLines.Count; $i++) {
    $line = $slnLines[$i]
    $m = [regex]::Match($line, $projectEntryRegex)
    if ($m.Success) {
        $typeGuid = $m.Groups['typeGuid'].Value
        $projName = $m.Groups['projName'].Value
        $projGuid = $m.Groups['projGuid'].Value
        if ($typeGuid.ToUpper() -eq $sfTypeGuid.ToUpper() -and $projName -eq $Name) {
            $existingFolderGuid = $projGuid
            $existingFolderIndex = $i
            break
        }
    }
}
if ($existingFolderGuid) {
    Write-Host "Info: solution folder '$Name' exists as $existingFolderGuid"
    if (-not $Force) {
        Write-Host "Info: no changes made (use -Force to recreate)"
        exit 0
    } else {
        Write-Host "Info: -Force specified; removing existing solution folder and its nested mappings"
    }
} else {
    Write-Host "Info: solution folder '$Name' not present; will create it"
}

if ($existingFolderGuid -and $Force) {
    Write-Debug "Branch: remove existing folder project block"
    $start = $existingFolderIndex
    $end = $start
    for ($j = $start+1; $j -lt $slnLines.Count; $j++) {
        if ($slnLines[$j] -match '^\s*EndProject\s*$') { $end = $j; break }
    }
    if ($end -ge $start) {
        try {
            $slnLines = $slnLines[0..($start-1)] + $slnLines[($end+1)..($slnLines.Count-1)]
            # Re-find Global after removing lines
            $globalIndex = -1
            for ($i=0; $i -lt $slnLines.Count; $i++) {
                if ($slnLines[$i].Trim() -eq "Global") {
                    $globalIndex = $i + 1  # +1 because LineNumber is 1-based
                    break
                }
            }
            if ($globalIndex -eq -1) { throw "Global missing after removal" }
            $insertIndex = $globalIndex - 1
        } catch {
            Write-Error "Critical: failed removing existing folder block: $_"
            exit 6
        }
    }
    $nestedRegex = '^\s*([0-9A-Fa-f{}\-]+)\s*=\s*' + [regex]::Escape($existingFolderGuid)
    $beforeCount = $slnLines.Count
    $slnLines = $slnLines | Where-Object { -not ([regex]::IsMatch($_, $nestedRegex)) }
    $removed = $beforeCount - $slnLines.Count
    Write-Host "Info: removed $removed nested mapping line(s) for folder"
}

# Create or reuse folder GUID
if (-not $existingFolderGuid) {
    $folderGuid = ("{"+([guid]::NewGuid().ToString().ToUpper())+"}")
    Write-Debug "Branch: created new folder GUID $folderGuid"
} else {
    $folderGuid = $existingFolderGuid
    Write-Debug "Branch: reusing folder GUID $folderGuid"
}

# Extract solution configurations
Write-Debug "Branch: extract solution configurations"
$solutionConfigurations = @()
$inSolutionConfigurations = $false
for ($i=0; $i -lt $slnLines.Count; $i++) {
    $line = $slnLines[$i].Trim()
    if ($line -eq "GlobalSection(SolutionConfigurationPlatforms) = preSolution") {
        $inSolutionConfigurations = $true
        continue
    }
    if ($line -eq "EndGlobalSection" -and $inSolutionConfigurations) {
        $inSolutionConfigurations = $false
        break
    }
    if ($inSolutionConfigurations -and $line -match '^\s*(.+)\s*=\s*(.+)\s*$') {
        $configName = $line.Split('=')[0].Trim()
        $solutionConfigurations += $configName
        Write-Debug "Found solution configuration: $configName"
    }
}
Write-Debug "Found $($solutionConfigurations.Count) solution configurations"

# Build project entries and nested mappings
Write-Debug "Branch: prepare project entries and nested mappings"
$projectEntriesToAdd = New-Object System.Collections.Generic.List[string]
$nestedMappingsToAdd = New-Object System.Collections.Generic.List[string]
$projectConfigsToAdd = New-Object System.Collections.Generic.List[string]

$slnDir = [System.IO.Path]::GetDirectoryName((Resolve-Path $solutionFile).ProviderPath)

$addedCount = 0
foreach ($proj in $projFiles) {
    Write-Debug "Loop: processing project file $($proj.FullName)"
    $projFull = (Resolve-Path $proj.FullName).ProviderPath
    $relPath = [IO.Path]::GetRelativePath($slnDir, $projFull).Replace('/', '\')
    $relKey = $relPath.ToLower()

    if ($existingProjects.ContainsKey($relKey)) {
        $projGuid = $existingProjects[$relKey]
        Write-Host "Found: project already in solution: $relPath -> $projGuid"
    } else {
        $projGuid = Get-ProjectGuidFromFile -projPath $projFull
        if (-not $projGuid) { $projGuid = Get-StableGuidForPath -path $projFull }

        $projTypeGuid = '{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}'  # default C#
        switch -Regex ($proj.Extension.ToLower()) {
            '\.vbproj'  { $projTypeGuid = '{F184B08F-C81C-45F6-A57F-5ABD9991F28F}'; Write-Debug "Branch: VB project" }
            '\.fsproj'  { $projTypeGuid = '{F2A71F9B-5D33-465A-A702-920D77279786}'; Write-Debug "Branch: F# project" }
            '\.vcxproj' { $projTypeGuid = '{E6FDF86B-F3D1-11D4-8576-0002A516ECE8}'; Write-Debug "Branch: VCX project" }
            default     { Write-Debug "Branch: assume C# project" }
        }
        $projName = [System.IO.Path]::GetFileNameWithoutExtension($projFull) -replace '"','\"'
        $entry = "Project(`"$projTypeGuid`") = `"$projName`", `"$relPath`", `"$projGuid`""
        $projectEntriesToAdd.Add($entry)
        $projectEntriesToAdd.Add("EndProject")
        Write-Host "Added: queued project $projName -> $projGuid"
        $addedCount++
        
        # Add project configuration platform entries for each solution configuration
        foreach ($config in $solutionConfigurations) {
            # Extract the configuration and platform parts
            $configParts = $config.Split('|')
            $configName = $configParts[0]
            $platformName = $configParts[1]
            
            # Add ActiveCfg entry
            $projectConfigsToAdd.Add("$projGuid.$config.ActiveCfg = $configName|$platformName")
            # Add Build.0 entry
            $projectConfigsToAdd.Add("$projGuid.$config.Build.0 = $configName|$platformName")
        }
    }

    $nestedMappingsToAdd.Add("$projGuid = $folderGuid")
}

Write-Host "Info: $addedCount new project(s) queued to be added to solution folder '$Name'"

# Add folder entry if not present
$folderEntryLines = @()
if (-not ($slnLines -match [regex]::Escape($folderGuid))) {
    # Use the folder name as the relative path for solution folders (virtual folder)
    $folderRel = $Name
    $folderEntryLines += "Project(`"$sfTypeGuid`") = `"$Name`", `"$Name`", `"$folderGuid`""
    $folderEntryLines += "EndProject"
    Write-Host "Added: queued solution folder Project entry for '$Name' (relPath: $folderRel)"
} else {
    Write-Debug "Branch: folder GUID already present in solution lines"
}

$toInsert = $folderEntryLines + $projectEntriesToAdd
if ($toInsert.Count -gt 0) {
    try {
        $before = $slnLines[0..($insertIndex-1)]
        $after = $slnLines[$insertIndex..($slnLines.Count-1)]
        $slnLines = $before + $toInsert + $after
        Write-Debug "Branch: inserted project and folder lines before Global"
    } catch {
        Write-Error "Critical: failed to insert project entries into solution lines: $_"
        exit 7
    }
} else {
    Write-Host "Info: nothing to insert"
}

# Update GlobalSection(NestedProjects) and ProjectConfigurationPlatforms
Write-Debug "Branch: update or create GlobalSection(NestedProjects) and ProjectConfigurationPlatforms"
# Re-find Global after modifications
$globalIndex = -1
for ($i=0; $i -lt $slnLines.Count; $i++) {
    if ($slnLines[$i].Trim() -eq "Global") {
        $globalIndex = $i
        break
    }
}
if ($globalIndex -eq -1) { 
    Write-Error "Critical: cannot find Global start after modifications."
    exit 8 
}
$globalStart = $globalIndex

# Find ProjectConfigurationPlatforms section
$projectConfigStart = $null; $projectConfigEnd = $null
for ($i = $globalStart; $i -lt $slnLines.Count; $i++) {
    if ($slnLines[$i] -match '^\s*GlobalSection\s*\(\s*ProjectConfigurationPlatforms\s*\)\s*=\s*postSolution\s*$') {
        $projectConfigStart = $i
        for ($j = $i+1; $j -lt $slnLines.Count; $j++) {
            if ($slnLines[$j] -match '^\s*EndGlobalSection\s*$') { $projectConfigEnd = $j; break }
        }
        break
    }
    if ($slnLines[$i] -match '^\s*EndGlobal\s*$') { break }
}

# Update ProjectConfigurationPlatforms section if we have configurations to add
if ($projectConfigsToAdd.Count -gt 0 -and $projectConfigStart -ne $null -and $projectConfigEnd -ne $null) {
    Write-Debug "Branch: update existing ProjectConfigurationPlatforms section"
    $newConfigLines = @()
    $newConfigLines += $slnLines[$projectConfigStart]
    
    # First add all existing lines in their original order
    for ($k = $projectConfigStart+1; $k -lt $projectConfigEnd; $k++) {
        $newConfigLines += $slnLines[$k]
    }
    
    # Then add new project configurations
    foreach ($configLine in $projectConfigsToAdd) {
        $newConfigLines += "`t`t" + $configLine
    }
    
    $newConfigLines += $slnLines[$projectConfigEnd]
    try {
        $slnLines = $slnLines[0..($projectConfigStart-1)] + $newConfigLines + $slnLines[($projectConfigEnd+1)..($slnLines.Count-1)]
    } catch {
        Write-Error "Critical: failed to merge ProjectConfigurationPlatforms section: $_"
        exit 13
    }
    Write-Host "Info: ProjectConfigurationPlatforms section updated (merged)"
} elseif ($projectConfigsToAdd.Count -gt 0) {
    Write-Debug "Branch: ProjectConfigurationPlatforms section not found but needed"
    Write-Warning "Warning: Could not find ProjectConfigurationPlatforms section to update. Projects may not be properly configured."
}

$nestedStart = $null; $nestedEnd = $null
for ($i = $globalStart; $i -lt $slnLines.Count; $i++) {
    if ($slnLines[$i] -match '^\s*GlobalSection\s*\(\s*NestedProjects\s*\)\s*=\s*preSolution\s*$') {
        $nestedStart = $i
        for ($j = $i+1; $j -lt $slnLines.Count; $j++) {
            if ($slnLines[$j] -match '^\s*EndGlobalSection\s*$') { $nestedEnd = $j; break }
        }
        break
    }
    if ($slnLines[$i] -match '^\s*EndGlobal\s*$') { break }
}

# Only create/update NestedProjects section if we have mappings to add
if ($nestedMappingsToAdd.Count -gt 0) {
    if ($nestedStart -ne $null -and $nestedEnd -ne $null) {
    Write-Debug "Branch: merge into existing NestedProjects section"
    # Store existing nested mappings with their original order
    $existingNestedDict = @{}
    
    for ($k = $nestedStart+1; $k -lt $nestedEnd; $k++) {
        $line = $slnLines[$k].Trim()
        $m = [regex]::Match($line, '^(?<child>{[^}]+})\s*=\s*(?<parent>{[^}]+})$')
        if ($m.Success) { 
            $child = $m.Groups['child'].Value.ToUpper()
            $parent = $m.Groups['parent'].Value.ToUpper()
            $existingNestedDict[$child] = $parent
        }
    }
    
    # Prepare new nested lines while preserving original order
    $newNestedLines = @()
    $newNestedLines += $slnLines[$nestedStart]
    
    # First add all existing lines in their original order
    for ($k = $nestedStart+1; $k -lt $nestedEnd; $k++) {
        $line = $slnLines[$k]
        $newNestedLines += $line
    }
    
    # Then add new mappings
    foreach ($nm in $nestedMappingsToAdd) {
        $split = $nm.Split('=',2)
        $child = $split[0].Trim().ToUpper()
        $parent = $split[1].Trim().ToUpper()
        if (-not $existingNestedDict.ContainsKey($child)) {
            $newNestedLines += ("`t" + $child + " = " + $parent)
            $existingNestedDict[$child] = $parent
        }
    }
    
    $newNestedLines += $slnLines[$nestedEnd]
    try {
        $slnLines = $slnLines[0..($nestedStart-1)] + $newNestedLines + $slnLines[($nestedEnd+1)..($slnLines.Count-1)]
    } catch {
        Write-Error "Critical: failed to merge NestedProjects section: $_"
        exit 9
    }
    Write-Host "Info: NestedProjects section updated (merged)"
    } else {
        Write-Debug "Branch: create new NestedProjects section"
        $endGlobal = ($slnLines | Select-String -Pattern '^\s*EndGlobal\s*$' -SimpleMatch).LineNumber
        if (-not $endGlobal) { Write-Error "Critical: Cannot find EndGlobal in solution file."; exit 10 }
        $egIndex = $endGlobal - 1
        $nestedBlock = @()
        $nestedBlock += "    GlobalSection(NestedProjects) = preSolution"
        foreach ($nm in $nestedMappingsToAdd) {
            $split = $nm.Split('=',2)
            $child = $split[0].Trim().ToUpper()
            $parent = $split[1].Trim().ToUpper()
            $nestedBlock += "        $child = $parent"
        }
        $nestedBlock += "    EndGlobalSection"
        try {
            $before = $slnLines[0..($egIndex-1)]
            $endAndAfter = $slnLines[$egIndex..($slnLines.Count-1)]
            $slnLines = $before + $nestedBlock + $endAndAfter
        } catch {
            Write-Error "Critical: failed to create NestedProjects section: $_"
            exit 11
        }
        Write-Host "Info: NestedProjects section created"
    }
} else {
    Write-Debug "Branch: no nested mappings to add, skipping NestedProjects section update"
}

# Backup and write
Write-Debug "Branch: backup and write"
try {
    if ($CreateBackup) {
        Copy-Item -LiteralPath $solutionFile -Destination $backupPath -ErrorAction Stop
        Write-Host "Success: backup created at $backupPath"
    }
    $slnLines -join "`r`n" | Set-Content -LiteralPath $solutionFile -NoNewline -Encoding UTF8
} catch {
    $errorMsg = if ($CreateBackup) { "failed to write modified solution or backup: $_" } else { "failed to write modified solution: $_" }
    Write-Error "Critical: $errorMsg"
    exit 12
}

Write-Host "Success: modified solution saved to $solutionFile"
Write-Debug "EXIT: script end"
[PSCustomObject]@{
    Solution = $solutionFile
    Backup = if ($CreateBackup) { $backupPath } else { $null }
    FolderGuid = $folderGuid
    ProjectsAdded = $addedCount
}
