[CmdletBinding()]
param(
	[Parameter(Mandatory=$True)]
	[string]$DirectoryBuildPropsPath,
	[Parameter(Mandatory=$True)]
	[int]$BuildId,
	[Parameter(Mandatory=$True)]
	[string]$InformationalVersionSuffix
)

. "$PSScriptRoot\helpers.ps1"

Function Update-CsProj-Versions([string]$DirectoryBuildPropsPath, [int]$BuildId, [string]$InformationalVersionSuffix)
{
    Write-Host "Patching Directory.Build.props file = $DirectoryBuildPropsPath"
    Write-Host "Build number = $BuildId"
    Write-Host "Informational version suffix = $InformationalVersionSuffix"

    [xml]$XmlContent = Get-Content -Path $DirectoryBuildPropsPath -Encoding UTF8

    Write-Host
    Write-Host "Directory.Build.props Content:"
    Write-Xml -xml $XmlContent
    Write-Host

    $FileVersionNode = $XmlContent.SelectSingleNode("//Project/PropertyGroup/FileVersion")
    [string]$FileVersion = $FileVersionNode.InnerText

    Write-Host "Read FileVersion = $FileVersion"

    $Split = $FileVersion.Split(".")

    Write-Host "Major = $($Split[0]), Minor = $($Split[1]), Patch = $($Split[2])"

    [string]$UpdatedFileVersion = "$($Split[0]).$($Split[1]).$($Split[2]).$BuildId"
    $FileVersionNode.InnerText = $UpdatedFileVersion

    Write-Host "Set updated FileVersion = $UpdatedFileVersion"

    $UpdatedVersion = "$($Split[0]).0.0.0"

    Write-Host "Set updated Version = $UpdatedVersion"

    $VersionNode = $XmlContent.SelectSingleNode("//Project/PropertyGroup/Version")
    $VersionNode.InnerText = $UpdatedVersion

    [string]$UpdatedInformationalVersion = "$($Split[0]).$($Split[1]).$($Split[2])-$InformationalVersionSuffix"

    Write-Host "Set updated InformationalVersion = $UpdatedInformationalVersion"

    $InformationalVersionNode = $XmlContent.SelectSingleNode("//Project/PropertyGroup/InformationalVersion")
    [string]$InformationalVersionNode.InnerText = $UpdatedInformationalVersion

    Write-Host
    Write-Host "Udated Directory.Build.props Content:"
    Write-Xml -xml $XmlContent
    Write-Host

    Write-Host "Saving updated Directory.Build.props file to $DirectoryBuildPropsPath"
    $XmlContent.Save($DirectoryBuildPropsPath)
}

Update-CsProj-Versions -DirectoryBuildPropsPath $DirectoryBuildPropsPath -BuildId $BuildId -InformationalVersionSuffix $InformationalVersionSuffix
Write-Host

