Function Read-Assembly-Version([string]$PathToAssembly)
{
    $AssemblyVersion = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($pathToAssembly).FileVersion
    $Split = $AssemblyVersion.Split('.')	
    return "$($split[0]).$($split[1]).$($split[2])"
}

Function Write-Xml([xml]$xml)
{
    $StringWriter = New-Object System.IO.StringWriter;
    $XmlWriter = New-Object System.Xml.XmlTextWriter $StringWriter;
    $XmlWriter.Formatting = "indented";
    $xml.WriteTo($XmlWriter);
    $XmlWriter.Flush();
    $StringWriter.Flush();
    Write-Host $StringWriter.ToString();
}