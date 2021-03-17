[CmdletBinding()]
param(
	[Parameter(Mandatory=$True)]
	[string]$PathToCsprojFile
)

Write-Host "Updating build number from assembly version read from .csproj file"
Write-Host "File: $PathToCsprojFile"

$xml = New-Object XML
$xml.Load($PathToCsprojFile)

[string]$version = $xml.Project.PropertyGroup.FileVersion
Write-Host "version = $version"

[string]$buildNumber = $env:BUILD_BUILDNUMBER
[string]$newBuildNumber = "$version-$buildNumber"

Write-Host "##vso[build.updatebuildnumber]$newBuildNumber"



  