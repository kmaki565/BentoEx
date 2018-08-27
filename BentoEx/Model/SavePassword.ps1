Write-Host "This script saves your Obento-Net user ID and password into HKCU.
Rest assured, password will be encrypted and only you ($env:USERNAME) on the same computer can decrypt!"
$RegPath = "HKCU:\Software\BentoEx"

$Cred = $host.ui.PromptForCredential("おべんとサッ！と", "おべんとね！っとのユーザーIDとパスワードを入力してください。", "$env:USERNAME", "")
if ($Cred -eq $null) { exit; }

$SecureStringAsPlainText_pw = $Cred.Password | ConvertFrom-SecureString 
if (!(Test-Path $RegPath))
{
    New-Item -Path $RegPath | Out-Null
}

try 
{
    New-ItemProperty -Path $RegPath -Name UserId -PropertyType String -Value $Cred.UserName -Force
    New-ItemProperty -Path $RegPath -Name Password -PropertyType String -Value $SecureStringAsPlainText_pw -Force
    New-ItemProperty -Path $RegPath -Name Date -PropertyType String -Value $(Get-Date) -Force | Out-Null
    Write-Host "User id and password have been saved in $RegPath. Please run the exe again."
}
catch 
{
    Write-Host "Cannot save user id/password to registry"
}
Read-Host "Press Enter to exit"
