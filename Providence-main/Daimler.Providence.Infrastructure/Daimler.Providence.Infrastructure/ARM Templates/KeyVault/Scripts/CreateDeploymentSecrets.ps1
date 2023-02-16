param (
    [Parameter(Mandatory=$true)]
	[String[]] $ARMOutputs,

	[Parameter(Mandatory=$true)]
	[String] $VaultName,

    [Parameter(Mandatory=$true)]
    [String] $ResourceGroupName
)

try
{
    $existingKeyVault = Get-AzKeyVault -VaultName $VaultName -ResourceGroupName $ResourceGroupName
    if(!$existingKeyVault) 
    {
        throw "Error getting the key vault"
    } 
}
catch
{
	Write-Error "Key vault $VaultName does not exist in resource group $ResourceGroupName. $Errors"
}

foreach ($ARMOutput in $ARMOutputs) 
{
	$json = $ARMOutput | convertfrom-json
	foreach ($jsonOutput in $json.PSObject.Properties) 
	{
		#Check if secret already exists
		$secretName = $jsonOutput.Name.substring(0,1).toupper()+$jsonOutput.Name.substring(1)
		$secretValue = ConvertTo-SecureString -String $jsonOutput.Value.value -AsPlainText -Force

		$targetVaultSecret = Get-AzKeyVaultSecret -VaultName $VaultName -Name $secretName
		if(!$targetVaultSecret)
		{	  
			Write-Host "Adding secret with name: $secretName to target KeyVault."
			Set-AzKeyVaultSecret -VaultName $VaultName -Name $secretName -SecretValue $secretValue
		}
		else
		{
			Write-Host "Secret with name: $secretName already exists in key vault with same value."
		}
	}
}