[CmdletBinding()]
param(
	[Parameter(Mandatory=$True)]
	[string]$PathToCsprojFile
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

$xml = New-Object XML
$xml.Load($PathToCsprojFile)

Write-Host "Setting GeneratePackageOnBuild = true ..."

Edit-XmlNodes -doc $xml -xpath 'Project/PropertyGroup/GeneratePackageOnBuild' -value "true"

Write-Host "Saving "$PathToCsprojFile "..."

$xml.Save($PathToCsprojFile)