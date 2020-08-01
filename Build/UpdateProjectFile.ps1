[CmdletBinding()]
param(
[Parameter(Mandatory=$True)]
[string]$PathToCsprojFile,
[Parameter(Mandatory=$True)]
[int]$BuildId
)

function Edit-XmlNodes {
    param (
        [xml] $doc,
        [string] $xpath = $(throw "xpath is a required parameter"),
        [string] $value = $(throw "value is a required parameter")
    )

    $nodes = $doc.SelectNodes($xpath)
    $count = $nodes.Count

    Write-Host "Found $count nodes with path '$xpath'"

    foreach ($node in $nodes) {
        if ($node -ne $null) {
            if ($node.NodeType -eq "Element")
            {
                $node.InnerXml = $value
            }
            else
            {
                $node.Value = $value
            }
        }
    }
}

Write-Host "Updating .csproj file versions with build ID"
Write-Host "File: " $PathToCsprojFile
Write-Host "Build ID: " $BuildId

$xml = New-Object XML
$xml.Load($PathToCsprojFile)

[string]$version = $xml.Project.PropertyGroup.Version
$majorMinorPatchBuild = $version.Split('.')
$major = $majorMinorPatchBuild[0]
$minor = $majorMinorPatchBuild[1]
$patch = $majorMinorPatchBuild[2]
Write-Host "major = "$major
Write-Host "minor = "$minor
Write-Host "patch = "$patch

$majorMinorPatch = $major+"."+$minor+"."+$patch
$updatedVersion = $majorMinorPatch+"."+$BuildId
Write-Host "updated version = "$updatedVersion
Write-Host "File version = "$majorMinorPatch
Write-Host "package version = "$majorMinorPatch

Edit-XmlNodes -doc $xml -xpath 'Project/PropertyGroup/Version' -value $updatedVersion
Edit-XmlNodes -doc $xml -xpath 'Project/PropertyGroup/FileVersion' -value $majorMinorPatch
Edit-XmlNodes -doc $xml -xpath 'Project/PropertyGroup/PackageVersion' -value $majorMinorPatch
Edit-XmlNodes -doc $xml -xpath 'Project/PropertyGroup/GeneratePackageOnBuild' -value "true"
Write-Host "Set Version to "$xml.Project.PropertyGroup.Version
Write-Host "Set FileVersion to "$xml.Project.PropertyGroup.FileVersion
Write-Host "Set PackageVersion to "$xml.Project.PropertyGroup.PackageVersion
Write-Host "Set GeneratePackageOnBuild to true"

Write-Host "Saving " $PathToCsprojFile "..."
$xml.Save($PathToCsprojFile)

Write-Host ""
Write-Host "##vso[task.setvariable variable=UpdatedVersion;]$updatedVersion"
Write-Host "Set environment variable to ($env:UpdatedVersion)"



  