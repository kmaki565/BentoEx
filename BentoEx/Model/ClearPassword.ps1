Write-Host "This script deletes your Obento-Net user ID and password from HKCU.
Everything will be restored before you ran SavePassword.ps1."

$RegPath = "HKCU:\Software\BentoEx"

Remove-Item -Path $RegPath 
