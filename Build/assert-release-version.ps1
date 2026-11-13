[CmdletBinding()]
param(
	[Parameter(Mandatory=$True)]
	[string]$Tag,
	[Parameter(Mandatory=$True)]
	[string]$DirectoryBuildPropsPath,
	[Parameter(Mandatory=$True)]
	[string]$ChangelogPath
)

$ErrorActionPreference = "Stop"

[xml]$XmlContent = Get-Content -Path $DirectoryBuildPropsPath -Encoding UTF8
[string]$PackageVersion = $XmlContent.SelectSingleNode("//Project/PropertyGroup/PackageVersion").InnerText

Write-Host "Tag = $Tag, PackageVersion = $PackageVersion"

if ($Tag -ne "v$PackageVersion")
{
    throw "Tag '$Tag' does not match PackageVersion '$PackageVersion' in $DirectoryBuildPropsPath."
}

$Header = "## $PackageVersion - "

if (-not (Get-Content -Path $ChangelogPath | Where-Object { $_.StartsWith($Header) }))
{
    throw "$ChangelogPath has no section starting with '$Header'."
}

Write-Host "Tag, PackageVersion and changelog section match."
