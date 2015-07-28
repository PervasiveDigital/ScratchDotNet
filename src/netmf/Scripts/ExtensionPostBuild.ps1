[CmdletBinding()]
param($SolutionDir, $ProjectDir, $ProjectName, $TargetDir, $TargetFileName, $ConfigurationName, [switch]$Disable)

if(-not ($SolutionDir -and $ProjectDir -and $ProjectName -and $TargetDir -and $TargetFileName -and $ConfigurationName))
{
	Write-Error "SolutionDir, ProjectDir, TargetDir, TargetFileName and ConfigurationName are all required"
	exit 1
}

Add-Type -assembly "system.io.compression.filesystem"

$source = $TargetDir
$destination = $SolutionDir + "Desktop\CloudResources\AddIns\" + $ProjectName + ".zip"

Write-Verbose "$source => $destination"

[io.compression.zipfile]::CreateFromDirectory($source, $destination)