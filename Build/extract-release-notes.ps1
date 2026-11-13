[CmdletBinding()]
param(
	[Parameter(Mandatory=$True)]
	[string]$Version,
	[Parameter(Mandatory=$True)]
	[string]$ChangelogPath,
	[Parameter(Mandatory=$True)]
	[string]$OutputPath
)

$ErrorActionPreference = "Stop"

$Header = "## $Version - "
$Notes = @()
$InSection = $false

foreach ($Line in Get-Content -Path $ChangelogPath)
{
    if ($Line.StartsWith("## "))
    {
        $InSection = $Line.StartsWith($Header)
        continue
    }

    if ($InSection)
    {
        $Notes += $Line
    }
}

$Text = ($Notes -join "`n").Trim()

if (-not $Text)
{
    throw "No release notes found for version $Version in $ChangelogPath."
}

Set-Content -Path $OutputPath -Value $Text -Encoding UTF8
Write-Host "Wrote release notes for $Version to $OutputPath"
